using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Persons.PersonOld;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Settings;
using Item = Kesco.Lib.Web.Controls.V4.Item;

namespace Kesco.App.Web.Stores
{
    /// <inheritdoc />
    /// <summary>
    ///     Класс формы StorePerson
    /// </summary>
    public partial class StorePerson : EntityPage
    {
        /// <summary>
        ///     Локальная переменная кода лица
        /// </summary>
        private int? _personId;

        /// <summary>
        ///     Получение кода лица из строки запроса
        /// </summary>
        private int PersonId
        {
            get
            {
                if (!_personId.HasValue)
                    _personId = Request.QueryString["personId"].ToInt();
                return _personId.Value;
            }
        }

        #region Fields

        /// <summary>
        ///     Ссылка на справку
        /// </summary>
        public override string HelpUrl { get; set; }

        /// <summary>
        ///     Текущее лицо
        /// </summary>
        protected PersonOld PersonObj { get; set; }

        /// <summary>
        ///     Название лица
        /// </summary>
        protected string PersonName { get; set; }

        /// <summary>
        ///     Название лица на английском
        /// </summary>
        protected string PersonNameEn { get; set; }

        /// <summary>
        ///     КПП
        /// </summary>
        protected string PersonKpp { get; set; }

        /// <summary>
        ///     Адрес лица
        /// </summary>
        protected string PersonAddress { get; set; }

        /// <summary>
        ///     Адрес лица на английском
        /// </summary>
        protected string PersonAddressEn { get; set; }

        #endregion

        #region Инициализация

        protected override void EntityInitialization(Entity copy = null)
        {
        }

        /// <summary>
        ///     Инициализация контролоа
        /// </summary>
        private void InitControls()
        {
            #region Язык

            radioLang.IsRow = false;
            radioLang.Items.Add(new Item {Code = "ru", Name = Resx.GetString("STORE_SP_Lbl_LangRu")});
            radioLang.Items.Add(new Item {Code = "en", Name = Resx.GetString("STORE_SP_Lbl_LangEn")});
            radioLang.Value = "ru";
            radioLang.Changed += RadioLang_Changed;
            radioLang.NextControl = "flagName";

            #endregion

            #region Название

            flagName.Changed += FlagName_Changed;
            flagName.NextControl = "flagINN";

            #endregion

            #region ИНН

            flagINN.Changed += FlagINN_Changed;
            flagINN.NextControl = "flagOGRN";

            #endregion

            #region ОГРН

            flagOGRN.Changed += FlagOGRN_Changed;
            flagOGRN.NextControl = "flagKPP";

            #endregion

            #region КПП

            flagKPP.Changed += FlagKPP_Changed;
            flagKPP.NextControl = "flagAddress";

            #endregion

            #region Адрес

            flagAddress.Changed += FlagAddress_Changed;
            flagAddress.NextControl = "flagRS";

            #endregion

            #region Расчетный счет

            dbsStoreRS.Filter.ManagerId.Value = PersonId.ToString();
            dbsStoreRS.Filter.StoreResourceId.Value = "183";
            dbsStoreRS.Filter.StoreTypeId.Add("1");
            dbsStoreRS.Filter.ValidAt.Value = DateTime.Today.ToString("yyyyMMdd");

            flagRS.Changed += FlagRS_Changed;
            dbsStoreRS.IsDisabled = true;
            dbsStoreRS.NextControl = "flagDirector";

            dbsStoreRS.Changed += DBSStoreRS_Changed;

            #endregion

            #region Специальный счет

            dbsStoreSpec.Filter.ManagerId.Value = PersonId.ToString();
            dbsStoreSpec.Filter.StoreResourceId.Value = "183";
            dbsStoreSpec.Filter.StoreTypeId.Add("8");
            dbsStoreSpec.Filter.ValidAt.Value = DateTime.Today.ToString("yyyyMMdd");

            flagSpec.Changed += FlagSpec_Changed;
            dbsStoreSpec.IsDisabled = true;
            dbsStoreSpec.NextControl = "flagDirector";

            dbsStoreSpec.Changed += DBSStoreSpec_Changed;

            #endregion

            #region Валютный счет

            flagValS.Changed += FlagValS_Changed;
            dbsStoreValS.IsDisabled = true;
            dbsCurrency.IsDisabled = true;
            dbsCurrency.NextControl = "dbsStoreValS";
            dbsStoreValS.NextControl = "flagDirector";

            dbsCurrency.Changed += DBSCurrency_Changed;

            var listCurrency = GetCurrencyValStore();
            if (listCurrency.Count > 0)
                listCurrency.ForEach(x => dbsCurrency.Filter.CurrencyId.Add(x.ToString()));

            dbsStoreValS.Filter.ManagerId.Value = PersonId.ToString();
            dbsStoreValS.Filter.StoreTypeId.Add("2");
            dbsStoreValS.Filter.StoreTypeId.Add("3");
            dbsStoreValS.Filter.ValidAt.Value = DateTime.Today.ToString("yyyyMMdd");
            dbsStoreValS.BeforeSearch += DBSStoreValS_BeforeSearch;
            dbsStoreValS.Changed += DBSStoreValS_Changed;

            #endregion

            #region Директор

            dbsSign1.IsAlwaysAdvancedSearch = false;
            dbsSign1.IsAlwaysCreateEntity = true;

            dbsSign1.Filter.PersonLink = PersonId;
            dbsSign1.Filter.PersonLinkType = 1;
            dbsSign1.Filter.PersonLinkValidAt = DateTime.Today;
            dbsSign1.Filter.PersonSignType = 1;
            dbsSign1.Changed += DBSSign1_Changed;

            flagDirector.Changed += FlagDirector_Changed;
            dbsSign1.IsDisabled = true;

            dbsSign1.NextControl = "btnPrint";

            #endregion
        }

        /// <summary>
        ///     Получение данных текущего лица по IpPerson
        /// </summary>
        private void InitPerson()
        {
            if (PersonId <= 0)
                throw new Exception(string.Format("Неправильно переданы параметры! Id = {0}", PersonId));

            PersonObj = new PersonOld(PersonId.ToString());

            if (PersonObj == null && PersonObj.Unavailable)
                throw new Exception(string.Format("Лицо с кодом #{0} не найдено или не доступно!", PersonId));

            var crd = PersonObj.GetCard(DateTime.Today);

            if (crd == null)
                throw new Exception(
                    string.Format("У лица {0} нет действующих реквизитов на текущую дату!", PersonObj.Name));

            PersonName = crd.NameRus;
            PersonNameEn = crd.NameLat;
            PersonKpp = crd.КПП;
            PersonAddress = crd.АдресЮридический;
            PersonAddressEn = crd.АдресЮридическийЛат;
        }

        /// <summary>
        ///     Инициализация/создание кнопок меню
        /// </summary>
        private void SetMenuButtons()
        {
            var btnReport = new Button
            {
                ID = "btnPrint",
                V4Page = this,
                Text = Resx.GetString("STORE_SP_Btn_Report"),
                Title = Resx.GetString("STORE_SP_Btn_Report"),
                Width = 175,
                IconJQueryUI = ButtonIconsEnum.Print,
                OnClick = "stores_printStoresPerson();"
            };

            var btnClear = new Button
            {
                ID = "btnClear",
                V4Page = this,
                Text = Resx.GetString("STORE_SP_Btn_Clear"),
                Title = Resx.GetString("STORE_SP_Btn_Clear_Title"),
                IconJQueryUI = ButtonIconsEnum.Delete,
                Width = 90,
                OnClick = "cmdasync('cmd', 'ClearData');"
            };

            var btnClose = new Button
            {
                ID = "btnClose",
                V4Page = this,
                Text = Resx.GetString("cmdClose"),
                Title = Resx.GetString("cmdCloseTitleApp"),
                IconJQueryUI = ButtonIconsEnum.Close,
                Width = 90,
                OnClick = "v4_dropWindow();"
            };


            Button[] buttons = {btnReport, btnClear, btnClose};
            AddMenuButton(buttons);
        }

        /// <summary>
        ///     Отображение клиентских элементов в зависимости о наличия счетов опреденнного типа
        /// </summary>
        private void DisplayStoreTypes()
        {
            if (PersonObj.INN.Length > 0)
                JS.Write("$(\"#divINN\").show();");

            if (PersonObj.OGRN.Length > 0)
                JS.Write("$(\"#divOGRN\").show();");

            if (PersonKpp.Length > 0)
                JS.Write("$(\"#divKPP\").show();");

            if (PersonAddress.Length > 0)
                JS.Write("$(\"#divAddress\").show();");


            if (ExistsStores("1"))
                JS.Write("$(\"#divRS\").show();");

            if (ExistsStores("8"))
                JS.Write("$(\"#divSpec\").show();");

            if (ExistsStores("2,3"))
                JS.Write("$(\"#divValS\").show();");

            if (ExistsFirstSign())
                JS.Write("$(\"#divDirector\").show();");
        }

        #endregion

        #region Override

        /// <summary>
        ///     Обработка загрузки страницы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!V4IsPostBack)
            {
                InitPerson();
                InitControls();
                V4SetFocus("radioLangen_0");
                DisplayStoreTypes();
            }

            if (!V4IsPostBack)
            {
                dbsSign1.TryFindSingleValue();
                if (dbsSign1.Value.Length > 0)
                    DBSSign_Changed("divSign1", dbsSign1.Value, 1);
            }
        }

        /// <summary>
        ///     Обработка клиентских команд
        /// </summary>
        /// <param name="cmd">Название команды</param>
        /// <param name="param">Параметры</param>
        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            switch (cmd)
            {
                case "RefreshData":
                    DBSSign_Changed("divSign1", dbsSign1.Value, 1);
                    break;
                case "ClearData":
                    СlearData();


                    break;
                case "PrintData":
                    PrintData();
                    break;
                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        #endregion

        #region Report Settings

        /// <summary>
        ///     Очистка данных на форме
        /// </summary>
        private void СlearData()
        {
            flagName.Checked = false;
            flagINN.Checked = false;
            flagOGRN.Checked = false;
            flagKPP.Checked = false;
            flagAddress.Checked = false;
            flagRS.Checked = false;
            flagSpec.Checked = false;
            flagValS.Checked = false;
            flagDirector.Checked = false;


            RenderTextName();
            RenderTextInn();
            RenderTextOGRN();
            RenderTextKpp();
            RenderTextAddress();
            RS_Manage();
            Spec_Manage();
            ValS_Manage();
            Director_Manage();

            flagName.Focus();
        }

        /// <summary>
        ///     Открытие отчета Reporting Service
        /// </summary>
        private void PrintData()
        {
            if (!CheckData()) return;
            var url = string.Format(
                "{0}?/Persons/PersonRequisites&personId={1}{2}{3}{4}{5}{6}{7}{8}{9}{10}&DT={11}&rc:parameters=false&rs:ClearSession=true",
                Config.uri_Report,
                PersonId,
                flagDirector.Checked && dbsSign1.Value.Length > 0 ? string.Format("&sign1={0}", dbsSign1.Value) : "",
                flagName.Checked ? "&isName=1" : "",
                flagINN.Checked ? "&isInn=1" : "",
                flagOGRN.Checked ? "&isOgrn=1" : "",
                flagKPP.Checked ? "&isKpp=1" : "",
                flagAddress.Checked ? "&isAddress=1" : "",
                flagRS.Checked && dbsStoreRS.Value.Length > 0 || flagSpec.Checked && dbsStoreSpec.Value.Length > 0
                    ? string.Format("&isRS=1&storeVal={0}", flagRS.Checked ? dbsStoreRS.Value : dbsStoreSpec.Value)
                    : "",
                flagValS.Checked && dbsStoreValS.Value.Length > 0
                    ? string.Format("&isRS=0&storeVal={0}", dbsStoreValS.Value)
                    : "",
                string.Format("&lang={0}", radioLang.Value),
                DateTime.Now.ToString("HHmmss"));

            JS.Write("v4_windowOpen('{0}');", HttpUtility.JavaScriptStringEncode(url));
        }

        /// <summary>
        ///     Проверка наличия требуемых реквизитов для формирования отчета
        /// </summary>
        /// <returns>false/true</returns>
        private bool CheckData()
        {
            if (!flagName.Checked
                && !flagINN.Checked
                && !flagOGRN.Checked
                && !flagKPP.Checked
                && !flagAddress.Checked
                && !flagRS.Checked
                && !flagSpec.Checked
                && !flagValS.Checked
                && !flagDirector.Checked
            )
            {
                ShowMessage(Resx.GetString("STORE_SP_Msg_1"), Resx.GetString("STORE_SP_Msg_Error"),
                    MessageStatus.Error);
                return false;
            }

            if (flagRS.Checked && dbsStoreRS.Value.Length == 0)
            {
                ShowMessage(Resx.GetString("STORE_SP_Msg_2"), Resx.GetString("STORE_SP_Msg_Error"),
                    MessageStatus.Error, "dbsStoreRS_0");
                return false;
            }

            if (flagSpec.Checked && dbsStoreSpec.Value.Length == 0)
            {
                ShowMessage(Resx.GetString("STORE_SP_Msg_6"), Resx.GetString("STORE_SP_Msg_Error"),
                    MessageStatus.Error, "dbsStoreSpec_0");
                return false;
            }

            if (flagValS.Checked)
            {
                if (dbsCurrency.Value.Length == 0)
                {
                    ShowMessage(Resx.GetString("STORE_SP_Msg_3"), Resx.GetString("STORE_SP_Msg_Error"),
                        MessageStatus.Error, "dbsCurrency_0");
                    return false;
                }

                if (dbsStoreValS.Value.Length == 0)
                {
                    ShowMessage(Resx.GetString("STORE_SP_Msg_4"), Resx.GetString("STORE_SP_Msg_Error"),
                        MessageStatus.Error, "dbsStoreValS_0");
                    return false;
                }
            }

            if (flagDirector.Checked && dbsSign1.Value.Length == 0)
            {
                ShowMessage(Resx.GetString("STORE_SP_Msg_5"), Resx.GetString("STORE_SP_Msg_Error"),
                    MessageStatus.Error, "dbsSign1_0");
                return false;
            }

            return true;
        }

        #endregion

        #region Handlers

        private void RadioLang_Changed(object sender, ProperyChangedEventArgs e)
        {
            RenderTextName();
            RenderTextAddress();
            DBSStore_RenderText(dbsStoreRS.Value, "textRS");
            DBSStore_RenderText(dbsStoreValS.Value, "textValS");

            using (var w = new StringWriter())
            {
                RenderCompanyName(w, true);
                JS.Write("document.title = '{0}';", HttpUtility.JavaScriptStringEncode(w.ToString()));
            }

            using (var w = new StringWriter())
            {
                RenderCompanyName(w);
                JS.Write("$('#spanCompanyName').html('{0}');", HttpUtility.JavaScriptStringEncode(w.ToString()));
            }
        }

        private void FlagName_Changed(object sender, ProperyChangedEventArgs e)
        {
            RenderTextName();
        }

        private void FlagINN_Changed(object sender, ProperyChangedEventArgs e)
        {
            RenderTextInn();
        }

        private void FlagOGRN_Changed(object sender, ProperyChangedEventArgs e)
        {
            RenderTextOGRN();
        }

        private void FlagKPP_Changed(object sender, ProperyChangedEventArgs e)
        {
            RenderTextKpp();
        }

        private void FlagAddress_Changed(object sender, ProperyChangedEventArgs e)
        {
            RenderTextAddress();
        }

        private void FlagRS_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (flagRS.Checked)
            {
                flagValS.Checked = false;
                ValS_Manage();
                flagSpec.Checked = false;
                Spec_Manage();
                dbsStoreRS.TryFindSingleValue();
                DBSStore_RenderText(dbsStoreRS.Value, "textRS");
            }

            RS_Manage();
        }

        private void FlagSpec_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (flagSpec.Checked)
            {
                flagRS.Checked = false;
                RS_Manage();
                flagValS.Checked = false;
                ValS_Manage();
                dbsStoreSpec.TryFindSingleValue();
                DBSStore_RenderText(dbsStoreSpec.Value, "textSpec");
            }

            Spec_Manage();
        }

        private void FlagValS_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (flagValS.Checked)
            {
                flagRS.Checked = false;
                RS_Manage();
                flagSpec.Checked = false;
                Spec_Manage();
                dbsStoreValS.TryFindSingleValue();
                DBSStoreValS_SetValue();
            }

            ValS_Manage();
        }

        private void FlagDirector_Changed(object sender, ProperyChangedEventArgs e)
        {
            Director_Manage();
        }

        private void DBSSign1_Changed(object sender, ProperyChangedEventArgs e)
        {
            DBSSign_Changed("divSign1", dbsSign1.Value, 1);
            JS.Write("SetContolFocus('btnPrint');");
        }

        private void DBSSign_Changed(string ctrlId, string value, int inx)
        {
            if (value.Length == 0)
            {
                RenderSignPost(ctrlId, value);
                return;
            }

            var sqlParams = new Dictionary<string, object>
            {
                {"@КодЛицаРодителя", PersonId},
                {"@КодЛицаПотомка", value},
                {"@Параметр", inx},
                {"@КодТипаСвязиЛиц", 1},
                {"@Дата", DateTime.Today.ToString("yyyyMMdd")}
            };

            var dt = DBManager.GetData(SQLQueries.SELECT_ДолжностьФизическогоЛица,
                Config.DS_person, CommandType.Text, sqlParams);

            if (dt.Rows.Count != 1)
            {
                RenderSignPost(ctrlId, "");
                return;
            }

            RenderSignPost(ctrlId, dt.Rows[0][0].ToString());
        }

        private void DBSCurrency_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (dbsCurrency.Value.Length == 0) return;
            dbsStoreValS.Value = "";
            dbsStoreValS.TryFindSingleValue();
            DBSStore_RenderText(dbsStoreValS.Value, "textValS");
        }

        private void DBSStoreRS_Changed(object sender, ProperyChangedEventArgs e)
        {
            DBSStore_RenderText(dbsStoreRS.Value, "textRS");
        }

        private void DBSStoreSpec_Changed(object sender, ProperyChangedEventArgs e)
        {
            DBSStore_RenderText(dbsStoreSpec.Value, "textSpec");
        }

        private void DBSStoreValS_Changed(object sender, ProperyChangedEventArgs e)
        {
            DBSStoreValS_SetValue();
        }

        private void DBSStoreValS_SetValue()
        {
            DBSStore_RenderText(dbsStoreValS.Value, "textValS");
            if (dbsStoreValS.Value.Length <= 0 || dbsCurrency.Value.Length != 0) return;

            var sVal = new Lib.Entities.Stores.Store(dbsStoreValS.Value);
            if (sVal != null && !sVal.Unavailable)
            {
                dbsCurrency.Value = sVal.ResourceId.ToString();
                dbsCurrency.RefreshRequired = true;
            }
        }

        private void DBSStoreValS_BeforeSearch(object sender)
        {
            dbsStoreValS.Filter.StoreResourceId.Clear();
            if (dbsCurrency.Value.Length > 0)
                dbsStoreValS.Filter.StoreResourceId.Add(dbsCurrency.Value);
        }

        private void DBSStore_RenderText(string rsId, string ctrlId)
        {
            var sb = new StringBuilder();

            if (string.IsNullOrEmpty(rsId))
            {
                sb.Append("");
            }
            else
            {
                var s = new Lib.Entities.Stores.Store(rsId);
                if (s != null && !s.Unavailable)
                {
                    var bankName = "";
                    var bankAddress = "";
                    var bankKpp = "";

                    var bank = GetPerson(s.KeeperId, radioLang.Value, ref bankName, ref bankAddress, ref bankKpp);

                    sb.AppendFormat("{0}: ",
                        radioLang.Value == "en" ? "Bank" : Resx.GetString("STORE_SP_Lbl_Data_Bank"));

                    sb.Append(bankName);
                    sb.Append(" ");
                    sb.Append(bankAddress);

                    sb.Append(radioLang.Value == "en"
                        ? " SWIFT: " + bank.SWIFT
                        : $"{(bank.BIK.Length > 0 ? $" БИК: {bank.BIK}" : "")}{(bank.CorrAccount.Length > 0 ? $" К/С: {bank.CorrAccount}" : "")}");
                }
            }

            SetTextValue(sb.ToString(), ctrlId);
        }

        #endregion

        #region Manage

        /// <summary>
        ///     Управление контролом выбора директора
        /// </summary>
        private void Director_Manage()
        {
            if (!flagDirector.Checked)
            {
                dbsSign1.Value = "";
                RenderSignPost("divSign1", "");
            }
            else
            {
                dbsSign1.TryFindSingleValue();
                if (dbsSign1.Value.Length > 0)
                    DBSSign_Changed("divSign1", dbsSign1.Value, 1);

                dbsSign1.Focus();
            }

            dbsSign1.IsDisabled = !flagDirector.Checked;
            dbsSign1.IsRequired = flagDirector.Checked;
        }

        /// <summary>
        ///     Управление контролом выбора расчетного счета
        /// </summary>
        private void RS_Manage()
        {
            if (!flagRS.Checked)
            {
                dbsStoreRS.Value = "";
                SetTextValue("", "textRS");
            }
            else
            {
                if (dbsStoreRS.Value.Length > 0) flagDirector.Focus();
                else dbsStoreRS.Focus();
            }

            dbsStoreRS.IsDisabled = !flagRS.Checked;
            dbsStoreRS.IsRequired = flagRS.Checked;
        }

        /// <summary>
        ///     Управление контролом выбора спецсчета
        /// </summary>
        private void Spec_Manage()
        {
            if (!flagSpec.Checked)
            {
                dbsStoreSpec.Value = "";
                SetTextValue("", "textSpec");
            }
            else
            {
                if (dbsStoreSpec.Value.Length > 0) flagDirector.Focus();
                else dbsStoreSpec.Focus();
            }

            dbsStoreSpec.IsDisabled = !flagSpec.Checked;
            dbsStoreSpec.IsRequired = flagSpec.Checked;
        }

        /// <summary>
        ///     Управление контролом выбора валютных счетов
        /// </summary>
        private void ValS_Manage()
        {
            if (!flagValS.Checked)
            {
                dbsStoreValS.Value = "";
                dbsCurrency.Value = "";
                SetTextValue("", "textValS");
            }
            else
            {
                if (dbsCurrency.Value.Length > 0) flagDirector.Focus();
                else dbsCurrency.Focus();
            }

            dbsStoreValS.IsDisabled = !flagValS.Checked;
            dbsCurrency.IsDisabled = !flagValS.Checked;

            dbsStoreValS.IsRequired = flagValS.Checked;
            dbsCurrency.IsRequired = flagValS.Checked;
        }

        /// <summary>
        ///     Получение информации о лице по индетификатору
        /// </summary>
        /// <param name="pId">КодЛица</param>
        /// <param name="lang">На каком языке нужны данные</param>
        /// <param name="fullName">Возвращает краткое название лица</param>
        /// <param name="address">Возвращает адрес лица</param>
        /// <param name="kpp">Возвращает КПП</param>
        /// <returns>Новый объект лица</returns>
        private PersonOld GetPerson(int? pId, string lang, ref string fullName, ref string address, ref string kpp)
        {
            if (!pId.HasValue) return null;
            var p = new PersonOld(pId.Value.ToString());
            if (p == null || p.Unavailable) return null;

            if (p == null && p.Unavailable)
                throw new Exception(string.Format("Лицо с кодом #{0} не найдено или не доступно!", pId.Value));

            var crd = p.GetCard(DateTime.Today);

            if (crd == null)
                throw new Exception(
                    string.Format("У лица {0} нет действующих реквизитов на текущую дату!", p.Name));

            kpp = crd.КПП;

            if (lang == "en")
            {
                fullName = crd.NameLat;
                address = crd.АдресЮридическийЛат;
            }
            else
            {
                fullName = crd.NameRus;
                address = crd.АдресЮридический;
            }


            return p;
        }

        #endregion

        #region Render

        /// <summary>
        ///     Отрисовка верхней панели формы
        /// </summary>
        /// <returns>Разметка</returns>
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
        ///     Отрисовка назвния лица
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="title">Где отрисовываем(в заголовке страницы или нет)</param>
        protected void RenderCompanyName(TextWriter w, bool title = false)
        {
            if (PersonName == null) InitPerson();
            var name = radioLang.Value == "en" ? PersonNameEn : PersonName;
            w.Write("{0} ", Resx.GetString("STORE_SP_Lbl_Requisites"));
            if (!title)
                RenderLinkPerson(w, "person_" + PersonId, PersonId.ToString(), name);
            else
                w.Write(name);
        }

        /// <summary>
        ///     Отрисовка названия лица
        /// </summary>
        private void RenderTextName()
        {
            using (var w = new StringWriter())
            {
                w.Write(!flagName.Checked ? "" : radioLang.Value == "ru" ? PersonName : PersonNameEn);
                SetTextValue(w, "textName");
            }
        }

        /// <summary>
        ///     Отрисовка ИНН
        /// </summary>
        private void RenderTextInn()
        {
            using (var w = new StringWriter())
            {
                w.Write(!flagINN.Checked ? "" : PersonObj.INN);
                SetTextValue(w, "textINN");
            }
        }

        /// <summary>
        ///     Отрисовка OGRN
        /// </summary>
        private void RenderTextOGRN()
        {
            using (var w = new StringWriter())
            {
                w.Write(!flagOGRN.Checked ? "" : PersonObj.OGRN);
                SetTextValue(w, "textOGRN");
            }
        }

        /// <summary>
        ///     Отрисовка КПП
        /// </summary>
        private void RenderTextKpp()
        {
            using (var w = new StringWriter())
            {
                w.Write(!flagKPP.Checked ? "" : PersonKpp);
                SetTextValue(w, "textKPP");
            }
        }

        /// <summary>
        ///     Отрисовка адреса лица
        /// </summary>
        private void RenderTextAddress()
        {
            using (var w = new StringWriter())
            {
                w.Write(!flagAddress.Checked ? "" : radioLang.Value == "ru" ? PersonAddress : PersonAddressEn);
                SetTextValue(w, "textAddress");
            }
        }

        /// <summary>
        ///     Отрисовка должности
        /// </summary>
        /// <param name="ctrlId">Идентификатор контрола</param>
        /// <param name="value">Значение, которое нужно отрисовать</param>
        private void RenderSignPost(string ctrlId, string value)
        {
            JS.Write("var objDiv_{0}=document.getElementById('{0}Post'); if (objDiv_{0}) objDiv_{0}.innerHTML='{1}';",
                ctrlId, HttpUtility.JavaScriptStringEncode(string.Format("<span>{0}</span>", value)));
        }

        #endregion

        #region Adv functions

        /// <summary>
        ///     Получение уникального списка действующих счетов лица
        /// </summary>
        /// <returns></returns>
        private List<int> GetCurrencyValStore()
        {
            var sqlParams = new Dictionary<string, object>
                {{"@КодРаспорядителя", PersonId}, {"@ТипыСкладов", "2,3"}, {"@Дата", DateTime.Today}};
            var list = new List<int>();
            using (var dbReader = new DBReader(SQLQueries.SELECT_ВалютыВСчетахКомпании, CommandType.Text,
                Config.DS_person, sqlParams))
            {
                if (!dbReader.HasRows)
                    return list;
                var colКодРесурса = dbReader.GetOrdinal("КодРесурса");

                while (dbReader.Read()) list.Add(dbReader.GetInt32(colКодРесурса));
            }

            return list;
        }

        /// <summary>
        ///     Присвоение текста клиентскому контролу
        /// </summary>
        /// <param name="s">Текст, который необходимо вывести</param>
        /// <param name="ctrlId">Индентификатор контрола</param>
        private void SetTextValue(string s, string ctrlId)
        {
            using (var w = new StringWriter())
            {
                w.Write(s);
                SetTextValue(w, ctrlId);
            }
        }

        /// <summary>
        ///     Присвоение текста клиентскому контролу
        /// </summary>
        /// <param name="w">Поток вывода</param>
        /// <param name="ctrlId">Индентификатор контрола</param>
        private void SetTextValue(TextWriter w, string ctrlId)
        {
            JS.Write("var objT=document.getElementById('{0}'); if(objT) objT.innerHTML='{1}';", ctrlId,
                HttpUtility.JavaScriptStringEncode(w.ToString()));
        }


        /// <summary>
        ///     Проверка наличия у лица складов указанных типов(вызов sql-batch)
        /// </summary>
        /// <param name="typesStores">Список типов складов через ","</param>
        /// <returns>false/true</returns>
        private bool ExistsStores(string typesStores)
        {
            var sqlParams = new Dictionary<string, object>
                {{"@КодРаспорядителя", PersonId}, {"@ТипыСкладов", typesStores}, {"@Дата", DateTime.Today}};
            var objData = DBManager.ExecuteScalar(SQLQueries.SELECT_РаспорядительИмеетСкладыУказанныхТипов,
                CommandType.Text, Config.DS_person, sqlParams);

            return (int) objData == 1;
        }

        /// <summary>
        ///     Проверка наличия у лица подписантов с правом первой подписи
        /// </summary>
        /// <returns>false/true</returns>
        private bool ExistsFirstSign()
        {
            var sqlParams = new Dictionary<string, object>
                {{"@КодЛицаРодителя", PersonId}, {"@Параметр", 1}, {"@Дата", DateTime.Today}, {"@КодТипаСвязиЛиц", 1}};
            var objData = DBManager.ExecuteScalar(SQLQueries.SELECT_ЛицоИмеетПодписантовСПравомПодписи,
                CommandType.Text, Config.DS_person, sqlParams);

            return (int) objData == 1;
        }

        #endregion
    }
}