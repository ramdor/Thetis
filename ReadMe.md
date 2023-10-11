# Latest Release v2.10.2 October, 11th 2023

# 2.10.2 (2023-11-10)

# 2.10.0 (2023-19-06)

# 2.9.0 (2022-03-04)
See [ Thetis Change Log ](https://github.com/TAPR/OpenHPSDR-Thetis/blob/master/Thetis%20v2.9.0%20Change%20Log.pdf) for more details.

# 2.8.11 (2020-20-10)
See [ Thetis Change Log ](https://github.com/TAPR/OpenHPSDR-Thetis/blob/master/Thetis%20v2.8.11%20Change%20Log.pdf) for more details.

# 2.8.9 (2020-13-10)
See [ Thetis Change Log ](https://github.com/TAPR/OpenHPSDR-Thetis/blob/master/Thetis%20v2.8.9%20Change%20Log.pdf) for more details.

# 2.8.8 (2020-10-10)
See [ Thetis Change Log ](https://github.com/TAPR/OpenHPSDR-Thetis/blob/master/Thetis%20v2.8.8%20Change%20Log.pdf) for more details.

# 2.8.7 (2020-10-7)
See [ Thetis Change Log ](https://github.com/TAPR/OpenHPSDR-Thetis/blob/master/Thetis%20v2.8.7%20Change%20Log.pdf) for more details.

# 2.8.6 (2020-10-6)
See [ Thetis Change Log ](https://github.com/TAPR/OpenHPSDR-Thetis/blob/master/Thetis%20v2.8.6%20Change%20Log.pdf) for more details.

# 2.7.0 Not Officially Released

# 2.6.9 (2020-1-24)
See [ Thetis Change Log ](https://github.com/TAPR/OpenHPSDR-Thetis/blob/master/Thetis%20v2.6.9%20Change%20Log.pdf) for more details.

# 2.6.8 (2019-11-3)
See [ Thetis Change Log ](https://github.com/TAPR/OpenHPSDR-Thetis/blob/master/Thetis%20v2.6.8%20Change%20Log.pdf) for more details.

# 2.6.7 (2019-4-29)
- fixed bug where the VOX/DEXP LookAhead feature was enabled when VOX/DEXP was not.
- corrected compatiblity issue with the ANAN-10E. This requires new firmare to be flashed. v10.3
- corrected the Spectrum and Histogram diplay during transmit

# 2.6.6 (2019-4-21)
- corrects issue with EU region using commas
- corrects issue with having out of band frequency on startup
- fixed transmit filter not being displayed when using split

# 2.6.5 (2019-4-18)
- corrected issue with console remaining open after exiting Thetis
- fixed problem of program crashing when recording while transmitting
- fixed problem with program crashing when receiving a bad packet

# 2.6.4 (2019-4-13)
- improved VOX/DEXP features and performance
- QSK cabibility for the ANAN-200D, 7000DLE, and 8000DLE models
- fixed VAC1 startup problem
- fixed VAC2 resampler problem
- added option to use VAC2 on split
- improved TX-RX and RX-TX transistion on voice modes
- transverter T/R relay bug fixed
- added control for BYPS-EXT1-XVTR RX ANT for 7000DLE

  * see "Release Notes for 2-6-4.docx" for detailed information.

# 2.6.0 (2018-4-10)
- added diagnostic LED array
- divided open collector controls into 3 groups (HF-VHF-SWL)
- bug fix for step tune using MIDI

# 2.5.9 (2018-3-29)
- changed "MDECAY" constant to 0.99 in netInterface.c
- added 2Hz step tune choice
- corrected duplicate db import dialogs
- modified behavior of sequence errors so that sequence errors are ignored for seq 0
- changes to VAC includes tooltips for various controls, fix for the Output Ringbuffer latency Monitor control not working, and added the ability to reset the diagnostics
- forced BPF1 into ByPass during transmit if PureSignal is enabled for Orion MkII boards only

# 2.5.8 (2018-3-25)
- changed "MDECAY" constant to 0.9 in netInterface.c
- fixes for VFO A&B Lock 
- NB/NB2 is turned OFF while transmitting when DUP is enabled
- Added 2kHz Tune Step
- Changed ANF behavior so that it is disabled when in CW mode
- Removed the 750Hz CW filter and added a 150Hz CW filter
- Increased display buffer to support larger than 4k displays

# 2.5.7 (2018-3-25)
- spectrum roll-off adjusted to clip 4%
- calls to PeakFwdPower(…) and PeakRevPower(…) moved from netInterface.c to network.c
- skin graphics added for chkRxAnt and chkVFOBLock controls

# 2.5.6 (2018-3-25)
- added MIDI/CAT updates
- added independent VFO Locks

# 2.5.5 (2018-3-24)
- added support for ANAN-7000DLE
- added 'Rx Ant' support

# 2.5.4 (2018-3-22)
- added Audio Adaptive Variable Resampler with monitor tools
