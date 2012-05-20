using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperPutty.Classes
{
    class RestoreFromMinimizedTracker : WindowEventHandler
    {
        const int EVENT_SYSTEM_MINIMIZEEND = 23;

        public RestoreFromMinimizedTracker(frmSuperPutty form) : base(form)
        {
            HookEvent(EVENT_SYSTEM_MINIMIZEEND);
        }

        protected override void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == this.m_form.Handle)
            {
                this.m_form.FocusCurrentTab();
            }
        }
    }
}
