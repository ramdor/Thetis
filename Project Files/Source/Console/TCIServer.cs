/*  TCIServer.cs

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


// info from
// https://www.codeproject.com/Articles/5733/A-TCP-IP-Server-written-in-C
// https://developer.mozilla.org/en-US/docs/Web/API/WebSockets_API/Writing_WebSocket_server
// https://stackoverflow.com/questions/10200910/creating-a-hello-world-websocket-example
// https://github.com/ExpertSDR3/TCI

// from my SunSDR2PRO as reference
// protocol: ExpertSDR3,1.8;
// device: SunSDR2PRO;
// receive_only: false;
// trx_count: 2;
// channels_count: 2;
// vfo_limits: 0.0,160000000.0;
// if_limits: -156250,156250;
// modulations_list: AM,LSB,USB,CW,NFM,WSPR,FT8,FT4,JT65,JT9,RTTY,BPSK,DIGL,DIGU,WFM,DRM;
// iq_samplerate: 48000;
// audio_samplerate: 48000;
// volume: -8;
// mute: false;
// mon_volume: -45;
// mon_enable: false;
// digl_offset: 0;
// digu_offset: 0;
// dds: 0,144559730;
// dds: 1,1950000;
// if:0,0,106320;
// if:0,1,-429530;
// if:1,0,0;
// if:1,1,0;
// vfo: 0,0,144666050;
// vfo: 0,1,144130200;
// vfo: 1,0,1950000;
// vfo: 1,1,1950000;
// modulation: 0,USB;
// modulation: 1,AM;
// agc_mode: 0,normal;
// agc_gain: 0,100;
// agc_mode: 1,normal;
// agc_gain: 1,80;
// rx_filter_band: 0,10,3960;
// rx_filter_band: 1,-3000,3000;
// rx_nb_enable: 0,false;
// rx_nb_param: 0,70,20;
// rx_nb_enable: 1,false;
// rx_nb_param: 1,70,20;
// rx_bin_enable: 0,false;
// rx_bin_enable: 1,false;
// rx_nr_enable: 0,false;
// rx_nr_enable: 1,false;
// rx_anc_enable: 0,false;
// rx_anc_enable: 1,false;
// rx_anf_enable: 0,false;
// rx_anf_enable: 1,false;
// rx_apf_enable: 0,false;
// rx_apf_enable: 1,false;
// rx_dse_enable: 0,false;
// rx_dse_enable: 1,false;
// rx_nf_enable: 0,false;
// rx_nf_enable: 1,false;
// rx_enable: 1,false;
// tx_enable: 0,true;
// tx_enable: 1,true;
// lock:0,false;
// lock:1,false;
// rx_channel_enable: 0,1,false;
// rit_enable: 0,false;
// xit_enable: 0,false;
// split_enable: 0,false;
// rit_offset: 0,0;
// xit_offset: 0,0;
// sql_enable: 0,false;
// sql_level: 0,-80;
// rx_mute: 0,false;
// rx_volume: 0,0,0;
// rx_balance: 0,0,0;
// rx_volume: 0,1,0;
// rx_balance: 0,1,0;
// rx_channel_enable: 1,1,false;
// rit_enable: 1,false;
// xit_enable: 1,false;
// split_enable: 1,false;
// rit_offset: 1,0;
// xit_offset: 1,0;
// sql_enable: 1,false;
// sql_level: 1,-80;
// rx_mute: 1,false;
// rx_volume: 1,0,0;
// rx_balance: 1,0,0;
// rx_volume: 1,1,0;
// rx_balance: 1,1,0;
// cw_macros_speed: 30;
// cw_macros_delay: 10;
// cw_keyer_speed: 30;
// drive: 0,0;
// tune_drive: 0,100;
// drive: 1,50;
// tune_drive: 1,50;
// trx: 0,false;
// tune: 0,false;
// iq_stop: 0;
// trx: 1,false;
// tune: 1,false;
// iq_stop: 1;
// app_focus: false;
// tx_frequency: 1900000;
// stop;
// ready;
//

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Thetis
{
    public enum TCICWSpotForce
    {
        CWU = 0,
        CWL,
        DEFAULT = 99
    }

    public class TCPIPtciSocketListener
	{
		//
		public delegate void ClientConnected();
		public delegate void ClientDisconnected();
		public delegate void ClientError(SocketException se);
		public ClientConnected ClientConnectedHandlers;
		public ClientDisconnected ClientDisconnectedHandlers;
		public ClientError ClientErrorHandlers;
        //

        public struct VFOData
        {
            public double freqMHz;
            public int offsetHz;
            public double centreMHz;
            public int chan;
            public int duplicate_tochan;
            public bool replace_if_duplicated;
            public bool cen;
            public int rx;
            public bool sendIF;

            //bespoke for tx info only
            public bool SendTXInfo;
            public Band TXfreqBand;
            public bool RX2EnabledForTX;
            public bool TXVFOB;
        }

        private class JsonSpotData
        {
            public string Spotter { get; set; } = "";
            public string Comment { get; set; } = "";
            public int Heading { get; set; } = -1; // -1 = no heading
            public string Continent { get; set; } = "";
            public string Country { get; set; } = "";
            public string UtcTime { get; set; } = "";

        }

        private Console _console;
		private TcpClient m_client = null;
		private NetworkStream m_stream = null;
		private bool m_stopClient = false;
		private bool m_disconnected = false;
		private Thread m_clientListenerThread = null;
		private bool m_markedForDeletion = false;
		private bool m_bWebSocket = false;
		private Thread m_VFODataThread = null;
		private TCPIPtciServer m_server = null;
		private int m_nRateLimit;
        private LinkedList<VFOData> m_vfoDataList = new LinkedList<VFOData>();
        private List<byte> _m_buffer = new List<byte>();
        private Stopwatch m_swVFO = new Stopwatch();
        private System.Threading.Timer m_tmVFOtimer;
        private Stopwatch m_swCentre = new Stopwatch();
        private System.Threading.Timer m_tmCentretimer;
        private Stopwatch m_swTXFrequency = new Stopwatch();
        private System.Threading.Timer m_tmTXFrequency;

        public TCPIPtciSocketListener(TcpClient client, Console c, TCPIPtciServer server, int rateLimit)
		{
			_console = c;
			m_nRateLimit = rateLimit;
			m_server = server;
			m_client = client;
			m_stream = client.GetStream();
		}
		~TCPIPtciSocketListener()
		{
			StopSocketListener();
		}
        private Console console
        {
            get
            {
                if (_console == null) return null;

                if (_console.InvokeRequired)
                {
                    return (Console)_console.Invoke(new Func<Console>(() => _console));
                }
                else
                    return _console;
            }
        }
        //
        public void ClickedOnSpot(string callsign, long frequency, int rx = -1, int chan = -1)
        {
			if(rx == -1 || chan == -1)
            {
				sendClickedOnSpot(callsign, frequency);
			}
            else
            {
				sendClickedOnSpotRX(rx - 1, chan, callsign, frequency);
            }
        }
		public void ThetisFocusChange(bool focus)
		{
			if (m_disconnected) return;
			sendAppFocus(focus);
		}
		public void RX2EnabledChange(bool enabled)
        {
			if (m_disconnected) return;
			sendRXEnable(1, enabled);
			sendTXEnable(1, enabled && !console.ThreadSafeTCIAccessor.MOX);
		}
		public void SampleRateChange(int rx, int oldSampleRate, int newSampleRate)
        {
			if (m_disconnected) return;
			if (rx == 1)
			{
				int halfSample = newSampleRate / 2;
				sendIFLimits(-halfSample, halfSample);
			}
		}
		public void DrivePowerChange(int rx, int newPower, bool tune)
        {
			if (m_disconnected) return;
            if (tune)
				sendTunePower(rx - 1, newPower);
			else
				sendDrivePower(rx - 1, newPower);
		}
		public void TuneChange(int rx, bool oldTune, bool newTune)
        {
			if (m_disconnected) return;
			sendTune(rx-1, newTune);
        }
		public void SplitChange(int rx, bool newSplit)
		{
			if (m_disconnected) return;
			bool bSplit = console.ThreadSafeTCIAccessor.VFOSplit;
			sendSplit(rx-1, bSplit);
		}
        public void MuteChanged(int rx, bool newState)
        {
            if (m_disconnected) return;
			sendMute(console.ThreadSafeTCIAccessor.MUT || (console.ThreadSafeTCIAccessor.RX2Enabled && console.ThreadSafeTCIAccessor.MUT2));
			sendMuteRX(rx - 1, newState);
        }
		public void MONChanged(bool newState)
		{
            if (m_disconnected) return;
			sendMONEnable(newState);
        }
		public void MONVolumeChanged(int newVolume)
		{
			if (m_disconnected) return;
            sendMONVolume(linearToDbVolume(newVolume));
		}
		public void TXFrequencyChanged(long new_frequency, Band new_band, bool rx2_enabled, bool tx_vfob)
		{
            if (m_disconnected) return;
            sendTXFrequencyChanged(new_frequency, new_band, rx2_enabled, tx_vfob);
        }
        private void limitList()
        {
			lock (m_objVFODataLock)
			{
				while (m_vfoDataList.Count > 10)
				{
					m_vfoDataList.RemoveFirst();
				}
			}
		}

		private object m_objVFODataLock = new Object();
		private async void VFOdata()
        {
			while (!m_stopClient)
            {
				int nCount;
				lock (m_objVFODataLock)
                {
					nCount = m_vfoDataList.Count;
                }

				if(nCount > 0)
                {
					LinkedListNode<VFOData> vfon = null;
					VFOData vfoData;

					lock (m_objVFODataLock)
                    {
						vfon = m_vfoDataList.First;
						vfoData = vfon.Value;
						vfon = null;
						m_vfoDataList.RemoveFirst();
					}

					if (vfoData.SendTXInfo)
					{
						// tx info only
						sendTXFrequencyChanged((long)(vfoData.freqMHz * 1e6), vfoData.TXfreqBand, vfoData.RX2EnabledForTX, vfoData.TXVFOB);
					}
					else
					{
						if (vfoData.cen)
						{
							sendDDS(vfoData.rx, (long)(vfoData.centreMHz * 1e6));
							if (vfoData.sendIF) sendIF(vfoData.rx, vfoData.chan, (int)vfoData.offsetHz);
						}
						else
						{
							if (vfoData.duplicate_tochan != -1)
							{
								if (!vfoData.replace_if_duplicated)
								{
									if (vfoData.sendIF) sendIF(vfoData.rx, vfoData.chan, (int)vfoData.offsetHz);
									sendVFO(vfoData.rx, vfoData.chan, (long)(vfoData.freqMHz * 1e6));
								}
								if (vfoData.sendIF) sendIF(vfoData.rx, vfoData.duplicate_tochan, (int)vfoData.offsetHz);
								sendVFO(vfoData.rx, vfoData.duplicate_tochan, (long)(vfoData.freqMHz * 1e6));
							}
							else
							{
								if (vfoData.sendIF) sendIF(vfoData.rx, vfoData.chan, (int)vfoData.offsetHz);
								sendVFO(vfoData.rx, vfoData.chan, (long)(vfoData.freqMHz * 1e6));
							}
						}
					}
                }

                lock (m_objVFODataLock)
                {
					nCount = m_vfoDataList.Count;
				}

				if (!m_stopClient)
				{ 
					if(nCount == 0) 
						await Task.Delay(1);
				}
			}
		}

		private void vfoFrequencyChange(VFOData vfod)
		{
			if (m_disconnected) return;
			lock (m_objVFODataLock)
			{
				//limitList();
				m_vfoDataList.AddLast(vfod);
			}
		}
		private void centreFrequencyChange(VFOData vfod)
		{
			if (m_disconnected) return;
			lock (m_objVFODataLock)
			{
				//limitList();
				m_vfoDataList.AddLast(vfod);
			}
		}
        private void txFrequencyChange(VFOData vfod)
        {
            if (m_disconnected) return;
            lock (m_objVFODataLock)
            {
                //limitList();
                m_vfoDataList.AddLast(vfod);
            }
        }
        public void MoxChange(int rx, bool oldMox, bool newMox)
        {
			if (m_disconnected) return;

            if (newMox)
            {
				if (rx == 1)
				{
					if (console.ThreadSafeTCIAccessor.RX2Enabled) sendTXEnable(1, false);
				}
				else
				{
					sendTXEnable(0, false);
				}
			}
			else
			{
				if (rx == 1)
				{
					if(console.ThreadSafeTCIAccessor.RX2Enabled) sendTXEnable(1, true);
				}
				else
				{
					sendTXEnable(0, true);
				}
			}
		
			sendMOX(rx - 1, newMox);
		}
		public void ModeChange(int rx, DSPMode oldMode, DSPMode newMode, Band oldBand, Band newBand)
        {
			if (m_disconnected) return;
			sendMode(rx-1, newMode);
        }
		public void BandChange(int rx, Band oldBand, Band newBand)
        {
			if (m_disconnected) return;

			// check band for tx? TODO
			sendTXEnable(rx-1, rx == 1 ? true : console.ThreadSafeTCIAccessor.RX2Enabled); // MW0LGE_22b fixed, rx1 should be tx only, not rx2
		}
		public void FilterChange(int rx, Filter oldFilter, Filter newFilter, Band band, int low, int high)
        {
			if (m_disconnected) return;
			sendFilterBand(rx-1, low, high);
		}
		public void FilterEdgesChange(int rx, Filter filter, Band band, int low, int high)
		{
			if (m_disconnected) return;
			sendFilterBand(rx-1, low, high);
        }
		public void PowerChange(bool oldPower, bool newPower)
        {
			if (m_disconnected) return;
			sendStartStop(newPower);
        }
		//

		public void StartSocketListener()
		{
			if (m_client != null)
			{
				m_VFODataThread = new Thread(new ThreadStart(VFOdata));
				m_VFODataThread.Priority = ThreadPriority.Normal;
				m_VFODataThread.Start();

				m_clientListenerThread =
					new Thread(new ThreadStart(SocketListenerThreadStart));
				m_clientListenerThread.Name = "TCI client listener Thread";

                m_clientListenerThread.Start();
			}
		}

		private enum EOpcodeType
		{			
			Fragment = 0, /* continuation code */			
			Text = 1, /*  text code */			
			Binary = 2, /*  binary code */			
			ClosedConnection = 8, /* closed connection */			
			Ping = 9, /* ping */			
			Pong = 10 /* pong */
		}

		private static byte[] GetFrameFromString(string Message, EOpcodeType Opcode = EOpcodeType.Text)
		{
			byte[] response;
			byte[] bytesRaw = Encoding.Default.GetBytes(Message);
			byte[] frame = new byte[10];

			long indexStartRawData = -1;
			long length = (long)bytesRaw.Length;

			frame[0] = (byte)(128 + (int)Opcode);
			if (length <= 125)
			{
				frame[1] = (byte)length;
				indexStartRawData = 2;
			}
			else if (length >= 126 && length <= 65535)
			{
				frame[1] = (byte)126;
				frame[2] = (byte)((length >> 8) & 255);
				frame[3] = (byte)(length & 255);
				indexStartRawData = 4;
			}
			else
			{
				frame[1] = (byte)127;
				frame[2] = (byte)((length >> 56) & 255);
				frame[3] = (byte)((length >> 48) & 255);
				frame[4] = (byte)((length >> 40) & 255);
				frame[5] = (byte)((length >> 32) & 255);
				frame[6] = (byte)((length >> 24) & 255);
				frame[7] = (byte)((length >> 16) & 255);
				frame[8] = (byte)((length >> 8) & 255);
				frame[9] = (byte)(length & 255);

				indexStartRawData = 10;
			}

			response = new byte[indexStartRawData + length];

			long i, reponseIdx = 0;

			//Add the frame bytes to the reponse
			for (i = 0; i < indexStartRawData; i++)
			{
				response[reponseIdx] = frame[i];
				reponseIdx++;
			}

			//Add the data bytes to the response
			for (i = 0; i < length; i++)
			{
				response[reponseIdx] = bytesRaw[i];
				reponseIdx++;
			}

			return response;
		}

		private bool upgradeToWebSocket(string msg)
        {
			bool bRet;

			try
			{
				// 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
				// 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
				// 3. Compute SHA-1 and Base64 hash of the new value
				// 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
				string swk = Regex.Match(msg, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
				string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
				byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
				string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

				// HTTP/1.1 defines the sequence CR LF as the end-of-line marker
				byte[] response = Encoding.UTF8.GetBytes(
					"HTTP/1.1 101 Switching Protocols\r\n" +
					"Connection: Upgrade\r\n" +
					"Upgrade: websocket\r\n" +
					"Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

				m_stream.Write(response, 0, response.Length);

				bRet = true;
			}
			catch
            {
				bRet = false;
            }

			return bRet;
		}

		//
		private void sendStart()
        {
			sendTextFrame("start;");
		}
		private void sendStop()
		{
			sendTextFrame("stop;");
		}
		private void sendSplit(int rx, bool bSplit)
        {
			string s = "split_enable:" + rx.ToString() + "," + bSplit.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendVFO(int rx, int chan, long vfo = -1)
        {
			bool bVFOaUseRX2;
			if (m_server != null && console != null)
				bVFOaUseRX2 = console.ThreadSafeTCIAccessor.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
			else
				bVFOaUseRX2 = false;

			if (vfo == -1)
			{
				if (rx == 0)
				{
					if (chan == 0)
						vfo = (long)(console.ThreadSafeTCIAccessor.VFOAFreq * 1e6);
					else if (chan == 1)
						vfo = (long)(console.ThreadSafeTCIAccessor.VFOBFreq * 1e6);
				}
				else if (rx == 1)
				{
					if(chan == 0)
                    {
						if(bVFOaUseRX2)
							vfo = (long)(console.ThreadSafeTCIAccessor.VFOAFreq * 1e6);
						else
							vfo = (long)(console.ThreadSafeTCIAccessor.VFOBFreq * 1e6);
					}
                    else if (chan == 1)
                    {
						vfo = (long)(console.ThreadSafeTCIAccessor.VFOBFreq * 1e6);
					}
				}					
			}
			string s = "vfo:" + rx.ToString() + "," + chan.ToString() + "," + vfo.ToString() + ";";
			sendTextFrame(s);
        }
		private void sendIF(int rx, int chan, int offset = -999999999)
		{
			if (offset == -999999999)
			{
				if (rx == 0)
				{
					if (chan == 0)
					{
						offset = (int)console.ThreadSafeTCIAccessor.radio.GetDSPRX(0, 0).RXOsc;
					}
					else if (chan == 1)
					{
						offset = (int)console.ThreadSafeTCIAccessor.radio.GetDSPRX(0, 1).RXOsc;
					}
					else offset = 0;
				}
				else if (rx == 1)
					offset = (int)console.ThreadSafeTCIAccessor.radio.GetDSPRX(1, 0).RXOsc;
			}

			offset += -console.ThreadSafeTCIAccessor.GetDSPcwPitchShiftToZero(rx + 1); //MW0LGE [2.9.0.7] note we invert with -

            string s = "if:" + rx.ToString() + "," + chan.ToString() + "," + offset.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendMOX(int rx, bool mox, bool signalTCI = false)
        {
			string sTXSignalFromTCI;
			if (signalTCI)
				sTXSignalFromTCI = ",tci";
			else
				sTXSignalFromTCI = "";

			string s = "trx:" + rx.ToString() + "," + mox.ToString().ToLower() + sTXSignalFromTCI + ";";
			sendTextFrame(s);
		}
		private void sendMode(int rx, DSPMode mode = DSPMode.FIRST)
        {
			if(mode == DSPMode.FIRST)
            {
                if (rx == 0)
					mode = console.ThreadSafeTCIAccessor.RX1DSPMode;
				else if(rx == 1)
					mode = console.ThreadSafeTCIAccessor.RX2DSPMode;
            }
			if (mode == DSPMode.FIRST || mode == DSPMode.LAST) return;

			string sMode;
			if (m_server != null && m_server.CWLUbecomesCW && (mode == DSPMode.CWL || mode == DSPMode.CWU))
			{
				sMode = "cw";
			}
			else
				sMode = mode.ToString().ToLower();

			string s = "modulation:" + rx.ToString() + "," + sMode.ToUpper() + ";"; // MW0LGE_22b mods are uppcase on the sun, replicate
			sendTextFrame(s);
		}
        private void sendMute(bool mute)
        {
            string s = "mute:" + mute.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
        private void sendMuteRX(int rx, bool mute)
        {
            string s = "rx_mute:" + rx.ToString() + "," + mute.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
		private void sendMONEnable(bool enable)
		{
            string s = "mon_enable:" + enable.ToString().ToLower() + ";";
            sendTextFrame(s);
        }
		private void sendMONVolume(double volume)
		{
            if (volume < -60f || volume > 0f) return;

            string s = "mon_volume:" + volume.ToString("F1").ToLower() + ";";
            sendTextFrame(s);
        }
		private void sendTXFrequencyChanged(long new_frequency, Band new_band, bool rx2_enabled, bool tx_vfob)
		{
            string s = $"tx_frequency:{new_frequency};";
            sendTextFrame(s.ToLower());

            // bespoke TCI command for anan to make life easier determining active TX frequency
            // format is : tx_frequency_thetis:3700000,b80m,false,false;
            // arg1 freq (long)
            // arg2 band b80m, b40m etc
            // arg3 rx2 enabled  true/false
            // arg4 tx on vfoB  true/false
            s = $"tx_frequency_thetis:{new_frequency},{new_band.ToString()},{rx2_enabled.ToString()},{tx_vfob.ToString()};";
            sendTextFrame(s.ToLower());
        }
        private void sendTunePower(int rx, int drive)
		{
			if (drive < 0 || drive > 100) return;

			string s = "tune_drive:" + rx.ToString() + "," + drive.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendDrivePower(int rx, int drive)
		{
			if (drive < 0 || drive > 100) return;

			string s = "drive:" + rx.ToString() + "," + drive.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendTune(int rx, bool tune)
		{
			string s = "tune:" + rx.ToString() + "," + tune.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendRXEnable(int rx, bool enable)
		{
			string s = "rx_enable:" + rx.ToString() + "," + enable.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendTXEnable(int rx, bool bEnable)
        {
			string s = "tx_enable:" + rx.ToString() + "," + bEnable.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendVFOLimits(int low, int high)
        {
			string s = "vfo_limits:" + low.ToString() + "," + high.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendAppFocus(bool focus)
        {
			string s = "app_focus:" + focus.ToString().ToLower() + ";";
			sendTextFrame(s);
		}
		private void sendIFLimits(int low, int high)
		{
			string s = "if_limits:" + low.ToString() + "," + high.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendClickedOnSpot(string callsign, long frequency)
		{
			string s = "clicked_on_spot:" + callsign.Trim() + "," + frequency.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendClickedOnSpotRX(int rx, int chan, string callsign, long frequency)
		{
			string s = "rx_clicked_on_spot:" + rx.ToString() + "," + chan.ToString() + "," + callsign.Trim() + "," + frequency.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendDDS(int rx, long ddsFreq = -1)
        {
			if (ddsFreq == -1)
			{
				if (rx == 0)
					ddsFreq = (long)(console.ThreadSafeTCIAccessor.CentreFrequency * 1e6);
				else if (rx == 1)
					ddsFreq = (long)(console.ThreadSafeTCIAccessor.CentreRX2Frequency * 1e6);
			}

			ddsFreq += console.ThreadSafeTCIAccessor.GetDSPcwPitchShiftToZero(rx+1); //MW0LGE [2.9.0.7]

            string s = "dds:" + rx.ToString() + "," + ddsFreq.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendFilterBand(int rx, int low, int high)
        {
			string s = "rx_filter_band:" + rx.ToString() + "," + low.ToString() + "," + high.ToString() + ";";
			sendTextFrame(s);
		}
		private void sendStartStop(bool bPower)
        {
			if (bPower)
				sendStart();
			else
				sendStop();
        }
		//

        private void sendInitialRadioState()
        {
			bool bSend = m_server != null ? m_server.SendInitialFrequencyStateOnConnect : true;
			bool bRX2Enabled = console.ThreadSafeTCIAccessor.RX2Enabled;

            if (bSend)
			{
				sendDDS(0);
				sendDDS(1);
				sendIF(0, 0);
				sendIF(0, 1);
				sendIF(1, 1);
				sendIF(1, 1);
				sendVFO(0, 0);
				sendVFO(0, 1);
				sendVFO(1, 0);
				sendVFO(1, 1);

                //bespoke
                sendTXFrequencyChanged((long)(console.ThreadSafeTCIAccessor.TXFreq * 1e6), console.ThreadSafeTCIAccessor.TXBand, console.ThreadSafeTCIAccessor.RX2Enabled, console.ThreadSafeTCIAccessor.VFOBTX);
            }

            sendMode(0);
			sendMode(1);

			//TODO sendFilterBand(0)

			sendRXEnable(0, !console.ThreadSafeTCIAccessor.MOX);
			sendRXEnable(1, bRX2Enabled && !console.ThreadSafeTCIAccessor.MOX);

			//lock
			//TODO rx channel enable
			//rit/xit

			sendSplit(0, console.ThreadSafeTCIAccessor.VFOSplit);
			sendSplit(1, bRX2Enabled && console.ThreadSafeTCIAccessor.VFOSplit);

			sendTXEnable(0, !console.ThreadSafeTCIAccessor.MOX);
			sendTXEnable(1, bRX2Enabled && !console.ThreadSafeTCIAccessor.MOX);

            sendMOX(0, console.ThreadSafeTCIAccessor.MOX && !(console.ThreadSafeTCIAccessor.VFOBTX && bRX2Enabled));
            sendMOX(1, console.ThreadSafeTCIAccessor.MOX && (console.ThreadSafeTCIAccessor.VFOBTX && bRX2Enabled));

            sendTune(0, console.ThreadSafeTCIAccessor.TUN && !(console.ThreadSafeTCIAccessor.VFOBTX && bRX2Enabled));
            sendTune(1, console.ThreadSafeTCIAccessor.TUN && (console.ThreadSafeTCIAccessor.VFOBTX && bRX2Enabled));

			//TODO iq sample rate
			//TODO iq stop
			//TODO audio_samplerate

			sendMute(console.ThreadSafeTCIAccessor.MUT || (console.ThreadSafeTCIAccessor.MUT2 && bRX2Enabled));
			sendMuteRX(0, console.ThreadSafeTCIAccessor.MUT);
            sendMuteRX(1, console.ThreadSafeTCIAccessor.MUT2);

			sendMONEnable(console.ThreadSafeTCIAccessor.MON);
            sendMONVolume(linearToDbVolume(console.ThreadSafeTCIAccessor.TXAF));

            sendStartStop(console.ThreadSafeTCIAccessor.PowerOn);// MW0LGE_22b moved here to replicate sun

			Debug.Print("SENT INITIAL STATE");
		}

		private void sendInitialisationData()
        {
			string sProtocol; //MW0LGE_22 emulate ee3 protocol
			if (m_server != null && m_server.EmulateExpertSDR3Protocol)
				sProtocol = "ExpertSDR3";
            else
				sProtocol = "Thetis";

			sendTextFrame("protocol:" + sProtocol + ",1.8;");

			string sDevice; //MW0LGE_22 emulate sunsdr
			if (m_server != null && m_server.EmulateSunSDR2Pro)
				sDevice = "SunSDR2PRO";
			else
				sDevice = HardwareSpecific.Model.ToString();
			sendTextFrame("device:" + sDevice + ";");
			sendTextFrame("receive_only:false;");
			sendTextFrame("trx_count:2;");
			sendTextFrame("channels_count:2;");

			sendVFOLimits(0, (int)(console.ThreadSafeTCIAccessor.MaxFreq * 1e6));

			int halfSample = console.ThreadSafeTCIAccessor.SampleRateRX1 / 2;
			sendIFLimits(-halfSample, halfSample); // only VFOA/rx1

			string sCW;
			if (m_server != null)
				sCW = m_server.CWLUbecomesCW ? "cwl, cwu, cw" : "cwl, cwu";
			else
				sCW = "cwl, cwu";

			sendTextFrame("modulations_list:" + ("am,sam,dsb,lsb,usb,nfm,fm,digl,digu," + sCW).ToUpper() + ";"); // MW0LGE_22b modulations are upper in sun, so replicate

			sendInitialRadioState();

			sendTextFrame("ready;");

			Debug.Print("SENT INIT DATA");
		}
		private int findEndOfHeader(byte[] bytes)
		{
			int nFind = 0;
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] == '\r' &&
					bytes[i + 1] == '\n' &&
					bytes[i + 2] == '\r' &&
					bytes[i + 3] == '\n')
				{
					nFind = i + 4;
					break;
				}
			}
			return nFind;
		}		

		private void SocketListenerThreadStart()
		{
			System.Threading.Timer t = new System.Threading.Timer(new TimerCallback(PingFrameTimer),
				null, 1000 * 20, 1000 * 20); // per websock spec ping frames are every 20 seconds.
											 // Ideally we should receive something
											// back within 20 seconds, but just use it to cause exception
											// on socket if client has dc'ed without telling us with a disconnect frame

			Debug.Print("TCPIP TCI Client Connected !");
			ClientConnectedHandlers?.Invoke();

			//SendClientData("# Thetis TCP/IP TCI #" + Environment.NewLine);

			while (!m_stopClient)
			{
				try
				{
					if (m_stream != null && m_stream.DataAvailable)
					{
						byte[] bytes = new byte[m_client.Available];
						int nRead = m_stream.Read(bytes, 0, bytes.Length);

						if (nRead > 0)
						{
							int nStart = 0;

							if (!m_bWebSocket)
							{
                                string msg = Encoding.UTF8.GetString(bytes);
                                if (Regex.IsMatch(msg, "^GET", RegexOptions.IgnoreCase))
								{

									if (upgradeToWebSocket(msg))
									{
										m_bWebSocket = true;
										Debug.Print("Upgraded to websocket");

										nStart = findEndOfHeader(bytes);

										// move rest of bytes if any to the buffer
										_m_buffer.Clear();
                                        //for (int i = nStart; i < nRead; i++)
										//	_m_buffer.Add(bytes[i]);
                                        _m_buffer.AddRange(bytes.Skip(nStart).Take(nRead - nStart));
                                        nRead = 0; // so that we dont re-add these below

										sendInitialisationData();
									}
									else
									{
										Debug.Print("Not Upgraded to websocket");
										m_stopClient = true;
									}
								}
							}

							if (m_bWebSocket)
							{
                                // add new bytes to buffer
                                //for (int i = 0; i < nRead; i++)
                                //    _m_buffer.Add(bytes[i]);
                                _m_buffer.AddRange(bytes.Take(nRead));

                                byte[] bytesAsArray = _m_buffer.ToArray();
								int frameLen = GetFrameLength(bytesAsArray);
								while (!m_stopClient && frameLen > -1 && bytesAsArray.Length >= frameLen)
								{
									// enough data to process a frame, dump bytes from the buffer
									_m_buffer.RemoveRange(0, frameLen);

									ParseReceiveBuffer(bytesAsArray);

									bytesAsArray = _m_buffer.ToArray();
									frameLen = GetFrameLength(bytesAsArray);
								}
							}
						}
                        else
                        {
							m_stopClient = true;
						}
					}
                    else
                    {
                        if (!m_client.Connected)
							m_stopClient = true;
						else
							Thread.Sleep(50);
                    }
				}
				catch (SocketException se)
				{
					m_stopClient = true;
					ClientErrorHandlers?.Invoke(se);
				}
                catch
                {
					m_stopClient = true;
				}
			}

            m_markedForDeletion = true;

            t.Change(Timeout.Infinite, Timeout.Infinite);
			t = null;

			Debug.Print("TCPIP TCI Client Disconnected !");
			m_disconnected = true;
			ClientDisconnectedHandlers?.Invoke();
		}
		private void sendPingFrame(string sMsg)
		{
			try
			{
				if (m_bWebSocket && m_client != null && m_stream != null)
				{
					if (m_client.Connected)
					{
						byte[] frame = GetFrameFromString(sMsg, EOpcodeType.Ping);
						m_stream.Write(frame, 0, frame.Length);						
					}
				}
			}
			catch
			{
				Debug.Print("problem writing ping frame");
				m_stopClient = true;
			}
		}
		private void sendPongFrame(string sMsg)
		{
			try
			{
				if (m_bWebSocket && m_client != null && m_stream != null)
				{
					if (m_client.Connected)
					{
						byte[] frame = GetFrameFromString(sMsg, EOpcodeType.Pong);
						m_stream.Write(frame, 0, frame.Length);
					}
				}
			}
			catch
			{
				Debug.Print("problem writing pong frame");
				m_stopClient = true;
			}
		}
		private void sendTextFrame(string sMsg)
		{
			try
			{
				if (m_bWebSocket && m_client != null && m_stream != null)
				{
					if (m_client.Connected)
					{
						byte[] frame = GetFrameFromString(sMsg, EOpcodeType.Text);
						m_stream.Write(frame, 0, frame.Length);
						if (m_server != null && m_server.LogForm != null) m_server.LogForm.Log(false, sMsg);
					}
				}
			}
			catch(Exception)
			{
				Debug.Print("problem writing text frame");
				m_stopClient = true;
			}
		}
		private void sendCloseFrame()
		{
			try
			{
				if (m_bWebSocket && m_client != null && m_stream != null)
				{
					if (m_client.Connected)
					{
						byte[] frame = GetFrameFromString("", EOpcodeType.ClosedConnection);
						m_stream.Write(frame, 0, frame.Length);
					}
				}
			}
            catch
            {

            }
		}
		public void StopSocketListener()
		{
			if (m_client != null)
			{
                if (m_tmVFOtimer != null)
                {
                    m_tmVFOtimer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_tmVFOtimer = null;
                }
                if (m_tmCentretimer != null)
                {
                    m_tmCentretimer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_tmCentretimer = null;
                }

                if (m_stream != null)
                {
					sendStop();
					
					sendCloseFrame();

					m_stream.Close();
					m_stream = null;
				}

				m_stopClient = true;
				m_client.Close();

				if (m_VFODataThread != null)
				{
					m_VFODataThread.Join(50);

					if (m_VFODataThread.IsAlive)
					{
						m_VFODataThread.Abort();
					}
					m_VFODataThread = null;
				}

				if (m_clientListenerThread != null)
				{
					m_clientListenerThread.Join(50);

					if (m_clientListenerThread.IsAlive)
					{
						m_clientListenerThread.Abort();
						m_disconnected = true;
						ClientDisconnectedHandlers?.Invoke();
					}

					m_clientListenerThread = null;
				}

				m_server = null;
				m_client = null;
				m_markedForDeletion = true;
			}
		}

		public bool IsMarkedForDeletion()
		{
			return m_markedForDeletion;
		}
		public bool IsDisconnected()
		{
			return m_disconnected;
		}
		private int GetFrameLength(Byte[] bytes)
		{
            //https://noio-ws.readthedocs.io/en/latest/overview_of_websockets.html

            if (bytes.Length < 2) return -1; // at least 2 bytes to get payload length

            bool mask = (bytes[1] & 0b10000000) != 0;	// frame is masked bit
            int payloadLen = bytes[1] & 0b01111111;

			if (payloadLen <= 125)
			{
				return mask ? payloadLen + 6 : payloadLen + 2;
            }
			else if (payloadLen == 126)
			{
                // need more bytes
				if(bytes.Length < 4) return -1;
                int extendedLen = (int)BitConverter.ToInt16(new byte[] { bytes[3], bytes[2] }, 0);
                return mask ? extendedLen + 8 : extendedLen + 4;
            }
            else if (payloadLen == 127)
            {
                // need more bytes
                if (bytes.Length < 10) return -1;
                int extendedLen = (int)BitConverter.ToInt64(new byte[] { bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2] }, 0);
                return mask ? extendedLen + 14 : extendedLen + 10;
            }

            return -1;
        }
		private void ParseReceiveBuffer(Byte[] bytes)
		{
			if (bytes.Length == 0) return;

            bool fin = (bytes[0] & 0b10000000) != 0;	// frame finished bit
			bool rsv1 = (bytes[0] & 0b01000000) != 0;
            bool rsv2 = (bytes[0] & 0b00100000) != 0;
            bool rsv3 = (bytes[0] & 0b00010000) != 0;
            bool mask = (bytes[1] & 0b10000000) != 0;	// frame is masked bit

			//if (!fin) return 0; // this is not the finish of the data

			EOpcodeType opcode = (EOpcodeType)(bytes[0] & 0b00001111);
			
			if (opcode == EOpcodeType.ClosedConnection)
			{
				// reply with close frame
				sendCloseFrame();

				m_stopClient = true;
				return;
			}
			else if (opcode == EOpcodeType.Text)
			{
                int dataLength = 0;
                int keyIndex = 0;

                int payloadLen = bytes[1] & 0b01111111; // strip off the 8th bit. 8th being the mask

                if (payloadLen <= 125)
                {
                    keyIndex = 2;
					dataLength = payloadLen;
                }
				else if (payloadLen == 126) // payload len
                {
                    keyIndex = 4;
                    dataLength = (int)BitConverter.ToInt16(new byte[] { bytes[3], bytes[2] }, 0);
                }
				else if (payloadLen == 127) // extended payload len
                {
                    keyIndex = 10;
                    dataLength = (int)BitConverter.ToInt64(new byte[] { bytes[9], bytes[8], bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2] }, 0);
                }

				int dataIndex = keyIndex;

				if (mask)
				{
					byte[] key = new byte[] { bytes[keyIndex], bytes[keyIndex + 1], bytes[keyIndex + 2], bytes[keyIndex + 3] };

					dataIndex += 4;
					int count = 0;
					for (int i = dataIndex; i < dataIndex + dataLength; i++)
					{
						bytes[i] = (byte)(bytes[i] ^ key[count % 4]);
						count++;
					}
				}

				parseTextFrame(Encoding.UTF8.GetString(bytes, dataIndex, dataLength));
			}
			else if(opcode == EOpcodeType.Ping)
            {
				sendPongFrame("Thetis");
            }
			else if(opcode == EOpcodeType.Binary)
            {
				// todo !
				Debug.Print("Binary Frame");
            }
		}
		private void handleSetInFocus()
        {
			console.ThreadSafeTCIAccessor.Focus();
        }
		private void handleStart()
        {
			if(!console.ThreadSafeTCIAccessor.PowerOn)
				console.ThreadSafeTCIAccessor.PowerOn = true;
        }
		private void handleStop()
		{
            if (console.ThreadSafeTCIAccessor.PowerOn)
				console.ThreadSafeTCIAccessor.PowerOn = false;
		}
		private void handleSpotClear()
		{
			SpotManager2.ClearAllSpots(true, false);
		}
		private void handleSplitEnableMessage(string[] args)
		{
			int rx = 0;
			bool bSplit = false;

			bool bOK = int.TryParse(args[0], out rx);
			if (args.Length == 2)
			{
				// set
				if (bOK)
					bOK = bool.TryParse(args[1], out bSplit);

				if (bOK)
				{
					if (!console.ThreadSafeTCIAccessor.IsSetupFormNull)
					{
                        if (console.ThreadSafeTCIAccessor.SetupForm.SplitFromCATorTCIcancelsQSPLIT)
                        {
                            if (console.ThreadSafeTCIAccessor.SetupForm.QuickSplitEnabled)
                                console.ThreadSafeTCIAccessor.SetupForm.QuickSplitEnabled = false;
                        }
                    }
					if (rx == 0 || rx == 1)
						if(console.ThreadSafeTCIAccessor.VFOSplit != bSplit)
							console.ThreadSafeTCIAccessor.VFOSplit = bSplit;
				}
			}
			else if(args.Length == 1)
			{
				// get
				if (bOK)
				{
					bool bSplitGet = console.ThreadSafeTCIAccessor.VFOSplit;
					sendSplit(rx, bSplitGet);
				}
			}
		}
		private void handleTrxMessage(string[] args)
		{
			int rx = 0;
			bool bMox = false;

			bool bOK = int.TryParse(args[0], out rx);
			if (bOK)
				bOK = bool.TryParse(args[1], out bMox);

			if (args.Length > 1)
			{ 
				if (args.Length > 2 && args[2].ToLower() == "tci")
				{
					// radio audio stream needs to be from TCI !!!!!!
					// TODO !!!!
				}

				if (bOK)
				{
					if (bMox && console.ThreadSafeTCIAccessor.MOX) return; // NOTE: ignore if already tx'ing

					if (rx == 0)
					{
						// if tx'ing on rx2 (which uses vfob) then swith to tx on vfoa
						if (console.ThreadSafeTCIAccessor.RX2Enabled && console.ThreadSafeTCIAccessor.VFOBTX)
							console.ThreadSafeTCIAccessor.VFOATX = true;

						if (console.ThreadSafeTCIAccessor.MOX != bMox)
							console.ThreadSafeTCIAccessor.TCIPTT = bMox;
					}
                    else if (rx == 1 && console.ThreadSafeTCIAccessor.RX2Enabled)
                    {
						if (!console.ThreadSafeTCIAccessor.VFOBTX)
							console.ThreadSafeTCIAccessor.VFOBTX = true;

						if (console.ThreadSafeTCIAccessor.MOX != bMox)
							console.ThreadSafeTCIAccessor.TCIPTT = bMox;
					}
				}
			}
			else if (bOK && args.Length == 1)
            {
				//query
				// single notion of tx in thetis
				sendMOX(rx, console.ThreadSafeTCIAccessor.MOX, false);
            }
		}

		private void handleIF(string[] args)
		{
			int rx = 0;
			int chan = 0;
			long lIF = 0;

			bool bOK = int.TryParse(args[0], out rx);
			if (bOK)
				bOK = int.TryParse(args[1], out chan);
			if (args.Length == 3)
			{
				if (bOK)
					bOK = long.TryParse(args[2], out lIF);

				if (bOK)
				{
					double dIF = lIF / 1e6;
					double vfo;

					if (rx == 0)
					{
						vfo = console.ThreadSafeTCIAccessor.CentreFrequency + dIF;
						vfo = Math.Round(vfo, 6);
						if (chan == 0)
						{
							if (console.ThreadSafeTCIAccessor.VFOAFreq != vfo)
								console.ThreadSafeTCIAccessor.VFOAFreq = vfo;
						}
						else if (chan == 1)
						{
							if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
								console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
						}
					}
					else if (rx == 1)
					{
						if (console.ThreadSafeTCIAccessor.RX2Enabled)
						{
							vfo = console.ThreadSafeTCIAccessor.CentreRX2Frequency + dIF;
							vfo = Math.Round(vfo, 6);
							if (chan == 0)
							{
								if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
									console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
							}
							else if (chan == 1)
							{
								if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
									console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
							}
						}
					}
				}
			}
			else if (args.Length == 2)
			{
				bool bVFOaUseRX2;
				if (m_server != null && console != null)
					bVFOaUseRX2 = console.ThreadSafeTCIAccessor.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
				else
					bVFOaUseRX2 = false;

				// query
				if (bOK)
				{
					double dIF = 0;
					if (rx == 0)
					{
						if (chan == 0)
						{
							dIF = console.ThreadSafeTCIAccessor.VFOAFreq - console.ThreadSafeTCIAccessor.CentreFrequency;
						}
						else if (chan == 1)
						{
							dIF = console.ThreadSafeTCIAccessor.VFOBFreq - console.ThreadSafeTCIAccessor.CentreFrequency;
						}
					}
					else if (rx == 1)
					{
						if (chan == 0)
						{
							if(bVFOaUseRX2)
								dIF = console.ThreadSafeTCIAccessor.VFOAFreq - console.ThreadSafeTCIAccessor.CentreFrequency;
							else
								dIF = console.ThreadSafeTCIAccessor.VFOBFreq - console.ThreadSafeTCIAccessor.CentreRX2Frequency;
						}
						else
						{
							dIF = console.ThreadSafeTCIAccessor.VFOBFreq - console.ThreadSafeTCIAccessor.CentreRX2Frequency;
						}
					}

					dIF *= 1e6; // into HZ
					sendIF(rx, chan, (int)dIF);
				}
			}
		}

		private void handleDDS(string[] args)
		{
			int rx = 0;
			long ddsLong = 0;

			bool bOK = int.TryParse(args[0], out rx);

			if (args.Length == 2)
			{
				if (bOK)
					bOK = long.TryParse(args[1], out ddsLong);
				if (bOK)
				{
					double dds = ddsLong / 1e6;

					if (rx == 0)
					{
						double c = dds - console.ThreadSafeTCIAccessor.CentreFrequency;
						c = Math.Round(c, 6);
						console.ThreadSafeTCIAccessor.CentreFrequency = dds;
						console.ThreadSafeTCIAccessor.VFOAFreq += c;
					}
					else if (rx == 1)
					{
						double c = dds - console.ThreadSafeTCIAccessor.CentreRX2Frequency;
						c = Math.Round(c, 6);
						console.ThreadSafeTCIAccessor.CentreRX2Frequency = dds;
						console.ThreadSafeTCIAccessor.VFOBFreq += c;
					}
				}
			}
			else if (bOK && args.Length == 1)
			{
				// query
				if (bOK)
				{
					double dds = 0;
					if (rx == 0)
					{
						dds = console.ThreadSafeTCIAccessor.CentreFrequency;
					}
					else if (rx == 1)
					{
						dds = console.ThreadSafeTCIAccessor.CentreRX2Frequency;
					}

					sendDDS(rx, (long)(dds * 1e6));
				}
			}
		}

		private void handleVFOMessage(string[] args)
		{
			int rx = 0;
			int chan = 0;
			long freq = 0;

			bool bVFOaUseRX2;
			if (m_server != null && console != null)
				bVFOaUseRX2 = console.ThreadSafeTCIAccessor.RX2Enabled && m_server.UseRX1VFOaForRX2VFOa;
			else
				bVFOaUseRX2 = false;

			bool bOK = int.TryParse(args[0], out rx);
			if (bOK)
				bOK = int.TryParse(args[1], out chan);
			if (args.Length == 3)
			{
				if (bOK)
					bOK = long.TryParse(args[2], out freq);

				if (bOK)
				{
					double vfo = freq / 1e6;
					vfo = Math.Round(vfo, 6);

					if (rx == 0)
					{
						if (chan == 0)
						{
							if (console.ThreadSafeTCIAccessor.VFOAFreq != vfo)
								console.ThreadSafeTCIAccessor.VFOAFreq = vfo;
						}
						else if (chan == 1)
						{
							if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
								console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
						}
					}
					else if (rx == 1)
					{
						if (console.ThreadSafeTCIAccessor.RX2Enabled)
						{
							if (chan == 0)
							{
								if (bVFOaUseRX2)
								{
									if (console.ThreadSafeTCIAccessor.VFOAFreq != vfo)
										console.ThreadSafeTCIAccessor.VFOAFreq = vfo;
								}
                                else
                                {
									if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
										console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
								}
							}
							else if (chan == 1)
							{
								if (console.ThreadSafeTCIAccessor.VFOBFreq != vfo)
									console.ThreadSafeTCIAccessor.VFOBFreq = vfo;
							}
						}
					}
				}
			}
			else if(args.Length == 2)
			{
				if (bOK)
				{
					double vfo = 0;
					if (rx == 0)
					{
						if (chan == 0)
						{
							vfo = console.ThreadSafeTCIAccessor.VFOAFreq;
						}
						else if (chan == 1)
						{
							vfo = console.ThreadSafeTCIAccessor.VFOBFreq;
						}
					}
					else if (rx == 1)
					{
						if (chan == 0)
						{
							if (bVFOaUseRX2)
								vfo = console.ThreadSafeTCIAccessor.VFOAFreq;
							else
								vfo = console.ThreadSafeTCIAccessor.VFOBFreq;
						}
						else
						{
							vfo = console.ThreadSafeTCIAccessor.VFOBFreq;
						}
					}

					TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData()
					{
						cen = false,
						centreMHz = -1,
						rx = bVFOaUseRX2 ? 1 : rx,
						freqMHz = vfo,
						offsetHz = -1,
						chan = chan,
						duplicate_tochan = -1,
						replace_if_duplicated = false,
						sendIF = false
					};

					VFOChange(vfod);
				}
			}
		}

		private void handleModulationMessage(string[] args)
        {
			bool bOK = int.TryParse(args[0], out int rx);

			if (args.Length == 2)
			{
				if (bOK)
				{
					DSPMode mode;

					switch (args[1].ToLower())
					{
						case "lsb":
							mode = DSPMode.LSB;
							break;
						case "usb":
							mode = DSPMode.USB;
							break;
						case "dsb":
							mode = DSPMode.DSB;
							break;
						case "am":
							mode = DSPMode.AM;
							break;
						case "sam":
							mode = DSPMode.SAM;
							break;
						case "nfm":
						case "fm":
							mode = DSPMode.FM;
							break;
						case "cw":
                            //change if needed [2.10.3.6]MW0LGE fixes #365
                            bool bChange = false;
                            if (m_server != null && console != null)
							{
								if(m_server.CWbecomesCWUabove10mhz)
								{
									bool bVFOA10orAbove = console.ThreadSafeTCIAccessor.VFOAFreq >= 10.0;
                                    bool bVFOB10orAbove = console.ThreadSafeTCIAccessor.VFOBFreq >= 10.0;
									
                                    if (rx == 0)
									{
										if(console.ThreadSafeTCIAccessor.VFOATX)
                                            bChange = bVFOA10orAbove;
										else
                                            bChange = bVFOB10orAbove;
                                    }
                                    else if (rx == 1)
									{
										if (console.ThreadSafeTCIAccessor.VFOBTX)
											bChange = bVFOB10orAbove;
										else
                                            bChange = bVFOA10orAbove;
                                    }
                                }
                            }
                            mode = bChange ? DSPMode.CWU : DSPMode.CWL;
                            break;
                        case "cwl":
							mode = DSPMode.CWL;
							break;
						case "cwu":
							mode = DSPMode.CWU;
							break;
						case "digl":
							mode = DSPMode.DIGL;
							break;
						case "digu":
							mode = DSPMode.DIGU;
							break;
						default:
							mode = DSPMode.FIRST;
							break;

					}
					if (mode != DSPMode.FIRST)
					{
						if (rx == 0)
						{
							if(console.ThreadSafeTCIAccessor.RX1DSPMode != mode)
								console.ThreadSafeTCIAccessor.RX1DSPMode = mode;
						}
						else if (rx == 1)
						{
							if(console.ThreadSafeTCIAccessor.RX2DSPMode != mode)
								console.ThreadSafeTCIAccessor.RX2DSPMode = mode;
						}
					}
				}
			}
            else if (bOK && args.Length == 1)
            {
                //query
                if (rx == 0)
                {
					sendMode(rx, console.ThreadSafeTCIAccessor.RX1DSPMode);
                }
				else if(rx == 1)
                {
					sendMode(rx, console.ThreadSafeTCIAccessor.RX2DSPMode);
				}
            }
        }
		private void handleDeleteSpot(string[] args)
        {
			if(args.Length == 1)
            {
				SpotManager2.DeleteSpot(args[0]);
            }
        }

        // line out to switch vacs
		private void lineOutEnable(int vac_number, bool enable)
		{			
			switch (vac_number)
			{
				case 0:
                    console.ThreadSafeTCIAccessor.Invoke(new MethodInvoker(() =>
                    {
						if (console.ThreadSafeTCIAccessor.SetupForm.VACEnable != enable)
						{
							console.ThreadSafeTCIAccessor.SetupForm.VACEnable = enable;
						}
                    }));
                    break;
				case 1:
                    console.ThreadSafeTCIAccessor.Invoke(new MethodInvoker(() =>
                    {
						if (console.ThreadSafeTCIAccessor.SetupForm.VAC2Enable != enable)
						{
							console.ThreadSafeTCIAccessor.SetupForm.VAC2Enable = enable;
						}
                    }));
                    break;
				default:
					break;
			}
        }

        private void handleLineOutStart(string[] args)
        {
            if (args.Length == 1)
            {
                bool bOK = int.TryParse(args[0], out int vac_number);

                if (bOK && !console.IsSetupFormNull)
                {
                    lineOutEnable(vac_number, true);
                }
            }
        }

        private void handleLineOutStop(string[] args)
        {
            if (args.Length == 1)
            {
                bool bOK = int.TryParse(args[0], out int vac_number);

                if (bOK && !console.IsSetupFormNull)
                {
                    lineOutEnable(vac_number, false);
                }
            }
        }
        //
        private void handleDrive(string[] args)
		{
			if (args.Length < 1) return;

			bool bOK = int.TryParse(args[0], out int rx);

			if (bOK && args.Length == 2)
            {
				//set
				bOK = int.TryParse(args[1], out int pwr);
                if (bOK)
                {
					console.ThreadSafeTCIAccessor.PWR = pwr;
                }
			}
			else if (bOK && args.Length == 1)
            {
				//read
				int pwr;
				if (console.SendLimitedPowerLevels)
					pwr = console.ThreadSafeTCIAccessor.PWRConstrained;
				else
					pwr = console.ThreadSafeTCIAccessor.PWR;

				sendDrivePower(rx, pwr);
            }
		}
		private void handleTuneDrive(string[] args)
        {
			if (args.Length < 1) return;

			bool bOK = int.TryParse(args[0], out int rx);

			if (bOK && args.Length == 2)
			{
				//set
				bOK = int.TryParse(args[1], out int pwr);
				if (bOK)
				{
					console.ThreadSafeTCIAccessor.TunePWR = pwr;
				}
			}
			else if (bOK && args.Length == 1)
			{
				//read
				sendTunePower(rx, console.ThreadSafeTCIAccessor.TunePWRConstrained);
			}
		}
        private void handleMute(string[] args, bool hasArgs = true)
        {
            if (hasArgs && args.Length == 1)
            {
                bool bOK = bool.TryParse(args[0], out bool mute);
                //set
                if (bOK)
                {
					console.ThreadSafeTCIAccessor.MUT = mute;
					console.ThreadSafeTCIAccessor.MUT2 = mute;
                }
            }
            else if (!hasArgs)
            {
                //read
                sendMute(console.ThreadSafeTCIAccessor.MUT || console.ThreadSafeTCIAccessor.MUT2);
            }
        }
        private void handleMuteRX(string[] args)
        {
            if (args.Length < 1) return;

            bool bOK = int.TryParse(args[0], out int rx);

            if (bOK && args.Length == 2)
            {
                //set
                bOK = bool.TryParse(args[1], out bool mute);
                if (bOK)
                {
                    if(rx == 0)
                        console.ThreadSafeTCIAccessor.MUT = mute;
                    else if(rx == 1)
                        console.ThreadSafeTCIAccessor.MUT2 = mute;
                }
            }
            else if (bOK && args.Length == 1)
            {
                //read
                sendMuteRX(rx, rx == 0 ? console.ThreadSafeTCIAccessor.MUT : console.ThreadSafeTCIAccessor.MUT2);
            }
        }
		private void handleMONEnable(string[] args, bool hasArgs = true)
		{
            if (hasArgs && args.Length == 1)
            {
                //set
                bool bOK = bool.TryParse(args[0], out bool enable);
                if (bOK)
                {
					console.ThreadSafeTCIAccessor.MON = enable;
                }
            }
            else if (!hasArgs)
            {
				//read
				sendMONEnable(console.ThreadSafeTCIAccessor.MON);
            }
        }
		private double linearToDbVolume(int volume)
		{
            double dbMin = -60f;
            double dbMax = 0;
            double linearMax = 100f;
            double linearMin = 0;

            double dbValue = ((volume - linearMin) / (linearMax - linearMin)) * (dbMax - dbMin) + dbMin;

            return Math.Max(dbMin, Math.Min(dbMax, dbValue));
        }
		private void handleMONVolume(string[] args, bool hasArgs = true)
		{
            if (hasArgs && args.Length == 1)
            {
                //set
                bool bOK = double.TryParse(args[0], out double dBLevel);
                if (bOK)
                {
                    double dbMin = -60f;
                    double dbMax = 0;
                    double linearMax = 100f;
                    double linearMin = 0;
                    double linearValue = ((dBLevel - dbMin) / (dbMax - dbMin)) * (linearMax - linearMin) + linearMin;
                    linearValue = Math.Max(linearMin, Math.Min(linearMax, linearValue));

                    console.ThreadSafeTCIAccessor.TXAF = (int)linearValue;
                }
            }
            else if (!hasArgs)
            {
				//read
                sendMONVolume(linearToDbVolume(console.ThreadSafeTCIAccessor.TXAF));
            }
        }

        private void handleSpotSimulateClick(string[] args)
        {
			if (m_server == null) return;

            if (args.Length == 2)
            {
				string callsign = args[0];
                bool bOK = long.TryParse(args[1], out long freq);
                if (bOK)
				{
					m_server.SendSpotSimulationClickToAll(callsign, freq);
                }
            }
        }

		private void handleSpot(string[] args, bool is_json, string msg)
        {
			if (args.Length >= 4) // 4 as argument 5 may contain commas
			{
				long freq = 0;
				uint argb = 0;
				DSPMode mode = DSPMode.FIRST;
				bool json_found = false;

                string sAdditional = "";
				if (!is_json)
				{
                    // join 5+ arguments back together
                    for (int i = 4; i < args.Length; i++)
					{
						sAdditional += args[i] + ",";
					}
					if (sAdditional.EndsWith(",")) sAdditional = sAdditional.Substring(0, sAdditional.Length - 1);
				}
				else
				{
					int thetis_tag = msg.ToLower().IndexOf("[json]{");
					if (thetis_tag > -1)
					{
						sAdditional = msg.Substring(thetis_tag + 6); // +8  =  the len of [json]
                        json_found = true;
					}
					else
					{
                        // we should have found a [json] tag, ignore if not
                        return;
					}
                }

				bool bOK = long.TryParse(args[2], out freq);
                if (bOK)
					bOK = uint.TryParse(args[3], out argb);
				if (bOK)
				{
					bOK = Enum.TryParse(args[1].ToUpper(), out mode);
                    if (!bOK)
                    {
						bool isFreqencyNormallyUSB = freq >= 10000000 || (freq >= 5300000 && freq < 5410000);

						string mode_filtered = SpotManager2.FilterForRawMode(args[1].ToLower());

                        switch (mode_filtered)
                        {
							case "lsb":
								mode = DSPMode.LSB;
								break;
							case "usb":
								mode = DSPMode.USB;
								break;
							case "am":
								mode = DSPMode.AM;
								break;
							case "fm":
                            case "nfm":
                                mode = DSPMode.FM;
								break;
							case "dsb":
								mode = DSPMode.DSB;
								break;
							case "drm":
								mode = DSPMode.DRM;
								break;
							case "spec":
								mode = DSPMode.SPEC;
								break;
							case "sam":
								mode = DSPMode.SAM;
								break;
							case "cwl":
								mode = DSPMode.CWL;
								break;
							case "cwu":
								mode = DSPMode.CWU;
                                break;
                            case "digu":
								mode = DSPMode.DIGU;
                                break;
                            case "digl":
								mode = DSPMode.DIGL;
                                break;
                            case "ssb":
								if(isFreqencyNormallyUSB)
									mode = DSPMode.USB;
								else
									mode = DSPMode.LSB;
								break;
							case "cw":
								switch(m_server.CWSpotForce)
								{
									case TCICWSpotForce.CWU:
                                        mode = DSPMode.CWU;
                                        break;
									case TCICWSpotForce.CWL:
                                        mode = DSPMode.CWL;
                                        break;
									case TCICWSpotForce.DEFAULT:
									default:
                                        if (isFreqencyNormallyUSB)
                                            mode = DSPMode.CWU;
                                        else
                                            mode = DSPMode.CWL;
                                        break;
								}
								break;
							case "rtty":
							case "psk":
							case "olivia":
							case "jt65":
							case "jt9":
							case "contesa":
							case "fsk":
							case "mt63":
							case "domi":
							case "packtor":
							case "sstv":
								if (isFreqencyNormallyUSB)
									mode = DSPMode.DIGU;
								else
									mode = DSPMode.DIGL;
								break;
							case "ft8":
								mode = DSPMode.DIGU; // always usb on ft8
								break;
							default:
								// unknown mode, just set to first, so that Display will draw it centred at least
								mode = DSPMode.FIRST;
								break;
                        }
						bOK = true;
                    }
				}
				if (bOK)
				{
					byte[] bytes = Encoding.UTF8.GetBytes(sAdditional);
					byte[] converted = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bytes);

					string sAdditionalConvertedText = Encoding.Unicode.GetString(converted, 0, converted.Length);

					if (sAdditionalConvertedText.ToLower() == "nil") sAdditionalConvertedText = ""; //[2.10.3.6]MW0LGE rumlog fills arg5 with Nil - spotted buy GW3JVB
																									//downside is that any additional text that is the string 'nil'
																									//will be removed, not that it is too much of an issue

					string spotter = "";
                    int heading = -1;
					string continent = "";
					string country = "";
					string spot_time = "";

                    if (json_found)
					{
						try
						{
                            JsonSpotData spotData = JsonConvert.DeserializeObject<JsonSpotData>(sAdditionalConvertedText);

							spotter = spotData.Spotter;
							heading = spotData.Heading;
							continent = spotData.Continent;
							country = spotData.Country;
							spot_time = spotData.UtcTime;

                            sAdditionalConvertedText = spotData.Comment;
                        }
                        catch 
						{
							// json failed in some way, ignore
							return;
						}
					}

                    SpotManager2.AddSpot(args[0], mode, freq, Color.FromArgb((int)argb), sAdditionalConvertedText, spotter, args[1], heading, continent, country, spot_time);
				}
			}
		}

        private void handleTune(string[] args)
        {
			int rx = 0;
			bool tune = false;

			if (args.Length == 2)
			{
				bool bOK = int.TryParse(args[0], out rx);
				if (bOK)
					bOK = bool.TryParse(args[1], out tune);
                if (bOK)
                {
					if(console.ThreadSafeTCIAccessor.TUN != tune)
						console.ThreadSafeTCIAccessor.TUN = tune;
                }
			}
            else if(args.Length == 1)
            {
				//query
				sendTune(rx, console.ThreadSafeTCIAccessor.TUN);
            }
		}
		private void handleRXEnable(string[] args)
        {
			int rx = 0;
			bool enable = false;

			bool bOK = int.TryParse(args[0], out rx);

			if (args.Length == 2)
			{				
				if (bOK)
					bOK = bool.TryParse(args[1], out enable);
				if (bOK)
				{
					// rx0 is always enabled
					if (rx == 1)
					{
						if (console.ThreadSafeTCIAccessor.RX2Enabled != enable)
							console.ThreadSafeTCIAccessor.RX2Enabled = enable;
					}
				}
			}
			else if (bOK && args.Length == 1)
			{
				//query
				if (rx == 0)
				{
					sendRXEnable(rx, !console.ThreadSafeTCIAccessor.MOX);
				}
                else if(rx == 1)
                {
					sendRXEnable(rx, console.ThreadSafeTCIAccessor.RX2Enabled && !console.ThreadSafeTCIAccessor.MOX);
                }
			}
		}
		private void parseTextFrame(string msg)
        {
			//Debug.Print("TCI Msg : " + msg);

			if (m_server != null && m_server.LogForm != null) m_server.LogForm.Log(true, msg);

			if (msg.EndsWith(";")) msg = msg.Substring(0, msg.Length - 1);

			string[] parts = msg.Split(':');

			bool json_spot = parts.Length >= 2 && parts[0].ToLower().Trim() == "spot" && msg.ToLower().IndexOf("[json]{") >= 0; // a spot with some json info

            if (parts.Length == 2 || json_spot)
            {
				string cmd = parts[0].ToLower().Trim();
				string[] args = parts[1].Split(',');

				switch (cmd)
                {
					case "modulation":
						handleModulationMessage(args);
						break;
					case "vfo":
						handleVFOMessage(args);
						break;
					case "trx":
						handleTrxMessage(args);
						break;
					case "split_enable":
						handleSplitEnableMessage(args);
						break;
					case "tune":
						handleTune(args);
						break;
					case "rx_enable":
						handleRXEnable(args);
						break;
					case "dds":
						handleDDS(args);
						break;
					case "if":
						handleIF(args);
						break;
					case "spot":
						handleSpot(args, json_spot, msg);
						break;
					case "spot_delete":
						handleDeleteSpot(args);
						break;
					case "drive":
						handleDrive(args);
						break;
					case "tune_drive":
						handleTuneDrive(args);
						break;
					case "mute":
                        handleMute(args);
						break;
                    case "rx_mute":
                        handleMuteRX(args);
						break;
					case "mon_volume":
						handleMONVolume(args);
						break;
					case "mon_enable":
                        handleMONEnable(args);
                        break;
                    case "line_out_start":
                        handleLineOutStart(args);
                        break;
                    case "line_out_stop":
                        handleLineOutStop(args);
						break;
					case "spot_simulate_click": // bespoke command to Thetis
						handleSpotSimulateClick(args);
						break;
                }
            }
			else if (parts.Length == 1)
            {
				string cmd = parts[0].ToLower().Trim();
				// just command
				switch (cmd)
                {
					case "start":
						handleStart();
						break;
					case "stop":
						handleStop();
						break;
					case "set_in_focus":
						handleSetInFocus();
						break;
                    case "mute":
                        handleMute(null, false);
                        break;
                    case "mon_enable":
                        handleMONEnable(null, false);
                        break;
                    case "mon_volume":
                        handleMONVolume(null, false);
                        break;
					case "spot_clear":
						handleSpotClear();
						break;
                }
            }
		}

		private void PingFrameTimer(object o)
		{
			sendPingFrame("Thetis");
		}

        private void VFOcallback(Object o)
        {
            TCPIPtciSocketListener.VFOData vfod = (TCPIPtciSocketListener.VFOData)o;
            vfoFrequencyChange(vfod);
        }
        private void Centrecallback(Object o)
        {
            TCPIPtciSocketListener.VFOData vfod = (TCPIPtciSocketListener.VFOData)o;
            centreFrequencyChange(vfod);
        }
        private void TXFrequencycallback(Object o)
        {
            TCPIPtciSocketListener.VFOData vfod = (TCPIPtciSocketListener.VFOData)o;
            TXFrequencyChange(vfod);
        }
        public void VFOChange(VFOData vfod)
        {
            if (m_tmVFOtimer != null)
            {
                m_tmVFOtimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_tmVFOtimer = null;
            }

			bool bOK = !m_swVFO.IsRunning || (m_swVFO.IsRunning && m_swVFO.ElapsedMilliseconds > m_nRateLimit);

            if (bOK)
            {
                vfoFrequencyChange(vfod);
                if (m_nRateLimit > 0) m_swVFO.Restart();
            }
            else
            {
                m_tmVFOtimer = new System.Threading.Timer(VFOcallback, vfod, m_nRateLimit, Timeout.Infinite);
            }
        }
		public void CentreChange(VFOData vfod)
        {
            if (m_tmCentretimer != null)
            {
                m_tmCentretimer.Change(Timeout.Infinite, Timeout.Infinite);
                m_tmCentretimer = null;
            }

			bool bOK = !m_swCentre.IsRunning || (m_swCentre.IsRunning && m_swCentre.ElapsedMilliseconds > m_nRateLimit);

            if (bOK)
            {
                centreFrequencyChange(vfod);
                if (m_nRateLimit > 0) m_swCentre.Restart();
            }
            else
            {
                m_tmCentretimer = new System.Threading.Timer(Centrecallback, vfod, m_nRateLimit, Timeout.Infinite);
            }
        }
        public void TXFrequencyChange(VFOData vfod)
        {
            if (m_tmTXFrequency != null)
            {
                m_tmTXFrequency.Change(Timeout.Infinite, Timeout.Infinite);
                m_tmTXFrequency = null;
            }

            bool bOK = !m_swTXFrequency.IsRunning || (m_swTXFrequency.IsRunning && m_swTXFrequency.ElapsedMilliseconds > m_nRateLimit);

            if (bOK)
            {
                txFrequencyChange(vfod);
                if (m_nRateLimit > 0) m_swTXFrequency.Restart();
            }
            else
            {
                m_tmTXFrequency = new System.Threading.Timer(Centrecallback, vfod, m_nRateLimit, Timeout.Infinite);
            }
        }
    }

	public class TCPIPtciServer
	{
		//
		public delegate void ClientConnected();
		public delegate void ClientDisconnected();
		public delegate void ClientError(SocketException se);
		public delegate void ServerError(SocketException se);
		public ClientConnected ClientConnectedHandlers;
		public ClientDisconnected ClientDisconnectedHandlers;
		public ClientError ClientErrorHandlers;
		public ServerError ServerErrorHandlers;
		//

		private Console _console;

		public static IPAddress DEFAULT_SERVER = IPAddress.Parse("127.0.0.1");
		public static int DEFAULT_PORT = 31001;
		public static IPEndPoint DEFAULT_IP_END_POINT =
			new IPEndPoint(DEFAULT_SERVER, DEFAULT_PORT);

		private TcpListener m_server = null;
		private bool m_stopServer = false;
		private bool m_stopPurging = false;
		private Thread m_serverThread = null;
		private Thread m_purgingThread = null;
		private List<TCPIPtciSocketListener> m_socketListenersList = null;
		private Object m_objLocker = new Object();
		private bool m_bSleepingInPurge = false;
		private bool m_bDelegatesAdded = false;
		private int m_nRateLimit = 0;
		private bool m_bEmulateSunSDR2Pro = false;
		private bool m_bEmulateExpertSDR3Protocol = false;

		private frmLog _log;

		private Console console
		{
			get
			{
				if (_console == null) return null;

				if (_console.InvokeRequired)
				{
                    return (Console)_console.Invoke(new Func<Console>(() => _console));
                }
				else 
					return _console;
			}
		}
		public TCPIPtciServer()
		{
			Init(DEFAULT_IP_END_POINT);
		}
		public TCPIPtciServer(IPAddress serverIP)
		{
			Init(new IPEndPoint(serverIP, DEFAULT_PORT));
		}

		public TCPIPtciServer(int port)
		{
			Init(new IPEndPoint(DEFAULT_SERVER, port));
		}

		public TCPIPtciServer(IPAddress serverIP, int port)
		{
			Init(new IPEndPoint(serverIP, port));
		}

		public TCPIPtciServer(IPEndPoint ipNport)
		{
			Init(ipNport);
		}
		~TCPIPtciServer()
		{
			StopServer();
			if(_log != null)
            {
				_log.Close();
				_log = null;
            }
		}

		private void Init(IPEndPoint ipNport)
		{
			try
			{
				if (_log != null)
                {
					_log = null;
                }
				_log = new frmLog();
				m_server = new TcpListener(ipNport);
			}
			catch
			{
				m_server = null;
            }
        }

		public frmLog LogForm
        {
            get { return _log; }
        }
		private bool m_bCopyRX2VFObToVFOa = false;
		public bool CopyRX2VFObToVFOa
		{
			get { return m_bCopyRX2VFObToVFOa; }
			set { m_bCopyRX2VFObToVFOa = value;	}
        }
        private bool _replace_if_copy_RX2VFObToVFOa = false;
        public bool ReplaceRX2VFObToVFOa
        {
			get { return _replace_if_copy_RX2VFObToVFOa; }
			set { _replace_if_copy_RX2VFObToVFOa = value; }
        }
        private bool m_bCWLUbecomesCW = false;
		public bool CWLUbecomesCW
        {
			get { return m_bCWLUbecomesCW; }
			set { m_bCWLUbecomesCW = value; }
        }
		private bool m_bCWbecomesCWUabove10mhz = false;
		public bool CWbecomesCWUabove10mhz
		{
			get { return m_bCWbecomesCWUabove10mhz; }
			set { m_bCWbecomesCWUabove10mhz = value; }
		}
        private TCICWSpotForce _spot_force = TCICWSpotForce.DEFAULT;
        public TCICWSpotForce CWSpotForce
        {
            get { return _spot_force; }
            set { _spot_force = value; }
        }
        private bool m_bUseRX1VFOaForRX2VFOa = false;
		public bool UseRX1VFOaForRX2VFOa
		{
			get { return m_bUseRX1VFOaForRX2VFOa; }
			set { m_bUseRX1VFOaForRX2VFOa = value; }
		}
		private bool m_bSendInitialStateOnConnect = true;
		public bool SendInitialFrequencyStateOnConnect
		{
			get { return m_bSendInitialStateOnConnect; }
			set { m_bSendInitialStateOnConnect = value;	}
		}
		public bool EmulateSunSDR2Pro
        {
            get { return m_bEmulateSunSDR2Pro; }
			set { m_bEmulateSunSDR2Pro = value; }
        }
		public bool EmulateExpertSDR3Protocol
		{
			get { return m_bEmulateExpertSDR3Protocol; }
			set { m_bEmulateExpertSDR3Protocol = value; }
		}
		public void StartServer(Console c, int rateLimit = 0)
		{
			if (m_server != null)
			{
				m_nRateLimit = rateLimit;

				if (c != null && !c.IsSetupFormNull)
				{
					m_bSendInitialStateOnConnect = c.SetupForm.TCIsendInitialStateOnConnect;;

					m_bCopyRX2VFObToVFOa = c.SetupForm.TCIcopyRX2VFObToVFOa;
					_replace_if_copy_RX2VFObToVFOa = c.SetupForm.TCIreplaceRX2VFObToVFOa;
					m_bUseRX1VFOaForRX2VFOa = c.SetupForm.TCIuseRX1vfoaForRX2vfoa;

					m_bCWLUbecomesCW = c.SetupForm.TCICWLUbecomesCW;
					m_bCWbecomesCWUabove10mhz = c.SetupForm.TCICWbecomesCWUabove10mhz; //[2.10.3.9]MW0LGE fixes issue #559

					m_bEmulateSunSDR2Pro = c.SetupForm.EmulateSunSDR2Pro;
					m_bEmulateExpertSDR3Protocol = c.SetupForm.EmulateExpertSDR3Protocol;

					_spot_force = c.SetupForm.CWSpotForce;
                }
				else
				{
					m_bSendInitialStateOnConnect = true;
					m_bCopyRX2VFObToVFOa = false;
					_replace_if_copy_RX2VFObToVFOa = false;
					m_bUseRX1VFOaForRX2VFOa = false;
					m_bCWLUbecomesCW = false;
					m_bCWbecomesCWUabove10mhz = false;
					m_bEmulateSunSDR2Pro = false;
					m_bEmulateExpertSDR3Protocol = false;
					_spot_force = TCICWSpotForce.DEFAULT;
                }

				_console = c;

				m_socketListenersList = new List<TCPIPtciSocketListener>();

                if (console != null && !m_bDelegatesAdded)
				{
					console.ThreadSafeTCIAccessor.VFOAFrequencyChangeHandlers += OnVFOAFrequencyChangeHandler;
					console.ThreadSafeTCIAccessor.VFOBFrequencyChangeHandlers += OnVFOBFrequencyChangeHandler;
					console.ThreadSafeTCIAccessor.MoxChangeHandlers += OnMoxChangeHandler;
					console.ThreadSafeTCIAccessor.ModeChangeHandlers += OnModeChangeHandler;
					console.ThreadSafeTCIAccessor.BandChangeHandlers += OnBandChangeHandler;
					console.ThreadSafeTCIAccessor.CentreFrequencyHandlers += OnCentreFrequencyChanged;
					console.ThreadSafeTCIAccessor.FilterChangedHandlers += OnFilterChanged;
					console.ThreadSafeTCIAccessor.FilterEdgesChangedHandlers += OnFilterEdgesChanged;
					console.ThreadSafeTCIAccessor.PowerChangeHanders += OnPowerChangeHander;
					console.ThreadSafeTCIAccessor.SplitChangedHandlers += OnSplitChanged;
					console.ThreadSafeTCIAccessor.TuneChangedHandlers += OnTuneChanged;
					console.ThreadSafeTCIAccessor.DrivePowerChangedHandlers += OnDrivePowerChanged;
					console.ThreadSafeTCIAccessor.SampleRateChangedHandlers += OnSampleRateChanged;
					console.ThreadSafeTCIAccessor.ThetisFocusChangedHandlers += OnThetisFocusChanged;
					console.ThreadSafeTCIAccessor.RX2EnabledChangedHandlers += OnRX2EnabledChanged;
					console.ThreadSafeTCIAccessor.SpotClickedHandlers += OnSpotClicked;
					console.ThreadSafeTCIAccessor.MuteChangedHandlers += OnMuteChanged;
					console.ThreadSafeTCIAccessor.MONChangedHandlers += OnMONChanged;
                    console.ThreadSafeTCIAccessor.MONVolumeChangedHandlers += OnMONVolumeChanged;
					console.ThreadSafeTCIAccessor.TXFrequncyChangedHandlers += OnTXFrequencyChanged;

                    m_bDelegatesAdded = true;
				}

				try
				{
					m_server.Start();

					m_serverThread = new Thread(new ThreadStart(ServerThreadStart));
					m_serverThread.Priority = ThreadPriority.BelowNormal;
					m_serverThread.Name = "TCI server Thread";
					m_serverThread.Start();

					m_purgingThread = new Thread(new ThreadStart(PurgingThreadStart));
					m_purgingThread.Priority = ThreadPriority.Lowest;
                    m_purgingThread.Name = "TCI purging Thread";
                    m_purgingThread.Start();
				}
				catch(SocketException se)
                {
					m_sLastError = se.Message;
					StopServer();

					ServerErrorHandlers?.Invoke(se);
				}
                catch
				{
					StopServer();
				}
			}
		}

		//public void SendToClients(string sMsg)
		//{
		//	if (m_server == null) return;

		//	try
		//	{
		//		lock (m_objLocker)
		//		{
		//			foreach (TCPIPtciSocketListener tsl in m_socketListenersList)
		//			{
		//				tsl.SendClientData(sMsg);
		//			}
		//		}
		//	}
		//	catch (Exception e)
		//	{
		//	}
		//}

		private string m_sLastError = "";
		public string LastError
        {
			get {
				string s = m_sLastError;
				m_sLastError = "";
				return s; 
			}
        }

		public void StopServer()
		{
			if (m_server != null)
			{
				if (m_bDelegatesAdded)
				{
					console.ThreadSafeTCIAccessor.VFOAFrequencyChangeHandlers -= OnVFOAFrequencyChangeHandler;
					console.ThreadSafeTCIAccessor.VFOBFrequencyChangeHandlers -= OnVFOBFrequencyChangeHandler;
					console.ThreadSafeTCIAccessor.MoxChangeHandlers -= OnMoxChangeHandler;
					console.ThreadSafeTCIAccessor.ModeChangeHandlers -= OnModeChangeHandler;
					console.ThreadSafeTCIAccessor.BandChangeHandlers -= OnBandChangeHandler;
					console.ThreadSafeTCIAccessor.CentreFrequencyHandlers -= OnCentreFrequencyChanged;
					console.ThreadSafeTCIAccessor.FilterChangedHandlers -= OnFilterChanged;
					console.ThreadSafeTCIAccessor.FilterEdgesChangedHandlers -= OnFilterEdgesChanged;
					console.ThreadSafeTCIAccessor.PowerChangeHanders -= OnPowerChangeHander;
					console.ThreadSafeTCIAccessor.SplitChangedHandlers -= OnSplitChanged;
					console.ThreadSafeTCIAccessor.TuneChangedHandlers -= OnTuneChanged;
					console.ThreadSafeTCIAccessor.DrivePowerChangedHandlers -= OnDrivePowerChanged;
					console.ThreadSafeTCIAccessor.SampleRateChangedHandlers -= OnSampleRateChanged;
					console.ThreadSafeTCIAccessor.ThetisFocusChangedHandlers -= OnThetisFocusChanged;
					console.ThreadSafeTCIAccessor.RX2EnabledChangedHandlers -= OnRX2EnabledChanged;
					console.ThreadSafeTCIAccessor.SpotClickedHandlers -= OnSpotClicked;
                    console.ThreadSafeTCIAccessor.MuteChangedHandlers -= OnMuteChanged;
                    console.ThreadSafeTCIAccessor.MONChangedHandlers -= OnMONChanged;
                    console.ThreadSafeTCIAccessor.MONVolumeChangedHandlers -= OnMONVolumeChanged;
                    console.ThreadSafeTCIAccessor.TXFrequncyChangedHandlers -= OnTXFrequencyChanged;

                    m_bDelegatesAdded = false;
				}

				// Stop the TCP/IP Server, so can clean up without more clients connecting
				m_stopServer = true;
				try
				{
					m_server.Stop();
				}
                catch
                {

                }

				if (m_serverThread != null) {
					m_serverThread.Join(50); // dont need to wait long here, as we are blocking anyway
					if (m_serverThread.IsAlive)
						m_serverThread.Abort();
					m_serverThread = null;
				}

				m_stopPurging = true;
				if (m_purgingThread != null)
				{
					if (!m_bSleepingInPurge)
						m_purgingThread.Join(500);

					if (m_purgingThread.IsAlive)
						m_purgingThread.Abort();
					m_purgingThread = null;
				}

				m_server = null;

				// Stop All clients.
				StopAllSocketListers();
			}
		}

		public int ClientsConnected
        {
            get {
				if(m_server == null || m_socketListenersList == null) return 0;

				int nRet = 0;
				lock (m_objLocker)
				{
					foreach(TCPIPtciSocketListener socketListener in m_socketListenersList)
                    {
						if (!socketListener.IsDisconnected()) nRet++;
                    }
				}
				return nRet;
			}
        }
		public bool IsServerRunning
        {
			get { return m_server != null; }
        }
		private void StopAllSocketListers()
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener
							 in m_socketListenersList)
				{
					socketListener.StopSocketListener();

					socketListener.ClientConnectedHandlers -= ClientConnectedHandler;
					socketListener.ClientDisconnectedHandlers -= ClientDisconnectedHandler;
					socketListener.ClientErrorHandlers -= ClientErrorHandler;
				}

				m_socketListenersList.Clear();
				m_socketListenersList = null;
			}
		}

		private void ServerThreadStart()
		{
			//Socket clientSocket = null;
			TcpClient client = null;

			TCPIPtciSocketListener socketListener = null;
			bool bAddedDelegates = false;
			while (!m_stopServer)
			{
				try
				{
					bAddedDelegates = false;
					//clientSocket = m_server.AcceptSocket();
					client = m_server.AcceptTcpClient();
					client.NoDelay = true;

					socketListener = new TCPIPtciSocketListener(client, console, this, m_nRateLimit);

					lock (m_objLocker)
					{
						m_socketListenersList.Add(socketListener);
					}

					socketListener.ClientConnectedHandlers += ClientConnectedHandler;
					socketListener.ClientDisconnectedHandlers += ClientDisconnectedHandler;
					socketListener.ClientErrorHandlers += ClientErrorHandler;
					bAddedDelegates = true;

					socketListener.StartSocketListener();
				}
				catch (SocketException se)
				{
                    if (bAddedDelegates && socketListener != null)
                    {
						socketListener.ClientConnectedHandlers -= ClientConnectedHandler;
						socketListener.ClientDisconnectedHandlers -= ClientDisconnectedHandler;
						socketListener.ClientErrorHandlers -= ClientErrorHandler;
					}
					m_stopServer = true;
					m_sLastError = se.Message;
					ServerErrorHandlers?.Invoke(se);
				}
                catch
                {
					if (bAddedDelegates && socketListener != null)
					{
						socketListener.ClientConnectedHandlers -= ClientConnectedHandler;
						socketListener.ClientDisconnectedHandlers -= ClientDisconnectedHandler;
						socketListener.ClientErrorHandlers -= ClientErrorHandler;
					}
					m_stopServer = true;
				}
			}
		}

		private void PurgingThreadStart()
		{
			while (!m_stopPurging)
			{
                List<TCPIPtciSocketListener> deleteList = new List<TCPIPtciSocketListener>();

				lock (m_objLocker)
				{
					foreach (TCPIPtciSocketListener socketListener
								 in m_socketListenersList)
					{
						if (socketListener.IsMarkedForDeletion())
						{
							deleteList.Add(socketListener);
							socketListener.StopSocketListener();

							socketListener.ClientConnectedHandlers -= ClientConnectedHandler;
							socketListener.ClientDisconnectedHandlers -= ClientDisconnectedHandler;
							socketListener.ClientErrorHandlers -= ClientErrorHandler;
						}
					}

					for (int i = 0; i < deleteList.Count; ++i)
					{
						m_socketListenersList.Remove(deleteList[i]);
					}
				}

				deleteList = null;

				m_bSleepingInPurge = true;
				Thread.Sleep(5000);
				m_bSleepingInPurge = false;
			}
		}

		private void ClientConnectedHandler()
		{
			ClientConnectedHandlers?.Invoke();
		}
		private void ClientDisconnectedHandler()
        {
			ClientDisconnectedHandlers?.Invoke();
        }
		private void ClientErrorHandler(SocketException se)
        {
			m_sLastError = se.Message;

			ClientErrorHandlers?.Invoke(se);
		}

		public void OnVFOAFrequencyChangeHandler(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
		{
            bool bVFOaUseRX2;
            if (console != null)
                bVFOaUseRX2 = console.ThreadSafeTCIAccessor.RX2Enabled && UseRX1VFOaForRX2VFOa;
            else
                bVFOaUseRX2 = false;

            TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData()
            {
                cen = false,
                centreMHz = -1,
                rx = bVFOaUseRX2 ? 1 : rx - 1,
                freqMHz = newFreq,
                offsetHz = (int)-offset,
                chan = 0,
                duplicate_tochan = -1,
                replace_if_duplicated = false,
                sendIF = true
            };

            lock (m_objLocker)
            {
                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
					socketListener.VFOChange(vfod);
                }
            }
        }
		public void OnVFOBFrequencyChangeHandler(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
		{
            TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData()
            {
                cen = false,
                centreMHz = -1,
                rx = rx - 1,
                freqMHz = newFreq,
                offsetHz = (int)-offset,
                chan = 1,
                duplicate_tochan = m_bCopyRX2VFObToVFOa && console.ThreadSafeTCIAccessor.RX2Enabled ? 0 : -1,
                replace_if_duplicated = m_bCopyRX2VFObToVFOa && _replace_if_copy_RX2VFObToVFOa && console.ThreadSafeTCIAccessor.RX2Enabled,
                sendIF = true
            };

			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.VFOChange(vfod);
				}
			}
        }
		public void OnMoxChangeHandler(int rx, bool oldMox, bool newMox)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.MoxChange(rx, oldMox, newMox);
				}
			}
		}
		public void OnModeChangeHandler(int rx, DSPMode oldMode, DSPMode newMode, Band oldBand, Band newBand)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.ModeChange(rx,oldMode,newMode, oldBand, newBand);
				}
			}
		}
		public void OnBandChangeHandler(int rx, Band oldBand, Band newBand)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.BandChange(rx, oldBand, newBand);
				}
			}
		}
		public void OnCentreFrequencyChanged(int rx, double oldFreq, double newFreq, Band band, double offset)
		{
			//only want to send IF with this if CTUN is enabled
			bool bCTun = rx == 1 ? console.ThreadSafeTCIAccessor.ClickTuneDisplay : console.ThreadSafeTCIAccessor.ClickTuneRX2Display;

            TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData()
            {
                freqMHz = -1,
                offsetHz = bCTun ? (int)-offset : -1,
                chan = 0,
                centreMHz = newFreq,
                cen = true,
                rx = rx - 1,
                duplicate_tochan = -1,
                replace_if_duplicated = false,
                sendIF = bCTun
            };
            lock (m_objLocker)
            {
                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.CentreChange(vfod);
                }
            }
        }
		public void OnFilterChanged(int rx, Filter oldFilter, Filter newFilter, Band band, int low, int high, string sName)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.FilterChange(rx, oldFilter, newFilter, band, low, high);
				}
			}
		}
		public void OnFilterEdgesChanged(int rx, Filter filter, Band band, int low, int high, string sName, int max_width, int max_shift)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.FilterEdgesChange(rx, filter, band, low, high);
				}
			}
		}
		public void OnPowerChangeHander(bool oldPower, bool newPower)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.PowerChange(oldPower, newPower);
				}
			}
		}
		public void OnThetisFocusChanged(bool focus)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.ThetisFocusChange(focus);
				}
			}
		}
		public void OnRX2EnabledChanged(bool enabled)
        {
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.RX2EnabledChange(enabled);
				}
			}
		}
		private void OnSampleRateChanged(int rx, int oldSampleRate, int newSampleRate)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.SampleRateChange(rx, oldSampleRate, newSampleRate);
				}
			}
		}
		private void OnDrivePowerChanged(int rx, int newPower, bool tune)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.DrivePowerChange(rx, newPower, tune);
				}
			}
		}
		private void OnTuneChanged(int rx, bool oldTune, bool newTune)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.TuneChange(rx, oldTune, newTune);
				}
			}
		}
		private void OnSplitChanged(int rx, bool oldSplit, bool newSplit)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.SplitChange(rx, newSplit);
				}
			}
		}

		private void OnSpotClicked(string callsign, long frequencyHz, int rx = -1, bool vfoB = false)
		{
			lock (m_objLocker)
			{
				foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
				{
					socketListener.ClickedOnSpot(callsign, frequencyHz); // also send legacy command (EESDR3 does this)	MW0LGE [2.9.0.8]																	
                    socketListener.ClickedOnSpot(callsign, frequencyHz, rx, vfoB ? 1 : 0);
				}
			}
		}
		private void OnMuteChanged(int rx, bool oldState, bool newState)
		{
            lock (m_objLocker)
            {
                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.MuteChanged(rx, newState);
                }
            }
        }
        private void OnMONChanged(bool oldState, bool newState)
        {
            lock (m_objLocker)
            {
                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.MONChanged(newState);
                }
            }
        }
        private void OnMONVolumeChanged(int oldVolume, int newVolume)
        {
            lock (m_objLocker)
            {
                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.MONVolumeChanged(newVolume);
                }
            }
        }
		private void OnTXFrequencyChanged(double old_frequency, double new_frequency, Band old_band, Band new_band, bool rx2_enabled, bool tx_vfob, double centre_freq)
		{
			TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData()
			{
				cen = false,
				centreMHz = -1,
				rx = -1,
				freqMHz = new_frequency,
				offsetHz = -1,
				chan = -1,
				duplicate_tochan = -1,
				replace_if_duplicated = false,
				sendIF = false,

				SendTXInfo = true,
				TXfreqBand = new_band,
                RX2EnabledForTX = rx2_enabled,
				TXVFOB = tx_vfob
			};

            lock (m_objLocker)
            {
                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
                    socketListener.TXFrequencyChange(vfod);
                }
            }
        }
        public void ShowLog()
        {
			if (_log != null) _log.ShowWithTitle("TCI");
		}

		public void CloseLog()
        {
			if (_log != null) _log.Hide();
		}
		public void SendSpotSimulationClickToAll(string callsign, long freq)
		{
			lock (m_objLocker)
			{
                foreach (TCPIPtciSocketListener socketListener in m_socketListenersList)
                {
					socketListener.ClickedOnSpot(callsign, freq);
                    socketListener.ClickedOnSpot(callsign, freq, 1, 0); //[2.10.3.9]MW0LGE also send out RX_CLICKED_ON_SPOT defaults to rx1 and vfoA
                }
            }
		}
        public async Task ConnectToServer(string serverAddress, int port, int timeoutMilliseconds)
        {
            using (TcpClient client = new TcpClient())
            {
                Task connectTask = client.ConnectAsync(serverAddress, port);
                Task timeoutTask = Task.Delay(timeoutMilliseconds);

                Task completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == connectTask)
                {
                    client.Close();
                }
                else
                {
                    //throw new TimeoutException("Connection timed out.");
                }
            }
        }
    }
}
