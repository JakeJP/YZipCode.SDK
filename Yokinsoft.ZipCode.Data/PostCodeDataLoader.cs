using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml.XPath;

namespace Yokinsoft.ZipCode.Data
{
    public class PostCodeDataLoader
    {
        public virtual XPathDocument Data { get; set; }

        public virtual void Load(Stream stream)
        {
            Data = new XPathDocument(stream);
        }
    }
    public class LocalCachedDataFile : PostCodeDataLoader
    {
        public string DataSourcePath { get; set; }
        public bool AutoRefresh { get; set; } = false;
        public LocalCachedDataFile(string filePath)
        {
            DataSourcePath = filePath;
        }

        public override void Load(Stream stream)
        {
            throw new NotImplementedException();
        }


#if false

                if (!Path.IsPathRooted(dataPath))
                {
                    var baseDir = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
                    if (baseDir == null)
                    {
                        baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    }
                    dataPath = Path.GetFullPath(Path.Combine(baseDir, dataPath));
                }
                if (File.Exists(dataPath))
                {
                    using (var fs = File.Open(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        xml = new XPathDocument(fs);
                        fs.Close();
                    }
                }
                if (fileWatcher == null)
                {
                    fileWatcher = new FileSystemWatcher
                    {
                        Path = Path.GetDirectoryName(dataPath),
                        Filter = Path.GetFileName(dataPath),
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.Size
                    };
                    fileWatcher.Changed += Watcher_Changed;
                    fileWatcher.Created += Watcher_Changed;
                    //watcher.Deleted += Watcher_Changed;
                    fileWatcher.Renamed += Watcher_Renamed;
                    fileWatcher.EnableRaisingEvents = true;
                }
                 private static void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            PostCodeXmlDocument = new Lazy<XPathDocument>(InitPostCodeXmlDocument, true);
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            PostCodeXmlDocument = new Lazy<XPathDocument>(InitPostCodeXmlDocument, true);
        }
#endif
    }
    public class RemoteCachedDataFile : PostCodeDataLoader
    {
        public RemoteCachedDataFile( string url )
        {
            RemoteDataSource = url;
        }
        public bool AutoRefresh { get; set; } = true;
        public string RemoteDataSource { get; set; }
        public string LocalDataSource { get; set; }
        public DateTimeOffset? LastModifiied { get; set; }
        public EntityTagHeaderValue? ETag { get; set; }
        public DateTime? LastChecked { get; set; }
        private object RemoteAccessLockObjectg = new object();

        public override void Load(Stream stream)
        {
            var x = new XPathDocument(stream);
            Data = x;
        }
        public void RefreshOnChange()
        {
            bool lockTaken = false;
            Monitor.TryEnter(RemoteAccessLockObjectg, 1000, ref lockTaken);
            try
            {
                Data = LoadRemoteDataSource();
                LastChecked = DateTime.Now;
            }
            finally
            {
                Monitor.Exit(RemoteAccessLockObjectg);
            }
        }
        public override XPathDocument Data
        {
            get
            {
                if (AutoRefresh)
                {
                    if (Data == null || LastChecked + PollingInterval >= DateTime.Now)
                    {
                        RefreshOnChange();
                    }
                }
                return base.Data;
            }
        }


        protected XPathDocument LoadRemoteDataSource()
        {
            var client = new HttpClient();
            if (LastModifiied != null)
                client.DefaultRequestHeaders.IfModifiedSince = LastModifiied.Value;
            if (ETag != null)
                client.DefaultRequestHeaders.IfNoneMatch.Add(ETag);

            var response = client.GetAsync(RemoteDataSource).Result;
            if (response.StatusCode == HttpStatusCode.NotModified)
            {

            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                DateTime dt;
                if (response.Headers.Date != null)
                {
                    LastModifiied = response.Content.Headers.LastModified;
                    ETag = response.Headers.ETag;
                }
                else
                    LastModifiied = DateTime.Now;
                var stream = response.Content.ReadAsStreamAsync().Result;
                Data = new XPathDocument(stream);
            }
            response.Dispose();
            return Data;
        }
        public bool CacheLocalFile { get; set; }

        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(10);
        public bool IsChanged { get; set; }

    }
}
