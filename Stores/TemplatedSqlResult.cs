using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Xml.Xsl;
using System.Xml;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Controls.V4.PagingBar;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.DALC;
using Kesco.Lib.Web.Settings;
using Kesco.Lib.BaseExtention;

namespace Kesco.App.Web.Stores
{
    /// <summary>
    /// Класс объекта для формирования и управления таблицей с результатами выполнения запроса
    /// </summary>
    public class TemplatedSqlResult
    {
        /// <summary>
        /// Вспомогательный класс для добавления ссылки на Лицо в таблицу с результатами поиска
        /// </summary>
        public class EmpoyeeData
        {
            Kesco.Lib.Web.Controls.V4.Common.Page _parent;

            public EmpoyeeData(Kesco.Lib.Web.Controls.V4.Common.Page p)
            {
                _parent = p;
            }

            public string GetLinkToPersonInfo(string id, string name)
            {
                using (StringWriter writer = new StringWriter())
                {
                    _parent.RenderLinkPerson(writer, "person" + id, id, name);
                    return writer.ToString();
                }
            }
        }

        //Имя файла xslt шаблона для вывода таблицы
        private string _template;

        //Страница на которой располагается таблица
        private Page _parentPage;

        //Элемент управения страничным отображением таблицы
        private PagingBar _pageBar;

        //Идентификатор html элемента в который будет вставлена таблица
        private string _divId;

        //Строка запроса SQL для получения результатов
        private string _sqlCmd;
        public string sqlCmd { set { _sqlCmd = value; } get { return _sqlCmd; } }

        //Тип строки запроса SQL для получения результатов
        private CommandType _sqlCmdType;

        //Параметры используемые в последнем запросе поиска
        public Dictionary<string, object> SqlParams;

        //Параметры сортировки результатов поиска по колонкам в формате имя колонки - _stdAsc или _stdDesc
        private List<KeyValuePair<string, object>> sortData = new List<KeyValuePair<string, object>>();

        //Строки используемые для формирования запроса сортировки по выбранным колонкам
        private const string _stdAsc = " ASC";
        private const string _stdDesc = " DESC";

        private TemplatedSqlResult() {}

        public TemplatedSqlResult(Page page, string ctrl_id, PagingBar bar, string template, string sqlCmd, CommandType sqlCmdType)
        {
            _template = template;
            _parentPage = page;
            _pageBar = bar;
            _divId = ctrl_id;
            _sqlCmd = sqlCmd;
            _sqlCmdType = sqlCmdType;
            SqlParams = new Dictionary<string, object>();
        }

        /// <summary>
        /// Метод устанавливает первую колонку параметров сортировки результатов и вызывает обновление таблицы результатов поиска
        /// </summary>
        /// <param name="strColumn">Имя первой колонки в списке сортировки</param>
        public void SortResultColumn(string strColumn)
        {
            if (string.IsNullOrWhiteSpace(strColumn)) return;

            object sorObj = _stdAsc;

            for (int i = 0; i < sortData.Count; i++)
            {
                if (sortData[i].Key == strColumn)
                {
                    if ((object)_stdAsc == sortData[i].Value)
                        sorObj = _stdDesc;

                    sortData.RemoveAt(i);
                    break;
                }
            }

            sortData.Insert(0, new KeyValuePair<string, object>(strColumn, sorObj));

            Update();
        }

        /// <summary>
        /// Метод формирует список колонок сортировки и заданной строки в формате SQL
        /// Используется для восстановления списка колонок сортировки из сохраненных настроек
        /// </summary>
        /// <param name="strSortMode"></param>
        public void SetSortMode(string strSortMode)
        {
            sortData.Clear();
            string[] sa = strSortMode.Split(',');
            foreach (string s in sa)
            {
                if (string.IsNullOrWhiteSpace(s)) continue;
                string sort_column = s.Trim();

                object sorObj = _stdAsc;
                int space_index = sort_column.LastIndexOf(' ');
                if (space_index > -1)
                {
                    if (0 == string.Compare(sort_column, space_index, _stdDesc, 0, _stdDesc.Length, true))
                        sorObj = _stdDesc;
                    else
                        if (0 != string.Compare(sort_column, space_index, _stdAsc, 0, _stdAsc.Length, true))
                            space_index = sort_column.Length;
                }
                else space_index = sort_column.Length;

                string strKey = sort_column.Substring(0, space_index).TrimEnd();
                if (!sortData.Exists(p => { return p.Key == strKey; }))
                    sortData.Add(new KeyValuePair<string, object>(strKey, sorObj));
            }
        }

        /// <summary>
        /// Метод формирует строку-условие сортировка в формате SQL
        /// </summary>
        /// <param name="columns">Микасимальное количество используемых колонок</param>
        /// <returns>Строку сортироки формате SQL</returns>
        public string GetSortMode(int columns)
        {
            if (columns < 0) columns = sortData.Count;

            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, object> kvp in sortData)
            {
                if (--columns < 0) break;

                if (sb.Length > 0) sb.Append(", ");
                sb.Append(kvp.Key + kvp.Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Метод устанавливает отображение таблицы результатов поиска как не соответствующей установленному фильтру поиска
        /// </summary>
        public void Disable()
        {
            if (null != _pageBar)
                _pageBar.SetDisabled(true, false);

            _parentPage.JS.Write("GrayResultTable();");
        }

        /// <summary>
        /// Метод устанавливает параметры сортировки и отправляет запрос поиска в БД в соответствии с ранее установленными параметрами.
        /// Отображает или обновляет таблицу с результатами поиска
        /// </summary>
        public DataTable Update()
        {
            string rowNumber = "-1";
            string pageNum = null==_pageBar ? "-1" : _pageBar.CurrentPageNumber.ToString();
            string itemsPerPage = null==_pageBar ? "-1" : _pageBar.RowsPerPage.ToString();
            string pageCount = "-1";

            string strSort = GetSortMode(-1);

            if (_sqlCmdType == CommandType.StoredProcedure)
                SqlParams["@Sort"] = strSort;

            DataTable dtStoresResult = DBManager.GetData(_sqlCmd, Config.DS_person, _sqlCmdType, SqlParams, null, strSort, string.Empty, null, null, null, ref pageNum, ref itemsPerPage, ref pageCount, out rowNumber);

            if (null == dtStoresResult)
            {
                StoresClientScripts.SendSetInnerHtml(_parentPage, _divId, string.Empty);
                if(null!=_pageBar) _pageBar.SetDisabled(true, true);
                return null;
            }

            if (null != _pageBar)
            {
                _pageBar.MaxPageNumber = pageCount.ToInt();

                while (_pageBar.MaxPageNumber < _pageBar.CurrentPageNumber)
                {
                    //Такая ситуация возможна при удалении последней записи на последней странице
                    //Используем --_pageBar.CurrentPageNumber > 0 что избежать бесконечной рекурсии
                    if (--_pageBar.CurrentPageNumber > 0 && _pageBar.CurrentPageNumber <= _pageBar.MaxPageNumber)
                    {
                        return Update();
                    }
                }

                _pageBar.SetDisabled(dtStoresResult.Rows.Count < 1, false);
            }

            string strXmlResult = string.Empty;
            using (StringWriter writer = new StringWriter())//warning : CA2000 : Microsoft.Reliability
            {
                dtStoresResult.WriteXml(writer);
                strXmlResult = writer.ToString();
            }

            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load(_parentPage.Server.MapPath(_template));

            XsltArgumentList argsList = new XsltArgumentList();

            EmpoyeeData ed = new EmpoyeeData(_parentPage);
            argsList.AddExtensionObject("urn:kesco-stores-person", ed);
            if (_parentPage.ReturnId == "1")
                argsList.AddParam("return_id", "", true);

            if (null == _pageBar)
            {
                argsList.AddParam("total_count", "", dtStoresResult.Rows.Count);
                argsList.AddParam("current_page", "", 1);
                argsList.AddParam("page_size", "", dtStoresResult.Rows.Count);
            }
            else
            {
                argsList.AddParam("total_count", "", rowNumber);
                argsList.AddParam("current_page", "", _pageBar.CurrentPageNumber);
                argsList.AddParam("page_size", "", _pageBar.RowsPerPage);
            }

            using (StringReader sreader = new StringReader(strXmlResult))//warning : CA2000 : Microsoft.Reliability
            {
                XmlTextReader xreader = new XmlTextReader(sreader);
                using (StringWriter swriter = new StringWriter())//warning : CA2000 : Microsoft.Reliability
                {
                    XmlTextWriter xwriter = new XmlTextWriter(swriter);

                    xslt.Transform(xreader, argsList, xwriter);
                    StoresClientScripts.SendSetInnerHtml(_parentPage, _divId, swriter.ToString());
                    //_parentCtrl.Value = swriter.ToString();
                }
            }

            //Таблицу необходимо перерисовать в любом случае (возможно она была отображена с другими стилями)
            //При изменении содержимого элементов из JavaScript (изменили аттрибут class) серверные значения Value
            //остаются неизменными, что приводит к неправильному определению изменения содержимого элемента
            //Но !!! При выполнении какой-либо следующей команды, все изменения сделанные на стороне клиента будут отменены...
            //_parentCtrl.SetPropertyChanged("Value");

            return dtStoresResult;
        }
    }
}