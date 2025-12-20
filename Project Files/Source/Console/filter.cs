//=================================================================
// filter.cs
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

namespace Thetis
{
    /// <summary>
	/// Summary description for filter.
	/// </summary>
	public class FilterPreset
	{
		private int[] low;
		private int[] high;
		private string[] name;

		public FilterPreset()
		{			
			low = new int[(int)Filter.LAST];
			high = new int[(int)Filter.LAST];
			name = new string[(int)Filter.LAST];
		}

        public void SetLow(Filter f, int val)
        {
            low[(int)f] = val;
        }  

        public void SetHigh(Filter f, int val)
		{
			high[(int)f] = val;
		}

		public void SetName(Filter f, string n)
		{
			name[(int)f] = n;
		}

		public void SetFilter(Filter f, int l, int h, string n)
		{
			low[(int)f] = l;
			high[(int)f] = h;
			name[(int)f] = n;
		}

		public int GetLow(Filter f)
		{
			return low[(int)f];
		}

		public int GetHigh(Filter f)
		{
			return high[(int)f];
		}

        public int GetBW(Filter f)
        {
            return high[(int)f] - low[(int)f];
        }

        public string GetName(Filter f)
		{
			return name[(int)f];
		}

		private Filter last_filter;
		public Filter LastFilter
		{
			get { return last_filter; }
			set { last_filter = value; }
		}

		public string ToString(Filter f)
		{
			return name[(int)f]+": "+low[(int)f].ToString()+"| "+high[(int)f].ToString();
		}

	}
}
