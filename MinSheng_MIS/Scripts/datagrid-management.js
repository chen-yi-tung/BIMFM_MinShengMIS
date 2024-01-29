function DG() {
    this.Options = DG.prototype.Options;
    this.PageOptions = DG.prototype.PageOptions;
    this.ColumnsOptions = DG.prototype.ColumnsOptions;
    this.event = {};
    return this
}

DG.prototype.Options = {
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
};
DG.prototype.PageOptions = {
    pageSize: 10,
    showPageList: true,
    pageList: [10, 20, 50],
    beforePageText: '第',
    afterPageText: '頁，共 {pages} 頁',
    displayMsg: '顯示 {from} 到 {to} 筆資料，共 {total} 筆資料'
};
DG.prototype.ColumnsOptions = {
    align: 'center',
    sortable: true,
};

DG.prototype.init = function (selector, options) {
    const self = this;
    const $dg = $(selector);
    const op = Object.assign({}, this.Options, options);
    this.Options = op;
    $dg.datagrid(op);
    op.pagination && $($dg.datagrid('getPager')).pagination(this.PageOptions);

    $($dg.datagrid("getPanel")).on("click", "button[data-btn-type]:not([data-btn-type='null'])", function () {
        let btn = $(this);
        let index = +(btn.attr("data-index"));
        let type = btn.attr("data-btn-type");
        let row = $dg.datagrid('getRows')[index];
        self.event[type](row, index);
    })

    return $dg;
}

DG.prototype.addEvent = function (key, value) {
    this.event[key] = value;
}

DG.prototype.removeEvent = function (key) {
    delete this.event[key];
}

DG.prototype.frozenColumn = function (/* field, title, width, formatter */) {
    let args = [].slice.call(arguments);
    let res = { field: args[0], width: 74, align: this.ColumnsOptions.align, formatter: args.at(-1) };
    switch (args.length) {
        case 4: res.width = args[2];
        case 3: res.title = args[1];
    }
    return res;
}

DG.prototype.column = function (/* field, title, width */) {
    let args = [].slice.call(arguments);
    let res = { field: args[0], align: this.ColumnsOptions.align, sortable: this.ColumnsOptions.sortable };
    switch (args.length) {
        case 4: res.formatter = args[3];
        case 3: res.width = args[2];
        case 2: res.title = args[1];
    }
    return res;
}

DG.prototype.formatColumn = function (/* field, title, width, formatter */) {
    let args = [].slice.call(arguments);
    let res = { field: args[0], align: this.ColumnsOptions.align, sortable: this.ColumnsOptions.sortable, formatter: args.at(-1) };
    switch (args.length) {
        case 4: res.width = args[2];
        case 3: res.title = args[1];
    }
    return res;
}

DG.prototype.hiddenColumn = function (field) {
    return { field: field, hidden: true }
}

DG.prototype.eventButton = function (index, text, eventName = null, options = null) {
    let optionStr = [];

    let className = "btn btn-datagrid";
    if (options) {
        options.className && options.className.length !== 0 && (className = options.className)
        options.disabled === true && optionStr.push("disabled")
        options.hidden === true && optionStr.push("hidden")
    }
    return `<button class="${className}" data-index="${index}" data-btn-type="${eventName ?? 'null'}" ${optionStr.join(" ")}>${text}</button>`;
}