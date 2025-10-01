//=================================================================
// hiperftimer.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2009  FlexRadio Systems
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// You may contact us via email at: sales@flex-radio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    8900 Marybank Dr.
//    Austin, TX 78750
//    USA
//=================================================================
// Taken directly from a Code Projects article written by
// Daniel Strigl.
// http://www.codeproject.com/csharp/highperformancetimercshar.asp
//=================================================================
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
using System.Runtime.InteropServices;
using System.Threading;

namespace Thetis
{
	public class HiPerfTimer
	{
		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceCounter(
			out long lpPerformanceCount);

		[DllImport("Kernel32.dll")]
		private static extern bool QueryPerformanceFrequency(
			out long lpFrequency);

		private long startTime, stopTime, elapsedTime;
		private long freq;

		// Constructor
		public HiPerfTimer()
		{
			startTime = 0;
			stopTime  = 0;
            elapsedTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
			{
				// high-performance counter not supported
				throw new Exception();
			}
		}

		// Start the timer
		public void Start()
		{
			// let the waiting threads do their work - start on fresh timeslice
			Thread.Sleep(0);

			QueryPerformanceCounter(out startTime);
		}

		// Stop the timer
		public void Stop()
		{
			QueryPerformanceCounter(out stopTime);
		}

		// Returns the duration of the timer (in seconds)
		public double Duration
		{
			get
			{
				return (double)(stopTime - startTime) / (double) freq;
			}
		}

		public double DurationMsec
		{
			get
			{
				return (1000.0)*(double)((stopTime - startTime)) / (double) freq;
			}
		}

        public double Elapsed {
            get {
                QueryPerformanceCounter(out elapsedTime);
                return (double)(elapsedTime - startTime) / (double)freq;
            }
        }

        public void Reset()
        {
            Start();
        }

        public double ElapsedMsec {
            get {
                QueryPerformanceCounter(out elapsedTime);
                return (1000.0) * (double)((elapsedTime - startTime)) / (double)freq;
            }
        }

        public long GetFreq()
		{
			long freq = 0;
			QueryPerformanceFrequency(out freq);
			return freq;
		}
	}
}