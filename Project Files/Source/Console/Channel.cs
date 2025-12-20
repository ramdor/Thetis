//=================================================================
// Channel.cs
//=================================================================
// PowerSDR is a C# implementation of a Software Defined Radio.
// Copyright (C) 2004-2012  FlexRadio Systems
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
// You may contact us via email at: gpl@flexradio.com.
// Paper mail may be sent to: 
//    FlexRadio Systems
//    4616 W. Howard Lane  Suite 1-150
//    Austin, TX 78728
//    USA
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

namespace Thetis
{
    public class Channel : IComparable
    {
        /// <summary>
        /// Center frequency for the channel in MHz
        /// </summary>
        private double freq;
        public double Freq
        {
            get { return freq; }
            set { freq = value; }
        }

        /// <summary>
        /// Bandwidth of the channel in Hz
        /// </summary>
        private int bw;
        public int BW
        {
            get { return bw; }
            set { bw = value; }
        }

        public double Low {
            get { return freq - ((bw * 1e-6) / 2); }
            set {}
        }
        public double High {
            get { return freq + ((bw * 1e-6) / 2); }
            set {}
        }

        /// <summary>
        /// Creates a channel object
        /// </summary>
        /// <param name="f">Starting frequency in MHz</param>
        /// <param name="bandwidth">Starting bandwidth in Hz</param>
        public Channel(double f, int bandwidth)
        {
            freq = f;
            bw = bandwidth;
        }

        public Channel(double f, int bandwidth, bool perm, int dep)
        {
            freq = f;
            bw = bandwidth;
        }

        /// <summary>
        /// Displays the Channel details in a string
        /// </summary>
        /// <returns>The contents of the Channel in a string</returns>
        public override string ToString()
        {
            return freq.ToString("R") + " MHz| " + bw + " Hz";
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            return this.freq.CompareTo(((Channel)obj).freq);
        }

        public Channel Copy()
        {
            return new Channel(this.freq, this.bw);
        }

        public bool InBW(double low, double high)
        {
            double channel_low = Freq - ((double)BW / 2) * 1e-6;
            double channel_high = Freq + ((double)BW / 2) * 1e-6;

            return ((low > channel_low && low < channel_high) ||
                (high > channel_low && high < channel_high) ||
                (channel_low > low && channel_high < high));
        }
    }
}
