function createTableInner(data, sn) {
    const nullString = "-";
    return sn.map((e) => {
        let html;
        switch (e.value) {
            case "ImgPath":
                html = `
                    <tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" id="d-${e.value}">${data[e.value] && data[e.value].length !== 0 ? putImage(data[e.value]) : nullString}</td>
                    </tr>`;
                break;
            case "FilePath":
                html = `
                    <tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" id="d-${e.value}">${data[e.value] && data[e.value].length !== 0 ? putFile(data[e.value]) : nullString}</td>
                    </tr>`;
                break;
            default:
                if (e.formatter) {
                    html = `
                        <tr>
                            <td class="datatable-table-th">${e.text}</td>
                            <td class="datatable-table-td" id="d-${e.value}">${e.formatter(data[e.value])}</td>
                        </tr>`;
                }
                else {
                    html = `
                        <tr>
                            <td class="datatable-table-th">${e.text}</td>
                            <td class="datatable-table-td" id="d-${e.value}">${data[e.value] ?? nullString}</td>
                        </tr>`;
                }
                break;
        }
        return html;
    }).join("");

    function putImage(imgs) {
        let div = `<div class="datatable-img-area">${imgs.map(img => {
            return `<div class="datatable-img-item"><img src="${img}"/></div>`;
        }).join("")}
        </div>`;
        return div;
    }

    function putFile(urls) {
        let div = urls.map(url => { return `<a href="${url}" target="_blank">${url.split('/').at(-1)}</a>` }).join("<br>");
        return div;
    }
}

function createTableGrid(data, options) {
    const nullString = "-";
    console.log(options)
    let columns = options.columns;
    let thead = options.thead == true ? `<thead><tr>${createThs(columns)}</tr></thead>` : "";
    let tbody = `<tbody>${createTrs(columns, data)}</tbody>`;
    return thead + tbody;
    function createThs(op) {
        return op.map(o => {
            let w = typeof o.width == "string" ? o.width : o.width + "px";
            let th = `<th class="datatable-header" style="width:${w}">${o.title}</th>`;
            return th;
        }).join("");
    }

    function createTrs(op, ds) {
        return ds.map((d, i) => {
            let tr = `<tr id="${i}">${createTds(op, d, i)}</tr>`;
            return tr;
        }).join("");
    }

    function createTds(op, d, i) {
        return op.map((o) => {
            let td
            if (o.formatter) {
                td = `
                <td id="d-${o.id}">
                    ${o.formatter(d[o.id], d, i)}
                </td>`;
            }
            else {
                td = `<td id="d-${o.id}">${d[o.id] ?? nullString}</td>`;
            }
            return td;
        }).join("");
    }
}

function createAccordionItem(options, i) {
    return `
    <div class="accordion-item" id="${options.id}-${i}">
        <h2 class="accordion-header" id="header-${options.id}-${i}">
            <button class="accordion-button collapsed" type="button"
                data-bs-toggle="collapse"
                data-bs-target="#body-${options.id}-${i}" aria-expanded="false"
                aria-controls="body-${options.id}-${i}">
                ${options.data[i][options.itemTitleKey]}
            </button>
        </h2>
        <div id="body-${options.id}-${i}" class="accordion-collapse collapse"
            aria-labelledby="header-${options.id}-${i}">
            <div class="accordion-body">
                <div class="datatable border-0 w-100">
                    <div class="datatable-body">
                        <table class="datatable-table">
                            ${createTableInner(options.data[i], options.sn)}
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
    `;
}

function putData_InspectionPlan(data = null, sn = null, i = 0) {

    sn == null ? sn = {
        InspectionPlan: [
            { text: "計畫編號", value: "IPSN" },
            { text: "計畫名稱", value: "IPName" },
            { text: "計畫日期", value: "PlanDate" },
            { text: "計畫執行狀態", value: "PlanState" },
            { text: "巡檢班別", value: "Shift" },
            { text: "巡檢人員", value: "MyName" },
        ],
        InspectionPlanRepair: [
            { text: "本次維修狀態", value: "RepairState" },
            { text: "維修備註", value: "RepairContent" },
            { text: "填報人員", value: "MyName" },
            { text: "填報時間", value: "RepairDate" },
            { text: "維修照片", value: "ImgPath" },
        ],
        RepairSupplementaryInfo: [
            { text: "補件人", value: "MyName" },
            { text: "補件日期", value: "SupplementaryDate" },
            { text: "補件說明", value: "SupplementaryContent" },
            { text: "補件檔案", value: "FilePath" },
        ],
        RepairAuditInfo: [
            { text: "審核者", value: "MyName" },
            { text: "審核日期", value: "AuditDate" },
            { text: "審核結果", value: "AuditResult" },
            { text: "審核意見", value: "AuditMemo" },
            { text: "審核照片", value: "ImgPath" },
        ]
    } : sn;

    data == null ? data = {
        InspectionPlan: {
            IPSN: "P23010401",
            IPName: "巡檢A區計畫",
            PlanDate: "2023/1/8",
            PlanState: "巡檢完成",
            Shift: "早班",
            MyName: "王大明、李泰順",
        },
        InspectionPlanRepair: {
            RepairState: "完成",
            RepairContent: "修復完成",
            MyName: "王大明",
            RepairDate: "2023/1/7 12:40:56",
            ImgPath: [
                "/Content/img/bg.png",
                "/Content/img/bg.png",
                "/Content/img/bg.png",
                "/Content/img/bg.png",
            ],
        },
        RepairSupplementaryInfo: [
            {
                MyName: "王大明",
                SupplementaryDate: "2023/1/15",
                SupplementaryContent: "OOXX",
                FilePath: ["/file/電燈故障.pdf", "/file/電燈故障2.pdf"],
            }
        ],
        RepairAuditInfo: [
            {
                MyName: "陳組長",
                AuditDate: "2023/1/15",
                AuditResult: "審核未過",
                AuditMemo: "未處理完善",
                ImgPath: [
                    "/Content/img/bg.png",
                    "/Content/img/bg.png",
                    "/Content/img/bg.png",
                    "/Content/img/bg.png",
                ],
            }
        ],
    } : data;

    let title = `${data.InspectionPlan.IPSN} ${data.InspectionPlan.IPName}`;
    return `
    <div class="accordion-item" id="InspectionPlan-${i}">
        <h2 class="accordion-header" id="header-InspectionPlan-${i}">
            <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse"
                data-bs-target="#body-InspectionPlan-${i}" aria-expanded="false" aria-controls="body-InspectionPlan-${i}">
                ${title}
            </button>
        </h2>
        <div id="body-InspectionPlan-${i}" class="accordion-collapse collapse" aria-labelledby="header-InspectionPlan-${i}">
            <div class="accordion-body">
                ${createItem("計劃資訊", "InspectionPlan", createTableInner(data.InspectionPlan, sn.InspectionPlan))}
                ${createItem("維修資料", "InspectionPlanRepair", createTableInner(data.InspectionPlanRepair, sn.InspectionPlanRepair))}
                ${data.RepairSupplementaryInfo.length !== 0 ? createAccordion({
        title: "補件資料",
        id: "RepairSupplementaryInfo",
        sn: sn.RepairSupplementaryInfo,
        data: data.RepairSupplementaryInfo,
        itemTitleKey: "SupplementaryDate"
    }) : ""}
                ${data.RepairAuditInfo.length !== 0 ? createAccordion({
        title: "審核資料",
        id: "RepairAuditInfo",
        sn: sn.RepairAuditInfo,
        data: data.RepairAuditInfo,
        itemTitleKey: "AuditDate"
    }) : ""}
            </div>
        </div>
    </div>
    `;

    function createItem(title, id, inner) {
        return `
        <div class="datatable border-0 w-100">
            <div class="datatable-header">${title}</div>
            <div class="datatable-body">
                <table class="datatable-table" id="${id}">
                    ${inner}
                </table>
            </div>
        </div>
        `;
    }

    function createAccordion(options) {
        return `
        <div class="datatable border-0 w-100">
            <div class="datatable-header">${options.title}</div>
            <div class="datatable-body">
                <div class="accordion accordion-flush datatable-accordion" id="accordion-${options.id}">
                    ${options.data.map((d, i) => {
            return createAccordionItem(options, i)
        }).join("")}
                </div>
            </div>
        </div>
        `;
    }


}

function putData_MaintainRecord(data = null, sn = null) {
    sn == null ? sn = {
        EquipmentMaintainFormItem: [
            { text: "本項目保養狀態", value: "FormItemState" },
            { text: "保養單項目編號", value: "EMFISN" },
            { text: "保養項目", value: "MIName" },
            { text: "保養週期單位", value: "Unit" },
            { text: "保養週期", value: "Period" },
            { text: "上次保養", value: "LastTime" },
            { text: "最近應保養", value: "Date" },
            {
                text: "設備屬性", value: "ESN", formatter: (val) => {
                    return val ? `<a class="btn btn-search" href="${val}" target="_blank">設備資料</a>` : "-"
                }
            },
        ],
        InspecitonPlan: [
            { text: "計畫編號", value: "IPSN" },
            { text: "計畫名稱", value: "IPName" },
            { text: "計畫日期", value: "PlanDate" },
            { text: "計畫執行狀態", value: "PlanState" },
            { text: "巡檢班別", value: "Shift" },
            { text: "巡檢人員", value: "MyName" },
        ],
        InspectionPlanMaintain: [
            { text: "本次保養狀態", value: "MaintainState" },
            { text: "填報人員", value: "MyName" },
            { text: "保養備註", value: "MaintainContent" },
            { text: "填報時間", value: "MaintainDate" },
            { text: "保養照片", value: "ImgPath" },
        ],
        MaintainSupplementarInfo: [
            { text: "補件人", value: "MyName" },
            { text: "補件日期", value: "SupplementaryDate" },
            { text: "補件說明", value: "SupplementaryContent" },
            { text: "補件檔案", value: "FilePath" },
        ],
        MaintainAuditInfo: [
            { text: "審核者", value: "MyName" },
            { text: "審核日期", value: "AuditDate" },
            { text: "審核結果", value: "AuditResult" },
            { text: "審核意見", value: "AuditMemo" },
            { text: "審核照片", value: "ImgPath" },
        ]
    } : sn;

    data == null ? data = {
        EquipmentMaintainFormItem: {
            FormItemState: "完成",
            EMFISN: "E00003_000001_230107",
            MIName: "軸承潤滑",
            Unit: "月",
            Period: "1",
            LastTime: "2022/12/01",
            Date: "2022/01/01",
            ESN: "E00003",
        },
        InspecitonPlan: {
            IPSN: "P23010401",
            IPName: "巡檢A區計畫",
            PlanDate: "2023/01/08",
            PlanState: "巡檢完成",
            Shift: "早班",
            MyName: "王大明、李泰順",
        },
        InspectionPlanMaintain: {
            MaintainState: "完成",
            MyName: "王大明",
            MaintainContent: "修復完成",
            MaintainDate: "2023/01/07 12:40:56",
            ImgPath: [
                "/Content/img/bg.png"
            ],
        },
        MaintainSupplementarInfo: [
            {
                MyName: "王大明",
                SupplementaryDate: "2023/01/10",
                SupplementaryContent: "OOXX",
                FilePath: ["/file/電燈故障.pdf"],
            }
        ],
        MaintainAuditInfo: [
            {
                MyName: "陳組長",
                AuditDate: "2023/1/12",
                AuditResult: "審核未過",
                AuditMemo: "未處理完善",
                ImgPath: [
                    "/Content/img/bg.png"
                ],
            }
        ],
        InspectionPlanList: [
            {
                "InspectionPlan": {
                    "IPSN": "P23022203",
                    "IPName": "維修設備E00003",
                    "PlanDate": "2023/2/22",
                    "PlanState": "巡檢完成",
                    "Shift": "2",
                    "MyName": "賈皓麟、羅紹齊"
                },
                "InspectionPlanRepair": {
                    "RepairState": "審核未過",
                    "RepairContent": "好了",
                    "MyName": "賈皓麟",
                    "RepairDate": "2023/2/22",
                    "ImgPath": []
                },
                "RepairSupplementaryInfo": [
                    {
                        "MyName": "王大明",
                        "SupplementaryDate": "2023/1/15",
                        "SupplementaryContent": "OOXX",
                        "FilePath": ["/file/電燈故障.pdf", "/file/電燈故障2.pdf"],
                    }
                ],
                "RepairAuditInfo": [
                    {
                        "MyName": "陳進國",
                        "AuditDate": "2023/2/22",
                        "AuditResult": "審核未過",
                        "AuditMemo": "沒修好",
                        "ImgPath": []
                    }
                ]
            },
        ]
    } : data;

    $("#EquipmentMaintainFormItem").append(
        createTableInner(data.EquipmentMaintainFormItem, sn.EquipmentMaintainFormItem)
    );

    $("#InspecitonPlan").append(
        createTableInner(data.InspecitonPlan, sn.InspecitonPlan)
    );
    $("#InspectionPlanMaintain").append(
        createTableInner(data.InspectionPlanMaintain, sn.InspectionPlanMaintain)
    );

    if (data.MaintainSupplementarInfo && data.MaintainSupplementarInfo.length != 0) {
        let accordion = $("#accordion-MaintainSupplementarInfo");
        data.MaintainSupplementarInfo.forEach((d, i, arr) => {
            accordion.append(createAccordionItem({
                title: "補件資料",
                id: "MaintainSupplementarInfo",
                sn: sn.MaintainSupplementarInfo,
                data: arr,
                itemTitleKey: "SupplementaryDate"
            }, i));
        })
    }

    if (data.MaintainAuditInfo && data.MaintainAuditInfo.length != 0) {
        let accordion = $("#accordion-MaintainAuditInfo");
        data.MaintainAuditInfo.forEach((d, i, arr) => {
            accordion.append(createAccordionItem({
                title: "審核資料",
                id: "MaintainAuditInfo",
                sn: sn.MaintainAuditInfo,
                data: arr,
                itemTitleKey: "AuditDate"
            }, i));
        })
    }
    
    if (data.InspectionPlanList.length != 0) {
        let accordion = $("#accordion-InspectionPlan");
        data.InspectionPlanList.forEach((d, i) => {
            accordion.append(putData_InspectionPlan(d, null, i));
        })
        $("#InspectionPlan_List").removeClass("d-none");
    }
    else {
        $("#accordion-InspectionPlan").parents(".datatable").remove();
    }
}

