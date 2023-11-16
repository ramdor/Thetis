//using System.Net.Sockets;
//using System;
//using System.Text.RegularExpressions;
//using System.Net;
//using System.Text;
//using System.Diagnostics;
//using System.Threading;

//namespace HttpServer
//{
//    public class HandlerFactory
//    {
//        private HandlerFactory()
//        {
//        }

//        private const String IMAGE_REQUEST = "/image";
//        private const String SET_PARAM_REQUEST = "/setparam";
//        private const String GET_TRX_PARAM_REQUEST = "/gettrxparam";

//        public static IHandler getHandler(TcpClient tcpClient)
//        {
//            if (tcpClient == null) return null;
//            string postParams = null;
//            switch (getType(tcpClient, ref postParams))
//            {
//                case RequestType.GET_IMAGE:
//                    return new ImageRequestHandler(null, tcpClient);
//                case RequestType.GET_HTML_INDEX_PAGE:
//                    return new WebPageRequestHandler(null, tcpClient);
//                case RequestType.SET_PARAM:
//                    return new SetParamRequestHandler(postParams, tcpClient); //chage null to real body content
//                case RequestType.GET_TRX_PARAM:
//                    return new GetTRXParamHandler(null, tcpClient);
//                case RequestType.UNKNOWN:
//                    return new UnknownRequestHandler(null, tcpClient);
//            }
//            return null;
//        }

//        private static RequestType getType(TcpClient tcpClient, ref String postParam)
//        {
//            string Request = "";
//            byte[] Buffer = new byte[1024];

//            NetworkStream stream = tcpClient.GetStream();
//            stream.ReadTimeout = 10;
//            try
//            {
//                if (stream.CanRead)
//                {
//                    int numberOfBytesRead = 0;
//                    do
//                    {
//                        numberOfBytesRead = stream.Read(Buffer, 0, Buffer.Length);
//                        Request += Encoding.UTF8.GetString(Buffer, 0, numberOfBytesRead);
//                        Thread.Sleep(50);
//                    }
//                    while (stream.DataAvailable);
                     
//                    //Debug.WriteLine(Request);
//                    int indexData = Request.IndexOf("\r\n\r\n");
//                    if (indexData > 0)
//                    {
//                        postParam = Request.Substring(indexData + 4);
//                    }
//                    else
//                    {
//                        postParam = null;
//                    }
//                }
//            }
//            catch (System.Exception e)
//            {
//                Debug.WriteLine(e.ToString());
//            }
//            //stream.Close();
       
//            /*
//            while ((Count = tcpClient.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
//            {
//                Request += Encoding.UTF8.GetString(Buffer, 0, Count);
//                if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
//                {
//                    break;
//                }
//            }
//            Debug.WriteLine(Request);
//            */

//            Match ReqMatch = Regex.Match(Request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

//            if (ReqMatch == Match.Empty)
//            {
//                SendError(tcpClient, 400);
//                return RequestType.ERROR;
//            }

//            string RequestUri = ReqMatch.Groups[1].Value;

//            RequestUri = Uri.UnescapeDataString(RequestUri);

//            if (RequestUri.IndexOf("..") >= 0)
//            {
//                SendError(tcpClient, 400);
//                return RequestType.ERROR;
//            }
//            else if (RequestUri.CompareTo(IMAGE_REQUEST) == 0)
//            {
//                return RequestType.GET_IMAGE;
//            }
//            else if (RequestUri.CompareTo("/") == 0)
//            {
//                return RequestType.GET_HTML_INDEX_PAGE;
//            }
//            else if (RequestUri.CompareTo(SET_PARAM_REQUEST) == 0)
//            {
//                return RequestType.SET_PARAM;
//            }
//            else if(RequestUri.CompareTo(GET_TRX_PARAM_REQUEST) == 0)
//            {
//                return RequestType.GET_TRX_PARAM;
//            }
//            return RequestType.UNKNOWN;
//        }

//        private static void SendError(TcpClient Client, int Code)
//        {
//            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
//            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
//            string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
//            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
//            Client.GetStream().Write(Buffer, 0, Buffer.Length);
//            Client.Close();
//        }
//    }
//}