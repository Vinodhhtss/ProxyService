using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace ProxyServiceAppln
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public string AppDataFolder
        {
            get { return "ProxyService"; }
        }

        public string WebRootFolder
        {
            get { return "ProxyUI"; }
        }

        public ProjectInstaller()
        {
            // Instantiate installers for process and services.
            InitializeComponent();
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
            try
            {
                string installationPath = Context.Parameters["targetdir"];
                if (Directory.Exists(installationPath))
                {
                    DirectoryInfo dir = new DirectoryInfo(installationPath);
                    dir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }
            }
            catch (Exception ex)
            {
                // ProjectInstaller.Logger.WriteLogException(ex);
            }
            string tempProxyUIPath = System.IO.Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ProxyUIFiles");
            if (tempProxyUIPath != null && Directory.Exists(tempProxyUIPath))
            {
                //TO copy the Proxy UI related files to Comman App Data
                string strWbroot = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                                   AppDataFolder), WebRootFolder);

                Util.CopyFilesAndFolders(tempProxyUIPath, strWbroot);
                Util.SafeDeleteDirectory(tempProxyUIPath);
            }
        }


        private void serviceInstaller1_BeforeUninstall(object sender, InstallEventArgs e)
        {
            try
            {
                ServiceController sc = new ServiceController(serviceInstaller.ServiceName);
                if (sc.CanStop)
                {
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    sc.Close();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
