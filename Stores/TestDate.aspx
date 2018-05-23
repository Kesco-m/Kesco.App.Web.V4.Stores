<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestDate.aspx.cs" Inherits="StoresV4.TestDate" %>
<%@ Register TagPrefix="cs" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
   <div style="margin-left: 100px; margin-top: 100px"><cs:DatePicker runat="server" ID="dateControl" IsUseCondition="true"></cs:DatePicker></div>
   
   <div style="margin-left: 100px; margin-top: 10px">
       <button id="btnDisabled"  onclick="cmd('cmd','Disabled');">Disabled</button>
   </div>

   <div style="margin-left: 100px; margin-top: 10px">
       <button id="btnEnabled"  onclick="cmd('cmd','Enabled');">Enabled</button>
   </div>
   <hr/>
   <div style="margin-left: 100px; margin-top: 10px">
       <button id="btnRequired"  onclick="cmd('cmd','Required');">Required</button>
   </div>

   <div style="margin-left: 100px; margin-top: 10px">
       <button id="btnNotRequired"  onclick="cmd('cmd','NotRequired');">NotRequired</button>
   </div>
</body>
</html>
