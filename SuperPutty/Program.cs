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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using SuperPutty.Classes;

namespace SuperPutty
{
    [StructLayout(LayoutKind.Sequential)]
    public struct COPYDATA
    {
        public uint dwData;
        public uint cbData;
        public IntPtr lpData;
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool onlyInstance = false;
#if DEBUG
            Mutex mutex = new Mutex(true, "SuperPutty", out onlyInstance);
#else
            Mutex mutex = new Mutex(true, "SuperPuttyRelease", out onlyInstance);
#endif
            if (!onlyInstance)
            {
                string strArgs = "";
				if(args.Length > 0)
				{
					strArgs += args[0];
					
					for (int i = 1; i < args.Length; i++)
					{
						strArgs += " " + args[i];
					}
				}

                COPYDATA cd = new COPYDATA();
                cd.dwData = 0;
                cd.cbData = (uint)strArgs.Length + 1;

                cd.lpData = Marshal.StringToHGlobalAnsi(strArgs);
                IntPtr lpPtr = Marshal.AllocHGlobal(Marshal.SizeOf(cd));
                Marshal.StructureToPtr(cd, lpPtr, true);
                Process[] plist = Process.GetProcessesByName("SuperPutty");
                foreach (Process spProcess in plist)
                {
                    WinAPI.SendMessage(spProcess.MainWindowHandle, 0x004A, 0, lpPtr);
                }
                Marshal.FreeHGlobal(lpPtr);
            }
            else
            {
            
#if DEBUG
            	Logger.OnLog += delegate(string logMessage)
           		{
                	Console.WriteLine(logMessage);
            	};
#endif

            	Application.EnableVisualStyles();
            	Application.SetCompatibleTextRenderingDefault(false);
            	Application.Run(new frmSuperPutty(args));
            	GC.KeepAlive(mutex);
            }
        }
    }
}
