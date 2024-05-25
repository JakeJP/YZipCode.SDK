# 陽気に郵便番号検索 [SDK] Yokinsoft.ZipCode.Data.dll

陽気に郵便番号検索 検索エンジン（XML データファイル読み込み）

ConvXML ツールなどが出力する 郵便番号データである XML ファイルをメモリ中に読み込み、郵便番号・住所検索を実行します。


## サンプル

```c#
using Yokinsoft.ZipCode.Data;
var data = new PostCodeData(@"Path\To\PostCodeData.xml");

var result = data.Lookup("123-4567");

```