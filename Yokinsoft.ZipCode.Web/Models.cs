using System.Text.Json;
using System.Text.Json.Serialization;

namespace Yokinsoft.ZipCode.Web
{
    /// <summary>
    /// WebAPI からの応答メッセージの area 要素に対応する JSON モデル。
    /// </summary>
    [JsonDerivedType(typeof(Place))]
    public class Area
    {
        [JsonPropertyName("address")]
        public string Name {  get; set; }
        [JsonPropertyName("post-code")]
        public string PostCode {  get; set; }
        [JsonPropertyName("post-code5")] 
        public string PostCode5 { get; set; }
        [JsonPropertyName("jis-code")]
        public string JISCode { get; set; }
        [JsonPropertyName("address-kana")]
        public string NameKana { get; set; }
        [JsonPropertyName("address-rome"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string NameRome { get; set; }
        [JsonPropertyName("business-place"),JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public Place Place { get; set; }
        [JsonPropertyName("end"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool End { get; set; }
        [JsonPropertyName("ambiguous"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Ambiguous { get; set; }

    }
    /// <summary>
    /// WebAPI からの応答メッセージの place 要素に対応する JSON モデル。
    /// </summary>
    public class Place : Area
    {
        [JsonPropertyName("business-name")]
        public string BusinessName { get; set; }
        [JsonPropertyName("business-name-kana")]
        public string BusinessNameKana { get; set; }
        [JsonPropertyName("pob"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string POB { get; set; }
    }
}
