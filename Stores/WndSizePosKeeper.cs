using System.Collections.Specialized;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Settings.Parameters;

namespace Kesco.App.Web.Stores
{
    /// <summary>
    ///     Класс вспомогательного объекта для сохранения и восстановления размеров и положения окна
    /// </summary>
    public class WndSizePosKeeper
    {
        private readonly Page _p;
        private bool _page_closed;
        private readonly string _paramHeight;
        private readonly string _paramWidth;
        private readonly string _paramX;
        private readonly string _paramY;

        private WndSizePosKeeper()
        {
        }

        public WndSizePosKeeper(Page p, string param_x, string param_y, string param_width, string param_height)
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
            var WindowParameterNamesCollection = new StringCollection {_paramX, _paramY, _paramWidth, _paramHeight};

            var pageHelper2 =
                new StoresPageHelper(_p.Request, new AppParamsManager(_p.ClId, WindowParameterNamesCollection));
            var isRequired2 = false;
            var strX = pageHelper2.getParameterValue(_paramX, out isRequired2, "-1");
            var strY = pageHelper2.getParameterValue(_paramY, out isRequired2, "-1");
            var strWidth = pageHelper2.getParameterValue(_paramWidth, out isRequired2, "640");
            var strHeight = pageHelper2.getParameterValue(_paramHeight, out isRequired2, "480");

            if (strWidth.ToInt() > 0 && strHeight.ToInt() > 0)
                StoresClientScripts.SetWindowSizePos(_p, strX, strY, strWidth, strHeight);
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
        ///     Метод для сохранения размеров окна в БН настроек пользователей
        /// </summary>
        /// <param name="strWidth">Ширина окна</param>
        /// <param name="strHeight">Высота окна</param>
        private void StoreWindowSize(string strX, string strY, string strWidth, string strHeight)
        {
            var parametersManager = new AppParamsManager(_p.ClId, new StringCollection());

            parametersManager.Params.Add(new AppParameter(_paramX, strX, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(_paramY, strY, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(_paramWidth, strWidth, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(_paramHeight, strHeight, AppParamType.SavedWithClid));

            parametersManager.SaveParams();
        }
    }
}