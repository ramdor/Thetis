//MW0LGE 2023
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace Thetis
{
    internal static class TimeOutTimerManager
    {
        public delegate void ToTOccured(string msg);
        private static ToTOccured _callback;

        private static string _hostAddress;
        private static int _pingTimeOutSeconds;
        private static bool _pingTimeoutEnabled;

        private static int _moxTimeOutSeconds;
        private static bool _moxTimeOutEnabled;

        private static bool _tickerRunning;

        private static DateTime _lastPing;
        private static DateTime _lastMox;
        private static bool _mox;

        private static readonly object _locker = new object();
        private static Console _console;

        private static Ping _ping;
        private static bool _init = false;

        public static void Initialise(Console c)
        {
            if (_init) return;

            _ping = new Ping();

            _tickerRunning = false;
            _lastMox = DateTime.Now;
            _lastPing = _lastMox;

            _moxTimeOutEnabled = false;
            _pingTimeoutEnabled = false;

            _console = c;

            _mox = _console.MOX;
            _console.MoxChangeHandlers += onMox;

            //ticker always runs, 1 second interval
            startSecondTicker();

            _init = true;
        }
        public static void Shutdown()
        {
            if (_init)
            {
                _console.MoxChangeHandlers -= onMox;
                _tickerRunning = false;
                _console = null;
                _init = false;
            }
        }
        public static void SetCallback(ToTOccured cb)
        {
            _callback += cb;
        }

        public static void RemoveCallback(ToTOccured cb)
        {
            _callback -= cb;
        }
        public static void PingTimeOut(string hostAddress, int timeOutSeconds, bool enabled)
        {
            lock (_locker)
            {
                _hostAddress = hostAddress;
                _pingTimeOutSeconds = timeOutSeconds;
                _pingTimeoutEnabled = enabled;
            }
        }
        public static void MoxTimeOut(int timeOutSeconds, bool enabled)
        {
            lock (_locker)
            {
                _moxTimeOutSeconds = timeOutSeconds;
                _moxTimeOutEnabled = enabled;
            }
        }
        private static void startSecondTicker()
        {            
            Thread totTickerThread = new Thread(new ThreadStart(tickLoop))
            {
                Name = "ToT Tick",
                Priority = ThreadPriority.BelowNormal,
                IsBackground = true,
            };
            totTickerThread.Start();
        }
        public static void onMox(int rx, bool oldMox, bool newMox)
        {
            lock (_locker)
            {
                _mox = newMox;

                if (newMox && !oldMox)
                {
                    _lastMox = DateTime.Now;
                    _lastPing = _lastMox;
                }
            }
        }
        private static void tickLoop()
        {
            _tickerRunning = true;
            while (_tickerRunning)
            {
                bool totMox = false;
                bool totPing = false;
                int pingRoundTrip = 0;

                lock (_locker)
                {
                    if (_mox && (_moxTimeOutEnabled || _pingTimeoutEnabled))
                    {
                        DateTime now = DateTime.Now;                        

                        // basic mox ToT
                        if (_moxTimeOutEnabled)
                            totMox = (now - _lastMox).Seconds >= _moxTimeOutSeconds;

                        // ping ToT
                        if (!totMox && _pingTimeoutEnabled)
                        {
                            try
                            {
                                PingReply pr = _ping.Send(_hostAddress, 900); // wait 900ms max
                                if (pr.Status == IPStatus.Success)
                                    _lastPing = now;
                                pingRoundTrip = (int)pr.RoundtripTime; // reduce sleep time by the time it took for the ping
                            }
                            catch
                            {
                            }
                            totPing = (now - _lastPing).Seconds >= _pingTimeOutSeconds;
                        }
                    }
                }

                if (totMox || totPing)
                {
                    string msg = totMox ? "MOX" : "PING";
                    _callback?.Invoke(msg); // ok to keep calling this every second if needed
                }

                Thread.Sleep(1000 - pingRoundTrip);
            }
        }
    }
}
