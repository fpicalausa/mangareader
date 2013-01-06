using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MangaParserTest
{
    public class DataLoader
    {
        public const string TestFileName = "TestData.xml";
        public const string TestDirectory = "testcases";
        public const string TestDirectoryInput = "inputdata";
        public const string TestDirectoryAny = "segmentation-any";
        public const string TestDirectoryAll = "segmentation-all";

        public class TestDescription
        {
            XElement element;

            public FileInfo GetPicturePath(string type)
            {
                FileInfo file = new FileInfo(
                    Path.Combine(Properties.Settings.Default.TestdataDirectory, type, element.Element("input").Value));

                // This does not ensures that the file will exist later on, but can help diagnosing issues.
                if (!file.Exists) throw new FileNotFoundException("Could not find the test image");

                return file;
            }

            public String Name { get { return element.Element("name").Value; } }
            public String Description { get { return element.Element("description").Value; } }
            public Bitmap GetBitmap(String type) { return new Bitmap(GetPicturePath(type).FullName); }

            public TestDescription(XElement test)
            {
                this.element = test;
            }
        }


        XElement root;

        private FileInfo GetTestData()
        {
            FileInfo file = new FileInfo(
                Path.Combine(Properties.Settings.Default.TestdataDirectory, TestFileName));

            // This does not ensures that the file will exist later on, but might avoid
            // some late night debugging.
            if (!file.Exists) throw new FileNotFoundException("Could not find the tests file");

            return file;
        }

        public DataLoader()
        {
            XDocument testdocument = XDocument.Load(GetTestData().OpenText());
            root = testdocument.Root;
        }

        public IEnumerable<Color> TestColors {
            get {
                return (from color in root.Element("data").Element("colors").Elements("color")
                     select ColorTranslator.FromHtml(color.Value));
            }
        }

        public IEnumerable<TestDescription> Tests {
            get {
                return (from test in root.Element("testcases").Elements("testcase")
                     select new TestDescription(test));
            }
        }

    }
}
