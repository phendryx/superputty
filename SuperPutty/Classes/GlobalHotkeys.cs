using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SuperPutty.Classes
{
    // Modified from http://www.pinvoke.net/default.aspx/user32.registerhotkey
    /// <summary> This class allows you to manage a hotkey </summary>
    public class GlobalHotkeys : IDisposable
    {
        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32", SetLastError = true)]
        public static extern int UnregisterHotKey(IntPtr hwnd, int id);

        public const int MOD_ALT = 1;
        public const int MOD_CONTROL = 2;
        public const int MOD_SHIFT = 4;
        public const int MOD_WIN = 8;

        public const int WM_HOTKEY = 0x312;

        public enum Purpose
        {
            None, NewMinttyTab, Previous, Next, CloseTab
        };

        private short hotkeyCount = 0;

        /// <summary>The IDs for the hotkeys</summary>
        private Dictionary<short, Purpose> hotkeys;

        public GlobalHotkeys(IntPtr handle)
        {
            //this.Handle = Process.GetCurrentProcess().Handle;
            this.Handle = handle;
            hotkeys = new Dictionary<short, Purpose>();
        }

        /// <summary>Handle of the current process</summary>
        public IntPtr Handle;

        /// <summary>Register the hotkey</summary>
        public short RegisterGlobalHotKey(int hotkey, int modifiers, Purpose purpose, IntPtr handle)
        {
            this.Handle = handle;
            return RegisterGlobalHotKey(hotkey, modifiers, purpose);
        }

        /// <summary>Register the hotkey</summary>
        public short RegisterGlobalHotKey(int hotkey, int modifiers, Purpose purpose)
        {
            try
            {
                // register the hotkey, throw if any error
                if (!RegisterHotKey(this.Handle, hotkeyCount, (uint)modifiers, (uint)hotkey))
                    throw new Exception("Unable to register hotkey. Error: " + Marshal.GetLastWin32Error().ToString());

                this.hotkeys.Add(hotkeyCount, purpose);
                return hotkeyCount++;
            }
            catch (Exception ex)
            {
                // clean up if hotkey registration failed
                Dispose();
                Console.WriteLine(ex);
            }

            return -1;
        }

        public bool HasHotKey(short hotkeyId)
        {
            return this.hotkeys.ContainsKey(hotkeyId);
        }

        public Purpose GetHotKeyPurpose(short hotkeyId)
        {
            if (this.hotkeys.ContainsKey(hotkeyId))
            {
                return this.hotkeys[hotkeyId];
            }

            return Purpose.None;
        }

        /// <summary>Unregister the hotkey</summary>
        public void UnregisterGlobalHotKey()
        {
            foreach (var pair in this.hotkeys)
            {
                UnregisterHotKey(this.Handle, pair.Key);
            }
        }

        public void Dispose()
        {
            UnregisterGlobalHotKey();
        }
    }
}
