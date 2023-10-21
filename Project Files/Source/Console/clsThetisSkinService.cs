//MW0LGE 2023
//SkinServer implementation
using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Cache;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Thetis
{   
    public class ThetisSkin
    {
        private string _skinName;
        public string SkinName {
            get { return _skinName; }
            set
            {
                _skinName = value.Left(20);
            }
        }
        public string SkinUrl { get; set; }
        public string SkinVersion { get; set; }
        public string FromThetisVersion { get; set; }
        public string ThumbnailUrl { get; set; }
        public string SkinHomepageUrl { get; set; }
        public string DateReleased { get; set; }
        public string Overview { get; set; }
        public bool IsMeterSkin { get; set; }
    }

    public class SkinsData
    {
        public string AuthorName { get; set; }
        public string AuthorCallsign { get; set; }
        public string AuthorNickname { get; set; }
        public string SkinsHomepageUrl { get; set; }
        public string DonateUrl { get; set; }
        public List<ThetisSkin> ThetisSkins { get; set; }
    }

    public class SkinServer
    {
        private string _desc;
        public string SkinServerUrl { get; set; }
        public string Description
        {
            get 
            {
                return _desc;
            }
            set 
            {
                _desc = value.Left(50);
            }
        }
        public bool BypassRootFolderCheck { get; set; }
    }

    public class SkinServersData
    {
        public List<SkinServer> SkinServers { get; set; }
    }

    public class SkinHttpImage
    {
        public Image Image { get; set; }
        public string ID { get; set; }
    }
    public class SkinFileDownload
    {
        public string Path { get; set; }
        public long BytesDownloaded { get; set; }
        public long TotalBytes { get; set; }
        public string Url { get; set; }
        public bool Complete {  get; set; }
        public bool Cancelled { get; set; }
        public int PercentageDownloaded { get; set; }
        public bool BypassRootFolderCheck { get; set; }
        public bool IsMeterSkin { get; set; }
    }
    public static class ThetisSkinService
    {
        // perhaps change to static instead of creating new HTTPclients all over?
        private static HttpClient httpClient = new HttpClient();

        public static event EventHandler<SkinsData> ThetisSkinsData;
        public static event EventHandler<SkinServersData> ThetisSkinServerData;
        public static event EventHandler<SkinHttpImage> ImageLoaded;
        public static event EventHandler<SkinFileDownload> FileDownload;

        public static CancellationTokenSource _downloadCancellationTokenSource = null;

        public static async void GetThetisSkinsData(string jsonUrl)
        {
            HttpRequestCachePolicy cp = null;
            WebRequestHandler wrh = null;
            HttpClient client = null;

            try
            {
                cp = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

                wrh = new WebRequestHandler()
                {
                    CachePolicy = cp,
                    UseCookies = false
                };

                client = new HttpClient(wrh);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true };

                // Send a GET request to the JSON URL and read the content asynchronously.
                // In some scenarios, especially in UI applications (e.g., WinForms, WPF),
                // calling asynchronous methods without ConfigureAwait(false) can lead to a deadlock. 
                string jsonContent = await httpClient.GetStringAsync(jsonUrl + "?timestamp=" + DateTime.Now.Ticks);

                // Deserialize the JSON data into the pre-defined class.
                SkinsData thetisSkinsData = JsonConvert.DeserializeObject<SkinsData>(jsonContent);

                // Notify subscribers that the data has been received.
                ThetisSkinsData?.Invoke(null, thetisSkinsData);

                client.Dispose();
                client = null;

                wrh.Dispose();
                wrh = null;

                cp = null;
            }
            catch (Exception ex)
            {
                // Notify subscribers of the error.
                ThetisSkinsData?.Invoke(null, null);
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }

                if (wrh != null)
                {
                    wrh.Dispose();
                    wrh = null;
                }

                cp = null;
            }
        }
        public static async void GetSkinServers(string jsonUrl)
        {
            HttpRequestCachePolicy cp = null;
            WebRequestHandler wrh = null;
            HttpClient client = null;

            try
            {
                cp = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

                wrh = new WebRequestHandler()
                {
                    CachePolicy = cp,
                    UseCookies = false
                };

                client = new HttpClient(wrh);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue(){ NoCache = true };

                string jsonContent = await httpClient.GetStringAsync(jsonUrl + "?timestamp=" + DateTime.Now.Ticks);
                SkinServersData skinServersData = JsonConvert.DeserializeObject<SkinServersData>(jsonContent);
                ThetisSkinServerData?.Invoke(null, skinServersData);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                ThetisSkinServerData?.Invoke(null, null);
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }

                if (wrh != null)
                {
                    wrh.Dispose();
                    wrh = null;
                }

                cp = null;
            }
        }

        public static void SubscribeForSkinData(EventHandler<SkinsData> eventHandler)
        {
            ThetisSkinsData += eventHandler;
        }

        public static void UnsubscribeFromSkinData(EventHandler<SkinsData> eventHandler)
        {
            ThetisSkinsData -= eventHandler;
        }
        public static void SubscribeForSkinServerData(EventHandler<SkinServersData> eventHandler)
        {
            ThetisSkinServerData += eventHandler;
        }

        public static void UnsubscribeFromSkinServerData(EventHandler<SkinServersData> eventHandler)
        {
            ThetisSkinServerData -= eventHandler;
        }
        public static void SubscribeForImageLoaded(EventHandler<SkinHttpImage> eventHandler)
        {
            ImageLoaded += eventHandler;
        }

        public static void UnsubscribeFromImageLoaded(EventHandler<SkinHttpImage> eventHandler)
        {
            ImageLoaded -= eventHandler;
        }
        public static void SubscribeForDownload(EventHandler<SkinFileDownload> eventHandler)
        {
            FileDownload += eventHandler;
        }

        public static void UnsubscribeFromDownload(EventHandler<SkinFileDownload> eventHandler)
        {
            FileDownload -= eventHandler;
        }
        public static async void LoadImageFromUrl(string imageUrl, string sID)
        {
            HttpRequestCachePolicy cp = null;
            WebRequestHandler wrh = null;
            HttpClient client = null;

            try
            {
                cp = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

                wrh = new WebRequestHandler()
                {
                    CachePolicy = cp,
                    UseCookies = false
                };

                client = new HttpClient(wrh);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true };

                byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                using (System.IO.MemoryStream stream = new System.IO.MemoryStream(imageBytes))
                {
                    Image image = Image.FromStream(stream);

                    SkinHttpImage shi = new SkinHttpImage();
                    shi.Image = image;
                    shi.ID = sID;

                    ImageLoaded?.Invoke(null, shi);
                }
            }
            catch (Exception ex)
            {
                ImageLoaded?.Invoke(null, null);
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }

                if (wrh != null)
                {
                    wrh.Dispose();
                    wrh = null;
                }

                cp = null;
            }
        }

        public static async void DownloadFile(string fileUrl, string savePath, bool bypassFolderCheck, bool isMeterSkin)
        {
            if (_downloadCancellationTokenSource != null) return; // busy
            _downloadCancellationTokenSource = new CancellationTokenSource();

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    using (HttpResponseMessage response = await httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead,_downloadCancellationTokenSource.Token))
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            SkinFileDownload sfd = new SkinFileDownload();

                            long totalBytes = response.Content.Headers.ContentLength ?? -1; /// null-coalescing operator, left left side is null then -1
                            long downloadedBytes = 0;

                            sfd.BypassRootFolderCheck = bypassFolderCheck;
                            sfd.IsMeterSkin = isMeterSkin;
                            sfd.Url = fileUrl;
                            sfd.Path = savePath;
                            sfd.TotalBytes = totalBytes;
                            sfd.Complete = false;
                            sfd.PercentageDownloaded = 0;                            

                            FileDownload?.Invoke(null, sfd);
                            int buffLen = 4096;
                            byte[] buffer = new byte[buffLen];

                            using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, buffLen, true))
                            {                                
                                int bytesRead;
                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, _downloadCancellationTokenSource.Token)) > 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead, _downloadCancellationTokenSource.Token);
                                    downloadedBytes += bytesRead;

                                    sfd.BytesDownloaded = downloadedBytes;

                                    if (totalBytes != -1)
                                    {
                                        float perc = (downloadedBytes / (float)totalBytes) * 100f;

                                        if ((int)perc != sfd.PercentageDownloaded)
                                        {
                                            sfd.PercentageDownloaded = (int)perc;
                                            FileDownload?.Invoke(null, sfd);
                                        }
                                    }

                                }
                            }

                            sfd.BytesDownloaded = downloadedBytes;
                            sfd.PercentageDownloaded = 100;
                            sfd.Complete = true;
                            sfd.Cancelled = false;

                            FileDownload?.Invoke(null, sfd);
                        }
                    }
                }
            }
            catch (TaskCanceledException tex)
            {
                SkinFileDownload sfd = new SkinFileDownload();
                sfd.Complete = false;
                sfd.Cancelled = true;
                FileDownload?.Invoke(null, sfd);
            }
            catch (Exception ex)
            {
                FileDownload?.Invoke(null, null);
            }

            _downloadCancellationTokenSource = null;
        }
        public static void CancelDownload()
        {
            if (_downloadCancellationTokenSource != null)
            {
                _downloadCancellationTokenSource.Cancel();
            }
        }
    }
}
