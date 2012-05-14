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
using SuperPutty.Classes;
using System.Windows.Input;
using System.Collections.Concurrent;

namespace SuperPutty
{
    public partial class frmSuperPutty : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

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

        private static string _MinttyExe;
        public static string MinttyExe
        {
            get { return _MinttyExe; }
            set { _MinttyExe = value; }
        }

        public static bool IsScpEnabled
        {
            get { return File.Exists(PscpExe); }
        }

        private SessionTreeview m_Sessions;
    
		private Classes.Database m_db;

        private ConcurrentDictionary<IntPtr, bool> children;
        private ConcurrentDictionary<IntPtr, ctlPuttyPanel> m_panelMapping;

        GlobalHotkeys m_hotkeys;
        KeyboardListener m_keyboard;
        WindowTitleTracker m_titleTracker;

        ~frmSuperPutty()
        {
            m_keyboard.Dispose();
        }

        public frmSuperPutty(string[] args)
        {
            this.children = new ConcurrentDictionary<IntPtr, bool>();
            m_panelMapping = new ConcurrentDictionary<IntPtr, ctlPuttyPanel>();
            m_hotkeys = new GlobalHotkeys();
            m_keyboard = new KeyboardListener(this, m_hotkeys);
            m_titleTracker = new WindowTitleTracker(this);

            // Check SQLite Database
            openOrCreateSQLiteDatabase();
            this.AddChild(this.Handle);

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

            // Get mintty executable path
            if (File.Exists(this.m_db.GetKey("mintty_exe")))
            {
                MinttyExe = this.m_db.GetKey("mintty_exe");
            }

            if (String.IsNullOrEmpty(PuttyExe))
            {
                editLocations();
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
            dockPanel1.LostFocus += dockPanel1_ActiveDocumentChanged;

            /* 
             * Open the session treeview and dock it on the right
             */
            m_Sessions = new SessionTreeview(this, dockPanel1);
            if(Classes.Database.GetBooleanKey("ShowSessionTreeview", true))
            {
            	showSessionTreeview();
            }
            /*
             * Parsing CL Arguments
             */
            ParseClArguments(args);
            
            // First time automatic update check
            firstTimeAutomaticUpdateCheck();
            
            // Set automatic update check menu item
            setAutomaticUpdateCheckMenuItem();
            
            // Set addtional timing menu item
            setAdditionalTimingMenuItem();
            
            // Check for updates.
            checkForUpdate(true);
            
            // Set window state and size
            setWindowStateAndSize();

            registerHotkeys();

            focusHacks();
        }

        public void AddChild(IntPtr handle)
        {
            if (!this.children.ContainsKey(handle))
            {
                this.children.TryAdd(handle, true);
            }
        }

        public void AddChild(ctlPuttyPanel panel, IntPtr handle)
        {
            AddChild(handle);
            m_panelMapping.TryAdd(handle, panel);
        }

        public void RemoveChild(IntPtr handle)
        {
            bool outValue;
            if (this.children.ContainsKey(handle))
            {
                this.children.TryRemove(handle, out outValue);
            }
        }

        public bool ContainsForegroundWindow()
        {
            return ContainsChild(GetForegroundWindow());
        }

        public bool ContainsChild(IntPtr child)
        {
            return this.children.ContainsKey(child);
        }

        private void editLocations()
        {
            dlgFindPutty dialog = new dlgFindPutty();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.m_db.SetKey("putty_exe", dialog.PuttyLocation);
                this.m_db.SetKey("pscp_exe", dialog.PscpLocation);
                this.m_db.SetKey("mintty_exe", dialog.MinttyLocation);
                PuttyExe = dialog.PuttyLocation;
                PscpExe = dialog.PscpLocation;
                MinttyExe = dialog.MinttyLocation;
            }
        }

        public void SetPanelTitle(IntPtr handle, String title)
        {
            if (this.m_panelMapping.ContainsKey(handle))
            {
                SetPanelTitle(this.m_panelMapping[handle], title);
            }
        }

        public void SetPanelTitle(ctlPuttyPanel panel, String title)
        {
            panel.TabText = title;
        }

        public void FocusCurrentTab()
        {
            if (dockPanel1.ActiveDocument is ctlPuttyPanel)
            {
                ctlPuttyPanel p = (ctlPuttyPanel)dockPanel1.ActiveDocument;

                this.Text = p.ApplicationTitle.Replace(" - PuTTY", "") + " - SuperPutty";
                p.Text = p.ApplicationTitle.Replace(" - PuTTY", "");
                p.SetFocusToChildApplication();
            }
        }

        /// <summary>
        /// Handles focusing on tabs/windows which host PuTTY
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dockPanel1_ActiveDocumentChanged(object sender, EventArgs e)
        {
            FocusCurrentTab();
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
            editLocations();
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
            if (MessageBox.Show("Exit SuperPuTTY?", "Confirm Exit", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
            {
				Classes.Database.SetKeyStatic("main_form_state", this.WindowState.ToString());
				Classes.Database.SetKeyStatic("main_form_height", this.Height.ToString());
				Classes.Database.SetKeyStatic("main_form_width", this.Width.ToString());                              

            	this.Close();
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
        	if (this.HostTextBox.Text == "")
        	{
        		MessageBox.Show("You must enter a host ip or name to connect.", "SuperPutty", MessageBoxButtons.OK);
        	}
        	else
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
        }
        
        public void ParseClArguments(string[] args)
        {
        	if (args.Length > 0)
        	{
	        	SessionData sessionData = Classes.CLI.ParseCLIArguments(args);
	
	            if (sessionData.UseSCP && IsScpEnabled)
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
            CreatePuttyPanel(sessionData, true);
        }
        
        public void CreatePuttyPanel(SessionData sessionData, bool isPutty)
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

            sessionPanel = new ctlPuttyPanel(this, sessionData, callback, isPutty);
            sessionPanel.Show(dockPanel1, sessionData.LastDockstate);
        }

        private void newMintty_Click(object sender, EventArgs e)
        {
            launchMintty();
        }

        private void launchMintty()
        {
            SessionData sessionData = new SessionData();
            sessionData.SessionName = "mintty";
            CreatePuttyPanel(sessionData, false);
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

        private void registerHotkeys()
        {
            m_hotkeys.RegisterGlobalHotkey(Key.M, GlobalHotkeys.MOD_ALT, GlobalHotkeys.Purpose.NewMinttyTab);
            m_hotkeys.RegisterGlobalHotkey(Key.Left, GlobalHotkeys.MOD_ALT, GlobalHotkeys.Purpose.Previous);
            m_hotkeys.RegisterGlobalHotkey(Key.Right, GlobalHotkeys.MOD_ALT, GlobalHotkeys.Purpose.Next);
            m_hotkeys.RegisterGlobalHotkey(Key.D1, GlobalHotkeys.MOD_CONTROL, GlobalHotkeys.Purpose.Tab1);
            m_hotkeys.RegisterGlobalHotkey(Key.D2, GlobalHotkeys.MOD_CONTROL, GlobalHotkeys.Purpose.Tab2);
            m_hotkeys.RegisterGlobalHotkey(Key.D3, GlobalHotkeys.MOD_CONTROL, GlobalHotkeys.Purpose.Tab3);
            m_hotkeys.RegisterGlobalHotkey(Key.D4, GlobalHotkeys.MOD_CONTROL, GlobalHotkeys.Purpose.Tab4);
            m_hotkeys.RegisterGlobalHotkey(Key.D5, GlobalHotkeys.MOD_CONTROL, GlobalHotkeys.Purpose.Tab5);
            m_hotkeys.RegisterGlobalHotkey(Key.D6, GlobalHotkeys.MOD_CONTROL, GlobalHotkeys.Purpose.Tab6);
            m_hotkeys.RegisterGlobalHotkey(Key.D7, GlobalHotkeys.MOD_CONTROL, GlobalHotkeys.Purpose.Tab7);
            m_hotkeys.RegisterGlobalHotkey(Key.D8, GlobalHotkeys.MOD_CONTROL, GlobalHotkeys.Purpose.Tab8);
            m_hotkeys.RegisterGlobalHotkey(Key.D9, GlobalHotkeys.MOD_CONTROL, GlobalHotkeys.Purpose.LastTab);

            m_keyboard.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
            m_keyboard.KeyUp += new RawKeyEventHandler(KListener_KeyUp);
        }

        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            m_hotkeys.KeyDown(args.Key);
            if (ContainsForegroundWindow())
            {
                handleHotkeys(args.Key);
            }
        }

        void KListener_KeyUp(object sender, RawKeyEventArgs args)
        {
            m_hotkeys.KeyUp(args.Key);
        }

        private void handleHotkeys(Key key)
        {
            switch (m_hotkeys.GetHotkey(key))
            {
                case GlobalHotkeys.Purpose.NewMinttyTab:
                    launchMintty();
                    break;

                case GlobalHotkeys.Purpose.Previous:
                    nextTab(-1);
                    break;

                case GlobalHotkeys.Purpose.Next:
                    nextTab(1);
                    break;

                case GlobalHotkeys.Purpose.Tab1:
                case GlobalHotkeys.Purpose.Tab2:
                case GlobalHotkeys.Purpose.Tab3:
                case GlobalHotkeys.Purpose.Tab4:
                case GlobalHotkeys.Purpose.Tab5:
                case GlobalHotkeys.Purpose.Tab6:
                case GlobalHotkeys.Purpose.Tab7:
                case GlobalHotkeys.Purpose.Tab8:
                case GlobalHotkeys.Purpose.LastTab:
                    selectTab(m_hotkeys.GetHotkey(key));
                    break;

                case GlobalHotkeys.Purpose.None:
                default:
                    break;
            }
        }

        private void nextTab(int direction)
        {
            int tabs = this.children.Count - 1;
            if (tabs > 1)
            {
                int current = this.dockPanel1.Contents[1].DockHandler.GetCurrentTabIndex();
                current += direction;

                if (current < 0)
                {
                    current = tabs - 1;
                }
                else
                {
                    current %= tabs;
                }

                selectTab(current);
            }
        }

        private void selectTab(GlobalHotkeys.Purpose tabPosition)
        {
            int tabs = this.children.Count - 1;
            if (tabs > 1)
            {
                int index = ((int)tabPosition) % ((int)GlobalHotkeys.Purpose.Tab1);
                if (tabPosition == GlobalHotkeys.Purpose.LastTab)
                {
                    index = this.dockPanel1.Contents.Count - 1;
                }
                selectTab(index);
            }
        }

        private void selectTab(int index)
        {
            if (this.children.Count > 1)
            {
                IDockContent content = this.dockPanel1.Contents[1].DockHandler.SetActiveTab(index);
                if (content != null)
                {
                    ctlPuttyPanel p = (ctlPuttyPanel)content;
                    p.SetFocusToChildApplication();
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_COPYDATA = 0x004A;

            switch (m.Msg)
            {
                case WM_COPYDATA:
                    COPYDATA cd = (COPYDATA)Marshal.PtrToStructure(m.LParam, typeof(COPYDATA));
                    string strArgs = Marshal.PtrToStringAnsi(cd.lpData);
                    string[] args = strArgs.Split(' ');
                    ParseClArguments(args);
                    break;
                default:
                    break;
            }

            bool callBase = WndProcForFocus(ref m);
            if (callBase)
            {
                base.WndProc(ref m);
            }
        }

        
        void ToolStripMenuItem3Click(object sender, EventArgs e)
        {
        	checkForUpdate(false);
        }
        
        void checkForUpdate(bool automatic)
        {
        	// Get the current value from the database
        	Classes.Database d = new SuperPutty.Classes.Database();
        	d.Open();
			string key = "automatic_update_check";
			bool performCheck = false;
			performCheck = d.GetKey(key) == "" ? false : bool.Parse(d.GetKey(key));
        	
			// Check to see if we should even perform a check
			if (performCheck && automatic)
			{
	            try
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
	                    if (MessageBox.Show("There is a new version available. Would you like to open the dicussion page to download it?", "SuperPutty - New version available!", MessageBoxButtons.YesNo) == DialogResult.Yes)
	                    {
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
	            catch (WebException e)
	            {
	                if (!automatic)
	                {
	                    MessageBox.Show("Error while checking for updates: " + e.Message, "SuperPutty");
	                }
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

        private void CopyPuttySessionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to copy all sessions from PuTTY? This may overwrite identically named sessions in SuperPutty!", "SuperPutty", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                SessionTreeview.copySessionsFromPuTTY();
                m_Sessions.LoadSessions();
            }
        }
        
        void AutomaticUpdateCheckToolStripMenuItemClick(object sender, EventArgs e)
        {
         	// Get the current value from the database
        	Classes.Database d = new SuperPutty.Classes.Database();
        	d.Open();
			string key = "automatic_update_check";
			bool val = false;
			val = d.GetKey(key) == "" ? false : bool.Parse(d.GetKey(key));
			
			// If the value is true, then set it to false and uncheck menu item,
			// else, set it to true.
        	if (val)
        	{
        		val = false;
        	}
        	else
        	{
        		val = true;
        	}
        	
        	// Set the menu item check state.
    		this.automaticUpdateCheckToolStripMenuItem.Checked = val;

        	// Update the database
        	d.SetKey(key, val.ToString());
        }
        
        
	    private void setAutomaticUpdateCheckMenuItem()
        {
        	// Get the current value from the d1Gatabase
        	Classes.Database d = new SuperPutty.Classes.Database();
        	d.Open();
			string key = "automatic_update_check";
			bool val = false;
			val = d.GetKey(key) == "" ? false : bool.Parse(d.GetKey(key));

			// Set the checked property
    		this.automaticUpdateCheckToolStripMenuItem.Checked = val;
        }
	    
	    private void firstTimeAutomaticUpdateCheck()
	    {
        	// Get the current value from the database
        	Classes.Database d = new SuperPutty.Classes.Database();
        	d.Open();
			string key = "automatic_update_check";
			string val = d.GetKey(key);

			// If the value hasnt been set, then the user has not chosen.
			if (val == "")
			{
				bool enabled = false;
				if (MessageBox.Show("Do you wish to enable automatic update checks?", "SuperPutty", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					enabled = true;
				}

				// Store the users decision
				d.SetKey(key, enabled.ToString());

				MessageBox.Show("You may enable/disable automatic update checks by navigating to the File->Settings menu.", "SuperPutty", MessageBoxButtons.OK);
			}
	    }
        
        void WindowToolStripMenuItemClick(object sender, EventArgs e)
        {
        }
        
        private void showSessionTreeview()
        {
            m_Sessions = new SessionTreeview(this, dockPanel1);
            m_Sessions.Show(dockPanel1, WeifenLuo.WinFormsUI.Docking.DockState.DockRight);
        }
        
        void ToolbarViewSessionsClick(object sender, EventArgs e)
        {
        	if (m_Sessions.Visible == false)
        	{
        		showSessionTreeview();
        		Classes.Database d = new SuperPutty.Classes.Database();
        		d.Open();
        		d.SetKey("ShowSessionTreeview", "true");
        	}
        }
        
        
        
        void FrmSuperPuttySizeChanged(object sender, EventArgs e)
        {
        }
        
        private void setWindowStateAndSize()
        {
        	//this.WindowState = FormWindowState.Parse(FormWindowState, Classes.Database.GetStringKey("main_form_state", ""));
        	this.Height = Classes.Database.GetIntegerKey("main_form_height", 600);
        	this.Width = Classes.Database.GetIntegerKey("main_form_width", 800);
        }
        
        void FrmSuperPuttyFormClosing(object sender, FormClosingEventArgs e)
        {
			Classes.Database.SetKeyStatic("main_form_state", this.WindowState.ToString());
			Classes.Database.SetKeyStatic("main_form_height", this.Height.ToString());
			Classes.Database.SetKeyStatic("main_form_width", this.Width.ToString());                              
        }
        
        void AdditionalTimingClick(object sender, EventArgs e)
        {
        	// Get the current value from the database
        	Classes.Database d = new SuperPutty.Classes.Database();
        	d.Open();
			string key = "additional_timing";
			bool val = false;
			val = d.GetKey(key) == "" ? false : bool.Parse(d.GetKey(key));
			
			// If the value is true, then set it to false and uncheck menu item,
			// else, set it to true.
        	if (val)
        	{
        		val = false;
        	}
        	else
        	{
        		val = true;
        	}
        	
        	// Set the menu item check state.
        	this.additionalTiming.Checked = val;

        	// Update the database
        	d.SetKey(key, val.ToString());
        }
        
        private void setAdditionalTimingMenuItem()
        {
        	// Get the current value from the database
        	Classes.Database d = new SuperPutty.Classes.Database();
        	d.Open();
			string key = "additional_timing";
			bool val = false;
			val = d.GetKey(key) == "" ? false : bool.Parse(d.GetKey(key));

			// Set the checked property
			this.additionalTiming.Checked = val;
        }


        #region FocusHacks Code used to get the children window to focus at the right time
        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int RegisterWindowMessage(string lpString);

        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        static extern int RegisterShellHookWindow(IntPtr hWnd);

        private int m_shellHookNotify;
        private bool m_externalWindow = false;
        private DateTime m_lastMouseDownOnTitleBar = DateTime.Now;
        private TimeSpan m_delayUntilMouseMove = new TimeSpan(0, 0, 0, 0, 200); // 200ms
        private Point m_mouseDownLocation = new Point(0, 0);

        private int GET_X_LPARAM(int lParam)
        {
            return (lParam & 0xffff);
        }

        private int GET_Y_LPARAM(int lParam)
        {
            return (lParam >> 16);
        }

        private bool WndProcForFocus(ref Message m)
        {
            const int WM_NCLBUTTONDOWN = 0x00A1;
            const int WM_NCMOUSEMOVE = 0x00A0;
            const int WM_NCACTIVATE = 0x0086;

            switch (m.Msg)
            {
                case WM_NCLBUTTONDOWN:
                    // This is in conjunction with the WM_NCMOUSEMOVE. We cannot detect
                    // WM_NCLBUTTONUP because it gets swallowed up on many occasions. As a result
                    // we detect the button down and check the NCMOUSEMOVE to see if it has
                    // changed location. If the mouse location is different, then we let
                    // the resize handler deal with the focus. If not, then we assume that it
                    // is a mouseup action.
                    this.m_lastMouseDownOnTitleBar = DateTime.Now;
                    m_mouseDownLocation = new Point(GET_X_LPARAM((int)m.LParam), GET_Y_LPARAM((int)m.LParam));
                    break;
                case WM_NCMOUSEMOVE:
                    Point currentLocation = new Point(GET_X_LPARAM((int)m.LParam), GET_Y_LPARAM((int)m.LParam));
                    if ((this.m_lastMouseDownOnTitleBar - DateTime.Now < this.m_delayUntilMouseMove)
                            && currentLocation == m_mouseDownLocation)
                    {
                        FocusCurrentTab();
                    }
                    break;
                case WM_NCACTIVATE:
                    // Never allow this window to display itself as inactive
                    DefWindowProc(this.Handle, m.Msg, (IntPtr)1, m.LParam);
                    m.Result = (IntPtr)1;
                    return false;
                default:
                    if (m.Msg == m_shellHookNotify)
                    {
                        switch (m.WParam.ToInt32())
                        {
                            case 4:
                                IntPtr current = GetForegroundWindow();
                                if (current != this.Handle && !ContainsChild(current))
                                {
                                    m_externalWindow = true;
                                }
                                else if (m_externalWindow)
                                {
                                    m_externalWindow = false;
                                    FocusCurrentTab();
                                }
                                break;
                            default:
                                break;
                        }


                    }
                    break;
            }

            return true;
        }

        // Hook into events to handle focus problems.
        private void focusHacks()
        {
            this.ResizeEnd += HandleResizeEnd;
            m_shellHookNotify = RegisterWindowMessage("SHELLHOOK");
            RegisterShellHookWindow(this.Handle);
        }

        // Handle various events to keep the child window focused
        private void HandleResizeEnd(Object sender, EventArgs e)
        {
            FocusCurrentTab();
        }

        #endregion
    }
}
