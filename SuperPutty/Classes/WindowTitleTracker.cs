using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SuperPutty.Classes
{
    class WindowTitleTracker : WindowEventHandler
    {
        // Constants from winuser.h
        const uint EVENT_OBJECT_NAMECHANGE = 0x800C;

        public WindowTitleTracker(frmSuperPutty form) : base(form)
        {
            HookEvent(EVENT_OBJECT_NAMECHANGE);
        }

        protected override void WinEventProc(IntPtr hWinEventHook, uint eventType,
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
