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
				nProcessed += RevitApplicationDB.ExportRfa2Ifc(ifcPath);
				string result = string.Format("{0} rvt files exported into ifc files", nProcessed);
				RevitApplicationDB.responseQueue.Enqueue(result);
			}
		}

		public string GetName()
		{
			return "ExtEvtHandler4ExportRvtFiles2IFCByPath";
		}
	}

    public class NewFamilyLoadOption : IFamilyLoadOptions
    {
        public virtual bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
        {
            overwriteParameterValues = false;
            return true;
        }
        public virtual bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
        {
            overwriteParameterValues = false;
            source = FamilySource.Family;
            return true;
        }
    }

    public class RevitApplicationDB : Autodesk.Revit.DB.IExternalDBApplication
	{
		static public ConcurrentQueue<string> requestQueue = new ConcurrentQueue<string>();
		static public ConcurrentQueue<string> responseQueue = new ConcurrentQueue<string>();

        static public List<string> familySymbolNames = new List<string>();
        static public Family familyOut;

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

		static public int ExportRfa2Ifc(string ifcPath)
		{
			int nProcessed = 0;
			var rvtFiles = Directory.EnumerateFiles(ifcPath, "*.rfa", SearchOption.AllDirectories);
			string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
			FileInfo fi = new FileInfo(dllPath);
			string configPath = Path.Combine(fi.DirectoryName, "IfcExportConfig.xml");
			if (!File.Exists(configPath))
			{
				MessageBox.Show(string.Format("Cannot find file: {0}", configPath));
				return nProcessed;
			}
			foreach (string rfaFile in rvtFiles)
			{
				Document doc;
				try
				{
					string fileNameWithExt = rfaFile.Substring(ifcPath.Length + 1);
					string fileName = fileNameWithExt.Substring(0, fileNameWithExt.Length - ".rfa".Length);
					doc = _app.OpenDocumentFile(rfaFile);
					try
					{
						ExportConfig exportConfig = ExportConfig.Create(configPath, doc);
						if (exportConfig != null)
						{
							using (Transaction t = new Transaction(doc, "rfa2ifc"))
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
								//doc.Export(ifcPath, fileName + ".ifc", ifcOptions);                                
                                CreateContainerDocAndExportRfa(rfaFile, ifcPath, fileName + ".ifc", ifcOptions);
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
		static public int ExportRvt2Ifc(string ifcPath)
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
			foreach (string rvtFile in rvtFiles)
			{
				Document doc;
				try
				{
					string fileNameWithExt = rvtFile.Substring(ifcPath.Length + 1);
					string fileName = fileNameWithExt.Substring(0, fileNameWithExt.Length - ".rvt".Length);
					doc = _app.OpenDocumentFile(rvtFile);
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

        static void CreateContainerDocAndExportRfa(string rfaFile, string ifcFilePath, string ifcFileName, IFCExportOptions ifcOptions)
        {            
            Document newDoc = _app.NewProjectDocument(_app.DefaultProjectTemplate);
            NewFamilyLoadOption familyLoadOptions = new NewFamilyLoadOption();
            Transaction trans = new Transaction(newDoc);
            trans.Start("LoadFamily");
            try
            {
                newDoc.LoadFamily(rfaFile, familyLoadOptions, out familyOut);

                FamilyPlacementType ftp = familyOut.FamilyPlacementType;

                trans.Commit();
            }
            catch (Exception ex)
            {
                trans.RollBack();
                MessageBox.Show("Can't parse the family file!");
                return;
            }
            foreach (ElementId eleId in familyOut.GetFamilySymbolIds())
            {
                FamilySymbol currentFamilySymbol = newDoc.GetElement(eleId) as FamilySymbol;
                familySymbolNames.Add(currentFamilySymbol.Name);
            }

            ExportRfa2Ifc(newDoc, ifcFilePath, ifcFileName, ifcOptions);
        }

        static void ExportRfa2Ifc(Document newDoc, string ifcFilePath, string ifcFileName, IFCExportOptions ifcOptions)
        {
            string familyOutName = familyOut.Name;
            IList<String> filesTobeDeleted = new List<String>();
            int invalidCount = 0;
            int invalidIndex = -1;
            char[] invalidChars = new char[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };

            if (newDoc != null)
            {
                SaveAsOptions saveAsOptions = new SaveAsOptions();
                saveAsOptions.OverwriteExistingFile = true;

                //create family folder
                if (!Directory.Exists(ifcFilePath + familyOutName + "\\"))
                {
                    Directory.CreateDirectory(ifcFilePath + familyOutName + "\\");
                }
                //expor ifc file
                FamilyInstance famInstance = null;
                FilteredElementCollector filter = new FilteredElementCollector(newDoc);
                IList<Element> eles = filter.OfClass(typeof(Autodesk.Revit.DB.View3D)).ToElements();
                Autodesk.Revit.DB.View3D view3D = null;
                foreach (Element ele in eles)
                {
                    if (!(ele as Autodesk.Revit.DB.View3D).IsTemplate)
                    {
                        view3D = ele as Autodesk.Revit.DB.View3D;
                        break;
                    }
                }

                if (null == view3D)
                {
                    Transaction viewTrans = new Transaction(newDoc, "view");
                    viewTrans.Start();
                    ViewFamilyType viewFamilyType3D
                    = new FilteredElementCollector(newDoc)
                      .OfClass(typeof(ViewFamilyType))
                      .Cast<ViewFamilyType>()
                      .FirstOrDefault<ViewFamilyType>(
                        x => ViewFamily.ThreeDimensional
                          == x.ViewFamily);

                    view3D = View3D.CreateIsometric(
                      newDoc, viewFamilyType3D.Id);
                    viewTrans.Commit();
                }

                Transaction tran = new Transaction(newDoc);
                foreach (String familySymbolName in familySymbolNames)
                {
                    tran.Start("DeleteElement");
                    if (famInstance != null)
                    {
                        newDoc.Delete(famInstance.Id);
                    }
                    tran.Commit();

                    FamilySymbol familySymbol = null;
                    CheckFamilySymbol(familyOut, familySymbolName, ref familySymbol);
                    if (familySymbol != null)
                    {                                           
                        string symbolName = familySymbol.Name;
                        invalidIndex = -1;
     
                        invalidIndex = familySymbolName.IndexOfAny(invalidChars);
                        while (invalidIndex != -1)
                    {
                        symbolName = symbolName.Remove(invalidIndex);
                        invalidCount++;

                        invalidIndex = symbolName.IndexOfAny(invalidChars);
                    }
                        if (invalidCount > 0)
                        {
                            MessageBox.Show("The familySymbol name contains the illegal character!\n\"/\\:,*?\"<>|\", they will be removed.");
                        }
     
                        try
                        {
                            tran.Start("CreateFamilyInstance");

                            if (!familySymbol.IsActive)
                                familySymbol.Activate();

                            if (familyOut.FamilyPlacementType == FamilyPlacementType.OneLevelBased)
                            {
                                famInstance = newDoc.Create.NewFamilyInstance(XYZ.Zero, familySymbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            }
                            else
                                if (familyOut.FamilyPlacementType == FamilyPlacementType.OneLevelBasedHosted)
                            {
                                // Build a location line for the wall creation
                                XYZ start = new XYZ(-10, 0, 0);
                                XYZ end = new XYZ(10, 0, 0);
                                Line geomLine = Line.CreateBound(start, end);
                                double elevation = 20.0;
                                Level lvl = Level.Create(newDoc, elevation);
                                Wall wall = Wall.Create(newDoc, geomLine, lvl.Id, true);
                                IList<ElementId> elementIdSet = new List<ElementId>();
                                elementIdSet.Add(wall.Id);
                                view3D.HideElements(elementIdSet);
                                famInstance = newDoc.Create.NewFamilyInstance(XYZ.Zero, familySymbol, wall, lvl, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                            }
                            else
                            {
                                if (tran.GetStatus() == TransactionStatus.Started)
                                {
                                    tran.RollBack();
                                }
                                MessageBox.Show("Only for the OneLevelBased or OneLevelBasedHosted family!");
                                break;
                            }
                        
                            tran.Commit();
                            if (famInstance != null)
                            {
                                newDoc.SaveAs(ifcFilePath + familyOutName + "\\" + symbolName + ".rvt", saveAsOptions);
                                filesTobeDeleted.Add(ifcFilePath + familyOutName + "\\" + symbolName + ".rvt");
                                tran.Start("ExportIFC");
                                IFCExportOptions ifcExp = new IFCExportOptions();
                                ifcExp.FilterViewId = view3D.Id;
                                bool bl = newDoc.Export(ifcFilePath + familyOutName + "\\", symbolName, ifcExp);
                                tran.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            if (tran.GetStatus() == TransactionStatus.Started)
                            {
                                tran.RollBack();
                            }
                            MessageBox.Show(ex.ToString());
                        }
                    }
                }
                // close family file
                newDoc.Close(false);
                //delete the temporary revit files
                foreach (String filePath in filesTobeDeleted)
                {
                    File.Delete(filePath);
                }
            }
        }

        public static bool LoadFamilyStr(Document doc, String familyPath, String familyName, ref Family familyOut)
        {

            NewFamilyLoadOption myOptions = new NewFamilyLoadOption();
            bool hasLoadFamily = false;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> allFamilies = collector.OfClass(typeof(Family)).ToElements();
            for (int i = 0; i < allFamilies.Count; i++)
            {
                Family family = allFamilies[i] as Family;
                if (allFamilies[i].Name == familyName)
                {
                    familyOut = family;
                    hasLoadFamily = true;
                    break;
                }
            }
            if (!hasLoadFamily)
            {
                if (!doc.LoadFamily(familyPath + familyName + ".rfa", myOptions, out familyOut))
                {
                    return false;
                }
            }
            return true;
        }


        public static bool CheckFamilySymbol(Family family, string beamTypeStr, ref FamilySymbol fmSymbol)
        {
            ISet<ElementId> familySymbolIds = family.GetFamilySymbolIds();
            bool IsTrue = false;
            if (familySymbolIds.Count > 0)
            {
                // Get family symbols which is contained in this family
                foreach (ElementId id in familySymbolIds)
                {
                    FamilySymbol familySymbol = family.Document.GetElement(id) as FamilySymbol;
                    // Get family symbol name

                    if (familySymbol.Name == beamTypeStr)
                    {
                        IsTrue = true;
                        fmSymbol = familySymbol;
                        return IsTrue;                        
                    }
                }
            }
            return IsTrue;
            /*
                        FamilySymbolSetIterator famSymSetIte = family.Symbols.ForwardIterator();
                        bool IsTrue = false;
                        famSymSetIte.Reset();
                        while (famSymSetIte.MoveNext())
                        {
                            if ((((FamilySymbol)(famSymSetIte.Current))).Name == beamTypeStr)
                            {
                                familySymbol = (FamilySymbol)(famSymSetIte.Current);
                                IsTrue = true;
                                return IsTrue;
                            }
                        }
                        famSymSetIte.Reset();
                        famSymSetIte.MoveNext();
                        familySymbol = (FamilySymbol)(famSymSetIte.Current);
                        return IsTrue;
            */
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
