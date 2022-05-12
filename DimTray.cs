//    DimTray/DimTray.cs Copyright (C) 2021 Bryan Rykowski
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Linq;
using System.Threading;
using System.Net.Http.Headers;

namespace DimTray
{
    class DTmonitor : IDisposable
    {
        public bool Fake { get; private set; }
        public IntPtr Handle { get; private set; }
        public bool BrightnessSupported { get; private set; }
        public short MinimumBrightness { get; private set; }
        public short CurrentBrightness { get; private set; }
        public short MaximumBrightness { get; private set; }
        public String Name { get; private set; }
        public String Resolution { get; private set; }

        public DTmonitor(
            IntPtr Handle,
            bool BrightnessSupported,
            short MinimumBrightness,
            short CurrentBrightness,
            short MaximumBrightness,
            String Name,
            String Resolution
        )
        {
            Fake = false;
            this.Handle = Handle;
            this.BrightnessSupported = BrightnessSupported;
            this.MinimumBrightness = MinimumBrightness;
            this.CurrentBrightness = CurrentBrightness;
            this.MaximumBrightness = MaximumBrightness;
            this.Name = Name;
            this.Resolution = Resolution;
        }

        // For making a fake monitor (for UI testing).
        public DTmonitor()
        {
            Fake = true;
            Handle = IntPtr.Zero;
            BrightnessSupported = false;
            MinimumBrightness = 0;
            CurrentBrightness = 50;
            MaximumBrightness = 100;
            Name = "fake";
            Resolution = "0 x 0";
        }

        public void Dispose()
        {
        }

        public void Dispose(bool disposing)
        {
        }


        public int GetBrightness()
        {
            short min = -1, cur = -1, max = -1;

            int i = 0;
            bool result = false;
            int error = 0;

            if (!Fake)
            {
                while ((i < 3) && (!result))
                {
                    result = NativeMethods.GetMonitorBrightness(this.Handle, ref min, ref cur, ref max);
                    error = Marshal.GetLastWin32Error();

                    System.Threading.Thread.Sleep(50);

                    ++i;
                }

                if (result)
                {
                    this.MinimumBrightness = min;
                    this.CurrentBrightness = cur;
                    this.MaximumBrightness = max;
                }
                else
                {
                    this.BrightnessSupported = false;
                }
            }


            return error;
        }

        public int SetBrightness(short val)
        {
            uint bright = (uint)val;

            int error = 0;

            if ((!Fake) && (BrightnessSupported) && (MinimumBrightness <= bright) && (bright <= MaximumBrightness))
            {
                int i = 0;
                bool result = false;

                while ((i < 10) && (!result) && (this.CurrentBrightness != val))
                {
                    result = NativeMethods.SetMonitorBrightness(this.Handle, bright);
                    error = Marshal.GetLastWin32Error();

                    Thread.Sleep(50);

                    GetBrightness();

                    Thread.Sleep(50);

                    ++i;
                }

                if (!result)
                {
                    this.BrightnessSupported = false;
                    this.MinimumBrightness = -1;
                    this.CurrentBrightness = -1;
                    this.MaximumBrightness = -1;
                }
            }
            else
            {
                error = 0x57;
            }

            System.Threading.Thread.Sleep(100);

            GetBrightness();

            return error;
        }

    }
    
    class MonitorManager
    {
        public List<DTmonitor> Monitors;
        private EnumMonitorsDelegate CallBackInstance;

        public MonitorManager()
        {
            Monitors = new List<DTmonitor>();
            CallBackInstance = new EnumMonitorsDelegate(EnumDisplayMonitorsCallback);
        }

        public void getDTmonitors()
        {
            foreach(DTmonitor item in Monitors)
            {
                item.Dispose();
            }

            Monitors.Clear();

            {
                int i = 0;
                bool result = false;
                int error = 0;

                while((i < 10) && (Monitors.Count == 0))
                {
                    result = NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, CallBackInstance, 0x0);
                    error = Marshal.GetLastWin32Error();

                    if (!result)
                    {
                        Thread.Sleep(100);
                    }
                    ++i;
                }
                
                if (!result)
                {
                    throw new Exception( String.Format("Call to EnumDisplayMonitors failed with code 0x{0}", error.ToString("X")));
                }
            }
        }

        // Populate the Monitors list with fake monitors (for UI testing).
        // Takes the number of fake monitors to create as an argument (limited to 16).
        public void getFakeDTmonitors(int count)
        {
            foreach (DTmonitor item in Monitors)
            {
                item.Dispose();
            }

            Monitors.Clear();

            if ((count > 0) && (count < 17))
            {
                int i = 1;
                while (i <= count)
                {
                    Monitors.Add(new DTmonitor());
                    ++i;
                }
            }
        }

        public bool EnumDisplayMonitorsCallback(
            IntPtr hMonitor,
            IntPtr hdc,
            IntPtr rect,
            uint param
        )
        {
            NativeStructures.RECT rectStruct = Marshal.PtrToStructure<NativeStructures.RECT>(rect);

            int height = rectStruct.bottom - rectStruct.top;
            int width = rectStruct.right - rectStruct.left;

            String resolution = width.ToString() + "x" + height.ToString();

            uint physicalMonitorCount = 0;

            {
                int i = 0;
                bool result = false;
                int error = 0;

                while ((i < 10) && (!result))
                {
                    result = NativeMethods.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref physicalMonitorCount);
                    error = Marshal.GetLastWin32Error();

                    Thread.Sleep(100);

                    ++i;
                }

                if (!result)
                {
                    throw new Exception( String.Format("Call to GetNumberOfPhysicalMonitorsFromHMONITOR failed with code 0x{0}", error.ToString("X")));
                }
            }

            NativeStructures.PHYSICAL_MONITOR[] physicalMonitors = new NativeStructures.PHYSICAL_MONITOR[physicalMonitorCount];

            {
                int i = 0;
                bool result = false;
                int error = 0;

                while ((i < 10) && (!result))
                {
                    result = NativeMethods.GetPhysicalMonitorsFromHMONITOR(hMonitor, physicalMonitorCount, physicalMonitors);
                    error = Marshal.GetLastWin32Error();

                    Thread.Sleep(50);

                    ++i;
                }

                if (!result)
                {
                    throw new Exception( String.Format("Call to GetPhysicalMonitorsFromHMONITOR failed with code 0x{0}", error.ToString("X")));
                }
            }

            foreach (var pm in physicalMonitors)
            {
                DTmonitor mon;

                IntPtr pmHandle = pm.hPhysicalMonitor;

                short min = 0, cur = 0, max = 0;

                String name = pm.szPhysicalMonitorDescription;

                {
                    int i = 0;
                    bool result = false;
                    int error = 0;

                    while((i < 3) && (!result))
                    {
                        result = NativeMethods.GetMonitorBrightness(pmHandle, ref min, ref cur, ref max);
                        error = Marshal.GetLastWin32Error();

                        Thread.Sleep(50);

                        ++i;
                    }

                    if (result)
                    {
                        mon = new DTmonitor(pmHandle, true, min, cur, max, name, resolution);
                    }
                    else
                    {
                        mon = new DTmonitor(pmHandle, false, -1, -1, -1, name, resolution);
                    }
                }

                Monitors.Add(mon);

            }
            return true;
        }
    }
}
