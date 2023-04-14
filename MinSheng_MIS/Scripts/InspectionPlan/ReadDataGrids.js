function initDatagrid_Maintain(data) {
    let dg = new DG();
    dg.init("#DataGrid-Maintain", {
        data: data,
        fit: false,
        rownumbers: false,
        pagination: false,
        idField: 'EMFISN',
        //sortName: 'EMFISN',
        frozenColumns: [[
            dg.frozenColumn('_detail', (v, row, i) => dg.eventButton(i, "詳情", "detail")),
            dg.frozenColumn('_locate', (v, row, i) => {
                let disabled = row.DBID == null || row.DBID == "";
                return dg.eventButton(i, "定位", "locate", { disabled: disabled })
            }),
        ]],
        columns: [[
            dg.column('StockState', '庫存狀態', 120),
            dg.column('FormItemState', '保養項目狀態', 150),
            dg.column('Area', '棟別', 130),
            dg.column('Floor', '樓層', 80),
            dg.column('ESN', '設備編號', 100),
            dg.column('EName', '設備名稱', 220),
            dg.column('EMFISN', '保養單項目編號', 225),
            dg.column('MIName', '保養項目', 200),
            dg.column('Period', '週期', 100),
            dg.column('Unit', '週期單位', 130),
            dg.column('LastTime', '上次保養', 150),
            dg.column('Date', '最近應保養', 150),
        ]]
    })
    dg.addEvent("detail", function (row, i) {
        
        window.open('/MaintainForm_Management/Read/' + row.EMFISN, "_blank");
    })
    dg.addEvent("locate", function (row, i) {
        window.open('/MaintainForm_Management/Read/' + row.EMFISN, "_blank");
    })
}

function initDatagrid_Repair(data) {
    let dg = new DG();
    dg.init("#DataGrid-Repair", {
        data: data,
        fit: false,
        rownumbers: false,
        pagination: false,
        idField: 'EMFISN',
        //sortName: 'EMFISN',
        frozenColumns: [[
            dg.frozenColumn('_detail', (v, row, i) => dg.eventButton(i, "詳情", "detail")),
            dg.frozenColumn('_locate', (v, row, i) => {
                let disabled = row.DBID == null || row.DBID == "";
                return dg.eventButton(i, "定位", "locate", { disabled: disabled })
            }),
        ]],
        columns: [[
            dg.column('StockState', '庫存狀態', 120),
            dg.column('ReportState', '報修單狀態', 150),
            dg.column('Area', '棟別', 130),
            dg.column('Floor', '樓層', 80),
            dg.column('ESN', '設備編號', 100),
            dg.column('EName', '設備名稱', 220),
            dg.column('RSN', '報修單號', 116),
            dg.column('ReportLevel', '報修等級', 95),
            dg.column('Date', '報修時間', 200),
            dg.column('MyName', '報修人員', 110),
            dg.column('ReportContent', '報修內容', 300),
        ]]
    })
    dg.addEvent("detail", function (row, i) {
        window.open('/Report_Management/Read/' + row.RSN, "_blank");
    })
    dg.addEvent("locate", function (row, i) {
        window.open('/Report_Management/Read/' + row.RSN, "_blank");
    })
}