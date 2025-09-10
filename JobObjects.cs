using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GeminiCLIAutoCommander
{
    internal sealed class KillOnCloseJob : IDisposable
    {
        private IntPtr _handle;
        private bool _disposed;

        public KillOnCloseJob()
        {
            _handle = CreateJobObject(IntPtr.Zero, null);
            if (_handle == IntPtr.Zero || _handle == new IntPtr(-1))
                throw new InvalidOperationException("CreateJobObject 失敗");
            var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
            info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
            int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr ptr = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr(info, ptr, false);
                if (!SetInformationJobObject(_handle, JobObjectInfoType.ExtendedLimitInformation, ptr, (uint)length))
                    throw new InvalidOperationException("SetInformationJobObject 失敗");
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        public void AddProcess(Process p)
        {
            if (!AssignProcessToJobObject(_handle, p.Handle))
                throw new InvalidOperationException("AssignProcessToJobObject 失敗");
        }

        public void Dispose()
        {
            if (_disposed) return; _disposed = true;
            if (_handle != IntPtr.Zero)
            {
                CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }
        }

        #region Native
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool CloseHandle(IntPtr hObject);
        private const int JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000;
        private enum JobObjectInfoType { ExtendedLimitInformation = 9 }
        [StructLayout(LayoutKind.Sequential)] private struct JOBOBJECT_BASIC_LIMIT_INFORMATION { public long PerProcessUserTimeLimit; public long PerJobUserTimeLimit; public int LimitFlags; public UIntPtr MinimumWorkingSetSize; public UIntPtr MaximumWorkingSetSize; public int ActiveProcessLimit; public long Affinity; public int PriorityClass; public int SchedulingClass; }
        [StructLayout(LayoutKind.Sequential)] private struct IO_COUNTERS { public ulong ReadOperationCount; public ulong WriteOperationCount; public ulong OtherOperationCount; public ulong ReadTransferCount; public ulong WriteTransferCount; public ulong OtherTransferCount; }
        [StructLayout(LayoutKind.Sequential)] private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION { public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation; public IO_COUNTERS IoInfo; public UIntPtr ProcessMemoryLimit; public UIntPtr JobMemoryLimit; public UIntPtr PeakProcessMemoryUsed; public UIntPtr PeakJobMemoryUsed; }
        #endregion
    }
}
