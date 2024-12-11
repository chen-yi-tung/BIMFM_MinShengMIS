/**
 * @typedef {object} SerializedName
 * @property {string} text - use to th
 * @property {string} value - use to td
 * @property {function (value):string?} formatter - use to formatter td data
 * @property {boolean} image - set true if value is image
 * @property {boolean} url - set true if value is url
 * @property {boolean} pre - set true will wrap value in pre
 */


/**
 * @typedef {object} TableOuterOptions
 * @property {string?} className - use to datatable custom class
 * @property {string?} title - use to datatable-header
 * @property {string} id - use to set datatable-table id
 * @property {string} inner - TableInner from createTableInner
 * 
 * @param {TableOuterOptions} options 
 * @returns {string} TableOuter
 */
function createTableOuter(options) {
    return `
    <div class="datatable ${options.className ?? ""}">
        ${options.title ? `<div class="datatable-header">${options.title}</div>` : ""}
        <div class="datatable-body">
            <table class="datatable-table" id="${options.id ?? ''}">
                ${options.inner ?? ''}
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
        let html = "";
        if (e.formatter) {
            html = `
            <tr>
                <td class="datatable-table-th">${e.text}</td>
                <td class="datatable-table-td" id="d-${e.value}">${e.formatter(data[e.value])}</td>
            </tr>`;
        }
        else if (e.image == true) {
            html = `
            <tr>
                <td class="datatable-table-th">${e.text}</td>
                <td class="datatable-table-td" id="d-${e.value}">${data[e.value] != null ? putImage(data[e.value]) : nullString}</td>
            </tr>`;
        }
        else if (e.url == true) {
            html = `
            <tr>
                <td class="datatable-table-th">${e.text}</td>
                <td class="datatable-table-td" id="d-${e.value}">${data[e.value] != null ? putFile(data[e.value]) : nullString}</td>
            </tr>`;
        }
        else if (e.pre == true) {
            html = `
            <tr>
                <td class="datatable-table-th">${e.text}</td>
                <td class="datatable-table-td" id="d-${e.value}"><pre>${data[e.value] ?? nullString}</pre></td>
            </tr>`;
        }
        else if (e.colspan == true) {
            html = `
            <tr>
                <td class="datatable-table-th">${e.text}</td>
                <td class="datatable-table-td" colspan=3 id="d-${e.value}">${data[e.value] ?? nullString}</td>
            </tr>`;
        }
        else {
            switch (e.type) {
                case "ImgPath":
                    html = `
                        <tr>
                            <td class="datatable-table-th">${e.text}</td>
                            <td class="datatable-table-td" id="d-${e.value}">${data[e.value] != null ? putImage(data[e.value]) : nullString}</td>
                        </tr>`;
                    break;
                case "FilePath":
                    html = `
                        <tr>
                            <td class="datatable-table-th">${e.text}</td>
                            <td class="datatable-table-td" id="d-${e.value}">${data[e.value] != null ? putFile(data[e.value]) : nullString}</td>
                        </tr>`;
                    break;
                case "dualCol": {
                    const rows = data[e.value]?.length || 0;
                    if (rows === 0) {
                        html = '';
                        break;
                    }
                    const first = data[e.value][0];
                    const isDanger = first.Value === '異常';
                    html = `
                        <tr>
                            <td class="datatable-table-th" rowspan="${rows}">${e.text}</td>
                            <td class="datatable-table-td datatable-table-sort">1</td>
                            <td class="datatable-table-td text-start ps-2">
                                <div>${first.Item}</div>
                            </td>
                            <td class="datatable-table-td" style="width: 160px;">
                                <div class="${isDanger ? 'text-danger' : ''}">${first.Value} ${first.Init ? first.Init : ""}</div>
                            </td>
                        </tr>
                        ${data[e.value]?.slice(1).map((item, i) => {
                        const isDanger = item.Value === '異常';
                        return `<tr>
                                    <td class="datatable-table-td datatable-table-sort">${i + 2}</td>
                                    <td class="datatable-table-td text-start ps-2">
                                        <div>${item.Item}</div>
                                    </td>
                                    <td class="datatable-table-td" style="width: 160px;">
                                        <div class="${isDanger ? 'text-danger' : ''}">${item.Value} ${item.Init ? item.Init : ""}</div>
                                    </td>
                                </tr>
                                `}).join('')}
                        `;
                }
                    break;
                default: {
                    if (e.itemNum) {
                        //為避免後端傳來的不是陣列，若不是陣列則先轉為陣列
                        let arr = data[e.value];
                        if (!Array.isArray(arr)) { arr = [data[e.value]] }

                        const rows = data[e.value]?.length || 0;
                        if (rows === 0) {
                            html = '';
                            break;
                        }
                        const first = data[e.value][0];
                        html = `<tr>
                                    <td class="datatable-table-th" rowspan="${rows}">${e.text}</td>
                                    <td class="datatable-table-td datatable-table-sort">1</td>
                                    <td class="datatable-table-td text-start" id="d-${e.value}" colspan="${e.colspan}" ${e.colspan ? `colspan="${e.colspan}"` : ""}>${first.value}</td>
                                </tr>

                            ${data[e.value]?.slice(1).map((item, i) => {
                            return `<tr>
                                             <td class="datatable-table-td datatable-table-sort">${i + 2}</td>
                                             <td class="datatable-table-td text-start" id="d-${e.value}" colspan="${e.colspan}" ${e.colspan ? `colspan="${e.colspan}"` : ""}>${item.value ?? nullString}</td>
                                        </tr>`;
                        }).join("")}
                        `
                    } else {
                        html = `
                            <tr>
                                <td class="datatable-table-th">${e.text}</td>
                                <td class="datatable-table-td" id="d-${e.value}" ${e.colspan ? `colspan="${e.colspan}"` : ""}>${data[e.value] ?? nullString}</td>
                            </tr>`;
                    }
                }

            }
        }
        return html;
    }).join("");

    function putImage(imgs) {
        if (Array.isArray(imgs) && imgs.length !== 0) {
            return `<div class="datatable-img-area">${imgs.map(img => {
                return `<div class="datatable-img-item"><img src="${img}"/></div>`;
            }).join("")}</div>`
        }
        else if (typeof imgs === 'string' || imgs instanceof String) {
            return `<div class="datatable-img-area"><div class="datatable-img-item"><img src="${imgs}"/></div></div>`;
        }
        return nullString;
    }

    function putFile(urls) {
        if (Array.isArray(urls) && urls.length !== 0) {
            return urls.map(url => { return `<a href="${url}" target="_blank">${url.split('/').at(-1)}</a>` }).join("<br>");
        }
        else if (typeof urls === 'string' || urls instanceof String) {
            return `<a href="${urls}" target="_blank">${urls.split('/').at(-1)}</a>`;
        }
        return nullString;
    }
}


/**
 * @typedef {object} TableGridColumnsOptions
 * @property {string} id - use to html id and the key to find value from data
 * @property {string} title - use to th
 * @property {string?} width - ex: "30%"
 * @property {boolean?} required
 * @property {function (string, object, number):string?} formatter - use to formatter td data
 * 
 * @typedef {object} TableGridOptions
 * @property {TableGridColumnsOptions} columns - use to datatable-header
 * @property {boolean?} thead - if true that have th
 * 
 * @param {*[]} data - the data need show
 * @param {TableGridOptions} options 
 * @returns {string} TableGrid
 */
function createTableGrid(data, options, tableZoneID, appendOnly) {
    const nullString = "-";
    let columns = options.columns;
    if (!appendOnly) {
        let thead = options.thead == true ? `<thead><tr>${createThs(columns)}</tr></thead>` : "";
        let tbody = `<tbody id="item-area">${createTrs(columns, data)}</tbody>`;
        return thead + tbody;
    } else {
        return createTrs(options.columns, [data]);
    }


    function createThs(op) {
        let ths = op.map(o => {
            let w = typeof o.width == "string" ? o.width : o.width + "px";
            let th = `<th class="datatable-header ${o.required ? "required" : ''}" style="${o.width ? `width:${w}` : ''}"><span>${o.title}</span></th>`;
            return th;
        }).join("");
        if (options.delBtn) {
            ths += `<th class="datatable-header" style="width: 48px; text-align: center;"></th>`;
        }
        return ths;
    }

    function createTrs(op, ds) {
        return ds.map((d, i) => {
            let tr = `<tr id="${i}">${createTds(op, d, i)}</tr>`;
            return tr;
        }).join("");
    }

    function createTds(op, d, i) {
        let tds = op.map((o) => {
            let width = ""
            if (options.thead == false) {
                let w = typeof o.width == "string" ? o.width : o.width + "px";
                width = `style="${o.width ? `width:${w}` : ''}"`;
            }
            if (o.formatter) {
                return `<td id="d-${o.id}" ${width}>${o.formatter(d[o.id], d, i)}</td>`;
            }
            else {
                return `<td id="d-${o.id}" ${width}>${d[o.id] ?? nullString}</td>`;
            }
        }).join("");
        if (options.delBtn) {
            tds += `<td><button type="button" class="btn-delete-item" data-row="${i}" onclick="delTemplateItem(this, '${tableZoneID}')"></button></td>`;
        }
        return tds;
    }
}

function delTemplateItem(delBtn, tableZoneID) {
    if (!delBtn || !(delBtn instanceof HTMLElement)) {
        console.error("無效的按鈕元素:", delBtn);
        return;
    }

    let tr = delBtn.closest('tr');
    if (tr) {
        tr.remove();
        checkTableEmpty(tableZoneID);
    }
}

function checkTableEmpty(tableZoneID) {
    const tbody = document.getElementById('item-area');
    const datatable = document.getElementById(tableZoneID);
    if (tbody.children.length === 0 && datatable) {
        datatable.style.display = 'none';
    }
}



/**
 * @typedef {object} AccordionOuterOptions
 * @property {string?} className - use to datatable custom class
 * @property {string} title - use to datatable-header
 * @property {string} id - use to set datatable-table id
 * @property {string} inner - TableInner from createAccordion
 * 
 * @param {AccordionOuterOptions} options 
 * @returns {string} AccordionOuter
 */
function createAccordionOuter(options) {
    return `
    <div class="datatable ${options.className ?? ""}" id="${options.id ?? ''}">
        <div class="datatable-header">${options.title ?? ''}</div>
        <div class="datatable-body">
            <div class="accordion accordion-flush datatable-accordion" id="accordion-${options.id ?? ''}">
            ${options.inner ?? ''}
            </div>
        </div>
    </div>
    `;
}
/**
 * @typedef {object} AccordionOptions
 * @property {string?} className - use to datatable custom class
 * @property {string} title - use to datatable-header
 * @property {string} id - use to set accordion id
 * @property {*[]} data - if data.length === 0 will return empty string
 * @property {SerializedName[]} sn
 * @property {string} itemTitleKey - use to accordion-header title from data[itemTitleKey]
 * 
 * @param {AccordionOptions} options 
 * @returns {string} Accordion
 */


function createInspectionTable(options) {
    if (options.data.length === 0) {
        return "";
    }
    return `
    <div class="datatable border-0 w-100">
        <div class="datatable-body">
            <table class="datatable-table">
               ${createTableInner(options.data, options.sn)}
            </table>
        </div>
    </div>
    `;
}

function createAccordion(options) {
    console.log("o", options)
    if (options.data.length === 0) {
        return "";
    }
    return `
    <div class="datatable border-0 ${options.className ?? ""} ${options.id === 'EquipmentItem' ? 'sub-accordion' : ''}">
        <div class="datatable-body">
            <div class="accordion accordion-flush datatable-accordion d-flex flex-column" style="gap: 12px" id="accordion-${options.id ?? ''}">
               ${options.data.map((d, i) => {
        return createAccordionItem(options, i)
    }).join("")}
            </div>
        </div>
    </div>
    `;
}

/**
 * @param {AccordionOptions} options 
 * @param {number} i Accordion Item index
 * @returns {string} TableOuter
 */
function createAccordionItem(options, i) {
    console.log("createAccordionItem", options)
    console.log("options.state", options.state)
    const subContent = `
        <div class="subDatatable ${options.className ?? ""} ${options.id === 'EquipmentItem' ? 'sub-accordion' : ''}">
            <div class="datatable-body">
                <div class="accordion accordion-flush datatable-accordion" id="subAccordion-${options.id ?? ''}">
                    ${options.layer === 2 ? createSubAccordionItem(options, i) : ""}
                </div>
            </div>
        </div>
    `
    return `
            <div class="accordion-item" id="${options.id}-${i}">
                <h2 class="accordion-header" id="header-${options.id}-${i}">
                    <button class="accordion-button collapsed type="button"
                        data-bs-toggle="collapse"
                        data-bs-target="#sub-body-${options.id}-${i}" aria-expanded="false"
                        aria-controls="body-${options.id}-${i}">
                        ${options.icon ? `<i class="fa-solid fa-${options.icon} me-1" style="color: #2C5984;"></i>` : ""}
                        ${options.data[i][options.itemTitleKey]} ${options.itemSubTitleKey ? `(${options.data[i][options.itemSubTitleKey]})` : ""}
                    </button>
                </h2>
                <div id="sub-body-${options.id}-${i}" class="accordion-collapse collapse"
                    aria-labelledby="header-${options.id}-${i}">
                    <div class="accordion-body">
                        <div class="datatable border-0 w-100">
                            <div class="datatable-body">
                                <table class="datatable-table">
                                    ${createTableInner(options.data[i], options.sn)}
                                </table>
                                    ${options.state === '完成' ? subContent : ""}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `
}

function createSubAccordionItem(options, i) {
    const equip = options.data[i].EquipmentItem
    return equip.map((t, index) => {
        return `
        <div class="accordion-item" id="${options.id}_${i + 1}-${index + 1}">
        <h2 class="accordion-header" id="header-${options.id}_${i + 1}-${index + 1}">
            <button class="accordion-button collapsed type="button"
                data-bs-toggle="collapse"
                data-bs-target="#body-${options.id}_${i + 1}-${index + 1}" aria-expanded="false"
                aria-controls="body-${options.id}_${i + 1}-${index + 1}">
                ${t.IName}
            </button>
        </h2>
        <div id="body-${options.id}_${i + 1}-${index + 1}" class="accordion-collapse collapse"
            aria-labelledby="header-${options.id}_${i + 1}-${index + 1}">
            <div class="accordion-body">
                <div class="datatable border-0 w-100">
                    <div class="datatable-body">
                         <table class="datatable-table">
                            ${createTableInner(t, options.subsn)}
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
    `;
    }).join("");
}
/**
 * @typedef {object} DataDetailModalOptions
 * @property {string?} className - DataDetailModal custom class
 * @property {string} title - modal-title
 * @property {string} id - set modal id
 * @property {boolean | string | function()} locate - locate url
 * @property {*[]} data
 * @property {SerializedName[]} sn
 * 
 * @param {DataDetailModalOptions} options 
 */
function createDataDetailModal(options) {
    let ModalJQ, ModalBs, inner, locate;
    readData(options.data);
    function readData(data) {
        inner = createTableInner(data, options.sn);
        locate = options?.locate ? `<div class="modal-footer justify-content-center"><a type="button" class="btn btn-search" href="${options.locate?.() || options.locate}" target="_blank">定位</a></div>` : "";
        const html = `
        <div class="modal fade data-detail-modal ${options.className ?? ''}" tabindex="-1" id="${options.id ?? ''}">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${options.title ?? ''}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <table class="datatable-table">${inner}</table>
                    </div>
                    ${locate}
                </div>
            </div>
        </div>
        `;
        ModalJQ = $(html);
        $(document.body).append(ModalJQ);
        ModalBs = new bootstrap.Modal(ModalJQ[0]);

        ModalBs.show();

        ModalJQ[0].addEventListener("hidden.bs.modal", function () {
            ModalBs.dispose();
            ModalJQ.remove();
        })
    }
}

/**
 * 
 * @typedef {object} DialogModalButtonOptions
 * @property {string?} className - use to custom class, default is 'btn btn-search'
 * @property {boolean?} cancel - if true add 'data-bs-dismiss="modal"'
 * @property {string} text - use to show button inner text
 * @property {function?} onClick - use to add click event
 * 
 * @typedef {object} DialogModalOptions
 * @property {string?} className - use to custom class
 * @property {string?} title - use to modal-header
 * @property {string} id - use to set accordion id
 * @property {string} inner - use to modal-body context
 * @property {DialogModalButtonOptions[]} button - use to button in modal-footer
 * @property {function?} onShow - when "show.bs.modal" event trigger
 * @property {function?} onShown - when "shown.bs.modal" event trigger
 * @property {function?} onHide - when "hide.bs.modal" event trigger
 * @property {function?} onHidden - when "hidden.bs.modal" event trigger
 * 
 * @param {DialogModalOptions} options 
 */
function createDialogModal(options) {
    //if inner is html then use the html title tag text content
    let htmlTitle = options.inner?.match(/(?<=<title>)([\s\S]+)(?=<\/title>)/g)?.[0]
    let modal = $(`
        <div class="modal fade modal-delete ${options.className ?? ''}" id="${options.id ?? ''}" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    ${options.title ? `<div class="modal-header"><h5 class="modal-title">${options.title}</h5><button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button></div>` : ''}
                    <div class="modal-body text-center pb-2">${htmlTitle ?? options.inner ?? ''}</div>
                    <div class="modal-footer justify-content-center pb-4"></div>
                </div>
            </div>
        </div>
        `);

    if (options.button === undefined) {
        modal.find(".modal-footer").append(createButton({ className: "btn btn-delete", cancel: true, text: "確定" }));
    }
    else if (options.button !== null || options.button !== false) {
        options.button.length !== 0 && options.button.forEach((b) => {
            modal.find(".modal-footer").append(createButton(b));
        })
    }


    let myModal = bootstrap.Modal.getOrCreateInstance(modal[0]);
    myModal.show();

    options.onShow && modal[0].addEventListener("show.bs.modal", options.onShow);

    options.onShown && modal[0].addEventListener("shown.bs.modal", options.onShown);

    options.onHide && modal[0].addEventListener("hide.bs.modal", options.onHide);

    modal[0].addEventListener("hidden.bs.modal", () => {
        options.onHidden && options.onHidden();
        myModal.dispose();
        modal.remove();
    })

    return myModal;

    function createButton(options) {
        let btn = $(`<button type="button"
            class="${options.className ?? 'btn btn-search'}"
            ${options.cancel ? 'data-bs-dismiss="modal"' : ''}>${options.text}</button>`);
        options.onClick ? btn.click(options.onClick) : 0;
        return btn;
    }
}


/**
 * @typedef {object} DeleteDialogOptions
 * @property {string?} type default is post
 * @property {string} url onDelete URL
 * @property {string} data ajax data to post
 * @property {string} onSuccess
 * 
 * @param {DeleteDialogOptions} options 
 */
function createDeleteDialog(options) {
    let modal = createDialogModal({
        id: "DialogModal-Delete",
        inner: `
            <div class="d-flex justify-content-center align-items-center gap-2">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                    class="bi bi-exclamation-triangle-fill" viewBox="0 0 16 16">
                    <path
                        d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z" />
                </svg>
                確認是否刪除？
            </div>
        `,
        button: [
            { className: "btn btn-cancel", cancel: true, text: "取消", },
            { className: "btn btn-delete", text: "確定刪除", onClick: onDelete },
        ]
    })

    function onDelete(e) {
        let spinner = ` <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`;
        $(this).append(spinner);

        $.ajax({
            url: options.url,
            data: JSON.stringify(options.data),
            type: options.type ?? "POST",
            dataType: "json",
            contentType: "application/json;charset=utf-8",
            success: onSuccess,
            error: onError
        })


        function onSuccess(res) {
            console.log(res);
            modal.hide();
            createDialogModal({
                id: "DialogModal-Success",
                inner: "刪除成功！",
                onHide: options.onSuccess
            })
        }

        function onError(res) {
            console.log(res);
            modal.hide();

            createDialogModal({ id: "DialogModal-Error", inner: "刪除失敗！" })
        }
    }
}

// form-tooltip-toggle init
window.addEventListener('load', () => {
    Array.from(document.querySelectorAll('.form-tooltip-toggle')).forEach((el) => {
        let aaa;
        const t = new bootstrap.Tooltip(el, {
            boundary: document.body,
            customClass: 'form-tooltip',
            offset: [0, 0],
            trigger: 'click',
            popperConfig(defaultBsPopperConfig) {
                const newPopperConfig = { ...defaultBsPopperConfig }
                aaa = newPopperConfig;
                const placement = el.getAttribute('data-bs-placement')
                console.log('placement', placement);

                
                if (placement === 'auto') return defaultBsPopperConfig;

                newPopperConfig.placement = placement;

                if (placement === 'top-start') {
                    const modifier_arrow = defaultBsPopperConfig.modifiers.find(x => x.name === 'arrow');
                    const modifier_offset = defaultBsPopperConfig.modifiers.find(x => x.name === 'offset');
                    
                    if (modifier_arrow) {
                        modifier_arrow.options.padding = 24;
                    }
                    if (modifier_offset) {
                        modifier_offset.options.offset = [-24, 0];
                    }
                }
                return newPopperConfig
            }
        })
        console.log(t, aaa);
        el.addEventListener('shown.bs.tooltip', () => {
            console.log('shown.bs.tooltip', aaa);
        })

    })
}, { once: true })