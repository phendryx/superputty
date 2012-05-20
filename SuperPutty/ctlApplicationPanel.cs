/*
 * Copyright (c) 2009 Jim Radford http://www.jimradford.com
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions: 
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using WeifenLuo.WinFormsUI.Docking;
using SuperPutty.Classes;

namespace SuperPutty
{
    public delegate void PuttyClosedCallback(bool error);

    public class ApplicationPanel : System.Windows.Forms.Panel
    {

        public event DataReceivedEventHandler OutputDataReceived;
        protected virtual void OnOutputDataReceived(DataReceivedEventArgs e)
        {
            if (OutputDataReceived != null)
            {
                OutputDataReceived(this, e);
            }
        }

        /*************************** Begin Hack to watch for windows focus change events **************************************
         * This is based on this form post:
         * http://social.msdn.microsoft.com/Forums/en-US/clr/thread/c04e343f-f2e7-469a-8a54-48ca84f78c28
         * 
         * The idea is to watch for the EVENT_SYSTEM_FOREGROUND window, and when we see that from the putty terminal window
         * bring the superputty window to the foreground
         */

        WinAPI.WinEventDelegate _WinEventDelegate;
        IntPtr m_hWinEventHook;

        public ApplicationPanel()
        {
            // setup up the hook to watch for all EVENT_SYSTEM_FOREGROUND events system wide
            this._WinEventDelegate = new WinAPI.WinEventDelegate(WinEventProc);
            m_hWinEventHook = WinAPI.SetWinEventHook(WinAPI.EVENT_SYSTEM_FOREGROUND, WinAPI.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, this._WinEventDelegate, 0, 0, WinAPI.WINEVENT_OUTOFCONTEXT);
        }

        ~ApplicationPanel()
        {
        }

		//http://social.msdn.microsoft.com/Forums/en-US/clr/thread/c04e343f-f2e7-469a-8a54-48ca84f78c28
        void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            // if we got the EVENT_SYSTEM_FOREGROUND, and the hwnd is the putty terminal hwnd (m_AppWin)
            // then bring the supperputty window to the foreground
            if (eventType == WinAPI.EVENT_SYSTEM_FOREGROUND && hwnd == m_AppWin)
            {
                // This is the easiest way I found to get the superputty window to be brought to the top
                // if you leave TopMost = true; then the window will always be on top.
                this.TopLevelControl.FindForm().TopMost = true;
                this.TopLevelControl.FindForm().TopMost = false;
            }
        }

        /*************************** End Hack to watch for windows focus change events ***************************************/



        // Win32 Exceptions which might occur trying to start the process
        const int ERROR_FILE_NOT_FOUND = 2;
        const int ERROR_ACCESS_DENIED = 5;

        #region Private Member Variables
        private Process m_Process;
        private bool m_Created = false;
        private IntPtr m_AppWin;
        private string m_ApplicationName = "";
        private string m_ApplicationParameters = "";

        internal PuttyClosedCallback m_CloseCallback;

        /// <summary>Set the name of the application executable to launch</summary>
        [Category("Data"), Description("The path/file to launch"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ApplicationName
        {
            get { return m_ApplicationName; }
            set { m_ApplicationName = value; }
        }
        
        [Category("Data"), Description("The parameters to pass to the application being launched"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ApplicationParameters
        {
            get { return m_ApplicationParameters; }
            set { m_ApplicationParameters = value; }
        }
        #endregion

        #region Public Member Variables
        public string ApplicationWindowTitle
        {
        	get { return this.m_Process.MainWindowTitle; }
        }

        #endregion



        #region Base Overrides
       
        /// <summary>
        /// Force redraw of control when size changes
        /// </summary>
        /// <param name="e">Not used</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            this.Invalidate();
            base.OnSizeChanged(e);
        }

       
        public bool ReFocusPuTTY()
        {           
            return (this.m_AppWin != null 
                && WinAPI.GetForegroundWindow() != this.m_AppWin
                && !WinAPI.SetForegroundWindow(this.m_AppWin));
        }

        private void m_Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnOutputDataReceived(e);
        }

        /// <summary>
        /// Create (start) the hosted application when the parent becomes visible
        /// </summary>
        /// <param name="e">Not used</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!m_Created && !String.IsNullOrEmpty(ApplicationName)) // only allow one instance of the child
            {
                m_Created = true;
                m_AppWin = IntPtr.Zero;

                try
                {
                    m_Process = new Process();
                    m_Process.EnableRaisingEvents = true;
                    //m_Process.Exited += new EventHandler(p_Exited);
                    m_Process.StartInfo.FileName = ApplicationName;
                    m_Process.StartInfo.Arguments = ApplicationParameters;
                    m_Process.StartInfo.RedirectStandardOutput = true;
                    m_Process.StartInfo.UseShellExecute = false;
                    m_Process.OutputDataReceived += m_Process_OutputDataReceived;

                    m_Process.Exited += delegate(object sender, EventArgs ev)
                    {
                        m_CloseCallback(true);
                    };

                    m_Process.Start();
                    m_Process.BeginOutputReadLine();

                    // Wait for application to start and become idle
                    m_Process.WaitForInputIdle();
                    
                    // Additional timing
                    if(Classes.Database.GetBooleanKey("additional_timing", false))
                    {
                    	System.Threading.Thread.Sleep(200);
                    }
                    
                    m_AppWin = m_Process.MainWindowHandle;
                }
                catch (InvalidOperationException ex)
                {
                    /* Possible Causes:
                     * No file name was specified in the Process component's StartInfo.
                     * -or-
                     * The ProcessStartInfo.UseShellExecute member of the StartInfo property is true while ProcessStartInfo.RedirectStandardInput, 
                     * ProcessStartInfo.RedirectStandardOutput, or ProcessStartInfo.RedirectStandardError is true. 
                     */
                    MessageBox.Show(this, ex.Message, "Invalid Operation Error");
                    throw;
                }
                catch (Win32Exception ex)
                {
                    /*
                     * Checks are elsewhere to ensure these don't occur, but incase they do we're gonna bail with a nasty exception
                     * which will hopefully send users kicking and screaming at me to fix this (And hopefully they will include a 
                     * stacktrace!)
                     */
                    if (ex.NativeErrorCode == ERROR_ACCESS_DENIED)
                    {
                        throw;
                    }
                    else if (ex.NativeErrorCode == ERROR_FILE_NOT_FOUND)
                    {
                        throw;
                    }
                }

                //Logger.Log("ApplicationPanel Handle: {0}", this.Handle.ToString("X"));              
                //Logger.Log("Process Handle: {0}", m_AppWin.ToString("X"));
                // Set the application as a child of the parent form
                WinAPI.SetParent(m_AppWin, this.Handle);

                // Show it! (must be done before we set the windows visibility parameters below                
                WinAPI.ShowWindow(m_AppWin, WinAPI.WindowShowStyle.Maximize);

                // set window parameters (how it's displayed)
                int lStyle = WinAPI.GetWindowLong(m_AppWin, WinAPI.GWL_STYLE);
                lStyle &= ~(WinAPI.WindowStyles.WS_CAPTION | WinAPI.WindowStyles.WS_THICKFRAME);
                WinAPI.SetWindowLong(m_AppWin, WinAPI.GWL_STYLE, lStyle);

                // Move the child so it's located over the parent
                WinAPI.MoveWindow(m_AppWin, 0, 0, this.Width, this.Height, true);
            }
                  
            base.OnVisibleChanged(e);
        }

        public IntPtr GetChildHandle()
        {
            return m_AppWin;
        }
        
        /// <summary>
        /// Send a close message to the hosted application window when the parent is destroyed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);

            WinAPI.UnhookWinEvent(m_hWinEventHook);
            if (m_AppWin != IntPtr.Zero)
            {
                WinAPI.PostMessage(m_AppWin, WinAPI.WM.CLOSE, 0, 0);

                System.Threading.Thread.Sleep(100);

                m_AppWin = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Refresh the hosted applications window when the parent changes size
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            if (this.m_AppWin != IntPtr.Zero)
            {
                if (this.Width > 0 && this.Height > 0)
                {
                    WinAPI.MoveWindow(m_AppWin, 0, 0, this.Width, this.Height, true);
                }
            }
            base.OnResize(e);
        }

        #endregion        
    }

}
