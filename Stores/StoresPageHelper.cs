using System;
using System.Web;
using System.Text;

using System.Collections.Specialized;
using System.Collections.Generic;

using Kesco.Lib.Web.Settings.Parameters;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.DBSelect.V4;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.Web.Settings;
using System.Collections;

namespace Kesco.App.Web.Stores
{
    /// <summary>
    /// Вспомогательный класс для работы с настройками и Javascript используемый всеми страницами приложения
    /// </summary>
    public class StoresPageHelper
    {
        //Константа для вычисления типа условия для текста
        public const int txtModeOffset = 11;

        //Константа для вычисления типа условия для идентификаторов, перечисленных через запятую
        public const int selectModeOffset = 1;

        //Коллекция имен параметров предыдущих версий приложения для использования с объектом типа AppParamsManager
        public static StringCollection OldParameterNamesCollection = new StringCollection() {
            "StoreExcept", //склады не включаемые в поиск
            "Search", //строка поиска в разных полях
            "StoreHowSearch", //1 - поле содержит текст, 0 - поле начинается с текста
            "StoreType", //тип склада
            "StoreActual", //склад 1-действующий, 0 - недействующий
            "StoreNoName", //номер счета неизвестен
            "StoreSize", //количество результатов на одну страницу
            "StoreResidence", //место хранения
            "StoreResource", //хранимый ресурс
            "StoreKeeper", //хранитель
            "StoreManager", //распорядитель
            "StoreContract" //договор хранения
        };

        //Коллекция имен параметров свойств окон приложения для использования с объектом типа AppParamsManager
        public static StringCollection WindowParameterNamesCollection = new StringCollection() {
            "StoreSrchWndLeft",
            "StoreSrchWndTop",
             //Размеры окна поиск складов, используется другими приложениями, изменять нельзя
            "Width",
            "Height",
            ///////////////////////////////////////////
            "StoreSrchFilterDescr",
            "StoreSrchFields",

            "StoreWndLeft",
            "StoreWndTop",
            "StoreWndWidth",
            "StoreWndHeight",

            "StoreRptWndLeft",
            "StoreRptWndTop",
            "StoreRptWndWidth",
            "StoreRptWndHeight"
        };

        //Коллекция имен параметров склада
        public static StringCollection StoreParameterNamesCollection = new StringCollection() {
            "StoreValidFrom",
            "StoreValidTo",
            "StoreType",
            "StoreStorage",
            "StoreName",
            "StoreIban",
            "StoreAccountUnknown",
            "StoreKeeper",
            "StoreManager",
            "StoreManagerDepartm",
            "StoreResource",
            "StoreAgreement",
            "StoreDepartment",
            "StoreDescription"
        };

        //Коллекция имен параметров для поиска складов для использования с объектом типа AppParamsManager
        public static StringCollection SearchParameterNamesCollection = new StringCollection() {
            "StoreSrchExceptions",
            "StoreSrchTextMode",
            "StoreSrchText",
            "StoreSrchValidMode",
            "StoreSrchValidPeriod",
            "StoreSrchValidFrom",
            "StoreSrchValidTo",
            "StoreSrchValid",
            "StoreSrchTypeMode",
            "StoreSrchType",
            "StoreSrchStorageMode",
            "StoreSrchStorage",
            "StoreSrchNameMode",
            "StoreSrchName",
            "StoreSrchIbanMode",
            "StoreSrchIban",
            "StoreSrchKeeperMode",
            "StoreSrchKeeper",
            "StoreSrchManagerMode",
            "StoreSrchManager",
            "StoreSrchMngrDptMode",
            "StoreSrchMngrDpt",
            "StoreSrchRsrcMode",
            "StoreSrchRsrc",
            "StoreSrchAgrmntMode",
            "StoreSrchAgrmnt",
            "StoreSrchDprtMode",
            "StoreSrchDprt",
            "StoreSrchDescrMode",
            "StoreSrchDescr",
            "StoreSrchQuery",
            "StoreSrchResults",
            "StoreSrchSort",
            "StoreSrchChecked",
            "StoreSrchStores",
            "StoreSrchStoresMode"
        };

        //Коллекция имен параметров для отчета по складам для использования с объектом типа AppParamsManager
        public static StringCollection ReportParametersNamesCollection = new StringCollection(){
            "StoreRprtType",
            "StoreRprtResults",
        };

        //Класс параметров предыдущих версий приложения
        public static class OldParameters
        {
            public static readonly string Except = OldParameterNamesCollection[0];
            public static readonly string Search = OldParameterNamesCollection[1];
            public static readonly string HowSearch = OldParameterNamesCollection[2];
            public static readonly string Type = OldParameterNamesCollection[3];
            public static readonly string Actual = OldParameterNamesCollection[4];
            public static readonly string NoName = OldParameterNamesCollection[5];
            public static readonly string Size = OldParameterNamesCollection[6];
            public static readonly string Residence = OldParameterNamesCollection[7];
            public static readonly string Resource = OldParameterNamesCollection[8];
            public static readonly string Keeper = OldParameterNamesCollection[9];
            public static readonly string Manager = OldParameterNamesCollection[10];
            public static readonly string Contract = OldParameterNamesCollection[11];
        }

        //Класс параметров окна редактирования склада
        public static class WindowParameters
        {
            public static readonly string SrchLeft = WindowParameterNamesCollection[0];
            public static readonly string SrchTop = WindowParameterNamesCollection[1];
            public static readonly string SrchWidth = WindowParameterNamesCollection[2];
            public static readonly string SrchHeight = WindowParameterNamesCollection[3];
            public static readonly string Filter = WindowParameterNamesCollection[4];
            public static readonly string Fields = WindowParameterNamesCollection[5];
            
            public static readonly string Left = WindowParameterNamesCollection[6];
            public static readonly string Top = WindowParameterNamesCollection[7];
            public static readonly string Width = WindowParameterNamesCollection[8];
            public static readonly string Height = WindowParameterNamesCollection[9];

            public static readonly string RptLeft = WindowParameterNamesCollection[10];
            public static readonly string RptTop = WindowParameterNamesCollection[11];
            public static readonly string RptWidth = WindowParameterNamesCollection[12];
            public static readonly string RptHeight = WindowParameterNamesCollection[13];
        }

        //Класс параметров склада
        public static class StoreParameters
        {
            public static readonly string ValidFrom = StoreParameterNamesCollection[0];
            public static readonly string ValidTo = StoreParameterNamesCollection[1];
            public static readonly string Type = StoreParameterNamesCollection[2];
            public static readonly string Storage = StoreParameterNamesCollection[3];
            public static readonly string Name = StoreParameterNamesCollection[4];
            public static readonly string Iban = StoreParameterNamesCollection[5];
            public static readonly string AccountNumberUnknown = StoreParameterNamesCollection[6];

            public static readonly string Keeper = StoreParameterNamesCollection[7];
            public static readonly string Manager = StoreParameterNamesCollection[8];
            public static readonly string ManagerDepartment = StoreParameterNamesCollection[9];

            public static readonly string Resource = StoreParameterNamesCollection[10];
            public static readonly string Agreement = StoreParameterNamesCollection[11];
            public static readonly string Department = StoreParameterNamesCollection[12];
            public static readonly string Description = StoreParameterNamesCollection[13];
        }

        //Класс параметров для поиска складов
        public static class SearchParameters
        {
            public static readonly string Exceptions = SearchParameterNamesCollection[0];
            public static readonly string TextMode = SearchParameterNamesCollection[1];
            public static readonly string Text = SearchParameterNamesCollection[2];
            public static readonly string ValidMode = SearchParameterNamesCollection[3];
            public static readonly string ValidPeriod = SearchParameterNamesCollection[4];
            public static readonly string ValidFrom = SearchParameterNamesCollection[5];
            public static readonly string ValidTo = SearchParameterNamesCollection[6];
            public static readonly string Valid = SearchParameterNamesCollection[7];
            public static readonly string TypeMode = SearchParameterNamesCollection[8];
            public static readonly string Type = SearchParameterNamesCollection[9];
            public static readonly string StorageMode = SearchParameterNamesCollection[10];
            public static readonly string Storage = SearchParameterNamesCollection[11];
            public static readonly string NameMode = SearchParameterNamesCollection[12];
            public static readonly string Name = SearchParameterNamesCollection[13];
            public static readonly string IbanMode = SearchParameterNamesCollection[14];
            public static readonly string Iban = SearchParameterNamesCollection[15];
            public static readonly string KeeperMode = SearchParameterNamesCollection[16];
            public static readonly string Keeper = SearchParameterNamesCollection[17];
            public static readonly string ManagerMode = SearchParameterNamesCollection[18];
            public static readonly string Manager = SearchParameterNamesCollection[19];
            public static readonly string ManagerDepartmentMode = SearchParameterNamesCollection[20];
            public static readonly string ManagerDepartment = SearchParameterNamesCollection[21];
            public static readonly string ResourceMode = SearchParameterNamesCollection[22];
            public static readonly string Resource = SearchParameterNamesCollection[23];
            public static readonly string AgreementMode = SearchParameterNamesCollection[24];
            public static readonly string Agreement = SearchParameterNamesCollection[25];
            public static readonly string DepartmentMode = SearchParameterNamesCollection[26];
            public static readonly string Department = SearchParameterNamesCollection[27];
            public static readonly string DescriptionMode = SearchParameterNamesCollection[28];
            public static readonly string Description = SearchParameterNamesCollection[29];
            public static readonly string Query = SearchParameterNamesCollection[30];
            public static readonly string ResultsPerPage = SearchParameterNamesCollection[31];
            public static readonly string Sort = SearchParameterNamesCollection[32];
            public static readonly string CheckedFields = SearchParameterNamesCollection[33];
            public static readonly string Stores = SearchParameterNamesCollection[34];
            public static readonly string StoresMode = SearchParameterNamesCollection[35];
        }

        //Класс параметров для отчёт по складам
        public static class ReportParameters
        {
            public static readonly string Type = ReportParametersNamesCollection[0];
            public static readonly string ResultsPerPage = ReportParametersNamesCollection[1];
        }

        //Условные идентификаторы полей поиска, используются при сохранении настроек,
        //из них формируются действительные идентификаторы элементов на странице
        public const string predStore = "Store";
        public const string predText = "Text";
        public const string predValid = "Valid";
        public const string predType = "Type";
        public const string predStorage = "Storage";
        public const string predName = "Name";
        public const string predIban = "Iban";
        public const string predKeeper = "Keeper";
        public const string predManager = "Manager";
        public const string predManagerDepartment = "ManagerDepartment";
        public const string predAgreement = "Agreement";
        public const string predResource = "Resource";
        public const string predDepartment = "Department";
        public const string predDescription = "Description";
        public const string predQuery = "Query";

        //Массив условных идентификаторов полей поиска
        static private string[] _searchFields = new string[] { predValid,
                                                predType,
                                                predStorage,
                                                predName,
                                                predIban,
                                                predKeeper,
                                                predManager,
                                                predManagerDepartment,
                                                predAgreement,
                                                predResource,
                                                predDepartment,
                                                predDescription,
                                                predText,
                                                predStore,
                                                predQuery};

        //Объект запрос текущей страницы
        private HttpRequest _request;

        //Объект доступа с параметрам из БД сохраненнных настроек
        private AppParamsManager _parametersManager;

        //Множество параметров полученных из строки запроса методами getRequestParameterValue() и getParameterValue(),
        //Используется для отображения соответстующих элементов
        //управления условиями поиска
        private HashSet<string> _requestParameters;

        private StoresPageHelper()
        {
        }

        public StoresPageHelper(HttpRequest request, AppParamsManager pm)
        {
            _request = request;
            _parametersManager = pm;
            _requestParameters = new HashSet<string>();
        }

        /// <summary>
        /// Метод формирует строку идентификаторов, которые необходимо скрыть на странице
        /// </summary>
        /// <param name="visible">список идентификаторов, которые быть отображены на странице</param>
        /// <param name="hiddenFields">массив идентификаторов скрытых элементов</param>
        /// <returns>Строковое представления массива для передачи его в Javascript код</returns>
        static public string GetStrHiddenFields(List<string> visible, ref string[] hiddenFields)
        {
            int index = 0;
            hiddenFields = new string[_searchFields.Length / _searchFields.Rank];

            StringBuilder sb = new StringBuilder("[");
            foreach (string s in _searchFields)
            {
                if (visible.IndexOf(s) < 0)
                {
                    sb.AppendFormat("'{0}',", s);
                    hiddenFields[index++] = s;
                }
            }

            Array.Resize(ref hiddenFields, index);

            /*
            foreach (string s in hidden)
            {
                //Проверка на допустимость параметра, чтобы нельзя было скрыть другие элементы управления через параметр запроса
                if (Array.Exists(fields, f => s==f))
                    sb.AppendFormat("'{0}',", s);
            }
            */

            if (sb[sb.Length - 1] == ',')
                sb[sb.Length - 1] = ']';
            else
                sb.Append(']');

            return sb.ToString();
        }

        /// <summary>
        /// Удаляет из строки идентификаторов скрытых элементов условий поиска те, которые
        /// не соответсвуют условиям заданным через строку запроса
        /// таким образом, элементы значения которых установлены через строку запроса остаются видимыми
        /// </summary>
        /// <param name="strVisible">Строка идентификаторов, которые быть отображены на странице</param>
        /// <param name="hiddenFields">Возвращаемый массив идентификаторов скрытых элементов</param>
        /// <param name="requiredFields">Возвращаемая строка представления массива идентификаторов обязательных полей поиска для передачи его в Javascript код</param>
        /// <returns>Строковое представления массива для передачи его в Javascript код</returns>
        public string GetFormHiddenFields(string strVisible, ref string[] hiddenFields, ref string requiredFields)
        {
            string[,] p = new string[,] { { _searchFields[0], SearchParameters.ValidMode },
                                            { _searchFields[0], SearchParameters.ValidPeriod },
                                            { _searchFields[0], SearchParameters.ValidFrom },
                                            { _searchFields[0], SearchParameters.ValidTo },
                                            { _searchFields[1], SearchParameters.TypeMode },
                                            { _searchFields[1], SearchParameters.Type },
                                            { _searchFields[2], SearchParameters.StorageMode },
                                            { _searchFields[2], SearchParameters.Storage },
                                            { _searchFields[3], SearchParameters.NameMode },
                                            { _searchFields[3], SearchParameters.Name },
                                            { _searchFields[4], SearchParameters.IbanMode },
                                            { _searchFields[4], SearchParameters.Iban },
                                            { _searchFields[5], SearchParameters.KeeperMode },
                                            { _searchFields[5], SearchParameters.Keeper },
                                            { _searchFields[6], SearchParameters.ManagerMode },
                                            { _searchFields[6], SearchParameters.Manager },
                                            { _searchFields[7], SearchParameters.ManagerDepartmentMode },
                                            { _searchFields[7], SearchParameters.ManagerDepartment },
                                            { _searchFields[8], SearchParameters.AgreementMode },
                                            { _searchFields[8], SearchParameters.Agreement },
                                            { _searchFields[9], SearchParameters.ResourceMode },
                                            { _searchFields[9], SearchParameters.Resource },
                                            { _searchFields[10], SearchParameters.DepartmentMode },
                                            { _searchFields[10], SearchParameters.Department },
                                            { _searchFields[11], SearchParameters.DescriptionMode },
                                            { _searchFields[11], SearchParameters.Description },
                                            { _searchFields[12], SearchParameters.Text },
                                            { _searchFields[13], SearchParameters.StoresMode },
                                            { _searchFields[13], SearchParameters.Stores },
                                            { _searchFields[14], SearchParameters.Query },

                                            { _searchFields[1], OldParameters.Type },
                                            { _searchFields[0], OldParameters.Actual },
                                            { _searchFields[3], OldParameters.NoName },
                                            { _searchFields[2], OldParameters.Residence },
                                            { _searchFields[9], OldParameters.Resource },
                                            { _searchFields[5], OldParameters.Keeper },
                                            { _searchFields[6], OldParameters.Manager },
                                            { _searchFields[8], OldParameters.Contract },
                                            { _searchFields[12], OldParameters.Search },
                                            { _searchFields[13], OldParameters.Except }
                                        };

            List<string> visible = new List<string>(strVisible.Split(new[] { ' ', ',' }));

            StringBuilder sb = new StringBuilder("[");

            for (int i = 0; i < p.Length / p.Rank; i++)
            {
                if (_requestParameters.Contains(p[i, 1]))
                {
                    visible.Add(p[i, 0]);
                    sb.AppendFormat("'{0}',", p[i, 0]);
                }
            }

            if (sb[sb.Length - 1] == ',')
                sb[sb.Length - 1] = ']';
            else
                sb.Append(']');

            requiredFields = sb.ToString();

            return GetStrHiddenFields(visible, ref hiddenFields);
        }

        /// <summary>
        /// Метод для установки параметров элемента управления TextBox
        /// </summary>
        /// <param name="ctrl">Элемент управления</param>
        /// <param name="param_mode">Имя параметры фильтра элемента управления</param>
        /// <param name="param_value">Имя параметры значения элемента управления</param>
        public void setTextBoxParameterValue(TextBox ctrl, string param_mode, string param_value)
        {
            bool isRequiredMode = false;
            bool isRequiredValue = false;

            setTextBoxValue(ctrl, getParameterValue(param_mode, out isRequiredMode, (txtModeOffset+(int)TextBoxEnum.ContainsAll).ToString()),
                getParameterValue(param_value, out isRequiredValue, string.Empty), isRequiredValue);
        }

        /// <summary>
        /// Метод для установки параметров элемента управления TextBox
        /// </summary>
        /// <param name="ctrl">Элемент управления</param>
        /// <param name="mode">Значение фильтра элемента управления</param>
        /// <param name="value">Значения элемента управления</param>
        /// <param name="isRequired">Флаг указывающий, что значение параметра обязятельное, его редактирование запрещено</param>
        public void setTextBoxValue(TextBox ctrl, string mode, string value, bool isRequired)
        {
            int intMode = 0;
            if(int.TryParse(mode, out intMode))
            {
                if (intMode >= txtModeOffset)
                    intMode -= txtModeOffset;
            }

            TextBoxEnum[] modes = (TextBoxEnum[])Enum.GetValues(typeof(TextBoxEnum));
            for (int i = 0; i < modes.Length; i++)
            {
                if (intMode == ((int)modes[i]))
                {
                    ctrl.ValueTextBoxEnum = intMode.ToString();
                    break;
                }
            }

            ctrl.Value = value;
            //if (!ctrl.IsDisabled) ctrl.IsDisabled = isRequired;
            if (!ctrl.IsReadOnly) ctrl.IsReadOnly = isRequired;// ctrl.IsDisabled;
            if (ctrl.HasCheckbox)
                ctrl.Checked = isRequired;//value != string.Empty || mode != ((int)TextBoxEnum.ContainsAll).ToString();
        }

        /// <summary>
        /// Метод для установки параметров элемента управления DBSelect
        /// </summary>
        /// <param name="ctrl">Элемент управления</param>
        /// <param name="param_mode">Имя параметры фильтра элемента управления</param>
        /// <param name="param_value">Имя параметры значения элемента управления</param>
        public void setSelectCtrlParameterValue(DBSelect ctrl, string param_mode, string param_value)
        {
            bool isRequiredMode = false;
            bool isRequiredValue = false;

            string mode_value = param_mode == null ? (selectModeOffset+(int)SelectEnum.Contain).ToString() : getParameterValue(param_mode, out isRequiredMode, ((int)SelectEnum.Contain).ToString());
            setSelectCtrlValue(ctrl, mode_value, getParameterValue(param_value, out isRequiredValue, string.Empty), isRequiredValue);
        }

        /// <summary>
        /// Метод для установки параметров элемента управления DBSelect
        /// </summary>
        /// <param name="ctrl">Элемент управления</param>
        /// <param name="mode">Значение фильтра элемента управления</param>
        /// <param name="value">Значения элемента управления - идентификаторы объектов, перечисленные через запятую</param>
        /// <param name="isRequired">Флаг указывающий, что значение параметра обязятельное, его редактирование запрещено</param>
        public void setSelectCtrlValue(DBSelect ctrl, string mode, string value, bool isRequired)
        {
            int intMode = 0;
            if (int.TryParse(mode, out intMode))
            {
                if (intMode >= selectModeOffset)
                    intMode -= selectModeOffset;
            }

            SelectEnum[] modes = (SelectEnum[])Enum.GetValues(typeof(SelectEnum));
            for (int i = 0; i < modes.Length; i++)
            {
                if (intMode == ((int)modes[i]))
                {
                    ctrl.ValueSelectEnum = intMode.ToString();
                    break;
                }
            }

            string[] arr = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            if (arr.Length > 0)
            {
                if (ctrl.IsMultiSelect)
                {
                    foreach (var id in arr)
                    {
                        object obj = ctrl.GetObjectById(id);
                        if (obj!=null)
                            ctrl.SelectedItems.Add(new Kesco.Lib.Entities.Item { Id = id, Value = obj });
                    }
                }
                else
                    ctrl.Value = arr[0];
            }

            //if (!ctrl.IsDisabled) ctrl.IsDisabled = isRequired;
            if (!ctrl.IsReadOnly) ctrl.IsReadOnly = isRequired;// ctrl.IsDisabled;

            if (ctrl.HasCheckbox) ctrl.Checked = isRequired;//value != string.Empty || mode != ((int)SelectEnum.Contain).ToString() || ctrl.SelectedItems.Count>0;
        }

        /// <summary>
        /// Метод получает значение параметра из строки запроса или БД сохраненных параметров
        /// </summary>
        /// <param name="name">Имя параметра</param>
        /// <param name="isRequired">Возращаемый флаг, указывающий, что применение этого параметра обязательно</param>
        /// <param name="valueType"> Возращаемый тип полученного значения 0-значение по умолчанию defstr, 1-из строки запроса, 2 - из БД </param>
        /// <param name="defstr">Значение по умолчанию для параметра</param>
        /// <returns>Значение параметра</returns>
        public string getParameterValue(string name, out bool isRequired, string defstr)
        {
            string value = _request.QueryString[name];
            isRequired = !string.IsNullOrEmpty(value);
            if (isRequired)
            {
                _requestParameters.Add(name);
                return value;
            }

            if (null!=_parametersManager)
                value = getDbParameterValue(name);

            if (null == value)
            {
                value = _request.QueryString["_" + name];

                if (null == value)
                    return defstr;
                //Добавляются только обязательные параметры, эти поля запрещено скрывать, необязательные можно
                //else
                //    _requestParameters.Add(name);
            }

            return value;
        }

        /// <summary>
        /// Метод получает значение параметра из строки запроса без обращения к БД сохраненных параметров
        /// </summary>
        /// <param name="name">Имя параметра</param>
        /// <param name="isRequired">Возращаемый флаг, указывающий, что применение этого параметра обязательно</param>
        /// <returns>Значение параметра или null, если параметр отсутствует в строке запроса</returns>
        public string getRequestParameterValue(string name, out bool isRequired)
        {
            string value = _request.QueryString[name];
            isRequired = !string.IsNullOrEmpty(value);
            if (isRequired)
            {
                _requestParameters.Add(name);
                return value;
            }

            if (null == value)
                value = _request.QueryString["_" + name];

            //Добавляются только обязательные параметры, эти поля запрещено скрывать, необязательные можно
            //if (null != value)
            //    _requestParameters.Add(name);

            return value;
        }

        /// <summary>
        /// Получение последнего значения параметра из БД сохраненных параметров пользователей
        /// </summary>
        /// <param name="name">Имя параметра</param>
        /// <returns>Значение параметра</returns>
        public string getDbParameterValue(string name)
        {
            AppParameter appParam = _parametersManager.Params.Find(p => p.Name == name);
            //if (null == appParam || null == appParam.Value) return string.Empty;
            if (null == appParam) return null;
            return appParam.Value;
        }
    }
}