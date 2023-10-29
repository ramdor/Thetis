//=================================================================
// basicaudio.cs - MW0LGE
//=================================================================

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
            catch (FileNotFoundException ex)
            {
                m_bLoading = false;
                loadCompleted?.Invoke(false);
            }
            catch (TimeoutException ex)
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
