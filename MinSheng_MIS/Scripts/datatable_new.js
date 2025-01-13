class DataTable {
    static instance = new DataTable();
    static nullString = "-";
    constructor() {
        if (!DataTable.instance) {
            DataTable.instance = this;
        }
        window.DT = DataTable.instance;
        window.addEventListener(
            "load",
            () => {
                this.setupFormTooltip();
            },
            { once: true }
        );
    }
    /**
     * Retrieves a DOM element based on the provided selector.
     *
     * @param {string|HTMLElement} selector - The selector to identify the element.
     *                                        Can be a string (ID or CSS selector) or an HTMLElement.
     * @returns {HTMLElement|null} The found DOM element, or null if not found or an error occurs.
     */
    getContainer(selector) {
        try {
            if (selector instanceof HTMLElement) return selector;
            return document.getElementById(selector) ?? document.querySelector(selector);
        } catch (error) {
            console.error(`[DataTable.getContainer] Error getting container, selector is: `, selector);
            return null;
        }
    }

    /**
     * Retrieves an array of DOM elements based on the provided selector.
     *
     * @param {string|HTMLElement[]} selector - The selector to identify the elements.
     *                                          Can be a string (CSS selector) or an array of HTMLElements.
     * @returns {HTMLElement[]} An array of found DOM elements. Returns an empty array if no elements are found or if the input is invalid.
     */
    getContainers(selector) {
        if (typeof selector === "string") {
            return Array.from(document.querySelectorAll(selector));
        }
        if (selector.every((e) => e instanceof HTMLElement)) {
            return selector;
        }
        return [];
    }

    /**
     * Converts a function or value to a value.
     * @param {Function|any} f - The function or value to convert.
     * @param {...any} args - Arguments to pass to the function if it is a function.
     * @returns {any} - The resulting value.
     */
    toValue(f, ...args) {
        return typeof f === "function" ? f(...args) : f;
    }
    /**
     * Converts a function or value to a value, and add 'px' to the end of number.
     * @param {Function|any} f - The function or value to convert.
     * @param {...any} args - Arguments to pass to the function if it is a function.
     * @returns
     */
    toPx(f, ...args) {
        f = this.toValue(f, ...args);
        return typeof f == "string" ? f : f + "px";
    }
    /**
     * @typedef AccordionOptions
     * @property {Function|string?} id
     * @property {Function|string?} className
     * @property {Function|string?} itemClassName
     * @property {Function|string?} style
     * @property {Function|string?} icon
     * @property {Function|string?} iconClass
     * @property {Function|string?} iconStyle
     * @property {AccordionItemOptions[]} items
     *
     * @typedef AccordionItemOptions
     * @property {string?} type - Type of accordion item, defaults is "tr"
     * @property {string?} id
     * @property {TableInnerOptions?} items - When type is tr, need this options to create tr items
     * @property {AccordionOptions?} options - When type is accordion, need this options to create accordion
     *
     * Creates an accordion component with nested items.
     * @param {string} selector - The selector
     * @param {Object[]} data - The data to populate the accordion.
     * @param {AccordionOptions} options - Configuration options for the accordion.
     * @param {string} [parentId=0] - The ID of the parent accordion item.
     * @returns
     */
    createAccordion(selector, data, options, parentId = 0) {
        if (data.length === 0) {
            return;
        }
        const container = this.getContainer(selector);
        const id = this.toValue(options.id, data) ?? "";
        const className = this.toValue(options.className, data) ?? "d-flex flex-column";
        const style = this.toValue(options.style, data) ?? "gap: 12px";

        container.insertAdjacentHTML(
            "beforeend",
            `<div class="accordion accordion-flush datatable-accordion ${className}" style="${style}" id="accordion-${parentId}-${id}"></div>`
        );

        const accordion = container.querySelector(`#accordion-${parentId}-${id}`);
        data.forEach((d, i) => {
            this.createAccordionItem(accordion, d, options, i, parentId);
        });
    }
    /**
     * Creates an accordion item.
     * @param {string} selector - The selector
     * @param {Object} data - The data for the item.
     * @param {AccordionOptions} options - Configuration options for the item.
     * @param {number} [index=0] - The index of the item.
     * @param {string} [parentId=0] - The ID of the parent item.
     * @returns {string} - The HTML string for the accordion item.
     */
    createAccordionItem(selector, data, options, index = 0, parentId = 0) {
        /**
         * Reduces items to their respective types (accordion or tr).
         * @param {Array} items - The items to reduce.
         * @returns {Array} - The reduced items.
         */
        function itemTypeReducer(items) {
            return items.reduce((t, x) => {
                if (x.type === "accordion") {
                    if (t.length === 0 || t.at(-1).type !== "accordion") {
                        t.push({ type: "accordion", items: [] });
                    }
                } else {
                    if (t.length === 0 || t.at(-1).type !== "tr") {
                        t.push({ type: "tr", items: [] });
                    }
                }
                t.at(-1).items.push(x);
                return t;
            }, []);
        }
        const container = this.getContainer(selector);
        const id = this.toValue(options.id, data) ?? "";
        const title = this.toValue(options.title, data) ?? "";
        const itemClassName = this.toValue(options.itemClassName, data) ?? "";
        const itemId = `${parentId}-${id}-${index}`;
        const icon = this.toValue(options.icon, data);
        const iconClass = this.toValue(options.iconClass, data) ?? "";
        const iconStyle = this.toValue(options.iconStyle, data) ?? "";

        container.insertAdjacentHTML(
            "beforeend",
            `<div class="accordion-item ${itemClassName}" id="accordionItem-${itemId}">
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
                        <div class="datatable-body"></div>
                    </div>
                </div>
            </div>
        </div>`
        );
        const accordionItemBody = container.querySelector(`#body-${itemId} .datatable-body`);
        itemTypeReducer(options.items).forEach((e) => {
            switch (e.type) {
                case "accordion": {
                    e.items.forEach((a) => {
                        this.createAccordion(accordionItemBody, data[a.id], a.options, itemId);
                    });
                    break;
                }
                case "tr": {
                    accordionItemBody.insertAdjacentHTML("beforeend", `<table class="datatable-table"><tbody></tbody></table>`);
                    const table = accordionItemBody.querySelector(`table.datatable-table tbody`);
                    this.createTableInner(table, data, e.items);
                    break;
                }
            }
        });
    }

    /**
     * @typedef {object} TableOptions
     * @property {string} id - use to set datatable-table id
     * @property {object[]} data - The data for the table.
     * @property {string?} className - use to datatable custom class
     * @property {string?} bodyClassName - use to datatable-body custom class
     * @property {string?} title - use to datatable-header
     * @property {string?} type - The type for the table.
     * @property {string?} inner - TableInner from createTableInner
     * @property {TableInnerOptions[]|TableGridOptions} items - The items to populate the table.
     *
     * @param {string} selector - The selector
     * @param {TableOptions} options
     * @returns {string} TableOuter
     */
    createTable(selector, options) {
        const container = this.getContainer(selector);
        const id = this.toValue(options.id) ?? "";
        const className = this.toValue(options.className) ?? "";
        const bodyClassName = this.toValue(options.bodyClassName) ?? "";
        const title = this.toValue(options.title) ?? "";

        container.insertAdjacentHTML(
            "beforeend",
            `<div class="datatable ${className}" id="${id}">
            ${title ? `<div class="datatable-header">${title}</div>` : ""}
            <div class="datatable-body ${bodyClassName}">
                <table class="datatable-table" id="table-${id}"></table>
            </div>
        </div>`
        );

        switch (options.type) {
            case "custom": {
                const table = container.querySelector(`#table-${id}`);
                const inner = this.toValue(options.inner, options.data, options.items);
                table.insertAdjacentHTML("beforeend", inner);
                break;
            }
            case "grid": {
                const table = container.querySelector(`#table-${id}`);
                return this.createTableGrid(table, options.data, options.items);
                break;
            }
            case "table":
            default: {
                const table = container.querySelector(`#table-${id}`);
                table.insertAdjacentHTML("beforeend", `<tbody></tbody>`);
                const tbody = container.querySelector(`#table-${id} tbody`);
                this.createTableInner(tbody, options.data, options.items);
                break;
            }
        }
    }
    /**
     * @typedef {object} TableInnerOptions
     * @property {string} id - The key of data object
     * @property {string?} title - The title of data
     * @property {number|boolean?} colspan - The number of columns
     * @property {string?} type - The type of column
     * @property {string?} style - use to datatable custom style
     * @property {string?} className - use to datatable custom class
     * @property {function (string, object, number):string?} formatter - formatter(value, row, index), use to formatter td data
     * @property {Object[]|Object} items - When type is items, need this options to create tr items
     * @property {Object[]|Object} button - When type is btn, need this options to create buttons
     *
     * Creates the inner HTML for a table.
     * @param {string} selector - The selector
     * @param {Object} data - The data for the table.
     * @param {TableInnerOptions[]} items - The items to populate the table.
     * @returns {string} - The HTML string for the table inner.
     */
    createTableInner(selector, data, items) {
        let hasImageType = false;
        const self = this;
        const container = this.getContainer(selector);
        const maxColspan = getMaxColspan(items);

        items.forEach((e, i) => {
            const colspan = getColspan(e.colspan, maxColspan);
            const value = data[e.id] ?? DataTable.nullString;
            const style = this.toValue(e.style, value, data, i) ?? "";
            const className = this.toValue(e.className, value, data, i) ?? "";
            if (e.type === "items") {
                const arr = !Array.isArray(value) ? [value] : value;
                const rows = arr?.length || 0;
                if (rows === 0) {
                    return;
                }

                arr.forEach((d, i) => {
                    const tr = document.createElement("tr");

                    const sort = document.createElement("td");
                    sort.classList.add("datatable-table-td", "datatable-table-sort");
                    sort.textContent = `${i + 1}`;

                    const tds = getItemNumCells(d, e.items);

                    if (i === 0) {
                        const th = document.createElement("td");
                        th.classList.add("datatable-table-th");
                        th.textContent = this.toValue(e.title, value, data);
                        th.rowSpan = rows;
                        tr.append(th, sort, ...tds);
                    } else {
                        tr.append(sort, ...tds);
                    }
                    container.appendChild(tr);
                });
            } else {
                const tr = document.createElement("tr");
                const th = document.createElement("td");
                th.classList.add("datatable-table-th");
                th.textContent = this.toValue(e.title, value, data);
                const td = document.createElement("td");
                td.classList.add("datatable-table-td");
                if (className) {
                    td.classList.add(...className.split(" "));
                }
                if (colspan) {
                    td.colSpan = colspan;
                }
                td.id = `d-${e.value}`;
                td.style.cssText = style;

                this.setCellContent(td, data, e, i);

                tr.append(th, td);
                container.appendChild(tr);
            }
        });

        if (hasImageType) {
            this.createImageDisplay(".datatable-img-item>img");
        }
        /**
         * Gets the colspan attribute for a cell.
         * @param {number|boolean} c - The colspan value.
         * @param {number} max - The maximum colspan value.
         * @returns {number} - The colspan attribute.
         */
        function getColspan(c, max) {
            if (typeof c === "number" && c > 0) {
                return c;
            }
            if (max) {
                return max;
            }
            if (c === true) {
                return 3;
            }
            return null;
        }

        /**
         * Gets the maximum colspan value for the items.
         * @param {Object[]} items - The items to check.
         * @returns {number} - The maximum colspan value.
         */
        function getMaxColspan(items) {
            const ii = items.filter((x) => x.type === "items");
            let iic = 0;
            if (ii.length > 0) {
                ii.forEach((e) => {
                    iic = Math.max(e.items.length, iic);
                });
                return iic + 1;
            }
            return 0;
        }

        /**
         * Gets the cells for an item.
         * @param {Object} data - The data for the cells.
         * @param {TableInnerOptions[]} items - The items to populate the cells.
         * @returns {string} - The HTML string for the cells.
         */
        function getItemNumCells(data, items) {
            return items.map((e, index) => {
                const value = data[e.id] ?? DataTable.nullString;
                const style = self.toValue(e.style, value, data, index) ?? "";
                const className = self.toValue(e.className, value, data, index) ?? "";
                const colspan = maxColspan > items.length + 1 ? getColspan(e.colspan, maxColspan - items.length) : null;
                const width = e.width ? self.toPx(e.width, value) : null;
                const td = document.createElement("td");
                td.classList.add("datatable-table-td");
                td.style.cssText = style;
                if (className) {
                    td.classList.add(...className.split(" "));
                }
                if (colspan) {
                    td.colSpan = colspan;
                }
                if (width) {
                    td.style.width = width;
                }
                td.id = `d-${e.id}`;
                self.setCellContent(td, data, e, index);
                return td;
            });
        }
    }
    /**
     * @typedef {object} TableGridColumnsOptions
     * @property {string} id - use to html id and the key to find value from data
     * @property {string} title - use to th
     * @property {string?} width - ex: "30%"
     * @property {boolean?} required
     * @property {string?} thClassName
     * @property {string?} tdClassName
     * @property {function (string, object, number):string?} formatter - formatter(value, row, index), use to formatter td data
     *
     * @typedef {object} TableGridOptions
     * @property {TableGridColumnsOptions} columns - use to datatable-header
     * @property {boolean?} rownumbers - if true that show row index
     * @property {boolean?} thead - if true that have th
     * @property {string?} metaKey - The meta key for the table
     *
     * @param {string} selector - The selector
     * @param {*[]} data - the data need show
     * @param {TableGridOptions} options
     * @returns {string} TableGrid
     */
    createTableGrid(selector, data, options) {
        const self = this;
        const container = this.getContainer(selector);
        const { columns, rownumbers, metaKey } = options;

        container.insertAdjacentHTML("beforeend", `${options.thead == true ? `<thead><tr></tr></thead>` : ""}<tbody id="item-area"></tbody>`);
        const thead = container.querySelector("thead tr");
        const tbody = container.querySelector("tbody");

        createThs(thead, columns);
        createTrs();

        function createThs(thead, op) {
            if (rownumbers) {
                const th = document.createElement("th");
                th.classList.add("datatable-header");
                thead.appendChild(th);
            }
            op.forEach((o) => {
                const d = data.map((x) => x[o.id]);
                const width = o.width ? self.toPx(o.width, d) : null;
                const required = self.toValue(o.required, d);
                const className = self.toValue(o.thClassName, d) ?? "";
                const title = self.toValue(o.title, d) ?? "";

                const th = document.createElement("th");
                th.textContent = title;
                th.classList.add("datatable-header");
                if (className) {
                    th.classList.add(...className.split(" "));
                }
                if (required) {
                    th.classList.add("required");
                }
                if (width) {
                    th.style.width = width;
                }
                thead.appendChild(th);
            });
        }

        function createTrs() {
            data.forEach(createTr);
        }

        function createTr(d, i) {
            const tr = document.createElement("tr");
            tr.id = i;
            if (metaKey) {
                tr.dataset.metaKey = d[metaKey];
            }
            if (rownumbers) {
                const td = document.createElement("td");
                td.id = "d-_Index";
                td.textContent = `${i + 1}`;
                tr.appendChild(td);
            }
            tr.append(...createTds(columns, i));
            tbody.appendChild(tr);
        }

        function createTds(op, i) {
            return op.map((o) => createTd(o, i));
        }

        function createTd(o, i) {
            const d = data[i];
            const value = d[o.id] ?? DataTable.nullString;
            const style = self.toValue(o.style, value, d, i) ?? "";
            const width = o.width ? self.toPx(o.width, d) : null;
            const className = self.toValue(o.tdClassName, value, d, i) ?? "";
            const td = document.createElement("td");
            td.id = `d-${o.id}`;
            td.style.cssText = style;
            if (width) {
                td.style.width = width;
            }
            td.className = className;
            if (width) {
                td.style.width = width;
            }
            self.setCellContent(td, d, o, i);

            if (typeof o.onAdd === "function") {
                o.onAdd(value, d, i);
            }
            return td;
        }

        function getRow(key) {
            return tbody.querySelector(`tr[data-meta-key="${key}"]`);
        }
        function getRowData(key) {
            return data[getRowIndex(key)];
        }
        function getRowIndex(key) {
            return data.findIndex((x) => x[metaKey] === key);
        }
        function add(row) {
            data.push(row);
            createTr(row, data.length - 1)
            rownumbers && renderRownumber();
            checkTableShow();
        }
        function edit(row) {
            const index = getRowIndex(row[metaKey]);
            if (index === -1) {
                console.warn("[createTableGrid] Not Found row data.")
                return;
            }
            const rowDOM = getRow(row[metaKey]);
            const oldRowData = Object.assign({}, data[index]);
            const newRowData = Object.assign(data[index], row);
            columns.forEach((o) => {
                if (newRowData[o.id] === oldRowData[o.id]) return;
                const td = rowDOM.querySelector("#d-" + o.id);
                if (typeof o.onEdit === "function") {
                    return o.onEdit(newRowData, oldRowData, index);
                }
                const ntd = createTd(o, index)
                td.replaceWith(ntd)
            })
        }
        function remove(row) {
            const index = getRowIndex(row[metaKey]);
            if (index === -1) {
                console.warn("[createTableGrid] Not Found row data.")
                return;
            }
            columns.forEach((o) => {
                if (typeof o.onRemove === "function") {
                    return o.onRemove(row, index);
                }
            })
            const rowDOM = getRow(row[metaKey]);
            rowDOM.remove();
            data.splice(index, 1);

            rownumbers && renderRownumber();
            checkTableShow();
        }
        function renderRownumber() {
            Array.from(tbody.querySelectorAll('td#d-_Index'))
                .forEach((td, i) => {
                    td.textContent = i + 1;
                });
        }
        function checkTableShow() {
            if (data.length === 0) {
                container.closest(".datatable").style.display = 'none';
            }
            else {
                container.closest(".datatable").style.display = null;
            }
        }

        return {
            add,
            edit,
            remove,
            getRow,
            getRowData,
            getRowIndex,
            renderRownumber,
            checkTableShow,
        }
    }
    /**
     *
     * @param {HTMLTableCellElement} cell
     * @param {Object} data
     * @param {TableInnerOptions} options
     * @param {number} index
     */
    setCellContent(cell, data, options, index) {
        const value = data[options.id] ?? DataTable.nullString;
        const formatter = this.toValue(options.formatter, value, data, index) ?? value ?? DataTable.nullString;
        switch (options.type) {
            case "image": {
                hasImageType = true;
                this.setImageCellContent(cell, formatter);
                break;
            }
            case "url": {
                this.setFileCellContent(cell, formatter);
                break;
            }
            case "btn": {
                const bos = Array.isArray(options.button) ? options.button : [options.button];
                const nullable = this.toValue(options.nullable, value, data, index);

                if (nullable) {
                    if (value === DataTable.nullString) {
                        cell.textContent = DataTable.nullString;
                        return;
                    }
                }
                const btnClassName = this.toValue(options.btnClassName);
                const btnStyle = this.toValue(options.btnStyle);
                const container = document.createElement('div');
                container.className = btnClassName;
                container.style.cssText = btnStyle;

                bos.forEach((bo) => {
                    const btnText = this.toValue(bo.text, value, data) ?? null;
                    const className = this.toValue(bo.className, value, data) ?? "btn btn-search";
                    const btnIcon = this.toValue(bo.icon, value, data, index) ?? null;
                    const disabled = this.toValue(bo.disabled, value, data, index);
                    const btn = document.createElement("button");
                    btn.type = "button";
                    if (btnIcon) {
                        btn.innerHTML = `<i class="${btnIcon}"></i>${btnText ?? ""}`;
                    }
                    if (btnText) {
                        btn.textContent = btnText;
                    }
                    btn.className = className;
                    btn.onclick = (event) => {
                        bo.onClick(event, value, data);
                    };
                    btn.disabled = disabled;
                    container.appendChild(btn);
                });

                cell.appendChild(container);
                break;
            }
            case "pre": {
                const pre = document.createElement("pre")
                if (formatter instanceof HTMLElement) {
                    pre.appendChild(formatter)
                }
                else {
                    pre.textContent = formatter;
                }
                cell.appendChild(pre)
                break;
            }
            default: {
                if (formatter instanceof HTMLElement) {
                    cell.appendChild(formatter)
                }
                else {
                    cell.textContent = formatter;
                }
                break;
            }
        }
    }
    /**
     * Puts an image in the cell.
     * @param {HTMLTableCellElement} cell
     * @param {string[]|string} imgs - The images to put in the cell.
     * @returns {string} - The HTML string for the images.
     */
    setImageCellContent(cell, imgs) {
        if (!imgs) {
            cell.textContent = DataTable.nullString;
            return;
        }
        if (!Array.isArray(imgs)) {
            imgs = [imgs];
        }
        cell.insertAdjacentHTML(
            "beforeend",
            `<div class="datatable-img-area">${imgs.map((img) => `<div class="datatable-img-item"><img src="${img}"/></div>`).join("")}</div>`
        );
    }

    /**
     * Puts a file link in the cell.
     * @param {HTMLTableCellElement} cell
     * @param {string[]|string} urls - The URLs to put in the cell.
     * @returns {string} - The HTML string for the file links.
     */
    setFileCellContent(cell, urls) {
        if (!urls) {
            cell.textContent = DataTable.nullString;
            return;
        }
        if (!Array.isArray(urls)) {
            urls = [urls];
        }
        cell.insertAdjacentHTML("beforeend", urls.map((url) => `<a href="${url}" target="_blank">${url.split("/").at(-1)}</a>`).join("<br>"));
    }

    /**
     * Add Index at Array data.
     * @param {Object[]} arr - The data to add index.
     * @returns {Object[]} - The data with index.
     */
    createIndexToArrayData(arr) {
        return arr.map((e, i) => {
            return { Index: i + 1, ...e };
        });
    }

    /**
     * Sets up an image display functionality for elements matching the given selector.
     * When an image is clicked, it creates a full-screen display of the image with a close button.
     * Clicking outside the image or on the close button removes the display.
     *
     * @param {string|HTMLElement[]} selector - A CSS selector string to identify the elements to which the image display functionality should be applied.
     * @returns {void} This function does not return a value.
     */
    createImageDisplay(selector) {
        const elements = this.getContainers(selector);
        elements.forEach((element) => {
            element.addEventListener("click", function () {
                const img = this.src;
                const imgDisplay = document.createElement("div");
                imgDisplay.className = "img-display";
                imgDisplay.innerHTML = `
                <div class="img-display-div">
                    <button class="btn-close btn-close-white"></button>
                    <img src="${img}">
                </div>`;
                document.body.appendChild(imgDisplay);
                imgDisplay.onclick = close;
                imgDisplay.querySelector(".btn-close").onclick = close;

                function close() {
                    imgDisplay.remove();
                }
            });
        });
    }

    /**
     *
     * @typedef {object} DialogModalButtonOptions
     * @property {string?} className - use to custom class, default is 'btn btn-search'
     * @property {boolean?} cancel - if true add 'data-bs-dismiss="modal"', can click to close the modal
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
     * @param {DialogModalOptions|string} opt - if type is string, it will be passed to inner option.
     *
     * @example
     * ```js
     * createDialogModal("This is inner.")
     * ```
     *
     * @example
     * ```js
     * createDialogModal({
     *     title: "A Title",
     *     inner: "This is inner.",
     *     button: [
     *         { text: "OK", className: "btn btn-secondary", onClick: ()=>{} },
     *         { text: "Cancel", cancel: true }, // click this button to close modal.
     *     ]
     * })
     * ```
     */
    createDialogModal(opt) {
        const options = {};
        if (typeof opt !== "object") {
            options.inner = opt;
        } else {
            Object.assign(options, opt);
        }

        //if inner is html then use the html title tag text content
        const htmlTitle = options.inner?.match(/(?<=<title>)([\s\S]+)(?=<\/title>)/g)?.[0];
        const modalTemplate = `
            <div class="modal fade modal-delete ${options.className ?? ""}" id="${options.id ?? ""}" tabindex="-1">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        ${options.title
                ? `
                        <div class="modal-header">
                            <h5 class="modal-title">${options.title}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>`
                : ""
            }
                        <div class="modal-body text-center pb-2">
                            ${htmlTitle ?? options.inner ?? ""}
                        </div>
                        <div class="modal-footer justify-content-center pb-4"></div>
                    </div>
                </div>
            </div>`;
        const template = document.createElement("template");
        template.innerHTML = modalTemplate.trim();
        const modal = template.content.firstChild;
        const footer = modal.querySelector(".modal-footer");

        if (options.button === undefined) {
            footer.appendChild(createButton({ className: "btn btn-delete", cancel: true, text: "確定" }));
        } else if (options.button !== null || options.button !== false) {
            options.button.length !== 0 &&
                options.button.forEach((b) => {
                    footer.appendChild(createButton(b));
                });
        }

        const myModal = bootstrap.Modal.getOrCreateInstance(modal);
        myModal.show();

        options.onShow && modal.addEventListener("show.bs.modal", options.onShow);

        options.onShown && modal.addEventListener("shown.bs.modal", options.onShown);

        options.onHide && modal.addEventListener("hide.bs.modal", options.onHide);

        modal.addEventListener("hidden.bs.modal", () => {
            options.onHidden && options.onHidden();
            myModal.dispose();
            modal.remove();
        });

        return myModal;

        function createButton(options) {
            const btn = document.createElement("button");
            btn.type = "button";
            btn.className = options.className ?? "btn btn-search";
            btn.textContent = options.text ?? "取消";
            if (options.cancel) {
                btn.setAttribute("data-bs-dismiss", "modal");
            }
            if (options.onClick) {
                btn.addEventListener("click", options.onClick);
            }
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
    createDeleteDialog(options) {
        const self = this;
        const modal = createDialogModal({
            id: "DialogModal-Delete",
            inner: `
            <div class="d-flex justify-content-center align-items-center gap-2">
                <i class="fa-solid fa-triangle-exclamation" style="font-size: 24px;"></i>
                確認是否刪除？
            </div>`,
            button: [
                { className: "btn btn-cancel", cancel: true, text: "取消" },
                { className: "btn btn-delete", text: "確定刪除", onClick: onDelete },
            ],
        });

        function onDelete(e) {
            const spinner = ` <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`;
            $(this).append(spinner);

            $.ajax({
                url: options.url,
                data: options.data ?? JSON.stringify(options.data),
                type: options.type ?? "POST",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success: onSuccess,
                error: onError,
            });

            function onSuccess(res) {
                console.log(res);
                modal.hide();
                self.createDialogModal({
                    id: "DialogModal-Success",
                    inner: "刪除成功！",
                    onHide: options.onSuccess(res),
                });
            }

            function onError(res) {
                console.log(res.responseText);
                modal.hide();
                self.createDialogModal("刪除失敗！");
            }
        }
    }

    /**
     * bim map location modal
     * @param {Object} data
     * @returns
     */
    createMapModal(data) {
        if (typeof UpViewer === "undefined") {
            console.warn(
                [
                    "請先載入以下資源：",
                    "https://developer.api.autodesk.com/modelderivative/v2/viewers/style.min.css",
                    "/Content/loading.css",
                    "/Content/bim.css",
                    "https://developer.api.autodesk.com/modelderivative/v2/viewers/viewer3D.min.js",
                    "/Scripts/Forge/Viewer.Loading.js",
                    "/Scripts/Forge/Viewer.Toolkit.js",
                    "/Scripts/Forge/ForgePin.js",
                    "/Scripts/Forge/UpViewer.js",
                ].join("\n")
            );
            return;
        }

        const modalTemplate = `<div class="modal fade modal-delete" id="Location" tabindex="-1">
            <div class="modal-dialog modal-dialog-centered modal-screen-md">
                <div class="modal-content bg-white rounded-0">
                    <div class="modal-header p-2 rounded-0" style="background: #D9EFFD; border-bottom: 1px solid #8A9BA5;">
                        <h5 class="modal-title w-100 text-center">定位資訊</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body text-center" style="padding: 0;">
                        <div id="BIM" style="height: 50vh;">
                            <div class="pin-area" id="pin-area"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>`;
        const template = document.createElement("template");
        template.innerHTML = modalTemplate.trim();
        const modal = template.content.firstChild;

        const bim = new UpViewer(modal.querySelector("#BIM"));
        const myModal = bootstrap.Modal.getOrCreateInstance(modal);
        modal.addEventListener("hidden.bs.modal", () => {
            bim.dispose();
            myModal.dispose();
            modal.remove();
            document.body.focus();
        });

        modal.addEventListener("shown.bs.modal", async function (event) {
            if (bim.equipmentPoint) {
                bim.equipmentPoint.hide();
            }
            await bim.init();
            await bim.loadModels(bim.getModelsUrl(data.RFIDViewName));
            const position = new THREE.Vector3(data.Location_X, data.Location_Y, 0);
            console.log("position", position);
            const tool = bim.activateEquipmentPointTool(position, false);
            tool.setPosition(position);
        });

        myModal.show();

        return myModal;
    }

    /**
     * setup form-tooltip-toggle
     * @param {string?} selector
     */
    setupFormTooltip(selector = ".form-tooltip-toggle") {
        const elements = this.getContainers(selector);
        elements.forEach((el) => {
            new bootstrap.Tooltip(el, {
                boundary: document.body,
                customClass: "form-tooltip",
                offset: [0, 0],
                //trigger: 'click',
                popperConfig(defaultBsPopperConfig) {
                    const newPopperConfig = { ...defaultBsPopperConfig };
                    const placement = el.getAttribute("data-bs-placement");
                    //console.log("placement", placement);

                    if (placement === "auto") return defaultBsPopperConfig;

                    newPopperConfig.placement = placement;

                    if (placement === "top-start") {
                        const modifier_arrow = defaultBsPopperConfig.modifiers.find((x) => x.name === "arrow");
                        const modifier_offset = defaultBsPopperConfig.modifiers.find((x) => x.name === "offset");

                        if (modifier_arrow) {
                            modifier_arrow.options.padding = 24;
                        }
                        if (modifier_offset) {
                            modifier_offset.options.offset = [-24, 0];
                        }
                    }
                    return newPopperConfig;
                },
            });
        });
    }

    setupFormTab(defaultTabName, { onTabChange = null } = {}) {
        // Determine the default tab if not provided
        if (!defaultTabName) {
            defaultTabName = document.querySelector(".form-tab-btn.active")?.dataset?.tabName || document.querySelector(".form-tab-btn")?.dataset?.tabName;
        }

        if (!defaultTabName) {
            console.error("Error: No default tab found. Ensure tab buttons have a 'data-tab-name' attribute.");
            return null;
        }

        const tabManager = {
            currentTab: defaultTabName,
            links: Array.from(document.querySelectorAll(".form-tab-btn")),
            contents: Array.from(document.querySelectorAll("[data-tab-content]")),
            comments: [],
            openTab,
            getCurrentTabLink,
            getCurrentTabContent,
        };
        // Pre-generate comment nodes for content placeholders
        tabManager.comments = tabManager.contents.map((content) => {
            const comment = document.createComment(" ");
            comment.tabName = content.dataset.tabContent;
            return comment;
        });
        // Set up click event listeners for tab links
        tabManager.links.forEach((link) => {
            link.addEventListener("click", () => openTab(link.dataset.tabName));
        });
        openTab(tabManager.currentTab);

        return tabManager;

        // Function to open a tab
        function openTab(tabName) {
            if (!tabName) return;

            const previousTab = tabManager.currentTab;
            tabManager.currentTab = tabName;

            // Update tab link styles
            tabManager.links.forEach((link) => link.classList.toggle("active", link.dataset.tabName === tabName));

            // Update tab contents
            tabManager.contents.forEach((content) => {
                if (content.dataset.tabContent === tabName) {
                    getComment(tabName).replaceWith(content);
                    return;
                }
                content.replaceWith(getComment(content.dataset.tabContent));
            });

            // Trigger the onTabChange callback if provided
            if (typeof onTabChange === "function") {
                onTabChange({
                    previousTab,
                    currentTab: tabName,
                });
            }
        }

        // Get the current active tab link
        function getCurrentTabLink() {
            return tabManager.links.find((link) => link.dataset.tabName === tabManager.currentTab);
        }

        // Get the current active tab content
        function getCurrentTabContent() {
            return tabManager.contents.find((content) => content.dataset.tabContent === tabManager.currentTab);
        }

        // Get the corresponding comment placeholder for a tab
        function getComment(tabName) {
            return tabManager.comments.find((comment) => comment.tabName === tabName);
        }
    }
}
