//using Thetis.Properties;
//using System;
//using System.Diagnostics;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;

//namespace HttpServer
//{

//    class WebPageRequestHandler : IHandler
//    {
//        public WebPageRequestHandler(String body, TcpClient tcpClient):
//            base(body, tcpClient)
//        {
            
//        }

//        private string UPDATE_TIME = "#UPDATE_TIME";

//        public override void handle()
//        {
//            if (m_tcpClient == null) return;

//            string timeRefresh_in_ms = getTimeRefresh();

//            string CodeStr = "200 " + ((HttpStatusCode)200).ToString();
//            string Html = Resources.template;
//            Html = Html.Replace(UPDATE_TIME, timeRefresh_in_ms);
            
//            string Str = "HTTP/1.1 " + CodeStr + "\nContent-Type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;

//            Debug.WriteLine("STRING TO SEND: " + Str);

//            byte[] Buffer = Encoding.ASCII.GetBytes(Str);

//            sendAnswer(Buffer);
//        }

//        private string getTimeRefresh()
//        {
//            return m_console.HTTP_REFRESH.ToString();
//        }
//    }

    
//}