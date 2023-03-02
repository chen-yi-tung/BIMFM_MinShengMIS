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
        $select.append('<option value="">-- 請選擇 --</option>');
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
        $select.append('<option value="">-- 請選擇 --</option>');
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
                        <td id="${pid}" name="${pid}">${d?d:""}</td>
                    </tr>
                    `;
        $("#" + tableId + " tbody").append(html);
        num = num + 1;
    });
};

/**
 * 
 * @param {string} name 
 * @param {number} i 
 * @returns {string}
 */
function itemId(name, i) {
    if (String.prototype.padStart) {
        return name + "_" + i.toString().padStart(3, "0");
    }
    else {
        if (num < 10) {
            return name + "_00" + i;
        } else if (num < 100) {
            return name + "_0" + i;
        } else {
            return name + "_" + i;
        }
    }
}

function filterIt(arr, objKey, searchKey) {
    return arr.filter(obj => obj[objKey] == searchKey);
}