namespace CertificateManagement
{
    class CertConst
    {

        public const string ALGORITHM = "SHA256";
        public const string SUBJECT_FORMAT = "CN={0}{1}";

        public static readonly char[] HOSTNAME_CHARS = new char[]
			{
				'"',
				'\r',
				'\n',
				'\0'
			};
    }
}
