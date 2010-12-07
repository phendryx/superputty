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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using WeifenLuo.WinFormsUI.Docking;
using System.Data.SQLite;

namespace SuperPutty
{
    public partial class frmSuperPutty : Form
    {
        private static string _PuttyExe;
        public static string PuttyExe
        {
            get { return _PuttyExe; }
            set { _PuttyExe = value; }
        }

        private static string _PscpExe;

        public static string PscpExe
        {
            get { return _PscpExe; }
            set { _PscpExe = value; }
        }

        public static bool IsScpEnabled
        {
            get { return File.Exists(PscpExe); }
        }

        private SessionTreeview m_Sessions;
		private Classes.Database m_db;
		
        public frmSuperPutty(string[] args)
        {
            // Check SQLite Database
            openOrCreateSQLiteDatabase();

            #region Exe Paths
            // Get putty executable path
            if (File.Exists(this.m_db.GetKey("putty_exe")))
            {
                PuttyExe = this.m_db.GetKey("putty_exe");
            }

            // Get pscp executable path
            if (File.Exists(this.m_db.GetKey("pscp_exe")))
            {
                PscpExe = this.m_db.GetKey("pscp_exe");
            }

            if (String.IsNullOrEmpty(PuttyExe))
            {
                dlgFindPutty dialog = new dlgFindPutty();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                	this.m_db.SetKey("putty_exe", dialog.PuttyLocation);
                	this.m_db.SetKey("pscp_exe", dialog.PscpLocation);
		            PuttyExe = this.m_db.GetKey("putty_exe");
		            PscpExe = this.m_db.GetKey("pscp_exe");
                }
            }

            if (String.IsNullOrEmpty(PuttyExe))
            {
                MessageBox.Show("Cannot find PuTTY installation. Please visit http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html to download a copy",
                    "PuTTY Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                System.Environment.Exit(1);
            }
			#endregion

            InitializeComponent();

#if DEBUG
            // Only show the option for the debug log viewer when we're compiled with DEBUG defined.
            debugLogToolStripMenuItem.Visible = true;
#endif

			//Activate PasswordChar for passwordfield in connectbar
            PasswordTextBox.TextBox.UseSystemPasswordChar = true;
			
			//Select protocol SSH
            ProtocolBox.SelectedItem = ProtocolBox.Items[0];
			
            dockPanel1.ActiveDocumentChanged += dockPanel1_ActiveDocumentChanged;

            /* 
             * Open the session treeview and dock it on the right
             */
            m_Sessions = new SessionTreeview(dockPanel1);
            m_Sessions.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockRight);

            /*
             * Parsing CL Arguments
             */
            ParseClArguments(args);
            
            // Check for updates.
            checkForUpdate(true);
        }

        /// <summary>
        /// Handles focusing on tabs/windows which host PuTTY
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dockPanel1_ActiveDocumentChanged(object sender, EventArgs e)
        {
            if (dockPanel1.ActiveDocument is ctlPuttyPanel)
            {
                ctlPuttyPanel p = (ctlPuttyPanel)dockPanel1.ActiveDocument;

	            this.Text = p.ApplicationTitle.Replace(" - PuTTY", "") + " - SuperPutty";
				p.Text = p.ApplicationTitle.Replace(" - PuTTY", "");
                p.SetFocusToChildApplication();
            }
        }


        private void frmSuperPutty_Activated(object sender, EventArgs e)
        {
            //dockPanel1_ActiveDocumentChanged(null, null);
        }

        private void aboutSuperPuttyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 about = new AboutBox1();
            about.ShowDialog();
            about = null;
        }

        private void superPuttyWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://code.google.com/p/superputty/");
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (File.Exists(Application.StartupPath + @"\superputty.chm"))
            {
                Process.Start(Application.StartupPath + @"\superputty.chm");
            }
            else
            {
                DialogResult result = MessageBox.Show("Local documentation could not be found. Would you like to view the documentation online instead?", "Documentation Not Found", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Process.Start("http://code.google.com/p/superputty/wiki/Documentation");
                }
            }
        }

        private void puTTYScpLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dlgFindPutty dialog = new dlgFindPutty();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                PuttyExe = dialog.PuttyLocation;
                PscpExe = dialog.PscpLocation;
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "XML Files|*.xml";
            saveDialog.FileName = "Sessions.XML";
            saveDialog.InitialDirectory = Application.StartupPath;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                SessionTreeview.ExportSessionsToXml(saveDialog.FileName);
            }
        }

        private void importSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "XML Files|*.xml";
            openDialog.FileName = "Sessions.XML";
            openDialog.CheckFileExists = true;
            openDialog.InitialDirectory = Application.StartupPath;
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                SessionTreeview.ImportSessionsFromXml(openDialog.FileName);
                m_Sessions.LoadSessions();
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Exit SuperPuTTY?", "Confirm Exit", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                System.Environment.Exit(0);
            }
        }

        private void puTTYConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = PuttyExe;
            p.Start();
        }

        private void debugLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DebugLogViewer logView = new DebugLogViewer();
            logView.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockBottomAutoHide);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SessionData sessionData = new SessionData();

            sessionData.Host = HostTextBox.Text;
            sessionData.Port = Convert.ToInt32(PortTextBox.Text);
            sessionData.Proto = (ProtocolBox.Text == "SCP") ? (ConnectionProtocol)Enum.Parse(typeof(ConnectionProtocol), "SSH") : (ConnectionProtocol)Enum.Parse(typeof(ConnectionProtocol), ProtocolBox.Text);
            sessionData.PuttySession = "Default Settings";
            sessionData.SessionName = HostTextBox.Text;
            sessionData.Username = LoginTextBox.Text;
			sessionData.Password = PasswordTextBox.Text;

            if (ProtocolBox.Text == "SCP" && IsScpEnabled)
            {
				CreateRemoteFileListPanel(sessionData);
            }
            else
            {
                CreatePuttyPanel(sessionData);
            }
        }

        public void ParseClArguments(string[] args)
        {
            SessionData sessionData = null;
            bool use_scp = false;
            if (args.Length > 0)
            {
                sessionData = new SessionData();
                string proto = "", port = "", username = "", puttySession = "", password = "";
                for (int i = 0; i < args.Length - 1; i++)
                {
                    switch (args[i].ToString().ToLower())
                    {
                        case "-ssh":
                            proto = "SSH";
                            break;

                        case "-serial":
                            proto = "Serial";
                            break;

                        case "-telnet":
                            proto = "Telnet";
                            break;

                        case "-scp":
                            proto = "SSH";
                            use_scp = true;
                            break;

                        case "-raw":
                            proto = "Raw";
                            break;

                        case "-rlogin":
                            proto = "Rlogin";
                            break;

                        case "-P":
                            port = args[i + 1];
                            i++;
                            break;

                        case "-l":
                            username = args[i + 1];
                            i++;
                            break;

                        case "-pw":
                            password = args[i + 1];
                            i++;
                            break;

                        case "-load":
                            puttySession = args[i + 1];
                            sessionData.PuttySession = args[i + 1];
                            i++;
                            break;
                    }
                }
                sessionData.Host = args[args.Length - 1];
                sessionData.SessionName = args[args.Length - 1];

                sessionData.Proto = (proto != "") ? (ConnectionProtocol)Enum.Parse(typeof(ConnectionProtocol), proto) : (ConnectionProtocol)Enum.Parse(typeof(ConnectionProtocol), "SSH");
                sessionData.Port = (port != "") ? Convert.ToInt32(port) : 22;
                sessionData.Username = (username != "") ? username : "";
                sessionData.Password = (password != "") ? password : "";
                sessionData.PuttySession = (puttySession != "") ? puttySession : "Default Settings";

                if (use_scp && IsScpEnabled)
                {
                    CreateRemoteFileListPanel(sessionData);
                }
                else
                {
                    CreatePuttyPanel(sessionData);
                }
            }
        }

        public void CreatePuttyPanel(SessionData sessionData)
        {
            ctlPuttyPanel sessionPanel = null;

            // This is the callback fired when the panel containing the terminal is closed
            // We use this to save the last docking location
            PuttyClosedCallback callback = delegate(bool closed)
            {
                if (sessionPanel != null)
                {
                    // save the last dockstate (if it has been changed)
                    if (sessionData.LastDockstate != sessionPanel.DockState
                        && sessionPanel.DockState != DockState.Unknown
                        && sessionPanel.DockState != DockState.Hidden)
                    {
                        sessionData.LastDockstate = sessionPanel.DockState;
                        sessionData.SaveToRegistry();
                    }
    
                    if (sessionPanel.InvokeRequired)
                    {
                        this.BeginInvoke((MethodInvoker)delegate()
                        {
                            sessionPanel.Close();
                         });
                    }
                    else
                    {
                        sessionPanel.Close();
                    }
                }
            };

            sessionPanel = new ctlPuttyPanel(sessionData, callback);
            sessionPanel.Show(dockPanel1, sessionData.LastDockstate);
        }

        public void CreateRemoteFileListPanel(SessionData sessionData)
        {
            RemoteFileListPanel dir = null;
            bool cancelShow = false;
            if (sessionData != null)
            {
                PuttyClosedCallback callback = delegate(bool error)
                {
                    cancelShow = error;
                };
                PscpTransfer xfer = new PscpTransfer(sessionData);
                xfer.PuttyClosed = callback;

                dir = new RemoteFileListPanel(xfer, dockPanel1, sessionData);
                if (!cancelShow)
                {
                    dir.Show(dockPanel1);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x004A)
            {
                COPYDATA cd = (COPYDATA) Marshal.PtrToStructure(m.LParam, typeof(COPYDATA));
                string strArgs = Marshal.PtrToStringAnsi(cd.lpData);
                string[] args = strArgs.Split(' ');
                ParseClArguments(args);
            }
            base.WndProc(ref m);
        }

        
        void ToolStripMenuItem3Click(object sender, EventArgs e)
        {
        	checkForUpdate(false);
        }
        
        void checkForUpdate(bool automatic)
        {
			string url = "https://github.com/phendryx/superputty/raw/master/VERSION";
	        string text = "";
			using (WebClient client = new WebClient())
	        {
	            text = client.DownloadString(url);
	        }

			string[] version = System.Text.RegularExpressions.Regex.Split(text, @"\|");
			
			string thisVersion = "";
			object[] attrs = System.Reflection.Assembly.GetEntryAssembly().GetCustomAttributes(true);
			foreach (object o in attrs)
			{
				if (o.GetType() == typeof(System.Reflection.AssemblyFileVersionAttribute))
				{
					thisVersion = ((System.Reflection.AssemblyFileVersionAttribute)o).Version;
				}
			}

			if (thisVersion != version[0])
			{
				if (MessageBox.Show("There is a new version available. Would you like to open the dicussion page to download it?", "SuperPutty - New version available!", MessageBoxButtons.YesNo) == DialogResult.Yes) {
					Process.Start(version[1]);
				}
			}			
			else
			{
				if (!automatic)
				{
					MessageBox.Show("No new version available.", "SuperPutty");
				}
			}        	
        }
        
        void ToolStripMenuItem4Click(object sender, EventArgs e)
        {
        	Process.Start("http://superputty.vanillaforums.com/");
        }
        
        void DockPanel1ActiveContentChanged(object sender, EventArgs e)
        {
        	
        }
        
        private void openOrCreateSQLiteDatabase()
        {
        	this.m_db = new SuperPutty.Classes.Database();
        	this.m_db.Open();
        }
		

        private void HostTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                toolStripButton1_Click(sender, e);
            }
        }
    }
}
