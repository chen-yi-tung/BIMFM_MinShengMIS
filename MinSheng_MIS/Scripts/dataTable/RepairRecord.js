function RepairRecord(selector, data) {
    console.log(" $(selector)", $(selector))
    $(selector).append(
        data.Inspections ?
            createAccordion({
                id: `Inspections`,
                state: data.PlanState,
                sn: [
                    { text: "巡檢狀態", value: "InspectionState" },
                    { text: "巡檢頻率", value: "Frequency" },
                    { text: "巡檢數量", value: "EquipmentCount" },
                    {
                        type: "accordion", value: "Equipments", sn: [
                            { text: "所在位置", value: "Location", colspan: true },
                            { text: "最新填報者", value: "ReportUserName", colspan: true },
                            { text: "最新填報時間", value: "FillinTime", colspan: true },
                            { text: "檢查項目", value: "CheckItems", type: "DualCol" },
                            { text: "填報列表", value: "RportItems", type: "DualCol" },
                        ]
                    },
                ],
                data: data.Inspections,
                itemTitleKey: `PathName`,
                itemSubTitleKey: `PlanDate`,
                layer: 2,
                icon: "clipboard-list",
            })
            : "",
    )

    $(selector).append(
        data.Inspections.Equipments ?
            createTableInner(data.Inspections,
                [
                    {
                        text: "填報列表",
                        value: "RportItems",
                        type: "DualCol",
                        itemNum: [
                            { value: "Value" },
                            { value: "Unit" },
                        ],
                    },
                ]
            )
            : "",
    )
}

function InspectionRecord(selector, data) {
    $(selector).append(
        data.ReportItemList ?
            createInspectionTable({

                id: `ReportItemList`,
                sn: [
                    { text: "巡檢頻率", value: "Frequency", colspan: "3" },
                    { text: "檢查項目", value: "CheckItemList", colspan: "2", itemNum: true },
                    { text: "填報項目名稱/單位", value: "ReportItemList", type: "DualCol" },
                ],
                data: data,
            })
            : "",
    )
}

function EquipmentRFID(selector, data) {
    $(selector).append(
        data.RFID ?
            createAccordion({
                id: `RFID`,
                sn: [
                    { text: "RFID名稱", value: "Name" },
                    { text: "RFID內碼", value: "RFIDInternalCode" },
                    { text: "RFID外碼", value: "RFIDExternalCode" },
                    { text: "棟別", value: "ASN" },
                    { text: "樓層", value: "FSN" },
                    { text: "定位", value: "Location", btn: true },
                    { text: "備註", value: "Memo" },
                ],
                data: data.RFID,
                itemTitleKey: `Name`,
            })
            : "",
    )
}

function MaintainInfo(selector, data) {
    //console.log("MaintainInfo data", data)
    //console.log('MaintainInfo selector', selector);

    $(selector).append(
        data.MaintainItemList ?
            createInspectionTable({
                id: `MaintainItemList`,
                sn: [
                    { text: "保養項目/週期", value: "MaintainItemList", type: "TripleCol" },
                ],
                data: {
                    ...data,
                    MaintainItemList: data.MaintainItemList.map(item => {
                        // 假設要處理每個項目的value
                        return {
                            ...item,
                            Value: item.Text,
                            Period: item.PeriodText, // 自訂轉換邏輯
                            NextMaintainDate: item.NextMaintainDate.replace(/-/g, "/"),
                        };
                    }),
                },
            })
            : "",
    )
}

function InspectionInfo(selector, data) {

    $(selector).append(
        data ?
            createTableInner(
                data,
                [
                    {
                        text: "巡檢頻率",
                        value: "Frequency",
                        colspan: true
                    }
                ]
            )
            : "",
    )
    $(selector).append(
        data.CheckItemList ?
            createTableInner(data,
                [
                    {
                        text: "檢查項目",
                        value: "CheckItemList",
                        itemNum: [
                            { value: "Value" },
                        ],
                        colspan: true
                    },
                ]
            )
            : "",
    )
    $(selector).append(
        data.ReportItemList ?
            createTableInner(data,
                [
                    {
                        text: "填報項目名稱/單位",
                        value: "ReportItemList",
                        itemNum: [
                            { value: "Value" },
                            { value: "Unit" },
                        ],
                    },
                ]
            )
            : "",
    )
}
