<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl objPerson"
                xmlns:objPerson="urn:kesco-stores-person">
  <xsl:output method="xml" indent="yes" />

  <xsl:param name="current_page" select="1" />
  <xsl:param name="page_size" select="0" />
  <xsl:param name="total_count" />

  <xsl:template match="/">
    <table width="100%" id="StoreReportTable" class="grid">
      <thead>
        <tr class="gridHeader">
          <th>
            <div class="actions_container">
              <input id="selectionUp" alt="Вверх" title="Переместить выбранные строки вверх" onclick="rowsUp(1);"
                     type="image" src="/STYLES/UpGrayed.gif" />
              <input id="selectionDown" alt="Вниз" title="Переместить выбранные строки вниз" onclick="rowsDown(1);"
                     type="image" src="/STYLES/DownGrayed.gif" />
            </div>
          </th>
          <th><!-- Номер строки -->№</th>
          <th>Распорядитель</th>
          <th>Ресурс/Валюта</th>
          <th>Тип склада</th>
          <th>Хранитель</th>
          <th>Название</th>
          <th>Начало действия</th>
          <th>Конец действия</th>
        </tr>
      </thead>
      <tbody>
        <xsl:apply-templates select="DocumentElement" />
      </tbody>
    </table>
    <div id="countDiv">Найдено [<xsl:value-of select="$total_count" />] записей</div>
    <!--Специально для вызова функции после загрузки таблицы-->
    <img src="" onerror="UpdateResultTable({$current_page},{$page_size})" />
  </xsl:template>

  <xsl:template match="Data">
    <tr>
      <td width="50px" height="30px">
        <div class="actions_container">
          <input type="checkbox" id="{КодСклада}" title="Выбрать склад" onclick="OnCheckStore();" />
          <!--xsl:text--> <!--/xsl:text-->
          <!--input alt="Редактировать" title="Изменить параметры склада" onclick="EditStore({КодСклада});" type="image" src="/Styles/Edit.gif"/ !-->
        </div>
      </td>
      <td width="20px">
        <xsl:variable name="row_number">
          <xsl:number />
        </xsl:variable>
        <xsl:value-of select="( $current_page - 1) * $page_size + $row_number" />
      </td>
      <td width="150px">
        <xsl:value-of disable-output-escaping="yes"
                      select="objPerson:GetLinkToPersonInfo(КодРаспорядителя,Распорядитель)" />
      </td>
      <td width="100px">
        <xsl:value-of select="Ресурс" />
      </td>
      <td width="100px">
        <xsl:value-of select="ТипСклада" />
      </td>
      <td width="150px">
        <xsl:value-of disable-output-escaping="yes" select="objPerson:GetLinkToPersonInfo(КодХранителя,Хранитель)" />
      </td>
      <td>
        <xsl:value-of select="Склад" />
      </td>
      <td width="80px">
        <xsl:if test="substring-before(От, 'T')">
          <xsl:value-of select="concat(substring(От, 9, 2), '.', substring(От, 6, 2), '.', substring(От, 1, 4))" />
        </xsl:if>
      </td>
      <td width="80px">
        <xsl:if test="substring-before(По, 'T')">
          <xsl:value-of select="concat(substring(По, 9, 2), '.', substring(По, 6, 2), '.', substring(По, 1, 4))" />
        </xsl:if>
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet>