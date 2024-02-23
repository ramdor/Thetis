//=================================================================
// ucOCLedStrip.cs - MW0LGE 2021
//=================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thetis
{
    public partial class ucOCLedStrip : UserControl
    {
        public ucOCLedStrip()
        {
            InitializeComponent();
        }

        private bool m_bTX = false;
        public bool TX
        {
            get { return m_bTX; }
            set {
                m_bTX = value;
                this.Invalidate();
            }
        }
        private int m_nBits = 0;
        public int Bits
        {
            get { return m_nBits; }
            set
            {
                m_nBits = value;
                this.Invalidate();
            }
        }

        private int m_displayBits = 7;
        public int DisplayBits
        {
            get { return m_displayBits; }
            set
            {
                m_displayBits = value;
            }
        }

        private void usOCLedStrip_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            for(int nPin = 0; nPin < m_displayBits; nPin++)
            {
                int x = nPin * 16;

                Rectangle r = new Rectangle(x, 0, 15, this.Height-1);
                Brush b;

                bool bOn = (m_nBits & (1 << nPin)) != 0;

                if (bOn)
                {
                    if (m_bTX)
                        b = Brushes.OrangeRed;
                    else
                        b = Brushes.GreenYellow;
                }
                else
                {
                    b = Brushes.Gray;
                }

                g.FillRectangle(b, r);
                g.DrawRectangle(Pens.Black, r);
            }
        }
    }
}
