function InspectionPlanMaintain(selector, data) {

    const sn = {
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

    if (data.length === 0) {
        return;
    }

    $(selector).append(
        createAccordionOuter({
            title: "本保養單相關保養紀錄",
            id: "InspectionPlanMaintain",
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
            title: "保養資料",
            id: "InspectionPlanMaintain",
            className: "border-0 w-100",
            inner: createTableInner(data.InspectionPlanMaintain, sn.InspectionPlanMaintain),
        })}
                    ${data.MaintainSupplementaryInfo ? createAccordion({
            title: "補件資料",
            id: `OtherMaintainSupplementaryInfo-${i}`,
            className: "border-0 w-100",
            sn: sn.MaintainSupplementaryInfo,
            data: data.MaintainSupplementaryInfo,
            itemTitleKey: "SupplementaryDate"
        }) : ""}
                    ${data.MaintainAuditInfo ? createAccordion({
            title: "審核資料",
            id: `OtherMaintainAuditInfo-${i}`,
            className: "border-0 w-100",
            sn: sn.MaintainAuditInfo,
            data: data.MaintainAuditInfo,
            itemTitleKey: "AuditDate"
        }) : ""}
                </div>
            </div>
        </div>
        `;
    }
}