using System;
using System.Text;
using System.Net;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using Yokinsoft.ZipCode.ConvXML;
using Yokinsoft.ZipCode.Data.JapanPost;
using System.Text.RegularExpressions;
using System.Runtime.Remoting.Messaging;
using Yokinsoft.ZipCode.Data;

namespace Yokinsoft.ZipCode.ConvXML
{
    public class DownloadedFile : IDisposable
    {
        public bool DeleteOnDispose {  get; set; }
        public JapanPostDataSource DataSource {  get; set; }
        public JapanPostDataSourceDefinition DataSourceDefinition { get; set; }
        public DateTime? OriginLastModified { get; set; }
        public FileInfo ZipFile { get; set; }
        public FileInfo File { get; set; }
        public XmlConverter GetConverter( XmlDocument xml )
        {
            return
                DataSource == JapanPostDataSource.UTF ? new KENALLConverter(xml, JapanPostDataSourceDefinition.UTF) :
                DataSource == JapanPostDataSource.Kogaki ? new KENALLConverter(xml, JapanPostDataSourceDefinition.Kogaki) :
                DataSource == JapanPostDataSource.Oogaki ? new KENALLConverter(xml, JapanPostDataSourceDefinition.Oogaki ) :
                DataSource == JapanPostDataSource.Jigyosyo ? new JIGYOSYOConverter(xml) :
                DataSource == JapanPostDataSource.Rome ? new RomeConverter(xml) : null;
        }
        public void ConvertOn(XmlDocument xml, KanaType strConvMode )
        {
            var attname = "date-of-" + Path.GetFileNameWithoutExtension(DataSourceDefinition.FileName).ToLower();
            if(OriginLastModified != null)
                ((XmlElement)xml.SelectSingleNode("/area")).SetAttribute(attname, OriginLastModified.Value.ToString("O"));
            using (var file = File.Open(FileMode.Open, FileAccess.Read, FileShare.None))
            using (var reader = new StreamReader(file, DataSourceDefinition.Encoding))
            {
                var converter = GetConverter(xml);
                converter.OutputKanaType = strConvMode;
                converter.ConvertFile(reader);

                reader.Close();
                file.Close();
            }
        }
        public void Dispose()
        {
            if (DeleteOnDispose)
            {
                if (File.Exists)
                    File.Delete();
                if( ZipFile.Exists)
                    ZipFile.Delete();
            }
        }

    }
    public class Downloader
    {


        public Downloader( string workDir = null)
        {
            WorkDirectoryPath = workDir ?? string.Empty;
        }
        //public JapanPostDataSource DataSource { get; }
        //public JapanPostDataSourceDefinition DataSourceDefinition { get; }
        public string WorkDirectoryPath { get; set; }

        public DownloadedFile DownloadAndExtract( JapanPostDataSource datasource, DateTime? ifModifiedSince = null )
        {
            JapanPostDataSourceDefinition definition = JapanPostDataSourceDefinition.DataSourceMap[datasource];
            Uri url = new Uri(definition.Url);
            Console.WriteLine("ダウンロード: " + url.ToString());

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url.ToString());
            if (ifModifiedSince != null)
            {
                req.IfModifiedSince = ifModifiedSince.Value;
            }
            string localArchiveFileName = string.IsNullOrEmpty(WorkDirectoryPath) ? Path.GetTempFileName()
                    : Path.Combine(WorkDirectoryPath, Path.GetFileName(definition.Url) );
            HttpWebResponse res = null;
            DateTime? lastModifed = null;
            try
            {
                res = (HttpWebResponse)req.GetResponse();
                Console.WriteLine(res.StatusCode + " " + res.StatusDescription);
                Console.WriteLine(res.LastModified);
                lastModifed = res.LastModified;
                using (var fs = File.Open(localArchiveFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var reader = res.GetResponseStream())
                {
                    reader.CopyTo(fs);
                    fs.Close();
                    reader.Close();
                }
                var fi = new FileInfo(localArchiveFileName);
                fi.LastWriteTime = res.LastModified;
                fi.CreationTime = res.LastModified;
                res.Close();
                Console.WriteLine();
                Console.WriteLine("ダウンロード完了");
            }
            catch (WebException e)
            {
                HttpWebResponse r = e.Response as HttpWebResponse;
                if (r == null || r.StatusCode != HttpStatusCode.NotModified)
                    throw e;
                Console.WriteLine("ファイルのダウンロードがスキップされました。(NotModified)");
                return null;
            }

#if !DOWNLOAD_LZH
            Console.WriteLine("ZIP ファイルを展開...");
#if DOTNET30 && false
            var zipFile = System.IO.Packaging.Package.Open( System.IO.Path.Combine(workDir, localArchiveFileName), FileMode.Open, FileAccess.Read );
            
#else   // using DotNetZip
            var zipFile = Ionic.Zip.ZipFile.Read(localArchiveFileName);
            zipFile.ParallelDeflateThreshold = -1;
            if ( zipFile != null)
            {
                foreach (Ionic.Zip.ZipEntry ze in zipFile)
                {
                    if (String.Compare(ze.FileName, definition.FileName, true) == 0)
                    {

                        var extractPath = string.IsNullOrEmpty(WorkDirectoryPath) ?
                            Path.GetTempFileName() : Path.Combine(WorkDirectoryPath, ze.FileName);
                        var extractTmpPath = string.Concat(extractPath, ".tmp");
                        if (File.Exists(extractTmpPath))
                        {
                            File.Delete(extractTmpPath);
                        }
                        using (var fs = File.OpenWrite(extractPath))
                        {
                            ze.Extract(fs);
                            fs.Close();
                        }
                        return new DownloadedFile
                        {
                            DataSource = datasource,
                            DataSourceDefinition = definition,
                            OriginLastModified = lastModifed,
                            File = new FileInfo(extractPath),
                            ZipFile = new FileInfo(localArchiveFileName),
                            DeleteOnDispose = string.IsNullOrEmpty(WorkDirectoryPath) ? true : false
                        };
                    }
                }
            }
           
#endif

            return null;
#else
            Console.WriteLine("LHA version : " + GetVersion());
            StringBuilder o = new StringBuilder(1024);
            int iRes = Unlha(0, "e " + localArchiveFileName + " " + csvFileName + " -c -jyo", o, 1024);
            if (iRes != 0)
            {
                Console.WriteLine(o.ToString());
                return false;
            }
            return true;
#endif
        }
    }
}
