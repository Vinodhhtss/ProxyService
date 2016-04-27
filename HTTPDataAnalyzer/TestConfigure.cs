using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace HTTPDataAnalyzer
{
    public class TestConfigure
    {
        public int TestCount { get; set; }
        public bool TestCheck { get; set; }

        public static TestConfigure GetConfig()
        {
            string path = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                                   "ProxyService\\TestConfig"), "TestConfig.xml");
             TestConfigure xmlConfig = null;
             if (File.Exists(path))
             {

                 XmlSerializer oXmlserialize = new XmlSerializer(typeof(TestConfigure));
                 TextReader oTextreader = new StreamReader(path);
                 xmlConfig = (TestConfigure)oXmlserialize.Deserialize(oTextreader);
                 oTextreader.Close();
             }

            return xmlConfig;
        }
       
        public static string Getpath()
        {
            string Path = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            String path = System.IO.Path.GetDirectoryName(Path);
            return path;
        }

        public static void Serialize()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                                  "ProxyService\\TestConfig");


            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = System.IO.Path.Combine(path, "TestConfig.xml");

            if (!File.Exists(path))
            {
                TestConfigure xmlConfig = new TestConfigure();
                xmlConfig.TestCheck = true;
                xmlConfig.TestCount = 10;

                XmlSerializer oXmlserialize = new XmlSerializer(typeof(TestConfigure));
                TextWriter oTextreader = new StreamWriter(path);
                oXmlserialize.Serialize(oTextreader, xmlConfig);
                oTextreader.Close();
            }
        }
    }  
}
