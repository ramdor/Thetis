//using System.Net;
//using System.Net.Sockets;
//using System.Threading;
//using HttpServer;
//using System.Diagnostics;
//using System;

//namespace Thetis
//{
//    public class HttpServer
//    {
//        private TcpListener m_listener;
//        private bool m_terminated;

//        public HttpServer(Console cs)
//        {
//            IHandler.setConsole(cs);
//            m_terminated = false;
//        }

//        ~HttpServer()
//        {
//            if (m_listener != null)
//            {
//                m_listener.Stop();
//            }
//        }

//        public void start(int port)
//        {
//            m_terminated = false;
//            try
//            {
//                m_listener = new TcpListener(IPAddress.Any, port);
//            }
//            catch(Exception e)
//            {
//                Debug.WriteLine("7exception" + e);
//                return;
//            }

//            Thread thread = new Thread(loop)
//            {
//                Name = "TCP SERVER THREAD",
//                IsBackground = true,
//                Priority = ThreadPriority.Normal
//            };
//            thread.Start();
//        }

//        public void stop()
//        {
//            m_terminated = true;
//            if (m_listener != null)
//            {
//                try
//                {
//                    m_listener.Stop();
//                }
//                catch (Exception e)
//                {
//                    Debug.WriteLine("Error stop http listener " + e.ToString());
//                }

//            }
//        }

//        private void loop()
//        {
//            try
//            {
//                m_listener.Start();
//            }
//            catch (Exception e)
//            {
//                Debug.WriteLine("Cannot start thread " + e);
//                return;
//            }

//            Debug.WriteLine("HTTP LISTENER STARTED");

//            while (!m_terminated)
//            {
//                try
//                {
//                    IHandler handler = HandlerFactory.getHandler(m_listener.AcceptTcpClient());
//                    if(m_terminated)
//                    {
//                        Thread.Sleep(1000);
//                        return;
//                    }
//                    if (handler != null)
//                    {
//                        handler.handle();
//                    }
//                    Thread.Sleep(1);
//                }
//                catch (Exception e)
//                {
//                    Debug.WriteLine("get TCP RECEIVE fault " + e);
//                }
//            }
//        }
//    }
//}