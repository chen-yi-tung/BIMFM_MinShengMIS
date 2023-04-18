function RepairRecord(selector, data) {
    const sn = {
        EquipmentReportItem: [
            { text: "報修單狀態", value: "ReportState" },
            { text: "報修單號", value: "RSN" },
            { text: "報修時間", value: "Date" },
            { text: "報修等級", value: "ReportLevel" },
            { text: "報修人員", value: "MyName" },
            { text: "區域", value: "Area" },
            { text: "樓層", value: "Floor" },
            { text: "國有財產編碼", value: "PorpertyCode" },
            { text: "設備編號", value: "ESN" },
            { text: "設備名稱", value: "EName" },
            {
                text: "設備屬性", value: "ESN_Button", formatter: (val) => {
                    return val ? `<button class="btn btn-search" onclick="EquipmentInfoModal('/EquipmentInfo_Management/ReadBody/${val}')">設備資料</button>` : "-"
                }
            },
            { text: "報修說明", value: "ReportContent" },
            { text: "報修照片", value: "ImgPath" },
        ],
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
    };

    $(selector).append(
        createTableOuter({
            title: "報修資料",
            id: "EquipmentReportItem",
            className: "mt-5",
            inner: createTableInner(data.EquipmentReportItem, sn.EquipmentReportItem),
        }),
        createTableOuter({
            title: "計劃資訊",
            id: "InspectionPlan",
            inner: createTableInner(data.InspectionPlan, sn.InspectionPlan),
        }),
        createTableOuter({
            title: "維修填報",
            id: "InspectionPlanRepair",
            inner: createTableInner(data.InspectionPlanRepair, sn.InspectionPlanRepair),
        }),
        data.RepairSupplementaryInfo ?
            createAccordion({
                title: "補件資料",
                id: `RepairSupplementaryInfo`,
                sn: sn.RepairSupplementaryInfo,
                data: data.RepairSupplementaryInfo,
                itemTitleKey: "SupplementaryDate"
            }) : "",
        data.RepairAuditInfo ?
            createAccordion({
                title: "審核資料",
                id: `RepairAuditInfo`,
                sn: sn.RepairAuditInfo,
                data: data.RepairAuditInfo,
                itemTitleKey: "AuditDate"
            }) : "",
    );

    let ESN = data.EquipmentReportItem.ESN;

    $("#MaintainRecord").click(function () {
        let url = `/MaintainRecord_Management/Management?ESN=${ESN}`;
        window.open(url, "_blank");
    });
    $("#OtherRepairRecord").click(function () {
        let url = `/RepairRecord_Management/Management?ESN=${ESN}`;
        window.open(url, "_blank");
    });
}