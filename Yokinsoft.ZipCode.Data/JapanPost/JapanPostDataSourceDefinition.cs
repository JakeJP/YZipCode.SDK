using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yokinsoft.ZipCode.Data.JapanPost;


namespace Yokinsoft.ZipCode.Data.JapanPost
{
    public class JapanPostDataSourceDefinition
    {
        public JapanPostDataSourceDefinition( JapanPostDataSource sourceType) {
            DataSourceType = sourceType;
        }
        public readonly JapanPostDataSource DataSourceType;
        public Encoding Encoding { get; private set; }
        public KanaType KanaType { get; private set; }
        public char HyphenChar { get; private set; }
        public string Url { get; private set; }
        public string FileName { get; private set; }
        public DataSourceColumn[] Columns { get; private set; }



        public static DataSourceColumn[] ColumnsForKENALL = new DataSourceColumn[] {
                        DataSourceColumn.JISCode ,
                        DataSourceColumn.PostalCode5,
                        DataSourceColumn.PostalCode,
                        DataSourceColumn.PrefectureRuby,
                        DataSourceColumn.CityRuby,
                        DataSourceColumn.TownRuby,
                        DataSourceColumn.Prefecture,
                        DataSourceColumn.City,
                        DataSourceColumn.Town,
                        DataSourceColumn.TownHasMultipleCodes,
                        DataSourceColumn.KoazaHasBlockNumbers,
                        DataSourceColumn.TownHasBlockNumbers,
                        DataSourceColumn.TownsSharePostalCode,
                        DataSourceColumn.UpdateStatus,
                        DataSourceColumn.UpdateFor
                    };
        public static DataSourceColumn[] ColumnsForRome = new DataSourceColumn[] {
                        DataSourceColumn.PostalCode,
                        DataSourceColumn.Prefecture,
                        DataSourceColumn.City,
                        DataSourceColumn.Town,
                        DataSourceColumn.PrefectureRuby,
                        DataSourceColumn.CityRuby,
                        DataSourceColumn.TownRuby
                    };
        public static DataSourceColumn[] ColumnsForJigyosyo = new DataSourceColumn[] {
                        DataSourceColumn.JISCode,
                        DataSourceColumn.BusinessRuby,
                        DataSourceColumn.Business,
                        DataSourceColumn.Prefecture,
                        DataSourceColumn.City,
                        DataSourceColumn.Town,
                        DataSourceColumn.Koaza,
                        DataSourceColumn.PostalCode,
                        DataSourceColumn.PostalCode5,
                        DataSourceColumn.PostOffice,
                        DataSourceColumn.POB,
                        DataSourceColumn.POBIndex,
                        DataSourceColumn.UpdateFor
                    };
        public static JapanPostDataSourceDefinition UTF = new JapanPostDataSourceDefinition(JapanPostDataSource.UTF)
        {
            Encoding = Encoding.UTF8,
            Url = "https://www.post.japanpost.jp/zipcode/dl/utf/zip/utf_ken_all.zip",
            FileName = "utf_ken_all.csv",
            Columns = ColumnsForKENALL,
            KanaType = KanaType.Katakana,
            HyphenChar = '−' // MINUS SIGN ID 8722 HEXD 2212 UTF8  E2 88 92
        };
        public static JapanPostDataSourceDefinition Kogaki = new JapanPostDataSourceDefinition(JapanPostDataSource.Kogaki)
        {
            Encoding = Encoding.GetEncoding("Shift_JIS"),
            Url = "https://www.post.japanpost.jp/zipcode/dl/kogaki/zip/ken_all.zip",
            FileName = "KEN_ALL.CS",
            Columns = ColumnsForKENALL,
            KanaType = KanaType.Narrow,
            HyphenChar = '－' // FULLWIDTH HYPHEN-MINUS ID 65293 HEXD FF0D UTF8  EF BC 8D
        };
        public static JapanPostDataSourceDefinition Oogaki = new JapanPostDataSourceDefinition(JapanPostDataSource.Oogaki)
        {
            Encoding = Encoding.GetEncoding("Shift_JIS"),
            Url = "https://www.post.japanpost.jp/zipcode/dl/oogaki/zip/ken_all.zip",
            FileName = "KEN_ALL.CSV",
            Columns = ColumnsForKENALL,
            KanaType = KanaType.Narrow,
            HyphenChar = '－' // FULLWIDTH HYPHEN-MINUS ID 65293 HEXD FF0D UTF8  EF BC 8D
        };
        public static JapanPostDataSourceDefinition Rome = new JapanPostDataSourceDefinition(JapanPostDataSource.Rome)
        {
            Encoding = Encoding.GetEncoding("Shift_JIS"),
            Url = "https://www.post.japanpost.jp/zipcode/dl/roman/KEN_ALL_ROME.zip",
            FileName = "KEN_ALL_ROME.CSV",
            Columns = ColumnsForRome,
            KanaType = KanaType.None,
            HyphenChar = '－' // FULLWIDTH HYPHEN-MINUS ID 65293 HEXD FF0D UTF8  EF BC 8D
        };
        public static JapanPostDataSourceDefinition Jigyosyo = new JapanPostDataSourceDefinition(JapanPostDataSource.Jigyosyo)
        {
            Encoding = Encoding.GetEncoding("Shift_JIS"),
            Url = "https://www.post.japanpost.jp/zipcode/dl/jigyosyo/zip/jigyosyo.zip",
            FileName = "JIGYOSYO.CSV",
            Columns = ColumnsForJigyosyo,
            KanaType = KanaType.Narrow,
            HyphenChar = '－' // FULLWIDTH HYPHEN-MINUS ID 65293 HEXD FF0D UTF8  EF BC 8D
        };

        public static Dictionary<JapanPostDataSource, JapanPostDataSourceDefinition> DataSourceMap =
            new Dictionary<JapanPostDataSource, JapanPostDataSourceDefinition>
            {
                        { JapanPostDataSource.UTF, UTF } ,
                        { JapanPostDataSource.Kogaki, Kogaki},
                        { JapanPostDataSource.Oogaki, Oogaki },
                        { JapanPostDataSource.Rome, Rome },
                        { JapanPostDataSource.Jigyosyo, Jigyosyo },
            };
    }
}
