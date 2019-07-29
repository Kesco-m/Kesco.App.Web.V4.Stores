<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="Store.aspx.cs" Inherits="Kesco.App.Web.Stores.Store" %>
<%@ Import Namespace="Kesco.Lib.Localization" %>
<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="cs" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="Kesco.Stores.css"/>
    <script src="Kesco.Stores.js?v=1" type="text/javascript"></script>
    <base target="_self"/>
</head>

<body>
<form id="mvcDialogResult" method="post">
    <input type="hidden" name="escaped" value="0"/>
    <input type="hidden" name="control" value=""/>
    <input type="hidden" name="multiReturn" value=""/>
    <input type="hidden" name="value" value=""/>
</form>

<%= RenderDocumentHeader() %>

<!--Управление отчетами-->
<cs:Div ID="ReportPanel" runat="server" CSSClass="wrap_inline_block">
    <fieldset>
        <legend><%= Resources.Resx.GetString("STORE_StoreReportLegend") %></legend>
        <table style="border-spacing: 0 0;">
            <tr>
                <td width="140px"><%= Resources.Resx.GetString("STORE_StoreReportType") %>:</td>
                <td width="260px">
                    <dbs:DBSStoreReportType ID="sStoreReportType" runat="server" Width="250px" MaxItemsInPopup="16" IsMultiSelect="True" isRemove="True" OnBeforeSearch="OnReportTypeBeforeSearch"></dbs:DBSStoreReportType>
                </td>
            </tr>
        </table>
    </fieldset>
</cs:Div>

<table width="100%" style="border-spacing: 0 0;">
    <tr>
        <!--Период действия склада-->
        <td width="140px"><%= Resources.Resx.GetString("STORE_IsActual") %>:</td>
        <td>
            <div class="date_label"><%= Resources.Resx.GetString("lFrom") %></div>
            <div class="wrap_select">
                <cs:DatePicker ID="dateValidFrom" runat="server" NextControl="dateValidTo"></cs:DatePicker>
            </div>
            <div class="date_label"><%= Resources.Resx.GetString("lTo") %></div>
            <div class="wrap_select">
                <cs:DatePicker ID="dateValidTo" runat="server" NextControl="sStoreType"></cs:DatePicker>
            </div>
        </td>
    </tr>
    <!--Тип склада-->
    <tr>
        <td><%= Resources.Resx.GetString("STORE_StoreType") %>:</td>
        <td>
            <dbs:DBSStoreType ID="sStoreType" runat="server" Width="400px" MaxItemsInPopup="16" AutoSetSingleValue="True" IsRequired="True" OnChanged="OnStoreTypeChanged" OnBeforeSearch="OnStoreTypeBeforeSearch" NextControl="sStorage"></dbs:DBSStoreType>
        </td>
    </tr>
    <!--Место хранения-->
    <tr id="StorageRow">
        <td><%= Resources.Resx.GetString("STORE_Residence") %>:</td>
        <td>
            <dbs:DBSResidence ID="sStorage" runat="server" Width="400px" CLID="26" NextControl="txtName"></dbs:DBSResidence>
        </td>
    </tr>
    <!--Название склада или номер счета-->
    <tr>
        <td>
            <cs:Div ID="NameLabel" runat="server">:</cs:Div>
        </td>
        <td>
            <cs:StoreNameTextBox ID="txtName" runat="server" Width="400px" IsRequired="True" NextControl="txtIBAN" OnChanged="OnStoreNameChanged"></cs:StoreNameTextBox>
        </td>
    </tr>
    <!--IBAN номар счета-->
    <tr id="IbanRow">
        <td><%= Resources.Resx.GetString("STORE_IBAN") %>:</td>
        <td>
            <cs:TextBox ID="txtIBAN" runat="server" Width="400px" NextControl="chkAccountNumberUnknown" OnChanged="OnStoreIbanChanged"></cs:TextBox>
        </td>
    </tr>
    <!--Номер счета неизвестен-->
    <tr id="AccountNumberUnknownRow">
        <td><%= Resources.Resx.GetString("STORE_AccountUnknown") %>:</td>
        <td>
            <cs:CheckBox ID="chkAccountNumberUnknown" runat="server" Width="400px" OnChanged="OnStoreNumberUndefinedChanged" Title="Установить номер счёта неизвестен" NextControl="sKeeperBank"></cs:CheckBox>
        </td>
    </tr>
    <!--Хранитель-->
    <tr>
        <td>
            <cs:Div ID="KeeperBankLabel" runat="server">:</cs:Div>
        </td>
        <td>
            <dbs:DBSPerson ID="sKeeperBank" runat="server" Width="400px" IsCaller="True" CLID="15" AutoSetSingleValue="True" IsAlwaysAdvancedSearch="True" CallerType="Person" IsRequired="True" NextControl="sManager" OnChanged="OnKeeperChanged"></dbs:DBSPerson>
        </td>
    </tr>
    <!--Распорядитель-->
    <tr>
        <td><%= Resources.Resx.GetString("STORE_Manager") %>:</td>
        <td>
            <dbs:DBSPerson ID="sManager" runat="server" Width="400px" IsRequired="True" CLID="16" AutoSetSingleValue="True" IsAlwaysAdvancedSearch="True" IsCaller="True" CallerType="Person" NextControl="sManagerDepartment" OnChanged="OnManagerChanged"></dbs:DBSPerson>
        </td>
    </tr>
    <!--Подразделение распорядителя-->
    <tr id="ManagerDepartmentRow">
        <td><%= Resources.Resx.GetString("STORE_ManagerDepartment") %>:</td>
        <td>
            <dbs:DBSPersonDepartment ID="sManagerDepartment" runat="server" Width="400px" MaxItemsInPopup="16" NextControl="sResource" OnBeforeSearch="OnManagerDepartmentBeforeSearch"></dbs:DBSPersonDepartment>
        </td>
    </tr>
    <!--Хранимый ресурс-->
    <tr>
        <td>
            <cs:Div ID="ResourceLabel" runat="server">:</cs:Div>
        </td>
        <td>
            <dbs:DBSResource ID="sResource" runat="server" Width="400px" IsRequired="True" CLID="17" AutoSetSingleValue="True" NextControl="sAgreement" OnBeforeSearch="OnResourceBeforeSearch"></dbs:DBSResource>
        </td>
    </tr>
    <!--Договор храненния-->
    <tr id="StoreAgreementRow">
        <td><%= Resources.Resx.GetString("STORE_Agreement") %>:</td>
        <td>
            <dbs:DBSDocument ID="sAgreement" runat="server" NextControl="txtStoreDepartment" OnBeforeSearch="OnAgreementBeforeSearch" OnChanged="OnAgreementChanged" Width="400px"></dbs:DBSDocument>
        </td>
    </tr>
    <!--Отделение или филиал банка-->
    <tr id="StoreDepartmentRow">
        <td>
            <cs:Div ID="StoreDepartmentLabel" runat="server">:</cs:Div>
        </td>
        <td>
            <cs:TextArea ID="txtStoreDepartment" runat="server" CSSclass="v4_resizable" Height="60px" Width="400px"></cs:TextArea>
        </td>
    </tr>
    <!--Примечание-->
    <tr>
        <td><%= Resources.Resx.GetString("STORE_Description") %>:</td>
        <td>
            <cs:TextArea ID="txtDescription" runat="server" CSSclass="v4_resizable" Height="60px" Width="400px"></cs:TextArea>
        </td>
    </tr>
</table>

<div class="footer">
    <!--Изменил Изменено-->
    <cs:Changed ID="DivChangedByAt" runat="server"></cs:Changed>
</div>

</body>
</html>