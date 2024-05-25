using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;
using Yokinsoft.ZipCode.Data;

namespace Yokinsoft.ZipCode.Web
{
    /// <summary>
    /// 郵便番号検索結果の WebAPI からの応答メッセージの JSON モデル。
    /// PostCodeData.Lookup の検索結果を格納する XML オブジェクトを WebAPI の JSON 応答メッセージに変換するためのクラス。
    /// </summary>
    public class LookupObjectResponse : Dictionary<string,object>
    {
        public List<Area> Result { get; set; }
        internal LookupObjectResponse( LookupXmlResponse response)
        {
            this["date"] = DateTime.Now.ToString("O");
            foreach( var kv in response.DocumentElement.Attributes.Cast<XmlAttribute>())
            {
                this[kv.Name] = kv.Value;
            }
                
            Result = response.DocumentElement!.ChildNodes.Cast<XmlElement>()
            .Select(m => new Area
            {
                Name = m.GetAttribute("name"),
                PostCode = m.GetAttribute("post-code"),
                PostCode5 = m.GetAttribute("post-code5"),
                JISCode = m.GetAttribute("jis-code"),
                NameKana = m.GetAttribute("name-kana"),
                NameRome = m.HasAttribute("name-rome") ? m.GetAttribute("name-rome") : null,
                End = m.HasAttribute("end") ? bool.Parse(m.GetAttribute("end")) : false,
                Ambiguous = m.HasAttribute("ambiguous") ? bool.Parse(m.GetAttribute("ambiguous")) : false,
                Place = m.HasChildNodes ? m.ChildNodes.Cast<XmlElement>()
                .Select(m => new Place
                {
                    Name = m.GetAttribute("name"),
                    PostCode = m.GetAttribute("post-code"),
                    PostCode5 = m.GetAttribute("post-code5"),
                    JISCode = m.GetAttribute("jis-code"),
                    NameKana = m.GetAttribute("name-kana"),
                    POB = m.HasAttribute("pob") ? m.GetAttribute("pob") : null,
                    End = true
                }).FirstOrDefault() : null
            })
            .SelectMany(m => m.Place == null ?
                new[] { m } :
                new[] { string.IsNullOrEmpty(m.PostCode) ? null : m, new Place
                {
                    BusinessName = m.Place.Name,
                    BusinessNameKana = m.Place.NameKana,
                    Name = m.Name,
                    PostCode = m.Place.PostCode ?? m.PostCode,
                    PostCode5 = m.Place.PostCode5 ?? m.PostCode5,
                    JISCode = m.Place.JISCode ?? m.JISCode,
                    NameKana = m.NameKana,
                    POB = m.Place.POB,
                    End = m.End
                }}.Where( mm => mm != null ) )
            .ToList();
            this["result"] = Result;
        }
    }
}
