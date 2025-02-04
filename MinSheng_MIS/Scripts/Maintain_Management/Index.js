async function addDropDownList() {
    await $('.maintain-user-name').tagbox({
        url: "/DropDownList/MaintainUserName",
        textField: 'Text',
        valueField: 'Value',
        method: 'get',
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