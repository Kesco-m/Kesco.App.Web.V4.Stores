
SaveStoresOrder.fDirty = false; //Флаг указывающий, что в таблице сделаны изменения
OnCheckStore.selected_stores = [];//Массив выбранных строк
UpdateResultTable.currentPage = 1;//Номер текущей страницы
UpdateResultTable.pageSize = 0;//Максимальное количество строк на странице
deleteSelected.selected_stores = []; //Массив удаленных строк

//window.addEventListener('beforeunload', SaveStoresOrder, false);
//window.removeEventListener('beforeunload', SaveStoresOrder, false);

//Функция обратного вызова вызываемая после закрытия окна редактирования склада
StoreSavedCreatedNotificationCallBack = function () {
    SaveStoresOrder();

    //Wait.render(true);
    cmd('cmd', 'UpdateTable');
};

//Функция продвигает выбранные строки вверх на rows позиций
function rowsUp(rows) {
    var page_rows = $("#StoreReportTable tbody>tr");
    page_rows.each(function (/*index, row*/) {
        if ($(this).has("td:has(input:checkbox:checked)").length) {

            SaveStoresOrder.fDirty = true;

            var n = rows;
            var el = $(this);
            var before = null;
            while (n > 0 && el.length) {
                el = el.prev();
                if (el.has("td:has(input:checkbox:not(:checked))").length) {
                    --n;
                    before = el;
                }
            }

            if (before)
                $(this).insertBefore(before);
        }
    });

    UpdateMoveButtons();
}

//Функция продвигает выбранные строки вниз на rows позиций
function rowsDown(rows) 
{
    var page_rows = $("#StoreReportTable tbody>tr");
    var last_pos = page_rows.length - 1;
    if (last_pos < 0) return;

    var selected_on_page = 0;
    for (var i = last_pos; i >= 0; --i) {
        var current = $(page_rows[i]);
        if (current.has("td:has(input:checkbox:checked)").length) {

            SaveStoresOrder.fDirty = true;

            var n = rows;
            var el = current;
            var after = null;
            while (n > 0 && el.length) {
                el = el.next();
                if (el.has("td:has(input:checkbox:not(:checked))").length) {
                    --n;
                    after = el;
                }
            }

            if (after) {
                current.insertAfter(after);
                selected_on_page++;
            }
        }
    }

    UpdateMoveButtons();
}

function HasChanges() {
    //if (deleteSelected.selected_stores.length > 0) return true;
    //if (SaveStoresOrder.fDirty) return true;
    //return SaveStoresOrder.fDirty && deleteSelected.selected_stores.length > 0;

    var table = document.getElementById('StoreReportTable');
    var rows = table.tBodies[0].rows;
    var last_pos = rows.length;
    if (last_pos < 0) return false;
    if (last_pos != UpdateResultTable.pageSize) return true;//строки удалены

    //var is_rows_deleted = page_rows[page_rows.length - 1].sectionRowIndex - page_rows[0].sectionRowIndex + 1 == page_rows.length

    for (var i = 0; i<last_pos; i++) {
        if (rows[i].cells[1].innerText != rows[i].sectionRowIndex + 1/*i+1*/) return true; //строки перемещены
    }

    //return page_rows.length > 0
    //    && page_rows[page_rows.length - 1].sectionRowIndex - page_rows[0].sectionRowIndex + 1 == page_rows.length
    //    && page_rows[page_rows.length - 1].sectionRowIndex == table.tBodies[0].rows.length - 1;
    return false;
}

function CancelStoresOrder() {

    var prev_save = CustomConfirmChangedTwoButtons.save;
    var prev_cancel = CustomConfirmChangedTwoButtons.cancel;
    CustomConfirmChangedTwoButtons.cancel = function () { prev_cancel(); CustomConfirmChangedTwoButtons.save = prev_save; CustomConfirmChangedTwoButtons.cancel = prev_cancel; };

    CustomConfirmChangedTwoButtons.save = function () {
        prev_save('');
        //location.href = location.href;
        //window.location.replace(window.location.href);

        deleteSelected.selected_stores = [];
        OnCheckStore.selected_stores = [];

        //Wait.render(true);
        cmd('cmd', 'UpdateTable');

        ClearSelectedStores();

        CustomConfirmChangedTwoButtons.save = prev_save;
        CustomConfirmChangedTwoButtons.cancel = prev_cancel;
    };

    if (HasChanges()) {
        CustomConfirmChangedTwoButtons.render('', StrResources.MsgCancelConfirm, StrResources.BtnYes, StrResources.BtnNo);
    }
    else
        CustomConfirmChangedTwoButtons.save();
}

function CloseStoresOrder() {
    if (HasChanges()) {
        ConfirmExit.render('', StrResources.MsgCloseConfirm, StrResources.BtnYes, StrResources.BtnNo);
    }
    else
        ConfirmExit.yes();
}

function ConfirmSaveStoresOrder(report_type) {

    var prev_save = CustomConfirmChangedTwoButtons.save;
    var prev_cancel = CustomConfirmChangedTwoButtons.cancel;

    CustomConfirmChangedTwoButtons.cancel = function () {
        prev_cancel();
        CustomConfirmChangedTwoButtons.save = prev_save;
        CustomConfirmChangedTwoButtons.cancel = prev_cancel;
    };

    CustomConfirmChangedTwoButtons.save = function () {

        prev_save('');

        SaveStoresOrder(report_type);

        CustomConfirmChangedTwoButtons.save = prev_save;
        CustomConfirmChangedTwoButtons.cancel = prev_cancel;
    };

    if (HasChanges()) {
        CustomConfirmChangedTwoButtons.render('', StrResources.MsgSaveConfirm, StrResources.BtnYes, StrResources.BtnNo);
    }
    else
        CustomConfirmChangedTwoButtons.save();
}

//Функция сохраняет порядок строк на текущей странице
function SaveStoresOrder(report_type) {
    if (deleteSelected.selected_stores.length > 0) {

        var store_ids = '';
        deleteSelected.selected_stores.forEach(function (item, i, arr) {
            if (store_ids.length) store_ids += ", ";
            store_ids += item[0];
        });

        if (store_ids.length)
            cmd('cmd', 'RemoveStores', 'stores', store_ids);

        deleteSelected.selected_stores = [];
    }

    if (!SaveStoresOrder.fDirty) return;

    var rows = $("#StoreReportTable tbody>tr input:checkbox");
    store_ids = [];
    rows.each(function (index, row) {
        store_ids.push(this.id);
    });

    //Wait.render(true);
    cmd('cmd', 'ApplyStoresOrder', 'stores', store_ids.toString(), 'at_bottom', '0', 'page', UpdateResultTable.currentPage, 'page_size', UpdateResultTable.pageSize, 'report_type', report_type);

    SaveStoresOrder.fDirty = false;
}

//Функция список выбранных складов через запятую
//index - индекс поля отображаемого элемента в массиве
function get_selected_stores(index) {

    //не работает с ассоциативными массивами
    var stores = OnCheckStore.selected_stores.slice();
    stores.sort(function (item1, item2) {
        return item1[1] - item2[1];
    });

    var selcount = 0;
    var store_ids = '';
    stores.forEach(function (item, i, arr) {
        if (store_ids.length) store_ids += ", ";
        if (index < 2)
            store_ids += item[index];
        else store_ids += "["+(item[1]+ 1) +"] " + item[index];

        selcount++;
    });

    return { text: store_ids, count: selcount };
}

//Функция снимает выбор со всех строк
function ClearSelectedStores() {

    /*
    Вызывается только после загрузки таблицы
    var rows = $("#StoreReportTable tbody>tr:has(td input:checkbox)");
    rows.each(function () {
    var checkbox = $("input:checkbox", this.cells[0])[0];
    checkbox.checked = false;
    });
    */

    deleteSelected.selected_stores = [];
    OnCheckStore.selected_stores = [];
    $("#SelectedStores").text('');
    $("#SelectedStoresGroup").hide();

    //Выбор складов указанных в параметрах запроса
    var fn_forEach = function (item, i, arr) {
        var checkbox = document.getElementById(item);
        if (checkbox) {
            checkbox.checked = true;
        }
    }

    var url_params = getUrlVars();
    var selected_stores_param = url_params["selected"]

    if(selected_stores_param)
    {
        var selected_stores = selected_stores_param.split(',');
        selected_stores.forEach(fn_forEach);

        OnCheckStore();
    }
}

//Функция вызываемая при изменении типа отчета и обновлении списка складов в отчете
function UpdateReportType(report_type) {
    ClearSelectedStores();
}

//Функция вызываемая при обновлении списка складов в отчете, восстанавливает выделение
function UpdateResultTable(page, size) {
    UpdateResultTable.currentPage = page;
    UpdateResultTable.pageSize = size;

    var fn_forEach = function (item, i, arr) {
        var checkbox = document.getElementById(item[0]);// document.getElementById(i); //для OnCheckStore.selected_stores i = store_id
        if (checkbox) {
            checkbox.checked = true;

            //var row = $(checkbox).parents("#StoreReportTable tbody>tr");
            //row.addClass("checked_row");
        }
    }

    deleteSelected.selected_stores = [];

    OnCheckStore.selected_stores.forEach(fn_forEach);

    OnCheckStore();
}

//Функция выбирает все строки на странице
//function OnSelectAll() {
//    $("#StoreReportTable tbody>tr td:nth-child(1) input:checkbox").prop('checked', $(event.target).prop('checked'));
//    OnCheckStore();
//}

//При выборе или отмене выбора проверяются все строки
//возможно, был изменен порядок строк и необходимо изменить порядок в массиве OnCheckStore.selected_stores
function OnCheckStore() {

    if (UpdateResultTable.currentPage < 1) return;

    var rows = $("#StoreReportTable tbody>tr:has(td input:checkbox)");

    rows.each(function () {

        var order = (UpdateResultTable.currentPage - 1) * UpdateResultTable.pageSize + $(this).index();

        var checkbox = $("input:checkbox", this.cells[0])[0];
        var store_id = checkbox.id;

        //Проверка присутствия элемента в массиве
        for (var i = 0; i < OnCheckStore.selected_stores.length; i++) {
            if (OnCheckStore.selected_stores[i] && OnCheckStore.selected_stores[i][0] == store_id) {
                break;
            }
        }

        if (checkbox.checked) {
            if (i >= OnCheckStore.selected_stores.length) {
                //После копирования и сортировки в get_selected_stores() индексирование в копии массива по store_id теряется
                //индексы элементов будут последовательные числа
                //поэтому дополнительно сохраняем store_id как элемент массива
                OnCheckStore.selected_stores.push([store_id, order, this.cells[6].innerText]);
                //не работает с ассоциативными массивами
                //OnCheckStore.selected_stores["_" + store_id] = [store_id, order, this.cells[6].innerText];
            }

            $(this).addClass("checked_row");
        }
        else {
            if (i < OnCheckStore.selected_stores.length)
                delete OnCheckStore.selected_stores[i];

            //не работает с ассоциативными массивами
            //delete OnCheckStore.selected_stores[store_id];
            $(this).removeClass("checked_row");
        }
    });

    var selstores = get_selected_stores(2);
    $("#SelectedStores").text(selstores.text);

    if (selstores.count > 0)
        $("#SelectedStoresGroup").show();
    else
        $("#SelectedStoresGroup").hide();

    //Подготовка Drad and Drop
    $('.ui-draggable').draggable('destroy');

    var droppable_obj = $(document.body);
    var checked_rows = $("#StoreReportTable tbody>tr:has(td input:checkbox:checked)");

    var dragged_table = $("#StoreReportTable");

    if (checked_rows.length > 0) {
        droppable_obj.droppable({ accept: dragged_table, scope: "report", tolerance:"touch", drop: onDropDraggedRows});

        //var checked_rows_height = 50;//добавляем небольшую область для возможности сброса строк и отмены операции
        //Не требуется если временно скрывать hide() переносимые строки
        //checked_rows.each(function (index, row) {
        //    checked_rows_height += $(this).height();
        //});
        
        var droppable_el = $("#StoreReport");
        var drag_area = [droppable_el.offset().left, droppable_el.offset().top, droppable_el.offset().left + droppable_el.width(), droppable_el.offset().top + droppable_el.height()/* + checked_rows_height*/];
        dragged_table.draggable({ appendTo: droppable_el,
            axis: "y",
            handle: checked_rows,
            helper: getDragHelper,
            containment: drag_area,
            revert: "invalid",
            scope: "report",
            //snap: "#StoreReportTable tbody",//Не очень хорошо действует, когда выбраны почти все строки в таблице, нарушается разметка
            start: function (e, ui) {
                checked_rows.hide();
            },
            stop: function (e, ui) {
                checked_rows.show();
            }
        });
    }
    else {
        $('.ui-droppable').droppable('destroy');
    }
    /////////////////////////////////////////

    UpdateMoveButtons();
}

//Функция для операции перетаскивания складов
function onDropDraggedRows(event, ui)
{
    var pos = ui.position;

    var tbody = $("#StoreReportTable tbody");
    var page_rows = $("tr", tbody);
    var dragged_rows = $("tr:has(td input:checkbox:checked)", tbody);

    SaveStoresOrder.fDirty = dragged_rows.length > 0;

    if (page_rows.length > 0) {
        for (var i = 0; i < page_rows.length; ++i) {
            var current = $(page_rows[i]);
            if (current.position().top + current.height() / 2 >= pos.top) {
                dragged_rows.insertBefore(current);
                break;
            }
        }

        //Последняя строка проверяется отдельно
        if (i >= page_rows.length)
        {
            var last_tr = page_rows.last();
            dragged_rows.each(function(index, row)
                {
                    $(this).insertAfter(last_tr);
                    last_tr =$(this);
                });
        }
    }
    else
        $(this).append(dragged_rows);

    UpdateMoveButtons();
}

//Функция для операции перетаскивания складов
function getDragHelper(event) {
    //event - mousedown event [undocumented]
    var dragged_rows = $("#StoreReportTable tbody>tr:has(td input:checkbox:checked)");

    //var borders = $( "#StoreReportTable" ).css([ "borderTopWidth", "borderBottomWidth"]);
    //var borders_offset = parseFloat(borders["borderTopWidth"])+parseFloat(borders["borderBottomWidth"]);
    //if(borders_offset==0) borders_offset=1;//1 - default border

    //Установка курсора
    var clicked_rows = $(event.target).closest('tr');
    if(clicked_rows.length>0)
    {
        var clicked_row = clicked_rows[0];//event.target.parentElement;
        var top_offset = 0;
        for (var i = 0; i < dragged_rows.length; ++i) {
            //this- HTMLTableRowElement
            if (dragged_rows[i].sectionRowIndex >= clicked_row.sectionRowIndex) {
                $('.ui-draggable').draggable( "option", "cursorAt", { top:  top_offset + event.offsetY} );
                break;
            }    

            top_offset+=dragged_rows[i].offsetHeight;//+2*borders_offset;
        }
    }

    var rows = dragged_rows.clone();
    var helper_elem = $('<table width="100%" class="grid"><tbody></tbody></table>');
    $("tbody", helper_elem).append(rows);

    return helper_elem;
}

//Функция обновляет отображение кнопок вверх-вниз
function UpdateMoveButtons() {
    var checked_rows = $("#StoreReportTable tbody>tr:has(td input:checkbox:checked)");

    var disable_up = true;
    for(var i=0; i<checked_rows.length && disable_up; i++ )
        disable_up = $(checked_rows[i]).prev().has("td input:checkbox:not(:checked)").length < 1;

    var disable_down = true;
    for(var i=0; i<checked_rows.length && disable_down; i++ )
        disable_down = $(checked_rows[i]).next().has("td input:checkbox:not(:checked)").length < 1;

    var src_down_path = disable_down ? "/STYLES/DownGrayed.gif" : "/STYLES/Down.gif";
    var src_up_path = disable_up ? "/STYLES/UpGrayed.gif" : "/STYLES/Up.gif";

    var href_void = "javascript: void;";

    var to_last = $('#moveLast');
    var to_first = $('#moveFirst');
    var del_selected = $('#deleteSelected');
    var insert_after = $('#inserAfterSelected');
    var insert_arter_edit = $('#insertAfterRow');

    if (checked_rows.length<1) {
        del_selected.attr("href", href_void);
        del_selected[0].className = "disabled_action_link";
    }
    else {
        del_selected.attr("href", "javascript: deleteSelected();");
        del_selected[0].className = "action_link";
    }

    if (disable_down && disable_up) {
        insert_after.attr("href", href_void);
        insert_after[0].className = "disabled_action_link";
        insert_arter_edit[0].disabled = true;
    }
    else {
        insert_after.attr("href", "javascript: setSelectedRowsAfter();");
        insert_after[0].className = "action_link";
        insert_arter_edit[0].disabled = false;
    }

    if (disable_down) {
        to_last.attr("href", href_void);
        to_last[0].className = "disabled_action_link";
    }
    else {
        to_last.attr("href", "javascript: rowsDown(UpdateResultTable.pageSize);");
        to_last[0].className = "action_link";
    }

    if(disable_up) {
        to_first.attr("href", href_void);
        to_first[0].className = "disabled_action_link";
    }
    else{
        to_first.attr("href", "javascript: rowsUp(UpdateResultTable.pageSize);");
        to_first[0].className = "action_link";
    }

    $('#selectionUp').attr("disabled", disable_up);
    $('#selectionUp').attr("src", src_up_path);

    $('#selectionDown').attr("disabled", disable_down);
    $('#selectionDown').attr("src", src_down_path);
}

//Функция удаляет выбранные склады
function deleteSelected() {
    var checked_rows = $("#StoreReportTable tbody>tr:has(td input:checkbox:checked)");
    if (checked_rows.length < 1) return;

    var prev_save = CustomConfirmChangedTwoButtons.save;
    var prev_cancel = CustomConfirmChangedTwoButtons.cancel;
    CustomConfirmChangedTwoButtons.cancel = function () { prev_cancel(); CustomConfirmChangedTwoButtons.save = prev_save; CustomConfirmChangedTwoButtons.cancel = prev_cancel; };

    CustomConfirmChangedTwoButtons.save = function () {

        checked_rows.remove();

        /*
        var table = document.getElementById('StoreReportTable');
        checked_rows.each(function (index, row) {
        table.tBodies[0].removeChild(this)
        });
        */

        deleteSelected.selected_stores = deleteSelected.selected_stores.concat(OnCheckStore.selected_stores);

        OnCheckStore.selected_stores = [];

        OnCheckStore();

        prev_save('');

        SaveStoresOrder.fDirty = true;//Что бы происходила перезагрузка списка при нажатии на Save

        CustomConfirmChangedTwoButtons.save = prev_save;
        CustomConfirmChangedTwoButtons.cancel = prev_cancel;
    };

    CustomConfirmChangedTwoButtons.render('', StrResources.MsgDeleteSelectedStoresConfirm, StrResources.BtnYes, StrResources.BtnNo);

    //if(!confirm(StrResources.MsgDeleteSelectedStoresConfirm)) return;

    //SaveStoresOrder();

    //Wait.render(true);
    //cmd('cmd', 'RemoveStores', 'stores', get_selected_stores(0).text);
}

function onInsertAfterKeyDown() {
    if (13 == event.keyCode)//Enter Key
        return setSelectedRowsAfter();
}

//Функция перемещает выбранные склады после указанной в поле ввода строки
function setSelectedRowsAfter() {

    //При вводе пустой строки лучше ничего не делать
    var string_value = insertAfterRow.value.trim();
    if (string_value.length < 1) return;

    var table = document.getElementById("StoreReportTable");
    if (null == table) return;

    var rows = table.tBodies[0].rows;
    if(rows<1) return;

    var num = Number(string_value);

    var byName = false;

    if(isNaN(num) || num>rows.length)
    {
        byName = true;
        string_value = string_value.toLowerCase()

        var name_prefix = "\"";
        if(string_value.indexOf(name_prefix) == 0)
        {
            //Если присутствует prefix то убираем и суффикс
            var last_index = string_value.lastIndexOf(name_prefix);
            if(last_index==string_value.length-name_prefix.length)
                string_value = string_value.substr(name_prefix.length, last_index-1);
            else
                string_value = string_value.substr(name_prefix.length);
        }
    }

    var checked_rows = $("tbody>tr:has(td input:checkbox:checked)", table);

    SaveStoresOrder.fDirty = checked_rows.length>0;

    var pred = num < 1;
    var fn_pred = function(strn){return num == strn};
    var cell_index = 1;

    if(byName)
    {
        pred =string_value.length<1;
        fn_pred = function(str){return string_value == str.toLowerCase()};
        cell_index = 6;
    }

    if(pred)
    {
        //checked_rows.insertBefore(rows[0])//не корректно работает

        var first_tr = rows[0];
        for(var i=checked_rows.length-1; i>=0; --i)
        {
            $(checked_rows[i]).insertBefore(first_tr);
            first_tr = checked_rows[i];
        }
    }
    else
    {
        for (var i = 0; i < rows.length; i++) {
            if (fn_pred(rows[i].cells[cell_index].innerText)) {
                var last_tr = rows[i];
                checked_rows.each(function (index, row) {
                    $(this).insertAfter(last_tr);
                    last_tr = this;
                });

                break;
            }
        }

        if(i >= rows.length)
        {
            alert(StrResources.MsgRowNotFound);
        }
    }

    UpdateMoveButtons();
}