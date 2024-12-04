/*  MeterManager.cs

This file is part of a program that implements a Software-Defined Radio.

This code/file can be found on GitHub : https://github.com/ramdor/Thetis

Copyright (C) 2020-2024 Richard Samphire MW0LGE

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thetis
{
    internal static class LegacyItemController
    {
        private static Console _console;
        private static bool _update_on_property_change;

        private static bool _hide_bands;
        private static bool _hide_modes;
        private static bool _hide_filters;
        private static bool _hide_meters;
        private static bool _expand_spectrum_to_right;
        private static bool _hide_vfoA;
        private static bool _hide_vfoB;
        private static bool _hide_vfo_sync;
        private static bool _expand_spectrum_to_top;
        static LegacyItemController()
        {
            _console = null;
            _hide_bands = false;
            _hide_modes = false;
            _hide_filters = false;
            _hide_meters = false;
            _expand_spectrum_to_right = false;
            _update_on_property_change = false;
            _hide_vfoA = false;
            _hide_vfoB = false;
            _hide_vfo_sync = false;
            _expand_spectrum_to_right = false;
    }
        public static void Init(Console c)
        {
            _console = c;
            _update_on_property_change = true;
        }
        public static bool HideMeters
        {
            get
            {
                if (_console != null)
                {
                    return _hide_meters && _console.IsExpandedView;
                }
                return _hide_meters;
            }
            set
            {
                _hide_meters = value;
                if (_update_on_property_change) Update();
            }
        }
        public static bool HideBands
        {
            get 
            { 
                if(_console != null)
                {
                    return _hide_bands && _console.IsExpandedView;
                }
                return _hide_bands; 
            }
            set 
            { 
                _hide_bands = value;
                if(_update_on_property_change) Update();
            }
        }
        public static bool HideModes
        {
            get 
            {
                if (_console != null)
                {
                    return _hide_modes && _console.IsExpandedView;
                }
                return _hide_modes; 
            }
            set 
            {
                _hide_modes = value;
                if (_update_on_property_change) Update();
            }
        }
        public static bool HideFilters
        {
            get 
            {
                if (_console != null)
                {
                    return _hide_filters && _console.IsExpandedView;
                }
                return _hide_filters; 
            }
            set 
            { 
                _hide_filters = value;
                if (_update_on_property_change) Update();
            }
        }
        public static bool HideVFOA
        {
            get
            {
                return _hide_vfoA;
            }
            set
            {
                _hide_vfoA = value;
                if (_update_on_property_change) Update();
            }
        }
        public static bool HideVFOB
        {
            get
            {
                return _hide_vfoB;
            }
            set
            {
                _hide_vfoB = value;
                if (_update_on_property_change) Update();
            }
        }
        public static bool HideVFOSync
        {
            get
            {
                return _hide_vfo_sync;
            }
            set
            {
                _hide_vfo_sync = value;
                if (_update_on_property_change) Update();
            }
        }
        public static bool ExpandSpectrumToRight
        {
            get { return _expand_spectrum_to_right && _hide_bands && _hide_filters && _hide_modes & _hide_meters; }
            set 
            { 
                _expand_spectrum_to_right = value;
                if (_update_on_property_change) Update();
            }
        }
        public static bool ExpandSpectrumToTop
        {
            get { return _expand_spectrum_to_top && _hide_vfoA && _hide_vfoB && _hide_vfo_sync & _hide_meters; }
            set
            {
                _expand_spectrum_to_top = value;
                if (_update_on_property_change) Update();
            }
        }
        public static void Update()
        {
            if (_console == null) return;
            if (_console.IsSetupFormNull) return;

            if (_console.IsCollapsedView && !_console.IsExpandedView) // check both, because, you never know
            {
                // make control groups visible
                _console.BandPanelVisible(!_console.SetupForm.chkShowBandControls.Checked || _console.SetupForm.chkShowAndromedaBar.Checked); // if we need to hide band controls in collapsed view, then force it with the optional parameter
                _console.ModePanelVisible(_console.SetupForm.chkShowModeControls.Checked && !_console.SetupForm.chkShowAndromedaBar.Checked); // if we need to hide mode controls in collapsed view, then force it with the optional parameter
                                                                                            // Note: I dislike the use of a public control, but that is how things have been done with this
                _console.FilterPanelVisible(false);

                _console.ExtendPanelDisplaySizeRight(false);

                _console.VFOAVisible(_console.ShowRX1 || _console.ShowAndromedaTopControls);
                _console.VFOBVisible(_console.ShowRX2 || _console.ShowAndromedaTopControls);
            }
            else if(_console.IsExpandedView && !_console.IsCollapsedView)
            {
                // set visible based on flags
                _console.BandPanelVisible();
                _console.ModePanelVisible(!_hide_modes);
                _console.FilterPanelVisible(!_hide_filters);

                if (_expand_spectrum_to_right && _hide_bands && _hide_filters && _hide_modes & _hide_meters)
                    _console.ExtendPanelDisplaySizeRight(true);
                else
                    _console.ExtendPanelDisplaySizeRight(false);

                if (_expand_spectrum_to_top && _hide_vfoA && _hide_vfoB && _hide_vfo_sync & _hide_meters)
                    _console.ExtendPanelDisplaySizeTop(true);
                else
                    _console.ExtendPanelDisplaySizeTop(false);

                _console.VFOAVisible(!_hide_vfoA);
                _console.VFOBVisible(!_hide_vfoB);
                _console.VFOSyncVisible(!_hide_vfo_sync);
            }
        }
    }
}
