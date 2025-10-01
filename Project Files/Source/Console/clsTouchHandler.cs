/*  clsTouchHandler.cs

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


//-----------------------
//Note, this will add touch handling to a windows control/form for mouse down/up/move. It will prevent mouse
//interaction for 250ms after a touch event.
//-----------------------

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Thetis
{
    internal static class TouchHandler
    {
        private class ControlTouchInfo
        {
            public IntPtr handler;
            public Action<int, int> TouchDown;
            public Action<int, int> TouchMove;
            public Action<int, int> TouchUp;
            public int touch_mask;
        }

        private const int WM_TOUCH = 0x0240;
        public const int TOUCHEVENTF_UP = 0x0004;
        public const int TOUCHEVENTF_DOWN = 0x0002;
        public const int TOUCHEVENTF_MOVE = 0x0001;
        //private const uint TWF_WANTPALM = 0x00000002;

        private const uint MW_MOUSEFIRST = 0x0200;
        private const uint MW_MOUSELAST = 0x020D;

        private static DateTime _last_touch_time = DateTime.MinValue;
        private static bool _is_touch_active = false;

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private static readonly Dictionary<Guid, ControlTouchInfo> controlInfoMap = new Dictionary<Guid, ControlTouchInfo>();
        private static readonly Dictionary<IntPtr, IntPtr> originalWndProcs = new Dictionary<IntPtr, IntPtr>();
        private static readonly WndProcDelegate customWndProcDelegate = CustomWndProc;

        private static readonly object _locker = new object();

        [StructLayout(LayoutKind.Sequential)]
        private struct TOUCHINPUT
        {
            public int x;
            public int y;
            public IntPtr hSource;
            public int dwID;
            public int dwFlags;
            public int dwMask;
            public int dwTime;
            public IntPtr dwExtraInfo;
            public int cxContact;
            public int cyContact;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetTouchInputInfo(IntPtr hTouchInput, int cInputs, [Out] TOUCHINPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern void CloseTouchInputHandle(IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool RegisterTouchWindow(IntPtr hWnd, uint ulFlags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterTouchWindow(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        public static Guid EnableTouchSupport(Control control, Action<int, int> touchDown, Action<int, int> touchMove, Action<int, int> touchUp, int touch_mask, string id = "")
        {
            lock (_locker)
            {
                IntPtr controlHandle = control.Handle;

                if (!originalWndProcs.ContainsKey(controlHandle))
                {
                    IntPtr customWndProc = Marshal.GetFunctionPointerForDelegate(customWndProcDelegate);
                    IntPtr originalWndProc = SetWindowLongPtr(controlHandle, -4, customWndProc);
                    originalWndProcs[controlHandle] = originalWndProc;

                    RegisterTouchWindow(controlHandle, 0);// TWF_WANTPALM);
                }

                Guid guid;
                if (string.IsNullOrEmpty(id))
                {
                    guid = Guid.NewGuid();
                }
                else
                {
                    Guid.TryParse(id, out guid);
                }

                controlInfoMap[guid] = new ControlTouchInfo
                {
                    handler = controlHandle,
                    TouchDown = touchDown,
                    TouchMove = touchMove,
                    TouchUp = touchUp,
                    touch_mask = touch_mask
                };

                return guid;
            }
        }

        public static void DisableTouchSupport(Guid id)
        {
            lock (_locker)
            {
                if (controlInfoMap.TryGetValue(id, out ControlTouchInfo touchInfo))
                {
                    IntPtr controlHandle = touchInfo.handler;
                    if (originalWndProcs.ContainsKey(controlHandle))
                    {
                        SetWindowLongPtr(controlHandle, -4, originalWndProcs[controlHandle]);
                        originalWndProcs.Remove(controlHandle);
                    }
                    UnregisterTouchWindow(controlHandle);
                    controlInfoMap.Remove(id);
                }
            }
        }

        private static IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_TOUCH)
            {
                bool got_touch_info = false;
                int inputCount = wParam.ToInt32() & 0xFFFF;
                TOUCHINPUT[] inputs = new TOUCHINPUT[inputCount];

                if (GetTouchInputInfo(lParam, inputCount, inputs, Marshal.SizeOf(typeof(TOUCHINPUT))))
                {
                    got_touch_info = true;
                }
                CloseTouchInputHandle(lParam);

                bool handled = false;
                if (got_touch_info) 
                {
                    lock (_locker)
                    {
                        foreach (KeyValuePair<Guid, ControlTouchInfo> pair in controlInfoMap)
                        {
                            if (pair.Value.handler == hWnd)
                            {
                                ControlTouchInfo touchInfo = pair.Value;

                                for (int i = 0; i < inputs.Length; i++)
                                {
                                    POINT pt = new POINT { x = inputs[i].x / 100, y = inputs[i].y / 100 };
                                    ScreenToClient(hWnd, ref pt);
                                    int x = pt.x;
                                    int y = pt.y;
                                    if (((touchInfo.touch_mask & TOUCHEVENTF_DOWN) != 0) && (inputs[i].dwFlags & TOUCHEVENTF_DOWN) != 0 && touchInfo.TouchDown != null)
                                    {
                                        touchInfo.TouchDown(x, y);
                                        handled = true;
                                    }
                                    else if (((touchInfo.touch_mask & TOUCHEVENTF_MOVE) != 0) && (inputs[i].dwFlags & TOUCHEVENTF_MOVE) != 0 && touchInfo.TouchMove != null)
                                    {
                                        touchInfo.TouchMove(x, y);
                                        handled = true;
                                    }
                                    else if (((touchInfo.touch_mask & TOUCHEVENTF_UP) != 0) && (inputs[i].dwFlags & TOUCHEVENTF_UP) != 0 && touchInfo.TouchUp != null)
                                    {
                                        touchInfo.TouchUp(x, y);
                                        handled = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if (handled)
                {
                    _is_touch_active = true;
                    _last_touch_time = DateTime.UtcNow;
                    return new IntPtr(1); // signify handled
                }
            }
            else if (msg >= MW_MOUSEFIRST && msg <= MW_MOUSELAST)
            {
                lock (_locker)
                {
                    // ignore any mouse events that occur within 250ms of a touch event
                    if (_is_touch_active && (DateTime.UtcNow - _last_touch_time).TotalMilliseconds <= 250)
                    {
                        return IntPtr.Zero;
                    }
                    else _is_touch_active = false;
                }
            }

            if (originalWndProcs.ContainsKey(hWnd))
            {
                lock (_locker)
                {
                    return CallWindowProc(originalWndProcs[hWnd], hWnd, msg, wParam, lParam);
                }
            }

            return IntPtr.Zero;
        }
    }
}