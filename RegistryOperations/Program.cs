using System;

namespace AddRegisterEntriesInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            AddRegisterEntriesInstaller.RegistryEditor regEditor = new AddRegisterEntriesInstaller.RegistryEditor();
            try
            {
                ////regEditor.Logger.Info("Enter Main");
                regEditor.Start(args[0]);
                ////regEditor.Logger.Info("Exit Main");
            }
            catch (Exception ex)
            {
                //regEditor.Logger.Error(ex);
            }
            finally
            {
            }
        }
    }
}
