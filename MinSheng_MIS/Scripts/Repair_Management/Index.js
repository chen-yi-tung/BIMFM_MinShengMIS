async function addDropDownList() {
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