using System;
using System.IO;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using Yokinsoft.ZipCode;
using Yokinsoft.ZipCode.ConvXML;
using Yokinsoft.ZipCode.Data.JapanPost;
using Yokinsoft.ZipCode.Data;

namespace Yokinsoft.ZipCode.ConvXML
{

    class Program
    {

        static void Main(string[] args)
        {
            Environment.ExitCode = -1;
            // コマンドラインの解析
            NameValueCollection arguments = new NameValueCollection();
            for (int i = 0; i < args.Length; i++)
            {
                string s = args[i];
                if (s.StartsWith("-") || (Path.DirectorySeparatorChar != '/' && s.StartsWith("/")))
                {
                    string vname = s.Substring(1).Trim();
                    string vvalue = string.Empty;
                    for (var ii = i + 1; ii < args.Length; ii++)
                    {
                        if (args[ii].StartsWith("-")) break;
                        vvalue += (string.IsNullOrEmpty(vvalue) ? "" : ",") + args[ii];
                        i++;
                    }
                    arguments[vname] = vvalue;
                }
            }
            Program p = new Program(arguments);
            p.Run();
        }
        Program(NameValueCollection arguments)
        {
            this.arguments = arguments;
        }

        NameValueCollection arguments;
        KanaType kana;
        JapanPostDataSource[] DataSources;


        void Run()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("陽気に郵便番号 XML データ作成プログラム. Ver.{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine($"(c) 2006-{DateTime.Now.Year} Yokinsoft. https://www.yo-ki.com All rights reserved.\n");

            kana = (KanaType)Enum.Parse(typeof(KanaType), arguments["kana"] ?? "katakana", true);
            DataSources = (arguments["source"] ?? "utf").Split(",; ".ToCharArray()).Select(m => m.Trim())
                .Select(m => (JapanPostDataSource)Enum.Parse(typeof(JapanPostDataSource), m, true))
                .ToArray();

            //
            if (arguments["?"] != null || arguments["h"] != null || arguments["help"] != null)
            {
                Stream rd = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Yokinsoft.ZipCode.ConvXML.CommandHelp.txt");
                if (rd == null) return;
                System.IO.StreamReader r = new StreamReader(rd, Encoding.UTF8);
                string s;
                while ((s = r.ReadLine()) != null) Console.WriteLine(s);
                return;
            }
            Console.WriteLine("[1] 日本郵便からCSVファイルをダウンロードします。");

            string workdir = arguments["workdir"];



            string outputxml =
                arguments["xml"] == null ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, XmlConverter.DefaulOutputFileName) :
                Path.IsPathRooted(arguments["xml"]) ? arguments["xml"] :
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, arguments["xml"]);


            var strConvMode =
                kana == KanaType.hankaku ? KanaType.Narrow :
                kana == KanaType.katakana ? KanaType.Katakana :
                kana == KanaType.hiragana ? KanaType.Hiragana :
                KanaType.None;

            Downloader loader = new Downloader(workdir);

            List<DownloadedFile> DownloadedFiles;
            if (arguments["force"] == null && File.Exists(outputxml))
            {
                XmlDocument x = new XmlDocument();
                x.Load(outputxml);
                XmlElement e = x.SelectSingleNode("/area") as XmlElement;
                var downloads = DataSources
                    .Select(ds => new { Type = ds, Definition = JapanPostDataSourceDefinition.DataSourceMap[ds] })
                    .Select(m => new {
                        m.Type, m.Definition, attname = "date-of-" + Path.GetFileNameWithoutExtension(m.Definition.FileName).ToLower(),
                        LocalFilename = !string.IsNullOrEmpty(workdir) ? Path.Combine(workdir, Path.GetFileName(m.Definition.Url)) : null })
                    .Select(m => new { m.Type, m.Definition,
                        lastModified = e.HasAttribute(m.attname) ? (DateTime?)DateTime.Parse(e.GetAttribute(m.attname)) : null,
                        localfiledate = !string.IsNullOrEmpty(m.LocalFilename) && File.Exists(m.LocalFilename) ? (DateTime?)File.GetLastWriteTime(m.LocalFilename) : null
                    })
                    .Select(m => new {
                        m.Type, m.Definition,
                        Downloaded = m.localfiledate == null || m.lastModified >= m.localfiledate ? loader.DownloadAndExtract(m.Type, m.lastModified) : null
                    })
                    .ToList();
     
                if(downloads.All(m => m.Downloaded == null))
                {
                    if (arguments["forceConvert"] == null)
                    {
                        Console.WriteLine("既存のデータは最新の状態です。ファイル変換処理を中断します。");
                        Environment.ExitCode = 1;
                        return;
                    }
                }
                DownloadedFiles = downloads.Select(m => new
                {
                    m.Type,
                    m.Definition,
                    Downloaded = m.Downloaded ?? loader.DownloadAndExtract(m.Type)
                })
                .Select( m=> m.Downloaded )
                .ToList();
            }
            else
            {
                DownloadedFiles = DataSources.Select(m => loader.DownloadAndExtract(m)).ToList();
            }
            Console.WriteLine("");
            Console.WriteLine("[2] CSVファイルをXMLファイルに変換します。");
            XmlDocument xml = new XmlDocument();
            xml.LoadXml("<area name=\'Japan\' cc2=\'JP\' cc3=\'JPN\'></area>");
            ((XmlElement)xml.FirstChild).SetAttribute("date-created", DateTime.Now.ToString("O"));
            foreach (var dl in DownloadedFiles)
            {
                Console.WriteLine();
                Console.WriteLine($" |--- {dl.File.Name} を変換...");

                dl.ConvertOn(xml, strConvMode);
                dl.Dispose();

            }

            var xmlWriter = new XmlTextWriter(outputxml, Encoding.UTF8);
            if (arguments["indent"] != null)
                xmlWriter.Formatting = Formatting.Indented;
            xml.Save(xmlWriter);
            xmlWriter.Close();

            Environment.Exit(0);
        }

    }
}
