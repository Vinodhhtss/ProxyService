using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Xml.Linq;

namespace ProxyServiceDBAppln
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private string ConfigFileName
        {
            get { return "Config.xml"; }
        }

        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void serviceDataAnalyzer_AfterInstall(object sender, InstallEventArgs e)
        {
            try
            {
                string tempMSIPath = Context.Parameters["SrcDir"].Replace(@"\\", @"\");
                string pathOfConfigFile = tempMSIPath + ConfigFileName;
                if (File.Exists(pathOfConfigFile))
                {
                    XElement xmlDoc = XElement.Load(pathOfConfigFile);
                    HTTPDataAnalyzer.ConfigHandler.SaveConfigFile(xmlDoc.ToString());
                }
                else
                {
                    throw new InstallException("Unable to copy \"Config.xml\" to destination folder");
                }
            }
            catch (Exception)
            {
                // System.Windows.Forms.MessageBox.Show(ex.Message, "Installation Error");
                throw;
            }
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            base.OnBeforeInstall(savedState);
          //  ClearAppData();
        }

        private void serviceDataAnalyzer_BeforeUninstall(object sender, InstallEventArgs e)
        {
            try
            {
                ServiceController sc = new ServiceController(serviceDataAnalyzer.ServiceName);
                if (sc.CanStop)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    sc.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message + " " + ex.StackTrace);
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            ClearAppData();
        }

        protected override void OnAfterRollback(IDictionary savedState)
        {
            base.OnAfterRollback(savedState);
            Util.SafeDeleteDirectory();
            ClearAppData();
        }

        private void serviceDataAnalyzer_AfterUninstall(object sender, InstallEventArgs e)
        {
            try
            {


                //if (pathtodelete != null && Directory.Exists(pathtodelete))
                //{
                //    foreach (var file in Directory.GetFiles(pathtodelete))
                //    {

                //        if (!file.Contains(System.Reflection.Assembly.GetAssembly(typeof(ProjectInstaller)).GetName().Name))
                //        {
                //            Util.SafeDeleteFile(file);
                //        }
                //    }
                //    foreach (var directory in Directory.GetDirectories(pathtodelete))
                //    {
                //        Util.SafeDeleteDirectory(directory);
                //    }
                //}
                Util.SafeDeleteDirectory();
                ClearAppData();
            }
            catch
            {
            }
        }

        private void ClearAppData()
        {
            try
            {
                string tempProxyAppPath = System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "ProxyService");
                if (tempProxyAppPath != null && Directory.Exists(tempProxyAppPath))
                {
                    foreach (var directory in Directory.GetDirectories(tempProxyAppPath))
                    {
                        if (Path.GetFileName(directory) == "TestConfig" || Path.GetFileName(directory) == "ServerRecords" || Path.GetFileName(directory) == "ConsoleServerLog")
                        {
                            continue;
                        }
                        try
                        {
                            Directory.Delete(directory, true);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
