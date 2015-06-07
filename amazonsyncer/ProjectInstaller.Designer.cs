namespace MTAService
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
            this.MTAServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.MTAServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // MTAServiceProcessInstaller
            // 
            this.MTAServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.MTAServiceProcessInstaller.Password = null;
            this.MTAServiceProcessInstaller.Username = null;
            this.MTAServiceProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.MTAServiceProcessInstaller_AfterInstall);
            // 
            // MTAServiceInstaller
            // 
            this.MTAServiceInstaller.Description = "MTAService";
            this.MTAServiceInstaller.ServiceName = "MTAService";
            this.MTAServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.MTAServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.MTAServiceInstaller_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.MTAServiceProcessInstaller,
            this.MTAServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller MTAServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller MTAServiceInstaller;
    }
}