/*  frmSerialPortPicker.cs

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
using System.IO.Ports;
using System.Threading;

namespace Thetis
{
    public partial class frmSerialPortPicker : Form
    {
        private int[] _baudRates = new int[] { 110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };
        private int[] _dataBits = new int[] { 5, 6, 7, 8 };
        private StopBits[] _stopBits = new StopBits[] { StopBits.One, StopBits.OnePointFive, StopBits.Two, StopBits.None };
        private Parity[] _parity = new Parity[] { Parity.None, Parity.Odd, Parity.Even, Parity.Mark, Parity.Space };
        private string _com_port_setting;
        private int _baud_rate_setting;
        private int _data_bits_setting;
        private StopBits _stop_bits_setting;
        private Parity _parity_setting;

        public frmSerialPortPicker()
        {
            InitializeComponent();
        }
        public string ComPort
        {
            get { return _com_port_setting; }
            set { _com_port_setting = value; }
        }
        public int BaudRate
        {
            get { return _baud_rate_setting; }
            set { _baud_rate_setting = value; }
        }
        public int DataBits
        {
            get { return _data_bits_setting; }
            set { _data_bits_setting = value; }
        }
        public StopBits StopBits
        {
            get { return _stop_bits_setting; }
            set { _stop_bits_setting = value; }
        }
        public Parity Parity
        {
            get { return _parity_setting; }
            set { _parity_setting = value; }
        }
        public void Init()
        {
            btnSelect.Enabled = false;
            comboComPort.Items.Clear();
            comboBaudRate.Items.Clear();
            comboDataBits.Items.Clear();
            comboStopBits.Items.Clear();
            comboParity.Items.Clear();

            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                int i = comboComPort.Items.Add(port);
                if(port == _com_port_setting) comboComPort.SelectedIndex = i;
            }

            if(comboComPort.SelectedIndex == -1)
            {
                updateCombos(false);
            }
        }
        private void updateCombos(bool enabled)
        {
            comboBaudRate.Enabled = enabled;
            comboDataBits.Enabled = enabled;
            comboStopBits.Enabled = enabled;
            comboParity.Enabled = enabled;
        }
        private void btnSelect_Click(object sender, EventArgs e)
        {
            bool ok;
            ok = IsComPortAvailable(_com_port_setting);
            if (!ok) MessageBox.Show(_com_port_setting + " can not be opened, it is probably already in use.");

            if (ok)
            {
                ok = IsBaudRateSupported(_com_port_setting, _baud_rate_setting);
                if (!ok) MessageBox.Show(_com_port_setting + " can not be opened with that baud rate.");
            }

            if (ok)
            {
                ok = IsDataBitsSupported(_com_port_setting, _baud_rate_setting, _data_bits_setting);
                if (!ok) MessageBox.Show(_com_port_setting + " can not be opened with those data bits.");
            }

            if (ok)
            {
                ok = IsStopBitsSupported(_com_port_setting, _baud_rate_setting, _data_bits_setting, _stop_bits_setting);
                if (!ok) MessageBox.Show(_com_port_setting + " can not be opened with those stop bits.");
            }

            if (ok)
            {
                ok = IsParitySupported(_com_port_setting, _baud_rate_setting, _data_bits_setting, _stop_bits_setting, _parity_setting);
                if (!ok) MessageBox.Show(_com_port_setting + " can not be opened with that parity.");
            }

            if (ok)
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private bool IsBaudRateSupported(string portName, int baudRate)
        {
            try
            {
                using (SerialPort port = new SerialPort(portName, baudRate))
                {
                    port.ReadTimeout = 100;
                    port.WriteTimeout = 100;
                    port.Open();
                    Thread.Sleep(100);
                    port.Close();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsDataBitsSupported(string portName, int baudRate, int dataBits)
        {
            try
            {
                using (SerialPort port = new SerialPort(portName, baudRate))
                {
                    port.ReadTimeout = 100;
                    port.WriteTimeout = 100;
                    port.DataBits = dataBits;
                    port.Open();
                    Thread.Sleep(100);
                    port.Close();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsStopBitsSupported(string portName, int baudRate, int dataBits, StopBits stopBits)
        {
            try
            {
                using (SerialPort port = new SerialPort(portName, baudRate))
                {
                    port.ReadTimeout = 100;
                    port.WriteTimeout = 100;
                    port.DataBits = dataBits;
                    port.StopBits = stopBits;
                    port.Open();
                    Thread.Sleep(100);
                    port.Close();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsParitySupported(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity)
        {
            try
            {
                using (SerialPort port = new SerialPort(portName, baudRate))
                {
                    port.ReadTimeout = 100;
                    port.WriteTimeout = 100;
                    port.DataBits = dataBits;
                    port.StopBits = stopBits;
                    port.Parity = parity;
                    port.Open();
                    Thread.Sleep(100);
                    port.Close();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool IsComPortAvailable(string portName)
        {
            try
            {
                using (SerialPort port = new SerialPort(portName))
                {
                    port.ReadTimeout = 100;
                    port.WriteTimeout = 100;
                    port.Open();
                    Thread.Sleep(100);
                    port.Close();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void comboComPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboComPort.SelectedIndex == -1) return;
            btnSelect.Enabled = true;
            setupSubCombos(comboComPort.SelectedItem.ToString());
            _com_port_setting = comboComPort.SelectedItem.ToString();
        }
        private void setupSubCombos(string com_port)
        {
            comboBaudRate.Items.Clear();
            comboDataBits.Items.Clear();
            comboStopBits.Items.Clear();
            comboParity.Items.Clear();

            if (string.IsNullOrEmpty(com_port)) return;

            updateCombos(true);

            foreach (int baudRate in _baudRates)
            {
                int i = comboBaudRate.Items.Add(baudRate);
                if (baudRate == _baud_rate_setting) comboBaudRate.SelectedIndex = i;
            }

            foreach(int dataBits in _dataBits)
            {
                int i = comboDataBits.Items.Add(dataBits);
                if (dataBits == _data_bits_setting) comboDataBits.SelectedIndex = i;
            }

            foreach(StopBits stopBits in _stopBits)
            {
                string sb = "";
                switch (stopBits)
                {
                    case StopBits.One:
                        sb = "1";
                        break;
                    case StopBits.Two:
                        sb = "2";
                        break;
                    case StopBits.OnePointFive:
                        sb = "1.5";
                        break;
                    case StopBits.None:
                        sb = "None";
                        break;
                }
                int i = comboStopBits.Items.Add(sb);
                if (stopBits == _stop_bits_setting) comboStopBits.SelectedIndex = i;
            }

            foreach (Parity parity in _parity)
            {
                string p = "";
                switch (parity)
                {
                    case Parity.Odd:
                        p = "Odd";
                        break;
                    case Parity.Even:
                        p = "Even";
                        break;
                    case Parity.Mark:
                        p = "Mark";
                        break;
                    case Parity.Space:
                        p = "Space";
                        break;
                    case Parity.None:
                        p = "None";
                        break;
                }
                int i = comboParity.Items.Add(p);
                if (parity == _parity_setting) comboParity.SelectedIndex = i;
            }
        }

        private void comboBaudRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBaudRate.SelectedIndex == -1) return;

            bool ok = int.TryParse(comboBaudRate.SelectedItem.ToString(), out int value);
            _baud_rate_setting = value;
        }

        private void comboDataBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboDataBits.SelectedIndex == -1) return;

            bool ok = int.TryParse(comboDataBits.SelectedItem.ToString(), out int value);
            _data_bits_setting = value;
        }

        private void comboStopBits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboStopBits.SelectedIndex == -1) return;

            switch (comboStopBits.SelectedItem.ToString())
            {
                case "1":
                    _stop_bits_setting = StopBits.One;
                    break;
                case "2":
                    _stop_bits_setting = StopBits.Two;
                    break;
                case "1.5":
                    _stop_bits_setting = StopBits.OnePointFive;
                    break;
                case "None":
                    _stop_bits_setting = StopBits.None;
                    break;
            }
        }

        private void comboParity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboParity.SelectedIndex == -1) return;

            switch (comboParity.SelectedItem.ToString())
            {
                case "Odd":
                    _parity_setting = Parity.Odd;
                    break;
                case "Even":
                    _parity_setting = Parity.Even;
                    break;
                case "Mark":
                    _parity_setting = Parity.Mark;
                    break;
                case "Space":
                    _parity_setting = Parity.Space;
                    break;
                case "None":
                    _parity_setting = Parity.None;
                    break;
            }
        }
    }
}
