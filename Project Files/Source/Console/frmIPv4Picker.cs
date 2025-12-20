/*  frmIPv4Picker.cs

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
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace Thetis
{
    public partial class frmIPv4Picker : Form
    {
        private string _ip = "";
        private int _port = -1;
        private bool _bPortOk = false;
        private string _sOldPort = "";
        public frmIPv4Picker()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (comboAddresses.Text.Contains(":"))
            {
                string[] parts = comboAddresses.Text.Split(':');
                if (parts.Length == 2)
                {
                    //port is defined
                    int oldP = _port;
                    _bPortOk = int.TryParse(parts[1], out _port);
                    if (_bPortOk)
                    {
                        _ip = parts[0] + ":" + parts[1];
                        return;
                    }
                    _port = oldP;
                }
            }

            if (_port != -1 && _bPortOk)
            {
                _ip = comboAddresses.Text + ":" + _port.ToString();
            }
            else
            {
                if (_sOldPort != "")
                {
                    _ip = comboAddresses.Text + ":" + _sOldPort.ToString();
                }
                else
                {
                    _ip = comboAddresses.Text;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _ip = "";
        }
        public string IP
        {
            get { return _ip; }
        }
        public int Port
        {
            get { return _port; }
        }

        public void Init(string sIPPort, bool addBroadcast = false)
        {
            _port = -1;
            _ip = "";
            _bPortOk = false;
            _sOldPort = "";

            comboAddresses.Items.Clear();

            try
            {
                string[] parts = sIPPort.Split(':');
                string sTmp = "";
                string sPort = "";
                IPAddress address;
                bool bEntries = false;

                if (parts.Length == 1) sTmp = sIPPort;
                else if (parts.Length > 1)
                {
                    sTmp = parts[0];
                    sPort = parts[1];
                    _sOldPort = sPort;
                    bool _bPortOk = int.TryParse(sPort, out _port);
                }

                bool bOK = IPAddress.TryParse(sTmp, out address);
                if (bOK) sTmp = address.ToString();

                foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (/*item.NetworkInterfaceType == NetworkInterfaceType. && */item.OperationalStatus == OperationalStatus.Up)
                    {
                        foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                string sIp = ip.Address.ToString();

                                int n = comboAddresses.Items.Add(sIp);

                                if (sIp == sTmp) comboAddresses.SelectedIndex = n;
                                if (sIp == "255.255.255.255") addBroadcast = false;

                                bEntries = true;
                            }
                        }
                    }
                }

                if (addBroadcast) comboAddresses.Items.Add("255.255.255.255");

                btnSelect.Enabled = bEntries;
            }
            catch
            {
                btnSelect.Enabled = false;
            }
        }
    }
}
