//Глобальные переменные должны быть инициализированы во время загрузки страницы, обычно в методе Page_Load()
var callbackUrl;
var control;
var multiReturn;

var mvc = 0;
var domain = '';
var storesUrl = 'Store.aspx';
var storeReportUrl = 'StoreOrder.aspx';
var isReturn = false;

var pred_prefix = "pred";
var display_prefix = "display_";
var display_prefix_len = display_prefix.length;

//Функция обратного вызова вызываемая после закрытия окна редактирования склада
var StoreSavedCreatedNotificationCallBack = SearchStore;

//Добавляет обработчик ошибки, с переопределённым методом focus(), который вызывается по нажатию кнопки диалога
function SetDialogOkHandler(href_url) {
    var newDiv = document.createElement("div");
    newDiv.id = "ErrHandler";
    newDiv.focus = function () {
        window.location.replace(href_url);
    }
    document.body.appendChild(newDiv);
}

//Сохраняет параметры склада
function SaveStore(param) {
   cmd('cmd', 'SaveButton', 'param', param);
}

//Добавляет способность изменять расположение inline элементов при отображении страницы в диалоге IE
//SetResizableInDialog.offset = 0;
function SetResizableInDialog() {
    //console.log('SetResizableInDialog');
    //console.log(window.dialogWidth);
    //console.log(window.dialogHeight);

    if (window.dialogWidth && window.dialogHeight) {
        //Ширина вертикальной полосы прокрутки
        //SetResizableInDialog.offset = document.documentElement.offsetWidth - document.documentElement.clientWidth;
        window.addEventListener('resize', function () {
            /*
            var vsb_offset = 0;
            if (window.innerHeight > document.documentElement.clientHeight) {
                //Отображена вертикальная полоса прокрутки
                vsb_offset = SetResizableInDialog.offset;
            }
            console.log(vsb_offset);
            */
            document.documentElement.style.width = document.documentElement.clientWidth + "px"; // vsb_offset + "px"; //window.dialogWidth;
            //console.log(window.dialogWidth + "/" + document.documentElement.clientWidth + "/" + document.documentElement.offsetWidth + "/" + document.documentElement.scrollWidth + "/" + window.innerWidth + " " + window.dialogHeight + "/" + document.documentElement.clientHeight + "/" + document.documentElement.offsetHeight + "/" + document.documentElement.scrollHeight + "/" + window.innerHeight);
            //document.documentElement.style.height = document.documentElement.clientHeight; //window.dialogHeight;
        },
        false);
    }
}

//Получает параметры запроса в виде ассоциативного массива
// Read a page's GET URL variables and return them as an associative array.
function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function ReturnValuePostForm(value, doEscape)
{
    var form = document.getElementById('mvcDialogResult');
    if (!form) return;

    var val = JSON.stringify(value);
    form.action = callbackUrl;
    form.elements["value"].value = doEscape ? escape(val) : val;
    form.elements["escaped"].value = doEscape ? "1" : "0";
    form.elements["control"].value = control;
    form.elements["multiReturn"].value = multiReturn;
    form.submit();
    //setTimeout(function () { parent.closeWindow(); }, 2000);
}

function ReturnValueSetCookie(storeId)
{
    document.cookie = 'DlgRez=1;domain=' + domain + ';path=/';
    document.cookie = 'RetVal=' + storeId + ';domain=' + domain + ';path=/';
    document.cookie = 'ParentAction=0;domain=' + domain + ';path=/';

    try {
        window.returnValue = storeId;
    }
    catch (e) { }

    var version = parseFloat(navigator.appVersion.split('MSIE')[1]);
    if (version < 7)
        window.opener = this;
    else
        window.open('', '_parent', '');
    window.close();
}

function ReturnValue(storeId, storeName)
{
    if (mvc == 1 || mvc == 4) {
        var result = [];

        result[0] = {
            value: storeId,
            label: storeName
        };

        ReturnValuePostForm(result, true);
    }
    else {
        ReturnValueSetCookie(storeId);
    }
}

//Функция устанавливает размеры и положение окна обозревателя
function SetWindowSizePos(x, y, width, height) {

    window.addEventListener('beforeunload', SrvSendWindowSizePos, false);

    var cw = width;
    var ch = height;

    if (window.dialogWidth && window.dialogHeight) {
        window.dialogWidth = cw + "px";
        window.dialogHeight = ch + "px";
    }
    else {
        if (typeof (window.innerWidth) == 'number') {
            // Modern browsers
            cw = width + (window.outerWidth - window.innerWidth);
            ch = height + (window.outerHeight - window.innerHeight);
        } else {
            // IE 8
            cw = width + (document.documentElement.offsetWidth - document.body.offsetWidth);
            ch = height + (document.documentElement.offsetHeight - document.body.offsetHeight);
        }

        window.resizeTo(cw, ch);
    }

    //if (x < 0 || y < 0) {
    //    x = (screen.width / 2) - (cw / 2);
    //    y = (screen.height / 2) - (ch / 2);
    //}

    //window.moveTo(x, y);
}

//Функция передает размеры и положение окна обозревателя на сервер
function SrvSendWindowSizePos() {

    window.removeEventListener('beforeunload', SrvSendWindowSizePos, false);

    var cw = 0;
    var ch = 0;
    if (window.dialogWidth && window.dialogHeight) {
        cw = parseInt(window.dialogWidth, 10);
        ch = parseInt(window.dialogHeight, 10);
    }
    else {
        if (typeof (window.innerWidth) == 'number') {
            // Modern browsers
            cw = window.innerWidth;
            ch = window.innerHeight;
        } else {
            // IE 8
            cw = document.documentElement.clientWidth;
            ch = document.documentElement.clientHeight;
        }
    }

    var winLeft = window.screenLeft ? window.screenLeft : window.screenX;
    var winTop = window.screenTop ? window.screenTop : window.screenY;

    cmd('cmd', 'SaveWindowSizePos', 'x', winLeft, 'y', winTop, 'width', cw, 'height', ch);
}

//Функция добавляет склад в отчёт по складам
//function AddStoreToReport() {
//    cmd('cmd', 'AddStoreToReport');
//}

//Функция очищает все поля формы и результаты поиска
function ClearSearchForm() {
    FilterDescriptionHide();
    //EditFilter();
    cmd('cmd', 'ClearButton');
}

//Функция открывает окно для создания нового склада
function CreateNewStore(params)
{
    var store_url = storesUrl;
    if (params != null && params.length > 0)
        store_url += params;
    var w = v4_windowOpen(store_url, "NewStore", "menubar=no, location=no, resizable=yes, scrollbars=yes, status=no, height=500px, width=600px");
    if (w!=null) w.focus();
}

//Функция открывает окно для редактирования склада
function EditStore(store_id) {
    var store_url = storesUrl + '&id=' + store_id;
    var w = v4_windowOpen(store_url, "_blank", "menubar=no, location=no, resizable=yes, scrollbars=yes, status=no, height=500px, width=600px");
    if (w != null) w.focus();
}

//Функция добавляет склад в отчёт по складам
//function DisplayStoresReport() {
//    cmd('cmd', 'DisplayStoresReport');
//}

//Функция добавляет склад в отчёт указанного типа по складам, вызывается в ответе сервера
function SrvDisplayStoresReport(report_type) {
    var store_url = storeReportUrl + '&StoreRprtType=' + report_type;
    var w = v4_windowOpen(store_url, "NewStore", "menubar=no, location=no, resizable=yes, scrollbars=yes, status=no, height=500px, width=600px");
    if (w != null) w.focus();
}

//Функция оправляет команду для поиска склада
function SearchStore() {
    //FilterDescriptionShow();
    //$("#SearchFilter").hide();
    //$("#editFilter").show();

    Wait.render(true);
    cmdasync('cmd', 'SearchButton');
}

//Функция осуществляет сортировку результатов по указанной колонке
function SortResultTable(column) {
    Wait.render(true);
    cmdasync('cmd', 'SortColumn', 'sort_column', column);
}

//Функция отображает результаты поиска как несоответствующие установленному фильтру
function GrayResultTable() {
    var table = $("#SearchResultTable");
    table.attr('class', 'Grid8Grayed');
    $("#SearchResultTable>thead>tr>th a").contents().unwrap();
    //var input_el = $("#SearchResultTable>tbody>tr>td:eq(0)>input[type=image]");
    //input_el.attr("src", "/Styles/EditGray.gif");
    //input_el.attr("disabled", "disabled");
    //$("#SearchResultTable tr:first").attr('class', 'GridHeaderGrayed');
}

//Функция скрывает или показывает условия поиска
function toggleFilterDescription() {
    if ($("#SearchFilterDescription:visible").length > 0)
        FilterDescriptionHide();
    else
        FilterDescriptionShow();
}

//Функция показывает описание фильтра поиска
function FilterDescriptionShow() {
    $("#SearchFilter").hide();
    $("#toggleFilterDescription").text(StrResources.SrchDisplayFields);
    $("#SearchFilterDescription").show();
    //Эту настройку решено не сохранять
    //Вызов cmdasync приводит к глюкам, создается новый поток для обработки страницы
    //cmd('cmd', 'SaveSrchPageSettings', 'display', '1');
    //cmdasync('cmd', 'SaveSrchPageSettings', 'display', '1');
}

//Функция скрывает описание фильтра поиска
function FilterDescriptionHide() {
    $("#SearchFilter").show();
    $("#toggleFilterDescription").text(StrResources.SrchDisplayDescr);
    $("#SearchFilterDescription").hide();
    //Эту настройку решено не сохранять
    //Вызов cmdasync приводит к глюкам, создается новый поток для обработки страницы
    //cmd('cmd', 'SaveSrchPageSettings', 'display', '0');
    //cmdasync('cmd', 'SaveSrchPageSettings', 'display', '0');
}

//Функция показывает форму редактирования фильтра поиска

/*
function EditFilter() {
    $("#SearchFilter").show();
    $("#editFilter").hide();

    //var checked = $("#display_SearchDescription").prop('checked');
    //if (checked) FilterDescriptionShow();
    //else FilterDescriptionHide();
}
*/


//Функция для уведомления родительского окна (обычно Поиск складов) о создании нового склада
function NotifyParentWindow(id, name) {
    try {
        if (null != window.opener && null != window.opener.OnNewStoreCreated)
            window.opener.OnNewStoreCreated(id, name);
    }
    catch (e) {
    }
}

//Функция обратного вызова, вызывается на странице параметров склада при его сохранении
function OnNewStoreCreated(new_id, name) {
    if (isReturn)
        ReturnValue(new_id, name);
    else {
        if (null != StoreSavedCreatedNotificationCallBack)
            StoreSavedCreatedNotificationCallBack();
    }
}

/*
function OnSetupFormClickOption() {
    var displayId = $(event.target).attr('display_id');
    //$("#" + displayId).toggle();
    if (event.target.checked) $("#" + displayId).show();
    else $("#" + displayId).hide();
}
*/

//Функции добавляет обязательное условие для поиска и всегда отображает его
function AddSrchFormPredicate(pred) {
    var n = SetupForm.requiredSearchPredicates.indexOf(pred);
    if (n > -1) return;

    SetupForm.requiredSearchPredicates.push(pred);
    $("#" + pred_prefix + pred).show();
}

//Функции удаляет условие из списка обязательных
function RemoveSrchFormPredicate(pred) {
    var n = SetupForm.requiredSearchPredicates.indexOf(pred);
    if (n > -1) {
        delete SetupForm.requiredSearchPredicates[n];

        var e = document.getElementById(display_prefix + pred);
        if (e) {
            if (e.checked) $("#" + pred_prefix + pred).show();
            else $("#" + pred_prefix + pred).hide();
        }
    }
}

//Функции восстанавливает отображение полей поиска на странице
function RestoreSrchFormSettings(required, ids) {
    SetupForm.requiredSearchPredicates = required;
    //var ids = settings.split(/[ ,]/);
    $.each(ids, function (index, value) {
        var n = required.indexOf(value);
        if (n < 0) {
            $("#" + pred_prefix + value).hide();
            var e = document.getElementById(display_prefix + value);
            if (e) e.checked = false;
        }
    });
}

//Функции передает на сервер настройки отображения полей поиска на странице
function SaveSrchFormSettings() {

    var settings = '';

    $("#setupForm #setupFormPredicates input:checkbox:checked").each(function () {
        if (settings.length>0) settings += ',';
        var displayId = this.id.substr(display_prefix_len);
        settings += displayId;
    });

    var checked = $("#SearchFilterDescription:visible").length > 0; //$("#display_SearchDescription").prop('checked');

    cmd('cmd', 'SaveSrchPageSettings', 'settings', settings, 'display', checked ? "1" : "0");

    $(this).dialog('close');

    $("#setupForm #setupFormPredicates input:checkbox").each(function () {

        var displayId = this.id.substr(display_prefix_len);
        if (this.checked) $("#" + pred_prefix + displayId).show();
        else $("#" + pred_prefix + displayId).hide();
    });

    //if (checked) FilterDescriptionShow();
    //else FilterDescriptionHide();
}

SetupForm.setup_dialog = null;
SetupForm.requiredSearchPredicates = []; //идентификаторы полей поиска, которые нельзя скрывать используя настройки

//Функция показывает диалог настройки полей поиска
function SetupForm() {

    if (null == SetupForm.setup_dialog) {
        //$("#display_SearchDescription").prop('checked', !$("#SearchFilterDescription").is(':hidden'));
        //var filterElement = $("#SearchFilterDescription");
        //$("#display_SearchDescription").prop('checked', filterElement.length > 0 && filterElement[0].style.display != "none");

        SetupForm.setup_dialog = $("#setupForm").dialog({
            autoOpen: false,
            resizable: false,
            modal: true,
            title: StrResources.SetupFields,
            buttons: [
                { id: 'btnSave', text: StrResources.BtnSave, click: SaveSrchFormSettings },
                { id: 'btnCancel', text: StrResources.BtnCancel, click: function () {
                        $(this).dialog('close');
                    }
                }
            ],
            open: function () { $("#btnSave").focus(); }
        });
    }

    $("#setupForm #setupFormPredicates input:checkbox").each(function () {

        var displayId = this.id.substr(display_prefix_len);

        this.disabled = SetupForm.requiredSearchPredicates.indexOf(displayId) > -1;

        //this.checked = !$("#" + pred_prefix + displayId).is(':hidden');
        var predElement = $("#" + pred_prefix + displayId);
        this.checked = predElement.length > 0 && predElement[0].style.display != "none";

        //$(this).attr('onclick', 'OnSetupFormClickOption();');

        /*
        $(this).attr({
        onclick: 'OnSetupFormClickOption();',
        checked: 'checked'
        });
        */
    });

    SetupForm.setup_dialog.dialog('open');
}