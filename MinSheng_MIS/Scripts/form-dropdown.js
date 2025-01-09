class formDropdown {
    constructor() { }

    static getSelect(id) {
        if (id instanceof HTMLElement) return id;
        return document.getElementById(id) ?? document.querySelector(id);
    }

    static setValue(id, value) {
        if (!value) return;
        const select = formDropdown.getSelect(id);
        if (!select) {
            console.warn(`[formDropdown.setValue] not found select element with id "${id}".`);
            return;
        }
        select.value = value;
    }
    static reset(select, placeholder) {
        // 清空下拉選單
        select.innerHTML = "";
        if (!placeholder) return;
        // 添加默認選項
        const defaultOption = document.createElement("option");
        defaultOption.value = "";
        defaultOption.textContent = placeholder;
        select.appendChild(defaultOption);
    }

    static addResetEvent(select, callback = () => { }) {
        const form = select.closest("form");
        if (!form) {
            console.warn(`[formDropdown.addResetEvent] not found form element closest select "${select}".`);
            return;
        }

        const resetButton = form.querySelector('[type="reset"]');

        if (resetButton) {
            resetButton.addEventListener("click", callback);
        }
    }

    static async pushSelect({ id, url, name = "Text", value = "Value", placeholder = "請選擇", filter } = {}) {
        const select = formDropdown.getSelect(id);
        if (!select) {
            console.warn(`[formDropdown.setValue] not found select element with id "${id}".`);
            return;
        }

        try {
            const response = await fetch(url);
            let res = await response.json();
            if (filter) {
                res = filter(res);
            } else {
                res = res.Datas || res;
            }
            formDropdown.reset(select, placeholder);

            // 動態添加其他選項
            res.forEach((item) => {
                const option = document.createElement("option");
                option.value = item[value];
                option.textContent = item[name];
                select.appendChild(option);
            });
        } catch (error) {
            console.error("Error fetching data:", error);
            formDropdown.reset(select, placeholder);
        }
        return select;
    }
    static async postPushSelect({ id, url, data, name = "Text", value = "Value", placeholder = "請選擇", useFormData = false, filter } = {}) {
        const select = formDropdown.getSelect(id);
        if (!select) {
            console.warn(`[formDropdown.setValue] not found select element with id "${id}".`);
            return;
        }

        try {
            let res = null;
            if (useFormData && data) {
                const formData = new FormData();
                Object.keys(data).forEach((key) => {
                    formData.append(key, data[key]);
                });
                res = await fetch(url, {
                    method: "POST",
                    body: formData,
                }).then((res) => res.json());
            } else {
                res = await fetch(url, {
                    method: "POST",
                    body: JSON.stringify(data),
                    headers: new Headers({
                        "Content-Type": "application/json",
                    }),
                }).then((res) => res.json());
            }
            // data filter
            if (filter) {
                res = filter(res);
            } else {
                res = res.Datas || res;
            }

            formDropdown.reset(select, placeholder);

            // 動態添加其他選項
            res.forEach((item) => {
                const option = document.createElement("option");
                option.value = item[value];
                option.textContent = item[name];
                select.appendChild(option);
            });
        } catch (error) {
            console.error("Error fetching data:", error);
            formDropdown.reset(select, placeholder);
        }

        return select;
    }

    static async ASN({ id = "ASN", fsnId = "FSN", value = null, fsnValue = null, placeholder = "請選擇" } = {}) {
        const asn = await formDropdown.pushSelect({ id, url: "/DropDownList/Area", placeholder });
        const fsn = formDropdown.getSelect(fsnId);
        const initialized = asn.dataset?.fdInitialized;
        formDropdown.setValue(asn, value);
        if (fsn) {
            await formDropdown.FSN({ id: fsn, data: value, value: fsnValue });
            if (!initialized) {
                return asn;
            }
            asn.dataset.fdInitialized = true;
            asn.addEventListener("change", async function (e) {
                await formDropdown.FSN({ id: fsn, data: asn.value, placeholder: asn.value ? placeholder : void 0 });
            });
            formDropdown.addResetEvent(asn, async () => {
                await formDropdown.FSN({ id: fsn, data: null });
            });

        }
        return asn;
    }
    static async FSN({ id = "FSN", data, value, placeholder = "請先選擇棟別" } = {}) {
        const fsn = await formDropdown.pushSelect({ id, url: `/DropDownList/Floor?ASN=${data}`, placeholder });
        formDropdown.setValue(fsn, value);
        return fsn;
    }

    static async StockTypeSN({ id = "StockTypeSN", sisnId = "SISN", unitId = null, value = null, sisnValue = null, placeholder = "請選擇" } = {}) {
        const sysn = await formDropdown.pushSelect({ id, url: "/DropDownList/StockType", placeholder });
        const sisn = formDropdown.getSelect(sisnId);
        const unit = formDropdown.getSelect(unitId);
        const initialized = sysn.dataset?.fdInitialized;
        formDropdown.setValue(sysn, value);
        if (sisn) {
            await formDropdown.SISN({ id: sisn, data: value, value: sisnValue });
            if (initialized) {
                return sysn;
            }
            sysn.dataset.fdInitialized = true;
            sysn.addEventListener("change", async function (e) {
                await formDropdown.SISN({ id: sisn, data: sysn.value, placeholder: sysn.value ? placeholder : void 0 });
            });
            formDropdown.addResetEvent(sysn, async () => {
                await formDropdown.SISN({ id: sisn, data: null });
            });

            if (unit) {
                sisn.addEventListener("change", async function (e) {
                    const res = await fetch(`/Stock_Management/GetComputationalStockDetail?id=${sisn.value}`)
                        .then(r => r.json())
                        .then(r => r.Datas)
                        .catch(err => null)
                    unit.textContent = res?.Unit ?? "";
                });
            }
        }
        return sysn;
    }
    static async SISN({ id = "FSN", data, value = null, placeholder = "請先選擇類別" } = {}) {
        const sisn = await formDropdown.pushSelect({ id, url: `/DropDownList/StockName?StockTypeSN=${data}`, placeholder });
        formDropdown.setValue(sisn, value);
        return sisn;
    }
}
