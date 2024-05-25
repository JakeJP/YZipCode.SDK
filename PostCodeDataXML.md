# 郵便番号データフォーマット(XML)

ConvXML.exe は日本郵便が配布する各種CSVファイルを処理・整形して１つの XML ファイルを出力します。XMLファイルは `Yokinsoft.ZipCode.Data.dll` などの
ライブラリから検索用のデータベースとして機能します。

このドキュメントではXMLファイルのフォーマットについて解説します。

## [住所]を構成する要素の概念（住所の階層構造）




## XML基本要素

- **&lt;area&gt;** 住所の断片要素を表す要素。
- **&lt;place&gt;** 事業所を表す要素


### &lt;area&gt;

住所文字列の断片を階層的に表現するための要素。XMLドキュメントのルート要素は **国** (日本全体）を表し、以下に、**都道府県**,
**市区町村** と続きます。

```
北海道札幌市中央区旭ケ丘５丁目６番５０号
```

という住所は

```xml
<area name="Japan">
    <area name="北海道">
        <area name="札幌市中央区">
            <area name="旭ケ丘">
                <area name="５丁目６番５０号">
                </area>
            </area>
        </area>
    </area>
</area>
```

のように表現されます。住所文字列の分割点は日本郵便配布のCSVが基準となるため市と区が１つになっている場合がありますが、
XML上の表現として、分割点は任意で
```xml
<area name="Japan">
    <area name="北海道">
        <area name="札幌市">
            <area name="中央区">
                <area name="旭ケ丘">
                    <area name="５丁目６番５０号">
                    </area>
                </area>
            </area>
        </area>
    </area>
</area
```

という表現も可能とします。

**area** が都道府県を表しているのか市区町村を表しているのかは `type` 属性がヒントを提供します。

**丁目** などの数字表現は、元データでは **大通西（１〜１９丁目）** などと表現されているものを個々の
**１丁目** **２丁目** と展開して表現している場合があります。展開した結果が１００を大きく超える場合などは
元データのままの表現となっているものあります。

```xml
     <area name="大通西" name-kana="オオドオリニシ" jis-code="01101" post-code="0600042" post-code5="060" name-rome="ODORINISHI">
        <area name="１丁目" name-kana="１チョウメ" jis-code="01101" post-code="0600042" post-code5="060" name-rome="1-CHOME" />
        <area name="２丁目" name-kana="２チョウメ" jis-code="01101" post-code="0600042" post-code5="060" name-rome="2-CHOME" />

```

#### 属性

- **name** (必須) 住所文字列の一部。県名、市区町村名など
- **type** `prefecture` `city` など住所断片のヒント
- **post-code** 郵便番号（7桁）
- **post-code5** 郵便番号（5桁）
- **name-kana** 住所文字列のかな表記（全角、半角、ひらがな、カタカナの種別は変換時オプションによる）
- **name-rome** ローマ字表記
- **jis-code** JISコード (全国地方公共団体コード（JIS X0401、X0402)


#### 下位要素

**&lt;area&gt;**

**&lt;place&gt;**


### &lt;place&gt;

#### 属性

- **name** (必須) 住所文字列の一部。県名、市区町村名など
- **post-code**
- **post-code5**
- **name-kana**
- **name-rome**
- **jis-code**
- **POB** 私書箱

#### 下位要素

なし


## 部分サンプル

```xml
<?xml version="1.0" encoding="utf-8"?>
<area name="Japan" cc2="JP" cc3="JPN" >
  <area name="北海道" name-kana="ホッカイドウ" type="prefecture" name-rome="HOKKAIDO">
    <area name="札幌市中央区" name-kana="サッポロシチュウオウク" jis-code="01101" post-code="0600000" post-code5="060" type="city" description="以下に掲載がない場合" description-kana="イカニケイサイガナイバアイ" name-rome="SAPPORO SHI CHUO KU">
      <area name="旭ケ丘" name-kana="アサヒガオカ" jis-code="01101" post-code="0640941" post-code5="064" name-rome="ASAHIGAOKA">
        <area name="５丁目６番５０号">
          <place name="社会福祉法人　札幌慈啓会　慈啓会病院" name-kana="シヤカイフクシホウジン　サツポロジケイカイ　ジケイカイビヨウイン" jis-code="01101" post-code="0648575" post-code5="064" />
        </area>

```