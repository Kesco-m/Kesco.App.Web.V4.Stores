<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="Kesco.App.Web.Stores.Test" %>
<%@ Register TagPrefix="dbs" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="cs" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>

<!DOCTYPE html>
<script>

</script>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .div_width
        {
            width: 100%;
        }
        
        #selectStore table.v4s td 
        {
             width: 100%;
        }

    </style>
</head>
<body >
    <form id="form1">
    <div>
    <dbs:DBSStore ID="selectStore" HtmlID="selectStore" runat="server" MaxItemsInPopup="16" CSSClass="div_width"></dbs:DBSStore>
    </div>
    <div>

    <cs:Button ID="btnClose" runat="server" Text="Закрыть" Width="145px" Height="20px" OnClick= "cmd('cmd', 'CancelButton');" Style = "BACKGROUND: buttonface url(/Styles/Cancel.gif) no-repeat left center;" ></cs:Button>
    </div>

    <input id ="searchStores" value="Поиск складов" title="Поиск складов" onclick='window.showModalDialog("test.html", "", "dialogHeight:480px; dialogWidth:640px; resizable:yes; scroll: yes;");' type="button" />
    </form>
</body>
</html>
