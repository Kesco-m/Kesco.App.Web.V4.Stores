<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Search.aspx.cs" Inherits="Kesco.App.Web.Stores.Search" %>
<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="cs" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<%@ Register TagPrefix="csp" Namespace="Kesco.Lib.Web.Controls.V4.PagingBar" Assembly="Controls.V4" %>
<%@ Import Namespace="Kesco.Lib.Localization" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title><%= Resources.Resx.GetString("STORE_FindStore")%></title>
    <link rel="stylesheet" type="text/css" href="Stores.css" />
    <script src='Stores.js' type='text/javascript'></script>
</head>
<body>

    <form id="mvcDialogResult" method="post">
        <input type="hidden" name="escaped" value="0" />
		<input type="hidden" name="control" value="" />
        <input type="hidden" name="multiReturn" value="" />
		<input type="hidden" name="value" value="" />
    </form>

<%=RenderDocumentHeader()%>

<div class="page_content"><!-- page_content -->

<h1 class="v4pageTitle"><%= Resources.Resx.GetString("STORE_Title")%></h1>

<div id="SearchFilter">

<!--div--> <!--Первая строка условий-->
<div class="predicate_block" id="predText"><!--Произвольная строка поиска-->
<div class="label" ><%= Resources.Resx.GetString("STORE_Text")%>:</div>
<cs:TextBox ID="txtText" runat="server" Width="220px" IsUseCondition="True" CSSClass="aligned_control" HasCheckbox="True"></cs:TextBox>
</div>

<div class="predicate_block" id="predStore"><!--Список складов-->
<div class="label" ><%= Resources.Resx.GetString("STORE_Stores")%>:</div>
<dbs:DBSStore ID="sStore" runat="server" Width="200px" CLID="75" IsUseCondition="True" IsNotUseEmpty="True" IsMultiSelect="True" IsRemove="True" HasCheckbox="True" AutoSetSingleValue="True" CSSClass="aligned_control"></dbs:DBSStore>
</div>

<div class="predicate_block" id="predValid"><!--Период действия склада-->
<div class="label" ><%= Resources.Resx.GetString("STORE_IsActual")%>:</div>
<div class="wrap_inline_block">
<div class="aligned_control">
<cs:FilterGroupBox ID="dateValidPeriodFilter" runat="server" Value="0" HasCheckbox="True">
<div class="aligned_date">
<cs:DatePicker ID="dateValid" runat="server" ></cs:DatePicker>
</div>
</cs:FilterGroupBox>
</div>
</div>
</div>
<!--/div--><!--Первая строка условий-->

<div class="predicate_block" id="predType"><!--Тип склада-->
<div class="label" ><%= Resources.Resx.GetString("STORE_StoreType")%>:</div>
<dbs:DBSStoreType ID="sStoreType" runat="server" MaxItemsInPopup="16" Width="200px" IsUseCondition="True"
 IsMultiSelect="True" IsRemove="True" HasCheckbox="True" AutoSetSingleValue="True" CSSClass="aligned_control"></dbs:DBSStoreType>
</div>

<div class="predicate_block" id="predStorage"><!--Место хранения-->
<div class="label" ><%= Resources.Resx.GetString("STORE_Residence")%>:</div>
<dbs:DBSResidence ID="sStorage" runat="server" Width="200px" CLID="26" IsUseCondition="True" IsMultiSelect="True"
 IsRemove="True" HasCheckbox="True" AutoSetSingleValue="True" CSSClass="aligned_control"></dbs:DBSResidence>
</div>

<div class="predicate_block" id="predName"><!--Название склада или номер счета-->
<div class="label" ><%= Resources.Resx.GetString("STORE_Name") + '/' + Resources.Resx.GetString("STORE_Account")%>:</div>
<cs:StoreNameTextBox ID="txtName" runat="server" Width="220px" IsUseCondition="True" HasCheckbox="True" CSSClass="aligned_control"></cs:StoreNameTextBox>
</div>

<div class="predicate_block" id="predIban"><!--IBAN номар счета-->
<div class="label" ><%= Resources.Resx.GetString("STORE_IBAN")%>:</div>
<cs:TextBox ID="txtIBAN" runat="server" Width="220px" IsUseCondition="True" HasCheckbox="True" CSSClass="aligned_control"></cs:TextBox>
</div>

<div class="predicate_block" id="predKeeper"><!--Хранитель-->
<div class="label" ><%= Resources.Resx.GetString("STORE_Keeper") + '/' + Resources.Resx.GetString("STORE_Bank")%>:</div>
<dbs:DBSPerson ID="sKeeperBank" runat="server" Width="200px" CLID="15" IsUseCondition="True" IsAlwaysAdvancedSearch="True"
 IsMultiSelect="True" IsRemove="True" HasCheckbox="True" IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control"></dbs:DBSPerson>
</div>

<div class="predicate_block" id="predManager"><!--Распорядитель-->
<div class="label" ><%= Resources.Resx.GetString("STORE_Manager")%>:</div>
<dbs:DBSPerson ID="sManager" runat="server" Width="200px" CLID="16" IsUseCondition="True" IsAlwaysAdvancedSearch="True"
 IsMultiSelect="True" IsRemove="True" HasCheckbox="True" IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control"></dbs:DBSPerson>
</div>

<div class="predicate_block" id="predManagerDepartment"><!--Подразделение распорядителя-->
<div class="label" ><%= Resources.Resx.GetString("STORE_ManagerDepartment")%>:</div>
<dbs:DBSPersonDepartment ID="sManagerDepartment" MaxItemsInPopup="16" runat="server" Width="200px" IsUseCondition="True"
 IsMultiSelect="True" IsRemove="True" HasCheckbox="True" OnBeforeSearch="OnManagerDepartmentBeforeSearch" AutoSetSingleValue="True" CSSClass="aligned_control"></dbs:DBSPersonDepartment>
</div>

<div class="predicate_block" id="predAgreement"><!--Договор храненния-->
<div class="label" ><%= Resources.Resx.GetString("STORE_Agreement")%>:</div>
<dbs:DBSDocument ID="sAgreement" runat="server" Width="200px" IsUseCondition="True" IsMultiSelect="True"
 IsRemove="True" HasCheckbox="True" OnBeforeSearch="OnAgreementBeforeSearch" AutoSetSingleValue="True" CSSClass="aligned_control"></dbs:DBSDocument>
 </div>

<div class="predicate_block" id="predResource"><!--Хранимый ресурс-->
<div class="label" ><%= Resources.Resx.GetString("STORE_Resource") + '/' + Resources.Resx.GetString("STORE_Currency")%>:</div>
<dbs:DBSResource ID="sResource" runat="server" Width="200px" CLID="17" IsAlwaysAdvancedSearch="true"
 IsUseCondition="True" IsMultiSelect="True" IsRemove="True" HasCheckbox="True" OnBeforeSearch="OnResourceBeforeSearch" AutoSetSingleValue="True" CSSClass="aligned_control"></dbs:DBSResource>
 </div>

<div class="predicate_block" id="predDepartment"><!--Отделение или филиал банка-->
<div class="label" ><%= Resources.Resx.GetString("STORE_Department") + '/' + Resources.Resx.GetString("STORE_Branch")%>:</div>
<cs:TextBox ID="txtStoreDepartment" runat="server" Width="220px" IsUseCondition="True" HasCheckbox="True" CSSClass="aligned_control"></cs:TextBox>
</div>

<div class="predicate_block" id="predDescription"><!--Примечание-->
<div class="label" ><%= Resources.Resx.GetString("STORE_Description")%>:</div>
<cs:TextBox ID="txtDescription" runat="server" Width="220px" IsUseCondition="True" HasCheckbox="True" CSSClass="aligned_control"></cs:TextBox>
</div>

<div> <!--Последняя строка условий-->
<div class="predicate_block" id="predQuery"><!--Дополнительные запросы-->
<div class="label" ><%= Resources.Resx.GetString("STORE_Queries")%>:</div>
<dbs:DBSSqlQuery ID="sQuery" runat="server" Width="200px" CLID="75" IsMultiSelect="True" IsRemove="True" HasCheckbox="True" QueryType="Склады" AutoSetSingleValue="True" CSSClass="aligned_control"></dbs:DBSSqlQuery>
</div>
</div><!--Последняя строка условий-->

</div><!-- SearchFilter -->

<!-- Диалог настройки полей поиска-->
<div id="setupForm" style="display: none;">
<h1><%= Resources.Resx.GetString("STORE_SetupFields")%>:</h1>
<div id="setupFormPredicates">
<div><input type="checkbox" id="display_Store" checked="checked"/><label for="display_Store"><%= Resources.Resx.GetString("STORE_Store")%></label></div>
<div><input type="checkbox" id="display_Valid" checked="checked"/><label for="display_Valid"><%= Resources.Resx.GetString("STORE_IsActual")%></label></div>
<div><input type="checkbox" id="display_Type" checked="checked" /><label for="display_Type"><%= Resources.Resx.GetString("STORE_StoreType")%></label></div>
<div><input type="checkbox" id="display_Storage" checked="checked" /><label for="display_Storage"><%= Resources.Resx.GetString("STORE_Residence")%></label></div>
<div><input type="checkbox" id="display_Name" checked="checked" /><label for="display_Name"><%= Resources.Resx.GetString("STORE_Name") + '/' + Resources.Resx.GetString("STORE_Account")%></label></div>
<div><input type="checkbox" id="display_Iban" checked="checked" /><label for="display_Iban"><%= Resources.Resx.GetString("STORE_IBAN")%></label></div>
<div><input type="checkbox" id="display_Keeper" checked="checked" /><label for="display_Keeper"><%= Resources.Resx.GetString("STORE_Keeper") + '/' + Resources.Resx.GetString("STORE_Bank")%></label></div>
<div><input type="checkbox" id="display_Manager" checked="checked" /><label for="display_Manager"><%= Resources.Resx.GetString("STORE_Manager")%></label></div>
<div><input type="checkbox" id="display_ManagerDepartment" checked="checked" /><label for="display_ManagerDepartment"><%= Resources.Resx.GetString("STORE_ManagerDepartment")%></label></div>
<div><input type="checkbox" id="display_Agreement" checked="checked" /><label for="display_Agreement"><%= Resources.Resx.GetString("STORE_Agreement")%></label></div>
<div><input type="checkbox" id="display_Resource" checked="checked" /><label for="display_Resource"><%= Resources.Resx.GetString("STORE_Resource") + '/' + Resources.Resx.GetString("STORE_Currency")%></label></div>
<div><input type="checkbox" id="display_Department" checked="checked" /><label for="display_Department"><%= Resources.Resx.GetString("STORE_Department") + '/' + Resources.Resx.GetString("STORE_Branch")%></label></div>
<div><input type="checkbox" id="display_Description" checked="checked" /><label for="display_Description"><%= Resources.Resx.GetString("STORE_Description")%></label></div>
<div><input type="checkbox" id="display_Query" checked="checked"/><label for="display_Query"><%= Resources.Resx.GetString("STORE_Queries")%></label></div>
</div>
</div>

<!--a id="editFilter" class="action_link" href="javascript: EditFilter();">Изменить условия поиска</a -->

<a id="toggleFilterDescription" class="action_link" href="javascript: toggleFilterDescription();"><%= Resources.Resx.GetString("STORE_SearchEdit")%></a>

<!--Описание фильтра поиска-->
<div id="SearchFilterDescription" >
<cs:Div ID="SearchFilterBody" runat="server"></cs:Div>
</div>

<csp:PagingBar id="pageBar" runat="server" Help="PagingBar" OnCurrentPageChanged="OnResultCurrentPageChanged" OnRowsPerPageChanged="OnResultRowsPerPageChanged" ></csp:PagingBar>
<!--Таблица с результатами поиска-->
<div id="SearchResult" class="result_table"></div>
</div> <!-- page_content -->
</body>
</html>

