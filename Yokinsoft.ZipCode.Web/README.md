# 陽気に郵便番号検索 [SDK] Yokinsoft.ZipCode.Web.dll

陽気に郵便番号検索 JSON 応答対応ライブラリ

Yokinsoft.ZipCode.Data.dll ライブラリの `PostCodeData` クラスの `Lookup` メソッドは検索結果を`LookupXmlResponse` クラスという `XmlDocument` の派生クラスを返します。
このライブラリは、 XML形式の検索結果を JSON 形式に変換をサポートします。

`System.Text.Json` に依存しています。

## 使い方


```c#
using Yokinsoft.ZipCode.Data;
using Yokinsoft.ZipCode.Web;

var data = new PostCodeData(@"path\to\PostCodeData.xml");
var result = data.Lookup("123);

var resultObj = result.AsObject();

```

