using System;
using System.Data;
using System.IO;
using System.Web;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Kesco.Lib.DALC;
using Kesco.Lib.Log;

using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.BaseExtention.Enums.Controls;

using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Settings;
using Kesco.Lib.Web.Settings.Parameters;
using Kesco.Lib.Web.DBSelect.V4;
using Kesco.Lib.Entities.Stores;
using Kesco.Lib.Entities.Documents;
using System.Text.RegularExpressions;


namespace Kesco.App.Web.Stores
{
    /// <summary>
    /// Класс объекта представляющего страницу редактирования склада
    /// </summary>
    public partial class Store : EntityPage
    {
        //Вспомогательный объект для сохранения и восстановления размеров и положения окна
        private WndSizePosKeeper _SizePosKeeper;

        //Код склада, свойства которого отображаются на странице, склад будет модифицирован при сохранении 
        private int _storeId = 0;

        //Предварительно загруженный список допустимых типов складов, для быстрого извлечения допустимых ресурсов склада
        private List<StoreType> _storeTypesList=null;

        //Идентификаторы отчетов по складам куда уже добавлен склад
        private int[] _reportTypes;

        //Оригинальный запрос, для того что бы иметь возможность перезагруки страницы
        private Uri _origUrl;

        //Исходные данные склада
        private DataRow _source;

        //Исходный список отчетов по складам
        private string[] _reports;

        //Параметр mvc в запроса
        public string mvc;

        protected override string HelpUrl {get;set;}

        public Store()
        {
            HelpUrl = "hlp/help_store.htm";
        }

#region EventHandlers

        protected void Page_Load(object sender, EventArgs e)
        {
            if (V4IsPostBack) return;

            _source = null;
            _reports = null;
            _origUrl = Request.Url;

            mvc = Request.QueryString["mvc"];
            if (string.IsNullOrWhiteSpace(mvc)) mvc = "0";

            StoresClientScripts.InitializeGlobalVariables(this);

            _storeTypesList = StoreType.GetList();

            //btnAddToReport.Text = Resx.GetString("cmdAddToReport");
            //btnDisplayReport.Text = Resx.GetString("cmdDisplayReport");

            NameLabel.Value = Resx.GetString("STORE_Name") + '/' + Resx.GetString("STORE_Account");
            KeeperBankLabel.Value = Resx.GetString("STORE_Keeper") + '/' + Resx.GetString("STORE_Bank");
            ResourceLabel.Value = Resx.GetString("STORE_Resource") + '/' + Resx.GetString("STORE_Currency");
            StoreDepartmentLabel.Value = Resx.GetString("STORE_Department") + '/' + Resx.GetString("STORE_Branch");

            string strStoreId = Request.QueryString["id"];

            if (null != strStoreId && (_storeId = strStoreId.ToInt()) != 0)
            {
                Title = Resx.GetString("STORE_EditStoreTitle");

                Dictionary<string, object> sqlParams = new Dictionary<string, object>();
                sqlParams.Add("@КодСклада", (object)_storeId);

                DataTable dt = DBManager.GetData(Kesco.Lib.Entities.SQLQueries.SELECT_СкладПараметры, Config.DS_person, CommandType.Text, sqlParams);

                if (null != dt && dt.Rows.Count > 0 && null != dt.Rows[0])
                {
                    DataRow r = dt.Rows[0];
                    _source = r;
                    object dbObj = r["КодСклада"];
                    if (DBNull.Value != dbObj && _storeId == (int)dbObj)
                    {
                        dbObj = r["Склад"];
                        if (DBNull.Value != dbObj) txtName.Value = (string)dbObj;

                        dbObj = r["IBAN"];
                        if (DBNull.Value != dbObj) txtIBAN.Value = (string)dbObj;

                        dbObj = r["От"];
                        if (DBNull.Value != dbObj)
                            dateValidFrom.ValueDate = (DateTime)dbObj;

                        dbObj = r["По"];
                        if (DBNull.Value != dbObj)
                            dateValidTo.ValueDate = (DateTime)dbObj;

                        dbObj = r["КодТипаСклада"];
                        if (DBNull.Value != dbObj) sStoreType.ValueInt = (int)dbObj;

                        dbObj = r["КодМестаХранения"];
                        if (DBNull.Value != dbObj) sStorage.ValueInt = (int)dbObj;

                        dbObj = r["КодРесурса"];
                        if (DBNull.Value != dbObj)
                        {
                            sResource.ValueInt = (int)dbObj;

                            //sStoreReportType.GetFilter().ResourceId.Value = sResource.Value;
                        }

                        dbObj = r["КодХранителя"];
                        if (DBNull.Value != dbObj)
                        {
                            sKeeperBank.ValueInt = (int)dbObj;

                            //sStoreReportType.GetFilter().PcId.Add(sKeeperBank.Value);
                        }

                        dbObj = r["КодРаспорядителя"];
                        if (DBNull.Value != dbObj)
                        {
                            sManager.ValueInt = (int)dbObj;

                            //sStoreReportType.GetFilter().PcId.Add(sManager.Value);
                        }

                        dbObj = r["КодПодразделенияРаспорядителя"];
                        if (DBNull.Value != dbObj) sManagerDepartment.ValueInt = (int)dbObj;

                        dbObj = r["КодДоговора"];
                        if (DBNull.Value != dbObj) sAgreement.ValueInt = (int)dbObj;

                        dbObj = r["Филиал"];
                        if (DBNull.Value != dbObj) txtStoreDepartment.Value = (string)dbObj;

                        dbObj = r["Примечание"];
                        if (DBNull.Value != dbObj) txtDescription.Value = (string)dbObj;

                        /*
                        string strDivChangedByAt = string.Empty;
                        dbObj = r["Изменил"];
                        if (DBNull.Value != dbObj) strDivChangedByAt = Resx.GetString("lblChangedBy") + ": " + (string)dbObj;//(new Employee(employee_id.ToString())).FullName;

                        dbObj = r["Изменено"];
                        if (DBNull.Value != dbObj)
                        {
                            if (strDivChangedByAt.Length>0)
                                strDivChangedByAt += " " + ((DateTime)dbObj).ToString();
                            else
                                strDivChangedByAt = Resx.GetString("lblChanged") + ": " + ((DateTime)dbObj).ToString();
                            //DivChangedAt.Value = Resx.GetString("lblChanged") + ": " + ((DateTime)dbObj).ToString();
                        }

                        DivChangedByAt.Value = strDivChangedByAt;
                        */

                        DivChangedByAt.SetChangeDateTime = (DateTime?)r["Изменено"];
                        DivChangedByAt.ChangedByID = (int?)r["Изменил"];
                    }
                }
                else
                {
                    _storeId = 0;
                }
            }
            else
            {
                Title = Resx.GetString("STORE_CreateStoreTitle");

                //Из БД параметры не загружаются
                StoresPageHelper pageHelper = new StoresPageHelper(Request, null /*new AppParamsManager(this.ClId, StoresPageHelper.StoreParameterNamesCollection)*/);

                bool isRequired = false;
                dateValidFrom.Value = pageHelper.getParameterValue(StoresPageHelper.StoreParameters.ValidFrom, out isRequired, string.Empty/*DateTime.Now.ToString("dd.MM.yyyy")*/);
                dateValidFrom.IsDisabled = isRequired;

                dateValidTo.Value = pageHelper.getParameterValue(StoresPageHelper.StoreParameters.ValidTo, out isRequired, dateValidFrom.Value);
                dateValidTo.IsDisabled = isRequired;

                pageHelper.setSelectCtrlParameterValue(sStoreType, null, StoresPageHelper.StoreParameters.Type);

                pageHelper.setTextBoxParameterValue(txtName, null, StoresPageHelper.StoreParameters.Name);
                pageHelper.setTextBoxParameterValue(txtIBAN, null, StoresPageHelper.StoreParameters.Iban);

                //Имя склада обязательно
                if (txtName.IsReadOnly)
                {
                    chkAccountNumberUnknown.IsReadOnly = true;
                }

                string checkBoxValue = pageHelper.getParameterValue(StoresPageHelper.StoreParameters.AccountNumberUnknown, out isRequired, "0");
                chkAccountNumberUnknown.Checked = checkBoxValue == "1";
                chkAccountNumberUnknown.IsDisabled = isRequired;

                pageHelper.setSelectCtrlParameterValue(sKeeperBank, null, StoresPageHelper.StoreParameters.Keeper);
                pageHelper.setSelectCtrlParameterValue(sManager, null, StoresPageHelper.StoreParameters.Manager);
                pageHelper.setSelectCtrlParameterValue(sManagerDepartment, null, StoresPageHelper.StoreParameters.ManagerDepartment);
                pageHelper.setSelectCtrlParameterValue(sResource, null, StoresPageHelper.StoreParameters.Resource);

                txtStoreDepartment.Value = pageHelper.getParameterValue(StoresPageHelper.StoreParameters.Department, out isRequired, String.Empty);
                txtStoreDepartment.IsDisabled = isRequired;

                txtDescription.Value = pageHelper.getParameterValue(StoresPageHelper.StoreParameters.Description, out isRequired, String.Empty);
                txtDescription.IsDisabled = isRequired;

                if (!TryOldParameters(pageHelper))
                {
                    pageHelper.setSelectCtrlParameterValue(sStorage, null, StoresPageHelper.StoreParameters.Storage);
                    pageHelper.setSelectCtrlParameterValue(sAgreement, null, StoresPageHelper.StoreParameters.Agreement);
                }
            }

            if (txtName.Value == StoreNameTextBox.NoNameValue)
            {
                chkAccountNumberUnknown.Checked = true;
                SetStoreNumberUndefined(true);
            }

            int storeTypeID = 0;
            if (int.TryParse(sStoreType.Value, out storeTypeID))
                SetStoreType(storeTypeID);

            sStoreReportType.URLShowEntity = Config.store_report;
            if (_storeId != 0)
                sStoreReportType.URLShowEntity += string.Format("?selected={0}", _storeId);

            //DisplayReport();
            DisplayStoreReportTypes();

            _SizePosKeeper = new WndSizePosKeeper(this, StoresPageHelper.WindowParameters.Left, StoresPageHelper.WindowParameters.Top, StoresPageHelper.WindowParameters.Width, StoresPageHelper.WindowParameters.Height);
            _SizePosKeeper.OnLoad();

            //Не показываем виртуальные склады
            sStoreType.Filter.IDs.IsException = true;
            sStoreType.Filter.IDs.Set("-1");

            //Оба лица должны присутствовать в договорах организации склада
            sAgreement.Filter.PersonIDs.UseAndOperator = true;

            dateValidFrom.OnRenderNtf += new RenderNtfDelegate(dateValidFrom_OnRenderNtf);
            dateValidTo.OnRenderNtf += new RenderNtfDelegate(dateValidTo_OnRenderNtf);
        }

        void dateValidTo_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            DateTime to = dateValidTo.ValueDate.GetValueOrDefault(DateTime.MaxValue);
            DateTime from = dateValidFrom.ValueDate.GetValueOrDefault(DateTime.MinValue);

            if (to < DateTime.Now)
                ntf.Add(Resx.GetString("STORE_ToInPast"), NtfStatus.Information);

            if (to < from)
                ntf.Add(Resx.GetString("STORE_DatesAreNotValid"), NtfStatus.Error);
        }

        void dateValidFrom_OnRenderNtf(object sender, Ntf ntf)
        {
            DateTime from = dateValidFrom.ValueDate.GetValueOrDefault(DateTime.MinValue);

            if (from > DateTime.Now)
                ntf.Add(Resx.GetString("STORE_FromInFuture"), NtfStatus.Information);

            dateValidTo.RenderNtf();
        }

        //public override bool OnBeforeClose()
        //{
            //Для работы _SizePosKeeper
            //return false;
        //}

        protected void OnStoreNumberUndefinedChanged(object sender, ProperyChangedEventArgs e)
        {
            SetStoreNumberUndefined(e.NewValue != "0");

            //Исправление непонятного поведения при нажатии кнопки сразу после загрузки
            V4SetFocus("chkAccountNumberUnknown");
        }

        protected void OnResourceBeforeSearch(object sender)
        {
            if (string.IsNullOrWhiteSpace(sStoreType.Value))
                sResource.Filter.AllChildrenWithParentIDs.Clear();
            else
            {
                StoreType st = _storeTypesList.Find(t => { return t.Id == sStoreType.Value; });
                if (null == st)
                    sResource.Filter.AllChildrenWithParentIDs.Clear();
                else
                    sResource.Filter.AllChildrenWithParentIDs.Value = st.RootSource;
            }
        }

        protected void OnManagerDepartmentBeforeSearch(object sender)
        {
            sManagerDepartment.GetFilter().PcId.Set(sManager.Value);
            sManagerDepartment.GetFilter().PcId.CompanyHowSearch = "0";
        }

        protected void OnAgreementBeforeSearch(object sender)
        {
            sAgreement.Filter.Type.Clear();

            int storeTypeID = sStoreType.Value.ToInt();
            sAgreement.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = Kesco.Lib.BaseExtention.Enums.Docs.DocTypeEnum.ДоговорХранения, QueryType = DocTypeQueryType.WithChildrenSynonyms });
            sAgreement.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = Kesco.Lib.BaseExtention.Enums.Docs.DocTypeEnum.ДоговорТранспортировки, QueryType = DocTypeQueryType.WithChildrenSynonyms });
            sAgreement.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = Kesco.Lib.BaseExtention.Enums.Docs.DocTypeEnum.ДоговорПодряда, QueryType = DocTypeQueryType.WithChildrenSynonyms });

            if (storeTypeID < 11)
                sAgreement.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = Kesco.Lib.BaseExtention.Enums.Docs.DocTypeEnum.ДоговорОказанияУслуг, QueryType = DocTypeQueryType.WithChildrenSynonyms });

            int managerId = sManager.Value.ToInt();
            int keeperId = sKeeperBank.Value.ToInt();

            sAgreement.Filter.PersonIDs.Clear();

            if (managerId != 0)
                sAgreement.Filter.PersonIDs.Add(managerId.ToString());

            if (keeperId != 0)
                sAgreement.Filter.PersonIDs.Add(keeperId.ToString());
        }

        protected void OnReportTypeBeforeSearch(object sender)
        {
            if (null != sResource.ValueInt)
                sStoreReportType.GetFilter().ResourceId.Value = sResource.Value;
            else
                sStoreReportType.GetFilter().ResourceId.Clear();

            sStoreReportType.GetFilter().PcId.Clear();

            if (null != sKeeperBank.ValueInt)
                sStoreReportType.GetFilter().PcId.Add(sKeeperBank.Value);

            if (null != sManager.ValueInt)
                sStoreReportType.GetFilter().PcId.Add(sManager.Value);
        }

        /*
        protected void OnReportTypeChanged(object sender, ProperyChangedEventArgs e)
        {
            btnAddToReport.Visible = !IsStoreInReport();
            btnDisplayReport.Visible = !btnAddToReport.Visible;
        }
        */

        protected void OnStoreTypeChanged(object sender, ProperyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            if (e.NewValue.Equals(e.OldValue)) return;

            int storeTypeID = 0;
            if (int.TryParse(e.NewValue, out storeTypeID))
                SetStoreType(storeTypeID);
        }

        protected void OnManagerChanged(object sender, ProperyChangedEventArgs e)
        {
            sManagerDepartment.ClearSelectedItems();
            sManagerDepartment.Value = null;

            //sAgreement.ClearSelectedItems();
            //sAgreement.Value = null;

            GetKeeperManagerFromsAgreement();

            int KeeperCode = sKeeperBank.Value.ToInt();
            int ManagerCode = sManager.Value.ToInt();

            sAgreement.Visible = KeeperCode != ManagerCode || KeeperCode == 0 || ManagerCode==0;
            if (sAgreement.Visible) Display("StoreAgreementRow");
            else Hide("StoreAgreementRow");
        }

        protected void OnKeeperChanged(object sender, ProperyChangedEventArgs e)
        {
            //sAgreement.ClearSelectedItems();
            //sAgreement.Value = null;

            GetKeeperManagerFromsAgreement();

            int KeeperCode = sKeeperBank.Value.ToInt();
            int ManagerCode = sManager.Value.ToInt();

            sAgreement.Visible = KeeperCode != ManagerCode || KeeperCode == 0 || ManagerCode == 0;
            if (sAgreement.Visible) Display("StoreAgreementRow");
            else Hide("StoreAgreementRow");
        }

        protected void OnAgreementChanged(object sender, ProperyChangedEventArgs e)
        {
            GetKeeperManagerFromsAgreement();
        }

#endregion

        /// <summary>
        /// Устанавливает распорядителя и хранителя склада в соответсвие с установленным договором хранения
        /// </summary>
        void GetKeeperManagerFromsAgreement()
        {
            if (sAgreement.Value.ToInt() == 0) return;

            bool noKeeper = string.IsNullOrEmpty(sKeeperBank.Value);
            bool noManager = string.IsNullOrEmpty(sManager.Value);
            if (!noKeeper && !noManager) return;

            Document doc = sAgreement.GetObjectById(sAgreement.Value) as Document;
            if (doc.DataUnavailable)
            {
                //List<DocPersons> persons = DocPersons.GetDocsPersonsByDocId(sAgreement.Value.ToInt);
                List<int> persons = DocPersons.LoadPersonsByDocId(sAgreement.Value.ToInt());
                if (persons.Count == 2
                    && persons[0] != 0
                    && persons[1] != 0
                    && persons[0] != persons[1])
                {
                    if (noKeeper)
                    {
                        int ManagerId = sManager.Value.ToInt();

                        if (ManagerId == persons[1])
                            sKeeperBank.Value = persons[0].ToString();
                        else
                            if (ManagerId == persons[0])
                                sKeeperBank.Value = persons[1].ToString();
                    }

                    if (noManager)
                    {
                        int KeeperBankId = sKeeperBank.Value.ToInt();

                        if (KeeperBankId == persons[1])
                            sManager.Value = persons[0].ToString();
                        else
                            if (KeeperBankId == persons[0])
                                sManager.Value = persons[1].ToString();
                    }
                }
            }
            else
            {
                if (doc.DocumentData.PersonId3 == null && doc.DocumentData.PersonId4 == null && doc.DocumentData.PersonId5 == null && doc.DocumentData.PersonId6 == null)
                {
                    if (doc.DocumentData.PersonId1 != doc.DocumentData.PersonId2)
                    {
                        if (noKeeper && doc.DocumentData.PersonId1.HasValue)
                        {
                            if (noManager || (doc.DocumentData.PersonId2.HasValue && sManager.Value.ToInt() == doc.DocumentData.PersonId2))
                                sKeeperBank.Value = doc.DocumentData.PersonId1.ToString();
                        }

                        if (noManager && doc.DocumentData.PersonId2.HasValue)
                        {
                            if (noKeeper || (doc.DocumentData.PersonId1.HasValue && sKeeperBank.Value.ToInt() == doc.DocumentData.PersonId1))
                                sManager.Value = doc.DocumentData.PersonId2.ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Метод пытается загрузить сохраненные параметры приложения Склады предыдущих версии
        /// </summary>
        /// <param name="pageHelper">Вспомогательный объект</param>
        /// <returns>True исли установлены настройки от предыдущих версий приложения Склады</returns>
        bool TryOldParameters(StoresPageHelper pageHelper)
        {
            bool isRequiredStoreResidence = false;
            string valueStoreResidence = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Residence, out isRequiredStoreResidence);//место хранения
            bool isRequiredStoreContract = false;
            string valueStoreContract = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Contract, out isRequiredStoreContract);//договор хранения

            bool isOldParameters = null != valueStoreResidence || null != valueStoreContract;

            if (isOldParameters)
            {
                if (null != valueStoreResidence)
                    pageHelper.setSelectCtrlValue(sStorage, ((int)SelectEnum.Contain).ToString(), valueStoreResidence, isRequiredStoreResidence);

                if (null != valueStoreContract)
                    pageHelper.setSelectCtrlValue(sAgreement, ((int)SelectEnum.Contain).ToString(), valueStoreContract, isRequiredStoreContract);
            }

            return isOldParameters;
        }

        /*
        /// <summary>
        /// Метод добавляет склад в выбранный отчёт по складам
        /// </summary>
        private void AddStoreToReport()
        {
            if (_storeId == 0) return;
            if (string.IsNullOrEmpty(sStoreReportType.Value))
            {
                ShowMessage(Resx.GetString("STORE_SelectReportType"), sStoreReportType);
                return;
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@КодСклада", _storeId);
            parameters.Add("@КодТипаОтчётаПоСкладам", sStoreReportType.Value);

            try
            {
                DBManager.ExecuteNonQuery(Kesco.Lib.Entities.SQLQueries.INSERT_СкладОтчётыПоСкладам, CommandType.Text, Config.DS_person, parameters);

                btnAddToReport.Visible = false;
                btnDisplayReport.Visible = true;
            }
            catch (DetailedException dex)
            {
                ShowMessage(string.Format(Resx.GetString("STORE_FailedAddToReportType"), dex.Message));
            }
        }

        /// <summary>
        /// Метод возвращает первое значение типа отчета по складам в котором присутствует текущий склад
        /// </summary>
        /// <returns>Идентификатор типа отчета по складам</returns>
        int GetStoreReportType()
        {
            if (_storeId == 0) return 0;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@КодСклада", _storeId);

            object report_type_id = DBManager.ExecuteScalar(Kesco.Lib.Entities.SQLQueries.SELECT_КодТипаОтчётаПоСкладам, CommandType.Text, Config.DS_person, parameters);

            if (report_type_id == null) return 0;

            return (int)report_type_id;
        }

        /// <summary>
        /// Метод проверяет что склад добавлен в отчет по складам с выбранным идентификатором
        /// </summary>
        /// <returns>True если склад уже добавлен в выбранный отчёт по складам</returns>
        private bool IsStoreInReport()
        {
            if (_storeId == 0) return false;
            if (string.IsNullOrEmpty(sStoreReportType.Value)) return false;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@КодСклада", _storeId);
            parameters.Add("@КодТипаОтчётаПоСкладам", sStoreReportType.Value);

            object report_type_id = DBManager.ExecuteScalar(Kesco.Lib.Entities.SQLQueries.SELECT_ОтчётПоСкладам, CommandType.Text, Config.DS_person, parameters);

            return report_type_id != null;
        }

        /// <summary>
        /// Метод управляет отображением элементов интерфейса для добавления склада в отчеты по складам
        /// </summary>
        private void DisplayReport()
        {
            ReportPanel.Visible = false;
            btnAddToReport.Visible = false;
            btnDisplayReport.Visible = false;

            if (_storeId != 0)
            {
                IEnumerable list = sStoreReportType.FillSelect(string.Empty);
                IEnumerator ir = list.GetEnumerator();
                ir.Reset();
                if (ir.MoveNext())
                {
                    ReportPanel.Visible = true;

                    int report_type_id = GetStoreReportType();

                    btnAddToReport.Visible = report_type_id == 0;

                    Kesco.Lib.Entities.Item item = (Kesco.Lib.Entities.Item)ir.Current;

                    if (!btnAddToReport.Visible)
                    {
                        do
                        {
                            if (item.Id.ToInt() == report_type_id)
                            {
                                sStoreReportType.Value = item.Id;
                                sStoreReportType.ValueText = item.Value.ToString();
                                break;
                            }
                        }
                        while (ir.MoveNext());
                    }
                    else
                    {
                        sStoreReportType.Value = item.Id;
                        sStoreReportType.ValueText = item.Value.ToString();
                    }

                    btnDisplayReport.Visible = report_type_id != 0;
                }
            }
        }
         */

        /// <summary>
        /// Метод формирует разметку панели инструментов страницы
        /// </summary>
        public string RenderDocumentHeader()
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
        protected void SetMenuButtons()
        {
            Button btnOk = new Button
            {
                ID = "btnOK",
                V4Page = this,
                Text = Resx.GetString("cmdOK"),
                Title = Resx.GetString("cmdOK"),
                IconJQueryUI = "v4_buttonIcons.Ok",
                Width = 105,
                OnClick = "SaveStore('ok');"
            };

            Button btnSave = new Button
            {
                ID = "btnSave",
                V4Page = this,
                Text = Resx.GetString("cmdSave"),
                Title = Resx.GetString("titleSave"),
                IconJQueryUI = "v4_buttonIcons.Save",
                Width = 105,
                OnClick = "SaveStore('save');"
            };

            Button btnRefresh = new Button
            {
                ID = "btnRefresh",
                V4Page = this,
                Text = Resx.GetString("cmdRefresh"),
                Title = Resx.GetString("cmdRefresh"),
                IconJQueryUI = "v4_buttonIcons.Refresh",
                Width = 105,
                OnClick = "cmd('cmd', 'RefreshButton');"//"if(confirm('Replace')) window.location.replace(window.location.href);"
            };

            Button btnCancel = new Button
            {
                ID = "btnCancel",
                V4Page = this,
                Text = Resx.GetString("cmdCancel"),
                Title = Resx.GetString("cmdCancel"),
                IconJQueryUI = "v4_buttonIcons.Cancel",
                Width = 105,
                OnClick = "cmd('cmd', 'CancelButton');"
            };
            /*
            Button btnClear = new Button
            {
                ID = "btnClear",
                V4Page = this,
                Text = Resx.GetString("lClear"),
                Title = Resx.GetString("lClear"),
                Style = "BACKGROUND: buttonface url(/Styles/Delete.gif) no-repeat left center;",
                Width = 105,
                OnClick = "cmd('cmd', 'ClearButton');"
            };

            Button btnDelete = new Button
            {
                ID = "btnDelete",
                V4Page = this,
                Text = Resx.GetString("cmdDelete"),
                Title = Resx.GetString("cmdDelete"),
                Style = "BACKGROUND: buttonface url(/Styles/Delete.gif) no-repeat left center;",
                Width = 105,
                OnClick = "cmd('cmd', 'DeleteButton');"
            };
            */

            Button exportTo1S = new Button
            {
                ID = "btnTo1S",
                V4Page = this,
                Text = Resx.GetString("cmdExportTo1S"),
                Title = Resx.GetString("cmdExportTo1S"),
                //Style = "BACKGROUND: buttonface url(/Styles/1s.gif) no-repeat left center;",
                Width = 125,
                OnClick = "cmd('cmd', 'ExportButton');"
            };

            Button btnClose = new Button
            {
                ID = "btnClose",
                V4Page = this,
                Text = Resx.GetString("cmdClose"),
                Title = Resx.GetString("cmdClose"),
                IconJQueryUI = "v4_buttonIcons.Close",
                Width = 105,
                OnClick = "cmd('cmd', 'CloseButton');"
            };

            Button[] buttons = new Button[] { btnOk, btnSave, btnRefresh, /*btnClear, btnDelete, */exportTo1S, btnCancel, btnClose };

            if (_storeId == 0)
            {
                //btnDelete.Visible = false;
                exportTo1S.Visible = false;
            }
            else
            {
                //btnDelete.Visible = true;
                exportTo1S.Visible = true;
            }

            AddMenuButton(buttons);
        }

        /// <summary>
        /// Метод добавляет склад в выбранные отчёты по складам, если он там отсутствует
        /// </summary>
        private bool SetStoreReportTypes()
        {
            if (_storeId == 0) return true;
            if (string.IsNullOrWhiteSpace(sStoreReportType.SelectedItemsString))
            {
                //DeleteStoreReportTypes();
                _reports = null;
                return true;
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@КодСклада", _storeId);
            parameters.Add("@КодыТиповОтчётаПоСкладам", (object)sStoreReportType.SelectedItemsString);

            try
            {
                DBManager.ExecuteNonQuery(Kesco.Lib.Entities.SQLQueries.INSERT_СкладОтчётыПоСкладам, CommandType.Text, Config.DS_person, parameters);

                //DBManager.ExecuteNonQuery(Kesco.Lib.Entities.SQLQueries.MERGE_СкладОтчётыПоСкладам, CommandType.Text, Config.DS_person, parameters);

                _reports = new string[sStoreReportType.SelectedItems.Count];
                int j=0;
                foreach (Kesco.Lib.Entities.Item item in sStoreReportType.SelectedItems)
                {
                    _reports[j++] = item.Id;
                }
            }
            catch (DetailedException dex)
            {
                ShowMessage(string.Format(Resx.GetString("STORE_FailedAddToReports"), dex.Message), Title, MessageStatus.Error, "ErrHandler");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Метод удаляет склад из всех отчетов кроме выбранных
        /// </summary>
        private bool DeleteStoreReportTypes()
        {
            if (_storeId == 0) return true;

            object objReports = DBNull.Value;
            if (null != sStoreReportType.SelectedItemsString) objReports=(object)sStoreReportType.SelectedItemsString;

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@КодСклада", _storeId);
            parameters.Add("@КодТипаОтчётаПоСкладам", objReports);

            try
            {
                DBManager.ExecuteNonQuery(Kesco.Lib.Entities.SQLQueries.DELETE_СкладИзОтчётовПоСкладам, CommandType.Text, Config.DS_person, parameters);
            }
            catch (DetailedException dex)
            {
                ShowMessage(string.Format(Resx.GetString("STORE_FailedRemoveFromReports"), dex.Message), Title);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Метод устанавливает список отчетов по складам куда уже добавлен склад
        /// </summary>
        void DisplayStoreReportTypes()
        {
            ReportPanel.Visible = false;

            //Проверка наличия роли сотрудника
            IEnumerable list = sStoreReportType.FillSelect(string.Empty);
            IEnumerator ir = list.GetEnumerator();
            ir.Reset();
            ReportPanel.Visible = ir.MoveNext();
            if (!ReportPanel.Visible) return;
            ////////////////////////////////

            if (_storeId == 0) return;

            sStoreReportType.SelectedItems.Clear();

            string rowNumber = "-1";
            string pageNum = "-1";
            string itemsPerPage = "-1";
            string pageCount = "-1";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@КодСклада", _storeId);

            DataTable dt = DBManager.GetData(Kesco.Lib.Entities.SQLQueries.SELECT_КодыТиповОтчётовПоСкладам, Config.DS_person, CommandType.Text, parameters, null, string.Empty, string.Empty, null, null, null, ref pageNum, ref itemsPerPage, ref pageCount, out rowNumber);

            if (null != dt && dt.Rows.Count > 0)
            {
                _reportTypes=new int[dt.Rows.Count];
                for(int i=0; i<dt.Rows.Count; i++)
                {
                    DataRow r = dt.Rows[i];
                    _reportTypes[i]=(int)r["КодТипаОтчётаПоСкладам"];
                    string strId = _reportTypes[i].ToString();

                    object objReportTypeById = sStoreReportType.GetObjectById(strId);
                    if (null!=objReportTypeById)
                        sStoreReportType.SelectedItems.Add(new Kesco.Lib.Entities.Item { Id = strId, Value = objReportTypeById });
                }

                _reports = new string[sStoreReportType.SelectedItems.Count];
                int j=0;
                foreach (Kesco.Lib.Entities.Item item in sStoreReportType.SelectedItems)
                {
                    _reports[j++] = item.Id;
                }
            }
        }

        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            _SizePosKeeper.ProcessCommand(cmd, param);

            switch (cmd)
            {
                case "SaveButton":
                    {
                        string strParam = param["param"];
                        int StoreTypeCode = 0;
                        int id = SaveStore(ref StoreTypeCode, strParam=="ok");
                        if (id != -1)
                        {
                            //SavePageParameters();
                        }
                    }
                    break;

                case "ExportButton":
                    {
                        if (_storeId != 0)
                        {
                            int StoreTypeCode = 0;
                            int id = _storeId;
                            if (IsChanged())
                                id = SaveStore(ref StoreTypeCode, false);
                            else
                                StoreTypeCode = sStoreType.Value.ToInt();
                            if (id != -1)
                                StoresClientScripts.ExportTo1S(this, id, StoreTypeCode);
                        }
                    }
                    break;

                case "ClearButton":
                    ClearForm();
                    break;

                case "CloseButton":
                    if (IsChanged())
                    {
                        StoresClientScripts.ConfirmReload(this, Resx.GetString("STORE_ConfirmClose"), "", "CloseConfirmed");
                    }
                    else
                        goto case "CloseConfirmed";

                    break;

                case "CancelButton":
                    if (IsChanged())
                    {
                        StoresClientScripts.ConfirmReload(this, Resx.GetString("STORE_ConfirmCancel"), "", "ReloadConfirmed");
                    }
                    break;

                case "RefreshButton":
                    if (IsChanged())
                    {
                        StoresClientScripts.ConfirmReload(this, Resx.GetString("STORE_ConfirmRefresh"), "", "ReloadConfirmed");
                    }
                    else
                    {
                        RefreshNtf();
                    }
                    break;

                case "CloseConfirmed":
                    _SizePosKeeper.ProcessCommand("PageClose", null);
                    Close();
                    break;

                case "ReloadConfirmed":
                    V4Navigate(_origUrl.AbsoluteUri);
                    break;

                /*
                case "AddStoreToReport":
                    AddStoreToReport();
                    break;

                case "DisplayStoresReport":
                    if (!string.IsNullOrEmpty(sStoreReportType.Value))
                        StoresClientScripts.DisplayStoresReport(this, sStoreReportType.Value);
                    break;
                */
                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        /// <summary>
        /// Метод устанавливает свойства и значения полей при установке или отключении неизвестного номера счета
        /// </summary>
        /// <param name="value">true - установить, false - отключить</param>
        private void SetStoreNumberUndefined(bool value)
        {
            //txtName.Value = string.Empty;
            //if (value)
            //    txtName.Value = StoreNameTextBox.NoNameValue;

            int storeTypeID = 0;
            bool fIban = false;
            bool fBank = false;
            if (int.TryParse(sStoreType.Value, out storeTypeID))
            {
                fIban = storeTypeID == 2;
                fBank = storeTypeID>=1 && storeTypeID<=7;
            }

            //Номер счета не является обязательным если указан IBAN
            txtName.IsRequired = !fBank || (!fIban && !chkAccountNumberUnknown.Checked);
            txtName.IsDisabled = value;
            txtIBAN.IsDisabled = txtName.IsDisabled;

            //txtName.IsReadOnly = value;
            //txtIBAN.IsReadOnly = txtName.IsReadOnly;
        }

        /// <summary>
        /// Метод устанавливает свойства полей в соответсвии с типом склада
        /// </summary>
        /// <param name="storeTypeID">Тип выбранного склада</param>
        void SetStoreType(int storeTypeID)
        {
            bool fBank = false;
            bool fIban = false;
            bool fStorage = false;
            bool fNoBankCurrency = false;//Для кассы и лицевого счёта необходимо показывать ресурс Валюта
            bool fNameRequired = true;

            switch (storeTypeID)
            {
                case 2://Валютный счёт
                    fIban = true;
                    fNameRequired = false;//можно указать IBAN
                    goto case 1;
                case 1://Расчетный счёт (руб.)
                case 3://Валютный транзитный счёт
                case 4://Валютный текущий счёт
                case 5://Счёт в банке-корреспонденте
                case 6://Спец. карточный счёт
                case 7://Депозитный счёт
                    fBank = true;
                    break;

                case 11://Касса
                case 12://Лицевой счёт
                    fNoBankCurrency = true;
                    break;

                case -1://Виртуальный склад
                    fNameRequired = false;
                    goto case 21;
                case 21://Склад продуктов
                case 22://Склад сырья и полуфабрикатов
                case 23://Склад готовой продукции
                    fStorage = true;
                    break;
                default:
                    sResource.Filter.AllChildrenWithParentIDs.Value = "2";//Товарно-материальные ценности (ТМЦ)
                    break;
            }

            if (!fBank && chkAccountNumberUnknown.Checked)
            {
                chkAccountNumberUnknown.Checked = false;
                if (txtName.Value == StoreNameTextBox.NoNameValue)
                    txtName.Value = string.Empty;
            }

            txtIBAN.IsDisabled = !fIban || chkAccountNumberUnknown.Checked;
            Display("IbanRow", fIban);

            //Номер счета не является обязательным если указан IBAN
            txtName.IsRequired = !fBank || (!fIban && !chkAccountNumberUnknown.Checked);
            txtName.IsDisabled = chkAccountNumberUnknown.Checked;

            //Для складов показывам поле Подразделения и Место хранения (обязательно)
            sStorage.IsDisabled = !fStorage;
            sStorage.IsRequired = fStorage && fNameRequired;
            Display("StorageRow", fStorage);

            sManagerDepartment.IsDisabled = !fStorage;
            Display("ManagerDepartmentRow", fStorage);

            chkAccountNumberUnknown.IsDisabled = !fBank;
            Display("AccountNumberUnknownRow", fBank);

            txtStoreDepartment.IsDisabled = !fBank;
            Display("StoreDepartmentRow", fBank);

            //Установка фильтров поиска на связанных элементах управления
            if (fBank)
            {
                sKeeperBank.Filter.PersonWhereSearch = 3;
                sKeeperBank.Filter.PersonType = 4;
            }
            else
            {
                sKeeperBank.Filter.PersonType = 0;
                sKeeperBank.Filter.PersonWhereSearch = 1;
            }

            StoreType st = new StoreType(storeTypeID.ToString());

            if (null != st.RootSource)
                sResource.Filter.AllChildrenWithParentIDs.Value = st.RootSource;
            else
                sResource.Filter.AllChildrenWithParentIDs.Clear();

            if (1 == storeTypeID)//Только рубль устанавливаем
            {
                if (string.IsNullOrWhiteSpace(sResource.Value))
                    sResource.Value = st.RootSource;
            }

            if (fBank)
            {
                NameLabel.Value = Resx.GetString("STORE_Account");
                KeeperBankLabel.Value = Resx.GetString("STORE_Bank");
                ResourceLabel.Value = Resx.GetString("STORE_Currency");
                StoreDepartmentLabel.Value = Resx.GetString("STORE_Branch");
            }
            else
            {
                NameLabel.Value = Resx.GetString("STORE_Name");
                KeeperBankLabel.Value = Resx.GetString("STORE_Keeper");
                if (fNoBankCurrency)
                    ResourceLabel.Value = Resx.GetString("STORE_Currency");
                else
                    ResourceLabel.Value = Resx.GetString("STORE_Resource");

                StoreDepartmentLabel.Value = Resx.GetString("STORE_Department");
            }
        }

        /// <summary>
        /// Метод очистки всех полей формы и результатов поиска
        /// </summary>
        void ClearForm()
        {
            //ReportPanel.Visible = false;
            //sStoreReportType.Value = null;

            dateValidFrom.ValueDate = DateTime.Now;
            dateValidTo.ValueDate = DateTime.Now;
            txtName.Value = string.Empty;
            txtIBAN.Value = string.Empty;
            txtStoreDepartment.Value = string.Empty;
            txtDescription.Value = string.Empty;

            foreach (DBSelect s in new DBSelect[] { sStoreType, sStorage, sKeeperBank, sManager, sManagerDepartment, sResource, sAgreement })
            {
                s.Value = null;
                s.ClearSelectedItems();
            }
        }

        /// <summary>
        /// Медот формирует строку вида имя склада/IBAN для возврата в другие приложения
        /// </summary>
        /// <param name="name">имя склада</param>
        /// <param name="iban">IBAN</param>
        /// <returns>строка вида имя склада/IBAN</returns>
        string GetStoreReturnName(string name, string iban)
        {
            bool n = !string.IsNullOrEmpty(name);
            bool i = !string.IsNullOrEmpty(iban);

            if (n && i) return name + '/' + iban;
            else
                if (i) return iban;
                else
                    if (n) return name;

            return string.Empty;
        }

        /// <summary>
        /// Сохраняет параметры склада в БД
        /// </summary>
        /// <returns>Идентификатор склада</returns>
        int SaveStore(ref int StoreTypeCode, bool fClose)
        {
            if (!V4Validation()) return -1;

            StoreTypeCode = sStoreType.Value.ToInt();

            string Name;
            string Iban;
            if (chkAccountNumberUnknown.Checked)
            {
                Name = Iban = StoreNameTextBox.NoNameValue;
            }
            else
            {
                Name = txtName.Value;
                Iban = txtIBAN.Value;
            }

            int StorageCode = sStorage.Value.ToInt();
            int ResourceCode = sResource.Value.ToInt();
            int KeeperCode = sKeeperBank.Value.ToInt();
            int ManagerCode = sManager.Value.ToInt();
            int ManagerDepartmentCode = sManagerDepartment.Value.ToInt();
            int AgreementCode = sAgreement.Value.ToInt();

            DateTime from = dateValidFrom.ValueDate.GetValueOrDefault(DateTime.MinValue);
            DateTime to = dateValidTo.ValueDate.GetValueOrDefault(DateTime.MaxValue);

            if (to < from)
            {
                ShowMessage(Resx.GetString("STORE_DatesAreNotValid"), Title);
                return -1;
            }

            Dictionary<string, object> sqlParams = new Dictionary<string, object>();

            sqlParams.Add("@КодСклада", _storeId == 0 ? DBNull.Value : (object)_storeId);
            sqlParams.Add("@КодХранителя", KeeperCode == 0 ? DBNull.Value : (object)KeeperCode);
            sqlParams.Add("@КодРаспорядителя", ManagerCode == 0 ? DBNull.Value : (object)ManagerCode);
            sqlParams.Add("@КодПодразделенияРаспорядителя", ManagerDepartmentCode == 0 ? DBNull.Value : (object)ManagerDepartmentCode);

            sqlParams.Add("@Склад", Name);
            sqlParams.Add("@IBAN", Iban);

            sqlParams.Add("@КодТипаСклада", StoreTypeCode == 0 ? DBNull.Value : (object)StoreTypeCode);
            sqlParams.Add("@КодМестаХранения", StorageCode == 0 ? DBNull.Value : (object)StorageCode);

            sqlParams.Add("@Филиал", txtStoreDepartment.Value);
            sqlParams.Add("@Примечание", txtDescription.Value);

            sqlParams.Add("@КодРесурса", ResourceCode == 0 ? DBNull.Value : (object)ResourceCode);

            if (sAgreement.Visible)
                sqlParams.Add("@КодДоговора", AgreementCode == 0 ? DBNull.Value : (object)AgreementCode);

            sqlParams.Add("@От", from == DateTime.MinValue ? DBNull.Value : (object)from);
            sqlParams.Add("@По", to == DateTime.MaxValue ? DBNull.Value : (object)to);

            Dictionary<string, object> outputParams = new Dictionary<string, object>();

            int storeId = 0;
            try
            {
                DBManager.ExecuteNonQuery(Kesco.Lib.Entities.SQLQueries.SP_Лица_InsUpd_Склады, CommandType.StoredProcedure, Config.DS_person, sqlParams, outputParams);

                if (outputParams.ContainsKey("@RETURN_VALUE"))
                    storeId = (int)outputParams["@RETURN_VALUE"];

                int origStoreId = _storeId;
                _storeId = storeId;

                string storeUrl = _origUrl.AbsoluteUri;
                if (origStoreId == 0)
                {
                    int start_id = -1;
                    if ((start_id = _origUrl.Query.IndexOf("?id=")) < 0 && (start_id = _origUrl.Query.IndexOf("&id=")) < 0)
                    {
                        //В запросе отсутствовал идентификатор склада
                        storeUrl = _origUrl.Scheme + "://" + _origUrl.Host + _origUrl.AbsolutePath + "?id=" + storeId.ToString();
                        if (_origUrl.Query.Length > 0)
                        {
                            if(_origUrl.Query[0] == '?')
                                storeUrl += "&" + _origUrl.Query.Substring(1);
                            else
                                storeUrl += _origUrl.Query.Substring(1);
                        }
                    }
                    else
                    {
                        int stop_id = _origUrl.Query.IndexOf('&', start_id + 4);
                        if (stop_id < 0) stop_id = _origUrl.Query.Length;
                        storeUrl = _origUrl.Scheme + "://" + _origUrl.Host + _origUrl.AbsolutePath + _origUrl.Query.Substring(0, start_id + 4) + storeId.ToString() + _origUrl.Query.Substring(stop_id);
                    }
                }

                if (ReportPanel.Visible)
                {
                    if (storeId != 0)
                        sStoreReportType.URLShowEntity += string.Format("?selected={0}", storeId);

                    if (!SetStoreReportTypes() || !DeleteStoreReportTypes())
                    {
                        StoresClientScripts.SetErrDialogOkHandler(this, storeUrl);

                        return storeId;
                    }
                }

                if (fClose)
                {
                    string strReturnStoreName = GetStoreReturnName(Name, Iban);

                    StoresClientScripts.NotifyParentWindow(this, storeId, strReturnStoreName);

                    if (ReturnId == "1" || mvc=="1")
                    {
                        StoresClientScripts.ReturnValue(this, storeId, strReturnStoreName);
                    }
                    else
                    {
                        ProcessCommand("CloseConfirmed", null);
                        //Close();
                    }
                }
                else
                {
                    this.V4Navigate(storeUrl);
                }
            }
            catch (DetailedException dex)
            {
                ShowMessage(string.Format(Resx.GetString("STORE_FailedSave"), dex.Message), Title);
                return -1;
            }

            return storeId;
        }

        /// <summary>
        /// Сохранение последних значений полей страницы в БД настроек
        /// </summary>
        void SavePageParameters()
        {
            AppParamsManager parametersManager = new AppParamsManager(this.ClId, new StringCollection());

            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.ValidFrom, dateValidFrom.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.ValidTo, dateValidTo.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.Type, sStoreType.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.Storage, sStorage.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.Name, txtName.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.Iban, txtIBAN.Value, AppParamType.SavedWithClid));

            string checkBoxValue = chkAccountNumberUnknown.Checked ? "1" : "0";

            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.AccountNumberUnknown, checkBoxValue, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.Keeper, sKeeperBank.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.Manager, sManager.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.ManagerDepartment, sManagerDepartment.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.Resource, sResource.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.Agreement, sAgreement.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.Department, txtStoreDepartment.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.StoreParameters.Description, txtDescription.Value, AppParamType.SavedWithClid));

            parametersManager.SaveParams();
        }

        /// <summary>
        /// Возвращает true если значение поля (строка) равно исходному значению полученному из БД
        /// </summary>
        /// <param name="index"></param>
        /// <param name="str_value"></param>
        /// <returns></returns>
        bool TestStringField(string index, string str_value)
        {
            object dbObj = _source[index];
            if (string.IsNullOrWhiteSpace(str_value))
            {
                if (DBNull.Value == dbObj) return true;
                if (str_value == (string)dbObj) return true;
            }
            else
                if (str_value == (string)dbObj) return true;

            return false;
        }

        /// Возвращает true если значение поля (целое число) равно исходному значению полученному из БД
        bool TestIntField(string index, int? n_value)
        {
            object dbObj = _source[index];
            if (DBNull.Value == dbObj)
            {
                if (n_value == null || n_value == 0) return true;
            }
            else
            {
                if (n_value.HasValue && n_value == (int)dbObj) return true;
            }

            return false;
        }

        /// Возвращает true если значение поля (дата и время) равно исходному значению полученному из БД
        bool TestDateField(string index, DateTime? dt_value)
        {
            object dbObj = _source[index];
            if (DBNull.Value == dbObj)
            {
                if (dt_value == null) return true;
            }
            else
                if (dt_value == (DateTime)dbObj) return true;

            return false;
        }

        bool IsChanged()
        {
            if (_source == null)
            {
                if (!string.IsNullOrWhiteSpace(txtName.Value)) return true;
                if (!string.IsNullOrWhiteSpace(txtIBAN.Value)) return true;
                if (!string.IsNullOrWhiteSpace(txtStoreDepartment.Value)) return true;
                if (!string.IsNullOrWhiteSpace(txtDescription.Value)) return true;

                if (sStoreType.ValueInt.HasValue && sStoreType.ValueInt!=0) return true;
                if (sResource.ValueInt.HasValue && sResource.ValueInt != 0) return true;
                if (sKeeperBank.ValueInt.HasValue && sKeeperBank.ValueInt != 0) return true;
                if (sManager.ValueInt.HasValue && sManager.ValueInt != 0) return true;
                if (sManagerDepartment.ValueInt.HasValue && sManagerDepartment.ValueInt != 0) return true;
                if (sAgreement.ValueInt.HasValue && sAgreement.ValueInt != 0) return true;

                if (dateValidFrom.ValueDate.HasValue) return true;
                if (dateValidTo.ValueDate.HasValue) return true;

                if (sStoreReportType.SelectedItems != null && sStoreReportType.SelectedItems.Count > 0) return true;
                return false;
            }

            if (!TestStringField("Склад", txtName.Value)) return true;
            if (!TestStringField("IBAN", txtIBAN.Value)) return true;
            if (!TestDateField("От", dateValidFrom.ValueDate)) return true;
            if (!TestDateField("По", dateValidTo.ValueDate)) return true;
            if (!TestIntField("КодТипаСклада", sStoreType.ValueInt)) return true;
            if (!TestIntField("КодМестаХранения", sStorage.ValueInt)) return true;
            if (!TestIntField("КодРесурса", sResource.ValueInt)) return true;
            if (!TestIntField("КодХранителя", sKeeperBank.ValueInt)) return true;
            if (!TestIntField("КодРаспорядителя", sManager.ValueInt)) return true;
            if (!TestIntField("КодПодразделенияРаспорядителя", sManagerDepartment.ValueInt)) return true;
            if (!TestIntField("КодДоговора", sAgreement.ValueInt)) return true;
            if (!TestStringField("Филиал", txtStoreDepartment.Value)) return true;
            if (!TestStringField("Примечание", txtDescription.Value)) return true;

            if (sStoreReportType.SelectedItems != null && sStoreReportType.SelectedItems.Count>0)
            {
                if (_reports == null) return true;
                if (sStoreReportType.SelectedItems.Count != _reports.Length) return true;

                for(int i = 0 ; i<_reports.Length; i++)
                {
                    if (!sStoreReportType.SelectedItems.Exists(si => si.Id == _reports[i])) return true;
                }
            }
            else
                if(_reports!=null && _reports.Length>0) return true;

            return false;
        }
    }
}