//=================================================================
// MW0LGE 2022
//=================================================================

// inspiration from https://www.codeproject.com/Articles/5733/A-TCP-IP-Server-written-in-C
//
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Thetis
{
	public class TCPIPSocketListener
	{
		//
		public delegate void ClientConnected();
		public delegate void ClientDisconnected();
		public delegate void ClientError(SocketException se);
		public ClientConnected ClientConnectedHandlers;
		public ClientDisconnected ClientDisconnectedHandlers;
		public ClientError ClientErrorHandlers;
		//

		private Console console;
		private TCPIPcatServer m_server;
		private Socket m_clientSocket = null;
		private bool m_stopClient = false;
		private bool m_disconnected = false;
		private Thread m_clientListenerThread = null;
		private bool m_markedForDeletion = false;
		private StringBuilder m_oneLineBuf = new StringBuilder();
		private DateTime m_lastReceiveDateTime;
		private DateTime m_lastSendDateTime;
		private DateTime m_currentReceiveDateTime;
		private DateTime m_currentSendDateTime;

        private ConcurrentQueue<string> _broadcast = new ConcurrentQueue<string>();

        public TCPIPSocketListener(Socket clientSocket, Console c, TCPIPcatServer server)
		{
			console = c;
			m_server = server;
			m_clientSocket = clientSocket;
		}

		~TCPIPSocketListener()
		{
			StopSocketListener();
			m_server = null;
		}

		public void StartSocketListener()
		{
			if (m_clientSocket != null)
			{
				m_clientListenerThread =
					new Thread(new ThreadStart(SocketListenerThreadStart));
				m_clientListenerThread.Name = "TCPIP cat clientListener Thread";

                m_clientListenerThread.Start();
			}
		}

		private void SocketListenerThreadStart()
		{
			int size = 0;
			Byte[] byteBuffer;

			// init date/time for timeouts
			m_lastReceiveDateTime = DateTime.UtcNow;
			m_lastSendDateTime = DateTime.UtcNow;
			m_currentReceiveDateTime = DateTime.UtcNow;
			m_currentSendDateTime = DateTime.UtcNow;

			Timer t = new Timer(new TimerCallback(checkClientCommInterval),
				null, 30000, 30000);

			Debug.Print("TCPIP CAT Client Connected !");
			ClientConnectedHandlers?.Invoke();

			if (m_server != null && m_server.SendWelcome && console != null)
			{
				internal_send_data("#Thetis TCP/IP Cat - " + console.VersionWithoutFW.Replace(";", "") + "#;");
			}

			while (!m_stopClient)
			{
				try
				{
					bool bReadData = false;
					int avialble = m_clientSocket.Available;
					if (avialble > 0)
					{
						byteBuffer = new byte[avialble];

						size = m_clientSocket.Receive(byteBuffer);
						if (size > 0)
						{
							m_currentReceiveDateTime = DateTime.UtcNow;
							ParseReceiveBuffer(byteBuffer, size);
							bReadData = true;
						}
					}
					else
					{
						Queue<string> local = new Queue<string>();
						while (_broadcast.TryDequeue(out string data))
						{
							local.Enqueue(data);
						}
						while (local.Count > 0)
						{
							internal_send_data(local.Dequeue());
                            bReadData = true;
                        }
                    }

                    if (!bReadData)
					{
						Thread.Sleep(50);
					}
				}
				catch (SocketException se)
				{
					//if (se.SocketErrorCode == SocketError.WouldBlock ||
					//	se.SocketErrorCode == SocketError.IOPending ||
					//	se.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
					//{
					//	Thread.Sleep(50);
					//}
					//else
					//{
						m_stopClient = true;
						m_markedForDeletion = true;

						ClientErrorHandlers?.Invoke(se);
					//}
				}
			}

			t.Change(Timeout.Infinite, Timeout.Infinite);
			t = null;

			Debug.Print("TCPIP CAT Client Disconnected !");
			m_disconnected = true;
			ClientDisconnectedHandlers?.Invoke();
		}

		public void StopSocketListener()
		{
			if (m_clientSocket != null)
			{
				m_stopClient = true;
				m_clientSocket.Close();

				m_clientListenerThread.Join(50);

				if (m_clientListenerThread.IsAlive) 
				{
					m_clientListenerThread.Abort();
					m_disconnected = true;
					ClientDisconnectedHandlers?.Invoke();
				}

				m_clientListenerThread = null;
				m_clientSocket = null;
				m_markedForDeletion = true;
			}
		}

		public bool IsMarkedForDeletion()
		{
			return m_markedForDeletion;
		}
		public bool IsDisconnected()
		{
			return m_disconnected;
		}
		private void ParseReceiveBuffer(Byte[] byteBuffer, int size)
		{
			//[2.10.3.9]MW0LGE fixed to handle multiple messages ending in ;
            string data = Encoding.ASCII.GetString(byteBuffer, 0, size);
            m_oneLineBuf.Append(data);
            m_oneLineBuf.Replace(Environment.NewLine, ""); // so can be used a by raw telnet client

            string bufferStr = m_oneLineBuf.ToString();
            int lineEndIndex = bufferStr.IndexOf(";");

            while (lineEndIndex > -1)
            {
                string msg = bufferStr.Substring(0, lineEndIndex + 1);
                processClientData(msg);

                m_oneLineBuf.Remove(0, lineEndIndex + 1);
                bufferStr = m_oneLineBuf.ToString();
                lineEndIndex = bufferStr.IndexOf(";");
            }

            if (m_oneLineBuf.Length > 255) m_oneLineBuf.Clear(); // not likely to be a cat message if it got this long
        }

		private void processClientData(string sInboundCatCommand)
		{
			if (m_server != null && m_server.LogForm != null) m_server.LogForm.Log(true, sInboundCatCommand);

			string sCatAnswer = console.ThreadSafeCatParse(sInboundCatCommand);
			if (sCatAnswer.Length > 0)
				internal_send_data(sCatAnswer);
		}
		private void internal_send_data(string oneLine)
        {
			if (m_clientSocket == null) return;

			try
			{
				if (m_clientSocket.Connected)
				{
					byte[] bytes = Encoding.ASCII.GetBytes(oneLine);
					m_clientSocket.Send(bytes);

					if (m_server != null && m_server.LogForm != null) m_server.LogForm.Log(false, oneLine);

					m_currentSendDateTime = DateTime.UtcNow;
				}
			}
            catch(SocketException se)
            {
				m_stopClient = true;
				m_markedForDeletion = true;

				ClientErrorHandlers?.Invoke(se);
			}
		}

		public void SendData(string data)
		{
            if (m_clientSocket == null) return;

            string[] parts = data.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                _broadcast.Enqueue(part + ";");
            }
        }

		private void checkClientCommInterval(object o)
		{
			bool stopR = false;
			bool stopS = false;

			if (m_lastReceiveDateTime.Equals(m_currentReceiveDateTime))
				stopR = true;
			else
				m_lastReceiveDateTime = m_currentReceiveDateTime;

			if (m_lastSendDateTime.Equals(m_currentSendDateTime))
				stopS = true;
			else
				m_lastSendDateTime = m_currentSendDateTime;

			if (stopR && stopS) this.StopSocketListener();
		}
	}

	public class TCPIPcatServer
	{
		//
		public delegate void ClientConnected();
		public delegate void ClientDisconnected();
		public delegate void ClientError(SocketException se);
		public delegate void ServerError(SocketException se);
		public ClientConnected ClientConnectedHandlers;
		public ClientDisconnected ClientDisconnectedHandlers;
		public ClientError ClientErrorHandlers;
		public ServerError ServerErrorHandlers;
		//

		private Console console;

		public static IPAddress DEFAULT_SERVER = IPAddress.Parse("127.0.0.1");
		public static int DEFAULT_PORT = 31001;
		public static IPEndPoint DEFAULT_IP_END_POINT =
			new IPEndPoint(DEFAULT_SERVER, DEFAULT_PORT);

		private TcpListener m_server = null;
		private bool m_stopServer = false;
		private bool m_stopPurging = false;
		private Thread m_serverThread = null;
		private Thread m_purgingThread = null;
		private List<TCPIPSocketListener> m_socketListenersList = null;
		private Object m_objLocker = new Object();
		private bool m_bSleepingInPurge = false;
		private frmLog _log;

		public TCPIPcatServer()
		{
			Init(DEFAULT_IP_END_POINT);
		}
		public TCPIPcatServer(IPAddress serverIP)
		{
			Init(new IPEndPoint(serverIP, DEFAULT_PORT));
		}

		public TCPIPcatServer(int port)
		{
			Init(new IPEndPoint(DEFAULT_SERVER, port));
		}

		public TCPIPcatServer(IPAddress serverIP, int port)
		{
			Init(new IPEndPoint(serverIP, port));
		}

		public TCPIPcatServer(IPEndPoint ipNport)
		{
			Init(ipNport);
		}
		~TCPIPcatServer()
		{
			StopServer();
			if (_log != null)
			{
				_log.Close();
				_log = null;
			}
		}

		private void Init(IPEndPoint ipNport)
		{
			try
			{
				if (_log != null)
				{
					_log = null;
				}
				_log = new frmLog();
				m_server = new TcpListener(ipNport);
			}
			catch (Exception)
			{
				m_server = null;
			}
		}

		public void StartServer(Console c, bool bTCPIPcatWelcomeMessage = true)
		{
			if (m_server != null)
			{
				console = c;
				m_bSendWelcome = bTCPIPcatWelcomeMessage;

				m_socketListenersList = new List<TCPIPSocketListener>();

                try
				{
					m_server.Start();

					m_serverThread = new Thread(new ThreadStart(ServerThreadStart));
					m_serverThread.Priority = ThreadPriority.BelowNormal;
                    m_serverThread.Name = "TCPIP cat server Thread";
                    m_serverThread.Start();

					m_purgingThread = new Thread(new ThreadStart(PurgingThreadStart));
					m_purgingThread.Priority = ThreadPriority.Lowest;
					m_purgingThread.Name = "TCPIP cat purging Thread";
                    m_purgingThread.Start();
				}
				catch(SocketException se)
                {
					m_sLastError = se.Message;
					StopServer();

					ServerErrorHandlers?.Invoke(se);
				}
			}
		}

		public void SendToClients(string sMsg)
		{
			if (m_server == null) return;

			try
			{
				lock (m_objLocker)
				{
					foreach (TCPIPSocketListener tsl in m_socketListenersList)
					{
						tsl.SendData(sMsg);
					}
				}
			}
			catch (Exception)
			{
			}
		}

		private string m_sLastError = "";
		public string LastError
        {
			get {
				string s = m_sLastError;
				m_sLastError = "";
				return s; 
			}
        }

		public void StopServer()
		{
			if (m_server != null)
			{
				// Stop the TCP/IP Server, so can clean up without more clients connecting
				m_stopServer = true;
				try
				{
					m_server.Stop();
				}
                catch
                {

                }

				if (m_serverThread != null) {
					m_serverThread.Join(50); // dont need to wait long here, as we are blocking anyway
					if (m_serverThread.IsAlive)
						m_serverThread.Abort();
					m_serverThread = null;
				}

				m_stopPurging = true;
				if (m_purgingThread != null)
				{
					if (!m_bSleepingInPurge)
						m_purgingThread.Join(500);

					if (m_purgingThread.IsAlive)
						m_purgingThread.Abort();
					m_purgingThread = null;
				}

				m_server = null;

				// Stop All clients.
				StopAllSocketListers();
			}
		}

		public int ClientsConnected
        {
            get {
				if(m_server == null || m_socketListenersList == null) return 0;

				int nRet = 0;
				lock (m_objLocker)
				{
					foreach(TCPIPSocketListener socketListener in m_socketListenersList)
                    {
						if (!socketListener.IsDisconnected()) nRet++;
                    }
				}
				return nRet;
			}
			//set { m_nTotalClientsConnected = value; }
        }
		public bool IsServerRunning
        {
			get { return m_server != null; }
        }
		private void StopAllSocketListers()
		{
			lock (m_objLocker)
			{
				foreach (TCPIPSocketListener socketListener
							 in m_socketListenersList)
				{
					socketListener.StopSocketListener();

					socketListener.ClientConnectedHandlers -= ClientConnectedHandler;
					socketListener.ClientDisconnectedHandlers -= ClientDisconnectedHandler;
					socketListener.ClientErrorHandlers -= ClientErrorHandler;
				}
				m_socketListenersList.Clear();
				m_socketListenersList = null;
			}
		}

		private void ServerThreadStart()
		{
			Socket clientSocket = null;
			TCPIPSocketListener socketListener = null;
			bool bAddedDelegates = false;
			while (!m_stopServer)
			{
				try
				{
					bAddedDelegates = false;
					clientSocket = m_server.AcceptSocket();

					socketListener = new TCPIPSocketListener(clientSocket, console, this);

					lock (m_objLocker)
					{
						m_socketListenersList.Add(socketListener);
					}

					socketListener.ClientConnectedHandlers += ClientConnectedHandler;
					socketListener.ClientDisconnectedHandlers += ClientDisconnectedHandler;
					socketListener.ClientErrorHandlers += ClientErrorHandler;
					bAddedDelegates = true;

					socketListener.StartSocketListener();
				}
				catch (SocketException se)
				{
                    if (bAddedDelegates && socketListener != null)
                    {
						socketListener.ClientConnectedHandlers -= ClientConnectedHandler;
						socketListener.ClientDisconnectedHandlers -= ClientDisconnectedHandler;
						socketListener.ClientErrorHandlers -= ClientErrorHandler;
					}
					m_stopServer = true;
					m_sLastError = se.Message;
					ServerErrorHandlers?.Invoke(se);
				}
			}
		}

		private void PurgingThreadStart()
		{
			while (!m_stopPurging)
			{
                List<TCPIPSocketListener> deleteList = new List<TCPIPSocketListener>();

				lock (m_objLocker)
				{
					foreach (TCPIPSocketListener socketListener
								 in m_socketListenersList)
					{
						if (socketListener.IsMarkedForDeletion())
						{
							deleteList.Add(socketListener);
							socketListener.StopSocketListener();

							socketListener.ClientConnectedHandlers -= ClientConnectedHandler;
							socketListener.ClientDisconnectedHandlers -= ClientDisconnectedHandler;
							socketListener.ClientErrorHandlers -= ClientErrorHandler;
						}
					}

					for (int i = 0; i < deleteList.Count; ++i)
					{
						m_socketListenersList.Remove(deleteList[i]);
					}
				}

				deleteList = null;

				m_bSleepingInPurge = true;
				Thread.Sleep(5000);
				m_bSleepingInPurge = false;
			}
		}

		private void ClientConnectedHandler()
		{
			ClientConnectedHandlers?.Invoke();
		}
		private void ClientDisconnectedHandler()
        {
			ClientDisconnectedHandlers?.Invoke();
        }
		private void ClientErrorHandler(SocketException se)
        {
			m_sLastError = se.Message;

			ClientErrorHandlers?.Invoke(se);
		}

		private Object m_WelcomeLock = new Object(); // not really needed, but future proofing
		private bool m_bSendWelcome = false;
		public bool SendWelcome
        {
            get {
				lock (m_WelcomeLock)
				{
					return m_bSendWelcome;
				}
			}
			set 
			{
				lock (m_WelcomeLock)
				{
					m_bSendWelcome = value;
				}
			}
        }
		public frmLog LogForm
		{
			get { return _log; }
		}
		public void ShowLog()
		{
			if (_log != null) _log.ShowWithTitle("TCPIPcat");
		}

		public void CloseLog()
		{
			if (_log != null) _log.Hide();
		}
	}
}
