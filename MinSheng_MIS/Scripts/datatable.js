function createTableInner(data, sn) {
    return sn.map((e) => {
        let html;
        switch (e.value) {
            case "ImgPath":
                html = `
                    <tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" id="d-${e.value}">${putImage(data[e.value])}</td>
                    </tr>
                `;
                break;
            case "FilePath":
                html = `
                    <tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" id="d-${e.value}">${putFile(data[e.value])}</td>
                    </tr>
                `;
                break;
            default:
                if (e.formatter) {
                    html = `
                    <tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" id="d-${e.value}">
                            ${e.formatter(data[e.value])}
                        </td>
                    </tr>
                    `;
                }
                else {
                    html = `
                    <tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" id="d-${e.value}">${data[e.value]}</td>
                    </tr>
                    `;
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
                td = `<td id="d-${o.id}">${d[o.id]}</td>`;
            }
            return td;
        }).join("");
    }
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
                ${createAccordion({
        title: "補件資料",
        id: "RepairSupplementaryInfo",
        sn: sn.RepairSupplementaryInfo,
        data: data.RepairSupplementaryInfo,
        itemTitleKey: "SupplementaryDate"
    })}
                ${createAccordion({
        title: "審核資料",
        id: "RepairAuditInfo",
        sn: sn.RepairAuditInfo,
        data: data.RepairAuditInfo,
        itemTitleKey: "AuditDate"
    })}
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
    `
}
