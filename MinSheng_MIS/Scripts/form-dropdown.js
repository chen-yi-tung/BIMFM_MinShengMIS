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
        const select = await formDropdown.pushSelect({ id, url: "/DropDownList/Area", placeholder });
        const subSelect = formDropdown.getSelect(fsnId);
        const initialized = select.dataset?.fdInitialized;
        formDropdown.setValue(select, value);
        if (subSelect) {
            await formDropdown.FSN({ id: subSelect, data: value, value: fsnValue });
            if (initialized) {
                return select;
            }
            select.dataset.fdInitialized = true;
            select.addEventListener("change", async function (e) {
                await formDropdown.FSN({ id: subSelect, data: select.value, placeholder: select.value ? placeholder : void 0 });
            });
            formDropdown.addResetEvent(select, async () => {
                await formDropdown.FSN({ id: subSelect, data: null });
            });

        }
        return select;
    }
    static async FSN({ id = "FSN", data, value, placeholder = "請先選擇棟別" } = {}) {
        const select = await formDropdown.pushSelect({ id, url: `/DropDownList/Floor?ASN=${data}`, placeholder });
        formDropdown.setValue(select, value);
        return select;
    }

    static async StockTypeSN({ id = "StockTypeSN", sisnId = "SISN", unitId = null, value = null, sisnValue = null, placeholder = "請選擇" } = {}) {
        const select = await formDropdown.pushSelect({ id, url: "/DropDownList/StockType", placeholder });
        const subSelect = formDropdown.getSelect(sisnId);
        const unit = formDropdown.getSelect(unitId);
        const initialized = select.dataset?.fdInitialized;
        formDropdown.setValue(select, value);
        if (subSelect) {
            await formDropdown.SISN({ id: subSelect, data: value, value: sisnValue });
            if (initialized) {
                return select;
            }
            select.dataset.fdInitialized = true;
            select.addEventListener("change", async function (e) {
                await formDropdown.SISN({ id: subSelect, data: select.value, placeholder: select.value ? placeholder : void 0 });
            });
            formDropdown.addResetEvent(select, async () => {
                await formDropdown.SISN({ id: subSelect, data: null });
            });

            if (unit) {
                subSelect.addEventListener("change", async function (e) {
                    const res = await fetch(`/Stock_Management/GetComputationalStockDetail?id=${subSelect.value}`)
                        .then(r => r.json())
                        .then(r => r.Datas)
                        .catch(err => null)
                    unit.textContent = res?.Unit ?? "";
                });
            }
        }
        return select;
    }
    static async SISN({ id = "FSN", data, value = null, placeholder = "請先選擇類別" } = {}) {
        const select = await formDropdown.pushSelect({ id, url: `/DropDownList/StockName?StockTypeSN=${data}`, placeholder });
        formDropdown.setValue(select, value);
        return select;
    }

    static async DSystemID({ id = "DSystemID", dssId = "DSubSystemID", value = null, dssValue = null, placeholder = "請選擇" } = {}) {
        const select = await formDropdown.pushSelect({ id, url: "/DropDownList/DSystem", placeholder });
        const subSelect = formDropdown.getSelect(dssId);
        const initialized = select.dataset?.fdInitialized;
        formDropdown.setValue(select, value);
        if (subSelect) {
            await formDropdown.DSubSystemID({ id: subSelect, data: value, value: dssValue });
            if (initialized) {
                return select;
            }
            select.dataset.fdInitialized = true;
            select.addEventListener("change", async function (e) {
                await formDropdown.DSubSystemID({ id: subSelect, data: select.value, placeholder: select.value ? placeholder : void 0 });
            });
            formDropdown.addResetEvent(select, async () => {
                await formDropdown.DSubSystemID({ id: subSelect, data: null });
            });
        }
        return select;

    }
    static async DSubSystemID({ id = "DSubSystemID", data, value = null, placeholder = "請先選擇系統別" } = {}) {
        const select = await formDropdown.pushSelect({ id, url: `/DropDownList/DSubSystem?DSystemID=${data}`, placeholder });
        formDropdown.setValue(select, value);
        return select;
    }

    static async ExperimentType({ id = "ExperimentType", tawId = "TAWSN", value = null, tawValue = null, placeholder = "請選擇" } = {}) {
        const select = await formDropdown.pushSelect({ id, url: "/DropDownList/FormExperimentType", placeholder });
        const subSelect = formDropdown.getSelect(tawId);
        const initialized = select.dataset?.fdInitialized;
        formDropdown.setValue(select, value);
        if (subSelect) {
            await formDropdown.TAWSN({ id: subSelect, data: value, value: tawValue });
            if (initialized) {
                return select;
            }
            select.dataset.fdInitialized = true;
            select.addEventListener("change", async function (e) {
                await formDropdown.TAWSN({ id: subSelect, data: select.value, placeholder: select.value ? placeholder : void 0 });
            });
            formDropdown.addResetEvent(select, async () => {
                await formDropdown.TAWSN({ id: subSelect, data: null });
            });
        }
        return select;

    }
    static async TAWSN({ id = "TAWSN", data, value = null, placeholder = "請先選擇實驗類型" } = {}) {
        const select = await formDropdown.pushSelect({ id, url: `/DropDownList/FormExperimentName?ExperimentType=${data}`, placeholder });
        formDropdown.setValue(select, value);
        return select;
    }
}
