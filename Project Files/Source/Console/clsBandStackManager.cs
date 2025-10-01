/*  clsBandStackManager.cs

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
using System.Drawing;
using System.Diagnostics;

namespace Thetis
{
    //move to enum .cs eventually
    public enum BandType
    {
        FIRST = -1,
        GEN,
        HF,
        VHF,
        UHF,
        SHF,
        LAST
    }
    public enum DSPSubMode
    {
        FIRST = -1,
        NONE,
        FT8,
        FT4,
        PSK31,
        PSK63,
        MFSK,
        THROB,
        DOM_EX,
        MT63,
        THOR,
        RTTY,
        OLIVIA,
        JT65,
        JT9
    }
    //

    public struct BandFrequencyData
    {
        public double low;
        public double high;
        public Band band;
        public bool lowOnly;
        public FRSRegion region;
        public BandType bandType;

        public BandFrequencyData(double low, double high, Band band, BandType bandType, bool lowOnly, FRSRegion region)
        {
            this.low = low;
            this.high = lowOnly ? 0 : high;
            this.band = band;
            this.lowOnly = lowOnly;
            this.region = region;
            this.bandType = bandType;
        }

        public BandFrequencyData Copy()
        {
            BandFrequencyData newBfd = new BandFrequencyData
            {
                low = this.low,
                high = this.high,
                band = this.band,
                lowOnly = this.lowOnly,
                region = this.region,
                bandType = this.bandType
            };

            return newBfd;
        }
    }
    public class BandStackEntry : IComparable
    {
        private double frequency;
        private double centreFrequency;
        private Band band;
        private bool cTUNEnabled;
        private DSPMode mode;
        private DSPSubMode subMode;
        private Filter filter;
        private double zoomFactor;
        private int zoomSlider;
        private bool locked;
        private string description;

        private int lowFilter; // not stored, used for display rendering, and calculated on the fly
        private int highFilter; // not stored, used for display rendering, and calculated on the fly

        public string GUID;

        public double Frequency { get => frequency; set { frequency = value; } }
        public double CentreFrequency { get => centreFrequency; set { centreFrequency = value; } }
        public Band Band { get => band; set { band = value; } }
        public bool CTUNEnabled { get => cTUNEnabled; set { cTUNEnabled = value; } }
        public DSPMode Mode { get => mode; set { mode = value; } }
        public DSPSubMode SubMode { get => subMode; set { subMode = value; } }
        public Filter Filter { get => filter; set { filter = value; } }
        public double ZoomFactor { get => zoomFactor; set { zoomFactor = value; } }
        public int ZoomSlider { get => zoomSlider; set { zoomSlider = value; } }
        public bool Locked { get => locked; set => locked = value; }
        public string Description { get => description; set => description = value; }
        // not stored, used for display rendering, and calculated on the fly
        public int LowFilter { get => lowFilter; set => lowFilter = value; }
        public int HighFilter { get => highFilter; set => highFilter = value; }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (obj.GetType() != typeof(BandStackEntry)) return 1;

            int nRet = 1;
            BandStackEntry o = obj as BandStackEntry;
            if( this.Frequency == o.Frequency &&
                this.Band == o.Band
                //this.CentreFrequency == o.CentreFrequency &&
                //this.CTUNEnabled == o.CTUNEnabled &&
                //this.Description == o.Description &&
                //this.Filter == o.Filter &&
                //this.GUID == o.GUID
                //this.Locked == o.Locked &&
                //this.Mode == o.Mode &&
                //this.SubMode == o.SubMode &&
                //this.ZoomFactor == o.ZoomFactor &&
                //this.ZoomSlider == o.ZoomSlider
                )
            {
                nRet = 0;
            }
            return nRet;
        }

        public BandStackEntry()
        {
            GUID = System.Guid.NewGuid().ToString();
        }
        public BandStackEntry Copy(bool bNewGUID = false)
        {
            BandStackEntry bse = new BandStackEntry()
            {
                Frequency = this.Frequency,
                CentreFrequency = this.CentreFrequency,
                Band = this.Band,
                CTUNEnabled = this.CTUNEnabled,
                Mode = this.Mode,
                SubMode = this.SubMode,
                Filter = this.Filter,
                ZoomFactor = this.ZoomFactor,
                ZoomSlider = this.ZoomSlider,
                Locked = this.Locked,
                Description = this.Description,
                GUID = this.GUID,
            };
            if (bNewGUID) bse.GUID = Guid.NewGuid().ToString();

            return bse;
        }
    }

    public class BandStackFilter
    {
        public enum FilterReturnMode
        {
            FIRST = 0,
            LastVisited,
            Specific,
            Current
        }
        public string GUID = Guid.NewGuid().ToString();
        public string FilterName;   // a unique name
        public string FilterDescription;

        public readonly List<BandFrequencyData> FrequenciesToFilterOn = new List<BandFrequencyData>();
        public readonly List<DSPMode> ModesToFilterOn = new List<DSPMode>();
        public readonly List<DSPSubMode> SubModesToFilterOn = new List<DSPSubMode>();
        public readonly List<Band> BandsToFilterOn = new List<Band>();

        public bool FilterOnModes;
        public bool FilterOnSubModes;
        public bool FilterOnFrequencies;
        public bool FilterOnBands;
        public bool UserDefined;

        private FilterReturnMode m_filterReturnMode;
        private string m_sSpecificReturnGUID;

        private int m_nCurrentlySelectedIndex;
        private string m_sCurrentlySelectedGUID;

        private List<BandStackEntry> m_lstFilteredList;

        private BandStackEntry m_lastVisited;

        public BandStackFilter()
        {
            m_sCurrentlySelectedGUID = "";
            m_nCurrentlySelectedIndex = -1;
            FilterOnFrequencies = true;
            FilterOnBands = true;
            FilterOnModes = false;
            FilterOnSubModes = false;
            UserDefined = false;
            m_sSpecificReturnGUID = "";
            m_filterReturnMode = FilterReturnMode.Current;
            m_lstFilteredList = new List<BandStackEntry>();
            m_lastVisited = new BandStackEntry();
        }
        public BandStackFilter Copy()
        {
            BandStackFilter newBse = new BandStackFilter
            {
                GUID = this.GUID,
                FilterName = this.FilterName,
                FilterDescription = this.FilterDescription,
                FilterOnModes = this.FilterOnModes,
                FilterOnSubModes = this.FilterOnSubModes,
                FilterOnFrequencies = this.FilterOnFrequencies,
                FilterOnBands = this.FilterOnBands,
                UserDefined = this.UserDefined,
                m_filterReturnMode = this.m_filterReturnMode,
                m_sSpecificReturnGUID = this.m_sSpecificReturnGUID,
                m_nCurrentlySelectedIndex = this.m_nCurrentlySelectedIndex
            };

            //duplice last visited
            newBse.m_lastVisited = this.LastVisited.Copy();
            //dupe lists
            foreach(BandFrequencyData bfd in this.FrequenciesToFilterOn)
            {
                newBse.FrequenciesToFilterOn.Add(bfd.Copy());
            }
            foreach (DSPMode m in this.ModesToFilterOn)
            {
                newBse.ModesToFilterOn.Add(m);
            }
            foreach (DSPMode sm in this.SubModesToFilterOn)
            {
                newBse.ModesToFilterOn.Add(sm);
            }
            foreach (Band b in this.BandsToFilterOn)
            {
                newBse.BandsToFilterOn.Add(b);
            }

            return newBse;
        }
        public string GuidOfCurrentBlind {
            get {
                return m_sCurrentlySelectedGUID;
            }
            set { m_sCurrentlySelectedGUID = value; }
        }
        public FilterReturnMode ReturnMode {
            get { return m_filterReturnMode; }
            set {
                m_filterReturnMode = value;
            }
        }
        public string ReturnGUID {
            get { return m_sSpecificReturnGUID; }
            set {
                m_sSpecificReturnGUID = value;
            }
        }
        public int NumberOfEntries {
            get { return m_lstFilteredList.Count; }
        }
        public List<BandStackEntry> Entries {
            // NOTE: the use of entries is accesing a DEEP COPY
            // of the filters internal entries. 
            // You need to use UpdateEntry to make an actual change
            get {
                List<BandStackEntry> newBse = new List <BandStackEntry>();
                foreach(BandStackEntry bse in m_lstFilteredList)
                {
                    newBse.Add(bse.Copy());
                }
                return newBse; 
            }
        }
        public int IndexFromGUID(string sGUID)
        {
            int nRet = -1;

            for(int n = 0;n < m_lstFilteredList.Count;n++)
            {
                BandStackEntry bse = m_lstFilteredList[n];
                if (bse.GUID == sGUID)
                {
                    nRet = n;
                    break;
                }
            }

            return nRet;
        }

        public bool UpdateEntry(BandStackEntry bse)
        {
            bool bRet = false;

            int index = IndexFromGUID(bse.GUID);
            if(index != -1)
            {
                if (!m_lstFilteredList[index].Locked)
                {
                    // assign a copy over the top of the original
                    // original will be auto cleaned up
                    m_lstFilteredList[index] = bse.Copy();
                }
                m_lstFilteredList[index].Locked = bse.Locked; // update the lock state anyway, otherwise no way to unlock it
                bRet = true;

                // replace master list entry
                int n = BandStackManager.IndexFromGUID(bse.GUID);
                if (n != -1)
                {
                    BandStackManager.BandstackEntries.RemoveAt(n);
                    BandStackManager.BandstackEntries.Insert(n, m_lstFilteredList[index].Copy());
                }
            }

            return bRet;
        }
        public bool UpdateCurrentWithLastVisitedData(bool bCheckForFreqDupe = false)
        {
            bool bRet = false;
            if (m_nCurrentlySelectedIndex < 0 || m_nCurrentlySelectedIndex > m_lstFilteredList.Count - 1) return bRet;
            if (m_lstFilteredList[m_nCurrentlySelectedIndex].Locked) return bRet;  // only do if unlocked

            if (bCheckForFreqDupe)
            {
                //BandStackEntry possibleDupeBse = FindEntriesForFrequency(m_lastVisited.Frequency);
                //if(possibleDupeBse != null)
                //{
                //    int nDupe = IndexFromGUID(possibleDupeBse.GUID);
                //    if (nDupe != -1 && nDupe != m_nCurrentlySelectedIndex) return bRet; // abort if there is an entry that has the same frequency at a different index
                //}
                List<BandStackEntry> entries = FindEntriesForFrequency(m_lastVisited.Frequency);
                if (entries.Count > 0)
                {
                    foreach (BandStackEntry entry in entries)
                    {
                        int index = IndexFromGUID(entry.GUID);
                        if (index != -1 && index != m_nCurrentlySelectedIndex) return false;// abort if there is an entry that has the same frequency at a different index
                    }
                }
            }

            BandStackEntry bse = m_lstFilteredList[m_nCurrentlySelectedIndex];

            bse.Band = m_lastVisited.Band;
            bse.Filter = m_lastVisited.Filter;
            bse.Frequency = m_lastVisited.Frequency;
            bse.CentreFrequency = m_lastVisited.CentreFrequency;
            bse.CTUNEnabled = m_lastVisited.CTUNEnabled;
            bse.Filter = m_lastVisited.Filter;
            bse.Mode = m_lastVisited.Mode;
            bse.SubMode = m_lastVisited.SubMode;
            bse.ZoomFactor = m_lastVisited.ZoomFactor;
            bse.ZoomSlider = m_lastVisited.ZoomSlider;

            // remove from main list
            int n = BandStackManager.IndexFromGUID(bse.GUID);
            if (n != -1)
            {
                BandStackManager.BandstackEntries.RemoveAt(n);
                BandStackManager.BandstackEntries.Insert(n, bse.Copy());
                bRet = true;
            }

            return bRet;
        }
        public BandStackEntry LastVisited {
            get { return m_lastVisited; }
        }
        public void Remove(int index)
        {
            if (index < 0 || index > m_lstFilteredList.Count - 1) return;

            m_lstFilteredList.RemoveAt(index);
            m_nCurrentlySelectedIndex--;
        }
        public int IndexOfCurrentBlind {
            get { return m_nCurrentlySelectedIndex; }
            set { m_nCurrentlySelectedIndex = value; } // No check version that can be used by the DB. Later calls generatefilter will fix this if needed
        }
        public int IndexOfCurrent {
            get { return m_nCurrentlySelectedIndex; }
            set {
                if(m_lstFilteredList.Count == 0)
                {
                    m_nCurrentlySelectedIndex = -1;
                    return;
                }

                int v = value;
                if (v > m_lstFilteredList.Count - 1) v = m_lstFilteredList.Count - 1;

                m_nCurrentlySelectedIndex = v;
            }
        }        
        public string GuidOfCurrent {
            get {
                string sRet = "";

                if(m_nCurrentlySelectedIndex >=0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
                {
                    sRet = m_lstFilteredList[m_nCurrentlySelectedIndex].GUID;
                }

                return sRet;
            }
        }
        public void RemoveCurrent()
        {
            if (m_lstFilteredList.Count == 0 || m_nCurrentlySelectedIndex == -1) return;

            m_lstFilteredList.RemoveAt(m_nCurrentlySelectedIndex);
            // mark to not be included again in this filter

            m_nCurrentlySelectedIndex--;
        }
        public BandStackEntry EntryByIndex(int index)
        {
            if (m_lstFilteredList.Count == 0) return null;
            if (index > m_lstFilteredList.Count - 1) return null;

            return m_lstFilteredList[index].Copy();
        }
        public BandStackEntry SelectInitial()
        {
            BandStackEntry bseRet = null;

            if (m_nCurrentlySelectedIndex == -1 && m_lstFilteredList.Count > 0) m_nCurrentlySelectedIndex = 0;
 
            switch (m_filterReturnMode)
            {
                case FilterReturnMode.Current:                    
                    if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
                    {
                        bseRet = m_lstFilteredList[m_nCurrentlySelectedIndex].Copy();
                    }
                    break;
                case FilterReturnMode.Specific:
                    int n = IndexFromGUID(ReturnGUID);
                    if(n < 0)
                    {
                        // failed to find this guid, just return to current mode
                        m_filterReturnMode = FilterReturnMode.Current;
                    }
                    else
                    {
                        m_nCurrentlySelectedIndex = n;
                    }
                    if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
                    {
                        bseRet = m_lstFilteredList[m_nCurrentlySelectedIndex].Copy();
                    }
                    break;
                case FilterReturnMode.LastVisited:
                    m_nCurrentlySelectedIndex = -1;
                    bseRet = m_lastVisited.Copy();
                    break;
                default:
                    break;
            }

            if (bseRet != null)
            {
                //reset last visited to this
                BandStackEntry bseLV = bseRet.Copy();// true);
                bseLV.Locked = false; // incase the inital was locked
                m_lastVisited = bseLV;                
            }
  
            return bseRet;
        }
        public BandStackEntry First()
        {
            BandStackEntry bseRet = null;            

            if (m_lstFilteredList.Count > 0)
            {
                bseRet = m_lstFilteredList[0].Copy();
            }

            return bseRet;
        }
        public BandStackEntry Current()
        {
            BandStackEntry bseRet = null;

            if (m_filterReturnMode == FilterReturnMode.LastVisited && m_nCurrentlySelectedIndex == -1)
            {
                bseRet = m_lastVisited.Copy();
            }
            else
            {
                if (m_nCurrentlySelectedIndex == -1 && m_lstFilteredList.Count > 0) m_nCurrentlySelectedIndex = 0;

                if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
                {
                    bseRet = m_lstFilteredList[m_nCurrentlySelectedIndex].Copy();
                }
            }

            return bseRet;
        }
        public BandStackEntry Next()
        {
            if (m_nCurrentlySelectedIndex == -1 && m_lstFilteredList.Count > 0)
            {
                m_nCurrentlySelectedIndex = 0;
            }
            else
            {
                m_nCurrentlySelectedIndex++;
                if (m_nCurrentlySelectedIndex > m_lstFilteredList.Count - 1) m_nCurrentlySelectedIndex = 0;
            }

            if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
            {
                return m_lstFilteredList[m_nCurrentlySelectedIndex].Copy();
            }
            else
                return null;
        }
        public BandStackEntry Previous()
        {
            if (m_nCurrentlySelectedIndex == -1 && m_lstFilteredList.Count > 0)
            {
                m_nCurrentlySelectedIndex = 0;
            }
            else
            {
                m_nCurrentlySelectedIndex--;
                if (m_nCurrentlySelectedIndex < 0) m_nCurrentlySelectedIndex = m_lstFilteredList.Count - 1;
            }

            if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
            {
                return m_lstFilteredList[m_nCurrentlySelectedIndex].Copy();
            }
            else
                return null;
        }

        public List<BandStackEntry> FindEntriesForFrequency(double frequency)
        {
            //BandStackEntry bse = null;
            IEnumerable<BandStackEntry> data = from item in m_lstFilteredList
                                               where item.Frequency == frequency
                                               select item;
            //if (data.Count() == 1)
            //{
            //    bse = data.First();
            //}

            //return bse;

            List<BandStackEntry> bse_list = new List<BandStackEntry>();
            foreach(BandStackEntry bse in data)
            {
                bse_list.Add(bse.Copy(false));
            }
            return bse_list;
        }
        public List<BandStackEntry> FindForFrequencyRange(double frequencyLow, double frequencyHigh)
        {
            

            IEnumerable<BandStackEntry> data = from item in m_lstFilteredList
                                               where item.Frequency >= frequencyLow && item.Frequency <= frequencyHigh
                                               select item;

            List<BandStackEntry> bsEntries = data.ToList();
            
            return bsEntries;
        }
        public void GenerateFilteredList(bool bMaintainSelected, bool bInitalising = false)
        {
            // inialising flag so use current values from the db restore

            int nOldCurrentlySelectedIndex = m_nCurrentlySelectedIndex;
            string sOldSelectedGUID = m_sCurrentlySelectedGUID; // this will be from DB only as part of CurrentlySelectedGUIDBlind
            
            if (!bInitalising)
            {
                sOldSelectedGUID = ""; // we are not initalising so dont care about the old DB guid
                if (m_nCurrentlySelectedIndex >= 0 && m_nCurrentlySelectedIndex < m_lstFilteredList.Count)
                {
                    sOldSelectedGUID = m_lstFilteredList[m_nCurrentlySelectedIndex].GUID;
                }
            }

            // generate a filtered list from all frequency entries that are in the gigantic list
            m_lstFilteredList.Clear();
            m_nCurrentlySelectedIndex = -1;            

            IEnumerable<BandStackEntry> data;

            List<BandStackEntry> bsesTmp = new List<BandStackEntry>();

            if (FilterOnBands)
            {
                foreach (Band b in BandsToFilterOn)
                {
                    data = from item in BandStackManager.BandstackEntries
                           where (item.Band == b)
                           orderby item.Frequency ascending
                           select item;

                    bsesTmp.AddRange(data);
                }
            }
            if (FilterOnFrequencies)
            {
                bsesTmp = FilterOnBands ? bsesTmp : BandStackManager.BandstackEntries;
                List <BandStackEntry> tmp = new List<BandStackEntry>();
                // we have some filtered by band, let us sub filter on freq
                foreach (BandFrequencyData bfd in FrequenciesToFilterOn)
                {
                    data = from item in bsesTmp
                            where (bfd.lowOnly == true && bfd.low == item.Frequency) || (bfd.lowOnly == false && (item.Frequency >= bfd.low && item.Frequency < bfd.high))
                            orderby item.Frequency ascending
                            select item;

                    tmp.AddRange(data);
                }
                bsesTmp.Clear();
                bsesTmp.AddRange(tmp);
            }
            if (FilterOnModes)
            {
                bsesTmp = FilterOnBands || FilterOnFrequencies ? bsesTmp : BandStackManager.BandstackEntries;
                List<BandStackEntry> tmp = new List<BandStackEntry>();
                // we have some filtered by band and/or frequency, let us sub filter on mode
                foreach (DSPMode mode in ModesToFilterOn)
                {
                    data = from item in bsesTmp
                            where (item.Mode == mode)
                            orderby item.Frequency ascending
                            select item;

                    tmp.AddRange(data);
                }
                bsesTmp.Clear();
                bsesTmp.AddRange(tmp);
            }
            if (FilterOnSubModes)
            {
                bsesTmp = FilterOnBands || FilterOnFrequencies || FilterOnSubModes ? bsesTmp : BandStackManager.BandstackEntries;
                List<BandStackEntry> tmp = new List<BandStackEntry>();
                // we have some filtered by band and/or frequency, let us sub filter on mode
                foreach (DSPSubMode subMode in SubModesToFilterOn)
                {
                    data = from item in bsesTmp
                            where (item.SubMode == subMode)
                            orderby item.Frequency ascending
                            select item;

                    tmp.AddRange(data);
                }
                bsesTmp.Clear();
                bsesTmp.AddRange(tmp);
            }

            List<BandStackEntry> bsesTmpSorted;
            // and result
            if (FilterOnBands || FilterOnFrequencies || FilterOnModes || FilterOnSubModes)
            {
                // sort
                bsesTmpSorted = bsesTmp.OrderBy(i => i.Frequency).ToList();
            }
            else
            {
                // sort
                // and add in full set as we are not filtering on anything
                bsesTmpSorted = BandStackManager.BandstackEntries.OrderBy(i => i.Frequency).ToList();
            }

            m_lstFilteredList.AddRange(bsesTmpSorted);

            // sort out the selected entry
            if (bMaintainSelected)
            {
                int n = IndexFromGUID(sOldSelectedGUID);
                if (n == -1)
                {
                    // perhaps it got deleted
                    n = nOldCurrentlySelectedIndex;
                }
                if (m_lstFilteredList.Count == 0) n = -1; // none in list
                if (n > m_lstFilteredList.Count - 1) n = m_lstFilteredList.Count - 1; // off end of list
                m_nCurrentlySelectedIndex = n;
            }

            if (m_lstFilteredList.Count > 0 && m_nCurrentlySelectedIndex == -1) m_nCurrentlySelectedIndex = 0; // set the current to the first entry if we have one
        }
    }

    static class BandStackManager
    {        
        private static List<BandStackEntry> m_lstEntries; // a big gigantic list of all the entries       
        private static Dictionary<string, BandStackFilter> m_dictFilters; // the filters
        private static FRSRegion m_Region; // region that everything works off, assigning a different region will rebuild just about everything
        private static bool m_bExtended;   // has side effect of setting region
        private static bool m_bReady;
        private static FRSRegion m_oldRegion = FRSRegion.FIRST;
        private static bool m_oldExtended = false;
        private static List<BandFrequencyData> m_frequencyData; //cached version
        private static bool m_bListsNeedRebuild;

        static BandStackManager()
        {
            m_bListsNeedRebuild = true;
            m_bReady = false;
            m_Region = FRSRegion.FIRST;
            m_bExtended = false;
            m_lstEntries = new List<BandStackEntry>();
            m_dictFilters = new Dictionary<string, BandStackFilter>();
        }

        public static List<BandStackEntry> BandstackEntries {
            get {
                if (m_bListsNeedRebuild) initLists();

                return m_lstEntries; 
            }
        }
        public static void SaveToDB()
        {
            DB.AddBandStack2Entry(m_lstEntries);

            foreach (KeyValuePair<string, BandStackFilter> kvp in m_dictFilters)
            {
                BandStackFilter bsf = kvp.Value;

                DB.SaveBandStack2Filter(bsf);
            }
        }
        public static void RegionReset()
        {
            DB.RemoveAllBandStack2Entries();
            m_bListsNeedRebuild = true;
            initLists();
        }
        private static void initLists()
        {
            m_bListsNeedRebuild = false;
            m_lstEntries.Clear();
            m_dictFilters.Clear();

            //recover entries from DB
            List<BandStackEntry> dbRecoverEntries = DB.GetBandStack2Entries();
            foreach (BandStackEntry bse in dbRecoverEntries)
            {
                //bool bInList = m_lstEntries.Any(i => (i.Frequency == bse.Frequency && i.Band == bse.Band));
                //if (bInList) m_lstEntries.Remove(bse);

                m_lstEntries.Add(bse.Copy());
            }
            if (m_lstEntries.Count == 0)
            {
                // this will be the case with a new DB
                addStandardFrequencies();   // adds basic frequencies into the list of all frequencies based on region
            }

            addStandardFilters();   // adds basic filters for each band that uses current region to define them

            //recover filters from DB
            Dictionary<string, BandStackFilter> dbRecoverFilters = DB.GetBandStack2Filters();
            //remove any from our lists, and use the ones from dbRecover
            foreach (KeyValuePair<string, BandStackFilter> kvp in dbRecoverFilters)
            {
                BandStackFilter bsf = kvp.Value;
                string sKey = kvp.Key;

                if (m_dictFilters.ContainsKey(sKey))
                {
                    m_dictFilters.Remove(sKey);
                }

                internalAddFilter(bsf.Copy(), true);
            }
            m_bReady = true;
        }
        public static bool Ready {
            get { return m_bReady; }
        }
        public static int IndexFromGUID(string sGUID)
        {
            int nRet = -1;

            for (int n = 0; n < m_lstEntries.Count; n++)
            {
                BandStackEntry bse = m_lstEntries[n];
                if (bse.GUID == sGUID)
                {
                    nRet = n;
                    break;
                }
            }

            return nRet;
        }        
        private static void addStandardFilters()
        {
            Array bands = Enum.GetValues(typeof(Band));
            foreach (Band b in bands)
            {
                if(b != Band.FIRST && b != Band.LAST)
                {
                    List<BandFrequencyData> bfds = GetFrequencyRangesForBand(b, m_bExtended, m_Region);

                    BandStackFilter bsf = new BandStackFilter();

                    bsf.FilterName = b.ToString();
                    bsf.FilterDescription = "";

                    bsf.FilterOnBands = true;       // the default filters on filter on band, as some standard bandstack entries span > 1 band
                    bsf.FilterOnFrequencies = false;
                    bsf.FilterOnModes = false;
                    bsf.UserDefined = false; // a flag used elsewhere to protect these standard filters

                    bsf.BandsToFilterOn.Add(b);

                    bsf.FrequenciesToFilterOn.AddRange(bfds);

                    internalAddFilter(bsf);
                }
            }
        }
        public static BandStackFilter GetFilter(string sFilterName, bool bIncludeUserDefined = true)
        {
            if (m_dictFilters.ContainsKey(sFilterName))
            {
                if((bIncludeUserDefined && m_dictFilters[sFilterName].UserDefined) || !m_dictFilters[sFilterName].UserDefined)
                    return m_dictFilters[sFilterName];
            }
            return null;
        }
        public static BandStackFilter GetFilter(Band b, bool bIncludeUserDefined = true)
        {
            string sFilterName = b.ToString();
            return GetFilter(sFilterName, bIncludeUserDefined);
        }
        public static List<BandStackFilter> GetFilters(Band b, bool onlyFirst = false, bool bIncludeUserDefined = true)
        {
            List<BandStackFilter> bsfilters = new List<BandStackFilter>();

            foreach(KeyValuePair<string, BandStackFilter> kvpBsf in m_dictFilters)
            {
                BandStackFilter bsf = kvpBsf.Value;
                if ((bIncludeUserDefined && bsf.UserDefined) || !bsf.UserDefined)
                {
                    foreach (Band bb in bsf.BandsToFilterOn)
                    {
                        if (b == bb)
                        {
                            bsfilters.Add(bsf);
                            if (onlyFirst) break;
                        }
                    }
                }
            }

            return bsfilters;
        }
        public static bool DoesFilterNameExist(string sFilterName)
        {
            return m_dictFilters.ContainsKey(sFilterName);
        }
        private static bool internalAddFilter(BandStackFilter bsf, bool bInitalising = false)
        {
            if (m_dictFilters.ContainsKey(bsf.FilterName)) return false;

            bsf.GenerateFilteredList(true, bInitalising);
            bsf.SelectInitial();

            m_dictFilters.Add(bsf.FilterName, bsf);

            return true;
        }
        public static bool AddFilter(BandStackFilter bsf)
        {
            return internalAddFilter(bsf);
        }
        public static void AddEntry(BandStackEntry bse)
        {
            bse.CentreFrequency = Math.Round(bse.CentreFrequency, 6);
            bse.Frequency = Math.Round(bse.Frequency, 6);

            m_lstEntries.Add(bse);

            DB.AddBandStack2Entry(bse);
        }
        public static bool DeleteEntry(BandStackEntry bse)
        {
            bool bRet = false;
            int n = IndexFromGUID(bse.GUID);
            if (n != -1)
            {
                m_lstEntries.RemoveAt(n);
                bRet = true;

                DB.RemoveBandStack2Entry(bse);
            }
            return bRet;
        }
        public static bool DeleteEntry(string sGUID)
        {
            bool bRet = false;
            int n = IndexFromGUID(sGUID);
            if (n != -1)
            {
                m_lstEntries.RemoveAt(n);
                bRet = true;
            }
            return bRet;
        }


        public static FRSRegion Region {
            get { return m_Region; }
            set 
            {
                FRSRegion tmp = value;

                if (tmp != m_Region)
                {
                    m_Region = tmp;
                    m_bListsNeedRebuild = true;
                    initLists();
                }
            }
        }
        public static bool Extended {
            get { return m_bExtended; }
            set 
            {
                bool bTmp = value;

                if (bTmp != m_bExtended) // we need a region to actually do anything
                {
                    m_bExtended = bTmp;
                    m_bListsNeedRebuild = true;
                    if (m_Region != FRSRegion.FIRST) initLists();
                }
            }
        }
        #region Helpers

        public static List<BandFrequencyData> GetBandFrequencyDataForFrequency(double frequency, bool extended, FRSRegion region, Band band = Band.LAST)
        {
            // returns all bands that would hold this frequency
            if (extended) region = FRSRegion.Extended;
            List<BandFrequencyData> fd = frequencyData(region);

            IEnumerable<BandFrequencyData> data;
            if (band == Band.LAST)
            {
                // ignore band
                data = from item in fd
                            where ((item.lowOnly == true && frequency == item.low) || ((frequency >= item.low && frequency < item.high) && item.lowOnly == false))
                            && item.region == region
                            select item;
            }
            else
            {
                data = from item in fd
                            where ((item.lowOnly == true && frequency == item.low) || ((frequency >= item.low && frequency < item.high) && item.lowOnly == false))
                            && item.region == region && item.band == band
                            select item;
            }
            List<BandFrequencyData> outList = data.ToList();

            if (outList.Count == 0)
            {
                if (band == Band.LAST)
                {
                    outList.Add(new BandFrequencyData(0, 0, Band.GEN, BandType.GEN, true, region)); // this is just a generic band if we dont find one
                }
                else
                {
                    outList.Add(new BandFrequencyData(0, 0, band, bandToBandType(band), true, region)); // this is just a generic band if we dont find one
                }
            }

            return outList;
        }
        public static List<BandFrequencyData> GetFrequencyRangesForBand(Band band, bool extended, FRSRegion region)
        {
            if (extended) region = FRSRegion.Extended;
            List<BandFrequencyData> fd = frequencyData(region);              

            IEnumerable<BandFrequencyData> data = from item in fd
                                         where item.band == band && item.region == region
                                         //orderby item.bandType ascending
                                         select item;

            List<BandFrequencyData> outList = data.ToList();

            return outList;
        }
        public static bool IsFrequencyInBandType(double frequency, BandType bandType, bool extended, FRSRegion region)
        {
            if (extended) region = FRSRegion.Extended;
            List<BandFrequencyData> bands = GetBandFrequencyDataForFrequency(frequency, extended, region);
            bool bRet = false;
            foreach(BandFrequencyData bfd in bands)
            {
                if(bfd.bandType == bandType)
                {
                    bRet = true;
                    break;
                }
            }
            return bRet;
        }
        public static bool IsOKToTX(double frequency, bool extended, FRSRegion region)
        {
            bool bRet = false;
            if (extended) region = FRSRegion.Extended;
            List<BandFrequencyData> bands = GetBandFrequencyDataForFrequency(frequency, extended, region);
            if (bands.Count > 0)
            {
                foreach(BandFrequencyData bfd in bands)
                {
                    if(bfd.bandType == BandType.HF)
                    {
                        if (bfd.band != Band.WWV && bfd.band != Band.BLMF)
                        {
                            bRet = true;
                            break;
                        }                            
                    }
                }
            }
            return bRet;
        }
        private static void addStandardFrequencies()
        {
            switch (m_Region)
            {
                case FRSRegion.Australia:
                    AddRegion2BandStack();
                    break;
                case FRSRegion.US:
                    AddRegion2BandStack(true);
                    AddUS_PlusBandStack();
                    break;
                case FRSRegion.Japan:
                    //AddRegionJapanBandStack();
                    AddRegion3BandStack();
                    break;
                case FRSRegion.India:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.Spain:
                case FRSRegion.Slovakia:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.Europe:
                case FRSRegion.Italy_Plus:
                case FRSRegion.Germany:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.Israel:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.UK:
                    AddUK_PlusBandStack();
                    break;
                case FRSRegion.Norway:
                case FRSRegion.Denmark:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.Latvia:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.Bulgaria:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.Greece:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.Hungary:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.Netherlands:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.France:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.Russia:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.Sweden:
                    AddSwedenBandStack();
                    break;
                case FRSRegion.Region1:
                    AddRegion1BandStack();
                    break;
                case FRSRegion.Region2:
                    AddRegion2BandStack();
                    break;
                case FRSRegion.Region3:
                    AddRegion3BandStack();
                    break;
            }

            // add to all
            AddBandStackSWL();
        }
        private static BandType bandToBandType(Band b)
        {
            switch (b)
            {
                case Band.B160M:
                case Band.B80M:
                case Band.B60M:
                case Band.B40M:
                case Band.B30M:
                case Band.B20M:
                case Band.B17M:
                case Band.B15M:
                case Band.B12M:
                case Band.B10M:
                case Band.B6M:
                    return BandType.HF;
                case Band.B120M:
                case Band.B90M:
                case Band.B61M:
                case Band.B49M:
                case Band.B41M:
                case Band.B31M:
                case Band.B25M:
                case Band.B22M:
                case Band.B19M:
                case Band.B16M:
                case Band.B14M:
                case Band.B13M:
                case Band.B11M:
                    return BandType.GEN;
                case Band.WWV:
                    return BandType.GEN;
                case Band.VHF0:
                case Band.VHF1:
                case Band.VHF2:
                case Band.VHF3:
                case Band.VHF4:
                case Band.VHF5:
                case Band.VHF6:
                case Band.VHF7:
                case Band.VHF8:
                case Band.VHF9:
                case Band.VHF10:
                case Band.VHF11:
                case Band.VHF12:
                case Band.VHF13:
                    return BandType.VHF;
                default:
                    return BandType.GEN;
            }
        }        
        public static Band GetNearestBandForFrequency(double freq, bool ignoreGen, bool ignoreWWV)
        {
            Band nearestBand = Band.FIRST;
            double smallestDifference = double.MaxValue;

            foreach (BandFrequencyData data in m_frequencyData)
            {
                if (ignoreGen && data.bandType == BandType.GEN)
                {
                    continue;
                }
                if (ignoreGen && data.band == Band.WWV)
                {
                    continue;
                }

                double low = data.low;
                double high = data.high;

                if (data.lowOnly)
                {
                    double difference = Math.Abs(freq - low);
                    if (difference < smallestDifference)
                    {
                        smallestDifference = difference;
                        nearestBand = data.band;
                    }
                }
                else
                {
                    if (freq >= low && freq <= high)
                    {
                        return data.band;
                    }
                    else
                    {
                        double differenceToLow = Math.Abs(freq - low);
                        double differenceToHigh = Math.Abs(freq - high);
                        double difference = Math.Min(differenceToLow, differenceToHigh);

                        if (difference < smallestDifference)
                        {
                            smallestDifference = difference;
                            nearestBand = data.band;
                        }
                    }
                }
            }

            return nearestBand;
        }

        public static BandType GetBandTypeForFrequency(double frequency)
        {
            BandType ret = BandType.GEN;
            if (m_frequencyData == null || m_frequencyData.Count == 0) return ret;

            List<BandFrequencyData> bfd_for_freq = m_frequencyData.Where(bfd => (bfd.lowOnly == true && bfd.low == frequency) 
                                    || (bfd.lowOnly == false && (frequency >= bfd.low && frequency < bfd.high))).ToList();

            if (bfd_for_freq.Count > 0) return bfd_for_freq.First().bandType;

            return ret;
        }
        private static List<BandFrequencyData> frequencyData(FRSRegion region)
        {
            if (m_bExtended) region = FRSRegion.Extended;

            if ((region != m_oldRegion) || (m_bExtended != m_oldExtended))
            {
                if (m_frequencyData != null) m_frequencyData.Clear();
                else m_frequencyData = new List<BandFrequencyData>();

                m_oldRegion = region;
                m_oldExtended = m_bExtended;
            }
            else return m_frequencyData;

            List<BandFrequencyData> frequencyData = m_frequencyData;

            switch (region)
            {
                case FRSRegion.Extended:
                    frequencyData.Add(new BandFrequencyData(0.20, 1.8, Band.BLMF, BandType.GEN, false, region));

                    frequencyData.Add(new BandFrequencyData(2.5, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(10.0, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(15.0, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(20.0, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(25.0, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(3.33, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(7.85, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(14.67, 0, Band.WWV, BandType.HF, true, region));

                    frequencyData.Add(new BandFrequencyData(2.3, 3.0, Band.B120M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(3.0, 3.5, Band.B90M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(4.0, 5.1, Band.B61M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(5.5, 7, Band.B49M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(7.3, 9, Band.B41M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(9.0, 10.1, Band.B31M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(10.16, 13.57, Band.B25M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(13.57, 14.0, Band.B22M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(15.1, 17.0, Band.B19M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(17, 18.068, Band.B16M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(18.168, 21.0, Band.B14M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(21.450, 24.89, Band.B13M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(24.99, 28.0, Band.B11M, BandType.GEN, false, region));

                    frequencyData.Add(new BandFrequencyData(1.8/*0.0*/, 2.75, Band.B160M, BandType.HF, false, region)); //MW0LGE_21e changed to be at least somewhere near 160m
                    frequencyData.Add(new BandFrequencyData(2.75, 5.25, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.25, 7.0, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 8.7, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(8.7, 12.075, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(12.075, 16.209, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(16.209, 19.584, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(19.584, 23.17, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(23.17, 26.495, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(26.495, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));                    

                    break;

                case FRSRegion.US:
                    frequencyData.Add(new BandFrequencyData(1.8, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 4, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.1, 5.5, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.3, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));

                    frequencyData.Add(new BandFrequencyData(2.5, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(10.0, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(15.0, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(20.0, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(25.0, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(3.33, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(7.85, 0, Band.WWV, BandType.HF, true, region));
                    frequencyData.Add(new BandFrequencyData(14.67, 0, Band.WWV, BandType.HF, true, region));

                    frequencyData.Add(new BandFrequencyData(0.20, 1.8, Band.BLMF, BandType.GEN, false, region));

                    frequencyData.Add(new BandFrequencyData(1.8, 3.0, Band.B120M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(3.0, 4.1, Band.B90M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(4.1, 5.06, Band.B61M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(5.06, 7.2, Band.B49M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(7.2, 9, Band.B41M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(9.0, 11.6, Band.B31M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(11.6, 13.57, Band.B25M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(13.57, 13.87, Band.B22M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(13.87, 17.0, Band.B19M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(17, 18.0, Band.B16M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(18.0, 21.0, Band.B14M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 25, Band.B13M, BandType.GEN, false, region));
                    frequencyData.Add(new BandFrequencyData(25.0, 28.0, Band.B11M, BandType.GEN, false, region));
                    break;

                case FRSRegion.India:
                    frequencyData.Add(new BandFrequencyData(1.81, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.9, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Spain:
                    frequencyData.Add(new BandFrequencyData(1.81, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.1, 5.5, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Europe:
                    frequencyData.Add(new BandFrequencyData(1.81, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.1, 5.5, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Israel:
                    frequencyData.Add(new BandFrequencyData(1.81, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 5.5, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.UK:
                    //list.Add(new BandFrequencyData(0.1357, 0.1378, Band.B2200M, BandType.HF, false, region));
                    //list.Add(new BandFrequencyData(0.472, 0.479, Band.B600M, BandType.HF, false, region)); // !!
                    frequencyData.Add(new BandFrequencyData(1.81, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.25, 5.41, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.03, 52.0, Band.B6M, BandType.HF, false, region));    // this is 51 in console
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Italy_Plus:
                    frequencyData.Add(new BandFrequencyData(1.83, 1.85, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 6.975, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(6.975, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.08, 51.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Japan:
                    //list.Add(new BandFrequencyData(0.1357, 0.1378, Band.B2200M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(1.83, 1.9125, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.805, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(4.629995, 4.630005, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(6.975, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Australia:
                    //list.Add(new BandFrequencyData(0.1357, 0.1378, Band.B2200M, BandType.HF, false, region));
                    //list.Add(new BandFrequencyData(0.472, 0.479, Band.B600M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(1.81, 1.875, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.3, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Norway:
                    frequencyData.Add(new BandFrequencyData(1.8, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.25, 5.45, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Denmark:
                    frequencyData.Add(new BandFrequencyData(1.8, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.25, 5.45, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Latvia:
                    frequencyData.Add(new BandFrequencyData(1.8, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Slovakia:
                    frequencyData.Add(new BandFrequencyData(1.8, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Bulgaria:
                    frequencyData.Add(new BandFrequencyData(1.8, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Greece:
                    frequencyData.Add(new BandFrequencyData(1.8, 1.85, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Hungary:
                    frequencyData.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.1, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Netherlands:
                    frequencyData.Add(new BandFrequencyData(1.8, 1.88, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.1, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.France:
                    frequencyData.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Russia:
                    frequencyData.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.0, 7.0, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 25.14, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Sweden:
                    frequencyData.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.31, 5.93, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Germany:
                    //list.Add(new BandFrequencyData(0.472, 0.479, Band.B2200M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(1.8, 2.0, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.3515, 5.3665, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 51.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Region1:
                    //list.Add(new BandFrequencyData(0.1357, 0.1378, Band.B2200M, BandType.HF, false, region));
                    //list.Add(new BandFrequencyData(0.472, 0.479, Band.B600M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(1.81, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.8, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.3515, 5.3665, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 52.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Region2:
                    //list.Add(new BandFrequencyData(0.1357, 0.1378, Band.B2200M, BandType.HF, false, region));
                    //list.Add(new BandFrequencyData(0.472, 0.479, Band.B600M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(1.80, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 4.0, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.3515, 5.3665, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.3, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;

                case FRSRegion.Region3:
                    //list.Add(new BandFrequencyData(0.1357, 0.1378, Band.B2200M, BandType.HF, false, region));
                    //list.Add(new BandFrequencyData(0.472, 0.479, Band.B600M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(1.80, 2, Band.B160M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(3.5, 3.9, Band.B80M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(5.3515, 5.3665, Band.B60M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(7.0, 7.2, Band.B40M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(10.1, 10.15, Band.B30M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(14.0, 14.35, Band.B20M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(18.068, 18.168, Band.B17M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(21.0, 21.45, Band.B15M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(24.89, 24.99, Band.B12M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(28.0, 29.7, Band.B10M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(50.0, 54.0, Band.B6M, BandType.HF, false, region));
                    frequencyData.Add(new BandFrequencyData(144.0, 148.0, Band.B2M, BandType.VHF, false, region));
                    break;
            }

            if (!(region == FRSRegion.US || region == FRSRegion.Extended))
            {
                // us and extended have their own version of this, all others do not

                frequencyData.Add(new BandFrequencyData(2.5, 0, Band.WWV, BandType.HF, true, region));
                frequencyData.Add(new BandFrequencyData(5.0, 0, Band.WWV, BandType.HF, true, region));
                frequencyData.Add(new BandFrequencyData(10.0, 0, Band.WWV, BandType.HF, true, region));
                frequencyData.Add(new BandFrequencyData(15.0, 0, Band.WWV, BandType.HF, true, region));
                frequencyData.Add(new BandFrequencyData(20.0, 0, Band.WWV, BandType.HF, true, region));
                frequencyData.Add(new BandFrequencyData(25.0, 0, Band.WWV, BandType.HF, true, region));
                frequencyData.Add(new BandFrequencyData(3.33, 0, Band.WWV, BandType.HF, true, region));
                frequencyData.Add(new BandFrequencyData(7.85, 0, Band.WWV, BandType.HF, true, region));
                frequencyData.Add(new BandFrequencyData(14.67, 0, Band.WWV, BandType.HF, true, region));

                frequencyData.Add(new BandFrequencyData(0.20, 1.8, Band.BLMF, BandType.GEN, false, region));
                
                frequencyData.Add(new BandFrequencyData(2.3, 3.0, Band.B120M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(3.0, 3.5, Band.B90M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(4.1, 5.06, Band.B61M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(5.06, 7.2, Band.B49M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(7.2, 9, Band.B41M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(9.0, 9.99, Band.B31M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(11.6, 13.57, Band.B25M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(13.57, 13.87, Band.B22M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(15.1, 17.0, Band.B19M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(17, 18.0, Band.B16M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(18.0, 21.0, Band.B14M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(21.0, 25, Band.B13M, BandType.GEN, false, region));
                frequencyData.Add(new BandFrequencyData(25.0, 28.0, Band.B11M, BandType.GEN, false, region));
            }                     

            return frequencyData;
        }        
        public static string ModeToString(DSPMode mode)
        {
            return mode.ToString();
        }
        public static DSPMode StringToMode(string mode)
        {
            bool bOk = Enum.TryParse(mode, out DSPMode result);
            if (bOk) return result;
            else return DSPMode.FIRST;
        }
        public static string FilterToString(Filter filter)
        {
            return filter.ToString();
        }
        public static Filter StringToFilter(string filter)
        {
            bool bOk = Enum.TryParse(filter, out Filter result);
            if (bOk) return result;
            else return Filter.NONE; // perhaps should be first
        }
        public static Color BandToColour(Band b)
        {
            Color c;
            switch (b)
            {
                case Band.GEN:
                    c = Color.White;
                    break;
                case Band.B160M:
                case Band.B80M:
                case Band.B60M:
                case Band.B40M:
                case Band.B30M:
                case Band.B20M:
                case Band.B17M:
                case Band.B15M:
                case Band.B12M:
                case Band.B10M:
                case Band.B6M: 
                case Band.B2M: 
                    c = Color.White;
                    break;
                case Band.WWV: 
                    c = Color.Green;
                    break;
                case Band.BLMF:
                    c = Color.White;
                    break;
                case Band.B120M: 
                case Band.B90M:
                case Band.B61M: 
                case Band.B49M:
                case Band.B41M:
                case Band.B31M:
                case Band.B25M:
                case Band.B22M:
                case Band.B19M:
                case Band.B16M: 
                case Band.B14M:
                case Band.B13M:
                case Band.B11M: 
                    c = Color.Coral;
                    break;
                case Band.VHF0: 
                case Band.VHF1: 
                case Band.VHF2:
                case Band.VHF3: 
                case Band.VHF4: 
                case Band.VHF5:
                case Band.VHF6:
                case Band.VHF7: 
                case Band.VHF8: 
                case Band.VHF9: 
                case Band.VHF10: 
                case Band.VHF11: 
                case Band.VHF12: 
                case Band.VHF13:
                    c = Color.Gold;
                    break;
                default:
                    c = Color.White;
                    break;
            }

            return c;
        }
        public static string BandToString(Band b)
        {
            string ret;
            switch (b)
            {
                case Band.GEN: ret = "GEN"; break;

                case Band.B160M: ret = "160M"; break;
                case Band.B80M: ret = "80M"; break;
                case Band.B60M: ret = "60M"; break;
                case Band.B40M: ret = "40M"; break;
                case Band.B30M: ret = "30M"; break;
                case Band.B20M: ret = "20M"; break;
                case Band.B17M: ret = "17M"; break;
                case Band.B15M: ret = "15M"; break;
                case Band.B12M: ret = "12M"; break;
                case Band.B10M: ret = "10M"; break;
                case Band.B6M: ret = "6M"; break;
                case Band.B2M: ret = "2M"; break;

                case Band.WWV: ret = "WWV"; break;

                case Band.BLMF: ret = "LMF"; break;

                case Band.B120M: ret = "120M"; break;
                case Band.B90M: ret = "90M"; break;
                case Band.B61M: ret = "61M"; break;
                case Band.B49M: ret = "49M"; break;
                case Band.B41M: ret = "41M"; break;
                case Band.B31M: ret = "31M"; break;
                case Band.B25M: ret = "25M"; break;
                case Band.B22M: ret = "22M"; break;
                case Band.B19M: ret = "19M"; break;
                case Band.B16M: ret = "16M"; break;
                case Band.B14M: ret = "14M"; break;
                case Band.B13M: ret = "13M"; break;
                case Band.B11M: ret = "11M"; break;

                case Band.VHF0: ret = "VHF0"; break;
                case Band.VHF1: ret = "VHF1"; break;

//                case Band.VHF0: ret = "VU 2m"; break;
//                case Band.VHF1: ret = "VU 70cm"; break;

                case Band.VHF2: ret = "VHF2"; break;
                case Band.VHF3: ret = "VHF3"; break;
                case Band.VHF4: ret = "VHF4"; break;
                case Band.VHF5: ret = "VHF5"; break;
                case Band.VHF6: ret = "VHF6"; break;
                case Band.VHF7: ret = "VHF7"; break;
                case Band.VHF8: ret = "VHF8"; break;
                case Band.VHF9: ret = "VHF9"; break;
                case Band.VHF10: ret = "VHF10"; break;
                case Band.VHF11: ret = "VHF11"; break;
                case Band.VHF12: ret = "VHF12"; break;
                case Band.VHF13: ret = "VHF13"; break;

                default: ret = "GEN"; break;
            }

            return ret.ToUpper();
        }

        public static Band StringToBand(string s)
        {
            // pull off the B if there is one
            if (s.StartsWith("B") || s.StartsWith("b")) s = s.Substring(1);
            //

            Band b;
            switch (s.ToUpper())
            {
                case "GEN": b = Band.GEN; break;

                case "160M": b = Band.B160M; break;
                case "80M": b = Band.B80M; break;
                case "60M": b = Band.B60M; break;
                case "40M": b = Band.B40M; break;
                case "30M": b = Band.B30M; break;
                case "20M": b = Band.B20M; break;
                case "17M": b = Band.B17M; break;
                case "15M": b = Band.B15M; break;
                case "12M": b = Band.B12M; break;
                case "10M": b = Band.B10M; break;
                case "6M": b = Band.B6M; break;
                case "2M": b = Band.B2M; break;

                case "WWV": b = Band.WWV; break;

                case "LMF": b = Band.BLMF; break;

                case "120M": b = Band.B120M; break;
                case "90M": b = Band.B90M; break;
                case "61M": b = Band.B61M; break;
                case "49M": b = Band.B49M; break;
                case "41M": b = Band.B41M; break;
                case "31M": b = Band.B31M; break;
                case "25M": b = Band.B25M; break;
                case "22M": b = Band.B22M; break;
                case "19M": b = Band.B19M; break;
                case "16M": b = Band.B16M; break;
                case "14M": b = Band.B14M; break;
                case "13M": b = Band.B13M; break;
                case "11M": b = Band.B11M; break;

                case "VHF0": b = Band.VHF0; break;
                case "VHF1": b = Band.VHF1; break;

                //case "VU 2M": b = Band.VHF0; break;  // remove these VU 2m/70cm MW0LGE_21h
                //case "VU 70CM": b = Band.VHF1; break;

                case "VHF2": b = Band.VHF2; break;
                case "VHF3": b = Band.VHF3; break;
                case "VHF4": b = Band.VHF4; break;
                case "VHF5": b = Band.VHF5; break;
                case "VHF6": b = Band.VHF6; break;
                case "VHF7": b = Band.VHF7; break;
                case "VHF8": b = Band.VHF8; break;
                case "VHF9": b = Band.VHF9; break;
                case "VHF10": b = Band.VHF10; break;
                case "VHF11": b = Band.VHF11; break;
                case "VHF12": b = Band.VHF12; break;
                case "VHF13": b = Band.VHF13; break;

                default: b = Band.GEN; break;
            }

            return b;
        }
        private static void addBSObjectToEntries(object[] o, bool bIgnore60m = false)
        {
            for (int i = 0; i < o.Length / 7; i++)
            {
                BandStackEntry bse = new BandStackEntry
                {
                    Band = StringToBand((string)o[i * 7 + 0]),
                    Description = (string)o[i * 7 + 0],
                    Mode = StringToMode((string)o[i * 7 + 1]),
                    Filter = StringToFilter((string)o[i * 7 + 2]),
                    Frequency = (double)o[i * 7 + 3],
                    CTUNEnabled = (bool)o[i * 7 + 4],
                    ZoomFactor = 1f,
                    ZoomSlider = ((int)o[i * 7 + 5]),
                    CentreFrequency = (double)o[i * 7 + 6],
                    Locked = false                                    
                };

                if (bIgnore60m && bse.Band == Band.B60M) continue;
                
                if (m_Region == FRSRegion.US && bse.Band == Band.B60M) bse.Locked = true; //pre-lock 60m band entries for the US
                if (bse.Band == Band.WWV) bse.Locked = true; // pre-lock the WWV's

                while (isGuidInList(bse.GUID))
                {
                    // total overkill, GUIDS can have 6.8*10^15 for each star in observable universe
                    bse.GUID = Guid.NewGuid().ToString();
                }

                m_lstEntries.Add(bse);
            }
        }
        private static bool isGuidInList(string sGUID)
        {
            IEnumerable<BandStackEntry> data = from item in m_lstEntries
                                                  where item.GUID == sGUID
                                                  select item;

            return data.Count<BandStackEntry>() > 0;
        }

        private static void AddRegion1BandStack()
        {
            object[] data = {
                                //"160M", "CWL", "F1", 1.805000, false, 150, 0.0,
                                "160M", "CWL", "F1", 1.810000, false, 150, 0.0,
                                "160M", "DIGU", "F1", 1.838000, false, 150, 0.0,
                                "160M", "LSB", "F6", 1.843000, false, 150, 0.0,
                                "160M", "LSB", "F6", 1.850000, false, 150, 0.0,
                                "80M", "CWL", "F1", 3.505000, false, 150, 0.0,
                                "80M", "CWL", "F1", 3.510000, false, 150, 0.0,
                                "80M", "DIGU", "F1", 3.590000, false, 150, 0.0,
                                "80M", "LSB", "F6", 3.750000, false, 150, 0.0,
                                //"80M", "LSB", "F6", 3.900000, false, 150, 0.0, // MW0LGE_21d should not be in region 1
                                //"60M", "USB", "F6", 5.250000, false, 150, 0.0,
                                //"60M", "USB", "F6", 5.325000, false, 150, 0.0,
                                //"60M", "USB", "F6", 5.400000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.354000, false, 150, 0.0,
                                "40M", "CWL", "F1", 7.005000, false, 150, 0.0,
                                "40M", "CWL", "F1", 7.010000, false, 150, 0.0,
                                "40M", "DIGU", "F1", 7.045000, false, 150, 0.0,
                                "40M", "LSB", "F6", 7.09000, false, 150, 0.0,
                                "40M", "LSB", "F6", 7.10000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.107000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.110000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.115000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.120000, false, 150, 0.0,
                                "30M", "DIGU", "F1", 10.140000, false, 150, 0.0,
                                "20M", "CWU", "F1", 14.005000, false, 150, 0.0,
                                "20M", "CWU", "F1", 14.010000, false, 150, 0.0,
                                "20M", "DIGU", "F1", 14.085000, false, 150, 0.0,
                                "20M", "USB", "F6", 14.155000, false, 150, 0.0,
                                "20M", "USB", "F6", 14.225000, false, 150, 0.0,
                                "17M", "CWU", "F1", 18.070000, false, 150, 0.0,
                                "17M", "CWU", "F1", 18.078000, false, 150, 0.0,
                                "17M", "DIGU", "F1", 18.100000, false, 150, 0.0,
                                "17M", "USB", "F6", 18.120000, false, 150, 0.0,
                                "17M", "USB", "F6", 18.140000, false, 150, 0.0,
                                "15M", "CWU", "F1", 21.005000, false, 150, 0.0,
                                "15M", "CWU", "F1", 21.010000, false, 150, 0.0,
                                "15M", "DIGU", "F1", 21.090000, false, 150, 0.0,
                                "15M", "USB", "F6", 21.210000, false, 150, 0.0,
                                "15M", "USB", "F6", 21.290000, false, 150, 0.0,
                                "12M", "CWU", "F1", 24.900000, false, 150, 0.0,
                                "12M", "CWU", "F1", 24.905000, false, 150, 0.0,
                                "12M", "DIGU", "F1", 24.920000, false, 150, 0.0,
                                "12M", "USB", "F6", 24.940000, false, 150, 0.0,
                                "12M", "USB", "F6", 24.950000, false, 150, 0.0,
                                "10M", "CWU", "F1", 28.005000, false, 150, 0.0,
                                "10M", "CWU", "F1", 28.010000, false, 150, 0.0,
                                "10M", "DIGU", "F1", 28.120000, false, 150, 0.0,
                                "10M", "USB", "F6", 28.400000, false, 150, 0.0,
                                "10M", "USB", "F6", 28.450000, false, 150, 0.0,
                                "6M", "CWU", "F1", 50.080000, false, 150, 0.0,
                                "6M", "CWU", "F1", 50.090000, false, 150, 0.0,
                                "6M", "USB", "F6", 50.150000, false, 150, 0.0,
                                "6M", "USB", "F6", 50.165000, false, 150, 0.0,
                                "6M", "DIGU", "F1", 50.250000, false, 150, 0.0,
                                "2M", "CWU", "F1", 144.040000, false, 150, 0.0,
                                "2M", "CWU", "F1", 144.050000, false, 150, 0.0,
                                "2M", "DIGU", "F1", 144.138000, false, 150, 0.0,
                                "2M", "USB", "F6", 144.300000, false, 150, 0.0,
                                "2M", "USB", "F6", 144.310000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 2.500000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 5.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 10.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 15.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 20.000000, false, 150, 0.0,
                                "WWV", "SAM", "F5", 25.000000, false, 150, 0.0,
								"GEN", "SAM", "F6", 13.845000, false, 150, 0.0,
                                "GEN", "SAM", "F7", 5.975000, false, 150, 0.0,
                                "GEN", "SAM", "F7", 9.550000, false, 150, 0.0,
                                "GEN", "SAM", "F7", 3.850000, false, 150, 0.0,
                                "GEN", "SAM", "F8", 0.590000, false, 150, 0.0,
            };

            addBSObjectToEntries(data);
        }
        private static void AddRegion2BandStack(bool bIgnore60m = false)
        {
            object[] data = {
                                "160M", "CWL", "F5", 1.810000, false, 150, 0.0,
                                "160M", "CWU", "F1", 1.835000, false, 150, 0.0,
                                "160M", "LSB", "F6", 1.840000, false, 150, 0.0,
                                "160M", "LSB", "F6", 1.845000, false, 150, 0.0,
                                "160M", "LSB", "F6", 1.845000, false, 150, 0.0,
                                "80M", "CWU", "F1", 3.501000, false, 150, 0.0,
                                "80M", "CWU", "F1", 3.520000, false, 150, 0.0,
                                "80M", "LSB", "F6", 3.650000, false, 150, 0.0,
                                "80M", "LSB", "F6", 3.715000, false, 150, 0.0,
                                "80M", "SAM", "F5", 3.875000, false, 150, 0.0,
                                //"60M", "USB", "F6", 5.330500, false, 150, 0.0,
                                //"60M", "USB", "F6", 5.346500, false, 150, 0.0,
                                "60M", "USB", "F6", 5.354000, false, 150, 0.0,
                                //"60M", "USB", "F6", 5.357000, false, 150, 0.0,
                                //"60M", "USB", "F6", 5.371500, false, 150, 0.0,
                                //"60M", "USB", "F6", 5.403500, false, 150, 0.0,
                                "40M", "CWU", "F1", 7.001000, false, 150, 0.0,
                                "40M", "CWU", "F3", 7.021000, false, 150, 0.0,
                                "40M", "LSB", "F6", 7.152000, false, 150, 0.0,
                                "40M", "LSB", "F6", 7.180000, false, 150, 0.0,
                                "40M", "LSB", "F6", 7.255000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.107000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.110000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.120000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.130000, false, 150, 0.0,
                                "30M", "CWU", "F5", 10.140000, false, 150, 0.0,
                                "20M", "CWU", "F1", 14.010000, false, 150, 0.0,
                                "20M", "CWU", "F1", 14.020000, false, 150, 0.0,
                                "20M", "USB", "F6", 14.155000, false, 150, 0.0,
                                "20M", "USB", "F6", 14.230000, false, 150, 0.0,
                                "20M", "USB", "F6", 14.334000, false, 150, 0.0,
                                "17M", "CWU", "F1", 18.070000, false, 150, 0.0,
                                "17M", "CWU", "F1", 18.090000, false, 150, 0.0,
                                "17M", "USB", "F6", 18.125000, false, 150, 0.0,
                                "17M", "USB", "F6", 18.135000, false, 150, 0.0,
                                "17M", "USB", "F6", 18.140000, false, 150, 0.0,
                                "15M", "CWU", "F1", 21.001000, false, 150, 0.0,
                                "15M", "CWU", "F1", 21.021000, false, 150, 0.0,
                                "15M", "USB", "F6", 21.205000, false, 150, 0.0,
                                "15M", "USB", "F6", 21.255000, false, 150, 0.0,
                                "15M", "USB", "F6", 21.300000, false, 150, 0.0,
                                "12M", "CWU", "F1", 24.895000, false, 150, 0.0,
                                "12M", "CWU", "F1", 24.898000, false, 150, 0.0,
                                "12M", "USB", "F6", 24.931000, false, 150, 0.0,
                                "12M", "USB", "F6", 24.940000, false, 150, 0.0,
                                "12M", "USB", "F6", 24.950000, false, 150, 0.0,
                                "10M", "CWU", "F1", 28.010000, false, 150, 0.0,
                                "10M", "CWU", "F1", 28.020000, false, 150, 0.0,
                                "10M", "USB", "F6", 28.305000, false, 150, 0.0,
                                "10M", "USB", "F6", 28.350000, false, 150, 0.0,
                                "10M", "USB", "F6", 28.450000, false, 150, 0.0,
                                "6M", "CWU", "F1", 50.010000, false, 150, 0.0,
                                "6M", "CWU", "F1", 50.015000, false, 150, 0.0,
                                "6M", "USB", "F6", 50.125000, false, 150, 0.0,
                                "6M", "USB", "F6", 50.130000, false, 150, 0.0,
                                "6M", "USB", "F6", 50.200000, false, 150, 0.0,
                                "2M", "CWU", "F1", 144.010000, false, 150, 0.0,
                                "2M", "CWU", "F1", 144.015000, false, 150, 0.0,
                                "2M", "USB", "F6", 144.200000, false, 150, 0.0,
                                "2M", "USB", "F6", 144.220000, false, 150, 0.0,
                                "2M", "USB", "F6", 144.210000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 2.500000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 5.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 10.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 15.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 20.000000, false, 150, 0.0,
                                "GEN", "SAM", "F5", 13.845000, false, 150, 0.0,
                                "GEN", "SAM", "F5", 9.550000, false, 150, 0.0,
                                "GEN", "SAM", "F5", 5.975000, false, 150, 0.0,
                                "GEN", "SAM", "F5", 3.250000, false, 150, 0.0,
                                "GEN", "SAM", "F4", 0.590000, false, 150, 0.0,
            };

            addBSObjectToEntries(data, bIgnore60m);
        }

        private static void AddRegion3BandStack()
        {
            object[] data = {
                                "160M", "CWL", "F1", 1.820000, false, 150, 0.0,
                                "160M", "DIGU", "F1", 1.832000, false, 150, 0.0,
                                "160M", "LSB", "F6", 1.843000, false, 150, 0.0,
                                "80M", "CWL", "F1", 3.510000, false, 150, 0.0,
                                "80M", "DIGU", "F1", 3.580000, false, 150, 0.0,
                                "80M", "LSB", "F6", 3.750000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.354000, false, 150, 0.0,
                                //"60M", "USB", "F6", 5.264000, false, 150, 0.0,
                                //"60M", "USB", "F6", 5.357000, false, 150, 0.0,
                                //"60M", "USB", "F6", 5.403500, false, 150, 0.0,
                                "40M", "CWL", "F1", 7.010000, false, 150, 0.0,
                                "40M", "DIGU", "F1", 7.035000, false, 150, 0.0,
                                "40M", "LSB", "F6", 7.12000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.110000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.120000, false, 150, 0.0,
                                "30M", "DIGU", "F1", 10.140000, false, 150, 0.0,
                                "20M", "CWU", "F1", 14.010000, false, 150, 0.0,
                                "20M", "DIGU", "F1", 14.085000, false, 150, 0.0,
                                "20M", "USB", "F6", 14.225000, false, 150, 0.0,
                                "17M", "CWU", "F1", 18.078000, false, 150, 0.0,
                                "17M", "DIGU", "F1", 18.100000, false, 150, 0.0,
                                "17M", "USB", "F6", 18.140000, false, 150, 0.0,
                                "15M", "CWU", "F1", 21.010000, false, 150, 0.0,
                                "15M", "DIGU", "F1", 21.090000, false, 150, 0.0,
                                "15M", "USB", "F6", 21.300000, false, 150, 0.0,
                                "12M", "CWU", "F1", 24.900000, false, 150, 0.0,
                                "12M", "DIGU", "F1", 24.920000, false, 150, 0.0,
                                "12M", "USB", "F6", 24.940000, false, 150, 0.0,
                                "10M", "CWU", "F1", 28.010000, false, 150, 0.0,
                                "10M", "DIGU", "F1", 28.120000, false, 150, 0.0,
                                "10M", "USB", "F6", 28.400000, false, 150, 0.0,
                                "6M", "CWU", "F1", 50.090000, false, 150, 0.0,
                                "6M", "USB", "F6", 50.150000, false, 150, 0.0,
                                "6M", "DIGU", "F1", 50.250000, false, 150, 0.0,
                                "2M", "CWU", "F1", 144.050000, false, 150, 0.0,
                                "2M", "DIGU", "F1", 144.138000, false, 150, 0.0,
                                "2M", "USB", "F6", 144.200000, false, 150, 0.0,
                                "WWV", "SAM", "F5", 2.500000, false, 150, 0.0,
                                "WWV", "SAM", "F5", 5.000000, false, 150, 0.0,
                                "WWV", "SAM", "F5", 10.000000, false, 150, 0.0,
                                "WWV", "SAM", "F5", 15.000000, false, 150, 0.0,
                                "WWV", "SAM", "F5", 20.000000, false, 150, 0.0,
                                "WWV", "SAM", "F5", 25.000000, false, 150, 0.0,
                                "WWV", "USB", "F6", 3.330000, false, 150, 0.0,
                                "WWV", "USB", "F6", 7.850000, false, 150, 0.0,
                                "WWV", "USB", "F6", 14.670000, false, 150, 0.0,
                                "GEN", "SAM", "F6", 13.845000, false, 150, 0.0,
                                "GEN", "SAM", "F7", 5.975000, false, 150, 0.0,
                                "GEN", "SAM", "F7", 9.550000, false, 150, 0.0,
                                "GEN", "SAM", "F7", 3.850000, false, 150, 0.0,
                                "GEN", "SAM", "F8", 0.590000, false, 150, 0.0,
            };

            addBSObjectToEntries(data);
        }
        private static void AddBandStackSWL()
        {
            object[] data = {

                                "LMF", "SAM", "F4", 0.560000, false, 150, 0.0,
                                "LMF", "SAM", "F4", 0.720000, false, 150, 0.0,
                                "LMF", "SAM", "F4", 0.780000, false, 150, 0.0,
                                "LMF", "SAM", "F4", 1.000000, false, 150, 0.0,
                                "LMF", "SAM", "F4", 1.700000, false, 150, 0.0,

                                "120M", "SAM", "F4", 2.400000, false, 150, 0.0,
                                "120M", "SAM", "F4", 2.410000, false, 150, 0.0,
                                "120M", "SAM", "F4", 2.420000, false, 150, 0.0,

                                "90M", "SAM", "F4", 3.300000, false, 150, 0.0,
                                "90M", "SAM", "F4", 3.310000, false, 150, 0.0,
                                "90M", "SAM", "F4", 3.320000, false, 150, 0.0,

                                "61M", "SAM", "F4", 4.700000, false, 150, 0.0,
                                "61M", "SAM", "F4", 4.800000, false, 150, 0.0,
                                "61M", "SAM", "F4", 4.820000, false, 150, 0.0,

                                "49M", "SAM", "F4", 5.600000, false, 150, 0.0,
                                "49M", "SAM", "F4", 5.700000, false, 150, 0.0,
                                "49M", "SAM", "F4", 5.800000, false, 150, 0.0,
                                "49M", "SAM", "F4", 5.900000, false, 150, 0.0,
                                "49M", "SAM", "F4", 6.000000, false, 150, 0.0,
                                "49M", "SAM", "F4", 6.200000, false, 150, 0.0,


                                "41M", "SAM", "F4", 7.310000, false, 150, 0.0,
                                "41M", "SAM", "F4", 7.400000, false, 150, 0.0,
                                "41M", "SAM", "F4", 7.500000, false, 150, 0.0,


                                "31M", "SAM", "F4", 9.100000, false, 150, 0.0,
                                "31M", "SAM", "F4", 9.200000, false, 150, 0.0,
                                "31M", "SAM", "F4", 9.300000, false, 150, 0.0,
                                "31M", "SAM", "F4", 9.400000, false, 150, 0.0,
                                "31M", "SAM", "F4", 9.500000, false, 150, 0.0,
                                "31M", "SAM", "F4", 9.600000, false, 150, 0.0,


                                "25M", "SAM", "F4", 11.700000, false, 150, 0.0,
                                "25M", "SAM", "F4", 11.800000, false, 150, 0.0,
                                "25M", "SAM", "F4", 11.900000, false, 150, 0.0,

                                "22M", "SAM", "F4", 13.600000, false, 150, 0.0,
                                "22M", "SAM", "F4", 13.700000, false, 150, 0.0,
                                "22M", "SAM", "F4", 13.800000, false, 150, 0.0,

                                "19M", "SAM", "F4", 15.200000, false, 150, 0.0,
                                "19M", "SAM", "F4", 15.300000, false, 150, 0.0,
                                "19M", "SAM", "F4", 15.400000, false, 150, 0.0,

                                "16M", "SAM", "F4", 17.500000, false, 150, 0.0,
                                "16M", "SAM", "F4", 17.600000, false, 150, 0.0,
                                "16M", "SAM", "F4", 17.700000, false, 150, 0.0,

                                "14M", "SAM", "F4", 18.900000, false, 150, 0.0,
                                "14M", "SAM", "F4", 19.000000, false, 150, 0.0,
                                "14M", "SAM", "F4", 19.100000, false, 150, 0.0,

                                "13M", "SAM", "F4", 21.500000, false, 150, 0.0,
                                "13M", "SAM", "F4", 21.600000, false, 150, 0.0,
                                "13M", "SAM", "F4", 21.700000, false, 150, 0.0,

                                "11M", "SAM", "F4", 25.700000, false, 150, 0.0,
                                "11M", "SAM", "F4", 26.000000, false, 150, 0.0,
                                "11M", "SAM", "F4", 26.500000, false, 150, 0.0,
                                "11M", "SAM", "F4", 27.000000, false, 150, 0.0,
                                "11M", "SAM", "F4", 27.500000, false, 150, 0.0,
                                "11M", "SAM", "F4", 27.800000, false, 150, 0.0,

            };

            addBSObjectToEntries(data);
        }

        private static void AddUK_PlusBandStack()
        {
            object[] data = {
                                "160M", "CWL", "F1", 1.810000, false, 150, 0.0,
                                "160M", "CWU", "F1", 1.820000, false, 150, 0.0,
                                "160M", "DIGU", "F1", 1.838000, false, 150, 0.0,
                                "160M", "USB", "F6", 1.840000, false, 150, 0.0,
                                "160M", "USB", "F6", 1.845000, false, 150, 0.0,
                                "80M", "CWL", "F1", 3.505000, false, 150, 0.0,
                                "80M", "CWU", "F1", 3.510000, false, 150, 0.0,
                                "80M", "DIGU", "F1", 3.590000, false, 150, 0.0,
                                "80M", "LSB", "F6", 3.750000, false, 150, 0.0,
                                "80M", "LSB", "F6", 3.770000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.258500, false, 150, 0.0,
                                "60M", "USB", "F6", 5.276000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.288500, false, 150, 0.0,
                                "60M", "USB", "F6", 5.298000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.313000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.333000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.354000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.362000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.378000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.395000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.403500, false, 150, 0.0,
                                "40M", "CWL", "F1", 7.005000, false, 150, 0.0,
                                "40M", "CWU", "F1", 7.010000, false, 150, 0.0,
                                "40M", "DIGU", "F1", 7.045000, false, 150, 0.0,
                                "40M", "LSB", "F6", 7.09000, false, 150, 0.0,
                                "40M", "LSB", "F6", 7.10000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.105000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.110000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.120000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.130000, false, 150, 0.0,
                                "30M", "DIGU", "F1", 10.140000, false, 150, 0.0,
                                "20M", "CWU", "F1", 14.005000, false, 150, 0.0,
                                "20M", "CWU", "F1", 14.010000, false, 150, 0.0,
                                "20M", "DIGU", "F1", 14.085000, false, 150, 0.0,
                                "20M", "USB", "F6", 14.145000, false, 150, 0.0,
                                "20M", "USB", "F6", 14.225000, false, 150, 0.0,
                                "17M", "CWU", "F1", 18.070000, false, 150, 0.0,
                                "17M", "CWU", "F1", 18.078000, false, 150, 0.0,
                                "17M", "DIGU", "F1", 18.100000, false, 150, 0.0,
                                "17M", "USB", "F6", 18.140000, false, 150, 0.0,
                                "17M", "USB", "F6", 18.150000, false, 150, 0.0,
                                "15M", "CWU", "F1", 21.005000, false, 150, 0.0,
                                "15M", "CWU", "F1", 21.010000, false, 150, 0.0,
                                "15M", "DIGU", "F1", 21.090000, false, 150, 0.0,
                                "15M", "USB", "F6", 21.250000, false, 150, 0.0,
                                "15M", "USB", "F6", 21.300000, false, 150, 0.0,
                                "12M", "CWU", "F1", 24.900000, false, 150, 0.0,
                                "12M", "CWU", "F1", 24.910000, false, 150, 0.0,
                                "12M", "DIGU", "F1", 24.920000, false, 150, 0.0,
                                "12M", "USB", "F6", 24.940000, false, 150, 0.0,
                                "12M", "USB", "F6", 24.950000, false, 150, 0.0,
                                "10M", "CWU", "F1", 28.005000, false, 150, 0.0,
                                "10M", "CWU", "F1", 28.010000, false, 150, 0.0,
                                "10M", "DIGU", "F1", 28.120000, false, 150, 0.0,
                                "10M", "USB", "F6", 28.400000, false, 150, 0.0,
                                "10M", "USB", "F6", 28.450000, false, 150, 0.0,
                                "6M", "CWU", "F1", 50.090000, false, 150, 0.0,
                                "6M", "CWU", "F1", 50.095000, false, 150, 0.0,
                                "6M", "USB", "F6", 50.150000, false, 150, 0.0,
                                "6M", "USB", "F6", 50.160000, false, 150, 0.0,
                                "6M", "DIGU", "F1", 50.250000, false, 150, 0.0,
                                "2M", "CWU", "F1", 144.030000, false, 150, 0.0,
                                "2M", "CWU", "F1", 144.050000, false, 150, 0.0,
                                "2M", "DIGU", "F1", 144.138000, false, 150, 0.0,
                                "2M", "USB", "F6", 144.300000, false, 150, 0.0,
                                "2M", "USB", "F6", 144.310000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 2.500000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 5.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 10.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 15.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 20.000000, false, 150, 0.0,
                                "WWV", "SAM", "F5", 25.000000, false, 150, 0.0,
								"GEN", "SAM", "F6", 13.845000, false, 150, 0.0,
                                "GEN", "SAM", "F7", 5.975000, false, 150, 0.0,
                                "GEN", "SAM", "F7", 9.550000, false, 150, 0.0,
                                "GEN", "SAM", "F7", 3.850000, false, 150, 0.0,
                                "GEN", "SAM", "F8", 0.590000, false, 150, 0.0,
            };

            addBSObjectToEntries(data);
        }

        private static void AddUS_PlusBandStack()
        {
            object[] data = {
                                "60M", "USB", "F6", 5.332000 - (0.002800 / 2f), false, 150, 0.0,
                                "60M", "USB", "F6", 5.348000 - (0.002800 / 2f), false, 150, 0.0,
                                "60M", "USB", "F6", 5.358500 - (0.002800 / 2f), false, 150, 0.0,
                                "60M", "USB", "F6", 5.373000 - (0.002800 / 2f), false, 150, 0.0,
                                "60M", "USB", "F6", 5.405000 - (0.002800 / 2f), false, 150, 0.0,
            };

            addBSObjectToEntries(data);
        }
        private static void AddSwedenBandStack()
        {
            object[] data = {
                            "160M", "CWL", "F1", 1.805000, false, 150, 0.0,
                            "160M", "CWL", "F1", 1.810000, false, 150, 0.0,
                            "160M", "DIGU", "F1", 1.838000, false, 150, 0.0,
                            "160M", "LSB", "F6", 1.843000, false, 150, 0.0,
                            "160M", "LSB", "F6", 1.850000, false, 150, 0.0,
                            "80M", "CWL", "F1", 3.505000, false, 150, 0.0,
                            "80M", "CWL", "F1", 3.510000, false, 150, 0.0,
                            "80M", "DIGU","F1", 3.590000, false, 150, 0.0,
                            "80M", "LSB", "F6", 3.750000, false, 150, 0.0,
                            "80M", "LSB", "F6", 3.900000, false, 150, 0.0,
                            "60M", "USB", "F6", 5.310000, false, 150, 0.0,
                            "60M", "USB", "F6", 5.320000, false, 150, 0.0,
                            "60M", "USB", "F6", 5.380000, false, 150, 0.0,
                            "60M", "USB", "F6", 5.390000, false, 150, 0.0,
                            "40M", "CWL", "F1", 7.005000, false, 150, 0.0,
                            "40M", "CWL", "F1", 7.010000, false, 150, 0.0,
                            "40M", "DIGU", "F1", 7.045000, false, 150, 0.0,
                            "40M", "LSB", "F6", 7.09000, false, 150, 0.0,
                            "40M", "LSB", "F6", 7.10000, false, 150, 0.0,
                            "30M", "CWU", "F1", 10.107000, false, 150, 0.0,
                            "30M", "CWU", "F1", 10.110000, false, 150, 0.0,
                            "30M", "CWU", "F1", 10.115000, false, 150, 0.0,
                            "30M", "CWU", "F1", 10.120000, false, 150, 0.0,
                            "30M", "DIGU", "F1",10.140000, false, 150, 0.0,
                            "20M", "CWU", "F1", 14.005000, false, 150, 0.0,
                            "20M", "CWU", "F1", 14.010000, false, 150, 0.0,
                            "20M", "DIGU", "F1", 14.085000, false, 150, 0.0,
                            "20M", "USB", "F6", 14.155000, false, 150, 0.0,
                            "20M", "USB", "F6", 14.225000, false, 150, 0.0,
                            "17M", "CWU", "F1", 18.070000, false, 150, 0.0,
                            "17M", "CWU", "F1", 18.078000, false, 150, 0.0,
                            "17M", "DIGU", "F1", 18.100000, false, 150, 0.0,
                            "17M", "USB", "F6", 18.120000, false, 150, 0.0,
                            "17M", "USB", "F6", 18.140000, false, 150, 0.0,
                            "15M", "CWU", "F1", 21.005000, false, 150, 0.0,
                            "15M", "CWU", "F1", 21.010000, false, 150, 0.0,
                            "15M", "DIGU", "F1", 21.090000, false, 150, 0.0,
                            "15M", "USB", "F6", 21.210000, false, 150, 0.0,
                            "15M", "USB", "F6", 21.290000, false, 150, 0.0,
                            "12M", "CWU", "F1", 24.900000, false, 150, 0.0,
                            "12M", "CWU", "F1", 24.905000, false, 150, 0.0,
                            "12M", "DIGU", "F1", 24.920000, false, 150, 0.0,
                            "12M", "USB", "F6", 24.940000, false, 150, 0.0,
                            "12M", "USB", "F6", 24.950000, false, 150, 0.0,
                            "10M", "CWU", "F1", 28.005000, false, 150, 0.0,
                            "10M", "CWU", "F1", 28.010000, false, 150, 0.0,
                            "10M", "DIGU", "F1", 28.120000, false, 150, 0.0,
                            "10M", "USB", "F6", 28.400000, false, 150, 0.0,
                            "10M", "USB", "F6", 28.450000, false, 150, 0.0,
                            "6M", "CWU", "F1", 50.080000, false, 150, 0.0,
                            "6M", "CWU","F1", 50.090000, false, 150, 0.0,
                            "6M", "USB", "F6", 50.150000, false, 150, 0.0,
                            "6M", "USB", "F6", 50.165000, false, 150, 0.0,
                            "6M", "DIGU", "F1", 50.250000, false, 150, 0.0,
                            "2M", "CWU", "F1", 144.040000, false, 150, 0.0,
                            "2M", "CWU", "F1", 144.050000, false, 150, 0.0,
                            "2M", "DIGU", "F1", 144.138000, false, 150, 0.0,
                            "2M", "USB", "F6", 144.300000, false, 150, 0.0,
                            "2M", "USB", "F6", 144.310000, false, 150, 0.0,
                            "WWV", "SAM", "F5", 2.500000, false, 150, 0.0,
                            "WWV", "SAM", "F5", 5.000000, false, 150, 0.0,
                            "WWV", "SAM", "F5", 10.000000, false, 150, 0.0,
                            "WWV", "SAM","F5", 15.000000, false, 150, 0.0,
                            "WWV", "SAM", "F5", 20.000000, false, 150, 0.0,
                            "WWV", "SAM", "F5", 25.000000, false, 150, 0.0,
                            "WWV", "USB", "F6", 3.330000, false, 150, 0.0,
                            "WWV", "USB", "F6", 7.850000, false, 150, 0.0,
                            "WWV", "USB", "F6", 14.670000, false, 150, 0.0,
                            "GEN", "SAM", "F6", 13.845000, false, 150, 0.0,
                            "GEN", "SAM", "F7", 5.975000, false, 150, 0.0,
                            "GEN", "SAM", "F7", 9.550000, false, 150, 0.0,
                            "GEN", "SAM", "F7", 3.850000, false, 150, 0.0,
                            "GEN", "SAM", "F8", 0.590000, false, 150, 0.0,
            };

            addBSObjectToEntries(data);
        }
        private static void AddRegionJapanBandStack()
        {
            object[] data = {
                                "160M", "CWL", "F5", 1.810000, false, 150, 0.0,
                                "160M", "CWU", "F1", 1.825000, false, 150, 0.0,
                                "160M", "CWU", "F1", 1.835000, false, 150, 0.0,
                                "160M", "USB", "F6", 1.840000, false, 150, 0.0,
                                "160M", "USB", "F6", 1.845000, false, 150, 0.0,
                                "80M", "CWL", "F1", 3.501000, false, 150, 0.0,
                                "80M", "CWU", "F1", 3.510000, false, 150, 0.0,
                                "80M", "LSB", "F6", 3.751000, false, 150, 0.0,
                                "80M", "LSB", "F6", 3.755000, false, 150, 0.0,
                                "80M", "LSB", "F6", 3.850000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.330500, false, 150, 0.0,
                                "60M", "USB", "F6", 5.346500, false, 150, 0.0,
                                "60M", "USB", "F6", 5.357000, false, 150, 0.0,
                                "60M", "USB", "F6", 5.371500, false, 150, 0.0,
                                "60M", "USB", "F6", 5.403500, false, 150, 0.0,
                                "40M", "CWL", "F1", 7.001000, false, 150, 0.0,
                                "40M", "CWU", "F1", 7.005000, false, 150, 0.0,
                                "40M", "LSB", "F6", 7.090000, false, 150, 0.0,
                                "40M", "LSB", "F6", 7.105000, false, 150, 0.0,
                                "40M", "LSB", "F6", 7.190000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.105000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.110000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.120000, false, 150, 0.0,
                                "30M", "CWU", "F1", 10.130000, false, 150, 0.0,
                                "30M", "CWU", "F5", 10.140000, false, 150, 0.0,
                                "20M", "CWU", "F1", 14.005000, false, 150, 0.0,
                                "20M", "CWU", "F1", 14.010000, false, 150, 0.0,
                                "20M", "USB", "F6", 14.136000, false, 150, 0.0,
                                "20M", "USB", "F6", 14.230000, false, 150, 0.0,
                                "20M", "USB", "F6", 14.336000, false, 150, 0.0,
                                "17M", "CWU", "F1", 18.090000, false, 150, 0.0,
                                "17M", "CWU", "F1", 18.095000, false, 150, 0.0,
                                "17M", "USB", "F6", 18.125000, false, 150, 0.0,
                                "17M", "USB", "F6", 18.140000, false, 150, 0.0,
                                "17M", "USB", "F6", 18.145000, false, 150, 0.0,
                                "15M", "CWU", "F1", 21.001000, false, 150, 0.0,
                                "15M", "CWU", "F1", 21.005000, false, 150, 0.0,
                                "15M", "USB", "F6", 21.255000, false, 150, 0.0,
                                "15M", "USB", "F6", 21.270000, false, 150, 0.0,
                                "15M", "USB", "F6", 21.300000, false, 150, 0.0,
                                "12M", "CWU", "F1", 24.895000, false, 150, 0.0,
                                "12M", "CWU", "F1", 24.897000, false, 150, 0.0,
                                "12M", "USB", "F6", 24.900000, false, 150, 0.0,
                                "12M", "USB", "F6", 24.935000, false, 150, 0.0,
                                "12M", "USB", "F6", 24.945000, false, 150, 0.0,
                                "10M", "CWU", "F1", 28.005000, false, 150, 0.0,
                                "10M", "CWU", "F1", 28.010000, false, 150, 0.0,
                                "10M", "USB", "F6", 28.300000, false, 150, 0.0,
                                "10M", "USB", "F6", 28.400000, false, 150, 0.0,
                                "10M", "USB", "F6", 28.450000, false, 150, 0.0,
                                "6M", "CWU", "F1", 50.010000, false, 150, 0.0,
                                "6M", "CWU", "F1", 50.015000, false, 150, 0.0,
                                "6M", "USB", "F6", 50.125000, false, 150, 0.0,
                                "6M", "USB", "F6", 50.18000, false, 150, 0.0,
                                "6M", "USB", "F6", 50.190000, false, 150, 0.0,
                                "2M", "CWU", "F1", 144.010000, false, 150, 0.0,
                                "2M", "CWU", "F1", 144.015000, false, 150, 0.0,
                                "2M", "USB", "F6", 144.200000, false, 150, 0.0,
                                "2M", "USB", "F6", 144.210000, false, 150, 0.0,
                                "2M", "USB", "F6", 144.215000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 2.500000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 5.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 10.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 15.000000, false, 150, 0.0,
                                "WWV", "SAM", "F7", 20.000000, false, 150, 0.0,
                                "GEN", "SAM", "F5", 13.845000, false, 150, 0.0,
                                "GEN", "SAM", "F5", 9.550000, false, 150, 0.0,
                                "GEN", "SAM", "F5", 5.975000, false, 150, 0.0,
                                "GEN", "SAM", "F5", 3.250000, false, 150, 0.0,
                                "GEN", "SAM", "F4", 0.590000, false, 150, 0.0,
            };

            addBSObjectToEntries(data);
        }
        #endregion
    }
}
