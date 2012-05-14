using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System;

namespace SuperPutty.Classes
{
    abstract class WindowEventHandler : IDisposable
    {
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
           hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
           uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(HandleRef hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);

        WinEventDelegate procDelegate;

        protected IntPtr m_hook = IntPtr.Zero;
        protected frmSuperPutty m_form;

        public WindowEventHandler(frmSuperPutty form)
        {
            m_form = form;
        }

        protected void HookEvent(uint eventType)
        {
            procDelegate = new WinEventDelegate(WinEventProc);

            // Listen for foreground changes across all processes/threads on current desktop...
            m_hook = SetWinEventHook(eventType, eventType, IntPtr.Zero,
                    procDelegate, 0, 0, 0);
        }

        ~WindowEventHandler()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (this.m_hook != IntPtr.Zero)
            {
                UnhookWinEvent(m_hook);
            }
        }

        protected abstract void WinEventProc(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
    }
}