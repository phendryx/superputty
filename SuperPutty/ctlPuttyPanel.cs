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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


namespace SuperPutty
{
    public partial class ctlPuttyPanel : ToolWindow
    {        
        private string ApplicationName = String.Empty;
        private string ApplicationParameters = String.Empty;

        private ApplicationPanel applicationwrapper1;
        private SessionData m_Session;
        private PuttyClosedCallback m_ApplicationExit;
        private frmSuperPutty m_SuperPutty;

        public string ApplicationTitle
        {
        	get { return this.applicationwrapper1.ApplicationWindowTitle; }
        }

        public ctlPuttyPanel(frmSuperPutty superPutty, SessionData session, PuttyClosedCallback callback, bool isPutty)
        {
            m_SuperPutty = superPutty;
            m_Session = session;
            m_ApplicationExit = callback;

            if (isPutty)
            {
                string args = "-" + session.Proto.ToString().ToLower() + " ";
                args += (!String.IsNullOrEmpty(m_Session.Password) && m_Session.Password.Length > 0) ? "-pw " + m_Session.Password + " " : "";
                args += "-P " + m_Session.Port + " ";
                args += (!String.IsNullOrEmpty(m_Session.PuttySession)) ? "-load \"" + m_Session.PuttySession + "\" " : "";
                args += (!String.IsNullOrEmpty(m_Session.Username) && m_Session.Username.Length > 0) ? m_Session.Username + "@" : "";
                args += m_Session.Host;
                Logger.Log("Args: '{0}'", args);
                this.ApplicationParameters = args;
            }
            else
            {
                this.ApplicationParameters = "/bin/bash -l";
            }

            InitializeComponent();

            this.Text = session.SessionName;

            CreatePanel(isPutty);
        }

        private void CreatePanel(bool isPutty)
        {
            this.applicationwrapper1 = new ApplicationPanel();
            this.SuspendLayout();            
            this.applicationwrapper1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.applicationwrapper1.ApplicationName = isPutty ? frmSuperPutty.PuttyExe : frmSuperPutty.MinttyExe;
            this.applicationwrapper1.ApplicationParameters = this.ApplicationParameters;
            this.applicationwrapper1.Location = new System.Drawing.Point(0, 0);
            this.applicationwrapper1.Name = "applicationControl1";
            this.applicationwrapper1.Size = new System.Drawing.Size(284, 264);
            this.applicationwrapper1.TabIndex = 0;            
            this.applicationwrapper1.m_CloseCallback = this.m_ApplicationExit;
            this.Controls.Add(this.applicationwrapper1);
            this.applicationwrapper1.VisibleChanged += applicationwrapper1_VisibleChanged;
            this.applicationwrapper1.HandleDestroyed += applicationwrapper1_HandleDestroyed;

            this.ResumeLayout();
        }

        /// <summary>
        /// Adding the child handle into our children. We use this information to decide
        /// when to trigger hotkeys.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void applicationwrapper1_VisibleChanged(object sender, EventArgs e)
        {
            m_SuperPutty.AddChild(this, this.applicationwrapper1.GetChildHandle());
        }

        /// <summary>
        /// Remove the child handle from our children.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void applicationwrapper1_HandleDestroyed(object sender, EventArgs e)
        {
            m_SuperPutty.RemoveChild(this.applicationwrapper1.GetChildHandle());
        }

        private void closeSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void duplicateSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SessionData sessionData = new SessionData(m_Session);
            m_SuperPutty.CreatePuttyPanel(sessionData);
        }

        /// <summary>
        /// Reset the focus to the child application window
        /// </summary>
        internal void SetFocusToChildApplication()
        {
            this.applicationwrapper1.ReFocusPuTTY();         
        }

        
        void RestartSessionToolStripMenuItemClick(object sender, EventArgs e)
        {
        	SessionData sessionData = new SessionData(m_Session);
            m_SuperPutty.CreatePuttyPanel(sessionData);
            this.Close();
        }
    }
}
