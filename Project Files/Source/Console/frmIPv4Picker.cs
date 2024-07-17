//=================================================================
// frmIPv4Picker.cs - MW0LGE 2022
//=================================================================

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
