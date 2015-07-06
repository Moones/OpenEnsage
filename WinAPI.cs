using System;
using System.Runtime.InteropServices;

namespace Loader
{
    internal class WinAPI
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByClass(string lpClassName, IntPtr ZeroOnly);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByTitle(IntPtr ZeroOnly, string title);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, WinAPI.ShowWindowCommands nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(WinAPI.ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, WinAPI.AllocationType flAllocationType, WinAPI.MemoryProtection flProtect);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("LoaderDLL.dll")]
        public static extern IntPtr GetProcAddress2(IntPtr hModule, string procName);

        [DllImport("LoaderDLL.dll")]
        public static extern uint GetPidByName(string funcName);

        [DllImport("LoaderDLL.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool CloakEnsage(uint pid, uint pathAddr);

        [DllImport("LoaderDLL.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool LoadEnsage(uint pid, out uint ensageBase, out uint ensageSize);

        [DllImport("LoaderDLL.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool LoadEnsage2(uint pid, out uint ensageBase, out uint ensageSize);

        [DllImport("LoaderDLL.dll")]
        public static extern uint GetModuleBase(uint pid, string name);

        [DllImport("LoaderDLL.dll")]
        public static extern uint GetModuleSize(uint pid, string name);

        [DllImport("LoaderDLL.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool LoadSecurityMod(out uint secBase, out uint secSize, out uint steamBase, out uint steamHandle);

        [DllImport("LoaderDLL.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsSecurityModLoaded();

        [DllImport("LoaderDLL.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool EnableDebugPrivileges();

        [DllImport("LoaderDLL.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Experimental();

        [DllImport("LoaderDLL.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsSecurityModServiceLoaded();

        [DllImport("LoaderDLL.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsSecurityModSteamLoaded();

        [DllImport("LoaderDLL.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool LoadSecurityModService(out uint baseHandle, out uint size, IntPtr module, int moduleSize);

        [DllImport("LoaderDLL.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool LoadSecurityModSteam(out uint baseHandle, out uint size, IntPtr module, int moduleSize);

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 2035711U,
            Terminate = 1U,
            CreateThread = 2U,
            VMOperation = 8U,
            VMRead = 16U,
            VMWrite = 32U,
            DupHandle = 64U,
            SetInformation = 512U,
            QueryInformation = 1024U,
            Synchronize = 1048576U,
        }

        [Flags]
        public enum AllocationType
        {
            Commit = 4096,
            Reserve = 8192,
            Decommit = 16384,
            Release = 32768,
            Reset = 524288,
            Physical = 4194304,
            TopDown = 1048576,
            WriteWatch = 2097152,
            LargePages = 536870912,
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 16,
            ExecuteRead = 32,
            ExecuteReadWrite = 64,
            ExecuteWriteCopy = 128,
            NoAccess = 1,
            ReadOnly = 2,
            ReadWrite = 4,
            WriteCopy = 8,
            GuardModifierflag = 256,
            NoCacheModifierflag = 512,
            WriteCombineModifierflag = 1024,
        }

        public enum ShowWindowCommands
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11,
        }
    }
}
