class formDropdown {
    constructor() { }

    static getSelect(id) {
        if (id instanceof HTMLSelectElement) return id;
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
        select.innerHTML = '';
        if (!placeholder) return;
        // 添加默認選項
        const defaultOption = document.createElement('option');
        defaultOption.value = '';
        defaultOption.textContent = placeholder;
        select.appendChild(defaultOption);
    }

    static addResetEvent(select, callback = () => { }) {
        const form = select.closest('form');
        if (!form) {
            console.warn(`[formDropdown.addResetEvent] not found form element closest select "${select}".`);
            return;
        }

        const resetButton = form.querySelector('[type="reset"]');

        if (resetButton) {
            resetButton.addEventListener('click', callback);
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
            if (filter) { res = filter(res) }
            else { res = res.Datas || res; }
            formDropdown.reset(select, placeholder)

            // 動態添加其他選項
            res.forEach(item => {
                const option = document.createElement('option');
                option.value = item[value];
                option.textContent = item[name];
                select.appendChild(option);
            });
        } catch (error) {
            console.error('Error fetching data:', error);
            formDropdown.reset(select, placeholder)
        }
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
                Object.keys(data).forEach(key => {
                    formData.append(key, data[key]);
                });
                res = await fetch(url, {
                    method: "POST",
                    body: formData,
                }).then(res => res.json());
            }
            else {
                res = await fetch(url, {
                    method: "POST",
                    body: JSON.stringify(data),
                    headers: new Headers({
                        "Content-Type": "application/json",
                    }),
                }).then(res => res.json());
            }
            // data filter
            if (filter) { res = filter(res) }
            else { res = res.Datas || res; }

            formDropdown.reset(select, placeholder)

            // 動態添加其他選項
            res.forEach(item => {
                const option = document.createElement('option');
                option.value = item[value];
                option.textContent = item[name];
                select.appendChild(option);
            });
        } catch (error) {
            console.error('Error fetching data:', error);
            formDropdown.reset(select, placeholder)
        }
    }

    static async ASN({ id = "ASN", fsnId = "FSN", value, placeholder = "請選擇" } = {}) {
        await formDropdown.pushSelect({ id, url: "/DropDownList/Area", placeholder });
        const asn = formDropdown.getSelect(id);
        const fsn = formDropdown.getSelect(fsnId);
        if (fsn) {
            await formDropdown.FSN({ id: fsnId });
            asn.addEventListener('change', async function (e) {
                await formDropdown.FSN({ id: fsnId, data: asn.value, placeholder: asn.value ? placeholder : void 0 });
            });
        }
        formDropdown.setValue(id, value)
    };
    static async FSN({ id = "FSN", data, value, placeholder = "請先選擇棟別" } = {}) {
        await formDropdown.pushSelect({ id, url: `/DropDownList/Floor?ASN=${data}`, placeholder });
        formDropdown.setValue(id, value)
    };
}