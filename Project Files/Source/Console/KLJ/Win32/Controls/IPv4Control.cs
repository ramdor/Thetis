/******************************************************/
/*          NULLFX FREE SOFTWARE LICENSE              */
/******************************************************/
/*  IPAddressControl                                  */
/*  by: Steve Whitley                                 */
/*  © 2004 NullFX Software                            */
/*                                                    */
/* NULLFX SOFTWARE DISCLAIMS ALL WARRANTIES,          */
/* RESPONSIBILITIES, AND LIABILITIES ASSOCIATED WITH  */
/* USE OF THIS CODE IN ANY WAY, SHAPE, OR FORM        */
/* REGARDLESS HOW IMPLICIT, EXPLICIT, OR OBSCURE IT   */
/* IS. IF THERE IS ANYTHING QUESTIONABLE WITH REGARDS */
/* TO THIS SOFTWARE BREAKING AND YOU GAIN A LOSS OF   */
/* ANY NATURE, WE ARE NOT THE RESPONSIBLE PARTY. USE  */
/* OF THIS SOFTWARE CREATES ACCEPTANCE OF THESE TERMS */
/*                                                    */
/* USE OF THIS CODE MUST RETAIN ALL COPYRIGHT NOTICES */
/* AND LICENSES (MEANING THIS TEXT).                  */
/*                                                    */
/******************************************************/


namespace NullFX.Controls
{
    using System;
    using System.Net;
    using System.Windows.Forms;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Drawing;
    [StructLayout(LayoutKind.Sequential)]
    public struct Nmhdr
    {
        public IntPtr HWndFrom;
        public UIntPtr IdFrom;
        public int Code;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct NmIPAddress
    {
        public Nmhdr Hdr;
        public int Field;
        public int Value;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct InitCommonControlsEX
    {
        public int Size;
        public int Icc;
    }
    public enum IPField { OctetOne = 0, OctetTwo = 1, OctetThree = 2, OctetFour = 3 }
    public delegate void FieldChangedHandler(object sender, FieldChangedEventArgs e);
    public class FieldChangedEventArgs : EventArgs
    {
        private int _field, _value;
        public int Field
        {
            get { return _field; }
        }
        public int Value
        {
            get { return _value; }
        }
        public FieldChangedEventArgs(int field, int value)
            : base()
        {
            _field = field;
            _value = value;
        }
    }
    public class IPAddressControl : TextBox
    {
        private const int WM_NOTIFY = 0x004E,
            WM_USER = 0x0400,
            WM_REFLECT = WM_USER + 0x1C00,
            IPN_FIRST = -860,
            IPM_SETRANGE = (WM_USER + 103),
            IPM_GETADDRESS = (WM_USER + 102),
            IPM_SETADDRESS = (WM_USER + 101),
            IPM_CLEARADDRESS = (WM_USER + 100),
            IPM_ISBLANK = (WM_USER + 105),
            ICC_INTERNET_CLASSES = 0x00000800,
            CS_VREDRAW = 0x0001,
            CS_HREDRAW = 0x0002,
            CS_DBLCLKS = 0x0008,
            CS_GLOBALCLASS = 0x4000,
            WS_CHILD = 0x40000000,
            WS_VISIBLE = 0x10000000,
            WS_TABSTOP = 0x00010000,
            WS_EX_RIGHT = 0x00001000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_CLIENTEDGE = 0x00000200;
        private int[] values = new int[4];
        bool initialized = false;
        public event FieldChangedHandler FieldChanged;
        public IPAddressControl()
            : base()
        {
            for (int i = 0; i < 4; i++)
                values[i] = 0;
        }
        [DllImport("comctl32")]
        static extern bool InitCommonControlsEx(ref InitCommonControlsEX lpInitCtrls);

        protected virtual void OnFieldChanged(FieldChangedEventArgs e)
        {
            if (FieldChanged != null) FieldChanged(this, e);
        }
        protected override CreateParams CreateParams
        {
            get
            {
                if (!initialized)
                {
                    InitCommonControlsEX ic = new InitCommonControlsEX();
                    ic.Size = Marshal.SizeOf(typeof(InitCommonControlsEX));
                    ic.Icc = ICC_INTERNET_CLASSES;
                    initialized = InitCommonControlsEx(ref ic);
                }
                if (initialized)
                {
                    CreateParams cp = base.CreateParams;
                    cp.ClassName = "SysIPAddress32";
                    cp.Height = 23;
                    cp.ClassStyle = CS_VREDRAW | CS_HREDRAW | CS_DBLCLKS | CS_GLOBALCLASS;
                    cp.Style = WS_CHILD | WS_VISIBLE | WS_TABSTOP | 0x80;
                    cp.ExStyle = WS_EX_NOPARENTNOTIFY | WS_EX_CLIENTEDGE;
                    if (RightToLeft == RightToLeft.No
                                                    || (RightToLeft == RightToLeft.Inherit
                                                    && Parent.RightToLeft == RightToLeft.No))
                    {
                        cp.ExStyle |= WS_EX_LEFT | WS_EX_LTRREADING | WS_EX_RIGHTSCROLLBAR;
                    }
                    else
                    {
                        cp.ExStyle |= WS_EX_RIGHT | WS_EX_RTLREADING | WS_EX_LEFTSCROLLBAR;
                    }
                    return cp;
                }
                else
                {
                    return base.CreateParams;
                }
            }
        }
        public bool SetIPRange(IPField field, byte lowValue, byte highValue)
        {
            if (!initialized) return false;
            Message m = Message.Create(Handle, IPM_SETRANGE, (IntPtr)((int)field), MakeRange(lowValue, highValue));
            WndProc(ref m);
            return m.Result.ToInt32() > 0;
        }
        public System.Net.IPAddress IPAddress
        {
            get
            {
                if (!initialized) return IPAddress.None;
                return IPAddress.Parse(base.Text);
            }
        }
        public bool IsBlank
        {
            get
            {
                if (!initialized) return !(base.Text.Length > 0);
                Message m = Message.Create(Handle, IPM_ISBLANK, IntPtr.Zero, IntPtr.Zero);
                WndProc(ref m);
                return m.Result.ToInt32() > 0;
            }
        }
        new public void Clear()
        {
            if (!initialized)
            {
                base.Clear();
                return;
            }
            Message m = Message.Create(Handle, IPM_CLEARADDRESS, IntPtr.Zero, IntPtr.Zero);
            WndProc(ref m);
        }
        private System.Net.IPAddress GetIpAddress(IntPtr ip)
        {
            if (!initialized) return IPAddress.None;
            return new IPAddress(ip.ToInt64());
        }
        private IntPtr MakeRange(byte low, byte high)
        {
            return (IntPtr)((int)((high << 8) + low));
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (WM_REFLECT + WM_NOTIFY))
            {
                NmIPAddress ipInfo = (NmIPAddress)Marshal.PtrToStructure(m.LParam, typeof(NmIPAddress));
                if (ipInfo.Hdr.Code == IPN_FIRST)
                {
                    if (values[ipInfo.Field] != ipInfo.Value)
                    {
                        values[ipInfo.Field] = ipInfo.Value;
                        OnFieldChanged(new FieldChangedEventArgs(ipInfo.Field, ipInfo.Value));
                    }
                }
            }
            base.WndProc(ref m);
        }
    }
}
