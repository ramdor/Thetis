/*  TimeOutTimerManager.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2025 Richard Samphire MW0LGE

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

The author can be reached by email at

mw0lge@grange-lane.co.uk
*/
//
//============================================================================================//
// Dual-Licensing Statement (Applies Only to Author's Contributions, Richard Samphire MW0LGE) //
// ------------------------------------------------------------------------------------------ //
// For any code originally written by Richard Samphire MW0LGE, or for any modifications       //
// made by him, the copyright holder for those portions (Richard Samphire) reserves the       //
// right to use, license, and distribute such code under different terms, including           //
// closed-source and proprietary licences, in addition to the GNU General Public License      //
// granted above. Nothing in this statement restricts any rights granted to recipients under  //
// the GNU GPL. Code contributed by others (not Richard Samphire) remains licensed under      //
// its original terms and is not affected by this dual-licensing statement in any way.        //
// Richard Samphire can be reached by email at :  mw0lge@grange-lane.co.uk                    //
//============================================================================================//

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
            _lastMox = DateTime.UtcNow;
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
                    _lastMox = DateTime.UtcNow;
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
                        DateTime now = DateTime.UtcNow;                        

                        // basic mox ToT
                        if (_moxTimeOutEnabled)
                            totMox = (now - _lastMox).TotalSeconds >= _moxTimeOutSeconds;
                        
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
                            totPing = (now - _lastPing).TotalSeconds >= _pingTimeOutSeconds;
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
