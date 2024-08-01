//MW0LGE 2024
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text;
using System.Net.Cache;
using SkiaSharp;

namespace Thetis
{
    public class ImageFetcher
    {
        public enum State
        {
            OK = 0,
            ERROR_NO_SUITABLE_IMAGE,
            ERROR_URL_ISSUE,
            ERROR_IMAGE_CONVERSION_PROBLEM,
            WAITING,
            GATHERING_IMAGES,

            IDLE = 99
        }

        private readonly ConcurrentDictionary<Guid, ImageStore> _image_stores;
        private readonly ConcurrentDictionary<Guid, Thread> _threads;
        private readonly ConcurrentDictionary<Guid, ManualResetEvent> _reset_events;
        private readonly ConcurrentDictionary<Guid, int> _timeouts;

        public class StateEventArgs : EventArgs
        {
            public Guid Guid { get; }
            public State WebImageState { get; }

            public StateEventArgs(Guid guid, State state)
            {
                Guid = guid;
                WebImageState = state;
            }
        }

        public event EventHandler<Guid> ImagesObtained;
        public event EventHandler<StateEventArgs> StateChanged;

        public ImageFetcher()
        {
            _image_stores = new ConcurrentDictionary<Guid, ImageStore>();
            _threads = new ConcurrentDictionary<Guid, Thread>();
            _reset_events = new ConcurrentDictionary<Guid, ManualResetEvent>();
            _timeouts = new ConcurrentDictionary<Guid, int>();
        }        

        public Guid RegisterURL(string url, int timeout_secs, int image_limit, bool file)
        {
            Guid id = Guid.NewGuid();
            ImageStore store = new ImageStore(image_limit);
            ManualResetEvent reset_event = new ManualResetEvent(false);
            Thread thread = new Thread(() => fetch_images(url, store, reset_event, id, file));

            if (_image_stores.TryAdd(id, store) &&
                _threads.TryAdd(id, thread) &&
                _reset_events.TryAdd(id, reset_event) && 
                _timeouts.TryAdd(id, timeout_secs))
            {
                thread.Start();
                return id;
            }
            else
            {
                //throw new Exception("Failed to register URL.");
                StopFetching(id);
                return Guid.Empty;
            }
        }

        public List<Image> LatestImages(Guid id)
        {
            if (_image_stores.TryGetValue(id, out ImageStore store))
            {
                return store.GetImages();
            }
            else
            {
                return new List<Image>();
            }
        }
        public void UpdateInterval(Guid id, int interval)
        {
            if (_timeouts.TryGetValue(id, out int current))
            {
                if (current != interval)
                {
                    _timeouts[id] = interval;
                    if (_reset_events.TryGetValue(id, out ManualResetEvent reset_event))
                        reset_event.Set();  // Signal the thread
                }
            }
        }
        private void clearAllImages()
        {
            Debug.Print("!!!!!!! CLEAR ALL IMAGES");
            foreach (ImageStore store in _image_stores.Values)
            {
                store.ClearImages();
            }
            Debug.Print("!!!!!!! CLEAR ALL IMAGES COMPLETE");
        }
        private void cleanupResources(Guid id)
        {
            Debug.Print("!!!!!!! CLEANUP : " + id.ToString());
            if (_image_stores.TryRemove(id, out ImageStore store))
            {
                store.ClearImages();
            }

            _reset_events.TryRemove(id, out ManualResetEvent reset_event);
            _threads.TryRemove(id, out Thread thread);
            Debug.Print("!!!!!!! CLEANUP COMPLETE");

        }
        public void StopFetching(Guid id)
        {
            Debug.Print("!!!!!!! STOPFETCHING : " + id.ToString());
            if (_reset_events.TryGetValue(id, out ManualResetEvent reset_event))
            {
                reset_event.Set();  // Signal the thread to stop
                cleanupResources(id);  // Clean up resources
            }
            Debug.Print("!!!!!!! STOPFETCHING COMPLETE");
        }

        public void Shutdown()
        {
            Debug.Print("!!!!!!! SHUTDOWN");
            List<Guid> toRemove = new List<Guid>();
            foreach(KeyValuePair<Guid, ManualResetEvent> kvp in _reset_events)
            {
                toRemove.Add(kvp.Key);
            }
            foreach(Guid guid in toRemove)
            {
                StopFetching(guid);
            }
            Debug.Print("!!!!!!! SHUTDOWN COMPLETE");
        }

        private void fetch_images(string url, ImageStore store, ManualResetEvent reset_event, Guid id, bool file)
        {
            try
            {
                int timeout = _timeouts[id];
                reset_event.Reset();
                while (true)
                {
                    bool imagesAdded = false;                    
                    try
                    {
                        StateChanged?.Invoke(this, new StateEventArgs(id, State.IDLE));
                        if (!file)
                        {
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                            request.Timeout = 2000;
                            request.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

                            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                            {
                                if (response.ContentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
                                {
                                    StateChanged?.Invoke(this, new StateEventArgs(id, State.GATHERING_IMAGES));
                                    using (Stream stream = response.GetResponseStream())
                                    using (StreamReader reader = new StreamReader(stream))
                                    {
                                        string html = reader.ReadToEnd();
                                        List<string> imageUrls = ExtractImageUrls(html, url);

                                        foreach (string imageUrl in imageUrls)
                                        {
                                            try
                                            {
                                                using (WebClient webClient = new WebClient())
                                                {
                                                    byte[] imageData = webClient.DownloadData(imageUrl);
                                                    using (MemoryStream imageStream = new MemoryStream(imageData))
                                                    {
                                                        Image image = Image.FromStream(imageStream);
                                                        bool full = store.AddImage(image);
                                                        imagesAdded = true;
                                                        if (full) break;
                                                        StateChanged?.Invoke(this, new StateEventArgs(id, State.OK));
                                                    }
                                                }
                                            }
                                            catch
                                            {
                                                StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
                                            }
                                        }
                                    }
                                }
                                else if (response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                                {
                                    StateChanged?.Invoke(this, new StateEventArgs(id, State.GATHERING_IMAGES));
                                    using (Stream responseStream = response.GetResponseStream())
                                    {
                                        // Copy the response stream to a MemoryStream
                                        using (MemoryStream memoryStream = new MemoryStream())
                                        {
                                            responseStream.CopyTo(memoryStream);

                                            // Reset the position of the MemoryStream
                                            memoryStream.Position = 0;

                                            // Attempt to decode with SkiaSharp
                                            SKBitmap skBitmap = SKBitmap.Decode(memoryStream);

                                            if (skBitmap != null)
                                            {
                                                // Convert SKBitmap to System.Drawing.Image
                                                using (SKImage skImage = SKImage.FromBitmap(skBitmap))
                                                using (SKData skData = skImage.Encode(SKEncodedImageFormat.Png, 100))
                                                {
                                                    byte[] imageData = skData.ToArray();

                                                    using (MemoryStream ms = new MemoryStream(imageData))
                                                    {
                                                        try
                                                        {
                                                            Image image = Image.FromStream(ms);
                                                            store.AddImage(image);
                                                            imagesAdded = true;
                                                            StateChanged?.Invoke(this, new StateEventArgs(id, State.OK));
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
                                            }
                                        }
                                    }
                                }
                                else if (response.ContentType.StartsWith("multipart/x-mixed-replace", StringComparison.OrdinalIgnoreCase))
                                {
                                    string boundary = getBoundary(response.ContentType);
                                    if (boundary != null)
                                    {
                                        ProcessMultipartContent(response.GetResponseStream(), boundary, store, ref imagesAdded, reset_event, id);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // file
                            if(File.Exists(url))
                            {
                                SKBitmap skBitmap = SKBitmap.Decode(url);
                                if(skBitmap != null)
                                {
                                    using (SKImage skImage = SKImage.FromBitmap(skBitmap))
                                    using (SKData skData = skImage.Encode(SKEncodedImageFormat.Png, 100))
                                    {
                                        byte[] imageData = skData.ToArray();

                                        using (MemoryStream ms = new MemoryStream(imageData))
                                        {
                                            try
                                            {
                                                Image image = Image.FromStream(ms);
                                                store.AddImage(image);
                                                imagesAdded = true;
                                                StateChanged?.Invoke(this, new StateEventArgs(id, State.OK));
                                            }
                                            catch (Exception ex)
                                            {
                                                StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
                                }
                            }
                            else
                            {
                                StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_URL_ISSUE));
                            }
                        }
                    }
                    catch
                    {
                        // Handle exceptions (e.g., network errors)
                        StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_URL_ISSUE));
                    }

                    if (imagesAdded)
                    {
                        OnImagesObtained(id);
                    }

                    // Wait for the interval or stop signal
                    StateChanged?.Invoke(this, new StateEventArgs(id, State.WAITING));
                    if (reset_event.WaitOne(timeout * 1000)) // Convert seconds to milliseconds
                    {
                        //check if _timeout is the same, if so break. It will be different if we have signaled
                        //due to timeout change
                        if (_timeouts[id] == timeout)
                            break;  // Exit the loop if signaled to stop
                        else
                        {
                            timeout = _timeouts[id];
                            reset_event.Reset();
                        }
                    }
                }
            }
            finally
            {
                StateChanged?.Invoke(this, new StateEventArgs(id, State.IDLE));
                cleanupResources(id);
            }
            Debug.Print("!!!!!!! ENDED");
        }        
        private void ProcessMultipartContent(Stream stream, string boundary, ImageStore store, ref bool imagesAdded, ManualResetEvent reset_event, Guid id)
        {
            byte[] boundaryBytes = Encoding.UTF8.GetBytes("--" + boundary);
            byte[] buffer = new byte[8192];
            int bytesRead;

            int boundaryLength = boundaryBytes.Length;
            MemoryStream contentStream = new MemoryStream();
            bool running = true;

            StateChanged?.Invoke(this, new StateEventArgs(id, State.GATHERING_IMAGES));

            while (running && (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                contentStream.Write(buffer, 0, bytesRead);
                byte[] content = contentStream.ToArray();
                int pos = 0;

                while (pos < content.Length)
                {
                    // Find the boundary
                    int boundaryPos = findBoundary(content, pos, boundaryBytes);
                    if (boundaryPos < 0)
                    {
                        // If no boundary is found, keep remaining bytes for the next read
                        contentStream.Position = 0;
                        contentStream.Write(content, pos, content.Length - pos);
                        contentStream.SetLength(content.Length - pos);
                        break;
                    }

                    pos = boundaryPos + boundaryLength + 2; // Move past boundary and CRLF

                    // Read headers
                    while (pos < content.Length && !(content[pos] == '\r' && content[pos + 1] == '\n'))
                    {
                        int endOfLine = Array.IndexOf(content, (byte)'\n', pos);
                        if (endOfLine < 0) break;
                        pos = endOfLine + 1;
                    }

                    pos += 2; // Skip the empty line after headers

                    // Find the next boundary
                    int nextBoundaryPos = findBoundary(content, pos, boundaryBytes);
                    if (nextBoundaryPos < 0)
                    {
                        // If no next boundary, keep remaining bytes for the next read
                        contentStream.Position = 0;
                        contentStream.Write(content, boundaryPos, content.Length - boundaryPos);
                        contentStream.SetLength(content.Length - boundaryPos);
                        break;
                    }

                    // Extract the image data
                    int imageDataLength = nextBoundaryPos - pos - 2; // Subtract 2 for the trailing CRLF
                    if (imageDataLength > 0)
                    {
                        byte[] imageData = new byte[imageDataLength];
                        Array.Copy(content, pos, imageData, 0, imageDataLength);

                        try
                        {
                            using (MemoryStream imageStream = new MemoryStream(imageData))
                            {
                                Image image = Image.FromStream(imageStream, true, true);
                                bool full = store.AddImage(image);
                                imagesAdded = true;
                                StateChanged?.Invoke(this, new StateEventArgs(id, State.OK));
                                if (full)
                                {
                                    running = false;
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //Debug.Print("Exception: " + ex.Message);
                            StateChanged?.Invoke(this, new StateEventArgs(id, State.ERROR_IMAGE_CONVERSION_PROBLEM));
                            running = false;
                            break;
                        }
                    }

                    pos = nextBoundaryPos;
                }

                bool sig = reset_event.WaitOne(10);
                if (sig)
                {
                    running = false;
                    break;
                }
            }
            Debug.Print("!!!!!!! ENDED MULTIPART");
        }

        private int findBoundary(byte[] content, int start, byte[] boundary)
        {
            for (int i = start; i <= content.Length - boundary.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < boundary.Length; j++)
                {
                    if (content[i + j] != boundary[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return i;
                }
            }
            return -1;
        }

        private bool CheckForBoundary(MemoryStream ms, string boundary)
        {
            long currentPosition = ms.Position;
            ms.Position -= Math.Min(ms.Length, boundary.Length + 4); // Check last few bytes for boundary
            StreamReader reader = new StreamReader(ms);
            string lastPart = reader.ReadToEnd();
            ms.Position = currentPosition; // Restore position

            return lastPart.Contains("--" + boundary);
        }
        private string getBoundary(string contentType)
        {
            string pattern = "boundary=(.*)";
            Match match = Regex.Match(contentType, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
        private List<string> ExtractImageUrls(string html, string baseUrl)
        {
            List<string> imageUrls = new List<string>();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//img[@src]");

            if(nodes != null && nodes.Count > 0) {
                foreach (HtmlNode img in nodes)
                {
                    string src = img.GetAttributeValue("src", null);
                    if (!string.IsNullOrEmpty(src))
                    {
                        Uri baseUri = new Uri(baseUrl);
                        Uri absoluteUri = new Uri(baseUri, src);
                        imageUrls.Add(absoluteUri.ToString());
                    }
                }
            }

            return imageUrls;
        }

        protected virtual void OnImagesObtained(Guid id)
        {
            ImagesObtained?.Invoke(this, id);
        }

        private class ImageStore
        {
            private readonly int _image_limit;
            private readonly Queue<Image> _images;
            private readonly object _lock_object = new object();

            public ImageStore(int image_limit)
            {
                _image_limit = image_limit;
                _images = new Queue<Image>();
            }

            public bool AddImage(Image image)
            {
                bool full = false;
                lock (_lock_object)
                {
                    if (_images.Count >= _image_limit)
                    {
                        Image old_image = _images.Dequeue();
                        old_image.Dispose();
                        full = true;
                    }
                    _images.Enqueue(image);
                }
                return full;
            }

            public List<Image> GetImages()
            {
                lock (_lock_object)
                {
                    return new List<Image>(_images);
                }
            }

            public void ClearImages()
            {
                lock (_lock_object)
                {
                    while (_images.Count > 0)
                    {
                        Image old_image = _images.Dequeue();
                        old_image.Dispose();
                    }
                }
            }
        }
    }
}
