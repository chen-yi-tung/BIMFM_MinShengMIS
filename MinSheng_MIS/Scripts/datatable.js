/**
 * @typedef {object} SerializedName
 * @property {string} text - use to th
 * @property {string} value - use to td
 * @property {function(value:string):string} formatter - use to formatter td data
 */

/**
 * @typedef {object} TableOuterOptions
 * @property {string} className - use to datatable custom class
 * @property {string} title - use to datatable-header
 * @property {string} id - use to set datatable-table id
 * @property {string} inner - TableInner from createTableInner
 * 
 * @param {TableOuterOptions} options 
 * @returns {string} TableOuter
 */
function createTableOuter(options) {
    return `
    <div class="datatable ${options.className ?? ""}">
        <div class="datatable-header">${options.title}</div>
        <div class="datatable-body">
            <table class="datatable-table" id="${options.id}">
                ${options.inner}
            </table>
        </div>
    </div>
    `;
}

/**
 * @param {*[]} data - the data need show
 * @param {SerializedName[]} sn 
 * @returns {string} TableInner
 */
function createTableInner(data, sn) {
    const nullString = "-";
    return sn.map((e) => {
        let html;
        switch (e.value) {
            case "ImgPath":
                html = `
                    <tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" id="d-${e.value}">${data[e.value] && data[e.value].length !== 0 ? putImage(data[e.value]) : nullString}</td>
                    </tr>`;
                break;
            case "FilePath":
                html = `
                    <tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" id="d-${e.value}">${data[e.value] && data[e.value].length !== 0 ? putFile(data[e.value]) : nullString}</td>
                    </tr>`;
                break;
            default:
                if (e.formatter) {
                    html = `
                        <tr>
                            <td class="datatable-table-th">${e.text}</td>
                            <td class="datatable-table-td" id="d-${e.value}">${e.formatter(data[e.value])}</td>
                        </tr>`;
                }
                else {
                    html = `
                        <tr>
                            <td class="datatable-table-th">${e.text}</td>
                            <td class="datatable-table-td" id="d-${e.value}">${data[e.value] ?? nullString}</td>
                        </tr>`;
                }
                break;
        }
        return html;
    }).join("");

    function putImage(imgs) {
        let div = `<div class="datatable-img-area">${imgs.map(img => {
            return `<div class="datatable-img-item"><img src="${img}"/></div>`;
        }).join("")}
        </div>`;
        return div;
    }

    function putFile(urls) {
        let div = urls.map(url => { return `<a href="${url}" target="_blank">${url.split('/').at(-1)}</a>` }).join("<br>");
        return div;
    }
}


function createTableGrid(data, options) {
    const nullString = "-";
    console.log(options)
    let columns = options.columns;
    let thead = options.thead == true ? `<thead><tr>${createThs(columns)}</tr></thead>` : "";
    let tbody = `<tbody>${createTrs(columns, data)}</tbody>`;
    return thead + tbody;
    function createThs(op) {
        return op.map(o => {
            let w = typeof o.width == "string" ? o.width : o.width + "px";
            let th = `<th class="datatable-header" style="width:${w}">${o.title}</th>`;
            return th;
        }).join("");
    }

    function createTrs(op, ds) {
        return ds.map((d, i) => {
            let tr = `<tr id="${i}">${createTds(op, d, i)}</tr>`;
            return tr;
        }).join("");
    }

    function createTds(op, d, i) {
        return op.map((o) => {
            let td
            if (o.formatter) {
                td = `
                <td id="d-${o.id}">
                    ${o.formatter(d[o.id], d, i)}
                </td>`;
            }
            else {
                td = `<td id="d-${o.id}">${d[o.id] ?? nullString}</td>`;
            }
            return td;
        }).join("");
    }
}


function createAccordionOuter(options) {
    return `
    <div class="datatable ${options.className ?? ""}" id="${options.id}">
        <div class="datatable-header">${options.title}</div>
        <div class="datatable-body">
            <div class="accordion accordion-flush datatable-accordion" id="accordion-${options.id}">
            ${options.inner}
            </div>
        </div>
    </div>
    `;
}


function createAccordion(options) {
    if (options.data.length === 0) {
        return "";
    }
    return `
    <div class="datatable ${options.className ?? ""}">
        <div class="datatable-header">${options.title}</div>
        <div class="datatable-body">
            <div class="accordion accordion-flush datatable-accordion" id="accordion-${options.id}">
                ${options.data.map((d, i) => {
        return createAccordionItem(options, i)
    }).join("")}
            </div>
        </div>
    </div>
    `;
}

/**
 * @typedef {object} AccordionItemOptions
 * @property {*[]} data - use to datatable custom class
 * @property {SerializedName[]} sn - use to datatable-header
 * @property {string} id - use to set accordion-item id
 * @property {string} itemTitleKey - use to accordion-header title from data[itemTitleKey]
 * 
 * @param {AccordionItemOptions} options 
 * @param {number} i Accordion Item index
 * @returns {string} TableOuter
 */
function createAccordionItem(options, i) {
    return `
    <div class="accordion-item" id="${options.id}-${i}">
        <h2 class="accordion-header" id="header-${options.id}-${i}">
            <button class="accordion-button collapsed" type="button"
                data-bs-toggle="collapse"
                data-bs-target="#body-${options.id}-${i}" aria-expanded="false"
                aria-controls="body-${options.id}-${i}">
                ${options.data[i][options.itemTitleKey]}
            </button>
        </h2>
        <div id="body-${options.id}-${i}" class="accordion-collapse collapse"
            aria-labelledby="header-${options.id}-${i}">
            <div class="accordion-body">
                <div class="datatable border-0 w-100">
                    <div class="datatable-body">
                        <table class="datatable-table">
                            ${createTableInner(options.data[i], options.sn)}
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
    `;
}
