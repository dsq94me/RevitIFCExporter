using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Timers;
using System.Net.Sockets;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM.IFC.Export.UI;
using System.Xml;
using System.Windows.Forms;

namespace IFCExporter
{
	public class ExtEvtHandler4ExportRvtFiles2IFCByPath : IExternalEventHandler
	{
		public void Execute(UIApplication app)
		{
			if (RevitApplicationDB.requestQueue.Count() > 0)
			{
				string ifcPath;
				bool res = RevitApplicationDB.requestQueue.TryDequeue(out ifcPath);
				int nProcessed = RevitApplicationDB.ExportRvt2Ifc(ifcPath);
				string result = string.Format("{0} rvt files exported into ifc files", nProcessed);
				RevitApplicationDB.responseQueue.Enqueue(result);
			}
		}

		public string GetName()
		{
			return "ExtEvtHandler4ExportRvtFiles2IFCByPath";
		}
	}

	public class RevitApplicationDB : Autodesk.Revit.DB.IExternalDBApplication
	{
		static public ConcurrentQueue<string> requestQueue = new ConcurrentQueue<string>();
		static public ConcurrentQueue<string> responseQueue = new ConcurrentQueue<string>();

		public static ExternalEvent s_exEvtHandler4ExportRvtFiles2IFCByPath;
		NetworkStream networkStream = null;

		static Autodesk.Revit.ApplicationServices.Application _app = null;

		public Autodesk.Revit.DB.ExternalDBApplicationResult OnShutdown(Autodesk.Revit.ApplicationServices.ControlledApplication application)
		{
			return Autodesk.Revit.DB.ExternalDBApplicationResult.Succeeded;
		}

		public Autodesk.Revit.DB.ExternalDBApplicationResult OnStartup(Autodesk.Revit.ApplicationServices.ControlledApplication application)
		{
			application.ApplicationInitialized += new EventHandler<Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs>(application_ApplicationInitialized);
			return Autodesk.Revit.DB.ExternalDBApplicationResult.Succeeded;
		}

		void application_ApplicationInitialized(object sender, Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs e)
		{
			_app = sender as Autodesk.Revit.ApplicationServices.Application;

			ExtEvtHandler4ExportRvtFiles2IFCByPath handler = new ExtEvtHandler4ExportRvtFiles2IFCByPath();
			s_exEvtHandler4ExportRvtFiles2IFCByPath = ExternalEvent.Create(handler);
			StartProcessing();
		}

		private async void StartProcessing()
		{
			await Task.Run(() =>
			{
				ReceiveAndRespondRequests();
			});
		}

		//private async void Process()
		private void ReceiveAndRespondRequests()
		{
			Int32 port = 13000;
			System.Net.IPAddress localAddr = System.Net.IPAddress.Parse("127.0.0.1");

			TcpListener serverSocket = new TcpListener(localAddr, port);
			int requestCount = 0;
			TcpClient clientSocket = default(TcpClient);
			serverSocket.Start();
			Console.WriteLine(" >> Server Started");

			while ((true))
			{
				try
				{
					clientSocket = serverSocket.AcceptTcpClient();
					Console.WriteLine(" >> Accept connection from client");
					requestCount = 0;

					requestCount = requestCount + 1;
					networkStream = clientSocket.GetStream();
					byte[] bytesFrom = new byte[clientSocket.ReceiveBufferSize];
					networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
					string ifcPath = System.Text.Encoding.ASCII.GetString(bytesFrom);
					ifcPath= ifcPath.Substring(0, ifcPath.IndexOf('\0'));

					string response;
					if (ifcPath != "quit")
					{
						Console.WriteLine(" >> Data from client - " + ifcPath);
						requestQueue.Enqueue(ifcPath);
						s_exEvtHandler4ExportRvtFiles2IFCByPath.Raise();

						while (responseQueue.Count() == 0)
							Thread.Sleep(1000);

						bool res = responseQueue.TryDequeue(out response);
						Reply2Client(ifcPath, response);
					}
					else
					{
						string replyStr = "exit socket loop ";
						Reply2Client(ifcPath, replyStr);
						clientSocket.Close();
						break;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
			serverSocket.Stop();
			Console.WriteLine(" >> exit");
		}

		public void Reply2Client(string path, string message)
		{
			string serverResponse;
			serverResponse = string.Format("Request for \'{0}\' processed, result is {1} ", path, message);
			Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);

			networkStream.Write(sendBytes, 0, sendBytes.Length);
			networkStream.Flush();
			Console.WriteLine(" >> " + serverResponse);
		}

		static public int ExportRvtRfa2Ifc(string ifcPath)
		{
			int nProcessed = 0;
			string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			FileInfo fi = new FileInfo(dllPath);
			string configPath = Path.Combine(fi.DirectoryName, "IfcExportConfig.xml");
			if (!File.Exists(configPath))
			{
				MessageBox.Show(string.Format("Cannot find file: {0}", configPath));
				return nProcessed;
			}

			var rvtFiles = Directory.EnumerateFiles(ifcPath, "*.rvt", SearchOption.AllDirectories);

			var rvtFiles = Directory.EnumerateFiles(ifcPath, "*.rfa", SearchOption.AllDirectories);


			return nProcessed;
		}

		static private int ExportRfa2Ifc(string ifcPath, string configPath)
		{
			int nProcessed = 0;

			var rfaFiles = Directory.EnumerateFiles(ifcPath, "*.rfa", SearchOption.AllDirectories);
			foreach (string modelFile in rfaFiles)
			{
				Document doc;
				try
				{
					string fileNameWithExt = modelFile.Substring(ifcPath.Length + 1);
					string fileName = fileNameWithExt.Substring(0, fileNameWithExt.Length - ".rvt".Length);
					doc = _app.OpenDocumentFile(fileName);
					try
					{
						ExportConfig exportConfig = ExportConfig.Create(configPath, doc);
						if (exportConfig != null)
						{
							using (Transaction t = new Transaction(doc, "rvt2ifc"))
							{
								t.Start();
								IFCExportOptions ifcOptions = new IFCExportOptions();
								IFCExportConfiguration config = IFCExportConfiguration.GetInSession();
								config.ActivePhaseId = exportConfig.PhaseToExport != null ?
										exportConfig.PhaseToExport : new ElementId(-1);
								config.ExportInternalRevitPropertySets = exportConfig.ExportRevitPropertySets;
								config.TessellationLevelOfDetail = exportConfig.LevelOfDetail;
								config.Use2DRoomBoundaryForVolume = exportConfig.Use2DRoomBoundaryForVolume;
								config.IncludeSiteElevation = exportConfig.IncludeIFCSiteElevation;
								config.UpdateOptions(ifcOptions, new ElementId(-1));
								doc.Export(ifcPath, fileName + ".ifc", ifcOptions);
								t.Commit();
								nProcessed++;
							}
						}
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
					}
					doc.Close(false);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}
			return nProcessed;
		}

		static public int ExportRvtIfc(string ifcPath)
		{
			int nProcessed = 0;
			var rvtFiles = Directory.EnumerateFiles(ifcPath, "*.rvt", SearchOption.AllDirectories);
			string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			FileInfo fi = new FileInfo(dllPath);
			string configPath = Path.Combine(fi.DirectoryName, "IfcExportConfig.xml");
			if (!File.Exists(configPath))
			{
				MessageBox.Show(string.Format("Cannot find file: {0}", configPath));
				return nProcessed;
			}

			return nProcessed;
		}
	}

    class ExportConfig
    {
        public ElementId PhaseToExport { get; private set; }
		public bool ExportRevitPropertySets { get; private set; }
		public double LevelOfDetail { get; private set; }
		public bool Use2DRoomBoundaryForVolume { get; private set; }
		public bool IncludeIFCSiteElevation { get; private set; }

		public static ExportConfig Create(string path, Document doc)
		{
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(path);
			XmlNode[] xmlNodes = new XmlNode[5];
			xmlNodes[0] = xmlDoc.SelectSingleNode("/Config/PhaseToExport");
			xmlNodes[1] = xmlDoc.SelectSingleNode("/Config/ExportRevitPropertySets");
			xmlNodes[2] = xmlDoc.SelectSingleNode("/Config/LevelOfDetail");
			xmlNodes[3] = xmlDoc.SelectSingleNode("/Config/Use2DRoomBoundaryForVolume");
			xmlNodes[4] = xmlDoc.SelectSingleNode("/Config/IncludeIFCSiteElevation");
			ExportConfig config = new ExportConfig();
			foreach (var xmlNode in xmlNodes)
			{
				if (xmlNode == null)
				{
					MessageBox.Show(string.Format("Cannot find the XML node: {0}", xmlNode.LocalName));
					return null;
				}
				if (xmlNode.InnerText.Trim().Length == 0)
				{
					MessageBox.Show(string.Format("The XML node value in null: {0}", xmlNode.LocalName));
					return null;
				}
			}

			// PhaseToExport
			string phaseName = xmlNodes[0].InnerText.Trim();
			if (phaseName == "Default phase to export")
			{
				config.PhaseToExport = new ElementId(-1);
			}
			else
			{
				var phaseId = GetPhaseIdByName(doc, phaseName);
				if (phaseId == null)
				{
					MessageBox.Show(string.Format("Cannot find the phase in '{1}': {0}", phaseName, doc.Title));
					return null;
				}
				config.PhaseToExport = phaseId;
			}

			// ExportRevitPropertySets
			bool bo;
			if (!bool.TryParse(xmlNodes[1].InnerText.Trim(), out bo))
			{
				MessageBox.Show(string.Format("ExportRevitPropertySets value in invalid"));
				return null;
			}
			config.ExportRevitPropertySets = bo;

			// LevelOfDetail
			string levelOfDetail = xmlNodes[2].InnerText.Trim();
			string[] levels = new string[] { "Extra Low", "Low", "Medium", "High" };
            if (!levels.Contains(levelOfDetail))
            {
				MessageBox.Show(string.Format("LevelOfDetail value in invalid"));
                return null;
            }
			if (levelOfDetail == "Extra Low") config.LevelOfDetail = 0.25;
			else if(levelOfDetail == "Low") config.LevelOfDetail = 0.50;
			else if (levelOfDetail == "Medium") config.LevelOfDetail = 0.75;
			else if (levelOfDetail == "High") config.LevelOfDetail = 1.00;

			// Use2DRoomBoundaryForVolume
			if (!bool.TryParse(xmlNodes[3].InnerText.Trim(), out bo))
			{
				MessageBox.Show(string.Format("Use2DRoomBoundaryForVolume value in invalid"));
				return null;
			}
			config.Use2DRoomBoundaryForVolume = bo;

			// IncludeIFCSiteElevation
			if (!bool.TryParse(xmlNodes[4].InnerText.Trim(), out bo))
			{
				MessageBox.Show(string.Format("IncludeIFCSiteElevation value in invalid"));
				return null;
			}
			config.IncludeIFCSiteElevation = bo;

			return config;
		}

        private static ElementId GetPhaseIdByName(Document doc, string name)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Phases).OfClass(typeof(Phase));
			foreach (var elem in collector)
			{
				if (elem.Name == name) return elem.Id;
			}
			return null;
        }
    }
}
