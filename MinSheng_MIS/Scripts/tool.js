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
async function pushSelect(selectId, jsonUrl, optionName, optionValue, defaultText = "請選擇") {
    const $select = $("#" + selectId);
    let name = optionName ? optionName : "Text";
    let value = optionValue ? optionValue : "Value";
    await $.getJSON(jsonUrl, function (data) {
        $select.empty();
        $select.append(`<option value="">${defaultText}</option>`);
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

function getAllParams(selector = null) {
    return $(selector ?? "form")
        .find("input:not([type='button']):not([type='submit']):not([type='reset']):not([name^='_']), select")
        .toArray();
}

function FileUploader({
    container,
    className = "form-group required g-col-2",
    buttonAreaClassName = "edit-button-area position-relative p-3 bg-white d-flex flex-column",
    buttonText = "選擇檔案",
    icon = "",
    label = "",
    id = "File",
    accept = [".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv"],
    required = true,
    multiple = false,
    customValidity = true,
    customValidityText = '請選擇檔案'
}) {
    const temp = () => {
        return `
        <div class="${className}">
            <label for="${id}">${label}</label>
            <div class="${buttonAreaClassName}">
                <div class="d-flex gap-2 align-items-center">
                    <div class="d-lg-contents d-flex" style="height: fit-content;">
                        <label for="${id}" type="button" class="btn btn-search mt-0 align-self-start" style="width: max-content;">
                            ${icon ? `<i class="fa-solid fa-${icon}"></i>` : ""}
                            <span>${buttonText}</span>
                            <input id="${id}" name="${id}" type="file" class="form-file-input" 
                            ${accept && Array.isArray(accept) && accept.length > 0 ? `accept="${accept.join(",")}"` : ''}
                            ${required && !customValidity ? 'required' : ''}
                            ${multiple ? 'multiple' : ''}>
                            ${required && customValidity ? `
                            <input type="checkbox" id="_checkFile" name="_checkFile" class="form-file-input" required
                                oninvalid="this.setCustomValidity(this.validity.valueMissing ? '${customValidityText}' : '')">
                            ` : ''}
                        </label>
                    </div>
                    <div class="upload-tips">
                        <div>檔案格式支援 .jpg、.jpeg、.png 或 .pdf。</div>
                        <div>檔案大小不得超過 10MB。</div>
                    </div>
                </div>
                <hr class="form-file-hr">
                <div id="FileGroup" class="form-file-list"></div>
            </div>
        </div>`
    }
    const temp_item = (name) => {
        return `<div class="form-file-item" data-file-name="${name}">
                    <img class="form-file-preview"/>
                    <div class="d-flex flex-column">
                        <div id="FileName" class="form-file-name"></div>
                        <div class="form-file-size"></div>
                    </div>
                    <button type="button" class="btn-delete-item flex-shrink-0" id="FileDelete"></button>
                </div>`
    }
    this.element = $(temp())
    this.input = this.element.find(".form-file-input")
    this.items = []

    const list = this.element.find(".form-file-list")

    if (customValidity) {
        this.check = this.element.find("#_checkFile")
    }
    this.hasFile = () => {
        return this.items.length !== 0
    }
    this.setFile = (path, file = null) => {
        if (!multiple) {
            this.clearAllFile()
        }
        let container = $(temp_item())
        list.append(container);

        if (file == null) {
            file = { name: path.split("/").pop(), type: "image/" };
            //file = { name: path.split("/").at(-1) }
        }
        this.items.push({ container, file })

        let fileName = container.find("#FileName")
        fileName.text(file.name);
        fileName.attr("href", path);

        document.querySelector('.form-file-list').style.display = 'flex';
        document.querySelector('.form-file-preview').style.display = 'flex';
        document.querySelector('.form-file-hr').style.display = 'block';
        document.querySelector('.form-file-preview').src = path;

        this.check && this.check.prop("checked", true);
    }

    this.getFile = (index = 0) => {
        return this.items[index]?.file;
    }
    this.getAllFile = () => {
        return this.items.map((item) => item.file)
    }
    this.getFileCount = () => {
        return this.items.length
    }
    this.clearAllFile = () => {
        this.items.forEach((item) => { item.container.remove() })
        this.items.length = 0
    }
    this.checkExtension = (index = null) => {
        if (index == null) {
            return this.items.map((item) => {
                let c = check(item)
                //console.log(item.file.name, c)
                return c
            })
        }
        else {
            return check(this.items[index])
        }

        function check({ file }) {
            const dotIndex = file.name.lastIndexOf('.');
            let ext = dotIndex ? file.name.slice(dotIndex).toLowerCase() : '';
            for (const a of accept) {
                if (a[0] === ".") {
                    if (a.toLowerCase() === ext) {
                        //console.log(a + ":", ext)
                        return true;
                    }
                }
                else if (file.type.match(a)) {
                    //console.log(a + ":", file.type.match(a))
                    return true;
                }
            }

            return false;
        }
    }
    this.checkAllExtension = () => {
        return this.checkExtension().every(e => e)
    }
    this.setCustomValidity = (text) => {
        this.input.get(0).setCustomValidity(text ?? '')
    }
    this.setExtensionValidity = (validity = true) => {
        if (validity) this.input.get(0).setCustomValidity("")
        else this.input.get(0).setCustomValidity("請上傳指定格式：\n"+accept.join(", "))
    }
    this.init = () => {
        $(container).after(this.element);
        $(container).remove();
        this.input.change((e) => {
            let input = this.input.get(0)
            if (!multiple) { this.clearAllFile() }
            if (input.files && input.files.length !== 0) {
                this.element.find(".form-file-hr").css("display", "block");
                this.element.find("#FileGroup").css("display", "flex");

                for (let i = 0; i < input.files.length; i++) {
                    let file = input.files[i];
                    let container = $(temp_item(file.name))

                    list.append(container);
                    this.items.push({ container, file })

                    //顯示檔名+附檔名
                    let fileName = container.find("#FileName")
                    fileName.text(file.name);
                    fileName.removeAttr("href");

                    //顯示檔案大小，單位換算成MB
                    let fileSizeMB = (file.size / (1024 * 1024)).toFixed(2);
                    container.find(".form-file-size").text(`${fileSizeMB} MB`);

                    //顯示縮圖
                    if (file.type.startsWith("image/")) {
                        const reader = new FileReader(); //轉換為Base64格式
                        reader.onload = function (event) {
                            let img = container.find(".form-file-preview");
                            img.attr("src", event.target.result);
                            img.css("display", "block");
                        };
                        reader.readAsDataURL(file);
                    }

                    this.check && this.check.prop("checked", true);
                }
            } else {
                // 當沒有檔案時，隱藏<hr>
                this.element.find(".form-file-hr").css("display", "none");
                this.element.find("#FileGroup").css("display", "none");
            }
            input.value = null
        })

        list.on("click", "#FileDelete", (event) => {
            let container = $(event.currentTarget).parent()
            let index = this.items.findIndex(e => e.container.data("fileName") == container.data("fileName"))
            if (index != -1) {
                this.items[index].container.remove()
                this.items.splice(index, 1)
            }
            if (this.items.length == 0) {
                this.element.find(".form-file-hr").css("display", "none");
                this.element.find("#FileGroup").css("display", "none");
                this.check && this.check.prop("checked", false);
            }
        })
    }
    this.init();
    return this
}