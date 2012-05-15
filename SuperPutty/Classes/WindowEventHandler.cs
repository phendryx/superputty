using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System;

namespace SuperPutty.Classes
{
    abstract class WindowEventHandler : IDisposable
    {
        WinAPI.WinEventDelegate procDelegate;

        protected IntPtr m_hook = IntPtr.Zero;
        protected frmSuperPutty m_form;

        public WindowEventHandler(frmSuperPutty form)
        {
            m_form = form;
        }

        protected void HookEvent(uint eventType)
        {
            procDelegate = new WinAPI.WinEventDelegate(WinEventProc);

            // Listen for foreground changes across all processes/threads on current desktop...
            m_hook = WinAPI.SetWinEventHook(eventType, eventType, IntPtr.Zero,
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
                WinAPI.UnhookWinEvent(m_hook);
            }
        }

        protected abstract void WinEventProc(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
    }
}