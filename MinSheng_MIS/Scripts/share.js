function addButtonEvent() {
    $("#back").click(function () {
        history.back();
    })

    $("#submit").click(function () {
        save();
    })
}

async function checkAuthority() {
    try {
        const res = await new Promise((resolve, reject) => {
            $.ajax({
                url: '/Account/UserAuthority',
                type: "GET",
                dataType: "json",
                contentType: "application/json;charset=utf-8",
                success: function (res) {
                    resolve(res.Datas.Authority);
                },
                error: function (res) {
                    reject(res);
                },
            });
        });
        //console.log("權限", res);
        window.__authority__ = res;
        return res;
    } catch (error) {
        console.error("權限回傳失敗", error);
        throw error;
    }
}

//轉換日期格式
function dateTransform(data) {
    // 民國轉西元
    // 檢查是否為物件（但不是陣列）
    if (typeof data === "object" && data !== null && !Array.isArray(data)) {
        let newObj = { ...data };
        for (let key in newObj) {
            let d = newObj[key];
            let datePattern = /^(\d{3})[\/-](\d{2})[\/-](\d{2})$/; // 民國年格式 YYY-MM-DD 或 YYY/MM/DD
            if (datePattern.test(d)) {
                newObj[key] = d.replace(datePattern, (_, year, month, day) => `${parseInt(year) + 1911}-${month}-${day}`);
            }
        }
        return newObj;
    }

    // 西元轉民國
    // 檢查是否為字串
    if (typeof data === "string") {
        if (data === '-') {
            return data;
        }
        const isDateTimeFormat = /^\d{4}-\d{2}-\d{2} \d{2}:\d{2}(:\d{2})?$/.test(data); // YYYY-MM-DD HH:mm
        const isDateRangeFormat = /^\d{4}-\d{2}-\d{2} \d{2}:\d{2}-\d{2}:\d{2}$/.test(data); // YYYY-MM-DD HH:mm-HH:mm
        const isDateOnlyFormat = /^\d{4}-\d{2}-\d{2}$/.test(data); // YYYY-MM-DD

        if (!isDateTimeFormat && !isDateRangeFormat && !isDateOnlyFormat) {
            throw new Error("Invalid date format. Expected 'YYYY-MM-DD', 'YYYY-MM-DD HH:mm', 'YYYY-MM-DD HH:mm:ss', or 'YYYY-MM-DD HH:mm-HH:mm'.");
        }

        const datePart = data.split(" ")[0]; // 只取 YYYY-MM-DD
        const [year, month, day] = datePart.split("-").map(Number);
        if (!year || !month || !day) {
            throw new Error("Invalid date value.");
        }
        const taiwanYear = year - 1911;
        const formattedDate = `${taiwanYear}-${String(month).padStart(2, "0")}-${String(day).padStart(2, "0")}`;

        // 時間部分直接拼接回去
        if (isDateTimeFormat) {
            const time = data.split(" ")[1]; // 取時間部分
            return `${formattedDate} ${time}`;
        }

        if (isDateRangeFormat) {
            const timeRange = data.split(" ")[1]; // 取時間範圍
            return `${formattedDate} ${timeRange}`;
        }
        return formattedDate;
    }
    return data;
}


// 生成年月份下拉選單（民國年）
function generateYearSelect(yearSelectId) {
    const startYear = 1912;
    const currentYear = new Date().getFullYear();
    const yearSelect = document.getElementById(yearSelectId);

    const defaultOption = document.createElement('option');
    defaultOption.value = "";
    defaultOption.textContent = "請選擇";
    yearSelect.appendChild(defaultOption);

    for (let year = currentYear; year >= startYear; year--) {
        const rocYear = year - 1911; // 轉換為民國年
        const option = document.createElement('option');
        option.value = rocYear; // 設定值為民國年份
        option.textContent = `民國 ${rocYear} 年`; // 顯示民國年份
        yearSelect.appendChild(option);
    }
}

function generateMonthSelect(monthSelectId) {
    const months = [
        "1月", "2月", "3月", "4月", "5月", "6月",
        "7月", "8月", "9月", "10月", "11月", "12月"
    ];
    const monthSelect = document.getElementById(monthSelectId);

    const defaultOption = document.createElement('option');
    defaultOption.value = "";
    defaultOption.textContent = "請選擇";
    monthSelect.appendChild(defaultOption);

    months.forEach((month, index) => {
        const option = document.createElement('option');
        option.value = index + 1;
        option.textContent = month;
        monthSelect.appendChild(option);
    });
}