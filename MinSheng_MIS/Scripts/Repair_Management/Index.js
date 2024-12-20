async function addDropDownList() {
    //await pushSelect("Shift", "/DropDownList/Shift");
    //await pushSelect("MaintainUserID", "/DropDownList/AllMyName");
    //await pushSelect("RepairUserID", "/DropDownList/AllMyName");

    //const DMM = "DatagridModal-Maintain";
    //await pushSelect(DMM + " #FormItemState", "/DropDownList/AddFormItemState");
    //await pushSelect(DMM + " #EState", "/DropDownList/EState?url=AddToPlan");
    //await pushSelect(DMM + " #StockState", "/DropDownList/StockState");

    //const DMR = "DatagridModal-Repair";
    //await pushSelect(DMR + " #ReportLevel", "/DropDownList/ReportLevel");
    //await pushSelect(DMR + " #ReportState", "/DropDownList/Report_Management_Management_ReportFormState?url=CanAddToPlanReportState");
    //await pushSelect(DMR + " #UserID", "/DropDownList/ReportUser");
    //await pushSelect(DMR + " #StockState", "/DropDownList/StockState");

    await $('.repair-user-name').tagbox({
        url: "/DropDownList/AllMyName",
        textField: 'Text',
        valueField: 'Value',
        hasDownArrow: true,
        limitToList: true,
        validateOnCreate: false,
        tagStyler: function (value) {
            if (value) {
                return 'background:#5480CA; color:#FFF; padding: 4px; height: fit-content;';
            }
        }
    });
}