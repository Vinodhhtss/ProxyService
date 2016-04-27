using System;
using System.Security.Cryptography.X509Certificates;

namespace HTTPDataAnalyzer.Registration
{
    public class CertHandler
    {
        public static void InstallConsoleServerCertificate(byte[] bytesOfCert)
        {
            try
            {
                if (!MachineHasPrivateCert())
                {

                    X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.MaxAllowed | OpenFlags.OpenExistingOnly);

                    X509Certificate2 cert = new X509Certificate2(bytesOfCert);

                    store.Add(cert);
                    store.Close();
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
                throw;
            }
        }

        public static bool MachineHasPrivateCert()
        {
            X509Store store = null;
            try
            {
                store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite | OpenFlags.IncludeArchived);

                // You could also use a more specific find type such as X509FindType.FindByThumbprint
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByIssuerName, ConfigHandler.Config.ServerDetails.MainServerIP, false);
                foreach (X509Certificate2 cert in col)
                {
                    if (cert.HasPrivateKey)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }
            return false;
        }

        public static void RemoveConsoleServerCertificate()
        {
            try
            {
                X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite | OpenFlags.IncludeArchived);

                // You could also use a more specific find type such as X509FindType.FindByThumbprint
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByIssuerName, ConfigHandler.Config.ServerDetails.MainServerIP, false);
                foreach (X509Certificate2 cert in col)
                {
                    if (cert.HasPrivateKey)
                    {
                        continue;
                    }
                    store.Remove(cert);
                }
                store.Close();
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
        }
    }
}
