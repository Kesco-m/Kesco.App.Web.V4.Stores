using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Kesco.Lib.Web.Controls.V4.Common;
using System.Collections.Specialized;

namespace Kesco.App.Web.Stores
{
    public partial class Test : EntityPage
    {
        /// <summary>
        /// Ссылка на справку текущей страницы
        /// </summary>
        private string _helpUrl = "hlp/help.htm?page=hlpStore";
        /// <summary>
        /// Ссылка на страницу со справкой
        /// </summary>
        protected override string HelpUrl
        {
            get
            {
                return _helpUrl;
            }
            set
            {
                HelpUrl = _helpUrl;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            switch (cmd)
            {
                case "CancelButton":
                    Close();
                    break;

                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

    }
}