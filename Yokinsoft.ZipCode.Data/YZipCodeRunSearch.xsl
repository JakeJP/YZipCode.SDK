<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="xml" encoding="utf-8" indent="yes"/>
	<xsl:param name="keyword"></xsl:param>
	<xsl:param name="date"></xsl:param>
	<xsl:param name="search-place">true</xsl:param>
	<xsl:param name="search-zipcode">true</xsl:param>
	<xsl:param name="search-address">true</xsl:param>
	<xsl:param name="search-rome">true</xsl:param>
	<xsl:param name="delimiter" xml:space="preserve"> </xsl:param>
	<xsl:param name="delimiter-rome" xml:space="preserve">,</xsl:param>
	<xsl:param name="depth">3</xsl:param>
	<xsl:template match="/*[@cc2='JP']">
		<result generated-by="Yokinsoft.ZipCode">
			<xsl:attribute name="keyword">
				<xsl:value-of select="$keyword"/>
			</xsl:attribute>
			<xsl:attribute name="date">
				<xsl:value-of select="$date"/>
			</xsl:attribute>
			<xsl:for-each select="@*[starts-with(name(), 'date-')]">
				<xsl:copy></xsl:copy>
			</xsl:for-each>
			<xsl:variable name="numberkeyword" select="translate( $keyword, '１２３４５６７８９０ー－-','1234567890')" />
			<xsl:variable name="alphakeyword" select="translate( $keyword, 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ,-','')" />

			<xsl:choose>
				<xsl:when test="not($keyword) and $depth &gt; 0">
					<!-- キーワードなしの場合第１階層を提示-->
					<xsl:apply-templates select="/area/area[$depth &gt; 0]" mode="areaResult"></xsl:apply-templates>
				</xsl:when>
				<xsl:when test="$search-zipcode='true' and  string-length($keyword) &gt; 1 and $keyword != '' and number(substring($numberkeyword,1,1)) &gt;= 0">
					<!-- 数字がキーワードのときは番号検索 -->
					<xsl:apply-templates select="(//area | //place[$search-place='true'])[string-length($numberkeyword) &gt; 2 or count(ancestor::area) &lt; 3][@post-code and (starts-with( @post-code, $numberkeyword ))]" mode="forPostCode">
						<xsl:with-param name="postcode" select="$numberkeyword" />
						<xsl:sort select="string-length(@post-code)" data-type="number" order="descending"/>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:when test="$search-rome='true' and string-length($keyword) &gt; 3 and $keyword != '' and $alphakeyword=''">
					<!-- すべてアルファベットの場合はローマ字検索 -->
					<xsl:variable name="kw" select="translate( $keyword, 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz,-','ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ')" />
					<xsl:variable name="max-depth">
						<xsl:choose>
							<xsl:when test="area[starts-with($kw, @name-rome)]">1</xsl:when>
							<xsl:when test="string-length($kw) &lt; 3">2</xsl:when>
							<xsl:otherwise>2</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
					<xsl:if test="$search-address='true'">
						<!--<xsl:apply-templates select="//area[starts-with( $keyword, @name ) or ( $keyword!='' and  starts-with( @name, $keyword ))] | /area/area[( $keyword='' and $depth &gt; 0 and count(ancestor::area)=1)]" mode="forAddressText">-->
						<xsl:apply-templates select="//area[count(ancestor::area) &lt;= $max-depth and (starts-with( $kw, @name-rome ) or ( starts-with( @name-rome, $kw )))]" mode="forAddressTextRome">
							<xsl:with-param name="address" select="string($kw)"/>
							<xsl:sort select="string-length(@name-rome)" data-type="number" order="descending"/>
						</xsl:apply-templates>
					</xsl:if>
					<xsl:if test="$search-place='true' and string-length($kw) &gt; 2">
						<xsl:apply-templates select="//place[contains(@name-rome,$kw)]" mode="forAddressTextRome">
							<xsl:with-param name="address" select="string($kw)"/>
							<xsl:sort select="string-length(@name-rome)" data-type="number" order="descending"/>
						</xsl:apply-templates>
					</xsl:if>
				</xsl:when>
				<xsl:when test="$keyword">
					<!-- 文字列のときは住所検索 -->
					<xsl:variable name="kw" select="translate($keyword,' ','')" />
					<xsl:variable name="max-depth">
						<xsl:choose>
							<xsl:when test="area[starts-with($kw, @name)]">1</xsl:when>
							<xsl:when test="string-length($kw) &lt; 3">2</xsl:when>
							<xsl:otherwise>2</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
					<xsl:if test="$search-address='true'">
						<!--<xsl:apply-templates select="//area[starts-with( $keyword, @name ) or ( $keyword!='' and  starts-with( @name, $keyword ))] | /area/area[( $keyword='' and $depth &gt; 0 and count(ancestor::area)=1)]" mode="forAddressText">-->
						<xsl:apply-templates select="//area[count(ancestor::area) &lt;= $max-depth and (starts-with( $keyword, @name ) or ( starts-with( @name, $keyword )))]" mode="forAddressText">
							<xsl:with-param name="address" select="string($kw)"/>
							<xsl:sort select="string-length(@name)" data-type="number" order="descending"/>
						</xsl:apply-templates>
					</xsl:if>
					<xsl:if test="$search-place='true' and string-length($kw) &gt; 2">
						<xsl:apply-templates select="//place[contains(@name,$keyword)]" mode="forAddressText">
							<xsl:with-param name="address" select="string($kw)"/>
							<xsl:sort select="string-length(@name)" data-type="number" order="descending"/>
						</xsl:apply-templates>
					</xsl:if>
				</xsl:when>
			</xsl:choose>
		</result>
	</xsl:template>
	<xsl:template match="@*" mode="resultElementAtt">
		<xsl:variable name="localAttName" select="name()" />
		<xsl:attribute name="{name()}">
			<xsl:apply-templates select="ancestor::area[position() &lt; last()]" mode="resultAreaElement">
				<xsl:with-param name="attname" select="$localAttName" />
			</xsl:apply-templates>
		</xsl:attribute>
	</xsl:template>
	<xsl:template match="area" mode="resultAreaElement">
		<xsl:param name="attname">name</xsl:param>
		<xsl:value-of select="@*[name()=$attname]" />
		<xsl:if test="position() &lt; last()">
			<xsl:choose>
				<xsl:when test="$attname='name-rome'">
					<xsl:value-of select="$delimiter-rome"/>
				</xsl:when>
				<xsl:otherwise><xsl:value-of select="$delimiter"/></xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>
	<xsl:template match="area" name="printAreaResult" mode="areaResult">
		<xsl:param name="place-name"/>
		<xsl:param name="end-point" />

		<xsl:element name="{name()}">
			<xsl:apply-templates select="@*[starts-with(name(),'name')]" mode="resultElementAtt">
			</xsl:apply-templates>

			<xsl:for-each select="@*[not(starts-with(name(),'name'))]">
				<xsl:attribute name="{name()}">
					<xsl:value-of select="."/>
				</xsl:attribute>
			</xsl:for-each>

			<xsl:attribute name="depth">
				<xsl:value-of select="count(ancestor::*)"/>
			</xsl:attribute>
			<xsl:if test="not(area) or $end-point='true' or ($search-place='false' and not(descendant::area[@post-code]))">
				<xsl:attribute name="end">true</xsl:attribute>
			</xsl:if>
			<xsl:if test="$search-place='true' and place">
				<xsl:copy-of select="place[not($place-name) or @name=$place-name]"/>
			</xsl:if>
		</xsl:element>
	</xsl:template>

	<xsl:template match="place" mode="forPostCode">
		<xsl:apply-templates select=".." mode="areaResult">
			<xsl:with-param name="place-name" select="@name" />
		</xsl:apply-templates>
	</xsl:template>
	<xsl:template match="area" mode="forPostCode">
		<xsl:param name="postcode" />
		<xsl:call-template name="printAreaResult">
			<xsl:with-param name="end-point">
				<xsl:if test="string-length($postcode) = 7">true</xsl:if>
			</xsl:with-param>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="place" mode="forAddressText">
		<xsl:apply-templates select=".." mode="areaResult">
			<xsl:with-param name="place-name" select="@name" />
		</xsl:apply-templates>

	</xsl:template>
	<xsl:template match="area" mode="forAddressText">
		<xsl:param name="address" />
		<xsl:param name="matched"/>
		<xsl:param name="current-depth" select="0"></xsl:param>
		<xsl:variable name="exactmatch" select="$address!='' and string-length(@name)=string-length($address)"/>
		<xsl:variable name="subaddress">
			<xsl:value-of select="substring($address, string-length(@name)+1)"/>
		</xsl:variable>
		<!--
		<xsl:if test="($depth!=0 and $subaddress='') or (string-length($matched) &gt; 3 and not(area[not(place)])) or ($depth=0 and @post-code and string-length($matched) &gt; 2)">-->
		<xsl:if test="($subaddress='' and ($search-place='true' or area or @post-code)) or (@post-code and not(area[starts-with($subaddress,@name) or starts-with(@name,$subaddress)]))">
			<xsl:call-template name="printAreaResult">
				<xsl:with-param name="extra" select="$subaddress"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="$depth=0 or ( ($subaddress!='' or $exactmatch) and $current-depth &lt; $depth )">
			<xsl:apply-templates select="area[starts-with( @name,  $subaddress ) or starts-with( $subaddress, @name ) or string-length($subaddress)=0]" mode="forAddressText">
				<xsl:with-param name="address" select="$subaddress"/>
				<xsl:with-param name="matched" select="concat( $matched, substring($address, 1, string-length(@name)))"/>
				<xsl:with-param name="current-depth">
					<xsl:choose>
						<xsl:when test="string-length($subaddress)=0">
							<xsl:value-of select="$current-depth + 1"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$current-depth"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:with-param>
			</xsl:apply-templates>
		</xsl:if>
	</xsl:template>
	<xsl:template match="area" mode="forAddressTextRome">
		<xsl:param name="address" />
		<xsl:param name="matched"/>
		<xsl:param name="current-depth" select="0"></xsl:param>
		<xsl:variable name="exactmatch" select="$address!='' and string-length(@name-rome)=string-length($address)"/>
		<xsl:variable name="subaddress">
			<xsl:value-of select="substring($address, string-length(@name-rome)+1)"/>
		</xsl:variable>
		<!--
		<xsl:if test="($depth!=0 and $subaddress='') or (string-length($matched) &gt; 3 and not(area[not(place)])) or ($depth=0 and @post-code and string-length($matched) &gt; 2)">-->
		<xsl:if test="($subaddress='' and ($search-place='true' or area or @post-code))  or (@post-code and not(area[starts-with($subaddress,@name-rome) or starts-with(@name-rome,$subaddress)]))">
			<xsl:call-template name="printAreaResult">
				<xsl:with-param name="extra" select="$subaddress"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="$depth=0 or ( ($subaddress!='' or $exactmatch) and $current-depth &lt; $depth )">
			<xsl:apply-templates select="area[@name-rome and (starts-with( @name-rome,  $subaddress ) or starts-with( $subaddress, @name-rome ) or string-length($subaddress)=0)]" mode="forAddressTextRome">
				<xsl:with-param name="address" select="$subaddress"/>
				<xsl:with-param name="matched" select="concat( $matched, substring($address, 1, string-length(@name-rome)))"/>
				<xsl:with-param name="current-depth">
					<xsl:choose>
						<xsl:when test="string-length($subaddress)=0">
							<xsl:value-of select="$current-depth + 1"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$current-depth"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:with-param>
			</xsl:apply-templates>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>