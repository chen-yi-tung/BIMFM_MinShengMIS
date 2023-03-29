

var dg = function () { }

dg.prototype.Options = Object.freeze({
    rownumbers: true,
    remoteSort: false,
    sortOrder: 'asc',
    fit: true,
    singleSelect: true,
    selectOnCheck: true,
    checkOnSelect: false,
    pagination: true,
    pagePosition: 'bottom',
    pageSize: 10
});
dg.prototype.PageOptions = Object.freeze({
    pageSize: 10,
    showPageList: true,
    pageList: [10, 20, 50],
    beforePageText: '第',
    afterPageText: '頁，共 {pages} 頁',
    displayMsg: '顯示 {from} 到 {to} 筆資料，共 {total} 筆資料'
});
dg.prototype.ColumnsOptions = Object.freeze({
    align: 'center',
    sortable: true,
});


dg.prototype.initDataGrid = function (selector, options) {

    const $dg = $(selector);

    Object.assign(options, dg.Options);

    $dg.datagrid({
        url: '',
        method: 'POST',
        queryParams: getQueryParams(),
        idField: 'PSSN',
        sortName: 'PSSN',
        frozenColumns: [[
            {
                field: '_detail', align: 'center',
                formatter: (val, row, index) => {
                    return `<button class="btn btn-datagrid" onclick="onDetail(${index})">詳情</button>`;
                }
            },
            {
                field: '_dispatch', align: 'center',
                formatter: (val, row, index) => {
                    return `<button class="btn btn-datagrid" onclick="onEdit(${index})">編輯</button>`;
                }
            },
            {
                field: '_locate', align: 'center',
                formatter: (val, row, index) => {
                    return `<button class="btn btn-datagrid" onclick="onDelete(${index})">刪除</button>`;
                }
            }
        ]]
    });
    $($dg.datagrid('getPager')).pagination(dgPageOptions);

    return $dg;
}

dg.prototype.frozenColumns = function (field, title, width, formatter) {
    let args = arguments;
    if (args.length === 2 && typeof args[1] === 'function'){
        return { field: field, align: dg.ColumnsOptions.align, formatter: formatter }
    }
    return { field: field, title: title, align: dg.ColumnsOptions.align, width: width, formatter: formatter }
}

dg.prototype.columns = function (field, title, width) {
    return { field: field, title: title, align: dg.ColumnsOptions.align, width: width, sortable: dg.ColumnsOptions.sortable }
}

dg.prototype.hiddenColumns = function (field) {
    return { field: field, hidden: true }
}