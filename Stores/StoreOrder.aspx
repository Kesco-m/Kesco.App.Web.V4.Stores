<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StoreOrder.aspx.cs" Inherits="Kesco.App.Web.Stores.StoreOrder" %>
<%@ Import Namespace="Kesco.Lib.Localization" %>
<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title><%= Resources.Resx.GetString("STORE_StoreReport") %></title>

    <link rel="stylesheet" type="text/css" href="Kesco.Stores.css"/>
    <script src="Kesco.Stores.js?v=1" type="text/javascript"></script>
    <script src="Kesco.StoreOrder.js?v=1" type="text/javascript"></script>
</head>
<body>
<%= RenderDocumentHeader() %>

<div class="page_content">
    <!-- page_content -->
    <h1 class="v4pageTitle"><%= Resources.Resx.GetString("STORE_ReportTitle") %></h1>

    <div class="predicate_block">
        <!--Тип отчета по складам-->
        <div class="label"><%= Resources.Resx.GetString("STORE_ReportType") %>:</div>
        <dbs:DBSStoreReportType ID="sStoreReportType" runat="server" MaxItemsInPopup="16" Width="200px" CSSclass="aligned_control" OnChanged="OnReportTypeChanged" OnValueChanged="OnReportTypeValueChanged"></dbs:DBSStoreReportType>
    </div>

    <div id="SelectedStoresGroup">

        <div><%= Resources.Resx.GetString("STORE_ReportSelected") %>:</div>
        <div id="SelectedStores"></div>
        <a id="moveFirst" href="javascript: rowsUp(UpdateResultTable.pageSize);" class="action_link"><%= Resources.Resx.GetString("STORE_ReportMoveToBegin") %></a>
        <a id="moveLast" href="javascript: rowsDown(UpdateResultTable.pageSize);" class="action_link"><%= Resources.Resx.GetString("STORE_ReportMoveToEnd") %></a>
        <a id="deleteSelected" href="javascript: deleteSelected();" class="action_link"><%= Resources.Resx.GetString("STORE_ReportDeleteFrom") %></a>
        <a id="inserAfterSelected" href="javascript: setSelectedRowsAfter();" class="action_link"><%= Resources.Resx.GetString("STORE_ReportInsertAfter") %>:</a>
        <input type="text" id="insertAfterRow" size="30" onkeydown="onInsertAfterKeyDown();"/>
    </div>

    <!--Таблица с результатами поиска-->
    <div id="StoreReport" class="result_table"></div>
</div> <!-- page_content -->

</body>
</html>