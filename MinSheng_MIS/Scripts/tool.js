/**
 * @param {JQueryEventObject} input 
 * @param {string} imgId 
 */

function readURL(input, imgId) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $("#" + imgId).attr('src', e.target.result);
        }
        reader.readAsDataURL(input.files[0]);
    }
}

/** 
 * @param {string} selectId
 * @param {string} jsonUrl
 * @param {string} [optionName=name]
 * @param {string} [optionValue=uuid]
 */
async function pushSelectOptions(selectId, jsonUrl, optionName, optionValue) {
    const $select = $("#" + selectId);
    let name = optionName ? optionName : "Name";
    let value = optionValue ? optionValue : "Uuid";
    await $.getJSON(jsonUrl, function (data) {
        $select.empty();
        $select.append('<option value="">請選擇</option>');
        $.each(data.rows, function (i, e) {
            $select.append('<option value="' + e[value] + '">' + e[name] + '</option>')
        })
    });
}
async function pushSelect(selectId, jsonUrl, optionName, optionValue) {
    const $select = $("#" + selectId);
    let name = optionName ? optionName : "Text";
    let value = optionValue ? optionValue : "Value";
    await $.getJSON(jsonUrl, function (data) {
        $select.empty();
        $select.append('<option value="">請選擇</option>');
        $.each(data, function (i, e) {
            $select.append('<option value="' + e[value] + '">' + e[name] + '</option>')
        })
    });
}
/** 
 * @param {string} selectId
 * @param {string} jsonUrl
 * @param {string} filterKey
 * @param {string} filterText
 * @param {string} [optionName=name]
 * @param {string} [optionValue=uuid]
 */
function pushSubSelectOptions(selectId, jsonUrl, filterKey, filterText, optionName, optionValue) {
    const $select = $("#" + selectId);
    let name = optionName ? optionName : "Name";
    let value = optionValue ? optionValue : "Uuid";
    $.getJSON(jsonUrl, function (data) {
        $select.empty();
        $select.append('<option value="select">-- 請選擇 --</option>');
        $.each(data.rows, function (i, e) {
            if (e[filterKey] == filterText) {
                $select.append('<option value="' + e[value] + '">' + e[name] + '</option>')
            }
        })
    });
}

/**
 * @param {string} tableId 
 * @param {*[]} data
 * @param {object} setting 
 */
function createTable(tableId, data, setting) {
    const defaultSetting = {
        itemId: "Item",
        HeadColumn: [],
        HeadRow: [],
        dataKey: []
    }
    let s = Object.assign(defaultSetting, setting);
    let th = s.HeadColumn;
    let td = s.HeadRow;
    let dataKey = s.dataKey;
    $("#" + tableId).append("<thead></thead><tbody></tbody>");
    $("#" + tableId + " thead").append("<tr></tr>");
    $.each(th, function (i) {
        let html = `
            <th scope="col">${th[i]}</th>
            `;
        $("#" + tableId + " thead tr").append(html);
    })
    let num = 1;
    $.each(td, function (i) {
        let pid = itemId(s.itemId, num);
        let d = dataKey.length > 0 ? data[dataKey[i]] : data[i];
        let html = `
                    <tr>
                        <th scope='row'>${td[i]}</th>
                        <td id="${pid}" name="${pid}">${d ? d : ""}</td>
                    </tr>
                    `;
        $("#" + tableId + " tbody").append(html);
        num = num + 1;
    });
};

function getQueryParams(selector = null) {
    const searchParams = $(selector ?? "form")
        .find("input:not([type='button']):not([type='submit']):not([type='reset']), select")
        .toArray()
        .map(e => $(e).attr("name"));
    //console.log(searchParams)
    let queryParams = searchParams.reduce((total, c) => {
        total[c] = $(selector ? `${selector} #${c}` : `#${c}`).val()
        return total;
    }, {})
    console.log("queryParams", queryParams)
    return queryParams;
}

function FileUploader({
    container,
    className = "form-group required g-col-2",
    label = "",
    id = "File",
    template = null,
    required = true,
    customValidity = false,
    customValidityText = '請選擇檔案'
}) {
    const temp = () => {
        return `
        <div class="${className}">
            <label for="${id}">${label}</label>
            <div class="edit-button-area position-relative justify-content-start align-items-center mt-1 flex-wrap flex-lg-nowrap">
                <div class="d-lg-contents d-flex w-100" style="gap: 14px;">
                    <label for="${id}" type="button" class="btn btn-search w-lg-auto w-100 h-100 mt-0 flex-shrink-0">
                        <span>選擇檔案</span>
                        <input id="${id}" name="${id}" type="file" class="form-file-input" ${required && !customValidity ? 'required' : ''}>
                        ${required && customValidity ? `
                        <input type="checkbox" id="_checkFile" name="_checkFile" class="form-file-input" required
                            oninvalid="this.setCustomValidity(this.validity.valueMissing ? ${customValidityText} : '')">
                        ` : ''}
                    </label>
                </div>
                <div id="FileGroup" class="order-first order-lg-last d-flex align-items-center text-start text-light w-100 w-lg-auto d-none">
                    <a id="FileName" class="form-file-name d-inline-block text-break me-2" style="margin: 0.375rem;" target="_blank"></a>
                    <button type="button" class="btn-delete-item flex-shrink-0" id="FileDelete"></button>
                </div>
            </div>
        </div>`
    }
    this.element = $(template ? template() : temp())
    this.input = this.element.find("#File")

    const fileName = this.element.find("#FileName")
    const fileGroup = this.element.find("#FileGroup")
    const deleteBtn = this.element.find("#FileDelete")

    if (customValidity) {
        this.check = this.element.find("#_checkFile")
    }
    this.hasFile = ()=>{
        let input = this.input.get(0)
        return input.files && input.files.length !== 0
    }
    this.setFile = (path) => {
        fileName.text(path.split("/").at(-1));
        fileName.attr("href", path);
        fileGroup.removeClass('d-none');
        this.check && this.check.prop("checked", true);
    }
    this.getFile = (index = 0) => {
        return this.input.get(0).files[index];
    }
    this.init = () => {
        $(container).after(this.element);
        $(container).remove();
        console.log(fileName)
        this.input.change((e) => {
            let input = this.input.get(0)
            if (input.files && input.files.length !== 0) {
                let file = this.getFile();
                fileName.text(file.name);
                fileName.removeAttr("href");
                fileGroup.removeClass('d-none');
                this.check && this.check.prop("checked", true);
            }
        })
        deleteBtn.click(() => {
            this.input.val('');
            fileName.text('');
            fileName.removeAttr("href");
            fileGroup.addClass('d-none');
            this.check && this.check.prop("checked", false);
        })
    }
    this.init();
    return this
}