#define UMAYADIA

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Yokinsoft.ZipCode.ConvXML;
using Yokinsoft.ZipCode.Data.JapanPost;
using System.IO;
using Yokinsoft.ZipCode.Data;
using System.Diagnostics;

#if UMAYADIA
using Umayadia.Kana;
#endif
//using Microsoft.VisualBasic;

namespace Yokinsoft.ZipCode.ConvXML
{

    public abstract class XmlConverter
    {
        public const string DefaulOutputFileName = @"JapanPostCodeData.xml";
        protected DataSourceColumns Columns;
        protected string TranslateKanaString(string s)
        {
            if( OutputKanaType == DataSourceDifinition.KanaType || OutputKanaType == KanaType.None)
                return s;
            return
                OutputKanaType == KanaType.Hiragana ? KanaConverter.ToHiragana( KanaConverter.ToWide(s)) :
                OutputKanaType == KanaType.Katakana ? KanaConverter.ToKatakana(KanaConverter.ToWide(s)) :
                s;

        }
        public KanaType OutputKanaType { get; set; } = KanaType.None;
        public char OutputHyphenChar { get; set; } = '－'; // FULL WIDTH HYPHEN-MINUS

        protected string RegulateAddressHyphen( string text)
        {
            if ( string.IsNullOrEmpty(text) || OutputHyphenChar == default || DataSourceDifinition.HyphenChar == OutputHyphenChar)
                return text;
            return text.Replace( DataSourceDifinition.HyphenChar, OutputHyphenChar );
        }

        protected Regex reBracket = new Regex("^(.*?)[（\\(](.+[^）\\)])([）\\)]?)$");
        protected Regex reBracketClosing = new Regex("^([^（]*)）$");
        protected Regex reNumberRange = new Regex(@"^([\w]*?)(?'PREFIX'\w*?)(?'START'[\d－-]+)(\w*?)[～〜](\k'PREFIX')(?'END'[\d－-]+)(?'SUFFIX'[\w]*?)$");
        protected Regex reNumberRangeKana = //new Regex(@"^([\w]*?)(?'START'[\d－-]+)[～－\-−](?'END'[\d－-]+)(?'SUFFIX'[\w]*?)$");
            new Regex(@"^([\w]*?)(?'PREFIX'\w*?)(?'START'[\d－-]+)(\w*?)[～－\-−](\k'PREFIX')(?'END'[\d－-]+)(?'SUFFIX'[\w]*?)$");
        protected Regex reNumberRangeRome = //new Regex(@"^([\w]*?)(?'START'[\d－-]+)[～－\-−](?'END'[\d－-]*\d)(?'SUFFIX'[\w\-]*?)$");
            new Regex(@"^([\w]*?)(?'PREFIX'\w?)(?'START'[\d]+)(\w*?)[～－\-−](\k'PREFIX')(?'END'[\d]+)(?'SUFFIX'[\w\-]*?)$");
        protected Regex reInlineCommaSeparator = new Regex(@"(?<=(^|[、､]))[^「」、､]*(「.*?」)?[^「」、､]*(?=(?:[、､]|$))");
        protected Regex reInlineCommaSeparatorKana = new Regex(@"(?<=(^|[、､\.]))[^、､\.＜＞]*?(＜.*?＞)?[^、､\.＜＞]*?(?=(?:[、､\.]|$))");
        protected Regex reSuppressText = new Regex("(以下に掲載がない場合|次のビルを除く|IKANIKEISAIGANAIBAAI|TSUGINOBIRUONOZOKU)");
        protected Regex reAmbiguous = new Regex(@"その他|を除く|以外|〜");
        protected bool bInBracket = false;
        protected XmlDocument Xml { get; set; }
        protected JapanPostDataSourceDefinition DataSourceDifinition {  get; set; }
        public XmlConverter( XmlDocument xml, JapanPostDataSourceDefinition dataSourceDefinition )
        {
            DataSourceDifinition = dataSourceDefinition;
            var cols = DataSourceDifinition.Columns;
            Xml = xml;
            Columns = new DataSourceColumns();
            for( int i = 0; i< cols.Length; i++)
            {
                var c = cols[i];
                var fi = typeof(DataSourceColumns).GetField(c.ToString());
                fi.SetValue(Columns, i);
            }

        }
        public void ConvertFile( TextReader reader)
        {
            string line;
            while ( (line = reader.ReadLine()) != null )
            {
                var columns = SplitCsv(line);
                ProcessOneLine(columns);
            }
        }
        static Regex reCsvSplitter = new Regex(@"(?>(?(IQ)\k<QUOTE>(?<-IQ>)|(?<=^|,)(?<QUOTE>[""])(?<IQ>))|(?(IQ).|[^,]))+|^(?=,)|(?<=,)(?=,)|(?<=,)$");
        string[] SplitCsv(string csv)
        {
            var values = new List<string>();
            Match m = reCsvSplitter.Match(csv);
            while (m != null && m.Success)
            {
                values.Add(m.Value.Trim('\"'));
                m = m.NextMatch();
            }

            return values.ToArray();
        }

        static Dictionary<char,char> WideNumberDic = "０１２３４５６７８９".ToArray()
            .Select( (n,i) => new { n, i }).ToDictionary( m=> m.n, m=> m.i.ToString().ToCharArray()[0] );
        static Regex ReWideNumber = new Regex("[０-９]+");
        static Regex ReNarrowNumber = new Regex("[0-9]+");
        protected string ToNarrow( string text, out bool iswide)
        {
            iswide = false;
            string result = text == null ? null :
                ReWideNumber.Replace(text, (m) =>
                {
                    return new string(m.Value.Select(n => WideNumberDic[n]).ToArray());
                });
            iswide = (result != text);
            return result;
        }
        protected string ToWide( string text)
        {
            return ReNarrowNumber.Replace(text, m => new string(m.Value.Select(n => "０１２３４５６７８９"[int.Parse(n.ToString())] ).ToArray() ));
        }
        protected class ExpandedLine
        {
            public string Text { get; set; }
            public string TextKana { get; set; }
            public bool Ambiguous { get; set; }
            public bool Expanded { get; set; }
        }
        protected ExpandedLine[] NumrangeExpander( string text, string textKana, Regex reText, Regex reTextKana )
        {
            ExpandedLine[] _Expand(string _text, Regex _re)
            {
                var nm = _re.Match(_text);
                bool _ambiguous = false;
                if (nm.Success)
                {
                    var prefix = nm.Groups[1].Value + nm.Groups["PREFIX"].Value;
                    var suffix = nm.Groups["SUFFIX"].Value;
                    int nmin = 0, nmax = 0; bool iswide = false;
                    if (int.TryParse(ToNarrow(nm.Groups["START"].Value, out iswide), out nmin) &&
                        int.TryParse(ToNarrow(nm.Groups["END"].Value, out iswide), out nmax))
                    {
                        if (nmax > nmin)
                        {
                            //Console.WriteLine(string.Format("{0}{1}-{2}{3}", prefix, nmin, nmax, suffix));
                            if (nmax - nmin <= 100)
                            {
                                return Enumerable.Range(nmin, nmax - nmin + 1).Select(nn => string.Format("{0}{1}{2}",
                                    prefix,
                                    iswide ? ToWide(nn.ToString()) : nn.ToString(),
                                    suffix))
                                    .Select( m => new ExpandedLine { Text = m, Expanded = true, Ambiguous = false })
                                    .ToArray();
                            }
                            else
                            {
                                _ambiguous = true;
                            }
                        }
                        Console.WriteLine(_text);
                    }
                }
                return new ExpandedLine[] { new ExpandedLine { Text = _text, Ambiguous = _ambiguous, Expanded = false } };
            }
            var hs = reInlineCommaSeparator.Matches(text).Cast<Match>().Select(m => m.Value).ToArray();
            //var hs = text.Split("、､".ToCharArray());
            //var hsKana = textKana.Split(@"、､\.".ToCharArray());
            var hsKana = reInlineCommaSeparatorKana.Matches(textKana).Cast<Match>().Select(m=>m.Value).ToArray();
            if ( /*(hs.Length == 1 && hsKana.Length == 1 ) || */ hs.Length != hsKana.Length)
                return new[] { new ExpandedLine { Text = text, TextKana = textKana, Ambiguous = reAmbiguous.IsMatch(text), Expanded = false } };
            var result = hs.Select((m, i) => new { Index = i, Text = m, Re = reText })
                    .Join(hsKana.Select((m, i) => new { Index = i, Text = m, Re = reTextKana }), o => o.Index, i => i.Index,
                    (o, i) => new { Name = o, NameKana = i })
                    .Select(m => new { NameExpanded = _Expand(m.Name.Text, m.Name.Re), KanaExpanded = _Expand(m.NameKana.Text, m.NameKana.Re) })
                    .SelectMany(m =>
                        m.NameExpanded.Select((mm, i) => new { Index = i, E = mm })
                            .Join(m.KanaExpanded.Select((mm, i) => new { Index = i, E = mm })
                                , o => o.Index, i => i.Index, (o, i) =>
                                    new ExpandedLine {
                                        Text = o.E.Text, TextKana = i.E.Text,
                                        Ambiguous = o.E.Ambiguous || reAmbiguous.IsMatch(o.E.Text)
                                    }))
                    .ToArray();
                 
            return result;
        }
        public abstract void ProcessOneLine(string[] cols);

        string previousLine = "";
        string previousLineKana = "";
        protected bool PreProcessMultipleLine( string[] cols, int colTown, int colTownRuby)
        {
            // 複数行に分割されている（）を１行に復元
            if (bInBracket)
            {
                var m = reBracketClosing.Match(cols[colTown]);
                if (m.Success)
                {
                    bInBracket = false;
                    cols[colTown] = previousLine + cols[colTown];
                    cols[colTownRuby] = previousLineKana + cols[colTownRuby];
                }
                else
                {
                    previousLine += cols[colTown];
                    previousLineKana += cols[colTownRuby];
                    return true;
                }
            }
            else
            {
                var m = reBracket.Match(cols[colTown]);
                if (m.Success && m.Groups[2].Success && m.Groups[3].Value.Length == 0)
                {
                    previousLine = cols[colTown];
                    previousLineKana = cols[colTownRuby];
                    bInBracket = true;
                    return true;
                }
            }
            return false;
        }
    }
    public class KENALLConverter : XmlConverter
    {
        public KENALLConverter( XmlDocument xml, JapanPostDataSourceDefinition definition ) : base( xml, definition) { }


        public override void ProcessOneLine( string[] cols)
        {
            if (PreProcessMultipleLine(cols, Columns.Town, Columns.TownRuby))
                return;
            var hi = new StringCollection {
                cols[Columns.Prefecture],
                cols[Columns.City],
                RegulateAddressHyphen(cols[Columns.Town])
            };
            var hi_kana = new StringCollection
            {
                cols[Columns.PrefectureRuby],
                cols[Columns.CityRuby],
                cols[Columns.TownRuby]
            };

            string[] areaType = new string[] { "prefecture", "city", null, null, null };
            XmlElement node = Xml.FirstChild as XmlElement;
            for (int i = 0; i < hi.Count; i++)
            {
                if (reSuppressText.IsMatch( hi[i] ) ) 
                {
                    if( node != null)
                    {
                        node.SetAttribute("description", hi[i]);
                        node.SetAttribute("description-kana", hi_kana[i]);
                    }
                    hi[i] = ""; hi_kana[i] = ""; 
                }
                Match match, match_kana;
                match = reBracket.Match(hi[i]);
                if (match.Success)
                {
                    hi[i] = match.Groups[1].Value;
                    hi.Insert(i + 1, match.Groups[2].Value);
                    match_kana = reBracket.Match(hi_kana[i]);
                    if (match_kana.Success)
                    {
                        hi_kana[i] = match_kana.Groups[1].Value;
                        hi_kana.Insert(i + 1, match_kana.Groups[2].Value);
                    }
                    else
                        hi_kana.Insert(i + 1, "");

                }

                var branches = NumrangeExpander(hi[i], hi_kana[i], reNumberRange, reNumberRangeKana);

                XmlElement a = null;
                //Debug.Assert(branch.Length <= 100);
                foreach ( var branch in branches )
                {
                    if (string.IsNullOrEmpty(branch.Text))
                    {
                        continue;
                    }
                    // ~ ~ issue
                    string name = branch.Ambiguous ? branch.Text.Replace("〜", "～") : branch.Text;
                    a = node.SelectSingleNode("area[@name=\'" + name + "\']") as XmlElement; ;
                    if (a == null)
                    {
                        a = Xml.CreateElement("area");
                        a.SetAttribute("name", name);
                        a.SetAttribute("name-kana", TranslateKanaString(branch.TextKana));
                        if (i >= 1)
                        {
                            a.SetAttribute("jis-code", cols[Columns.JISCode]);
                            a.SetAttribute("post-code", cols[Columns.PostalCode]);
                            a.SetAttribute("post-code5", cols[Columns.PostalCode5].Trim());
                        }
                        if (!string.IsNullOrEmpty(areaType[i])) a.SetAttribute("type", areaType[i]);
                        if (branch.Ambiguous)
                            a.SetAttribute("ambiguous", "true");
                        node.AppendChild(a);
                        //if (i == 0) Console.Write(hi[i] + "...");
                    }
                }
                node = a;
            }
        }

    }
    public class JIGYOSYOConverter : XmlConverter
    {
        public JIGYOSYOConverter(XmlDocument xml) : base(xml, JapanPostDataSourceDefinition.Jigyosyo ) { }
        public override void ProcessOneLine(string[] cols)
        {
            string[] areaType = new string[] { "prefecture", "city", "" };
            string pob = String.Empty;
            var hi = new List<string>
            {
                cols[Columns.Prefecture],
                cols[Columns.City],
                RegulateAddressHyphen(cols[Columns.Town]),
                //cols[Columns.Koaza]
            };

            if (cols[Columns.Koaza] != "")
            {
                Match match = Regex.Match(cols[Columns.Koaza], "(^.*)（(.+)）$");
                if (match.Success)
                {
                    if (match.Groups[1].Value != "")
                    {
                        hi.Add(cols[Columns.Koaza]);
                    }
                    pob = match.Groups[2].Value;
                }
                else hi.Add(cols[Columns.Koaza]);

            }
            hi = hi.Where(m => !string.IsNullOrEmpty(m)).ToList();

            XmlElement node = Xml.FirstChild as XmlElement;
            for (int i = 0; i < hi.Count; i++)
            {
                if (reSuppressText.IsMatch( hi[i] ) ) { hi[i] = ""; }

                string[] branch = new string[] { hi[i] };

                XmlElement a = null;
                for (int ii = 0; ii < branch.Length; ii++)
                {
                    a = node.SelectSingleNode(@$"area[@name='{branch[ii]}']") as XmlElement; ;
                    if (a == null)
                    {
                        a = Xml.CreateElement("area");

                        a.SetAttribute("name", branch[ii]);
                        //a.SetAttribute("name-kana", branch_kana[ii]);
                        //a.SetAttribute("post-code", cols[Columns.PostalCode]);
                        //a.SetAttribute("post-code5", cols[Columns.PostalCode5]);

                        //if( areaType[i] != "" ) a.SetAttribute("type", areaType[i] );
                        node.AppendChild(a);
                    }
                    if (i == hi.Count - 1)
                    {
                        XmlElement place = Xml.CreateElement("place");
                        place.SetAttribute("name", cols[Columns.Business]);
                        place.SetAttribute("name-kana", TranslateKanaString(cols[Columns.BusinessRuby]));
                        place.SetAttribute("jis-code", cols[Columns.JISCode]);
                        place.SetAttribute("post-code", cols[Columns.PostalCode]);
                        place.SetAttribute("post-code5", cols[Columns.PostalCode5].Trim());

                        if (cols[Columns.POB] == "1")
                        {
                            if (pob != string.Empty)
                            {
                                place.SetAttribute("POB", pob);
                            }
                            else
                            {
                                //place.SetAttribute("POB", "不明");
                                Console.WriteLine("POBフラグが１ですが、私書箱の記述が見つかりませんでした。{0}", cols[Columns.Business]);
                            }
                        }
                        a.AppendChild(place);
                    }
                }
                node = a;
            }
        }
    }
    public class RomeConverter : XmlConverter
    {
        public RomeConverter( XmlDocument xml ): base(xml, JapanPostDataSourceDefinition.Rome ) { }
        string stripSpace( string text)
        {
            return text == null ? null : text.Replace("　", "").Replace(" ", "");
        }
        public override void ProcessOneLine(string[] cols)
        {
            //if (PreProcessMultipleLine(cols, Columns.TownRuby, Columns.Town ))
            //    return;
            var hi = new StringCollection {
                stripSpace(cols[Columns.Prefecture]),
                stripSpace(cols[Columns.City]),
                stripSpace(RegulateAddressHyphen(cols[Columns.Town]))
            };
            var hi_kana = new StringCollection
            {
                cols[Columns.PrefectureRuby],
                cols[Columns.CityRuby],
                cols[Columns.TownRuby]
            };

            string[] areaType = new string[] { "prefecture", "city", null, null, null };
            XmlElement node = Xml.FirstChild as XmlElement;
            for (int i = 0; i < hi.Count; i++)
            {
                if (reSuppressText.IsMatch(hi[i]) ) { hi[i] = ""; hi_kana[i] = ""; }
                Match match, match_kana;
                match = reBracket.Match(hi[i]);
                if (match.Success)
                {
                    hi[i] = match.Groups[1].Value;
                    hi.Insert(i + 1, match.Groups[2].Value);
                    match_kana = reBracket.Match(hi_kana[i]);
                    if (match_kana.Success)
                    {
                        hi_kana[i] = match_kana.Groups[1].Value;
                        hi_kana.Insert(i + 1, match_kana.Groups[2].Value);
                    }
                    else
                        hi_kana.Insert(i + 1, "");

                }

                if (i == hi.Count - 1 && node != null && !node.HasAttribute("post-code"))
                {
                    Console.WriteLine("ROME: 番号なし: " + string.Join(" ", hi.Cast<string>()));
                    break;
                }

                var branches = NumrangeExpander(hi[i], hi_kana[i], reNumberRange, reNumberRangeRome );

                XmlElement a = null;
                foreach( var branch in branches )
                {
                    if (string.IsNullOrEmpty(branch.Text))
                    {
                        continue;
                    }
                    a = node.SelectSingleNode(@$"area[@name='{branch.Text}']") as XmlElement;
                    if( a == null )
                        a = a ?? node.SelectSingleNode(@$"area[@post-code='{cols[Columns.PostalCode]}']") as XmlElement;
                    if ( a == null)
                    {
                        a = node.SelectSingleNode(@$"//area[@post-code='{cols[Columns.PostalCode]}']") as XmlElement;
                        if( a != null)
                        {
                            a.SetAttribute("kana-rome", cols[Columns.TownRuby]);
                            return;
                        }
                    }
                    if (a == null)
                    {
                        Console.WriteLine($"ROME: 一致なし: {string.Join(" ", hi.Cast<string>())}, {branch.Text}, {branch.TextKana}");

                        a = Xml.CreateElement("area");
                        a.SetAttribute("name", branch.Text);
                        a.SetAttribute("name-rome", branch.TextKana);
                        /*if (i >= 1)
                        {
                            a.SetAttribute("jis-code", cols[Columns.JISCode]);
                            a.SetAttribute("post-code", cols[Columns.PostalCode]);
                            a.SetAttribute("post-code5", cols[Columns.PostalCode5]);
                        }*/
                        if (areaType.Length > i && !string.IsNullOrEmpty(areaType[i]))
                            a.SetAttribute("type", areaType[i]);
                        if (branch.Ambiguous)
                            a.SetAttribute("ambiguous", "true");
                        node.AppendChild(a);
                        //if (i == 0) Console.Write(hi[i] + "...");
                    }
                    else 
                        a.SetAttribute("name-rome", branch.TextKana);
                }
                node = a;
                
            }
        }
    }
}
