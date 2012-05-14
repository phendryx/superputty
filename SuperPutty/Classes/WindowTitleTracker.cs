using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SuperPutty.Classes
{
    class WindowTitleTracker : IDisposable
    {
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
           hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
           uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetWindowTextLength(HandleRef hWnd);

        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);
 
        // Constants from winuser.h
        const uint EVENT_OBJECT_NAMECHANGE = 0x800C;
        const uint EVENT_SYSTEM_FOREGROUND = 3;
        const uint WINEVENT_OUTOFCONTEXT = 0;

        WinEventDelegate procDelegate;

        private IntPtr m_hook;
        private frmSuperPutty m_form;

        public WindowTitleTracker(frmSuperPutty form)
        {
            m_form = form;
            procDelegate = new WinEventDelegate(WinEventProc);

            // Listen for foreground changes across all processes/threads on current desktop...
            m_hook = SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE, IntPtr.Zero,
                    procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
        }

        ~WindowTitleTracker()
        {
            Dispose();
        }

        public void Dispose()
        {
            UnhookWinEvent(m_hook);
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd != this.m_form.Handle && this.m_form.ContainsChild(hwnd))
            {
                int capacity = GetWindowTextLength(new HandleRef(this, hwnd)) * 2;
                StringBuilder stringBuilder = new StringBuilder(capacity);
                GetWindowText(new HandleRef(this, hwnd), stringBuilder, stringBuilder.Capacity);
                this.m_form.SetPanelTitle(hwnd, stringBuilder.ToString());
            }
        }
    }
}
