function InspectionPlanRepair(selector, data) {

    const sn = {
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

    if (data.length === 0) {
        return;
    }

    $(selector).append(
        createAccordionOuter({
            title: "本維修單相關維修紀錄",
            id: "InspectionPlanRepair",
            className: "datatable-secondary mt-5",
            inner: data.map((d, i) => createInner(d, i)).join("")
        })
    );

    function createInner(data, i) {
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
                    ${createTableOuter({
            title: "計劃資訊",
            id: "InspectionPlan",
            className: "border-0 w-100",
            inner: createTableInner(data.InspectionPlan, sn.InspectionPlan),
        })}
                    ${createTableOuter({
            title: "維修資料",
            id: "InspectionPlanRepair",
            className: "border-0 w-100",
            inner: createTableInner(data.InspectionPlanRepair, sn.InspectionPlanRepair),
        })}
                    ${data.RepairSupplementaryInfo ? createAccordion({
            title: "補件資料",
            id: `OtherRepairSupplementaryInfo-${i}`,
            className: "border-0 w-100",
            sn: sn.RepairSupplementaryInfo,
            data: data.RepairSupplementaryInfo,
            itemTitleKey: "SupplementaryDate"
        }) : ""}
                    ${data.RepairAuditInfo ? createAccordion({
            title: "審核資料",
            id: `OtherRepairAuditInfo-${i}`,
            className: "border-0 w-100",
            sn: sn.RepairAuditInfo,
            data: data.RepairAuditInfo,
            itemTitleKey: "AuditDate"
        }) : ""}
                </div>
            </div>
        </div>
        `;
    }
}