using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;

using Kesco.Lib.Log;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Settings.Parameters;

using Kesco.Lib.DALC;
using Kesco.Lib.Web.Settings;
using Kesco.Lib.BaseExtention.Enums.Controls;

namespace Kesco.App.Web.Stores
{
    /// <summary>
    /// Класс объекта представляющего страницу отчета по складам
    /// </summary>
    public partial class StoreOrder : EntityPage
    {
        //Вспомогательный объект для сохранения и восстановления размеров и положения окна
        private WndSizePosKeeper _SizePosKeeper;

        //Таблица с списком складов в отчете
        private TemplatedSqlResult _table;

        //Сохраним исходный порядок, для сравнения его с новым и выделения изменений
        int[,] originalStoresId;

        public override string HelpUrl { get; set; }

        public StoreOrder()
        {
            HelpUrl = "hlp/help_order.htm";
        }

#region EventHandlers

        protected void Page_Load(object sender, EventArgs e)
        {
            if (V4IsPostBack) return;

            Title = Resx.GetString("STORE_ReportTitle");

            StoresClientScripts.InitializeGlobalVariables(this);

            _table = new TemplatedSqlResult(this, "StoreReport", null, "StoreOrder.xslt", Kesco.Lib.Entities.SQLQueries.SELECT_ОтчётыПоСкладам, System.Data.CommandType.Text);

            StoresPageHelper pageHelper = new StoresPageHelper(Request, ReturnId != "1" ? new AppParamsManager(this.ClId, StoresPageHelper.ReportParametersNamesCollection) : null);

            bool isRequired = false;
            string strReportId = pageHelper.getRequestParameterValue("id", out isRequired);
            if (isRequired)
            {
                pageHelper.setSelectCtrlValue(sStoreReportType, ((int)SelectEnum.Contain).ToString(), strReportId, isRequired);
            }
            else
                pageHelper.setSelectCtrlParameterValue(sStoreReportType, null, StoresPageHelper.ReportParameters.Type);

            //string strRowsPerPage = pageHelper.getParameterValue(StoresPageHelper.ReportParameters.ResultsPerPage, out isRequired, string.Empty);
            //pageBar.RowsPerPage = strRowsPerPage.ToInt();

            if (!string.IsNullOrEmpty(sStoreReportType.Value))
            {
                FilterApply();
            }
            else
            {
                //pageBar.SetDisabled(true, false);
            }

            StoresClientScripts.ClearSelectedStores(this);

            _SizePosKeeper = new WndSizePosKeeper(this, StoresPageHelper.WindowParameters.RptLeft, StoresPageHelper.WindowParameters.RptTop, StoresPageHelper.WindowParameters.RptWidth, StoresPageHelper.WindowParameters.RptHeight);
            _SizePosKeeper.OnLoad();
        }

        //Без этого обработчика при вызове OnReportTypeValueChanged старое значение уже отсутствует
        protected void OnReportTypeChanged(object sender, ProperyChangedEventArgs e)
        {
             //StoresClientScripts.SaveStoresOrder(this, e.OldValue.ToInt());
        }

        protected void OnReportTypeValueChanged(object sender, ValueChangedEventArgs e)
        {
            int old_report_type = e.OldValue.ToInt();
            if (0 != old_report_type)
                StoresClientScripts.ConfirmSaveStoresOrder(this, old_report_type);

            int new_report_type = e.NewValue.ToInt();
            if (0 != new_report_type)
            {
                //Для того чтобы избежать лишних обновлений, которые случаются при выборе нового типа отчетов по складам
                object old_value = null;
                if (_table.SqlParams.TryGetValue("@КодТипаОтчётаПоСкладам", out old_value) && new_report_type == ((string)old_value).ToInt())
                    return;

                StoresClientScripts.UpdateReportType(this);
                FilterApply();
            }
        }

        /*
        protected void OnReportCurrentPageChanged(object sender, EventArgs args)
        {
            StoresClientScripts.SaveStoresOrder(this, sStoreReportType.Value.ToInt());
            _table.Update();
            RestoreCursor();
        }

        protected void OnReportRowsPerPageChanged(object sender, EventArgs args)
        {
            if (!string.IsNullOrEmpty(StoreReport.Value))
            {
                StoresClientScripts.SaveStoresOrder(this, sStoreReportType.Value.ToInt());
                pageBar.CurrentPageNumber = 1;
                _table.Update();
            }

            RestoreCursor();
        }
        */

#endregion

        /// <summary>
        /// Метод формирует разметку панели инструментов страницы
        /// </summary>
        protected string RenderDocumentHeader()
        {
            using (var w = new StringWriter())
            {
                try
                {
                    ClearMenuButtons();
                    SetMenuButtons();
                    RenderButtons(w);
                }
                catch (Exception e)
                {
                    var dex = new DetailedException("Не удалось сформировать кнопки формы: " + e.Message, e);
                    Logger.WriteEx(dex);
                    throw dex;
                }

                return w.ToString();
            }
        }

        /// <summary>
        /// Метод формирует кнопки панели инструментов страницы
        /// </summary>
        private void SetMenuButtons()
        {
            Button btnOk = new Button
            {
                ID = "btnOK",
                V4Page = this,
                Text = Resx.GetString("cmdOK"),
                Title = Resx.GetString("cmdOK"),
                //Style = "BACKGROUND: buttonface url(/Styles/KescoOK.gif) no-repeat left center;",
                IconJQueryUI = "v4_buttonIcons.Ok",
                Width = 105,
                OnClick = "SaveStoresOrder(); CloseStoresOrder();"
            };

            Button btnSave = new Button
            {
                ID = "btnSave",
                V4Page = this,
                Text = Resx.GetString("cmdSave"),
                Title = Resx.GetString("titleSave"),
                //Style = "BACKGROUND: buttonface url(/Styles/Save.gif) no-repeat left center;",
                IconJQueryUI = "v4_buttonIcons.Save",
                Width = 105,
                OnClick = "SaveStoresOrder();"
            };

            Button btnCancel = new Button
            {
                ID = "btnCancel",
                V4Page = this,
                Text = Resx.GetString("cmdCancel"),
                Title = Resx.GetString("cmdCancel"),
                //Style = "BACKGROUND: buttonface url(/Styles/Cancel.gif) no-repeat left center;",
                IconJQueryUI = "v4_buttonIcons.Cancel",
                Width = 105,
                OnClick = "CancelStoresOrder();"
            };

            Button btnClose = new Button
            {
                ID = "btnClose",
                V4Page = this,
                Text = Resx.GetString("cmdClose"),
                Title = Resx.GetString("cmdClose"),
                //Style = "BACKGROUND: buttonface url(/Styles/ArrowRight.gif) no-repeat left center;",
                IconJQueryUI = "v4_buttonIcons.Close",
                Width = 105,
                OnClick = "CloseStoresOrder();"
            };

            Button[] buttons = new Button[] { btnOk, btnSave, btnCancel, btnClose };

            AddMenuButton(buttons);
        }

        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            _SizePosKeeper.ProcessCommand(cmd, param);

            switch (cmd)
            {
                /*
                case "CloseButton":
                    StoresClientScripts.SaveStoresOrder(this, sStoreReportType.Value.ToInt());
                    _SizePosKeeper.ProcessCommand("PageClose", null);
                    Close();
                    break;
                */

                case "ApplyStoresOrder":
                    {
                        int page = param["page"].ToInt();
                        int page_size = param["page_size"].ToInt();
                        string strReportType = param["report_type"];
                        //if (null == strReportType) strReportType = sStoreReportType.Value;
                        int reportType = strReportType.ToInt();
                        //если "null" или "undefined"
                        //if (0 == reportType)
                        {
                            //reportType = sStoreReportType.Value.ToInt();

                            if (0 == reportType)
                            {
                                object old_value = null;
                                if (_table.SqlParams.TryGetValue("@КодТипаОтчётаПоСкладам", out old_value))
                                    reportType = ((string)old_value).ToInt();
                            }
                        }

                        if (0 != reportType)
                            ApplyStoresOrder(reportType, param["stores"], param["at_bottom"] == "1", page, page_size);
                    }
                    break;

                case "RemoveStores":
                    RemoveStores(param["stores"]);
                    break;

                case "UpdateTable":
                    FilterUpdate();
                    RestoreCursor();
                    break;

                case "PageClose":
                    SavePageParameters();
                    goto default;

                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        private void RemoveStores(string strStores)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@КодыСкладов", (object)strStores);
            parameters.Add("@КодТипаОтчётаПоСкладам", string.IsNullOrEmpty(sStoreReportType.Value) ? DBNull.Value : (object)sStoreReportType.Value);

            try
            {
                DBManager.ExecuteNonQuery(Kesco.Lib.Entities.SQLQueries.DELETE_СкладыИзОтчётаПоСкладам, CommandType.Text, Config.DS_person, parameters);

                //Удаление отложенное, затем при необходимости последует команда Save, которая и должна перезагрузить таблицу
                //FilterUpdate();
                //RestoreCursor();

                //StoresClientScripts.ClearSelectedStores(this);
            }
            catch (DetailedException dex)
            {
                ShowMessage(string.Format(Resx.GetString("STORE_FailedRemoveStores"), dex.Message),Title);
            }
        }

        /// <summary>
        /// Метод осущесвляет переупорядочивание всех складов исходя из нового представления складов на текущей странице
        /// </summary>
        /// <param name="reportType">Тип отчета по складам</param>
        /// <param name="strStores">Список кодов складов на странице</param>
        /// <param name="fAtBottomOfPage">Флаг размещения складов на странице</param>
        private void ApplyStoresOrder(int reportType, string strStores, bool fAtBottomOfPage, int toPage, int pageSize)
        {
            switch(toPage)
            {
                case 0:
                    //toPage = pageBar.CurrentPageNumber;
                    toPage = 1;
                    break;
                case -1:
                    //toPage = pageBar.MaxPageNumber;
                    toPage = 1;
                    break;
            }

            if (toPage < 1)
            {
                //PagingBar меняет курсор
                RestoreCursor();
                return;
            }

            int storesCount = originalStoresId.Length / originalStoresId.Rank;

            if (pageSize < 1)
            {
                //pageSize = pageBar.RowsPerPage;
                pageSize = storesCount;
            }

            string[] new_stores = strStores.Split(new[] { ' ', ',' });
            List<int> new_stores_id = new List<int>();
            foreach(string s in new_stores)
            {
                int id = s.ToInt();
                if(id!=0) new_stores_id.Add(id);
            }
            //int[] new_stores_id = new_stores.Select(s => s.ToInt()).Where(i=>i!=0).ToArray<int>();

            int nStart = (toPage - 1) * pageSize;
            if (fAtBottomOfPage)
            {
                //Если строки переносились в конец страницы и первые склады не попали на текущую страницу
                //или количество складов в списке меньше размера страницы, то произойдет корректировка начала размещения
                //if (new_stores_id.Count() > pageSize)
                    nStart += (pageSize - new_stores_id.Count);
                    if (nStart < 0) nStart = 0;
            }

            //DataTable prevStoresResult = null;
            //DataTable dtStoresResult = null;
            try
            {
                //Dictionary<string, object> parameters = new Dictionary<string, object>();
                //parameters.Add("@КодТипаОтчётаПоСкладам", reportType);

                //string rowNumber = "-1";
                //string pageNum = "-1";
                //string itemsPerPage = "-1";
                //string pageCount = "-1";

                //dtStoresResult = DBManager.GetData(Kesco.Lib.Entities.SQLQueries.SELECT_СкладыВОтчете, Config.DS_person, CommandType.Text, parameters, null, string.Empty, string.Empty, null, null, null, ref pageNum, ref itemsPerPage, ref pageCount, out rowNumber);

                //if(dtStoresResult.Equals(prevStoresResult))
                {
                    //Cтроки на последнюю страницу целиком они не влезают
                    if (nStart + new_stores_id.Count > storesCount)
                        nStart = storesCount - new_stores_id.Count;

                    int[] new_stores_order = new int[storesCount];
                    int new_index = 0;

                    //Сохраним исходный порядок, для сравнения его с новым и выделения изменений
                    //int[,] orig_stores_id = new int[dtStoresResult.Rows.Count,2];

                    int i = 0;
                    //for (int i = 0; i < dtStoresResult.Rows.Count; i++)
                    //while (i < dtStoresResult.Rows.Count)

                    //new_index < storesCount дополнительная проверка для исключения переполнение в случае неуникальности значений в списке
                    while (i < storesCount && new_index < storesCount)
                    {
                        if (new_index < nStart)
                        {
                            //DataRow r = dtStoresResult.Rows[i];
                            //int store_id = (int)r["КодСклада"];
                            //int store_order = (int)r["Порядок"];

                            //orig_stores_id[i,0] = store_id;
                            //orig_stores_id[i++, 1] = store_order;

                            int store_id = originalStoresId[i++, 0];

                            if (!new_stores_id.Contains(store_id))
                            {
                                new_stores_order[new_index++] = store_id;
                            }
                        }

                        //Условие проверяется снова потому - что new_index++ и i++, следующей итерации уже может и не быть
                        if (new_index >= nStart)
                        {
                            //Копируем текущую страницу
                            foreach (int id in new_stores_id)
                            {
                                new_stores_order[new_index++] = id;
                            }

                            //больше условие new_index >= nStart не должно работать, далее элементы просто добавляются в список
                            nStart = storesCount + 1;
                        }
                    }

                    if (new_index > 0)
                    {
                        //Определим только те склады порядок, которых изменился
                        int[] diff = new int[new_index];

                        for (int j = 0; j < new_stores_order.Length; j++)
                        {
                            //Поле Порядок могло быть ранее заполнено не последовательно (например, могут быть пропуски после удаления),
                            //поэтому следует его проверять и изменять при необходимости
                            //if (orig_stores_id.Length <= j || orig_stores_id[j, 1] != j+1 || orig_stores_id[j, 0] != new_stores_order[j])
                            if (originalStoresId.Length <= j || originalStoresId[j, 1] != j + 1 || originalStoresId[j, 0] != new_stores_order[j])
                                diff[j] = new_stores_order[j];
                        }

                        //StringBuilder sb = new StringBuilder("UPDATE vwОтчётыПоСкладам SET Порядок=SRC.Порядок FROM (");
                        //StringBuilder sb = new StringBuilder("UPDATE vwОтчётыПоСкладам SET Порядок=SRC.Порядок FROM VALUES(");
                        StringBuilder sb = new StringBuilder();
                        int changed_count = 0;
                        for (int n = 0; n < diff.Length; n++)
                        {
                            if (diff[n] != 0)
                            {
                                //if (changed_count++ > 0) sb.Append(" UNION ALL ");
                                //sb.AppendFormat("SELECT {0}, {1}", diff[n], n + 1);
                                if (changed_count++ > 0) sb.Append(",");
                                sb.AppendFormat("({0}, {1})", diff[n], n + 1);
                            }
                        }

                        if (changed_count > 0)
                        {
                            //sb.Append(") AS SRC(Код, Порядок) WHERE КодСклада = SRC.Код AND КодТипаОтчётаПоСкладам = @КодТипаОтчётаПоСкладам");

                            Dictionary<string, object> parameters = new Dictionary<string, object>();
                            parameters.Add("@КодТипаОтчётаПоСкладам", reportType);

                            //DBManager.ExecuteNonQuery(sb.ToString(), CommandType.Text, Config.DS_person, parameters);

                            DBManager.ExecuteNonQuery(string.Format(Kesco.Lib.Entities.SQLQueries.UPDATE_СкладОтчётыПоСкладам, sb.ToString()), CommandType.Text, Config.DS_person, parameters);

                            //if(pageBar.CurrentPageNumber == toPage)
                            //    _table.Update();
                            FilterUpdate();
                        }
                    }
                    else
                    {
                        throw new DetailedException(Resx.GetString("STORE_NoStoresToMove"), null);
                    }
                }
                //else
                //{
                //    throw new DetailedException(Resx.GetString("STORE_StoresReportChanged"), null);
                //}

                //PagingBar меняет курсор
                RestoreCursor();
            }
            catch (DetailedException dex)
            {
                ShowMessage(string.Format(Resx.GetString("STORE_FailedMoveStores"), dex.Message), Title);
            }
        }

        /// <summary>
        /// Метод обновляет содержимое таблицы с результатами
        /// </summary>
        private void FilterUpdate()
        {
            DataTable dtStoresResult = _table.Update();
            if (null != dtStoresResult && dtStoresResult.Rows.Count > 0)
            {
                originalStoresId = new int[dtStoresResult.Rows.Count, 2];
                for (int i = 0; i < dtStoresResult.Rows.Count; i++)
                {
                    DataRow r = dtStoresResult.Rows[i];
                    int store_id = (int)r["КодСклада"];
                    int store_order = (int)r["Порядок"];
                    originalStoresId[i, 0] = store_id;
                    originalStoresId[i, 1] = store_order;
                }
            }
        }

        /// <summary>
        /// Метод осуществляет поиск в соответствии с установленными параметрами
        /// </summary>
        private void FilterApply()
        {
            originalStoresId = null;
            StoresClientScripts.SendSetInnerHtml(this, "StoreReport", string.Empty);

            int report_type = sStoreReportType.Value.ToInt();
            if (report_type == 0) return;

            //pageBar.SetDisabled(true, false);

            _table.SqlParams.Clear();
            _table.SqlParams.Add("@КодТипаОтчётаПоСкладам", string.IsNullOrEmpty(sStoreReportType.Value) ? DBNull.Value : (object)sStoreReportType.Value);

            //pageBar.CurrentPageNumber = 1;

            FilterUpdate();
        }

        /// <summary>
        /// Метод осуществляет сохранение последних значений полей страницы в БД настроек пользователей
        /// </summary>
        private void SavePageParameters()
        {
            if (ReturnId == "1") return;

            AppParamsManager parametersManager = new AppParamsManager(this.ClId, new StringCollection());

            parametersManager.Params.Add(new AppParameter(StoresPageHelper.ReportParameters.Type, sStoreReportType.Value, AppParamType.SavedWithClid));
            //parametersManager.Params.Add(new AppParameter(StoresPageHelper.ReportParameters.ResultsPerPage, pageBar.RowsPerPage.ToString(), AppParamType.SavedWithClid));
            parametersManager.SaveParams();
        }
    }
}