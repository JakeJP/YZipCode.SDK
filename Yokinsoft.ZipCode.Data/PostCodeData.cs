using System;
using System.Reflection;

using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Threading;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Yokinsoft.ZipCode.Data.JapanPost;

namespace Yokinsoft.ZipCode.Data
{

    public class PostCodeData 
    {
        public PostCodeData()
        {
        }
        public PostCodeData( string pathToXMLData )
        {
            PostCodeDataSource = pathToXMLData;
        }
        public PostCodeData( Stream stream)
        {
            Load(stream);
        }
        private static string _PostCodeDataSource;
        public static string PostCodeDataSource
        {
            get => _PostCodeDataSource;
            set
            {
                if( _PostCodeDataSource != value)
                {
                    _PostCodeDataSource = value;
                    if(PostCodeXmlDocument.IsValueCreated)
                        PostCodeXmlDocument = new Lazy<PostCodeDataLoader>(InitPostCodeXmlDocument, true);
                }
            }
        }
        public bool InitializationOfDataRequired {  get; set; }
        public static bool EnableLocalCache { get; set; } = true;
        public bool EnableResultCache { get; set; } = true;
        public string Delimiter { get; set; } = " ";
        public int SearchDepth { get; set; } = 3;
        public void Load( Stream stream)
        {
            PostCodeXmlDocument.Value.Load(stream);
        }

        public LookupXmlResponse Lookup( string keyword, LookupTarget targets =  LookupTarget.All)
        {
            var xmlData = PostCodeXmlDocument.Value.Data;
            if( xmlData == null ){
                Console.WriteLine("PostCodeXML is not ready yet.(null)");
                return null;
            }
            XsltArgumentList args = new XsltArgumentList();
            args.AddParam("keyword", "", (keyword ?? "").Trim());
			args.AddParam("delimiter", "", Delimiter);
            args.AddParam("date", "", DateTime.Now.ToString("O"));
            if (SearchDepth > 0) args.AddParam("depth", "", SearchDepth);
            if ( (targets & LookupTarget.ZipCode) == 0 ) args.AddParam("search-zipcode", "", "false");
            if ((targets & LookupTarget.Address) ==0) args.AddParam("search-address", "", "false");
            if ((targets & LookupTarget.Place) == 0) args.AddParam("search-place", "", "false");

            var xmlDoc = new LookupXmlResponse();

            try
            {
                using (var wt = xmlDoc.CreateNavigator().AppendChild())
                {
                    Transformer.Value.Transform(xmlData, args, wt);
                    wt.Close();
                }
            }
            catch ( IOException ioEx)
            {
                xmlDoc.LoadXml("<result />");
                var error = xmlDoc.CreateElement("error");
                error.InnerText = "DataSource is not ready.";
                xmlDoc.DocumentElement.AppendChild(error);
            }

            return xmlDoc;


        }

        private static Lazy<XslCompiledTransform> Transformer = new Lazy<XslCompiledTransform>( InitTransformer, true );
        private static Lazy<PostCodeDataLoader> PostCodeXmlDocument = new Lazy<PostCodeDataLoader>(InitPostCodeXmlDocument, true);

        private static XslCompiledTransform InitTransformer()
        {
            XslCompiledTransform trans = new XslCompiledTransform();
            LoadResourceFile(trans, "YZipCodeRunSearch.xsl");//trans.Load(Server.MapPath("./YZipCodeRunSearch.xsl"));
            Console.WriteLine("Transformer loaded.");
            return trans;
        }

        private static PostCodeDataLoader InitPostCodeXmlDocument()
        {
            string dataPath = PostCodeDataSource;

            Console.WriteLine("PostCodeXml loaded.");
            if(string.IsNullOrEmpty(dataPath))
            {
                return new PostCodeDataLoader();
            }
            if (Uri.IsWellFormedUriString(dataPath, UriKind.Absolute))
            {
                return new RemoteCachedDataFile(dataPath);
            }
            else
            {
                return new LocalCachedDataFile(dataPath);
            }
        }
        static void LoadResourceFile(XslCompiledTransform trans, string name)
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            Stream st = assm.GetManifestResourceStream("Yokinsoft.ZipCode.Data." + name);
            trans.Load(XmlReader.Create(st));

            st.Close();
        }

    }
}
