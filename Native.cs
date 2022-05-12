//    DimTray/Native.cs Copyright (C) 2021 Bryan Rykowski
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
using System.Runtime.InteropServices;
using System.Drawing;

namespace DimTray
{
    public delegate bool EnumMonitorsDelegate(
        IntPtr hMonitor,
        IntPtr hdc,
        IntPtr rect,
        uint param
    );

    static  class NativeMethods
    {
        /*
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromPoint(
            Point pt,
            uint dwFlags
        );
        
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromWindow(
            IntPtr hwnd,
            uint dwFlags
        );
        */
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplayMonitors(
            IntPtr hdc,
            IntPtr lprcClip,
            [Out] EnumMonitorsDelegate lpfnEnum,
            uint dwData
        );
        /*
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorInfo(
            IntPtr hMonitor,
            [In, Out] NativeStructures.MONITORINFO lpmi
        );
        
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplayDevices(
            IntPtr lpDevice,
            uint iDevNum,
            ref NativeStructures.DISPLAY_DEVICE lpDisplayDevice,
            uint dwFlags
        );
        */
        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(
            IntPtr hMonitor,
            ref uint pdwNumberOfPhysicalMonitors
        );

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(
            IntPtr hMonitor,
            uint dwPhysicalMonitorArraySize,
            [Out] NativeStructures.PHYSICAL_MONITOR[] pPhysicalMonitorArray
        );
        /*
        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorCapabilities(
            IntPtr hMonitor,
            [Out] uint pdwMonitorCapabilities,
            [Out] uint pdwSupportColorTemperatures
        );
        */
        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMonitorBrightness(
            IntPtr hMonitor,
            ref short pdwMinimumBrightness,
            ref short pdwCurrentBrightness,
            ref short pdwMaximumBrightness
        );

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetMonitorBrightness(
            IntPtr hMonitor,
            uint dwNewBrightness
        );
    }

    static class NativeStructures
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            public uint cb;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public String DeviceName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public String DeviceString;

            public uint StateFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public String DeviceID;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public String DeviceKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;

            public bool Init()
            {
                cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFOEX));
                return true;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MONITORINFOEX
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public String szDevice;

            public bool Init()
            {
                cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFOEX));
                return true;
            }
        }
    }
}