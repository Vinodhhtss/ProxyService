namespace ProxyServiceDBAppln
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceDataAnalyzer = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller
            //
            this.serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstaller.Password = null;
            this.serviceProcessInstaller.Username = null;
            // 
            // serviceDataAnalyzer
            // 
            this.serviceDataAnalyzer.Description = "Analyzing HTTP Data";
            this.serviceDataAnalyzer.ServiceName = "DataAnalyzer";
            this.serviceDataAnalyzer.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.serviceDataAnalyzer.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.serviceDataAnalyzer_AfterInstall);
            this.serviceDataAnalyzer.BeforeUninstall += new System.Configuration.Install.InstallEventHandler(this.serviceDataAnalyzer_BeforeUninstall);
            this.serviceDataAnalyzer.AfterUninstall +=  new System.Configuration.Install.InstallEventHandler(this.serviceDataAnalyzer_AfterUninstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller,
            this.serviceDataAnalyzer});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller serviceDataAnalyzer;
    }
}