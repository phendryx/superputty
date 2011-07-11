/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 7/10/2011
 * Time: 3:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using NUnit.Framework;

namespace SuperPutty.nUnit
{
	[TestFixture]
	public class CLI
	{
		[Test]
		public void TestNoArgs()
		{
			SessionData s = Classes.CLI.ParseCLIArguments(new string[]{});
			SessionData result = new SessionData();
			Assert.AreEqual(result.GetType(), s.GetType());
			
		}
		
		[Test]
		public void TestSShUrlArg()
		{
			string [] args = {"ssh://localhost"};
			SessionData s = Classes.CLI.ParseCLIArguments(args);
			Assert.AreEqual("localhost", s.Host);
			
		}

		[Test]
		public void TestProtoSSH()
		{
			string [] args = {"-ssh"};
			SessionData s = Classes.CLI.ParseCLIArguments(args);
			Assert.AreEqual(ConnectionProtocol.SSH, s.Proto);
		}

		[Test]
		public void TestProtoSerial()
		{
			string [] args = {"-serial"};
			SessionData s = Classes.CLI.ParseCLIArguments(args);
			Assert.AreEqual(ConnectionProtocol.Serial, s.Proto);
		}

		[Test]
		public void TestProtoTelnet()
		{
			string [] args = {"-telnet"};
			SessionData s = Classes.CLI.ParseCLIArguments(args);
			Assert.AreEqual(ConnectionProtocol.Telnet, s.Proto);
		}

		[Test]
		public void TestProtoSCP()
		{
			string [] args = {"-scp"};
			SessionData s = Classes.CLI.ParseCLIArguments(args);
			Assert.AreEqual(ConnectionProtocol.SSH, s.Proto);
			Assert.AreEqual(true, s.UseSCP);
		}

		[Test]
		public void TestProtoRaw()
		{
			string [] args = {"-raw"};
			SessionData s = Classes.CLI.ParseCLIArguments(args);
			Assert.AreEqual(ConnectionProtocol.Raw, s.Proto);
		}

		[Test]
		public void TestProtoRLogin()
		{
			string [] args = {"-rlogin"};
			SessionData s = Classes.CLI.ParseCLIArguments(args);
			Assert.AreEqual(ConnectionProtocol.Rlogin, s.Proto);
		}

		[Test]
		public void TestPort()
		{
			string [] args = {"-p", "1234"};
			SessionData s = Classes.CLI.ParseCLIArguments(args);
			Assert.AreEqual(1234, s.Port);
		}

		[Test]
		public void TestUsername()
		{
			string [] args = {"-l", "testusername"};
			SessionData s = Classes.CLI.ParseCLIArguments(args);
			Assert.AreEqual("testusername", s.Username);
		}

		[Test]
		public void TestPassword()
		{
			string [] args = {"-pw", "testpassword"};
			SessionData s = Classes.CLI.ParseCLIArguments(args);
			Assert.AreEqual("testpassword", s.Password);
		}

		[Test]
		public void TestSession()
		{
			string [] args = {"-load", "testsession"};
			SessionData s = Classes.CLI.ParseCLIArguments(args);
			Assert.AreEqual("testsession", s.PuttySession);
		}

		[TestFixtureSetUp]
		public void Init()
		{
			// TODO: Add Init code.
		}
	}
}
