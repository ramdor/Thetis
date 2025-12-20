/*  clsCountryData.cs

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
using System.IO;
using System.Xml;

namespace Thetis
{
    internal static class CountryData
    {
        //pre-processed data from https://www.country-files.com/cty-dat-format/
        //as of 1st Nov 24        
        private static List<PrefixData> _prefixDataList;

        [Serializable]
        public class PrefixData
        {
            public string Country { get; set; }
            public List<string> Prefixes { get; set; } = new List<string>();
            public int ADIF { get; set; }
            public int CQZone { get; set; }
            public int ITUZone { get; set; }
            public string Continent { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public double GMTOffset { get; set; }
            public bool ExactCallsign { get; set; }
        }

        static CountryData()
        {
            try
            {
                string data = Properties.Resources.cty;
                _prefixDataList = Common.DeserializeFromBase64<List<PrefixData>>(data);
            }
            catch
            {
                _prefixDataList = null;
            }
        }

        public static PrefixData GetCallsignData(string callsign)
        {
            if (_prefixDataList == null) return null;

            PrefixData bestMatch = null;
            int longestPrefixLength = 0;

            foreach (PrefixData data in _prefixDataList)
            {
                foreach (string prefix in data.Prefixes.OrderByDescending(p => p.Length))
                {
                    if (callsign.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        if (prefix.Length > longestPrefixLength)
                        {
                            longestPrefixLength = prefix.Length;
                            bestMatch = data;
                        }
                        break;
                    }
                }
            }

            return bestMatch;
        }
        private static void build()
        {
            _prefixDataList = new List<PrefixData>();
            string plistPath = "C:\\Users\\Richie\\Downloads\\cty_plist-3436\\cty.plist"; //xml version
            LoadPrefixes(plistPath);

            string serializedData = Common.SerializeToBase64<List<PrefixData>>(_prefixDataList);

            string directoryPath = Path.GetDirectoryName(plistPath);
            string outputPath = Path.Combine(directoryPath, "cty.txt");
            File.WriteAllText(outputPath, serializedData);
        }
        private static void LoadPrefixes(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            XmlNode dictNode = xmlDoc.SelectSingleNode("//dict");
            if (dictNode == null) return;

            foreach (XmlNode node in dictNode.ChildNodes)
            {
                if (node.Name == "key")
                {
                    string prefix = node.InnerText;
                    XmlNode countryDict = node.NextSibling;

                    if (countryDict != null && countryDict.Name == "dict")
                    {
                        string countryName = null;
                        int adif = 0, cqZone = 0, ituZone = 0;
                        string continent = null;
                        double latitude = 0, longitude = 0, gmtOffset = 0;
                        bool exactCallsign = false;
                        List<string> prefixes = new List<string> { prefix };

                        foreach (XmlNode innerNode in countryDict.ChildNodes)
                        {
                            if (innerNode.Name == "key" && innerNode.InnerText == "Country")
                            {
                                countryName = innerNode.NextSibling?.InnerText;
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "Prefix")
                            {
                                string prefixValue = innerNode.NextSibling?.InnerText;
                                if (prefixValue != null)
                                {
                                    prefixes.Add(prefixValue);
                                }
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "ADIF")
                            {
                                adif = int.Parse(innerNode.NextSibling?.InnerText ?? "0");
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "CQZone")
                            {
                                cqZone = int.Parse(innerNode.NextSibling?.InnerText ?? "0");
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "ITUZone")
                            {
                                ituZone = int.Parse(innerNode.NextSibling?.InnerText ?? "0");
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "Continent")
                            {
                                continent = innerNode.NextSibling?.InnerText;
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "Latitude")
                            {
                                latitude = double.Parse(innerNode.NextSibling?.InnerText ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "Longitude")
                            {
                                longitude = double.Parse(innerNode.NextSibling?.InnerText ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "GMTOffset")
                            {
                                gmtOffset = double.Parse(innerNode.NextSibling?.InnerText ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "ExactCallsign")
                            {
                                exactCallsign = innerNode.NextSibling?.InnerText == "true";
                            }
                        }

                        if (countryName != null)
                        {
                            PrefixData existingData = _prefixDataList.FirstOrDefault(pd => pd.Country == countryName);
                            if (existingData == null)
                            {
                                PrefixData data = new PrefixData
                                {
                                    Country = countryName,
                                    Prefixes = prefixes,
                                    ADIF = adif,
                                    CQZone = cqZone,
                                    ITUZone = ituZone,
                                    Continent = continent,
                                    Latitude = latitude,
                                    Longitude = longitude,
                                    GMTOffset = gmtOffset,
                                    ExactCallsign = exactCallsign
                                };
                                _prefixDataList.Add(data);
                            }
                            else
                            {
                                existingData.Prefixes.AddRange(prefixes);
                            }
                        }
                    }
                }
            }

            foreach (PrefixData data in _prefixDataList)
            {
                data.Prefixes = data.Prefixes.Distinct().OrderBy(p => p).ToList();
            }
        }
    }
}
