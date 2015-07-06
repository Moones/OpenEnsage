// Decompiled with JetBrains decompiler
// Type: Loader.Program
// Assembly: Loader, Version=0.1.5611.35443, Culture=neutral, PublicKeyToken=null
// MVID: 767D8978-23D8-4AB7-BA8A-78DBFB5F0780
// Assembly location: E:\Downloads\ensage\Dumps\Loader_fix.exe

using Loader.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Loader
{
    internal static class Program
    {
        public static bool StartMainForm = false;
        public static bool StartLoginForm = false;
        public static int UserGroup = 2;
        public static string VersionString = string.Empty;
        public static byte[] SessionKey;
        public static string AccountName;

        public static bool IsContributor()
        {
            switch (Program.UserGroup)
            {
                case 5:
                case 6:
                case 7:
                case 9:
                case 10:
                    return true;
                default:
                    return false;
            }
        }

        [STAThread]
        private static void Main(string[] args)
        {
            bool createdNew;
            if (args.Length > 0)
            {
                do
                {
                    using (new Mutex(true, "EnsageMutex", out createdNew))
                        Thread.Sleep(125);
                }
                while (!createdNew);
                Thread.Sleep(125);
                if (args[0] == "-u")
                {
                    try
                    {
                        File.Delete("ELoader.exe");
                        File.Move("ELoader2.exe", "ELoader.exe");
                    }
                    catch
                    {
                    }
                    Process.Start("ELoader.exe", "-d");
                    return;
                }
                if (args[0] == "-d")
                    File.Delete("ELoader2.exe");
            }
            if (File.Exists("Ensage2.dll"))
            {
                try
                {
                    File.Delete("Ensage2.dll");
                }
                catch (Exception)
                {
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (new Mutex(true, "EnsageMutex", out createdNew))
            {
                if (!createdNew)
                {
                    IntPtr windowByTitle = WinAPI.FindWindowByTitle(IntPtr.Zero, "Ensage - Main");
                    if (!(windowByTitle != IntPtr.Zero))
                        return;
                    WinAPI.ShowWindow(windowByTitle, WinAPI.ShowWindowCommands.Restore);
                    WinAPI.SetForegroundWindow(windowByTitle);
                }
                else
                {
                    Program.AccountName = "EnsageUser" + new Random().Next(1, int.MaxValue);
                    Application.Run(new Updater());
                    if (Program.StartLoginForm)
                        Application.Run(new Login());
                    if (!Program.StartMainForm)
                        return;
                    Application.Run(new Main());
                }
            }
        }
    }
}
