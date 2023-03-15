function MaintainRecord(selector, data) {
    const sn = {
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
        InspectionPlan: [
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
        MaintainSupplementaryInfo: [
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
    };

    $(selector).append(
        createTableOuter({
            title: "保養項目資料",
            id: "EquipmentMaintainFormItem",
            className: "mt-5",
            inner: createTableInner(data.EquipmentMaintainFormItem, sn.EquipmentMaintainFormItem),
        }),
        createTableOuter({
            title: "計劃資訊",
            id: "InspectionPlan",
            inner: createTableInner(data.InspectionPlan, sn.InspectionPlan),
        }),
        createTableOuter({
            title: "保養填報",
            id: "InspectionPlanMaintain",
            inner: createTableInner(data.InspectionPlanMaintain, sn.InspectionPlanMaintain),
        }),
        data.MaintainSupplementaryInfo &&
        createAccordion({
            title: "補件資料",
            id: `MaintainSupplementaryInfo`,
            sn: sn.MaintainSupplementaryInfo,
            data: data.MaintainSupplementaryInfo,
            itemTitleKey: "SupplementaryDate"
        }),
        data.MaintainAuditInfo &&
        createAccordion({
            title: "補件資料",
            id: `MaintainAuditInfo`,
            sn: sn.MaintainAuditInfo,
            data: data.MaintainAuditInfo,
            itemTitleKey: "AuditDate"
        }),
    );
}