/**
 * Creates an accordion component with nested items.
 * @param {Array} data - The data to populate the accordion.
 * @param {Object} options - Configuration options for the accordion.
 * @param {string} [parentId=0] - The ID of the parent accordion item.
 * @returns {string} - The HTML string for the accordion component.
 */
function createAccordion(data, options, parentId = 0) {
    /**
     * Converts a function or value to a value.
     * @param {Function|any} f - The function or value to convert.
     * @param {...any} args - Arguments to pass to the function if it is a function.
     * @returns {any} - The resulting value.
     */
    function toValue(f, ...args) {
        return typeof f === 'function' ? f(...args) : f;
    }

    /**
     * Reduces items to their respective types (accordion or tr).
     * @param {Array} items - The items to reduce.
     * @returns {Array} - The reduced items.
     */
    function itemTypeReducer(items) {
        return items.reduce((t, x) => {
            if (x.type === "accordion") {
                if (t.length === 0 || t.at(-1).type !== 'accordion') {
                    t.push({ type: 'accordion', items: [] });
                }
            } else {
                if (t.length === 0 || t.at(-1).type !== 'tr') {
                    t.push({ type: 'tr', items: [] });
                }
            }
            t.at(-1).items.push(x);
            return t;
        }, []);
    }

    if (data.length === 0) {
        return "";
    }

    const id = toValue(options.id, data) ?? "";
    const className = toValue(options.className, data) ?? "d-flex flex-column";
    const style = toValue(options.style, data) ?? "gap: 12px";

    const inner = data.map((d, i) => {
        return createAccordionItem(d, options, i, parentId);
    }).join("");

    return `<div class="accordion accordion-flush datatable-accordion ${className}" style="${style}" id="accordion-${parentId}-${id}">${inner}</div>`;

    /**
     * Creates an accordion item.
     * @param {Object} data - The data for the item.
     * @param {Object} options - Configuration options for the item.
     * @param {number} [index=0] - The index of the item.
     * @param {string} [parentId=0] - The ID of the parent item.
     * @returns {string} - The HTML string for the accordion item.
     */
    function createAccordionItem(data, options, index = 0, parentId = 0) {
        const id = toValue(options.id, data) ?? "";
        const title = toValue(options.title, data) ?? "";
        const itemClassName = toValue(options.itemClassName, data) ?? "";
        const itemId = `${parentId}-${id}-${index}`;
        const icon = toValue(options.icon, data);
        const iconClass = toValue(options.iconClass, data) ?? "";
        const iconStyle = toValue(options.iconStyle, data) ?? "";
        const inner = itemTypeReducer(options.items).map((e) => {
            switch (e.type) {
                case "accordion": {
                    return e.items.map((a) => {
                        return createAccordion(data[a.value], a.options, itemId);
                    }).join('');
                }
                case "tr": {
                    return `<table class="datatable-table">${createTableInner(data, e.items)}</table>`;
                }
            }
        }).join('');

        return `<div class="accordion-item ${itemClassName}" id="accordionItem-${itemId}">
            <h2 class="accordion-header" id="header-${itemId}">
                <button class="accordion-button collapsed" type="button"
                    data-bs-toggle="collapse"
                    data-bs-target="#body-${itemId}" aria-expanded="false"
                    aria-controls="body-${itemId}">
                    ${icon ? `<i class="fa-solid fa-${icon} ${iconClass}" style="${iconStyle}"></i>` : ""}
                    ${title}
                </button>
            </h2>
            <div id="body-${itemId}" class="accordion-collapse collapse"
                aria-labelledby="header-${itemId}">
                <div class="accordion-body">
                    <div class="datatable border-0 w-100">
                        <div class="datatable-body">${inner}</div>
                    </div>
                </div>
            </div>
        </div>`;
    }

    /**
     * Creates the inner HTML for a table.
     * @param {Object} data - The data for the table.
     * @param {Array} items - The items to populate the table.
     * @returns {string} - The HTML string for the table inner.
     */
    function createTableInner(data, items) {
        const nullString = "-";
        const maxColspan = getMaxColspan(items);

        return items.map((e, i) => {
            const colspan = getColspan(e.colspan, maxColspan);

            switch (e.type) {
                case "items": {
                    let arr = data[e.value];
                    if (!Array.isArray(arr)) { arr = [data[e.value]]; }
                    const rows = data[e.value]?.length || 0;
                    if (rows === 0) {
                        return '';
                    }

                    const first = data[e.value][0];
                    const other = data[e.value]?.slice(1).map((item, i) => {
                        return `<tr>
                            <td class="datatable-table-td datatable-table-sort">${i + 2}</td>
                            ${getItemNumCells({ items: e.items, key: e.value, data: item })}
                        </tr>`;
                    }).join("");

                    return `<tr>
                        <td class="datatable-table-th" rowspan="${rows}">${toValue(e.text, data[e.value], data)}</td>
                        <td class="datatable-table-td datatable-table-sort">1</td>
                        ${getItemNumCells({ items: e.items, key: e.value, data: first })}
                    </tr>${other}`;

                    /**
                     * Gets the cells for an item.
                     * @param {Object} params - The parameters for the function.
                     * @param {Array} params.items - The items to populate the cells.
                     * @param {string} params.key - The key for the data.
                     * @param {Object} params.data - The data for the cells.
                     * @returns {string} - The HTML string for the cells.
                     */
                    function getItemNumCells({ items, key, data }) {
                        return items.map((e, index) => {
                            let colspan = '';
                            if (maxColspan > items.length + 1) {
                                colspan = getColspan(e.colspan, maxColspan - items.length);
                            }
                            const value = data[e.value] ?? nullString;
                            const style = toValue(e.style, value, data, index) ?? "";
                            const className = toValue(e.className, value, data, index) ?? "";
                            const cellClass = index === 0 ? 'text-start' : 'text-center';
                            const formatter = toValue(e.formatter, value, data, index) ?? value ?? nullString;
                            return `<td class="datatable-table-td ${cellClass} ${className}" style="${style}" ${colspan}>${formatter}</td>`;
                        }).join('');
                    }
                }
                case "image": {
                    return `<tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" ${colspan} id="d-${e.value}">${data[e.value] != null ? putImage(data[e.value]) : nullString}</td>
                    </tr>`;
                }
                case "url": {
                    return `<tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" ${colspan} id="d-${e.value}">${data[e.value] != null ? putFile(data[e.value]) : nullString}</td>
                    </tr>`;
                }
                case "btn": {
                    allData[data.RFIDInternalCode] = data;
                    return `<tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" ${colspan} id="d-${e.value}">
                            <button type="button" class="btn btn-search" onclick="createMapModal('Modal-Location-${data.RFIDInternalCode}','${data.RFIDInternalCode}')">查看定位</button>
                        </td>
                    </tr>`;
                }
                case "pre": {
                    return `<tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" ${colspan} id="d-${e.value}"><pre>${data[e.value] ?? nullString}</pre></td>
                    </tr>`;
                }
                default: {
                    return `<tr>
                        <td class="datatable-table-th">${e.text}</td>
                        <td class="datatable-table-td" ${colspan} id="d-${e.value}">${data[e.value] ?? nullString}</td>
                    </tr>`;
                }
            }
        }).join("");

        /**
         * Puts an image in the cell.
         * @param {Array|string} imgs - The images to put in the cell.
         * @returns {string} - The HTML string for the images.
         */
        function putImage(imgs) {
            if (Array.isArray(imgs) && imgs.length !== 0) {
                return `<div class="datatable-img-area">${imgs.map(img => {
                    return `<div class="datatable-img-item"><img src="${img}"/></div>`;
                }).join("")}</div>`;
            } else if (typeof imgs === 'string' || imgs instanceof String) {
                return `<div class="datatable-img-area"><div class="datatable-img-item"><img src="${imgs}"/></div></div>`;
            }
            return nullString;
        }

        /**
         * Puts a file link in the cell.
         * @param {Array|string} urls - The URLs to put in the cell.
         * @returns {string} - The HTML string for the file links.
         */
        function putFile(urls) {
            if (Array.isArray(urls) && urls.length !== 0) {
                return urls.map(url => { return `<a href="${url}" target="_blank">${url.split('/').at(-1)}</a>` }).join("<br>");
            } else if (typeof urls === 'string' || urls instanceof String) {
                return `<a href="${urls}" target="_blank">${urls.split('/').at(-1)}</a>`;
            }
            return nullString;
        }

        /**
         * Gets the colspan attribute for a cell.
         * @param {number|boolean} c - The colspan value.
         * @param {number} max - The maximum colspan value.
         * @returns {string} - The colspan attribute.
         */
        function getColspan(c, max) {
            if (c) {
                return `colspan="${c}"`;
            }
            if (max) {
                return `colspan="${max}"`;
            }
            if (c === true) {
                return `colspan="3"`;
            }
            return '';
        }

        /**
         * Gets the maximum colspan value for the items.
         * @param {Array} items - The items to check.
         * @returns {number} - The maximum colspan value.
         */
        function getMaxColspan(items) {
            const ii = items.filter(x => x.type === "items");
            let iic = 0;
            if (ii.length > 0) {
                ii.forEach((e) => {
                    iic = Math.max(e.items.length, iic);
                });
                return iic + 1;
            }
            return 0;
        }
    }
}