using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RevitMasterUI
{
    class Config
    {
        public string RevitPath { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string RevitAddinPath { get; set; } = string.Empty;
    }

    class Utils
    {
        public static Config LoadConfigXml(string path)
        {
            Config config = new Config();
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlNode[] xmlNodes = new XmlNode[3];
            xmlNodes[0] = doc.SelectSingleNode("/Config/RevitPath");
            xmlNodes[1] = doc.SelectSingleNode("/Config/FilePath");
            xmlNodes[2] = doc.SelectSingleNode("/Config/RevitAddinPath");
            if (xmlNodes[0] != null)
            {
                config.RevitPath = xmlNodes[0].InnerText.Trim();
            }
            if (xmlNodes[1] != null)
            {
                config.FilePath = xmlNodes[1].InnerText.Trim();
            }
            if (xmlNodes[2] != null)
            {
                config.RevitAddinPath = xmlNodes[2].InnerText.Trim();
            }
            return config;
        }

        public static void SaveConfigXml(Config config, string path)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlDeclaration xmldec = xmldoc.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlElement rootNode = xmldoc.CreateElement("Config");
            XmlNode[] xmlNodes = new XmlNode[3];
            xmlNodes[0] = xmldoc.CreateElement("RevitPath");
            xmlNodes[0].InnerText = config.RevitPath;
            xmlNodes[1] = xmldoc.CreateElement("FilePath");
            xmlNodes[1].InnerText = config.FilePath;
            xmlNodes[2] = xmldoc.CreateElement("RevitAddinPath");
            xmlNodes[2].InnerText = config.RevitAddinPath;
            xmldoc.AppendChild(xmldec);
            xmldoc.AppendChild(rootNode);
            foreach(var n in xmlNodes) rootNode.AppendChild(n);
            xmldoc.Save(path);
        }
    }
}
