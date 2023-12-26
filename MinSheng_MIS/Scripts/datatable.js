/**
 * @typedef {object} SerializedName
 * @property {string} text - use to th
 * @property {string} value - use to td
 * @property {function (value):string?} formatter - use to formatter td data
 * @property {boolean} image - set true if value is image
 * @property {boolean} url - set true if value is url
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
        let html;
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
        else {
            switch (e.value) {
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
                default:
                    html = `
                        <tr>
                            <td class="datatable-table-th">${e.text}</td>
                            <td class="datatable-table-td" id="d-${e.value}">${data[e.value] ?? nullString}</td>
                        </tr>`;
                    break;
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
 * @property {function (value):string?} formatter - use to formatter td data
 * 
 * @typedef {object} TableGridOptions
 * @property {TableGridColumnsOptions} columns - use to datatable-header
 * @property {boolean?} thead - if true that have th
 * 
 * @param {*[]} data - the data need show
 * @param {TableGridOptions} options 
 * @returns {string} TableGrid
 */
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
            let th = `<th class="datatable-header ${o.required ? "required" : ''}" style="${o.width ? `width:${w}` : ''}"><span>${o.title}</span></th>`;
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
function createAccordion(options) {
    if (options.data.length === 0) {
        return "";
    }
    return `
    <div class="datatable ${options.className ?? ""}">
        <div class="datatable-header">${options.title ?? ''}</div>
        <div class="datatable-body">
            <div class="accordion accordion-flush datatable-accordion" id="accordion-${options.id ?? ''}">
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

/**
 * @typedef {object} DataDetailModalOptions
 * @property {string?} className - use to DataDetailModal custom class
 * @property {string} title - use to modal-title
 * @property {string} id - use to set modal id
 * @property {*[]} data
 * @property {SerializedName[]} sn
 * 
 * @param {DataDetailModalOptions} options 
 */
function createDataDetailModal(options) {
    let ModalJQ, ModalBs, inner;
    readData(options.data);
    function readData(data) {
        inner = createTableInner(data, options.sn);
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
                    <div class="modal-footer justify-content-center">
                        <a type="button" class="btn btn-search" href="" target="_blank">定位</a>
                    </div>
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

    let modal = $(`
        <div class="modal fade modal-delete ${options.className ?? ''}" id="${options.id ?? ''}" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    ${options.title ? `
                    <div class="modal-header">
                        <h5 class="modal-title">${options.title}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    `: ''}
                    <div class="modal-body text-center">
                        ${options.inner ?? ''}
                    </div>
                    <div class="modal-footer justify-content-center"></div>
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
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="currentColor"
                class="bi bi-exclamation-triangle-fill" viewBox="0 0 16 16">
                <path
                    d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z" />
            </svg>
            確認是否刪除？
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