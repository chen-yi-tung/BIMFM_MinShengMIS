function RepairRecord(selector, data) {
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
                    { text: "檢查項目", value: "InspectItems", type: "DualCol" },
                    { text: "填報列表", value: "ReportItems", type: "DualCol" },
                ],
                data: data.InspectionRecord,
                itemTitleKey: `IName`,
                itemSubTitleKey: `ITime`,
                layer: 2,
                icon: "clipboard-list",
            })
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
