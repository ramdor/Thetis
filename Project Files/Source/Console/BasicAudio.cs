/*  BasicAudio.cs

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
using System.Media;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace Thetis
{
    public class BasicAudio
    {
        private SoundPlayer m_objPlayer;
        private bool m_bOkToPlay = false;
        private bool m_bLoading = false;
        private Thread m_objThread;

        private event LoadComplededEventHandler loadCompleted;
        public delegate void LoadComplededEventHandler(bool bLoadedOk);

        public event LoadComplededEventHandler LoadCompletedEvent {
            add {
                loadCompleted += value;
            }
            remove {
                loadCompleted -= value;
            }
        }

        public BasicAudio()
        {
            m_objPlayer = new SoundPlayer();
            m_objPlayer.LoadCompleted += new AsyncCompletedEventHandler(player_LoadCompleted);
            m_objPlayer.SoundLocationChanged += new EventHandler(player_LocationChanged);            
        }
        private void player_LocationChanged(object sender, EventArgs e)
        {
            m_bOkToPlay = false;
        }
        private void player_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            m_bOkToPlay = !(e.Cancelled || e.Error != null);
            m_bLoading = false;
            loadCompleted?.Invoke(m_bOkToPlay);
        }

        public bool IsReady {
            get { return m_bOkToPlay; }
            set { }
        }
        public string SoundFile {
            get { return m_objPlayer.SoundLocation; }
            set { }
        }
        public void LoadSound(string sFile)
        {
            if (m_bLoading || sFile == "") return;
            m_bOkToPlay = false;
            m_bLoading = true;

            m_objPlayer.SoundLocation = sFile;
            m_objPlayer.LoadTimeout = 1000;
            try
            {
                m_objPlayer.LoadAsync();
            }
            catch (FileNotFoundException)
            {
                m_bLoading = false;
                loadCompleted?.Invoke(false);
            }
            catch (TimeoutException)
            {
                m_bLoading = false;
                loadCompleted?.Invoke(false);
            }
        }
        public void Play()
        {            
            if (!m_bOkToPlay) return;

            if (m_objThread == null || !m_objThread.IsAlive)
            {
                // even though m_objPlayer.Play() is async for some reason perhaps on initial play there would
                // be noticabled glitch in specturm. Starting it on this thread seems to reduce the occurance
                m_objThread = new Thread(new ThreadStart(playSound));
                m_objThread.Name = "Basic Audio Thread";
                m_objThread.Priority = ThreadPriority.BelowNormal;
                m_objThread.IsBackground = true;
                m_objThread.Start();
            }
        }
        public void Stop()
        {
            m_objPlayer.Stop();
        }

        private void playSound()
        {
            try
            {
                m_objPlayer.Play();
            }
            catch
            {
            }
        }
    }
}
