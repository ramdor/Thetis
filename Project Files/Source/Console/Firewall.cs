/*  Firewall.cs

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

// -----------------------
// FIREWALL HELPER LICENSE
// -----------------------

//https://github.com/falahati/WindowsFirewallHelper

//The MIT License (MIT)

//Copyright(c) 2016 - 2019 Soroush

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using WindowsFirewallHelper;
using System.Windows.Forms;

namespace Thetis
{
    internal class Firewall
    {
        private const string _IN_TCP = "Thetis Allow IN TCP";
        private const string _OUT_TCP = "Thetis Allow OUT TCP";
        private const string _IN_UDP = "Thetis Allow IN UDP";
        private const string _OUT_UDP = "Thetis Allow OUT UDP";

        public static void Setup()
        {
            if (!FirewallManager.IsServiceRunning)
            {
                MessageBox.Show("Firewall service is not running.", "Firewall - No Service", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                return;
            }

            string sExe = System.Reflection.Assembly.GetEntryAssembly().Location;
            if (sExe == "")
            {
                MessageBox.Show("Could not obtain EXE path.", "Firewall - Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                return;
            }

            if (!Common.IsAdministrator())
            {
                //msgbox need to be admin !
                MessageBox.Show("To reset Thetis firewall entries please run Thetis 'As Administrator'.", "Firewall - No Administrator Rights", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                return;
            }

            // remove existing rules for this exe name
            try
            {
                IEnumerable<IFirewallRule> fwrs = FirewallManager.Instance.Rules.Where(a => a.ApplicationName != null && a.ApplicationName.Equals(sExe, StringComparison.InvariantCultureIgnoreCase));
                foreach (IFirewallRule fwr in fwrs)
                {
                    FirewallManager.Instance.Rules.Remove(fwr);
                }
            }
            catch 
            {
                MessageBox.Show("There was a problem removing existing firewall entries. Please configure manually.\nThetis.exe needs UDP/TCP in/out, all ports", "Firewall - Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                return;
            }

            // add the rules
            bool b1 = addApplicationRule(FirewallDirection.Outbound, FirewallProtocol.TCP, sExe, _OUT_TCP);
            bool b2 = addApplicationRule(FirewallDirection.Outbound, FirewallProtocol.UDP, sExe, _OUT_UDP);

            bool b3 = addApplicationRule(FirewallDirection.Inbound, FirewallProtocol.TCP, sExe, _IN_TCP);
            bool b4 = addApplicationRule(FirewallDirection.Inbound, FirewallProtocol.UDP, sExe, _IN_UDP);

            if (b1 && b2 && b3 && b4)
            {
                MessageBox.Show("Firewall is configured correctly.", "Firewall - Success", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }
            else
            {
                MessageBox.Show("There was a problem configuring the firewall. Please configure manually.\nThetis.exe needs UDP/TCP in/out, all ports", "Firewall - Problem", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
            }                    
        }

        private static IFirewallRule findRule(string sName)
        {
            IFirewallRule ifr = null;

            ICollection<IFirewallRule> fwr = FirewallManager.Instance.Rules;

            if (fwr != null)
            {
                try
                {
                    ifr = fwr.First(a => a.Name != null && a.Name == sName);
                }
                catch { }
            }

            return ifr;
        }
        private static bool addApplicationRule(FirewallDirection direction, FirewallProtocol protocol, string sPath, string ruleName)
        {
            bool bOk = false;            

            try
            {
                IFirewallRule ifr = findRule(ruleName);

                if (ifr != null) FirewallManager.Instance.Rules.Remove(ifr);

                ifr = FirewallManager.Instance.CreateApplicationRule(FirewallProfiles.Public | FirewallProfiles.Domain | FirewallProfiles.Private, ruleName, FirewallAction.Allow, sPath);

                if (ifr != null)
                {
                    ifr.Direction = direction;
                    ifr.Protocol = protocol;
                    ifr.Scope = FirewallScope.All;

                    FirewallManager.Instance.Rules.Add(ifr);

                    bOk = true;
                }
            }
            catch
            {
            }
            return bOk;
        }
    }
}
