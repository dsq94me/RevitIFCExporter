using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Configuration;

namespace SocketRequest
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Revit starting...");

			//string revitPath = GetSubkeyValue(args[0], "InstallationLocation");
			try
			{
				//if (revitPath != null)
				//{
				//revitPath += "Revit.exe";

				string revitPath = args[0];
				Process[] pname = Process.GetProcessesByName("revit");
				if (pname == null || pname.Count() == 0)
					Process.Start(revitPath);

				Console.WriteLine("Wait for Revit IFC Exporter ready...");
				Int32 port = 13000;
				TcpClient client = new TcpClient();

				bool bIFCExporterReady = false;
				while (!bIFCExporterReady)
				{
					try
					{
						client.Connect("127.0.0.1", port);
						bIFCExporterReady = true;
					}
					catch (Exception e)
					{
						Thread.Sleep(1000);
					}
				}

				Byte[] ifcPath = System.Text.Encoding.ASCII.GetBytes(args[1]);
				NetworkStream stream = client.GetStream();

				// Send the message to the connected TcpServer. 
				stream.Write(ifcPath, 0, ifcPath.Length);

				Console.WriteLine("Sent IFC path : {0}", args[1]);
				Byte[] data = new Byte[4096];

				// String to store the response ASCII representation.
				String responseData = String.Empty;

				// Read the first batch of the TcpServer response bytes.
				Int32 bytes = stream.Read(data, 0, data.Length);
				responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
				Console.WriteLine("Received: {0}", responseData);

				// Close everything.
				stream.Close();
				client.Close();

				Process[] proc = Process.GetProcessesByName("revit");
				proc[0].Kill();
				//}
			}
			catch (ArgumentNullException e)
			{
				Console.WriteLine("ArgumentNullException: {0}", e);
			}
			catch (SocketException e)
			{
				Console.WriteLine("SocketException: {0}", e);
			}
		}

		static string GetSubkeyValue(string reg_path_key, string value_name)
		{
			using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
			using (var key = hklm.OpenSubKey(reg_path_key))
			{
				if (key != null)
				{
					foreach (string subkey_name in key.GetSubKeyNames())
					{
						using (RegistryKey subkey = key.OpenSubKey(subkey_name))
						{
							if (subkey.GetValue("InstallationLocation") != null)
								return subkey.GetValue("InstallationLocation") as string;
						}
					}
				}
			}
			return null;
		}
	}
}