using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Concurrent;
using System.Linq;

namespace CertificateManagement
{
    internal class CertificateProvider
    {
        private class CertEnrollEngine
        {
            private CertificateProvider _ParentProvider;
            private Type typeX500DN;
            private Type typeX509PrivateKey;
            private Type typeOID;
            private Type typeOIDS;
            private Type typeEKUExt;
            private Type typeRequestCert;
            private Type typeX509Extensions;
            private Type typeBasicConstraints;
            private Type typeSignerCertificate;
            private Type typeX509Enrollment;
            private object _oSharedPrivateKey;
            internal static CertificateProvider.CertEnrollEngine GetEngine(CertificateProvider ParentProvider)
            {
                try
                {
                    return new CertificateProvider.CertEnrollEngine(ParentProvider);
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message + " " + ex.StackTrace);
                }
                return null;
            }
            private CertEnrollEngine(CertificateProvider ParentProvider)
            {
                this._ParentProvider = ParentProvider;
                this.typeX500DN = Type.GetTypeFromProgID("X509Enrollment.CX500DistinguishedName", true);
                this.typeX509PrivateKey = Type.GetTypeFromProgID("X509Enrollment.CX509PrivateKey", true);
                this.typeOID = Type.GetTypeFromProgID("X509Enrollment.CObjectId", true);
                this.typeOIDS = Type.GetTypeFromProgID("X509Enrollment.CObjectIds.1", true);
                this.typeEKUExt = Type.GetTypeFromProgID("X509Enrollment.CX509ExtensionEnhancedKeyUsage");
                this.typeRequestCert = Type.GetTypeFromProgID("X509Enrollment.CX509CertificateRequestCertificate");
                this.typeX509Extensions = Type.GetTypeFromProgID("X509Enrollment.CX509Extensions");
                this.typeBasicConstraints = Type.GetTypeFromProgID("X509Enrollment.CX509ExtensionBasicConstraints");
                this.typeSignerCertificate = Type.GetTypeFromProgID("X509Enrollment.CSignerCertificate");
                this.typeX509Enrollment = Type.GetTypeFromProgID("X509Enrollment.CX509Enrollment");
            }
            public X509Certificate2 CreateCert(string sSubject, bool isRoot)
            {
                sSubject = string.Format(CertConst.SUBJECT_FORMAT, sSubject, CONFIG.sMakeCertSubjectO);

                int num = -366;
                int int32Pref = 1825;

                int privateKeyLength = 1024;
                try
                {
                    X509Certificate2 x509Certificate;
                    if (isRoot)
                    {

                        x509Certificate = this.GenerateCertificate(true, sSubject, privateKeyLength, CertConst.ALGORITHM, DateTime.Now.AddDays((double)num), DateTime.Now.AddDays((double)int32Pref), null);
                    }
                    else
                    {
                        x509Certificate = this.GenerateCertificate(false, sSubject, privateKeyLength, CertConst.ALGORITHM, DateTime.Now.AddDays((double)num), DateTime.Now.AddDays((double)int32Pref), this._ParentProvider.GetRootCertificate());
                    }

                    return x509Certificate;
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message + " " + ex.StackTrace);
                }
                return null;
            }



            private X509Certificate2 GenerateCertificate(bool bIsRoot, string sSubject, int iPrivateKeyLength, string sHashAlg, DateTime dtValidFrom, DateTime dtValidTo, X509Certificate2 oSigningCertificate)
            {
                if (bIsRoot != (null == oSigningCertificate))
                {
                    throw new ArgumentException("You must specify a Signing Certificate if and only if you are not creating a root.", "oSigningCertificate");
                }
                object obj = Activator.CreateInstance(this.typeX500DN);
                object[] array = new object[]
				{
					sSubject,
					0
				};
                this.typeX500DN.InvokeMember("Encode", BindingFlags.InvokeMethod, null, obj, array);
                object obj2 = Activator.CreateInstance(this.typeX500DN);
                if (!bIsRoot)
                {
                    array[0] = oSigningCertificate.Subject;
                }
                this.typeX500DN.InvokeMember("Encode", BindingFlags.InvokeMethod, null, obj2, array);
                object obj3 = null;
                if (!bIsRoot)
                {
                    obj3 = this._oSharedPrivateKey;
                }
                if (obj3 == null)
                {
                    obj3 = Activator.CreateInstance(this.typeX509PrivateKey);
                    array = new object[]
					{   
						"Microsoft Enhanced Cryptographic Provider v1.0"
					};
                    this.typeX509PrivateKey.InvokeMember("ProviderName", BindingFlags.PutDispProperty, null, obj3, array);
                    array[0] = 2;
                    this.typeX509PrivateKey.InvokeMember("ExportPolicy", BindingFlags.PutDispProperty, null, obj3, array);
                    array = new object[]
					{
						bIsRoot ? 2 : 1
					};
                    this.typeX509PrivateKey.InvokeMember("KeySpec", BindingFlags.PutDispProperty, null, obj3, array);
                    array[0] = iPrivateKeyLength;                   
                    this.typeX509PrivateKey.InvokeMember("Length", BindingFlags.PutDispProperty, null, obj3, array);
                    array = new object[1];
                    array[0] = true;
                    this.typeX509PrivateKey.InvokeMember("MachineContext", BindingFlags.PutDispProperty, null, obj3, array);
                    this.typeX509PrivateKey.InvokeMember("Create", BindingFlags.InvokeMethod, null, obj3, null);

                    if (!bIsRoot)
                    {
                        this._oSharedPrivateKey = obj3;
                    }
                }

                array = new object[1];
                object obj4 = Activator.CreateInstance(this.typeOID);
                array[0] = "1.3.6.1.5.5.7.3.1";
                this.typeOID.InvokeMember("InitializeFromValue", BindingFlags.InvokeMethod, null, obj4, array);
                object obj5 = Activator.CreateInstance(this.typeOIDS);
                array[0] = obj4;
                this.typeOIDS.InvokeMember("Add", BindingFlags.InvokeMethod, null, obj5, array);
                object obj6 = Activator.CreateInstance(this.typeEKUExt);
                array[0] = obj5;
                this.typeEKUExt.InvokeMember("InitializeEncode", BindingFlags.InvokeMethod, null, obj6, array);
                object obj7 = Activator.CreateInstance(this.typeRequestCert);
                array = new object[]
				{
					2,
					obj3,
					string.Empty
				};
                this.typeRequestCert.InvokeMember("InitializeFromPrivateKey", BindingFlags.InvokeMethod, null, obj7, array);
                array = new object[]
				{
					obj
				};
                this.typeRequestCert.InvokeMember("Subject", BindingFlags.PutDispProperty, null, obj7, array);
                array[0] = obj2;
                this.typeRequestCert.InvokeMember("Issuer", BindingFlags.PutDispProperty, null, obj7, array);
                array[0] = dtValidFrom;
                this.typeRequestCert.InvokeMember("NotBefore", BindingFlags.PutDispProperty, null, obj7, array);
                array[0] = dtValidTo;
                this.typeRequestCert.InvokeMember("NotAfter", BindingFlags.PutDispProperty, null, obj7, array);
                object target = this.typeRequestCert.InvokeMember("X509Extensions", BindingFlags.GetProperty, null, obj7, null);
                array = new object[]
				{
					obj6
				};
                this.typeX509Extensions.InvokeMember("Add", BindingFlags.InvokeMethod, null, target, array);
                if (bIsRoot)
                {
                    object obj8 = Activator.CreateInstance(this.typeBasicConstraints);
                    array = new object[]
					{
						"true",
						"0"
					};
                    this.typeBasicConstraints.InvokeMember("InitializeEncode", BindingFlags.InvokeMethod, null, obj8, array);
                    array = new object[]
					{
						obj8
					};
                    this.typeX509Extensions.InvokeMember("Add", BindingFlags.InvokeMethod, null, target, array);
                }
                else
                {
                    object obj9 = Activator.CreateInstance(this.typeSignerCertificate);
                    array = new object[]
					{
						1,
						0,
						12,
						oSigningCertificate.Thumbprint
					};
                    this.typeSignerCertificate.InvokeMember("Initialize", BindingFlags.InvokeMethod, null, obj9, array);
                    array = new object[]
					{
						obj9
					};
                    this.typeRequestCert.InvokeMember("SignerCertificate", BindingFlags.PutDispProperty, null, obj7, array);
                }
                object obj10 = Activator.CreateInstance(this.typeOID);
                array = new object[]
				{
					1,
					0,
					0,
					sHashAlg
				};
                this.typeOID.InvokeMember("InitializeFromAlgorithmName", BindingFlags.InvokeMethod, null, obj10, array);
                array = new object[]
				{
					obj10
				};
                this.typeRequestCert.InvokeMember("HashAlgorithm", BindingFlags.PutDispProperty, null, obj7, array);
                this.typeRequestCert.InvokeMember("Encode", BindingFlags.InvokeMethod, null, obj7, null);
                object target2 = Activator.CreateInstance(this.typeX509Enrollment);
                array[0] = obj7;
                this.typeX509Enrollment.InvokeMember("InitializeFromRequest", BindingFlags.InvokeMethod, null, target2, array);
                array[0] = 0;
                object obj11 = this.typeX509Enrollment.InvokeMember("CreateRequest", BindingFlags.InvokeMethod, null, target2, array);
                array = new object[]
                {
                    2,
                    obj11,
                    0,
                    string.Empty
                };
                this.typeX509Enrollment.InvokeMember("InstallResponse", BindingFlags.InvokeMethod, null, target2, array);
                array = new object[]
				{
					null,
					0,
					1
				};
                string s = string.Empty;
                try
                {
                    s = (string)this.typeX509Enrollment.InvokeMember("CreatePFX", BindingFlags.InvokeMethod, null, target2, array);
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message + " " + ex.StackTrace);
                }
                return new X509Certificate2(Convert.FromBase64String(s), string.Empty, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
            }
        }

        private CertificateProvider.CertEnrollEngine CertCreator;
        private ConcurrentDictionary<string, X509Certificate2> certServerCache = new ConcurrentDictionary<string, X509Certificate2>();
        private X509Certificate2 certRoot;
        private object _oRWLock = new object();
        private object CreateLock = new object();

        internal CertificateProvider()
        {
            if (this.CertCreator == null)
            {
                this.CertCreator = CertificateProvider.CertEnrollEngine.GetEngine(this);
            }

        }

        private static X509Certificate2Collection FindCertsBySubject(StoreName storeName, StoreLocation storeLocation, string sFullSubject)
        {
            X509Store x509Store = new X509Store(storeName, storeLocation);
            X509Certificate2Collection result;
            try
            {
                x509Store.Open(OpenFlags.OpenExistingOnly);
                X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, sFullSubject, false);
                result = x509Certificate2Collection;
            }
            finally
            {
                x509Store.Close();
            }
            return result;
        }

        private static X509Certificate2Collection FindCertsByIssuer(StoreName storeName, string sFullIssuerSubject)
        {
            X509Store x509Store = new X509Store(storeName, StoreLocation.LocalMachine);
            X509Certificate2Collection result;
            try
            {
                x509Store.Open(OpenFlags.OpenExistingOnly);
                X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, sFullIssuerSubject, false);
                result = x509Certificate2Collection;
            }
            finally
            {
                x509Store.Close();
            }
            return result;
        }
        public bool ClearCertificateCache()
        {
            bool bRemoveRoot = true;
            bool result = true;
            try
            {
                lock (_oRWLock)
                {
                    this.certServerCache.Clear();
                    this.certRoot = null;
                    string text = string.Format(CertConst.SUBJECT_FORMAT, CONFIG.sMakeCertRootCN, CONFIG.sMakeCertSubjectO);
                    X509Certificate2Collection x509Certificate2Collection;
                    if (bRemoveRoot)
                    {
                        x509Certificate2Collection = CertificateProvider.FindCertsBySubject(StoreName.Root, StoreLocation.LocalMachine, text);
                        if (x509Certificate2Collection.Count > 0)
                        {
                            X509Store x509Store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                            x509Store.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
                            try
                            {
                                x509Store.RemoveRange(x509Certificate2Collection);
                            }
                            catch
                            {
                                result = false;
                            }
                            x509Store.Close();
                        }
                    }
                    x509Certificate2Collection = CertificateProvider.FindCertsByIssuer(StoreName.My, text);
                    if (x509Certificate2Collection.Count > 0)
                    {
                        if (!bRemoveRoot)
                        {
                            X509Certificate2 rootCertificate = this.GetRootCertificate();
                            if (rootCertificate != null)
                            {
                                x509Certificate2Collection.Remove(rootCertificate);
                                if (x509Certificate2Collection.Count < 1)
                                {
                                    return true;
                                }
                            }
                        }
                        X509Store x509Store2 = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                        x509Store2.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
                        try
                        {
                            x509Store2.RemoveRange(x509Certificate2Collection);
                        }
                        catch (Exception ex)
                        {
                            Debug.Write(ex.Message + " " + ex.StackTrace);
                            result = false;
                        }
                        x509Store2.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message + " " + ex.StackTrace);
                result = false;
            }
            finally
            {
            }

            return result;
        }

        public void ClearPersonalCertificate()
        {
            string text = string.Format(CertConst.SUBJECT_FORMAT, CONFIG.sMakeCertRootCN, CONFIG.sMakeCertSubjectO);
            try
            {
                X509Certificate2Collection x509Certificate2Collection = CertificateProvider.FindCertsByIssuer(StoreName.My, text);
                if (x509Certificate2Collection.Count > 0)
                {

                    X509Store x509Store2 = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                    x509Store2.Open(OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly);
                    try
                    {
                        foreach (X509Certificate2 item in x509Certificate2Collection)
                        {
                            if (!item.HasPrivateKey)
                            {
                                x509Store2.Remove(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex.Message + " " + ex.StackTrace);
                    }
                    x509Store2.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        public bool rootCertIsTrusted(out bool bUserTrusted, out bool bMachineTrusted)
        {
            bUserTrusted = (0 < CertificateProvider.FindCertsBySubject(StoreName.Root, StoreLocation.CurrentUser, string.Format("CN={0}{1}", CONFIG.sMakeCertRootCN, CONFIG.sMakeCertSubjectO)).Count);
            bMachineTrusted = (0 < CertificateProvider.FindCertsBySubject(StoreName.Root, StoreLocation.LocalMachine, string.Format("CN={0}{1}", CONFIG.sMakeCertRootCN, CONFIG.sMakeCertSubjectO)).Count);
            return bUserTrusted || bMachineTrusted;
        }
 
        public bool CreateRootCertificate()
        {
            return null != this.CreateCert(CONFIG.sMakeCertRootCN, true);
        }
        public X509Certificate2 GetRootCertificate()
        {
            if (this.certRoot != null)
            {
                return this.certRoot;
            }
            X509Certificate2 x509Certificate = CertificateProvider.LoadCertificateFromWindowsStore(CONFIG.sMakeCertRootCN, true);
            this.certRoot = x509Certificate;
            return x509Certificate;
        }

        public X509Certificate2 GetCertificateForHost(string sHostname)
        {
            X509Certificate2 x509Certificate;
            try
            {
                if (certServerCache.TryGetValue(sHostname, out x509Certificate))
                {
                    return x509Certificate;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message + " " + ex.StackTrace);
            }
            lock (CreateLock)
            {
                x509Certificate = LoadCertificateFromWindowsStore(sHostname, false);
                if (x509Certificate == null)
                {
                    x509Certificate = this.CreateCertForHost(sHostname, false);
                }
                    if (x509Certificate != null)
                    {
                        CacheCertificateForHost(sHostname, x509Certificate);
                    }
                }
                        return x509Certificate;
        }


        internal static X509Certificate2 LoadCertificateFromWindowsStore(string sHostname, bool isRoot)
        {
            X509Store x509Store;
            if (isRoot)
            {
                x509Store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            }
            else
            {
                x509Store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            }
            try
            {
                x509Store.Open(OpenFlags.ReadOnly);
                string inStr = string.Format(CertConst.SUBJECT_FORMAT, sHostname, CONFIG.sMakeCertSubjectO);
                X509Certificate2Enumerator enumerator = x509Store.Certificates.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    X509Certificate2 current = enumerator.Current;
                    if (inStr.Equals(current.Subject, StringComparison.OrdinalIgnoreCase))
                    {
                        return current;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message + " " + ex.StackTrace);
            }
            finally
            {
                x509Store.Close();
            }
            return null;
        }


        public bool CacheCertificateForHost(string sHost, X509Certificate2 oCert)
        {
            try
            {
                //if (certServerCache.Keys.Count > 200)
                //{
                //    X509Certificate2 x509Certificate;
                //    certServerCache.TryRemove(certServerCache.Keys.First(), out x509Certificate);

                //}
                this.certServerCache.TryAdd(sHost, oCert);
            }
            finally
            {
            }
            return true;
        }

        private X509Certificate2 CreateCertForHost(string sHostname, bool isRoot)
        {
            X509Certificate2 x509Certificate = this.CertCreator.CreateCert(sHostname, isRoot);
            return x509Certificate;
        }

        private X509Certificate2 CreateCert(string sHostname, bool isRoot)
        {
            if (sHostname.IndexOfAny(CertConst.HOSTNAME_CHARS) != -1)
            {
                return null;
            }
            if (!isRoot && this.GetRootCertificate() == null)
            {
                try
                {
                    lock (_oRWLock)
                    {
                        if (this.GetRootCertificate() == null && !this.CreateRootCertificate())
                        {
                            X509Certificate2 result = null;
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message + " " + ex.StackTrace);
                }
                finally
                {
                }
            }
            X509Certificate2 x509Certificate = null;
            try
            {
                if (isRoot)
                {
                    lock (_oRWLock)
                    {
                        x509Certificate = this.CertCreator.CreateCert(sHostname, isRoot);
                        if (x509Certificate != null)
                        {
                            if (isRoot)
                            {
                                this.certRoot = x509Certificate;
                            }
                        }
                    }
                }
                else
                {
                    x509Certificate = this.CertCreator.CreateCert(sHostname, isRoot);
                    if (x509Certificate != null)
                    {
                        if (isRoot)
                        {
                            this.certRoot = x509Certificate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message + " " + ex.StackTrace);

            }
            finally
            {
            }
            return x509Certificate;
        }
    }
}
