using System;
using System.Data;
using System.Text;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

using Kesco.Lib.DALC;
using Kesco.Lib.Log;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Docs;

using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.DBSelect.V4;
using Kesco.Lib.Web.Settings;
using Kesco.Lib.Web.Settings.Parameters;

using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Entities.Stores;

namespace Kesco.App.Web.Stores
{
    /// <summary>
    /// Класс объекта представляющего страницу поиска складов
    /// </summary>
    public partial class Search : EntityPage
    {
        //Класс соответствия условного идентификатора (сокращенное название, которое сохраняется в настройках) и поля на странице
        struct IdControl
        {
            public IdControl(string id, V4Control c)
            {
                predId = id;
                ctrl = c;
            }

            public string predId;
            public V4Control ctrl;
        }

        /// <summary>
        /// Класс объекта построителя запроса поиска складов
        /// Объект строит SQL запрос по частям для каждого элемента пользовательского интерфейса
        /// </summary>
        class QueryBuilder
        {
            //SQL логический оператор соединения условий
            string _where_operator = "AND";

            private QueryBuilder()
            {
            }

            public QueryBuilder(string where_operator)
            {
                _where_operator = where_operator;
                sqlParams = new Dictionary<string, object>();
            }

            //SQL параметры запроса
            public Dictionary<string, object> sqlParams { get; set; }

            //Построитель свойства SelectPart
            StringBuilder _selectPart = new StringBuilder();
            //Построитель свойства FromPart
            StringBuilder _fromPart = new StringBuilder();
            //Построитель свойства WherePart
            StringBuilder _wherePart = new StringBuilder();

            //SELECT часть запроса
            public string SelectPart
            {
                set
                {
                    _selectPart = new StringBuilder(value);
                }

                get
                {
                    return _selectPart.ToString();
                }
            }

            //FROM часть запроса
            public string FromPart
            {
                set
                {
                    _fromPart = new StringBuilder(value);
                }

                get
                {
                    return _fromPart.ToString();
                }
            }

            //WHERE часть запроса
            public string WherePart
            {
                set
                {
                    _wherePart = new StringBuilder(value);
                }

                get
                {
                    int start = _where_operator.Length + 2;//2 пробела
                    if (_wherePart.Length > start)
                        return _wherePart.ToString(start, _wherePart.Length - start);

                    return _wherePart.ToString();
                }
            }

            //Полный SQL текст запроса
            public string Text
            {
                get
                {
                    StringBuilder sbText = new StringBuilder("SELECT " + _selectPart.ToString() + " FROM " + _fromPart.ToString());

                    if(WherePart.Length > 0)
                        sbText.AppendFormat(" WHERE {0}", WherePart);

                    return sbText.ToString();
                }
            }

            /// <summary>
            /// Метод добавляет текстовое условие в часть WHERE запроса
            /// </summary>
            /// <param name="query">Текстовое условие</param>
            public void AddWhere(string query)
            {
                if (string.IsNullOrWhiteSpace(query)) return;

                _wherePart.AppendFormat(" {0} ({1})", _where_operator, query);
            }

            /// <summary>
            /// Метод добавляет условие из элемента управления TextBox в часть WHERE запроса
            /// </summary>
            /// <param name="t">Элемент TextBox</param>
            /// <param name="where_column">Имя колонки в части SELECТ соответствующей элементу</param>
            public string AddTextPartOfQuery(TextBox t, string where_column)
            {
                if (!t.HasCheckbox || t.Checked)
                {
                    string strPredicate = CreatePredicate(StoresPageHelper.txtModeOffset + t.ValueTextBoxEnum.ToInt(), where_column, t.Value);
                    if (null != strPredicate && strPredicate.Length > 0)
                        _wherePart.AppendFormat(" {0} ({1})", _where_operator, strPredicate);

                    return strPredicate;
                }

                return null;
            }

            /// <summary>
            /// Метод добавляет условие из элемента управления Select в часть WHERE запроса
            /// </summary>
            /// <param name="s">Элемент Select</param>
            /// <param name="select_column">Имя колонки в части SELECТ соответствующей элементу</param>
            /// <param name="where_column">Имя колонки в части WHERE соответствующей элементу</param>
            /// <param name="values_param">Имя параметра в части WHERE соответствующей элементу</param>
            /// <param name="join_type">Тип соединения с таблицей соответствующей элементу</param>
            /// <param name="join_on">Условие соединения с таблицей соответствующей элементу</param>
            /// <param name="base_table">Имя базовой таблицы соответствующей элементу</param>
            /// <param name="derived_table">Имя производной (созданной конструктором табличных значений) таблицы соответствующей элементу</param>
            public void AddCodesPartOfQuery(Select s, string select_column, string where_column, string values_param, string join_type, string join_on, string base_table, string derived_table)
            {
                SelectEnum se = SelectEnum.Any;

                bool fNoJoin = null == join_type || null == join_on || null == base_table || null == derived_table;

                if (!s.HasCheckbox || s.Checked)
                {
                    se = (SelectEnum)s.ValueSelectEnum.ToInt();

                    if (se != SelectEnum.Contain || fNoJoin)//Для Contain создается отдельная таблица для соединения
                    {
                        if (null != values_param)
                            sqlParams.Add(values_param, null == s.SelectedItemsString ? DBNull.Value : (object)s.SelectedItemsString);

                        string strPredicate = CreatePredicate(StoresPageHelper.selectModeOffset + s.ValueSelectEnum.ToInt(), where_column, values_param);
                        if (null != strPredicate && strPredicate.Length > 0)
                            _wherePart.AppendFormat(" {0} ({1})", _where_operator, strPredicate);
                    }
                }

                if (!fNoJoin)
                {
                    switch (se)
                    {
                        case SelectEnum.NoValue:
                            break;
                        case SelectEnum.Contain:
                            {
                                StringBuilder sbValues = new StringBuilder();
                                foreach (Kesco.Lib.Entities.Item i in s.SelectedItems)
                                {
                                    object o = i.Value;

                                    if (o is Kesco.Lib.Entities.Entity)
                                    {
                                        sbValues.AppendFormat("({0}, '{1}'),", i.Id, (o as Kesco.Lib.Entities.Entity).Name);
                                    }
                                    else
                                        if (o is Kesco.Lib.Entities.Item)
                                        {
                                            sbValues.AppendFormat("({0}, '{1}'),", i.Id, ((Kesco.Lib.Entities.Item)o).Value);
                                        }
                                        else
                                        {
                                            Type to = o.GetType();
                                            object oText = to.GetProperty(s.ValueField).GetValue(o, null);
                                            if (null != oText)
                                            {
                                                string strName = oText.ToString();
                                                sbValues.AppendFormat("({0}, '{1}'),", i.Id, strName);
                                            }
                                        }
                                }

                                if (sbValues.Length > 0)
                                {
                                    _fromPart.AppendFormat(" INNER JOIN (VALUES {0}) AS {1} ON {2}", sbValues.ToString(0, sbValues.Length - 1), derived_table, join_on);
                                    if (null != select_column) _selectPart.AppendFormat(",{0}", select_column);
                                }
                                else
                                    goto default;
                            }
                            break;
                        case SelectEnum.NotContain:
                            goto default;
                        case SelectEnum.Any:
                            goto default;
                        default:
                            _fromPart.Append(" "+join_type + " JOIN " + base_table + " ON " + join_on);
                            if (null != select_column) _selectPart.AppendFormat(",{0}", select_column);
                            break;
                    }
                }
            }

            /// <summary>
            /// Метод строит часть запроса WHERE для заданной колонки, условия и значения фильтра
            /// </summary>
            /// <param name="pred_type">Тип условия</param>
            /// <param name="column">Имя колонки</param>
            /// <param name="value">Значения фильтра</param>
            /// <returns>Сформированную строку-условие, которя моежет быть использвована в части WHERE запроса</returns>
            public static string CreatePredicate(int pred_type, string column, string value)
            {
                if (0 == pred_type) return null;
                if (pred_type > 10000) return null; //эти значения используются для пользовательских типов фильтрации

                Func<int, string, string> ResultFunc = (t, r) =>
                {
                    if (t % 2 == 0 && r.Length > 0) return "NOT (" + r + ")";
                    return r;
                };

                //условия фильтрациии для кодов перечисленных через запятую, value - это имя параметра
                if (pred_type > 0 && pred_type < 5)
                {

                    //Элементы из списка
                    //Элементы за исключением
                    if (pred_type < 3)
                    {
                        if (null != value && value.Trim().Length > 0)
                            return ResultFunc(pred_type, string.Format("{0} IN(SELECT value FROM Инвентаризация.dbo.fn_SplitInts({1}))", column, value));
                    }

                    //Указано какое-либо значение
                    //Значение не указано
                    if (pred_type < 5)
                        return ResultFunc(pred_type, string.Format("{0} IS NOT NULL", column));
                }

                //условия фильтрациии для текстовых полей, value - это строка из нескольких слов
                if (pred_type > 10 && pred_type < 25)
                {
                    //Слова разделяются следующими символами: пробел, неразрывный пробел, табуляция, перевод строки, возврат каретки,
                    //одинарная кавычка, двойная кавычка или символы из строки "_[]{}()/\%@&$:;!?". 
                    string clear_value = Regex.Replace(value, "[\xa0\t\n\r'`\\\"_\\[\\]{}()/\\\\%@&$:;!?]", " ");
                    clear_value = Regex.Replace(clear_value, "\\s+", " ");
                    clear_value = clear_value.Trim();

                    if (pred_type < 23 && clear_value.Length < 1) return null; //не допускаем появление условий типа LIKE '%%' или LIKE '%'

                    //Содержит все слова
                    //Не содержит какого-либо слова
                    if (pred_type < 13) return ResultFunc(pred_type, column + " LIKE '%" + clear_value.Replace(" ", "%' AND " + column + " LIKE '%") + "%'");

                    //Содержит все слова в заданном порялке
                    //Не содержит все слова в заданном порялке
                    if (pred_type < 15) return ResultFunc(pred_type, column + " LIKE '%" + clear_value.Replace(' ', '%') + "%'");

                    //Содержит какое-либо слово
                    //Не содержит ни одного слова
                    if (pred_type < 17) return ResultFunc(pred_type, column + " LIKE '%" + clear_value.Replace(" ", "%' OR " + column + " LIKE '%") + "%'");

                    //Слова начинаются
                    if (pred_type < 18) return ResultFunc(pred_type, "(' ' + " + column + ") LIKE '% " + clear_value.Replace(" ", "%' AND (' ' + " + column + ") LIKE '% ") + "%'");

                    //Слова не начинаются (к условию будет применен NOT)
                    if (pred_type < 19) return ResultFunc(pred_type, "(' ' + " + column + ") LIKE '% " + clear_value.Replace(" ", "%' OR (' ' + " + column + ") LIKE '% ") + "%'");

                    //Слова заканчиваются
                    if (pred_type < 20) return ResultFunc(pred_type, "(" + column + " + ' ') LIKE '%" + clear_value.Replace(" ", " %' AND (" + column + " + ' ') LIKE '%") + " %'");

                    //Слова не заканчиваются (к условию будет применен NOT)
                    if (pred_type < 21) return ResultFunc(pred_type, "(" + column + " + ' ') LIKE '%" + clear_value.Replace(" ", " %' OR (" + column + " + ' ') LIKE '%") + " %'");

                    //Полностью совпадает
                    //Полностью не совпадает
                    if (pred_type < 23) return ResultFunc(pred_type, column + "= '" + clear_value + "'");

                    //Пустая строка
                    //Не пустая строка
                    if (pred_type < 25) return ResultFunc(pred_type, column + "=''");
                }

                //условия фильтрациии для дат, value - это имя параметра
                if (pred_type > 50 && pred_type < 55)
                {
                    //После указанной даты
                    //До указанной даты
                    if (pred_type < 53) return ResultFunc(pred_type, column + ">" + value);

                    //Указана какая-либо дата
                    //Дата не указана
                    if (pred_type < 55) return ResultFunc(pred_type, column + " IS NOT NULL");
                }

                return null;
            }
        }

        //Вспомогательный объект для сохранения и восстановления размеров и положения окна
        private WndSizePosKeeper _SizePosKeeper;

        //Предварительно загруженный список допустимых типов складов, для быстрого извлечения допустимых ресурсов склада
        private List<StoreType> _storeTypesList = null;

        //Cтрока с перечислением кодов складов, которые не показываются в результирующей таблице, передается только через параметры
        //private string _storeExcept = null;

        //Строка указывающая вариант отображения условий поиска на странице установленный клиентом "1" - показывается только описание, иначе поля редактирования
        string _displayFilterDescription="0";

        //Строка с настройками отображения фильтров поиска
        string _displaySettings = null;

        //Идентификаторы полей поиска не отображаемые на странице
        private string[] _hiddenFields = null;

        //Таблица с результатами поиска
        private TemplatedSqlResult _table;

        //Массив соответствий условных идентификаторов и полей на странице
        IdControl[] _predIdCtrls;

        protected override string HelpUrl { get; set; }

        public Search()
        {
            HelpUrl = "hlp/help_search.htm";
        }

#region EventHandlers

        protected void Page_Load(object sender, EventArgs e)
        {
            if (V4IsPostBack) return;

            _predIdCtrls = new IdControl[14] {new IdControl(StoresPageHelper.predText, txtText),
                                                                new IdControl(StoresPageHelper.predStore, sStore),
                                                                new IdControl(StoresPageHelper.predType, sStoreType),
                                                                new IdControl(StoresPageHelper.predStorage, sStorage),
                                                                new IdControl(StoresPageHelper.predName, txtName),
                                                                new IdControl(StoresPageHelper.predIban, txtIBAN),
                                                                new IdControl(StoresPageHelper.predKeeper, sKeeperBank),
                                                                new IdControl(StoresPageHelper.predManager, sManager),
                                                                new IdControl(StoresPageHelper.predManagerDepartment, sManagerDepartment),
                                                                new IdControl(StoresPageHelper.predAgreement, sAgreement),
                                                                new IdControl(StoresPageHelper.predResource, sResource),
                                                                new IdControl(StoresPageHelper.predDepartment, txtStoreDepartment),
                                                                new IdControl(StoresPageHelper.predDescription, txtDescription),
                                                                new IdControl(StoresPageHelper.predQuery, sQuery)};

            StoresClientScripts.InitializeGlobalVariables(this);

            _table = new TemplatedSqlResult(this, "SearchResult", pageBar, "SearchResult.xslt", "", System.Data.CommandType.Text);

            dateValidPeriodFilter.FilterOptions = new string[] {Resx.GetString("STORE_ValidPeriodFilter"),
                                                                Resx.GetString("STORE_ValidPeriodFilterNotWorking"),
                                                                Resx.GetString("STORE_ValidPeriodFilterBegin"),
                                                                Resx.GetString("STORE_ValidPeriodFilterEnd"),
                                                                Resx.GetString("STORE_ValidFromToIsNull")
                                                                };
            /*
            queryFilter.FilterOptions = new string[] {Resx.GetString("STORE_queryFilterAnd"),
                                                      Resx.GetString("STORE_queryFilterOr")
                                                     };
            */

            _storeTypesList = StoreType.GetList();

            sAgreement.Filter.Type.Clear();

            sAgreement.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = Kesco.Lib.BaseExtention.Enums.Docs.DocTypeEnum.ДоговорХранения, QueryType = DocTypeQueryType.WithChildrenSynonyms });
            sAgreement.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = Kesco.Lib.BaseExtention.Enums.Docs.DocTypeEnum.ДоговорТранспортировки, QueryType = DocTypeQueryType.WithChildrenSynonyms });
            sAgreement.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = Kesco.Lib.BaseExtention.Enums.Docs.DocTypeEnum.ДоговорПодряда, QueryType = DocTypeQueryType.WithChildrenSynonyms });
            sAgreement.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = Kesco.Lib.BaseExtention.Enums.Docs.DocTypeEnum.ДоговорОказанияУслуг, QueryType = DocTypeQueryType.WithChildrenSynonyms });

            StoresPageHelper pageHelper = new StoresPageHelper(Request, ReturnId != "1" ? new AppParamsManager(this.ClId, StoresPageHelper.SearchParameterNamesCollection) : null);

            string checkedFields = null;

            if (!TryOldParameters(pageHelper))
            {
                bool isRequired = false;
                //_storeExcept = pageHelper.getParameterValue(StoresPageHelper.SearchParameters.Exceptions, out isRequired, string.Empty);

                string strSortData = pageHelper.getParameterValue(StoresPageHelper.SearchParameters.Sort, out isRequired, string.Empty);
                _table.SetSortMode(strSortData);

                checkedFields = pageHelper.getParameterValue(StoresPageHelper.SearchParameters.CheckedFields, out isRequired, string.Empty);

                pageHelper.setTextBoxParameterValue(txtText, StoresPageHelper.SearchParameters.TextMode, StoresPageHelper.SearchParameters.Text);

                bool isRequiredMode = false;//, isRequiredPeriod = false, isRequiredFrom = false, isRequiredTo = false;
                string val = pageHelper.getParameterValue(StoresPageHelper.SearchParameters.ValidMode, out isRequiredMode, string.Empty);
                dateValidPeriodFilter.Checked = isRequiredMode;

                if (val.ToInt() >= dateValidPeriodFilter.FilterOptions.Length) val = (dateValidPeriodFilter.FilterOptions.Length - 1).ToString();
                dateValidPeriodFilter.Value = val;
                //dateValidPeriodFilter.IsDisabled = isRequiredMode;

                /*
                dateValidPeriod.ValuePeriod = pageHelper.getParameterValue(StoresPageHelper.SearchParameters.ValidPeriod, out isRequiredPeriod, ((int)PeriodsEnum.Day).ToString());
                dateValidPeriod.ValueFrom = pageHelper.getParameterValue(StoresPageHelper.SearchParameters.ValidFrom, out isRequiredFrom, DateTime.Now.ToString("dd.MM.yyyy"));
                dateValidPeriod.ValueTo = pageHelper.getParameterValue(StoresPageHelper.SearchParameters.ValidTo, out isRequiredTo, dateValidPeriod.ValueFrom);
                if (!dateValidPeriod.IsDisabled)
                    dateValidPeriod.IsDisabled = isRequiredPeriod | isRequiredFrom | isRequiredTo;

                if (!dateValidPeriod.IsReadOnly)
                    dateValidPeriod.IsReadOnly = dateValidPeriod.IsDisabled;

                dateValidPeriodFilter.IsDisabled = dateValidPeriod.IsDisabled;

                if (dateValidPeriodFilter.IsDisabled)
                {
                    if (dateValidPeriod.ValueDateFrom > dateValidPeriod.ValueDateTo)
                        dateValidPeriod.ValueDateTo = dateValidPeriod.ValueDateFrom;
                }
                */

                dateValid.Value = pageHelper.getParameterValue(StoresPageHelper.SearchParameters.Valid, out isRequired, null);
                //if (!dateValid.IsDisabled)
                //    dateValid.IsDisabled = isRequired;

                if (!dateValid.IsReadOnly)
                    dateValid.IsReadOnly = isRequired;

                if (!dateValidPeriodFilter.Checked) dateValidPeriodFilter.Checked = isRequired;
                if (null == dateValid.Value) dateValid.Value = DateTime.Now.ToString("dd.MM.yyyy");

                if (!dateValidPeriodFilter.IsDisabled) dateValidPeriodFilter.IsDisabled = isRequired;

                pageHelper.setTextBoxParameterValue(txtName, StoresPageHelper.SearchParameters.NameMode, StoresPageHelper.SearchParameters.Name);
                pageHelper.setTextBoxParameterValue(txtIBAN, StoresPageHelper.SearchParameters.IbanMode, StoresPageHelper.SearchParameters.Iban);
                pageHelper.setTextBoxParameterValue(txtStoreDepartment, StoresPageHelper.SearchParameters.DepartmentMode, StoresPageHelper.SearchParameters.Department);
                pageHelper.setTextBoxParameterValue(txtDescription, StoresPageHelper.SearchParameters.DescriptionMode, StoresPageHelper.SearchParameters.Description);

                string storesExcept = pageHelper.getRequestParameterValue(StoresPageHelper.SearchParameters.Exceptions, out isRequired);
                if (string.IsNullOrWhiteSpace(storesExcept))
                {
                    pageHelper.setSelectCtrlParameterValue(sStore, StoresPageHelper.SearchParameters.StoresMode, StoresPageHelper.SearchParameters.Stores);
                }
                else
                {
                    pageHelper.setSelectCtrlValue(sStore, (StoresPageHelper.selectModeOffset+(int)SelectEnum.NotContain).ToString(), storesExcept, isRequired);
                }

                pageHelper.setSelectCtrlParameterValue(sStoreType, StoresPageHelper.SearchParameters.TypeMode, StoresPageHelper.SearchParameters.Type);
                pageHelper.setSelectCtrlParameterValue(sStorage, StoresPageHelper.SearchParameters.StorageMode, StoresPageHelper.SearchParameters.Storage);
                pageHelper.setSelectCtrlParameterValue(sKeeperBank, StoresPageHelper.SearchParameters.KeeperMode, StoresPageHelper.SearchParameters.Keeper);
                pageHelper.setSelectCtrlParameterValue(sManager, StoresPageHelper.SearchParameters.ManagerMode, StoresPageHelper.SearchParameters.Manager);
                pageHelper.setSelectCtrlParameterValue(sManagerDepartment, StoresPageHelper.SearchParameters.ManagerDepartmentMode, StoresPageHelper.SearchParameters.ManagerDepartment);
                pageHelper.setSelectCtrlParameterValue(sResource, StoresPageHelper.SearchParameters.ResourceMode, StoresPageHelper.SearchParameters.Resource);
                pageHelper.setSelectCtrlParameterValue(sAgreement, StoresPageHelper.SearchParameters.AgreementMode, StoresPageHelper.SearchParameters.Agreement);
                pageHelper.setSelectCtrlParameterValue(sQuery, null, StoresPageHelper.SearchParameters.Query);

                string strRowsPerPage = pageHelper.getParameterValue(StoresPageHelper.SearchParameters.ResultsPerPage, out isRequired, string.Empty);
                pageBar.RowsPerPage = strRowsPerPage.ToInt();
            }

            Display("editFilter", false);
            SetFilterControlDescription();

            //Восстановление выбранных полей
            string strCheckedFieldsArray="[]";
            RestoreCheckedFields(checkedFields, ref strCheckedFieldsArray);

            if (!dateValid.IsDisabled)
            {
                dateValid.IsDisabled = dateValidPeriodFilter.HasCheckbox && !dateValidPeriodFilter.Checked || dateValidPeriodFilter.Value.ToInt()>3;
            }

            //Восстановление полей поиска
            StringCollection fieldsParameterNamesCollection = new StringCollection() { StoresPageHelper.WindowParameters.Fields, StoresPageHelper.WindowParameters.Filter };
            StoresPageHelper pageHelper2 = new StoresPageHelper(Request, new AppParamsManager(this.ClId, fieldsParameterNamesCollection));
            bool isRequired2 = false;
            string strVisibleFields = pageHelper2.getParameterValue(StoresPageHelper.WindowParameters.Fields, out isRequired2, string.Empty);

            string requiredFields = "[]";
            string hiddenFields = pageHelper.GetFormHiddenFields(strVisibleFields, ref _hiddenFields, ref requiredFields);

            StoresClientScripts.RestoreSrchFormSettings(this, string.Format("{0}.concat({1})", requiredFields, strCheckedFieldsArray), hiddenFields);
            //поля восстановлены

            //Установка дополнительных обработчиков событий изменения фильтров поиска
            dateValidPeriodFilter.FilterChanged += OnChangedEventHandler;
            dateValidPeriodFilter.FilterChanged += OnFilterChangedEventHandler;
            dateValidPeriodFilter.CheckChanged += OnCheckChangedEventHandler<FilterGroupBox.CheckEventArgs>;

            foreach (V4Control c in new V4Control[] { dateValid, txtText, txtName, txtIBAN, txtStoreDepartment, txtDescription,
                                                      sStore, sStoreType, sStorage, sKeeperBank, sManager, sManagerDepartment, sResource, sAgreement, sQuery})
            {
                if (c is TextBox)
                {
                    TextBox tc = c as TextBox;
                    tc.CheckChanged += OnCheckChangedEventHandler<TextBox.CheckEventArgs>;
                }
                else
                    if (c is Select)
                    {
                        Select sc = c as Select;
                        sc.CheckChanged += OnCheckChangedEventHandler<Select.CheckEventArgs>;
                    }

                c.Changed += OnChangedEventHandler;
            }

            //Эту настройку решено не сохранять
            //Восстановление отображения текстового описания фильтра поиска
            //string strFilter = pageHelper2.getParameterValue(StoresPageHelper.WindowParameters.Filter, out isRequired2, null);

            //StoresClientScripts.DisplayFilterDescription(this, null != strFilter && strFilter == "1");

            StoresClientScripts.DisplayFilterDescription(this, false);

            SearchFilterBody.Value = FilterGetDescription();

            _SizePosKeeper = new WndSizePosKeeper(this, StoresPageHelper.WindowParameters.SrchLeft, StoresPageHelper.WindowParameters.SrchTop, StoresPageHelper.WindowParameters.SrchWidth, StoresPageHelper.WindowParameters.SrchHeight);
            _SizePosKeeper.OnLoad();

            pageBar.SetDisabled(true, true);
        }

        /// <summary>
        /// В этом обработчике устанавливается текстовое описание условий поиска
        /// и проверяется правильность установленных условий поиска
        /// снимаются флажки с полей в которых установлены неправильные условия поиска 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCheckChangedEventHandler<T>(object sender, T e)
        {
            SearchFilterBody.Value = FilterGetDescription();
            _table.Disable();

            //Флажки с других элементов снимаются тольки при установке флажка на object sender
            bool isChecked = false;
            if (e is Select.CheckEventArgs) isChecked = (e as Select.CheckEventArgs).Checked;
            else
                if (e is TextBox.CheckEventArgs) isChecked = (e as TextBox.CheckEventArgs).Checked;
                else
                    if (e is FilterGroupBox.CheckEventArgs) isChecked = (e as FilterGroupBox.CheckEventArgs).Checked;

            if (sender.Equals(dateValidPeriodFilter))
            {
                dateValid.IsDisabled = !dateValidPeriodFilter.Checked || dateValidPeriodFilter.Value.ToInt()>3;
                if (!dateValid.IsDisabled)
                {
                    dateValid.Focus();
                }

                StoresClientScripts.DisplaySrchFormField(this, StoresPageHelper.predValid, !dateValidPeriodFilter.HasCheckbox || dateValidPeriodFilter.Checked);
            }

            if (isChecked && dateValidPeriodFilter.Checked)
            {
                if (!sender.Equals(dateValidPeriodFilter)
                    && !sender.Equals(dateValid)
                    && string.IsNullOrWhiteSpace(dateValid.Value)
                    && dateValidPeriodFilter.Value.ToInt()<4) dateValidPeriodFilter.Checked = false;
            }

            for (int i = 0; i < _predIdCtrls.Length; i++)
            {
                bool bChecked = false;
                V4Control c = _predIdCtrls[i].ctrl;

                if (c is TextBox)
                {
                    TextBox tc = c as TextBox;
                    bChecked = !tc.HasCheckbox || tc.Checked;

                    if (isChecked && bChecked && !c.Equals(sender)) TestTextBoxCheck(tc);
                }
                else
                    if (c is Select)
                    {
                        Select sc = c as Select;
                        bChecked = !sc.HasCheckbox || sc.Checked;

                        if (isChecked && bChecked && !c.Equals(sender)) TestSelectCheck(sc);
                    }

                if (c.Equals(sender))
                {
                    StoresClientScripts.DisplaySrchFormField(this, _predIdCtrls[i].predId, bChecked);
                }
            }
        }

        protected void OnFilterChangedEventHandler(object sender, FilterGroupBox.FilterEventArgs e)
        {
            dateValid.IsDisabled = e.Filter.HasValue && e.Filter>3;
        }

        /// <summary>
        /// В этом обработчике устанавливается текстовое описание условий поиска
        /// и проверяется правильность установленных условий поиска
        /// снимаются флажки с полей в которых установлены неправильные условия поиска 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnChangedEventHandler(object sender, EventArgs e)
        {
            //Может изменяться только условие фильтра
            //if (e.NewValue != e.OldValue)
            {
                SearchFilterBody.Value = FilterGetDescription();
                _table.Disable();
            }

            if (dateValidPeriodFilter.Checked)
            {
                if (!sender.Equals(dateValidPeriodFilter)
                    && !sender.Equals(dateValid)
                    && string.IsNullOrWhiteSpace(dateValid.Value)
                    && dateValidPeriodFilter.Value!="4") dateValidPeriodFilter.Checked = false;
            }

            for (int i = 0; i < _predIdCtrls.Length; i++)
            {
                V4Control c = _predIdCtrls[i].ctrl;

                if (c.Equals(sender)) continue;

                if (c is TextBox)
                {
                    TextBox tc = c as TextBox;
                    if (!tc.HasCheckbox || tc.Checked) TestTextBoxCheck(tc);
                }
                else
                    if (c is Select)
                    {
                        Select sc = c as Select;
                        if (!sc.HasCheckbox || sc.Checked) TestSelectCheck(sc);
                    }
            }
        }

        protected void OnResourceBeforeSearch(object sender)
        {
            if ( (sStoreType.HasCheckbox && !sStoreType.Checked) || string.IsNullOrWhiteSpace(sStoreType.SelectedItemsString) || sStoreType.ValueSelectEnum.ToInt() != (int)SelectEnum.Contain)
                sResource.Filter.AllChildrenWithParentIDs.Clear();
            else
            {
                string[] strTypes = sStoreType.SelectedItemsString.Split(',');

                StringBuilder sb = new StringBuilder();
                foreach (string type in strTypes)
                {
                    StoreType st = _storeTypesList.Find(t => { return t.Id == type; });
                    if (sb.Length > 0) sb.Append(',');
                    sb.Append(st.RootSource);
                }

                sResource.Filter.AllChildrenWithParentIDs.Value = sb.ToString();
            }
        }

        protected void OnManagerDepartmentBeforeSearch(object sender)
        {
            if ((sManager.HasCheckbox && !sManager.Checked) || string.IsNullOrWhiteSpace(sManager.SelectedItemsString) || sManager.ValueSelectEnum.ToInt() != (int)SelectEnum.Contain)
                sManagerDepartment.GetFilter().PcId.Clear();
            else
            {
                sManagerDepartment.GetFilter().PcId.Value = sManager.SelectedItemsString;
                sManagerDepartment.GetFilter().PcId.CompanyHowSearch = "0";
            }
        }

        protected void OnAgreementBeforeSearch(object sender)
        {
            StringBuilder sb = new StringBuilder();

            if ((!sManager.HasCheckbox || sManager.Checked) && !string.IsNullOrWhiteSpace(sManager.SelectedItemsString) && sManager.ValueSelectEnum.ToInt() == (int)SelectEnum.Contain)
            {
                sb.Append(sManager.SelectedItemsString);
            }

            if ((!sKeeperBank.HasCheckbox || sKeeperBank.Checked) && !string.IsNullOrWhiteSpace(sKeeperBank.SelectedItemsString) && sKeeperBank.ValueSelectEnum.ToInt() == (int)SelectEnum.Contain)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(sKeeperBank.SelectedItemsString);
            }

            if (sb.Length > 0)
                sAgreement.Filter.PersonIDs.Value = sb.ToString();
            else
                sAgreement.Filter.PersonIDs.Clear();
        }

        protected void OnResultCurrentPageChanged(object sender, EventArgs args)
        {
            _table.Update();
            RestoreCursor();
        }

        protected void OnResultRowsPerPageChanged(object sender, EventArgs args)
        {
            if (!pageBar.Disabled)
            {
                pageBar.CurrentPageNumber = 1;
                _table.Update();
            }

            RestoreCursor();
        }

#endregion

        /// <summary>
        /// Метод пытается загрузить сохраненные параметры приложения Склады предыдущих версии
        /// </summary>
        /// <param name="pageHelper">Вспомогательный объект</param>
        /// <returns>True исли установлены настройки от предыдущих версий приложения Склады</returns>
        private bool TryOldParameters(StoresPageHelper pageHelper)
        {
            bool isRequiredStoreExcept = false;
            string valueStoreExcept = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Except, out isRequiredStoreExcept);//склады не включаемые в поиск
            bool isRequiredSearch = false;
            string valueSearch = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Search, out isRequiredSearch);//строка поиска в разных полях
            bool isRequiredStoreHowSearch = false;
            string valueStoreHowSearch = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.HowSearch, out isRequiredStoreHowSearch);//1 - поле содержит текст, 0 - поле начинается с текста
            bool isRequiredStoreType = false;
            string valueStoreType = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Type, out isRequiredStoreType);//тип склада
            bool isRequiredStoreActual = false;
            string valueStoreActual = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Actual, out isRequiredStoreActual);//склад 1-действующий, 0 - недействующий
            bool isRequiredStoreNoName = false;
            string valueStoreNoName = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.NoName, out isRequiredStoreNoName);//Использовать имя склада при поиске
            bool isRequiredStoreSize = false;
            string valueStoreSize = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Size, out isRequiredStoreSize);//количество результатов на одну страницу
            bool isRequiredStoreResidence = false;
            string valueStoreResidence = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Residence, out isRequiredStoreResidence);//место хранения
            bool isRequiredStoreResource = false;
            string valueStoreResource = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Resource, out isRequiredStoreResource);//хранимый ресурс
            bool isRequiredStoreKeeper = false;
            string valueStoreKeeper = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Keeper, out isRequiredStoreKeeper);//хранитель
            bool isRequiredStoreManager = false;
            string valueStoreManager = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Manager, out isRequiredStoreManager);//распорядитель
            bool isRequiredStoreContract = false;
            string valueStoreContract = pageHelper.getRequestParameterValue(StoresPageHelper.OldParameters.Contract, out isRequiredStoreContract);//договор хранения

            bool isOldParameters = null != valueStoreExcept
                                    || null != valueSearch
                                    || null != valueStoreHowSearch
                                    || null != valueStoreType
                                    || null != valueStoreActual
                                    || null != valueStoreNoName
                                    || null != valueStoreSize
                                    || null != valueStoreResidence
                                    || null != valueStoreResource
                                    || null != valueStoreKeeper
                                    || null != valueStoreManager
                                    || null != valueStoreContract;

            if (isOldParameters)
            {
                if (null != valueStoreExcept)
                    pageHelper.setSelectCtrlValue(sStore, ((int)SelectEnum.NotContain).ToString(), valueStoreExcept, isRequiredStoreExcept);

                //_storeExcept = valueStoreExcept;

                string defaultTextBoxMode = ((int)TextBoxEnum.ContainsAll).ToString();

                if (null == valueStoreHowSearch)
                    valueStoreHowSearch = defaultTextBoxMode;
                else
                    if ("0" == valueStoreHowSearch)
                        valueStoreHowSearch = ((int)TextBoxEnum.Starts).ToString();
                    else
                        valueStoreHowSearch = ((int)TextBoxEnum.ContainsAny).ToString();

                pageHelper.setTextBoxValue(txtText, valueStoreHowSearch, valueSearch, isRequiredSearch);

                if ("1" == valueStoreNoName)
                    pageHelper.setTextBoxValue(txtName, ((int)StoreNameTextBox.TextBoxEnum.AccountUnknown).ToString(), StoreNameTextBox.NoNameValue, isRequiredStoreNoName);

                if (null != valueStoreType)
                    pageHelper.setSelectCtrlValue(sStoreType, ((int)SelectEnum.Contain).ToString(), valueStoreType, isRequiredStoreType);

                if (null != valueStoreActual)
                {
                    /*
                    dateValidPeriodFilter.Value = valueStoreActual == "1" ? "0" : "1";//dateValidPeriodFilter.FilterOptions
                    dateValidPeriod.ValuePeriod = ((int)PeriodsEnum.Day).ToString();
                    dateValidPeriod.ValueFrom = DateTime.Now.ToString("dd.MM.yyyy");
                    dateValidPeriod.ValueTo = dateValidPeriod.ValueFrom;
                    if (!dateValidPeriod.IsDisabled)
                        dateValidPeriod.IsDisabled = isRequiredStoreActual;

                    if (!dateValidPeriod.IsReadOnly)
                        dateValidPeriod.IsReadOnly = dateValidPeriod.IsDisabled;

                    dateValidPeriodFilter.IsDisabled = dateValidPeriod.IsDisabled;
                    */

                    dateValidPeriodFilter.Value = valueStoreActual == "1" ? "0" : "1";//dateValidPeriodFilter.FilterOptions
                    dateValid.ValueDate = DateTime.Now;
                    //if (!dateValid.IsDisabled)
                    //    dateValid.IsDisabled = isRequiredStoreActual;

                    if (!dateValid.IsReadOnly)
                        dateValid.IsReadOnly = isRequiredStoreActual;

                    dateValidPeriodFilter.IsDisabled = isRequiredStoreActual;

                    dateValidPeriodFilter.Checked = isRequiredStoreActual;
                }

                if (null != valueStoreResidence)
                    pageHelper.setSelectCtrlValue(sStorage, ((int)SelectEnum.Contain).ToString(), valueStoreResidence, isRequiredStoreResidence);

                if (null != valueStoreKeeper)
                    pageHelper.setSelectCtrlValue(sKeeperBank, ((int)SelectEnum.Contain).ToString(), valueStoreKeeper, isRequiredStoreKeeper);

                if (null != valueStoreManager)
                    pageHelper.setSelectCtrlValue(sManager, ((int)SelectEnum.Contain).ToString(), valueStoreManager, isRequiredStoreManager);

                if (null != valueStoreResource)
                    pageHelper.setSelectCtrlValue(sResource, ((int)SelectEnum.Contain).ToString(), valueStoreResource, isRequiredStoreResource);

                if (null != valueStoreContract)
                    pageHelper.setSelectCtrlValue(sAgreement, ((int)SelectEnum.Contain).ToString(), valueStoreContract, isRequiredStoreContract);

                if (null != valueStoreSize)
                    pageBar.RowsPerPage = valueStoreSize.ToInt();
            }

            return isOldParameters;
        }

        /// <summary>
        /// Метод осуществляет сохранение последних значений полей страницы в БД настроек пользователей
        /// </summary>
        private void SavePageParameters()
        {
            if (ReturnId == "1") return;

            AppParamsManager parametersManager = new AppParamsManager(this.ClId, new StringCollection());

            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.TextMode, (txtText.ValueTextBoxEnum.ToInt() + StoresPageHelper.txtModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Text, null == txtText.Value ? string.Empty : txtText.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.StoresMode, (sStore.ValueSelectEnum.ToInt() + StoresPageHelper.selectModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Stores, null == sStore.SelectedItemsString ? string.Empty : sStore.SelectedItemsString, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.ValidMode, dateValidPeriodFilter.Value, AppParamType.SavedWithClid));
            //parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.ValidPeriod, dateValidPeriod.ValuePeriod, AppParamType.SavedWithClid));
            //parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.ValidFrom, dateValidPeriod.ValueFrom, AppParamType.SavedWithClid));
            //parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.ValidTo, dateValidPeriod.ValueTo, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Valid, dateValid.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.TypeMode, (sStoreType.ValueSelectEnum.ToInt() + StoresPageHelper.selectModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Type, null == sStoreType.SelectedItemsString ? string.Empty : sStoreType.SelectedItemsString, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.StorageMode, (sStorage.ValueSelectEnum.ToInt() + StoresPageHelper.selectModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Storage, null == sStorage.SelectedItemsString ? string.Empty : sStorage.SelectedItemsString, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.NameMode, (txtName.ValueTextBoxEnum.ToInt() + StoresPageHelper.txtModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Name, null == txtName.Value ? string.Empty : txtName.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.IbanMode, (txtIBAN.ValueTextBoxEnum.ToInt() + StoresPageHelper.txtModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Iban, null == txtIBAN.Value ? string.Empty : txtIBAN.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.KeeperMode, (sKeeperBank.ValueSelectEnum.ToInt() + StoresPageHelper.selectModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Keeper, null == sKeeperBank.SelectedItemsString ? string.Empty : sKeeperBank.SelectedItemsString, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.ManagerMode, (sManager.ValueSelectEnum.ToInt() + StoresPageHelper.selectModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Manager, null == sManager.SelectedItemsString ? string.Empty : sManager.SelectedItemsString, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.ManagerDepartmentMode, (sManagerDepartment.ValueSelectEnum.ToInt() + StoresPageHelper.selectModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.ManagerDepartment, null == sManagerDepartment.SelectedItemsString ? string.Empty : sManagerDepartment.SelectedItemsString, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.ResourceMode, (sResource.ValueSelectEnum.ToInt() + StoresPageHelper.selectModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Resource, null == sResource.SelectedItemsString ? string.Empty : sResource.SelectedItemsString, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.AgreementMode, (sAgreement.ValueSelectEnum.ToInt() + StoresPageHelper.selectModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Agreement, null == sAgreement.SelectedItemsString ? string.Empty : sAgreement.SelectedItemsString, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.DepartmentMode, (txtStoreDepartment.ValueTextBoxEnum.ToInt() + StoresPageHelper.txtModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Department, null == txtStoreDepartment.Value ? string.Empty : txtStoreDepartment.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.DescriptionMode, (txtDescription.ValueTextBoxEnum.ToInt() + StoresPageHelper.txtModeOffset).ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Description, null == txtDescription.Value ? string.Empty : txtDescription.Value, AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Query, null == sQuery.SelectedItemsString ? string.Empty : sQuery.SelectedItemsString, AppParamType.SavedWithClid));

            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.ResultsPerPage, pageBar.RowsPerPage.ToString(), AppParamType.SavedWithClid));
            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.Sort, _table.GetSortMode(4), AppParamType.SavedWithClid));

            parametersManager.Params.Add(new AppParameter(StoresPageHelper.SearchParameters.CheckedFields, GetCheckedFields(), AppParamType.SavedWithClid));

            parametersManager.SaveParams();
        }

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
            Button btnSearch = new Button
            {
                ID = "btnSearch",
                V4Page = this,
                Text = Resx.GetString("lRefresh"),
                Title = Resx.GetString("lRefresh"),
                //Style = "BACKGROUND: buttonface url(/Styles/Search.gif) no-repeat left center;",
                Width = 105,
                OnClick = "SearchStore();"
            };

            Button btnClear = new Button
            {
                ID = "btnClear",
                V4Page = this,
                Text = Resx.GetString("lClear"),
                Title = Resx.GetString("lClear"),
                //Style = "BACKGROUND: buttonface url(/Styles/Delete.gif) no-repeat left center;",
                Width = 105,
                OnClick = "ClearSearchForm();"
            };

            Button btnNew = new Button
            {
                ID = "btnNew",
                V4Page = this,
                Text = Resx.GetString("STORE_CreateStore"),
                Title = Resx.GetString("STORE_CreateStore"),
                //Style = "BACKGROUND: buttonface url(/Styles/Store.gif) no-repeat left center;",
                Width = 165,
                OnClick = "cmd('cmd', 'NewButton');"
            };

            Button btnSettings = new Button
            {
                ID = "btnSetup",
                V4Page = this,
                Text = Resx.GetString("STORE_PageSettings"),
                Title = Resx.GetString("STORE_PageSettings"),
                //Style = "BACKGROUND: buttonface url(/Styles/Tools.gif) no-repeat left center;",
                Width = 105,
                OnClick = "SetupForm();"
            };

            Button btnCancel = new Button
            {
                ID = "btnCancel",
                V4Page = this,
                Text = Resx.GetString("cmdClose"),
                Title = Resx.GetString("cmdClose"),
                //Style = "BACKGROUND: buttonface url(/Styles/Cancel.gif) no-repeat left center;",
                IconJQueryUI = "v4_buttonIcons.Cancel",
                Width = 105,
                //Нажатие кнопки приведет к закрытию окна методом window.close();
                OnClick = "cmd('cmd', 'CancelButton');"
            };

            Button[] buttons = new Button[] { btnSearch, btnClear, btnNew, btnSettings, btnCancel };

            AddMenuButton(buttons);
        }

        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            _SizePosKeeper.ProcessCommand(cmd, param);

            switch (cmd)
            {
                case "CancelButton":
                    _SizePosKeeper.ProcessCommand("PageClose", null);
                    Close();
                    break;

                case "SearchButton":
                    FilterApply();
                    //SavePageParameters();
                    break;

                case "ClearButton":
                    ClearForm();
                    break;

                case "NewButton":
                    CreateNewStore();
                    break;

                case "SortColumn":
                    _table.SortResultColumn(param["sort_column"]);
                    RestoreCursor();
                    break;

                case "SaveSrchPageSettings":
                    {
                        _displayFilterDescription = param["display"];
                        string strSettings = param["settings"];
                        if (null != strSettings)
                            _displaySettings = strSettings;
                    }
                    break;

                case "PageClose":
                    SaveSrchPageSettings(_displaySettings, _displayFilterDescription);
                    SavePageParameters();
                    goto default;

                default:
                    base.ProcessCommand(cmd, param);
                    break;
            }
        }

        /// <summary>
        /// Метод для сохранения настроек полей страницы Поиск складов
        /// </summary>
        /// <param name="strSettings">Строка настроек полей страницы</param>
        private void SaveSrchPageSettings(string strSettings, string strDisplay)
        {
            AppParamsManager parametersManager = new AppParamsManager(this.ClId, new StringCollection());
            if (null!=strSettings)
                parametersManager.Params.Add(new AppParameter(StoresPageHelper.WindowParameters.Fields, strSettings, AppParamType.SavedWithClid));

            //Эту настройку решено не сохранять
            //parametersManager.Params.Add(new AppParameter(StoresPageHelper.WindowParameters.Filter, strDisplay, AppParamType.SavedWithClid));
            parametersManager.SaveParams();

            if (null != strSettings)
            {
                List<string> visible = new List<string>(strSettings.Split(new[] { ' ', ',' }));

                StoresPageHelper.GetStrHiddenFields(visible, ref _hiddenFields);
            }

            //SearchFilterBody.Value = FilterGetDescription();
        }

        /// <summary>
        /// Процедура очистки установленного фильтра и таблицы с данными
        /// </summary>
        private void ClearForm()
        {
            dateValidPeriodFilter.Value = "0";
            dateValidPeriodFilter.SetPropertyChanged("Value");
            dateValidPeriodFilter.Checked = false;

            /*
            dateValidPeriod.ValuePeriod = ((int)PeriodsEnum.Day).ToString();
            dateValidPeriod.SetPeriod(dateValidPeriod.ValuePeriod);

            dateValidPeriod.ValueDateFrom = DateTime.Now;
            dateValidPeriod.ValueDateTo = dateValidPeriod.ValueDateFrom;
            */

            dateValid.ValueDate = DateTime.Now;

            for (int i = 0; i < _predIdCtrls.Length; i++)
            {
                V4Control c = _predIdCtrls[i].ctrl;

                if (c is TextBox)
                {
                    TextBox t = c as TextBox;
                    t.Value = string.Empty;
                    t.ValueTextBoxEnum = ((int)TextBoxEnum.ContainsAll).ToString();
                    t.SetPropertyChanged("ListChanged");
                    t.Checked = false;
                }
                else
                    if (c is Select)
                    {
                        Select s = c as Select;
                        s.Value = null;
                        s.ClearSelectedItems();
                        s.ValueSelectEnum = ((int)SelectEnum.Contain).ToString();
                        s.SetPropertyChanged("ListChanged");
                        s.Checked = false;
                    }
            }

            SearchFilterBody.Value = FilterGetDescription();
            StoresClientScripts.SendSetInnerHtml(this, "SearchResult", string.Empty);

            pageBar.SetDisabled(true, true);
        }

        /// <summary>
        /// Метод формирует строку javascript функции вызова страницы создания нового склада
        /// </summary>
        private void CreateNewStore()
        {
            StringBuilder sb = new StringBuilder("&id=0");
            //if (!string.IsNullOrWhiteSpace(dateValidPeriod.ValueFrom))
            //    sb.AppendFormat("&_{0}={1}", StoresPageHelper.StoreParameters.ValidFrom, HttpUtility.UrlEncode(dateValidPeriod.ValueFrom));

            //if (!string.IsNullOrWhiteSpace(dateValidPeriod.ValueTo))
            //    sb.AppendFormat("&_{0}={1}", StoresPageHelper.StoreParameters.ValidTo, HttpUtility.UrlEncode(dateValidPeriod.ValueTo));

            DBSelect[] s = new DBSelect[] { sStoreType, sStorage, sKeeperBank, sManager, sManagerDepartment, sResource, sAgreement };
            string[] p = new string[] { StoresPageHelper.StoreParameters.Type, StoresPageHelper.StoreParameters.Storage, StoresPageHelper.StoreParameters.Keeper, StoresPageHelper.StoreParameters.Manager,
                                        StoresPageHelper.StoreParameters.ManagerDepartment, StoresPageHelper.StoreParameters.Resource, StoresPageHelper.StoreParameters.Agreement};

            System.Diagnostics.Debug.Assert(s.Length==p.Length);

            for (int i = 0; i < s.Length; i++)
            {
                if ((!s[i].HasCheckbox || s[i].Checked) && s[i].SelectedItems.Count == 1 && s[i].ValueSelectEnum.ToInt() == (int)SelectEnum.Contain)
                {
                    if (!string.IsNullOrWhiteSpace(s[i].SelectedItemsString))
                        sb.AppendFormat("&_{0}={1}", p[i], HttpUtility.UrlEncode(s[i].SelectedItemsString));
                }
            }

            if (!txtName.HasCheckbox || txtName.Checked)
            {
                if (!string.IsNullOrWhiteSpace(txtName.Value))
                    sb.AppendFormat("&_{0}={1}", StoresPageHelper.StoreParameters.Name, HttpUtility.UrlEncode(txtName.Value));

                if (txtName.ValueTextBoxEnum == ((int)StoreNameTextBox.TextBoxEnum.AccountUnknown).ToString())
                    sb.AppendFormat("&_{0}={1}", StoresPageHelper.StoreParameters.AccountNumberUnknown, "1");
            }

            if ((!txtIBAN.HasCheckbox || txtIBAN.Checked) && !string.IsNullOrWhiteSpace(txtIBAN.Value))
                sb.AppendFormat("&_{0}={1}", StoresPageHelper.StoreParameters.Iban, HttpUtility.UrlEncode(txtIBAN.Value));

            if ((!txtStoreDepartment.HasCheckbox || txtStoreDepartment.Checked) && !string.IsNullOrWhiteSpace(txtStoreDepartment.Value))
                sb.AppendFormat("&_{0}={1}", StoresPageHelper.StoreParameters.Department, HttpUtility.UrlEncode(txtStoreDepartment.Value));

            if ((!txtDescription.HasCheckbox || txtDescription.Checked) && !string.IsNullOrWhiteSpace(txtDescription.Value))
                sb.AppendFormat("&_{0}={1}", StoresPageHelper.StoreParameters.Description, HttpUtility.UrlEncode(txtDescription.Value));

            StoresClientScripts.CreateNewStore(this, sb.ToString());
        }

        /// <summary>
        /// Метод устанавливает значения поле Description для всех элементов, чтобы использовать его при формировании описания фильтра поиска
        /// </summary>
        void SetFilterControlDescription()
        {
            /*
            txtText.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_Text"));
            sStoreType.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_StoreType"));
            sManagerDepartment.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_ManagerDepartment"));
            sStorage.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_Residence"));
            sAgreement.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_Agreement"));
            txtName.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_Name") + '/' + Resx.GetString("STORE_Account"));
            sResource.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_Resource") + '/' + Resx.GetString("STORE_Currency"));
            txtIBAN.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_IBAN"));
            txtStoreDepartment.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_Department") + '/' + Resx.GetString("STORE_Branch"));
            sKeeperBank.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_Keeper") + '/' + Resx.GetString("STORE_Bank"));
            txtDescription.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_Description"));
            sManager.Description = string.Format("<b>{0}</b>", Resx.GetString("STORE_Manager"));
            */

            txtText.Description = Resx.GetString("STORE_Text");
            sStore.Description = Resx.GetString("STORE_Store");
            sStoreType.Description = Resx.GetString("STORE_StoreType");
            sManagerDepartment.Description = Resx.GetString("STORE_ManagerDepartment");
            sStorage.Description = Resx.GetString("STORE_Residence");
            sAgreement.Description = Resx.GetString("STORE_Agreement");
            txtName.Description = Resx.GetString("STORE_Name") + '/' + Resx.GetString("STORE_Account");
            sResource.Description = Resx.GetString("STORE_Resource") + '/' + Resx.GetString("STORE_Currency");
            txtIBAN.Description = Resx.GetString("STORE_IBAN");
            txtStoreDepartment.Description = Resx.GetString("STORE_Department") + '/' + Resx.GetString("STORE_Branch");
            sKeeperBank.Description = Resx.GetString("STORE_Keeper") + '/' + Resx.GetString("STORE_Bank");
            txtDescription.Description = Resx.GetString("STORE_Description");
            sManager.Description = Resx.GetString("STORE_Manager");
            sQuery.Description = Resx.GetString("STORE_Queries");
        }

        bool IsFieldHidden(string predId)
        {
            if (null == _hiddenFields) return false;
            return Array.IndexOf(_hiddenFields, predId) >= 0;
        }

        /// <summary>
        /// Метод формирует текстовое описание установленного фильтра поиска и осуществляет HTML форматирование
        /// </summary>
        /// <returns>Строка описания в формате HTML</returns>
        string FilterGetDescription()
        {
            StringBuilder Description = new StringBuilder();

            if (!txtText.HasCheckbox || txtText.Checked)
            {
                string textFilter = txtText.GetFilterClauseText();
                if (!string.IsNullOrEmpty(textFilter))
                    Description.Append(textFilter);
            }

            //Описание фильтра поиска для периода действия склада
            ///if (!IsFieldHidden(StoresPageHelper.predValid) && (!string.IsNullOrEmpty(dateValidPeriod.ValueFrom) || !string.IsNullOrEmpty(dateValidPeriod.ValueTo)))
            if ((!dateValidPeriodFilter.HasCheckbox || dateValidPeriodFilter.Checked) /*&& !IsFieldHidden(StoresPageHelper.predValid)*/)
            {
                string periodDescription = Resx.GetString("STORE_IsActual");

                int periodFilterIndex = dateValidPeriodFilter.Value.ToInt();
                if(periodFilterIndex<0) periodFilterIndex = 0;
                if (periodFilterIndex >= dateValidPeriodFilter.FilterOptions.Length) periodFilterIndex = dateValidPeriodFilter.FilterOptions.Length-1;

                string strValidStr = null;

                if (periodFilterIndex==4)
                {
                    strValidStr = Resx.GetString("STORE_ValidFromToIsNullDescription").ToLower();
                }
                else
                    if (!string.IsNullOrWhiteSpace(dateValid.Value))
                    {
                        switch (periodFilterIndex)
                        {
                            case 0:
                                strValidStr = string.Format(Resx.GetString("STORE_Valid"), dateValid.Value).ToLower();
                                break;
                            case 1:
                                strValidStr = string.Format(Resx.GetString("STORE_NotValid"), dateValid.Value).ToLower();
                                break;
                            case 2:
                                strValidStr = Resx.GetString("STORE_NotValidBefore").ToLower() + " " + dateValid.Value;
                                break;
                            case 3:
                                strValidStr = Resx.GetString("STORE_NotValidAfter").ToLower() + " " + dateValid.Value;
                                break;
                        }
                 }

                if (null != strValidStr)
                {
                    if (Description.Length > 0) Description.Append("<br/>");

                    Description.AppendFormat("{0}: {1}", Resx.GetString("STORE_IsActual"), strValidStr);
                }
            }

            //txtText пропущен, он установлен самым первым
            for (int i = 1; i < _predIdCtrls.Length; i++)
            {
                V4Control c = _predIdCtrls[i].ctrl;

                //if (IsFieldHidden(_predIdCtrls[i].predId)) continue;

                string ctrl_filter = null;
                if (c is TextBox)
                {
                    TextBox tc = c as TextBox;
                    if (!tc.HasCheckbox || tc.Checked)
                        ctrl_filter = tc.GetFilterClauseText();
                }
                else
                    if (c is Select)
                    {
                        Select sc = c as Select;
                        if(!sc.HasCheckbox || sc.Checked)
                            ctrl_filter = sc.GetFilterClauseText();
                    }

                if (!string.IsNullOrEmpty(ctrl_filter))
                {
                    if (Description.Length > 0) Description.Append("<br/>");
                    Description.Append(ctrl_filter);
                }
            }

            return Description.ToString();
        }

        //Метод получет условные идентификаторы выбранных полей в виде строки разделенной запятыми
        private string GetCheckedFields()
        {
            StringBuilder sb = new StringBuilder();

            if ((!dateValidPeriodFilter.HasCheckbox || dateValidPeriodFilter.Checked) && !dateValidPeriodFilter.IsDisabled && !dateValidPeriodFilter.IsReadOnly)
                sb.AppendFormat("{0},", StoresPageHelper.predValid);

            for (int i = 0; i < _predIdCtrls.Length; i++)
            {
                V4Control c = _predIdCtrls[i].ctrl;
                if (c is TextBox)
                {
                    TextBox tc = c as TextBox;
                    if ((!tc.HasCheckbox || tc.Checked) && !tc.IsDisabled && !tc.IsReadOnly) sb.AppendFormat("{0},", _predIdCtrls[i].predId);
                }
                else
                    if (c is Select)
                    {
                        Select sc = c as Select;
                        if ((!sc.HasCheckbox || sc.Checked) && !sc.IsDisabled && !sc.IsReadOnly) sb.AppendFormat("{0},", _predIdCtrls[i].predId);
                    }
            }

            if (sb.Length > 0 && sb[sb.Length - 1] == ',')
                return sb.ToString(0, sb.Length - 1);

            return sb.ToString();
        }

        /// <summary>
        /// Метод восстанавливает значение свойства Checked элементов управления из строки параметров настройки
        /// </summary>
        /// <param name="strCheckedFields">Список условных идентификаторов полей</param>
        /// <param name="strstrCheckedFieldsArray">Список условных идентификаторов всех полей с установленным флажком</param>
        private void RestoreCheckedFields(string strCheckedFields, ref string strCheckedFieldsArray)
        {
            if(null==strCheckedFields) return;
            if (strCheckedFields.Length < 1) return;

            StringBuilder sb = new StringBuilder("[");

            List<string> checkedFields = new List<string>(strCheckedFields.Split(new[] { ' ', ',' }));

            if(!dateValidPeriodFilter.Checked) dateValidPeriodFilter.Checked = checkedFields.Contains(StoresPageHelper.predValid);

            if(dateValidPeriodFilter.Checked)
                sb.AppendFormat("'{0}',",StoresPageHelper.predValid);

            for (int i = 0; i < _predIdCtrls.Length; i++)
            {
                V4Control c = _predIdCtrls[i].ctrl;
                if (c is TextBox)
                {
                    TextBox tc = c as TextBox;
                    if (!tc.Checked) tc.Checked = checkedFields.Contains(_predIdCtrls[i].predId);

                    if(tc.Checked)
                        sb.AppendFormat("'{0}',", _predIdCtrls[i].predId);

                }
                else
                    if (c is Select)
                    {
                        Select sc = c as Select;
                        if (!sc.Checked) sc.Checked = checkedFields.Contains(_predIdCtrls[i].predId);

                        if (sc.Checked)
                            sb.AppendFormat("'{0}',", _predIdCtrls[i].predId);
                    }
            }

            if (sb[sb.Length - 1] == ',')
                sb[sb.Length - 1] = ']';
            else
                sb.Append(']');

            strCheckedFieldsArray = sb.ToString();
        }

        void TestTextBoxCheck(TextBox t)
        {
             if (string.IsNullOrWhiteSpace(t.Value)
                 && t.ValueTextBoxEnum != ((int)TextBoxEnum.Empty).ToString()
                 && t.ValueTextBoxEnum != ((int)TextBoxEnum.NotEmpty).ToString()
                 && t.ValueTextBoxEnum != ((int)StoreNameTextBox.TextBoxEnum.AccountUnknown).ToString())
                 t.Checked=false;
        }

        void TestSelectCheck(Select s)
        {
            if ((s.IsMultiSelect && string.IsNullOrWhiteSpace(s.SelectedItemsString) || !s.IsMultiSelect && string.IsNullOrWhiteSpace(s.Value))
                && s.ValueSelectEnum != ((int)SelectEnum.Any).ToString()
                && s.ValueSelectEnum != ((int)SelectEnum.NoValue).ToString())
                s.Checked = false;
        }

        /// <summary>
        /// Метод осуществляет поиск в соответствии с установленными параметрами
        /// </summary>
        private void FilterApply()
        {
            StoresClientScripts.SendSetInnerHtml(this, "SearchResult", string.Empty);

            //Сбрасывает номер текущей страницы...
            pageBar.SetDisabled(true, false);

            QueryBuilder qb = new QueryBuilder("AND");
            qb.SelectPart = @"Склады.КодСклада, Склады.Склад, Склады.КодХранителя, Склады.КодРаспорядителя, Склады.IBAN, Склады.Филиал, Склады.Примечание, Склады.От , DATEADD(day, -1, Склады.До) AS По";
            qb.FromPart = @"vwСклады Склады";

            if (!txtText.HasCheckbox || txtText.Checked)
            {
                QueryBuilder qb_or = new QueryBuilder("OR");
                qb_or.AddTextPartOfQuery(txtText, "Склады.Склад");
                qb_or.AddTextPartOfQuery(txtText, "Склады.IBAN");
                qb_or.AddTextPartOfQuery(txtText, "Склады.Филиал");
                qb_or.AddTextPartOfQuery(txtText, "Склады.Примечание");

                if (Regex.IsMatch(txtText.Value, "^\\d{1,9}$"))
                {
                    qb_or.AddWhere("Склады.КодСклада = @Text");

                    qb.sqlParams.Add("@Text", txtText.Value);
                }

                qb.AddWhere(qb_or.WherePart);
            }

            string strIBANPredicate = qb.AddTextPartOfQuery(txtIBAN, "Склады.IBAN");

            if (!txtName.HasCheckbox || txtName.Checked)
            {
                if (txtName.ValueTextBoxEnum.ToInt() == (int)StoreNameTextBox.TextBoxEnum.AccountUnknown)
                    qb.AddWhere("Склады.Склад = '" + StoreNameTextBox.NoNameValue + @"'");
                else
                {
                    if (null == strIBANPredicate || strIBANPredicate.Length < 1)
                    {
                        QueryBuilder qb_or = new QueryBuilder("OR");
                        qb_or.AddTextPartOfQuery(txtName, "Склады.Склад");
                        qb_or.AddTextPartOfQuery(txtName, "Склады.IBAN");
                        qb.AddWhere(qb_or.WherePart);
                    }
                    else
                        qb.AddTextPartOfQuery(txtName, "Склады.Склад");
                }
            }

            qb.AddTextPartOfQuery(txtStoreDepartment, "Склады.Филиал");
            qb.AddTextPartOfQuery(txtDescription, "Склады.Примечание");

            qb.AddCodesPartOfQuery(sStore, null, "Склады.КодСклада", "@Stores", null, null, null, null);
            qb.AddCodesPartOfQuery(sStoreType, "ТипыСкладов.ТипСклада", "Склады.КодТипаСклада", "@Type", "INNER", "Склады.КодТипаСклада = ТипыСкладов.КодТипаСклада", "ТипыСкладов", "ТипыСкладов(КодТипаСклада, ТипСклада)");
            qb.AddCodesPartOfQuery(sResource, "Ресурсы.РесурсРус", "Склады.КодРесурса", "@Resource", "INNER", "Склады.КодРесурса = Ресурсы.КодРесурса", "Ресурсы", "Ресурсы(КодРесурса, РесурсРус)");
            qb.AddCodesPartOfQuery(sStorage, "ISNULL(МестаХранения.МестоХранения,'#' + CONVERT(varchar, Склады.КодМестаХранения)) МестоХранения", "Склады.КодМестаХранения", "@Storage", "LEFT", "Склады.КодМестаХранения=МестаХранения.КодМестаХранения", "МестаХранения", "МестаХранения(КодМестаХранения, МестоХранения)");
            qb.AddCodesPartOfQuery(sKeeperBank, "ISNULL(Хранители.Кличка,'#' + CONVERT(varchar, Склады.КодХранителя)) Хранитель", "Склады.КодХранителя", "@Keeper", "LEFT", "Склады.КодХранителя = Хранители.КодЛица", "vwЛица Хранители", "Хранители(КодЛица, Кличка)");
            qb.AddCodesPartOfQuery(sManager, "ISNULL(Распорядители.Кличка, '#' + CONVERT(varchar, Склады.КодРаспорядителя)) Распорядитель", "Склады.КодРаспорядителя", "@Manager", "LEFT", "Склады.КодРаспорядителя = Распорядители.КодЛица", "vwЛица Распорядители", "Распорядители(КодЛица, Кличка)");
            qb.AddCodesPartOfQuery(sManagerDepartment, "ISNULL(ПодразделенияРаспорядителя.Подразделение, '#' + CONVERT(varchar, Склады.КодПодразделенияРаспорядителя)) ПодразделениеРаспорядителя", "Склады.КодПодразделенияРаспорядителя", "@ManagerDepartment", "LEFT", "Склады.КодПодразделенияРаспорядителя = ПодразделенияРаспорядителя.КодПодразделенияЛица", "vwПодразделенияЛиц ПодразделенияРаспорядителя", "ПодразделенияРаспорядителя(КодПодразделенияЛица, Подразделение)");
            qb.AddCodesPartOfQuery(sAgreement, null, "Склады.КодДоговора", "@Agreement", null, null, null, null);

             if ((!sQuery.HasCheckbox || sQuery.Checked) && !string.IsNullOrWhiteSpace(sQuery.SelectedItemsString))
             {
                Dictionary<string, object> querySqlParams = new Dictionary<string, object>();
                querySqlParams.Add("@КодУсловия", sQuery.SelectedItemsString);

                DataTable dtQueries = DBManager.GetData(Kesco.Lib.Entities.SQLQueries.SELECT_IDs_ДополнительныеФильтрыПриложений, Config.DS_user, System.Data.CommandType.Text, querySqlParams);
                if (null!=dtQueries)
                {
                    StringBuilder sbQuery = new StringBuilder();
                    foreach (DataRow r in dtQueries.Rows)
                    {
                        if (sbQuery.Length > 0)
                            sbQuery.Append(" AND ");

                        sbQuery.Append(r["Запрос"]);
                    }

                    qb.AddWhere(sbQuery.ToString());
                }
            }

             if (!dateValidPeriodFilter.HasCheckbox || dateValidPeriodFilter.Checked)
             {
                 object objValid = null == dateValid.ValueDate ? DBNull.Value : (object)((DateTime)dateValid.ValueDate).ToString("yyyyMMdd");

                 const int isDateAfter = 51; //После указанной даты
                 const int isDateBefore = 52; //До указанной даты
                 const int isDateNull = 54;//Дата не указана

                 QueryBuilder qb_valid_from = new QueryBuilder("OR");
                 qb_valid_from.AddWhere(QueryBuilder.CreatePredicate(isDateNull, "Склады.От", null));
                 qb_valid_from.AddWhere(QueryBuilder.CreatePredicate(isDateBefore, "Склады.От", "@Valid"));

                 QueryBuilder qb_valid_to = new QueryBuilder("OR");
                 qb_valid_to.AddWhere(QueryBuilder.CreatePredicate(isDateNull, "Склады.До", null));
                 qb_valid_to.AddWhere(QueryBuilder.CreatePredicate(isDateAfter, "Склады.До", "@Valid"));

                 int date_type = dateValidPeriodFilter.Value.ToInt();
                 switch (date_type)
                 {
                     case 0://Действовал на указанную дату
                         goto case 1;
                     case 1://Не действовал на указанную дату
                         if (DBNull.Value != objValid)
                         {
                             QueryBuilder qb_valid_at_date = new QueryBuilder("AND"); 

                             qb_valid_at_date.AddWhere(qb_valid_from.WherePart);
                             qb_valid_at_date.AddWhere(qb_valid_to.WherePart);

                            string validKeeperAndManager = @"(Склады.КодХранителя IS NULL OR EXISTS(
SELECT КодЛица FROM vwКарточкиФизЛиц AS Карточки WHERE КодЛица=Склады.КодХранителя AND Карточки.От<=@Valid AND Карточки.До>@Valid
UNION ALL
SELECT КодЛица FROM vwКарточкиЮрЛиц AS Карточки WHERE КодЛица=Склады.КодХранителя AND Карточки.От<=@Valid AND Карточки.До>@Valid))
AND
(Склады.КодРаспорядителя IS NULL OR EXISTS(
SELECT КодЛица FROM vwКарточкиФизЛиц AS Карточки WHERE КодЛица=Склады.КодРаспорядителя AND Карточки.От<=@Valid AND Карточки.До>@Valid
UNION ALL
SELECT КодЛица FROM vwКарточкиЮрЛиц AS Карточки WHERE КодЛица=Склады.КодРаспорядителя AND Карточки.От<=@Valid AND Карточки.До>@Valid))";

                             qb_valid_at_date.AddWhere(validKeeperAndManager);

                             if (date_type == 0)
                                 qb.AddWhere(qb_valid_at_date.WherePart);
                             else
                                 qb.AddWhere(string.Format(" NOT ({0})", qb_valid_at_date.WherePart));
                         }
                         break;

                     case 2://Не действовал до указанной даты
                         if (DBNull.Value != objValid)
                         {
                             //Отнимаем один день от запрошенной даты
                             DateTime prevDay = (DateTime)dateValid.ValueDate;
                             if (prevDay > DateTime.MinValue) prevDay = prevDay.AddDays(-1);

                             objValid = (object)Kesco.Lib.ConvertExtention.Convert.DateTime2Str(prevDay);

                             qb.AddWhere(string.Format(" NOT ({0})", qb_valid_from.WherePart));
                         }
                         break;

                     case 3://Не действовал после указанной даты
                         if (DBNull.Value != objValid)
                         {
                             //Добавляем один день к запрошенной дате
                             DateTime nextDay = (DateTime)dateValid.ValueDate;
                             if (nextDay < DateTime.MaxValue) nextDay = nextDay.AddDays(1);

                             objValid = (object)Kesco.Lib.ConvertExtention.Convert.DateTime2Str(nextDay);

                             qb.AddWhere(string.Format(" NOT ({0})", qb_valid_to.WherePart));
                         }
                         break;

                     case 4://Даты действия не установлены
                         qb.AddWhere(QueryBuilder.CreatePredicate(isDateNull, "Склады.До", null));
                         qb.AddWhere(QueryBuilder.CreatePredicate(isDateNull, "Склады.От", null));
                         break;
                 }

                 qb.sqlParams.Add("@Valid", objValid);
             }

            _table.sqlParams = qb.sqlParams;
            _table.sqlCmd = qb.Text;

            //pageBar.CurrentPageNumber = 1;

            //SearchFilterBody.Value = FilterGetDescription();

            DataTable dtResult = _table.Update();
            RestoreCursor();

            if (null != dtResult && dtResult.Rows.Count>1)
                StoresClientScripts.DisplayFilterDescription(this, true);
        }
    }
}