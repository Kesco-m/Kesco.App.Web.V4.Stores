using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Kesco.Lib.Web.Controls.V4.Common;

namespace StoresV4
{
    public partial class TestDate : EntityPage
    {
        protected override string HelpUrl { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (V4IsPostBack) return;

            dateControl.IsDisabled = true;

        }

        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            switch (cmd)
            {
                case "Disabled":
                    Disabled();
                    break;

                case "Enabled":
                    Enabled();
                    break;

                case "Required":
                    Required();
                    break;

                case "NotRequired":
                    NotRequired();
                    break;
                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        private void Disabled()
        {
            dateControl.IsDisabled = true;
        }

        private void Enabled()
        {
            dateControl.IsDisabled = false;
        }

        private void Required()
        {
            dateControl.IsRequired = true;
        }

        private void NotRequired()
        {
            dateControl.IsRequired = false;
        }
    }
}