using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Input;

namespace SuperPutty.Classes
{
    // Modified heavily from http://www.pinvoke.net/default.aspx/user32.registerhotkey
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
        // public const int MOD_SHIFT = 4; // Not needed
        public const int MOD_WIN = 8;

        public const int WM_HOTKEY = 0x312;

        public enum Purpose
        {
            None, NewMinttyTab, Previous, Next, CloseTab,
            Tab1 = 100, Tab2, Tab3, Tab4, Tab5, Tab6, Tab7, Tab8, LastTab
        };

        private bool altDown = false;
        private bool ctrlDown = false;
        private bool winDown = false;

        private short hotkeyCount = 0;

        private struct HotkeySignature
        {
            Key key;
            int modifiers;

            public HotkeySignature(Key key, int modifiers)
            {
                this.key = key;
                this.modifiers = modifiers;
            }
        }

        /// <summary>The IDs for the hotkeys</summary>
        private Dictionary<HotkeySignature, Purpose> hotkeys;

        public GlobalHotkeys()
        {
            hotkeys = new Dictionary<HotkeySignature, Purpose>();
        }

        /// <summary>Register the hotkey</summary>
        public short RegisterGlobalHotkey(Key hotkey, int modifiers, Purpose purpose, IntPtr handle)
        {
            return RegisterGlobalHotkey(hotkey, modifiers, purpose);
        }

        /// <summary>Register the hotkey</summary>
        public short RegisterGlobalHotkey(Key hotkey, int modifiers, Purpose purpose)
        {
            try
            {
                HotkeySignature signature = new HotkeySignature(hotkey, modifiers);
                this.hotkeys.Add(signature, purpose);
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

        public void KeyDown(Key key)
        {
            keyTrigger(key, true);
        }

        public void KeyUp(Key key)
        {
            keyTrigger(key, false);
        }

        private void keyTrigger(Key key, bool value)
        {
            if (key == Key.LeftAlt || key == Key.RightAlt)
            {
                altDown = value;
            }

            if (key == Key.LeftCtrl || key == Key.RightCtrl)
            {
                ctrlDown = value;
            }

            if (key == Key.LWin || key == Key.RWin)
            {
                winDown = value;
            }
        }

        public Purpose GetHotkey(Key hotkeyId)
        {
            int modifier = 0;
            if (altDown)
            {
                modifier |= MOD_ALT;
            }

            if (ctrlDown)
            {
                modifier |= MOD_CONTROL;
            }

            if (winDown)
            {
                modifier |= MOD_WIN;
            }

            HotkeySignature signature = new HotkeySignature(hotkeyId, modifier);

            if (this.hotkeys.ContainsKey(signature))
            {
                return this.hotkeys[signature];
            }

            return Purpose.None;
        }

        /// <summary>Unregister the hotkey</summary>
        public void UnregisterGlobalHotkey()
        {
            foreach (var pair in this.hotkeys)
            {
                //UnregisterHotKey(this.Handle, pair.Key);
            }
        }

        public void Dispose()
        {
            UnregisterGlobalHotkey();
        }
    }
}
