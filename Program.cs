using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ClickClack.Properties;

internal class CC
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private static readonly LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    private static Random rnd = new Random();
    private static List<CachedSound> downSounds = new List<CachedSound>();
    private static List<CachedSound> upSounds = new List<CachedSound>();

    private static AudioPlaybackEngine ape = new AudioPlaybackEngine(48000, 2);
    private static HashSet<int> keysDown = new HashSet<int>();

    public static void Main()
    {
        foreach (var s in new[]
        {
            Resources.d1,
            Resources.d2,
            Resources.d3,
            Resources.d4,
            Resources.d5,
            Resources.d6,
            Resources.d7,
            Resources.d8
        })
        {
            downSounds.Add(new CachedSound(s));
        }

        foreach (var s in new[]
        {
            Resources.u1,
            Resources.u2,
            Resources.u3,
            Resources.u4,
            Resources.u5,
            Resources.u6,
            Resources.u7,
            Resources.u8
        })
        {
            upSounds.Add(new CachedSound(s));
        }

        _hookID = SetHook(_proc);
        Application.Run();
        UnhookWindowsHookEx(_hookID);
    }


    private static IntPtr SetHook(LowLevelKeyboardProc proc)

    {
        using (var curProcess = Process.GetCurrentProcess())
        {
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
    }


    private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
    {
        if ((nCode >= 0))
        {
            var vkCode = Marshal.ReadInt32(lParam);

            if (wParam == (IntPtr) WM_KEYDOWN && !keysDown.Contains(vkCode))
            {
                var r = rnd.Next(downSounds.Count);
                var ss = downSounds[r];
                ape.PlaySound(ss);

                keysDown.Add(vkCode);
            }
            else if (wParam == (IntPtr) WM_KEYUP)
            {
                var r = rnd.Next(upSounds.Count);
                var ss = upSounds[r];
                ape.PlaySound(ss);

                keysDown.Remove(vkCode);
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }


    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);


    private delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);
}