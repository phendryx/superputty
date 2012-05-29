/*
 * Created by SharpDevelop.
 * User: Paul Hendryx
 * Date: 11/27/2010
 * Time: 10:25 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

using Microsoft.Win32;

namespace SuperPutty.Classes
{
	/// <summary>
	/// Description of Database.
	/// </summary>
	public class Database
	{
		private SQLiteConnection _conn;
		
		public Database()
		{
		}

		#region Static Methods
		public static Boolean GetBooleanKey(string key, bool defaultValue)
		{
			bool value = defaultValue;
			
			Database d = new Database();
			d.Open();
			
			string v = d.GetKey(key);
			if (v != "")
			{
				value = bool.Parse(v);
			}
			
			return value;
		}

		public static int GetIntegerKey(string key, int defaultValue)
		{
			int value = defaultValue;
			
			Database d = new Database();
			d.Open();
			
			string v = d.GetKey(key);
			if (v != "")
			{
				value = int.Parse(v);
			}
			
			return value;
		}

		public static string GetStringKey(string key, string defaultValue)
		{
			string value = defaultValue;
			
			Database d = new Database();
			d.Open();
			
			value = d.GetKey(key);

			return value;
		}
		
		public static void SetKeyStatic(string key, string value)
		{
			Database d = new Database();
			d.Open();
			
			d.SetKey(key, value);
		}
		#endregion

		#region Dynamic Methods
		public SQLiteConnection Open()
		{
        	//string db_filename = Application.StartupPath + "\\SuperPutty.db3";
            string db_filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SuperPutty.db3");
        	if (!File.Exists(db_filename))
        	{
        		SQLiteConnection.CreateFile(db_filename);
        	}
        	
        	string connection_string = "Data Source=" + db_filename + ";Version=3;";
        	_conn = new System.Data.SQLite.SQLiteConnection(connection_string);
        	_conn.Open();
        	
        	UpgradeSchema();
        	
        	return _conn;
		}
		
		public DataTable FillDataTable(string sql)
		{
			DataTable dt = new DataTable();
			
			SQLiteCommand cmd = new SQLiteCommand(sql);
			SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
			da.Fill(dt);
		
			return dt;
		}

		public string GetKey(string key)
		{
			SQLiteCommand cmd = new SQLiteCommand("select value from settings where key = '" + key + "';", _conn);
			SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
			DataTable dt = new DataTable();
			da.Fill(dt);
			string value = "";
			if (dt.Rows.Count == 1)
			{
				value = (string)dt.Rows[0][0];
			}
			
			return value;
		}
		
		public void SetKey(string key, string value)
		{
			SQLiteCommand cmd = new SQLiteCommand("select id from settings where key = '" + key + "'", _conn);
			SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
			DataTable dt = new DataTable();
			da.Fill(dt);

			if (dt.Rows.Count == 0)
			{
				cmd.CommandText = "insert into settings (key, value) values ('" + key + "', '" + value + "')";
			}
			else
			{
				cmd.CommandText = "update settings set value = '" + value + "' where key = '" + key + "'";
			}
			cmd.ExecuteNonQuery();				
		}
		
		private void UpgradeSchema()
		{
			// Settings table
			if (_conn.GetSchema("Tables").Select("Table_Name = 'settings'").Length == 0)
			{
				SQLiteCommand cmd = new SQLiteCommand("create table if not exists settings(id INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT, key varchar(255), value varchar(255));", _conn);
				cmd.ExecuteNonQuery();
				
				cmd.CommandText = "insert into settings (key, value) values ('db_version', '1.0.1');";
				cmd.ExecuteNonQuery();
			}
			
			// Settings table
			if (_conn.GetSchema("Tables").Select("Table_Name = 'sessions'").Length == 0)
			{
				string sql = "create table if not exists sessions (id INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT, auto_start boolean, host varchar(255), last_dock integer, ";
				sql += "last_path varchar(255), port integer, proto varchar(255), putty_session varchar(255));";
				SQLiteCommand cmd = new SQLiteCommand(sql, _conn);
				cmd.ExecuteNonQuery();
			}

			// Version Changes
			switch (GetKey("db_version"))
			{
				case "1.0.1":
					SetKey("db_version", "1.0.2");
					break;
			}
		}
		
		private void upgrade_1_0_1_to_1_0_2()
		{
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Jim Radford\SuperPuTTY\Settings");
            if (key != null)
            {
                string puttyExe = key.GetValue("PuTTYExe", "").ToString();
                if (File.Exists(puttyExe))
                {
					SetKey("putty_exe", puttyExe);
                }

                string pscpExe = key.GetValue("PscpExe", "").ToString();
                if (File.Exists(pscpExe))
                {
					SetKey("pscp_exe", pscpExe);
                }

                string minttyExe = key.GetValue("MinttyExe", "").ToString();
                if (File.Exists(minttyExe))
                {
                    SetKey("mintty_exe", minttyExe);
                }
            }
		}
		#endregion
	}
}
