using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
namespace CertificateManagement
{
    public class CertMaker
    {
        private static CertificateProvider oCertProvider = null;

        static CertMaker()
        {
            if (CertMaker.oCertProvider == null)
            {
                CertMaker.oCertProvider = new CertificateProvider();
            }
        }

        public static bool RemoveGeneratedCerts()
        {
            return CertMaker.oCertProvider.ClearCertificateCache();
        }
        
        public static void RemoveGeneratedPersonalCerts()
        {
             CertMaker.oCertProvider.ClearPersonalCertificate();
        }

        public static X509Certificate2 GetRootCertificate()
        {
            return CertMaker.oCertProvider.GetRootCertificate();
        }

        public static X509Certificate2 FindCert(string sHostname)
        {
            return CertMaker.oCertProvider.GetCertificateForHost(sHostname);
        }

        public static bool CreateRootCert()
        {
            return CertMaker.oCertProvider.CreateRootCertificate();
        }

        public static void CreateAndExportRootCert(string exportPath)
        {
            CertMaker.oCertProvider.CreateRootCertificate();
            X509Certificate2 _certificate = new X509Certificate2(CertMaker.GetRootCertificate());

            X509Store x509Store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            x509Store.Open(OpenFlags.ReadWrite);
            try
            {
                x509Store.Add(_certificate);
                if(exportPath != string.Empty)
                File.WriteAllBytes(exportPath, _certificate.Export(X509ContentType.Cert));
            }
            finally
            {
                x509Store.Close();
            }
        }

        public static void DoDispose()
        {
            if (CertMaker.oCertProvider == null)
            {
                return;
            }

            IDisposable disposable = CertMaker.oCertProvider as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
            CertMaker.oCertProvider = null;
        }
    }
}
