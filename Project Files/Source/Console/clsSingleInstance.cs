/*  clsSingleInstance.cs

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
using System.Threading;
using System.Windows.Forms;

namespace Thetis
{
    static class SingleInstance
    {
        private const string MUTEX_NAME = @"Global\Thetis_7F1F9E7F-6C3E-4F3E-9E4C-8E0A3C6E8C11"; // some random ID specifc to THETIS
                                                                                                 // this could be changed for a HL2 version for example
        private static Mutex _mutex;
        private static bool _owns_mutex;

        public static bool CheckAndPrompt()
        {
            // a much better single instance checker, that doesnt rely on process list
            // creates a global mutex, across all logon sessions including remote desktop
            // crash safety is built in as we will see an abandonded mutex eception
            // release must be called on shutdown

            try
            {
                _mutex = new Mutex(true, MUTEX_NAME, out bool created_new);

                if (created_new)
                {
                    _owns_mutex = true;
                    return true;
                }

                try
                {
                    if (_mutex.WaitOne(TimeSpan.Zero))
                    {
                        _owns_mutex = true;
                        return true;
                    }
                }
                catch (AbandonedMutexException)
                {
                    _owns_mutex = true;
                    return true;
                }

                DialogResult dr = MessageBox.Show("There is another Thetis instance running.\nAre you sure you want to continue?", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                if (dr == DialogResult.No) return false;
                return true;
            }
            catch (Exception ex)
            {
                DialogResult dr = MessageBox.Show("There was an issue trying to determine if another Thetis instance is running.\nAre you sure you want to continue?\n\n" + ex.GetType().Name + ": " + ex.Message, "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, Common.MB_TOPMOST);
                if (dr == DialogResult.No) return false;
                return true;
            }
        }

        public static void Release()
        {
            if (_owns_mutex && _mutex != null)
            {
                try { _mutex.ReleaseMutex(); } catch { }
                _mutex.Dispose();
                _mutex = null;
                _owns_mutex = false;
            }
        }
    }
}