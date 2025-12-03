async function addDropDownList() {
    await $('.repair-user-name').tagbox({
        url: "/DropDownList/AssignmentUserName",
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