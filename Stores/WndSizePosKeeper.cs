using System.Collections.Specialized;
using Kesco.Lib.Web.Settings.Parameters;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Docs;

namespace Kesco.App.Web.Stores
{
    /// <summary>
    /// Класс вспомогательного объекта для сохранения и восстановления размеров и положения окна
    /// </summary>
    public class WndSizePosKeeper
    {
        private WndSizePosKeeper() { }

        Kesco.Lib.Web.Controls.V4.Common.Page _p;
        private string _paramX;
        private string _paramY;
        private string _paramWidth;
        private string _paramHeight;
        private bool _page_closed;

        public WndSizePosKeeper(Kesco.Lib.Web.Controls.V4.Common.Page p, string param_x, string param_y, string param_width, string param_height)
        {
            _p = p;
            _paramX = param_x;
            _paramY = param_y;
            _paramWidth = param_width;
            _paramHeight = param_height;
        }

        public void OnLoad()
        {
            //Восстановление размеров окна
            StringCollection WindowParameterNamesCollection = new StringCollection() {_paramX, _paramY, _paramWidth, _paramHeight};

            StoresPageHelper pageHelper2 = new StoresPageHelper(_p.Request, new AppParamsManager(_p.ClId, WindowParameterNamesCollection));
            bool isRequired2 = false;
            string strX = pageHelper2.getParameterValue(_paramX, out isRequired2, "-1");
            string strY = pageHelper2.getParameterValue(_paramY, out isRequired2, "-1");
            string strWidth = pageHelper2.getParameterValue(_paramWidth, out isRequired2, "640");
            string strHeight = pageHelper2.getParameterValue(_paramHeight, out isRequired2, "480");

            if (strWidth.ToInt() > 0 && strHeight.ToInt() > 0)
            {
                StoresClientScripts.SetWindowSizePos(_p, strX, strY, strWidth, strHeight);
            }
            //размеры восстановлены
        }

        public void ProcessCommand(string cmd, NameValueCollection param)
        {
            switch (cmd)
            {
                case "SaveWindowSizePos":
                    StoreWindowSize(param["x"], param["y"], param["width"], param["height"]);
                    break;

                case "PageClose":
                    if (_page_closed) break;
                    StoresClientScripts.SendWindowSizePos(_p);
                    _page_closed = true;
                    break;
            }
        }

        /// <summary>
        /// Метод для сохранения размеров окна в БН настроек пользователей
        /// </summary>
        /// <param name="strWidth">Ширина окна</param>
        /// <param name="strHeight">Высота окна</param>
        private void StoreWindowSize(string strX, string strY, string strWidth, string strHeight)
        {
            AppParamsManager parametersManager = new AppParamsManager(_p.ClId, new StringCollection());

            parametersManager.Params.Add(new AppParameter(_paramX, strX, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(_paramY, strY, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(_paramWidth, strWidth, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(_paramHeight, strHeight, AppParamType.SavedWithClid));

            parametersManager.SaveParams();
        }
    }
}