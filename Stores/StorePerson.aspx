<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StorePerson.aspx.cs" Inherits="Kesco.App.Web.Stores.StorePerson" %>
<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="v4" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title><% RenderCompanyName(Response.Output, true); %></title>
    <style type="text/css">
        .marginL { margin-left: 5px !important; }

        .marginR { margin-right: 5px !important; }

        .marginB { margin-bottom: 5px; }

        .marginT { margin-top: 5px; }

        .paddingB { padding-bottom: 10px; }

        .paddingT { padding-top: 10px; }

        .alignText { text-align: left !important; }

        .heightEmpty { height: 5px; }

        .heightRow { height: 20px; }

        .heightRowRS { height: 60px; }

        .heightRowRS1 { height: 25px; }
    </style>
</head>
<body>
<div><%= RenderDocumentHeader() %></div>
<h3 class="marginL">
    <span id="spanCompanyName"><% RenderCompanyName(Response.Output); %></span>
</h3>
<fieldset class="marginL marginB paddingB">
    <legend><%= Resx.GetString("STORE_SP_Lbl_Lang") %>:</legend>
    <v4:Radio runat="server" id="radioLang" Name="radioLangName" PaddingLeftRadio="5" MarginLeftLabel="5" HeightRow="20"></v4:Radio>
</fieldset>

<fieldset class="marginL marginB paddingB paddingT">
    <legend><%= Resx.GetString("STORE_SP_Lbl_Data") %>:</legend>
    <div class="marginL v4DivTable">
        <div class="v4DivTableRow heightRow">
            <div class="v4DivTableCell">
                <v4:CheckBox runat="server" id="flagName"></v4:CheckBox>
            </div>
            <div class="v4DivTableCell alignText">
                <label for="flagName_0" class="marginL"><%= Resx.GetString("STORE_SP_Lbl_Data_Name") %></label>
            </div>
            <div class="v4DivTableCell alignText" id="textName"></div>
        </div>
        <div class="v4DivTableRow heightRow" id="divINN" style="display: none;">
            <div class="v4DivTableCell">
                <v4:CheckBox runat="server" id="flagINN"></v4:CheckBox>
            </div>
            <div class="v4DivTableCell alignText">
                <label for="flagINN_0" class="marginL"><%= Resx.GetString("STORE_SP_Lbl_Data_Inn") %></label>
            </div>
            <div class="v4DivTableCell alignText" id="textINN"></div>
        </div>
        <div class="v4DivTableRow heightRow" id="divOGRN" style="display: none;">
            <div class="v4DivTableCell">
                <v4:CheckBox runat="server" id="flagOGRN"></v4:CheckBox>
            </div>
            <div class="v4DivTableCell alignText">
                <label for="flagOGRN_0" class="marginL"><%= Resx.GetString("STORE_SP_Lbl_Data_OGRN") %></label>
            </div>
            <div class="v4DivTableCell alignText" id="textOGRN"></div>
        </div>
        <div class="v4DivTableRow heightRow" id="divKPP" style="display: none;">
            <div class="v4DivTableCell">
                <v4:CheckBox runat="server" id="flagKPP"></v4:CheckBox>
            </div>
            <div class="v4DivTableCell alignText">
                <label for="flagKPP_0" class="marginL"><%= Resx.GetString("STORE_SP_Lbl_Data_Kpp") %></label>
            </div>
            <div class="v4DivTableCell alignText" id="textKPP"></div>
        </div>
        <div class="v4DivTableRow heightRow" id="divAddress" style="display: none;">
            <div class="v4DivTableCell">
                <v4:CheckBox runat="server" id="flagAddress"></v4:CheckBox>
            </div>
            <div class="v4DivTableCell alignText">
                <label for="flagAddress_0" class="marginL"><%= Resx.GetString("STORE_SP_Lbl_Data_Address") %></label>
            </div>
            <div class="v4DivTableCell alignText" id="textAddress"></div>
        </div>
        <div class="v4DivTableRow heightRow" id="divRS" style="display: none;">
            <div class="v4DivTableCell">
                <v4:CheckBox runat="server" id="flagRS"></v4:CheckBox>
            </div>
            <div class="v4DivTableCell alignText">
                <label for="flagRS_0" class="marginL" style="white-space: nowrap"><%= Resx.GetString("STORE_SP_Lbl_Data_RS") %>&nbsp;</label>
            </div>
            <div class="v4DivTableCell alignText">
                <dbs:DBSStore runat="server" id="dbsStoreRS" Width="300" IsAlwaysCreateEntity="False" MaxItemsInPopup="50" MaxItemsInQuery="51"></dbs:DBSStore>
                <div id="textRS"></div>
            </div>
        </div>
        <div class="v4DivTableRow heightRowRS1" id="divSpec" style="display: none;">
            <div class="v4DivTableCell">
                <v4:CheckBox runat="server" id="flagSpec"></v4:CheckBox>
            </div>
            <div class="v4DivTableCell alignText">
                <label for="flagSpec_0" class="marginL" style="white-space: nowrap"><%= Resx.GetString("STORE_SP_Lbl_Data_Spec") %>&nbsp;</label>
            </div>
            <div class="v4DivTableCell alignText">
                <dbs:DBSStore runat="server" id="dbsStoreSpec" Width="300" IsAlwaysCreateEntity="False" MaxItemsInPopup="50" MaxItemsInQuery="51"></dbs:DBSStore>
                <div id="textSpec"></div>
            </div>
        </div>
        <div class="v4DivTableRow heightRowRS" id="divValS" style="display: none;">
            <div class="v4DivTableCell">
                <v4:CheckBox runat="server" id="flagValS"></v4:CheckBox>
            </div>
            <div class="v4DivTableCell alignText">
                <label for="flagValS_0" class="marginL" style="white-space: nowrap"><%= Resx.GetString("STORE_SP_Lbl_Data_ValS") %></label>
            </div>
            <div class="v4DivTableCell alignText">
                <div class="marginL v4DivTable">
                    <div class="v4DivTableRow heightRowRS1">
                        <div class="v4DivTableCell alignText"><%= Resx.GetString("STORE_SP_Lbl_Data_Currency") %>:&nbsp;</div>
                        <div class="v4DivTableCell alignText">
                            <dbs:DBSCurrency runat="server" id="dbsCurrency" Width="100" IsAlwaysCreateEntity="False"></dbs:DBSCurrency>
                        </div>
                    </div>
                    <div class="v4DivTableRow heightRowRS1">
                        <div class="v4DivTableCell alignText"><%= Resx.GetString("STORE_SP_Lbl_Data_Account") %>:</div>
                        <div class="v4DivTableCell alignText">
                            <dbs:DBSStore runat="server" id="dbsStoreValS" Width="246" IsAlwaysCreateEntity="False" MaxItemsInPopup="50" MaxItemsInQuery="51"></dbs:DBSStore>
                            <div id="textValS"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="v4DivTableRow heightRow" id="divDirector" style="display: none;">
            <div class="v4DivTableCell">
                <v4:CheckBox runat="server" id="flagDirector"></v4:CheckBox>
            </div>
            <div class="v4DivTableCell alignText">
                <label for="flagDirector_0" class="marginL marginR"><%= Resx.GetString("STORE_SP_Lbl_Data_Director") %></label>
            </div>
            <div class="v4DivTableCell alignText" id="textDirector">
                <dbs:DBSPerson runat="server" id="dbsSign1" Width="300px"></dbs:DBSPerson>
                <div id="divSign1Post"></div>
            </div>
        </div>
    </div>
</fieldset>

</body>
<script type="text/javascript">


    function stores_printStoresPerson() {

        cmdasync("cmd", "PrintData");
    }

    $(document).ready(function() {
        SetContolFocus("radioLangen_0");
    });

    function SetContolFocus(ctrlid) {
        setTimeout(function() {
                $("#" + ctrlid).focus();
            },
            100);
    }

</script>
</html>