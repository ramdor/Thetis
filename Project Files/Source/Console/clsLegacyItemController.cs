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
