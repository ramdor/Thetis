// ideas from https://www.dmcinfo.com/latest-thinking/blog/id/5578/sharing-enums-and-constants-between-a-c-and-c-project

#if WIN32 
    // removes public keyword
    #define public    
#else
    // C# code
    using System;
    namespace Thetis
    {
        public class Versions
        {
            //... C# Specific class code
#endif
        // c# & c

        // Version numbers always contain exactly three digits to the right of the decimal point.
        // For easy comparison with expected version values, the value returned by this function is
        //    version_number * 1000.  E.g., Version 1.899 will return 1899.

        public const int _CMASTER_VERSION = 1010;
        public const int _CMASTER_ASIO_VERSION = 1000;
        public const int _WDSP_VERSION = 1260; // from wdsp\version.c
        public const int _PORTAUDIO_VERSION = 1970;  // from pa_front.c

#if WIN32
        // c specific
#else
        // c# specific
        };
#endif

#if WIN32
    //C++ code
    #undef public

#else
    //C# code
    }    // end Thetis
#endif