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
    public class DTmonitor : IDisposable
    {
        private IntPtr _Handle;

        private bool _BrightnessSupported;

        private short _MinimumBrightness;
        private short _CurrentBrightness;
        private short _MaximumBrightness;

        private String _Name;

        private String _Resolution;

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
            this._Handle = Handle;
            this._BrightnessSupported = BrightnessSupported;
            this._MinimumBrightness = MinimumBrightness;
            this._CurrentBrightness = CurrentBrightness;
            this._MaximumBrightness = MaximumBrightness;
            this._Name = Name;
            this._Resolution = Resolution;
        }

        public void Dispose()
        {
        }

        public void Dispose(bool disposing)
        {
        }

        public IntPtr Handle { get => _Handle; }
        public bool BrightnessSupported { get => _BrightnessSupported; }
        public short MinimumBrightness { get => _MinimumBrightness; }
        public short CurrentBrightness { get => _CurrentBrightness; }
        public short MaximumBrightness { get => _MaximumBrightness; }
        public String Name { get => _Name; }
        public String Resolution { get => _Resolution; }

        public int GetBrightness()
        {
            short min = 0, cur = 0, max = 0;

            int i = 0;
            bool result = false;
            int error = 0;

            while ((i < 3) && (!result))
            {
                result = NativeMethods.GetMonitorBrightness(this._Handle, ref min, ref cur, ref max);
                error = Marshal.GetLastWin32Error();

                System.Threading.Thread.Sleep(50);

                ++i;
            }

            if (result)
            {
                this._MinimumBrightness = min;
                this._CurrentBrightness = cur;
                this._MaximumBrightness = max;
            }
            else
            {
                this._BrightnessSupported = false;
                this._MinimumBrightness = -1;
                this._CurrentBrightness = -1;
                this._MaximumBrightness = -1;
            }

            return error;
        }

        public int SetBrightness(short val)
        {
            uint bright = (uint)val;

            int error = 0;

            if ((_MinimumBrightness <= bright) && (bright <= _MaximumBrightness))
            {
                int i = 0;
                bool result = false;

                while ((i < 10) && (!result) && (this.CurrentBrightness != val))
                {
                    result = NativeMethods.SetMonitorBrightness(this._Handle, bright);
                    error = Marshal.GetLastWin32Error();

                    Thread.Sleep(50);

                    GetBrightness();

                    Thread.Sleep(50);

                    ++i;
                }

                if (!result)
                {
                    this._BrightnessSupported = false;
                    this._MinimumBrightness = -1;
                    this._CurrentBrightness = -1;
                    this._MaximumBrightness = -1;
                }
            }
            else
            {
                error = 0x57;
            }

            System.Threading.Thread.Sleep(150);

            GetBrightness();

            return error;
        }

    }
    
    class DTmonitors
    {
        public List<DTmonitor> Monitors;
        private EnumMonitorsDelegate CallBackInstance;

        public DTmonitors()
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

                while((i <= 10) && (Monitors.Count == 0))
                {
                    result = NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, CallBackInstance, 0x0);
                    error = Marshal.GetLastWin32Error();

                    if (!result)
                    {
                        Thread.Sleep(100);
                    }
                }
                    
                if (!result)
                {
                    throw new Exception( String.Format("Call to EnumDisplayMonitors failed with code 0x{0}", error.ToString("X")));
                }
            }
        }

        public void EnumDisplayMonitorsCallback(
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
        }
    }
}
