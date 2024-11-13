function RepairRecord(selector, data) {
    console.log("data" ,data)
    const sn = {
        //EquipmentReportItem: [
        //    { text: "報修單狀態", value: "ReportState" },
        //    { text: "報修單號", value: "RSN" },
        //    { text: "報修時間", value: "Date" },
        //],
        EquipmentItem: [
            { text: "設備名稱", value: "IName", colspan: true },
            { text: "設備型號", value: "Model", colspan: true },
            { text: "所在位置", value: "Location", colspan: true },
            { text: "檢查項目", value: "InspectItems" },
            { text: "填報列表", value: "ReportItems" },
        ],
        //RepairSupplementaryInfo: [
        //    { text: "補件人", value: "MyName" },
        //    { text: "補件日期", value: "SupplementaryDate" },
        //    { text: "補件說明", value: "SupplementaryContent" },
        //    { text: "補件檔案", value: "FilePath" },
        //],
    };
    console.log('selector', $(selector));
    
        //createTableOuter({
        //    title: "報修資料",
        //    id: "EquipmentReportItem",
        //    className: "mt-5",
        //    inner: createTableInner(data.EquipmentReportItem, sn.EquipmentReportItem),
        //}),
        //createAccordion({
        //    id: `EquipmentItem`,
        //    sn: sn.EquipmentItem,
        //    data: data.EquipmentItem,
        //    itemTitleKey: "IName"
        //}) : "",

        //createTableOuter({
        //    title: "計劃資訊",
        //    id: "InspectionPlan",
        //    inner: createTableInner(data.InspectionPlan, sn.InspectionPlan),
        //}),
        //createTableOuter({
        //    title: "維修填報",
        //    id: "InspectionPlanRepair",
        //    inner: createTableInner(data.InspectionPlanRepair, sn.InspectionPlanRepair),
        //}),


        //data.InspectionRecord?.EquipmentItem?
        //    createAccordion({
        //        id: `EquipmentItem`,
        //        sn: sn.EquipmentItem,
        //        data: data.EquipmentItem,
        //        itemTitleKey: "IName"
        //    }) : "",

        //data.InspectionRecord?.forEach((record, i) => {
        //    const accordionElement = createMainAccordion(
        //        {
        //            id: `InspectionRecord_${i}`,
        //            sn: [
        //                { text: "巡檢狀態", value: "IState", colspan: true },
        //                { text: "巡檢頻率", value: "Ifrequency", colspan: true },
        //                { text: "巡檢數量", value: "INum", colspan: true },
        //            ],
        //            data: record,
        //            itemTitleKey: "IName",
        //            layer: 2,
        //        },
        //        i
        //    )
        //    $(selector).append(accordionElement);
        //})


            $(selector).append(
                data.InspectionRecord ?
                    createAccordion({
                        id: `InspectionRecord`,
                        state: data.State,
                        sn: [
                                { text: "巡檢狀態", value: "IState" },
                                { text: "巡檢頻率", value: "Ifrequency" },
                                { text: "巡檢數量", value: "INum" },
                        ],
                        subsn: [
                                    { text: "所在位置", value: "Location", colspan: true },
                                    { text: "檢查項目", value: "InspectItems" },
                                    { text: "填報列表", value: "ReportItems" },
                        ],
                        data: data.InspectionRecord,
                        itemTitleKey: `IName`,
                        itemTime: `ITime`,
                        layer: 2,
                    })
                    : "",
            )

    //let ESN = data.EquipmentReportItem.ESN;

    //$("#MaintainRecord").click(function () {
    //    let url = `/MaintainRecord_Management/Management?ESN=${ESN}`;
    //    window.open(url, "_blank");
    //});
    //$("#OtherRepairRecord").click(function () {
    //    let url = `/RepairRecord_Management/Management?ESN=${ESN}`;
    //    window.open(url, "_blank");
    //});
}