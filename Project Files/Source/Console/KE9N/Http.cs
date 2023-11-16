////=================================================================
//// Http.cs
////=================================================================
//// Http Server
////
//// Николай  RN3KK
//// Darrin ke9ns
////
//// This program is free software; you can redistribute it and/or
//// modify it under the terms of the GNU General Public License
//// as published by the Free Software Foundation; either version 2
//// of the License, or (at your option) any later version.
////
//// This program is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//// GNU General Public License for more details.
////
//// You should have received a copy of the GNU General Public License
//// along with this program; if not, write to the Free Software
//// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
////
//// You may contact us via email at: gpl@flexradio.com.
//// Paper mail may be sent to: 
////    FlexRadio Systems
////    4616 W. Howard Lane  Suite 1-150
////    Austin, TX 78728
////    USA
////=================================================================

//using System;
//using System.Diagnostics;
//using System.Drawing;
//using System.Globalization;
//using System.IO;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Windows.Forms;
//using System.Xml;
//using System.Collections;
//using System.Data;
//using System.ComponentModel;
//using System.IO.Ports;
////using TDxInput;
//using System.Text.RegularExpressions;
//using System.Drawing.Imaging;

//namespace Thetis
//{
//    public class Http
//    {
//        public static Console console;   // ke9ns mod  to allow console to pass back values to setup screen
//        public static Setup SetupForm;          // ke9ns communications with setupform  (i.e. allow combometertype.text update from inside console.cs) 

//        public static TcpListener m_listener;

//        private const String IMAGE_REQUEST = "/image";

//        enum RequestType
//        {
//            GET_IMAGE,
//            GET_HTML_INDEX_PAGE,
//            UNKNOWN,
//            ERROR
//        }
//        public Http(Console c)
//        {
//            console = c;

//        }

     

//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        public void HttpServer1()
//        {
          
//            try
//            {
//              //  m_listener = new TcpListener(IPAddress.Any, console.HTTP_PORT);
              
//            }
//            catch (Exception e)
//            {

//                Debug.WriteLine("7exception" + e);
//                return;

//            }

         
//            Console.m_terminated = false;

//            Thread t = new Thread(new ThreadStart(TCPSERVER))
//            {
//                Name = "TCP SERVER THREAD",
//                IsBackground = true,
//                Priority = ThreadPriority.Normal
//            };
//            t.Start();

//        } // httpserver()


//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //========================================================================================
//        // ke9ns add  THREAD
//        private void TCPSERVER()
//        {
//            try
//            {
//                m_listener.Start();
//            }
//            catch (Exception e)
//            {
//                Debug.WriteLine("Cannot start thread " + e);

//                terminate();
//            }

//            Debug.WriteLine("LISTENER STARTED");


//            while (!Console.m_terminated)
//            {
    
//                try
//                {
//                      TcpClient tempClient = getHandler(m_listener.AcceptTcpClient());

//                 //   TcpClient client = m_listener.AcceptTcpClient();
//                 //   string ip = ((IPEndPoint)m_listener.Server.LocalEndPoint).Address.ToString();
//                //    TcpClient tempClient = getHandler(client);

//                    if ( TcpType != 0)
//                    {
//                        if (TcpType == 1)
//                        {
//                            ImageRequest(tempClient);
//                        }
//                        else if (TcpType == 2)
//                        {
//                            WebPageRequest(tempClient);
//                        }
//                        else
//                        {
//                            UnknownRequest(tempClient);
//                        }
//                    }

//                }
//                catch (Exception e)
//                {
//                    Debug.WriteLine("get TCP RECEIVE fault " + e);

//                    try
//                    {
//                        m_listener.Stop(); // try and close the getcontext thread
//                        m_listener.Start();
//                    }
//                    catch (Exception e1)
//                    {
//                        Debug.WriteLine("close THREAD " + e1);
//                        break;
//                    }

                   
//                }

//                Thread.Sleep(50);

//            } //while (!m_terminated)


//        } // TCPSERVER() THREAD


//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        public static void terminate()
//        {
//            Console.m_terminated = true;

//            try
//            {
//                if (m_listener != null)
//                m_listener.Stop(); // try and close the getcontext thread

//            }
//            catch (Exception e)
//            {
//                Debug.WriteLine("close THREAD " + e);
//            }
//        }

//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================

//        public static int TcpType = 0;

//        public static TcpClient getHandler(TcpClient tcpClient)
//        {
//            switch (getType(tcpClient))
//            {
//                case RequestType.GET_IMAGE:   //  ImageRequest(tempClient);
//                    TcpType = 1;
//                    return tcpClient;
//                case RequestType.GET_HTML_INDEX_PAGE: //  WebPageRequest(tempClient);
//                    TcpType = 2;
//                    return tcpClient;
//                 case RequestType.UNKNOWN: //  UnknownRequest(tempClient);
//                    TcpType = 3;
//                    return tcpClient;
//            }

//            TcpType = 0;
//            return tcpClient;

//        } // TcpClient getHandler


//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================

//        private static RequestType getType(TcpClient tcpClient)
//        {
//            string Request = "";
//            byte[] Buffer = new byte[1024];
//            int Count;

//            while ((Count = tcpClient.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
//            {
//                Request += Encoding.ASCII.GetString(Buffer, 0, Count);
//                if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
//                {
//                    break;
//                }
//            }

//            Match ReqMatch = Regex.Match(Request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

//            if (ReqMatch == Match.Empty)
//            {
//                SendError(tcpClient, 400);
//                return RequestType.ERROR;
//            }

//            string RequestUri = ReqMatch.Groups[1].Value;

//            Debug.WriteLine("URI " + RequestUri);

//            RequestUri = Uri.UnescapeDataString(RequestUri);

//            if (RequestUri.IndexOf("..") >= 0)
//            {
//                SendError(tcpClient, 400);
//                return RequestType.ERROR;
//            }

//            else if (RequestUri.CompareTo(IMAGE_REQUEST) == 0)  // /image
//            {
//                return RequestType.GET_IMAGE;
//            }

//            else if (RequestUri.CompareTo("/") == 0)
//            {
//              // return RequestType.GET_IMAGE;

//             return RequestType.GET_HTML_INDEX_PAGE;
//            }

//            return RequestType.UNKNOWN;

//        } // private static RequestType getType(TcpClient tcpClient)

      
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
         
//        public void ImageRequest(TcpClient m_tcpClient)
//        {
                     
//            if (m_tcpClient == null) return;

//            Debug.WriteLine("IMAGEREQUEST1");

//            byte[] imageArray = getImage();
                     
//            if (imageArray == null)
//            {
//                string CodeStr = "500 " + ((System.Net.HttpStatusCode)500).ToString();

//                string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";

//                string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;

//                byte[] Buffer = Encoding.ASCII.GetBytes(Str);

//                m_tcpClient.GetStream().Write(Buffer, 0, Buffer.Length);
//                m_tcpClient.Close();
//                return;
//            }

//            //  "<meta http-equiv= \"refresh\" content= \"500\" > \r\n" +

//            string responseHeaders =   "HTTP/1.1 200 The file is coming right up!\r\n" +
//                                     "Server: MyOwnServer\r\n" +
//                                    "Content-Length: " + imageArray.Length + "\r\n" +
//                                    "Content-Type: image/jpeg\r\n" +
//                                    "Content-Disposition: inline;filename=\"picDisplay.jpg;\"\r\n" +
//                                    "\r\n";


          
//            byte[] headerArray = Encoding.ASCII.GetBytes(responseHeaders);

//            NetworkStream stream = m_tcpClient.GetStream();

//            stream.Write(headerArray, 0, headerArray.Length);
//            stream.Write(imageArray, 0, imageArray.Length);

//            stream.Close();
//            m_tcpClient.Close();


//        } // ImageRequest()
          
//          //=========================================================================================
//          //=========================================================================================
//          //=========================================================================================
//          //=========================================================================================
//          //=========================================================================================
//          //=========================================================================================

//        public void UnknownRequest(TcpClient m_tcpClient)
//        {
//            Debug.WriteLine("Unknown_REQUEST");


//            if (m_tcpClient == null) return;

//            string CodeStr = "404 " + ((HttpStatusCode)404).ToString();
//            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
//            string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
//            byte[] Buffer = Encoding.ASCII.GetBytes(Str);

//            m_tcpClient.GetStream().Write(Buffer, 0, Buffer.Length);

//            m_tcpClient.Close();

//        } // UnknownRequest()

//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================

//        public void WebPageRequest(TcpClient m_tcpClient)
//        {
//            Debug.WriteLine("Web_REQUEST");


//            if (m_tcpClient == null) return;

//            Debug.WriteLine("Web_REQUEST2");


//            string timeRefresh_in_ms = getTimeRefresh();

//            string CodeStr = "200 " + ((HttpStatusCode)200).ToString();

//            string Html = "<!DOCTYPE html>\n" +
//                          "<html>\n" +
//                          "<head>\n" +
//                          "<title></title>\n" +
//                          "</head>\n" +
//                          "<body>\n" +
//                          "<div><img id = 'img' src = \"\"></div>\n" +
//                          "<script type = \"text/javascript\" src = \"https://code.jquery.com/jquery-3.1.1.min.js\"></script>\n" +
//                          "<script type = \"text/javascript\">\n" +
//                          "var link = \"http://\"+window.location.host;\n" +
//                          "console.log(link);\n" +
//                          "setInterval(function(){\n" +
//                          "var now = new Date();\n" +
//                          "$('#img').prop(\"src\",link+\"/image\" + '?_=' + now.getTime());\n" +
//                          "}, " + timeRefresh_in_ms + ");\n" +
//                          "</script>\n" +
//                          "</body>\n" +
//                          "</html>\n";


//            string Str = "HTTP/1.1 " + CodeStr + "\nContent-Type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;

//            Debug.WriteLine("STRING TO SEND: " + Str);


//            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
//            m_tcpClient.GetStream().Write(Buffer, 0, Buffer.Length);
//            m_tcpClient.Close();

//        } // webrequest

//        private string getTimeRefresh()
//        {
//            //  return "200"; // ************** Darrin,need add property "Refreh time in ms" and get data from his
//            return console.HTTP_REFRESH.ToString();
            
//        }

//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        Bitmap bitmap;
//        byte[] picDisplayOutput;
//        MemoryStream memstream;

//        private byte[] getImage()
//        {

//            bitmap = new Bitmap(console.picDisplay.Width, console.picDisplay.Height); // ke9ns set bitmap size to size of picDisplay since it gets resized with your screen
//            console.picDisplay.DrawToBitmap(bitmap, console.picDisplay.ClientRectangle); // ke9ns grab picDisplay and convert to bitmap
            
//            using (memstream = new MemoryStream())
//            {
//                bitmap.Save(memstream, ImageFormat.Jpeg);
//                picDisplayOutput = memstream.ToArray();
//            }
      
//            return picDisplayOutput;
          
//        } // getImage()


///*     // ke9ns if you want to save image as a file and then read file
//        private byte[] getImage()
//        {

//            bitmap = new Bitmap(console.picDisplay.Width, console.picDisplay.Height); // ke9ns set bitmap size to size of picDisplay since it gets resized with your screen
//            console.picDisplay.DrawToBitmap(bitmap, console.picDisplay.ClientRectangle); // ke9ns grab picDisplay and convert to bitmap
//            bitmap.Save(console.AppDataPath + "picDisplay.jpg", ImageFormat.Jpeg); // ke9ns save image into database folder
          
//            FileInfo picDisplayFile = new FileInfo(console.AppDataPath + "picDisplay.jpg");
//            FileStream picDisplayStream = new FileStream(console.AppDataPath + "picDisplay.jpg", FileMode.Open, FileAccess.Read); // open file  stream 
//            BinaryReader picDisplayReader = new BinaryReader(picDisplayStream); // open stream for binary reading

//            picDisplayOutput = picDisplayReader.ReadBytes((int)picDisplayFile.Length); // create array of bytes to transmit

//            picDisplayReader.Close();
//            picDisplayStream.Close();
                       
//            return picDisplayOutput;


//        } // getImage()

//    */
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        //=========================================================================================
//        private static void SendError(TcpClient Client, int Code)
//    {
//        string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
//        string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
//        string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
//        byte[] Buffer = Encoding.ASCII.GetBytes(Str);
//        Client.GetStream().Write(Buffer, 0, Buffer.Length);
//        Client.Close();
//    }

//} // class http


//} // namespace powersdr