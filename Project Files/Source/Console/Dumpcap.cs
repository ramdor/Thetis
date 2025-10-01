/*  Dumpcap.cs

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Thetis
{
    class DumpCap
    {
        private static Console m_objConsole;
        private static Thread m_objRunThread;
        private static int m_nTimeOut;
        private static bool m_bEnabled = false;
        private static bool m_bKillOnNegativeSeqOnly = true;
        private static int m_nInterface = 1;
        private static string m_sWireSharkPath = "";
        private static int m_nFileSizeKB = 10000;
        private static int m_nNumberOfFiles = 2;
        private static int m_nProcessID = -1;
        private static bool m_bClearFolderOnRestart = true;

        public static bool DumpCapExists()
        {
            return File.Exists(m_sWireSharkPath + "\\dumpcap.exe ");
        }

        public static bool Enabled {
            get { return m_bEnabled; }
            set { 
                m_bEnabled = value;
                if (m_bEnabled)
                {
                    StartDumpcap(m_nTimeOut);
                }
                else
                {
                    StopDumpcap();
                }
            }
        }

        public static int Interface {
            get { return m_nInterface; }
            set { 
                m_nInterface = value;
                restartDumpcap();
            }
        }
        public static string WireSharkPath {
            get { return m_sWireSharkPath ; }
            set { 
                m_sWireSharkPath = value;
                restartDumpcap();
            }
        }
        public static int FileSizeKB {
            get { return m_nFileSizeKB; }
            set { 
                m_nFileSizeKB = value;
                restartDumpcap();
            }
        }
        public static int NumberOfFiles {
            get { return m_nNumberOfFiles; }
            set { 
                m_nNumberOfFiles = value;
                restartDumpcap();
            }
        }

        public static bool ClearFolderOnRestart {
            // just a datastore, used by console
            get { return m_bClearFolderOnRestart; }
            set { m_bClearFolderOnRestart = value; }
        }
        public static bool KillOnNegativeSeqOnly {
            // just a datastore, used by console
            get { return m_bKillOnNegativeSeqOnly; }
            set { m_bKillOnNegativeSeqOnly = value; }
        }

        private static void restartDumpcap()
        {
            if (isDumpcapRunning())
            {
                StopDumpcap();
                StartDumpcap(m_nTimeOut);
            }
        }

        public static void Initalise(Console c)
        {
            m_objConsole = c;
        }

        private static string workingFolder {
            get {
                if (m_objConsole == null) return "";
                return  m_objConsole.AppDataPath + "dumpcap\\"; 
            }
        }
        public static void ClearDumpFolder()
        {
            if (m_objConsole == null) return;
            if (workingFolder == "") return;

            if (Directory.Exists(workingFolder))
            {
                DirectoryInfo di = new DirectoryInfo(workingFolder);

                foreach (FileInfo file in di.GetFiles("*.pcapng"))
                {
                    try { file.Delete(); }
                    catch { }                    
                }
            }                
        }

        private static void dumpcapGO()
        {
            if (!DumpCapExists()) return;
            if (m_objConsole == null) return;
            if (workingFolder == "") return;

            int n = 0;
            while (isDumpcapRunning() && n < m_nTimeOut)
            {
                // hold up so that it has time to shut down if needed
                Thread.Sleep(1);
                n++;
            }

            if (!isDumpcapRunning())
            {
                try
                {
                    string sArguments = String.Format("-i {0} -b filesize:{1} -b files:{2} -w dumpcap_thetis.pcapng", m_nInterface, m_nFileSizeKB, m_nNumberOfFiles);

                    if (!Directory.Exists(workingFolder))
                        Directory.CreateDirectory(workingFolder);

                    using (Process myProcess = new Process())
                    {
                        myProcess.StartInfo.UseShellExecute = false;
                        myProcess.StartInfo.WorkingDirectory = workingFolder;
                        myProcess.StartInfo.FileName = m_sWireSharkPath + "\\dumpcap.exe ";
                        myProcess.StartInfo.Arguments = sArguments;
                        myProcess.StartInfo.CreateNoWindow = true;
                        myProcess.Start();
                        m_nProcessID = myProcess.Id;
                    }
                }
                catch
                {
                }
            }
        }

        public static void StartDumpcap(int nTimeOut)
        {
            if (!m_bEnabled || isDumpcapRunning()) return;

            if (m_objRunThread == null || !m_objRunThread.IsAlive)
            {
                m_nTimeOut = nTimeOut;

                m_objRunThread = new Thread(new ThreadStart(dumpcapGO))
                {
                    Name = "Dumpcap start Thread",
                    Priority = ThreadPriority.AboveNormal,
                    IsBackground = true
                };
                m_objRunThread.Start();
            }
        }

        public static void StopDumpcap()
        {
            if (!isDumpcapRunning()) return;

            Process[] proc = Process.GetProcessesByName("dumpcap");
                
            foreach (Process p in proc)
            {
                if (p.Id == m_nProcessID)
                {
                    try { 
                        p.Kill();
                        m_nProcessID = -1;
                    }
                    catch { }
                    break;
                }
            }
        }

        private static bool isDumpcapRunning()
        {
            if (m_nProcessID == -1) return false;

            bool bRet = false;

            Process[] proc = Process.GetProcessesByName("dumpcap");
            foreach(Process p in proc)
            {
                if (p.Id == m_nProcessID)
                {
                    bRet = true;
                    break;
                }
            }

            return bRet; 
        }

        public static void ShowAppPathFolder()
        {
            if (m_objConsole == null) return;
            if (workingFolder == "") return;

            if (!Directory.Exists(workingFolder)) return;

            try {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = workingFolder,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch { }
        }
    }
}
