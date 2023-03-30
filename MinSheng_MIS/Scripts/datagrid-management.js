function DG() { return this }

DG.prototype.Options = Object.freeze({
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
DG.prototype.PageOptions = Object.freeze({
    pageSize: 10,
    showPageList: true,
    pageList: [10, 20, 50],
    beforePageText: '第',
    afterPageText: '頁，共 {pages} 頁',
    displayMsg: '顯示 {from} 到 {to} 筆資料，共 {total} 筆資料'
});
DG.prototype.ColumnsOptions = Object.freeze({
    align: 'center',
    sortable: true,
});

DG.prototype.init = function (selector, options) {

    const $dg = $(selector);
    $dg.datagrid(Object.assign({}, this.Options, options));
    $($dg.datagrid('getPager')).pagination(this.PageOptions);

    return $dg;
}

DG.prototype.event = {};

DG.prototype.addEvent = function (key, value) {
    this.event[key] = value;
}

DG.prototype.removeEvent = function (key) {
    delete this.event[key];
}

DG.prototype.frozenColumn = function (/* field, title, width, formatter */) {
    let args = arguments;
    if (args.length === 2 && typeof args[1] === 'function') {
        return { field: args[0], align: this.ColumnsOptions.align, formatter: args[1] }
    }
    else if (args.length === 3 && typeof args[2] === 'function') {
        return { field: args[0], title: args[1], align: this.ColumnsOptions.align, formatter: args[2] }
    }
    else if (args.length === 4 && typeof args[3] === 'function') {
        return { field: args[0], title: args[1], align: this.ColumnsOptions.align, width: args[2], formatter: args[3] }
    }
}

DG.prototype.column = function (/* field, title, width */) {
    let args = arguments;
    if (args.length === 1) {
        return { field: args[0], title: args[0], align: this.ColumnsOptions.align, sortable: this.ColumnsOptions.sortable }
    }
    else if (args.length === 2) {
        return { field: args[0], title: args[1], align: this.ColumnsOptions.align, sortable: this.ColumnsOptions.sortable }
    }
    else if (args.length === 3) {
        return { field: args[0], title: args[1], align: this.ColumnsOptions.align, width: args[2], sortable: this.ColumnsOptions.sortable }
    }
}

DG.prototype.formatColumn = function (/* field, title, width, formatter */) {
    let args = arguments;
    if (args.length === 3) {
        return { field: args[0], title: args[1], align: this.ColumnsOptions.align, sortable: this.ColumnsOptions.sortable, formatter: args[2] }
    }
    else if (args.length === 4) {
        return { field: args[0], title: args[1], align: this.ColumnsOptions.align, width: args[2], sortable: this.ColumnsOptions.sortable, formatter: args[3] }
    }
}

DG.prototype.hiddenColumn = function (field) {
    return { field: field, hidden: true }
}

var dg = new DG();