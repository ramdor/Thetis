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
        private static Console _c;
        private static bool _update_on_property_change;

        private static bool _hide_bands;
        private static bool _hide_modes;
        private static bool _hide_filters;
        private static bool _hide_meters;
        private static bool _expand_spectrum_to_right;
        static LegacyItemController()
        {
            _c = null;
            _hide_bands = false;
            _hide_modes = false;
            _hide_filters = false;
            _hide_meters = false;
            _expand_spectrum_to_right = false;
            _update_on_property_change = false;
        }
        public static void Init(Console c)
        {
            _c = c;
            _update_on_property_change = true;
        }
        public static bool HideMeters
        {
            get
            {
                if (_c != null)
                {
                    return _hide_meters && _c.IsExpandedView;
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
                if(_c != null)
                {
                    return _hide_bands && _c.IsExpandedView;
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
                if (_c != null)
                {
                    return _hide_modes && _c.IsExpandedView;
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
                if (_c != null)
                {
                    return _hide_filters && _c.IsExpandedView;
                }
                return _hide_filters; 
            }
            set 
            { 
                _hide_filters = value;
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
        public static void Update()
        {
            if (_c == null) return;

            if(_c.IsCollapsedView && !_c.IsExpandedView) // check both, because, you never know
            {
                // make control groups visible
                _c.BandPanelVisible();
                _c.ModePanelVisible(true);
                _c.FilterPanelVisible(true);

                _c.ExtendPanelDisplaySize(false);
            }
            else if(_c.IsExpandedView && !_c.IsCollapsedView)
            {
                // set visible based on flags
                _c.BandPanelVisible();
                _c.ModePanelVisible(!_hide_modes);
                _c.FilterPanelVisible(!_hide_filters);

                if (_expand_spectrum_to_right && _hide_bands && _hide_filters && _hide_modes & _hide_meters)
                    _c.ExtendPanelDisplaySize(true);
                else
                    _c.ExtendPanelDisplaySize(false);
            }
        }
    }
}
