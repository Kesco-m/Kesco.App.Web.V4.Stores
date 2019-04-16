using System.Text;
using System.Web;
using Kesco.Lib.Web.Settings;

namespace Kesco.App.Web.Stores
{
    /// <summary>
    /// Вспомогательный класс для выполнения скриптов на стороне клиента
    /// </summary>
    public static class StoresClientScripts
    {
        //Метод для установки глобальных переменных для функций из Kesco.Stores.js
        public static void InitializeGlobalVariables(Kesco.Lib.Web.Controls.V4.Common.Page p)
        {
            string callbackUrl = (p.Request.QueryString["callbackUrl"]);
            string control = (p.Request.QueryString["control"]);
            string multiReturn = (p.Request.QueryString["multiReturn"]);
            if (string.IsNullOrWhiteSpace(callbackUrl)) callbackUrl = Config.store_form;
            string mvc = p.Request.QueryString["mvc"];
            if (string.IsNullOrWhiteSpace(mvc)) mvc = "0";

            p.JS.Write("control = '{7}'; multiReturn = '{8}'; callbackUrl='{0}'; mvc='{1}'; domain='{2}'; storesUrl='{3}?clid={5}'; storeReportUrl='{4}?clid={5}'; isReturn={6}; SetResizableInDialog();"
                , HttpUtility.JavaScriptStringEncode(callbackUrl)
                , HttpUtility.JavaScriptStringEncode(mvc)
                , HttpUtility.JavaScriptStringEncode(Config.domain)
                , HttpUtility.JavaScriptStringEncode(Config.store_form)
                , HttpUtility.JavaScriptStringEncode(Config.store_report)
                , p.ClId
                , p.ReturnId == "1" ? "true" : "false"
                , control
                , multiReturn
                );

            p.JS.Write("StrResources = {{{0}}}; ", p.Resx.GetString("STORE_JsResources"));
        }

        public static void ConfirmReload(Kesco.Lib.Web.Controls.V4.Common.Page p, string message, string title, string btnCommand)
        {
            p.JS.Write("CustomConfirmChangedTwoButtons.render('{0}', '{1}', '{2}', '{3}', '{4}');",
                title, HttpUtility.JavaScriptStringEncode(message.Replace("\r\n", "<br>")), p.Resx.GetString("QSBtnYes"), p.Resx.GetString("QSBtnNo"), btnCommand);
        }

        public static void SetErrDialogOkHandler(Kesco.Lib.Web.Controls.V4.Common.Page p, string param)
        {
            p.JS.Write("SetDialogOkHandler('{0}');", param);
        }

        public static void SetWindowSizePos(Kesco.Lib.Web.Controls.V4.Common.Page p, string strX, string strY, string strWidth, string strHeight)
        {
            p.JS.Write("SetWindowSizePos({0}, {1}, {2}, {3});", strX, strY, strWidth, strHeight);
        }

        public static void SendWindowSizePos(Kesco.Lib.Web.Controls.V4.Common.Page p)
        {
            p.JS.Write("SrvSendWindowSizePos();");
        }

        //Метод используется для установки содержимого HTML элемента
        //Класс V4 Div не используем, что бы избежать хранение большого объема данных в процессе сервера
        public static void SendSetInnerHtml(Kesco.Lib.Web.Controls.V4.Common.Page p, string strId, string htmlContent)
        {
            StringBuilder sb = new StringBuilder(htmlContent);
            sb.Replace(@"\", @"\\");
            sb.Replace(@"'", @"\'");
            sb.Replace("\"", "\\\"");
            sb.Replace("\r", "\\\r");

            p.JS.Write("(function(){{ var el = document.getElementById('{0}'); if(el) el.innerHTML='{1}'; }})();", strId, sb.ToString());
        }

        //Медоды для Store.aspx
        public static void DisplayStoresReport(Kesco.Lib.Web.Controls.V4.Common.Page p, string strReportType)
        {
            p.JS.Write("SrvDisplayStoresReport({0});", strReportType);
        }

        public static void NotifyParentWindow(Kesco.Lib.Web.Controls.V4.Common.Page p, int storeId, string strReturnStoreName)
        {
            p.JS.Write("NotifyParentWindow('{0}','{1}');", storeId, HttpUtility.JavaScriptStringEncode(strReturnStoreName));
        }

        public static void ReturnValue(Kesco.Lib.Web.Controls.V4.Common.Page p, int storeId, string strReturnStoreName)
        {
            p.JS.Write("ReturnValue('{0}','{1}');", storeId, HttpUtility.JavaScriptStringEncode(strReturnStoreName));
        }

        public static void ExportTo1S(Kesco.Lib.Web.Controls.V4.Common.Page p, int storeId, int storeType)
        {
            p.JS.Write(
               storeType > 20
                   ? "v4_windowOpen('{0}1sdirectory.ashx?id={1}&DirOU=2&Dir1S=14,66','s_{1}','resizable=yes,scrollbars=yes,height=500,width=500,left=250,top=150,status=yes,toolbar=no,menubar=no,location=no');"
                   : "v4_windowOpen('{0}1sdirectory.ashx?id={1}&DirOU=2&Dir1S=3','s_{1}','resizable=yes,scrollbars=yes,height=500,width=500,left=250,top=150,status=yes,toolbar=no,menubar=no,location=no');",
               Config.store_export1s, storeId);
        }
        ////////////////////////////////////////////////////////

        //Медоды для Search.aspx
        public static void RestoreSrchFormSettings(Kesco.Lib.Web.Controls.V4.Common.Page p, string requiredPredicates, string hiddenFields)
        {
            p.JS.Write("RestoreSrchFormSettings({0}, {1});", requiredPredicates, hiddenFields);
        }

        public static void DisplaySrchFormField(Kesco.Lib.Web.Controls.V4.Common.Page p, string field, bool fShow)
        {
            if (fShow)
                p.JS.Write("AddSrchFormPredicate('{0}');", field);
            else
                p.JS.Write("RemoveSrchFormPredicate('{0}');", field);
        }

        public static void DisplayFilterDescription(Kesco.Lib.Web.Controls.V4.Common.Page p, bool fShow)
        {
            if (fShow)
                p.JS.Write("FilterDescriptionShow();");
            else
                p.JS.Write("FilterDescriptionHide();");
        }

        public static void CreateNewStore(Kesco.Lib.Web.Controls.V4.Common.Page p, string parameters)
        {
            p.JS.Write("CreateNewStore('{0}');", parameters);
        }
        ////////////////////////////////////////////////////////

        //Медоды для StoreOrder.aspx
        public static void ConfirmSaveStoresOrder(Kesco.Lib.Web.Controls.V4.Common.Page p, int report_type)
        {
            p.JS.Write("ConfirmSaveStoresOrder({0});", report_type);
        }

        public static void UpdateReportType(Kesco.Lib.Web.Controls.V4.Common.Page p)
        {
            p.JS.Write("UpdateReportType();");
        }

        public static void ClearSelectedStores(Kesco.Lib.Web.Controls.V4.Common.Page p)
        {
            p.JS.Write("ClearSelectedStores();");
        }
        ////////////////////////////////////////////////////////
    }
}