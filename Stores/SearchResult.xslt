<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl objPerson"
    xmlns:objPerson="urn:kesco-stores-person"
>
    <xsl:output method="xml" indent="yes"/>

  <xsl:param name="total_count" />
  <xsl:param name="return_id" />
  
  <xsl:template match="/">
        <table width="100%" id="SearchResultTable" class="grid">
        <thead>
          <tr class="gridHeader">
            <xsl:if test="$return_id">
              <th width="30px">
                <!-- Колонка со ссылкой на возврат выбранного склада -->
              </th>
            </xsl:if>
            <th width="30px">
            <!-- Колонка со ссылкой на редактирование склада -->
            </th>
            <th width="100px">
              <a href="javascript: SortResultTable('ТипыСкладов.ТипСклада');">Тип склада</a>
            </th>
            <th width="100px">
              <a href="javascript: SortResultTable('МестоХранения');">Место хранения</a>
            </th>
            <th width="100px">
              <a href="javascript: SortResultTable('Склады.Филиал');">Отделение/Филиал</a>
            </th>
            <th width="100px">
              <a href="javascript: SortResultTable('Склады.Склад');">Название</a>
            </th>
            <th width="100px">
              <a href="javascript: SortResultTable('Склады.IBAN');">IBAN</a>
            </th>
            <th width="100px">
              <a href="javascript: SortResultTable('Ресурсы.РесурсРус');">Ресурс/Валюта</a>
            </th>
            <th width="100px">
              <a href="javascript: SortResultTable('Хранитель');">Хранитель</a>
            </th>
            <th width="100px">
              <a href="javascript: SortResultTable('Распорядитель');">Распорядитель</a>
            </th>
            <th width="100px">
              <a href="javascript: SortResultTable('ПодразделениеРаспорядителя');">Подразделение распорядителя</a>
            </th>
            <th>
              <a href="javascript: SortResultTable('Склады.Примечание');">Примечание</a>
            </th>
            <th width="60px">
              <a href="javascript: SortResultTable('Склады.От');">Начало действия</a>
            </th>
            <th width="60px">
              <a href="javascript: SortResultTable('По');">Конец действия</a>
            </th>
          </tr>
        </thead>
        <tbody>
        <xsl:apply-templates select="DocumentElement" />
        </tbody>
      </table>
    <div id="countDiv">Найдено [<xsl:value-of select="$total_count"/>] записей</div>
    </xsl:template>

  <xsl:template match="Data">
    <tr>
      <xsl:if test="$return_id">
      <td>
        <xsl:variable name="store" select="Склад"/>
        <xsl:variable name="iban" select="IBAN"/>
        <xsl:variable name="store_display_name">
          <xsl:choose>
            <xsl:when test="string-length($store) &gt; 0 and string-length($iban) &gt; 0">
              <xsl:value-of select="concat($store,'/',$iban)"/>
            </xsl:when>
            <xsl:when test="string-length($store) &gt; 0">
              <xsl:value-of select="$store"/>
            </xsl:when>
            <xsl:when test="string-length($iban) &gt; 0">
              <xsl:value-of select="$iban"/>
            </xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <a href="javascript: ReturnValue({КодСклада}, '{$store_display_name}');">
           <img src='/Styles/BackToList.gif' />
        </a>
      </td>
      </xsl:if>
      <td>
        <div class="actions_container">
          <input alt="Редактировать" title="Изменить параметры склада" onclick="EditStore({КодСклада});" type="image" src="/Styles/Edit.gif"/>
        </div>
      </td>
      <td>
         <xsl:value-of select="ТипСклада" />
      </td>
      <td>
        <xsl:value-of select="МестоХранения" />
      </td>
      <td>
        <xsl:value-of select="Филиал" />
      </td>
      <td>
        <xsl:value-of select="Склад" />
      </td>
      <td>
        <xsl:value-of select="IBAN" />
      </td>
      <td>
        <xsl:value-of select="РесурсРус" />
      </td>
      <td>
        <xsl:value-of disable-output-escaping="yes" select="objPerson:GetLinkToPersonInfo(КодХранителя,Хранитель)"/>
      </td>
      <td>
        <xsl:value-of disable-output-escaping="yes" select="objPerson:GetLinkToPersonInfo(КодРаспорядителя,Распорядитель)"/>
      </td>
      <td>
        <xsl:value-of select="ПодразделениеРаспорядителя" />
      </td>
      <td>
        <xsl:value-of select="Примечание" />
      </td>
      <td>
        <xsl:if test="substring-before(От, 'T')">
          <xsl:value-of select="concat(substring(От, 9, 2), '.', substring(От, 6, 2), '.', substring(От, 1, 4))" />
        </xsl:if>
      </td>
      <td>
        <xsl:if test="substring-before(По, 'T')">
          <xsl:value-of select="concat(substring(По, 9, 2), '.', substring(По, 6, 2), '.', substring(По, 1, 4))" />
        </xsl:if>
      </td>      
    </tr> 
  </xsl:template>

</xsl:stylesheet>
