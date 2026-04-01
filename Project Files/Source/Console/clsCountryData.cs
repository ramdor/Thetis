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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Thetis
{
    internal static class CountryData
    {
        //pre-processed data from https://www.country-files.com/cty-dat-format/
        //CTY-3612 – 17 March 2026        
        private static List<PrefixData> _prefixDataList;
        private static readonly Dictionary<string, string> _countryCodeAliasMap = createCountryCodeAliasMap();
        private static readonly Dictionary<string, string> _regionCountryCodeMap = createRegionCountryCodeMap();
        private static readonly Dictionary<int, string> _adifCountryCodeMap = createAdifCountryCodeMap();
        private static readonly Dictionary<string, string> _assetCodeAliasMap = createAssetCodeAliasMap();
        private static readonly Dictionary<int, string> _adifAssetCodeMap = createAdifAssetCodeMap();

        [Serializable]
        public class PrefixData
        {
            public string Country { get; set; }
            public string CountryCode { get; set; }
            public string AssetCode { get; set; }
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

                if (_prefixDataList != null)
                {
                    foreach (PrefixData prefix_data in _prefixDataList)
                    {
                        prefix_data.CountryCode = getCountryCode(prefix_data.Country, prefix_data.ADIF);
                        prefix_data.AssetCode = getAssetCode(prefix_data.Country, prefix_data.ADIF, prefix_data.CountryCode);
                    }
                }
            }
            catch
            {
                _prefixDataList = null;
            }
        }

        public static PrefixData GetCallsignData(string callsign)
        {
            if (_prefixDataList == null) return null;
            if (string.IsNullOrWhiteSpace(callsign)) return null;

            callsign = callsign.Trim();

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

        //public static void build()
        //{
        //    _prefixDataList = new List<PrefixData>();
        //    string plistPath = "C:\\Users\\Richie\\Downloads\\cty_plist-3612\\cty.plist"; //xml version
        //    LoadPrefixes(plistPath);

        //    string serializedData = Common.SerializeToBase64<List<PrefixData>>(_prefixDataList);

        //    string directoryPath = Path.GetDirectoryName(plistPath);
        //    string outputPath = Path.Combine(directoryPath, "cty.txt");
        //    File.WriteAllText(outputPath, serializedData);
        //}

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
                        int adif = 0;
                        int cqZone = 0;
                        int ituZone = 0;
                        string continent = null;
                        double latitude = 0;
                        double longitude = 0;
                        double gmtOffset = 0;
                        bool exactCallsign = false;
                        List<string> prefixes = new List<string> { prefix };

                        foreach (XmlNode innerNode in countryDict.ChildNodes)
                        {
                            if (innerNode.Name == "key" && innerNode.InnerText == "Country")
                            {
                                countryName = innerNode.NextSibling == null ? null : innerNode.NextSibling.InnerText;
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "Prefix")
                            {
                                string prefixValue = innerNode.NextSibling == null ? null : innerNode.NextSibling.InnerText;
                                if (prefixValue != null)
                                {
                                    prefixes.Add(prefixValue);
                                }
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "ADIF")
                            {
                                adif = int.Parse(innerNode.NextSibling == null ? "0" : innerNode.NextSibling.InnerText);
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "CQZone")
                            {
                                cqZone = int.Parse(innerNode.NextSibling == null ? "0" : innerNode.NextSibling.InnerText);
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "ITUZone")
                            {
                                ituZone = int.Parse(innerNode.NextSibling == null ? "0" : innerNode.NextSibling.InnerText);
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "Continent")
                            {
                                continent = innerNode.NextSibling == null ? null : innerNode.NextSibling.InnerText;
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "Latitude")
                            {
                                latitude = double.Parse(innerNode.NextSibling == null ? "0" : innerNode.NextSibling.InnerText, CultureInfo.InvariantCulture);
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "Longitude")
                            {
                                longitude = double.Parse(innerNode.NextSibling == null ? "0" : innerNode.NextSibling.InnerText, CultureInfo.InvariantCulture);
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "GMTOffset")
                            {
                                gmtOffset = double.Parse(innerNode.NextSibling == null ? "0" : innerNode.NextSibling.InnerText, CultureInfo.InvariantCulture);
                            }
                            else if (innerNode.Name == "key" && innerNode.InnerText == "ExactCallsign")
                            {
                                exactCallsign = innerNode.NextSibling != null && innerNode.NextSibling.InnerText == "true";
                            }
                        }

                        if (countryName != null)
                        {
                            PrefixData existingData = _prefixDataList.FirstOrDefault(pd => pd.Country == countryName);
                            if (existingData == null)
                            {
                                string countryCode = getCountryCode(countryName, adif);

                                PrefixData data = new PrefixData
                                {
                                    Country = countryName,
                                    CountryCode = countryCode,
                                    AssetCode = getAssetCode(countryName, adif, countryCode),
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

                                if (string.IsNullOrWhiteSpace(existingData.CountryCode))
                                {
                                    existingData.CountryCode = getCountryCode(countryName, adif);
                                }

                                if (string.IsNullOrWhiteSpace(existingData.AssetCode))
                                {
                                    existingData.AssetCode = getAssetCode(countryName, adif, existingData.CountryCode);
                                }
                            }
                        }
                    }
                }
            }

            foreach (PrefixData data in _prefixDataList)
            {
                data.Prefixes = data.Prefixes.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            }
        }

        private static string getCountryCode(string country, int adif)
        {
            string normalized = normalizeCountryName(country);
            string code;

            if (_countryCodeAliasMap.TryGetValue(normalized, out code)) return code;
            if (_regionCountryCodeMap.TryGetValue(normalized, out code)) return code;
            if (_adifCountryCodeMap.TryGetValue(adif, out code)) return code;

            return string.Empty;
        }

        private static string getAssetCode(string country, int adif, string countryCode)
        {
            string normalized = normalizeCountryName(country);
            string code;

            if (_assetCodeAliasMap.TryGetValue(normalized, out code)) return code;
            if (_adifAssetCodeMap.TryGetValue(adif, out code)) return code;

            return countryCode ?? string.Empty;
        }

        private static Dictionary<int, string> createAdifCountryCodeMap()
        {
            Dictionary<int, string> map = new Dictionary<int, string>();

            addCountryCode(map, 6, "US");
            addCountryCode(map, 10, "TF");
            addCountryCode(map, 13, "AQ");
            addCountryCode(map, 15, "RU");
            addCountryCode(map, 16, "NZ");
            addCountryCode(map, 17, "VE");
            addCountryCode(map, 20, "UM");
            addCountryCode(map, 21, "ES");
            addCountryCode(map, 24, "BV");
            addCountryCode(map, 31, "KI");
            addCountryCode(map, 33, "IO");
            addCountryCode(map, 34, "NZ");
            addCountryCode(map, 36, "FR");
            addCountryCode(map, 37, "CR");
            addCountryCode(map, 41, "TF");
            addCountryCode(map, 46, "MY");
            addCountryCode(map, 47, "CL");
            addCountryCode(map, 48, "KI");
            addCountryCode(map, 54, "RU");
            addCountryCode(map, 67, "US");
            addCountryCode(map, 71, "EC");
            addCountryCode(map, 98, "VC");
            addCountryCode(map, 99, "TF");
            addCountryCode(map, 110, "US");
            addCountryCode(map, 111, "HM");
            addCountryCode(map, 117, "UN");
            addCountryCode(map, 123, "UM");
            addCountryCode(map, 124, "TF");
            addCountryCode(map, 126, "RU");
            addCountryCode(map, 131, "TF");
            addCountryCode(map, 133, "NZ");
            addCountryCode(map, 137, "KR");
            addCountryCode(map, 142, "IN");
            addCountryCode(map, 149, "PT");
            addCountryCode(map, 152, "MO");
            addCountryCode(map, 153, "AU");
            addCountryCode(map, 161, "CO");
            addCountryCode(map, 165, "MU");
            addCountryCode(map, 166, "MP");
            addCountryCode(map, 172, "PN");
            addCountryCode(map, 174, "UM");
            addCountryCode(map, 191, "CK");
            addCountryCode(map, 195, "GQ");
            addCountryCode(map, 199, "BV");
            addCountryCode(map, 201, "ZA");
            addCountryCode(map, 204, "MX");
            addCountryCode(map, 205, "SH");
            addCountryCode(map, 206, "UN");
            addCountryCode(map, 207, "MU");
            addCountryCode(map, 215, "CY");
            addCountryCode(map, 217, "CL");
            addCountryCode(map, 230, "DE");
            addCountryCode(map, 234, "CK");
            addCountryCode(map, 235, "GS");
            addCountryCode(map, 238, "AQ");
            addCountryCode(map, 240, "GS");
            addCountryCode(map, 241, "AQ");
            addCountryCode(map, 246, "MT");
            addCountryCode(map, 247, string.Empty);
            addCountryCode(map, 248, "IT");
            addCountryCode(map, 250, "SH");
            addCountryCode(map, 253, "BR");
            addCountryCode(map, 256, "PT");
            addCountryCode(map, 270, "TK");
            addCountryCode(map, 272, "PT");
            addCountryCode(map, 273, "BR");
            addCountryCode(map, 274, "SH");
            addCountryCode(map, 276, "TF");
            addCountryCode(map, 283, "GB");
            addCountryCode(map, 285, "VI");
            addCountryCode(map, 289, "UN");
            addCountryCode(map, 299, "MY");
            addCountryCode(map, 301, "KI");
            addCountryCode(map, 302, "EH");
            addCountryCode(map, 318, "CN");
            addCountryCode(map, 321, "HK");
            addCountryCode(map, 339, "JP");
            addCountryCode(map, 344, "KP");
            addCountryCode(map, 345, "BN");
            addCountryCode(map, 386, "TW");
            addCountryCode(map, 414, "CD");
            addCountryCode(map, 453, "RE");
            addCountryCode(map, 468, "SZ");
            addCountryCode(map, 489, "FJ");
            addCountryCode(map, 490, "KI");
            addCountryCode(map, 504, "SK");
            addCountryCode(map, 507, "SB");
            addCountryCode(map, 508, "PF");
            addCountryCode(map, 509, "PF");
            addCountryCode(map, 512, "NC");
            addCountryCode(map, 513, "PN");
            addCountryCode(map, 515, "AS");
            addCountryCode(map, 519, "BQ");
            addCountryCode(map, 520, "BQ");
            addCountryCode(map, 521, "SS");
            addCountryCode(map, 522, "XK");

            return map;
        }

        private static Dictionary<int, string> createAdifAssetCodeMap()
        {
            Dictionary<int, string> map = new Dictionary<int, string>();

            addCountryCode(map, 205, "SH-AC");
            addCountryCode(map, 250, "SH-HL");
            addCountryCode(map, 274, "SH-TA");

            return map;
        }

        private static Dictionary<string, string> createCountryCodeAliasMap()
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            addAlias(map, "Aland Islands", "AX");
            addAlias(map, "Agalega and Saint Brandon", "MU");
            addAlias(map, "Agaléga and Saint Brandon", "MU");
            addAlias(map, "Alaska", "US");
            addAlias(map, "Andaman and Nicobar Islands", "IN");
            addAlias(map, "Annobon", "GQ");
            addAlias(map, "Annobon Island", "GQ");
            addAlias(map, "Annobón", "GQ");
            addAlias(map, "Asiatic Russia", "RU");
            addAlias(map, "Asiatic Turkey", "TR");
            addAlias(map, "Aves Island", "VE");
            addAlias(map, "Azores", "PT");
            addAlias(map, "Balearic Islands", "ES");
            addAlias(map, "Bear Island", "SJ");
            addAlias(map, "Bouvet", "BV");
            addAlias(map, "Bouvet Island", "BV");
            addAlias(map, "Canary Islands", "ES");
            addAlias(map, "Ceuta and Melilla", "ES");
            addAlias(map, "Ceuta & Melilla", "ES");
            addAlias(map, "Corsica", "FR");
            addAlias(map, "Crete", "GR");
            addAlias(map, "Culebra and Vieques", "PR");
            addAlias(map, "Culebra & Vieques", "PR");
            addAlias(map, "Desecheo Island", "PR");
            addAlias(map, "Dodecanese", "GR");
            addAlias(map, "East Malaysia", "MY");
            addAlias(map, "Easter Island", "CL");
            addAlias(map, "England", "GB");
            addAlias(map, "European Russia", "RU");
            addAlias(map, "European Turkey", "TR");
            addAlias(map, "Fernando de Noronha", "BR");
            addAlias(map, "Franz Josef Land", "RU");
            addAlias(map, "Guantanamo Bay", "CU");
            addAlias(map, "Hawaii", "US");
            addAlias(map, "Howland and Baker Islands", "UM");
            addAlias(map, "Howland & Baker Islands", "UM");
            addAlias(map, "International Telecommunication Union Headquarters", "UN");
            addAlias(map, "Isla de Aves", "VE");
            addAlias(map, "Jan Mayen", "SJ");
            addAlias(map, "Juan Fernandez Islands", "CL");
            addAlias(map, "Juan Fernández Islands", "CL");
            addAlias(map, "Kaliningrad", "RU");
            addAlias(map, "Kure Island", "UM");
            addAlias(map, "Lakshadweep Islands", "IN");
            addAlias(map, "Lord Howe Island", "AU");
            addAlias(map, "Madeira", "PT");
            addAlias(map, "Malyj Vysotskij Island", "RU");
            addAlias(map, "Market Reef", "AX");
            addAlias(map, "Mellish Reef", "AU");
            addAlias(map, "Minami Torishima", "JP");
            addAlias(map, "Mona Island", "PR");
            addAlias(map, "Mount Athos", "GR");
            addAlias(map, "Navassa Island", "UM");
            addAlias(map, "New Zealand Subantarctic Islands", "NZ");
            addAlias(map, "North Cook Islands", "CK");
            addAlias(map, "Northern Ireland", "GB");
            addAlias(map, "Ogasawara", "JP");
            addAlias(map, "Ogasawara Islands", "JP");
            addAlias(map, "Okinawa", "JP");
            addAlias(map, "Palmyra and Jarvis Islands", "UM");
            addAlias(map, "Palmyra & Jarvis Islands", "UM");
            addAlias(map, "Peter I Island", "BV");
            addAlias(map, "Pratas Island", "TW");
            addAlias(map, "Rodrigues Island", "MU");
            addAlias(map, "Rodriguez Island", "MU");
            addAlias(map, "Rotuma Island", "FJ");
            addAlias(map, "Sable Island", "CA");
            addAlias(map, "San Andres and Providencia", "CO");
            addAlias(map, "San Andres & Providencia", "CO");
            addAlias(map, "Sardinia", "IT");
            addAlias(map, "Scarborough Reef", "CN");
            addAlias(map, "Scarborough Shoal", "CN");
            addAlias(map, "Scotland", "GB");
            addAlias(map, "Shetland Islands", "GB");
            addAlias(map, "Sicily", "IT");
            addAlias(map, "South Cook Islands", "CK");
            addAlias(map, "Spratly Islands", string.Empty);
            addAlias(map, "St. Paul Island", "CA");
            addAlias(map, "St Paul Island", "CA");
            addAlias(map, "Svalbard", "SJ");
            addAlias(map, "United Nations Headquarters", "UN");
            addAlias(map, "Wake Island", "UM");
            addAlias(map, "Wales", "GB");
            addAlias(map, "West Malaysia", "MY");
            addAlias(map, "Willis Island", "AU");

            return map;
        }

        private static Dictionary<string, string> createAssetCodeAliasMap()
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            addAlias(map, "Arab League", "ARAB");
            addAlias(map, "League of Arab States", "ARAB");
            addAlias(map, "Association of Southeast Asian Nations", "ASEAN");
            addAlias(map, "ASEAN", "ASEAN");
            addAlias(map, "Central European Free Trade Agreement", "CEFTA");
            addAlias(map, "CEFTA", "CEFTA");
            addAlias(map, "East African Community", "EAC");
            addAlias(map, "EAC", "EAC");
            addAlias(map, "England", "GB-ENG");
            addAlias(map, "Northern Ireland", "GB-NIR");
            addAlias(map, "Scotland", "GB-SCT");
            addAlias(map, "Wales", "GB-WLS");
            addAlias(map, "Catalonia", "ES-CT");
            addAlias(map, "Catalunya", "ES-CT");
            addAlias(map, "Galicia", "ES-GA");
            addAlias(map, "Basque Country", "ES-PV");
            addAlias(map, "Pais Vasco", "ES-PV");
            addAlias(map, "País Vasco", "ES-PV");
            addAlias(map, "Euskadi", "ES-PV");
            addAlias(map, "Ascension", "SH-AC");
            addAlias(map, "Ascension Island", "SH-AC");
            addAlias(map, "Saint Helena", "SH-HL");
            addAlias(map, "St Helena", "SH-HL");
            addAlias(map, "St. Helena", "SH-HL");
            addAlias(map, "Tristan da Cunha", "SH-TA");

            return map;
        }

        private static Dictionary<string, string> createRegionCountryCodeMap()
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

            foreach (CultureInfo culture in cultures)
            {
                try
                {
                    RegionInfo region = new RegionInfo(culture.Name);

                    addAlias(map, region.EnglishName, region.TwoLetterISORegionName);
                    addAlias(map, region.NativeName, region.TwoLetterISORegionName);
                }
                catch
                {
                }
            }

            addAlias(map, "The Gambia", "GM");
            addAlias(map, "Czech Republic", "CZ");
            addAlias(map, "Viet Nam", "VN");
            addAlias(map, "Bosnia-Herzegovina", "BA");
            addAlias(map, "Brunei", "BN");
            addAlias(map, "Cape Verde", "CV");
            addAlias(map, "Curacao", "CW");
            addAlias(map, "Curaçao", "CW");
            addAlias(map, "Democratic Republic of the Congo", "CD");
            addAlias(map, "East Timor", "TL");
            addAlias(map, "Eswatini", "SZ");
            addAlias(map, "Iran", "IR");
            addAlias(map, "Laos", "LA");
            addAlias(map, "Micronesia", "FM");
            addAlias(map, "Moldova", "MD");
            addAlias(map, "North Macedonia", "MK");
            addAlias(map, "Palestine", "PS");
            addAlias(map, "Republic of the Congo", "CG");
            addAlias(map, "Russia", "RU");
            addAlias(map, "South Korea", "KR");
            addAlias(map, "North Korea", "KP");
            addAlias(map, "Syria", "SY");
            addAlias(map, "Taiwan", "TW");
            addAlias(map, "Tanzania", "TZ");
            addAlias(map, "United States", "US");
            addAlias(map, "Venezuela", "VE");
            addAlias(map, "Antarctica", "AQ");
            addAlias(map, "Bonaire", "BQ");
            addAlias(map, "Brunei Darussalam", "BN");
            addAlias(map, "Hong Kong", "HK");
            addAlias(map, "Macao", "MO");
            addAlias(map, "Republic of Kosovo", "XK");
            addAlias(map, "Republic of Korea", "KR");
            addAlias(map, "Republic of South Sudan", "SS");
            addAlias(map, "Reunion Island", "RE");
            addAlias(map, "Slovak Republic", "SK");
            addAlias(map, "Vienna Intl Ctr", "UN");
            addAlias(map, "Wallis & Futuna Islands", "WF");
            addAlias(map, "Western Sahara", "EH");

            return map;
        }

        private static void addCountryCode(Dictionary<int, string> map, int adif, string code)
        {
            if (!map.ContainsKey(adif))
            {
                map.Add(adif, code);
            }
            else
            {
                map[adif] = code;
            }
        }

        private static void addAlias(Dictionary<string, string> map, string name, string code)
        {
            string normalized = normalizeCountryName(name);
            if (normalized.Length == 0) return;

            if (!map.ContainsKey(normalized))
            {
                map.Add(normalized, code);
            }
            else
            {
                map[normalized] = code;
            }
        }

        private static string normalizeCountryName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            string formD = value.Trim().Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder(formD.Length + 8);

            for (int i = 0; i < formD.Length; i++)
            {
                char c = formD[i];
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

                if (category == UnicodeCategory.NonSpacingMark) continue;

                if (c == '&')
                {
                    sb.Append(" AND ");
                    continue;
                }

                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToUpperInvariant(c));
                }
                else
                {
                    sb.Append(' ');
                }
            }

            string normalized = sb.ToString();

            while (normalized.Contains("  "))
            {
                normalized = normalized.Replace("  ", " ");
            }

            normalized = " " + normalized.Trim() + " ";
            normalized = normalized.Replace(" N Z ", " NEW ZEALAND ");
            normalized = normalized.Replace(" ST ", " SAINT ");
            normalized = normalized.Replace(" IS ", " ISLANDS ");
            normalized = normalized.Replace(" INTL ", " INTERNATIONAL ");
            normalized = normalized.Replace(" CTR ", " CENTRE ");

            while (normalized.Contains("  "))
            {
                normalized = normalized.Replace("  ", " ");
            }

            return normalized.Trim();
        }
    }
}