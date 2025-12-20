/*  ivac.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2000-2025 Original authors
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

namespace Thetis
{
    using System;
    using System.Runtime.InteropServices;

    unsafe class ivac
    {
        #region ivac method definitions

        // vac 

        [DllImport("ChannelMaster.dll", EntryPoint = "StartAudioIVAC", CallingConvention = CallingConvention.Cdecl)] // StartAudioVAC()
        public static extern int StartAudioIVAC(int id);

        [DllImport("ChannelMaster.dll", EntryPoint = "StopAudioIVAC", CallingConvention = CallingConvention.Cdecl)] // StopAudioVAC()
        public static extern void StopAudioIVAC(int id);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACstereo", CallingConvention = CallingConvention.Cdecl)] // SetVACStereo(int enable)
        public static extern void SetIVACstereo(int id, int stereo);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACrun", CallingConvention = CallingConvention.Cdecl)] // SetVACEnabled(int enabled)
        public static extern void SetIVACrun(int id, int run);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACiqType", CallingConvention = CallingConvention.Cdecl)] // SetVACOutputIQ(int enabled)
        public static extern void SetIVACiqType(int id, int type);

        // [DllImport("wdsp.dll", EntryPoint = "SetIVACmicRate", CallingConvention = CallingConvention.Cdecl)] // SetBlockSize(int size)
        // public static extern void SetIVACmicRate(int id, int rate);

        // [DllImport("wdsp.dll", EntryPoint = "SetIVACaudioRate", CallingConvention = CallingConvention.Cdecl)]
        // public static extern void SetIVACaudioRate(int id, int rate);

        // [DllImport("wdsp.dll", EntryPoint = "SetIVACtxmonRate", CallingConvention = CallingConvention.Cdecl)]
        // public static extern void SetIVACtxmonRate(int id, int rate);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACvacRate", CallingConvention = CallingConvention.Cdecl)] // SetBlockSizeVAC(int size)
        public static extern void SetIVACvacRate(int id, int rate);

        // [DllImport("wdsp.dll", EntryPoint = "SetIVACmicSize", CallingConvention = CallingConvention.Cdecl)] // SetSampleRate1(int size)
        // public static extern void SetIVACmicSize(int id, int size);

        // [DllImport("wdsp.dll", EntryPoint = "SetIVACiqSize", CallingConvention = CallingConvention.Cdecl)] // SetSampleRate1(int size)
        // public static extern void SetIVACiqSize(int id, int size);

        // [DllImport("wdsp.dll", EntryPoint = "SetIVACaudioSize", CallingConvention = CallingConvention.Cdecl)]
        // public static extern void SetIVACaudioSize(int id, int size);

        // [DllImport("wdsp.dll", EntryPoint = "SetIVACtxmonSize", CallingConvention = CallingConvention.Cdecl)]
        // public static extern void SetIVACtxmonSize(int id, int size);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACvacSize", CallingConvention = CallingConvention.Cdecl)] // SetSampleRate2(int size)
        public static extern void SetIVACvacSize(int id, int size);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVAChostAPIindex", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVAChostAPIindex(int id, int index);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACinputDEVindex", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACinputDEVindex(int id, int index);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACoutputDEVindex", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACoutputDEVindex(int id, int index);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACnumChannels", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACnumChannels(int id, int n);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACInLatency", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACInLatency(int id, double lat, int reset);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACOutLatency", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACOutLatency(int id, double lat, int reset);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACPAInLatency", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACPAInLatency(int id, double lat, int reset);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACPAOutLatency", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACPAOutLatency(int id, double lat, int reset);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACpreamp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACpreamp(int id, double preamp);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACbypass", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACbypass(int id, int enabled);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACRBReset", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACRBReset(int id, int enabled);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACvox", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACvox(int id, int enabled);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACrxscale", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACrxscale(int id, double scale);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACcombine", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACcombine(int id, int combine);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACmon", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACmon(int id, int mon);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACmonVol", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACmonVol(int id, double vol);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACmox", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACmox(int id, int mox);

        [DllImport("ChannelMaster.dll", EntryPoint = "getIVACdiags", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getIVACdiags(int id, int type, int* underflows, int* overflows, double* var, int* ringsize, int* nring);

        [DllImport("ChannelMaster.dll", EntryPoint = "forceIVACvar", CallingConvention = CallingConvention.Cdecl)]
        public static extern void forceIVACvar(int id, int type, bool force, double fvar);

        [DllImport("ChannelMaster.dll", EntryPoint = "resetIVACdiags", CallingConvention = CallingConvention.Cdecl)]
        public static extern void resetIVACdiags(int id, int type);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACFeedbackGain", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACFeedbackGain(int id, int type, double feedback_gain);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACSlewTime", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACSlewTime(int id, int type, double slew_time);

        //
        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACPropRingMin", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACPropRingMin(int id, int type, int prop_min);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACPropRingMax", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACPropRingMax(int id, int type, int prop_max);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACFFRingMin", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACFFRingMin(int id, int type, int ff_ringmin);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACFFRingMax", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACFFRingMax(int id, int type, int ff_ringmax);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACFFAlpha", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACFFAlpha(int id, int type, double ff_alpha);
        //[DllImport("ChannelMaster.dll", EntryPoint = "SetIVACvar", CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetIVACvar(int id, int type, double var);

        [DllImport("ChannelMaster.dll", EntryPoint = "GetIVACControlFlag", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetIVACControlFlag(int id, int type, int* control_flag);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACinitialVars", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACinitialVars(int id, double INvar, double OUTvar);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACswapIQout", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACswapIQout(int id, int swap);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACExclusiveOut", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACExclusiveOut(int id, int exclusive_out);

        [DllImport("ChannelMaster.dll", EntryPoint = "SetIVACExclusiveIn", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetIVACExclusiveIn(int id, int exclusive_in);
        //
        #endregion

    }
}
