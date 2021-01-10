using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace RevitMasterUI
{
    public partial class FormExporter : Form
    {
        public FormExporter()
        {
            InitializeComponent();
        }

        Config _Config;
        string _ConfigPath;

        private void FormExporter_Load(object sender, EventArgs e)
        {
            try
            {
                string configPath = Path.Combine(Environment.CurrentDirectory, @"bin\config.xml");
                _ConfigPath = configPath;
                if (!File.Exists(configPath))
                {
                    MessageBox.Show("cannot find config.xml!");
                    Close();
                }
                Config config = Utils.LoadConfigXml(configPath);
                _Config = config;
                tb_RevitPath.Text = config.RevitPath;
                tb_FilePath.Text = config.FilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_ChangeRevitPath_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Please select the installation directory of revit.";
            dialog.InitialDirectory = "c:\\";
            dialog.Filter = "EXE files|*.exe"; ;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tb_RevitPath.Text = dialog.FileName;
            }

        }

        private void btn_ChangeFilePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Please select the directory of revit files.";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tb_FilePath.Text = dialog.SelectedPath;
            }
        }

        private void btnIfcConfig_Click(object sender, EventArgs e)
        {
            string configPath = Path.Combine(_Config.RevitAddinPath, "IfcExportConfig.xml");
            if (File.Exists(configPath))
            {
                Process.Start(configPath);
            }
            else
            {
                MessageBox.Show("Cannot find IFC setting file: " + configPath);
            }
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            try
            {
                _Config.RevitPath = tb_RevitPath.Text;
                _Config.FilePath = tb_FilePath.Text;

                if (_Config.RevitPath.Length == 0)
                    MessageBox.Show("Please select the installation directory of revit.");

                if (_Config.FilePath.Length == 0)
                    MessageBox.Show("Please select the installation directory of revit.");

                if (_Config != null && _Config.RevitPath.Length > 0 && _Config.FilePath.Length > 0 && _ConfigPath.Length > 0)
                {
                    Utils.SaveConfigXml(_Config, _ConfigPath);
                    string exePath = Path.Combine(Environment.CurrentDirectory, @"bin\RevitMaster.exe");
                    if (File.Exists(exePath))
                    {
                        string args = string.Format("\"{0}\" \"{1}\"", _Config.RevitPath, _Config.FilePath);
                        Process process = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo(exePath, args.Trim());
                        process.StartInfo = startInfo;
                        process.StartInfo.UseShellExecute = false;
                        process.Start();

                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Cannot find RevitMaster.exe");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
