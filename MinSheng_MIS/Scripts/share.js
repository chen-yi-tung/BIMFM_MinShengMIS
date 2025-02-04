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

//轉換日期格式(西元轉民國)
function dateTransform(dateString) {
    if (dateString === '-') {
        return dateString;
    }
    const isDateTimeFormat = /^\d{4}-\d{2}-\d{2} \d{2}:\d{2}$/.test(dateString); // 年-月-日 時:分
    const isDateOnlyFormat = /^\d{4}-\d{2}-\d{2}$/.test(dateString); // 年-月-日

    if (!isDateTimeFormat && !isDateOnlyFormat) {
        throw new Error("Invalid date format. Expected 'YYYY-MM-DD' or 'YYYY-MM-DD HH:mm'.");

    }

    const date = new Date(dateString);
    if (isNaN(date)) {
        throw new Error("Invalid date value.");
    }

    const year = date.getFullYear();
    const taiwanYear = year - 1911; 
    const month = String(date.getMonth() + 1).padStart(2, "0"); // 月份（從 0 開始）
    const day = String(date.getDate()).padStart(2, "0");

    if (isDateTimeFormat) {
        const hours = date.getHours();
        const minutes = String(date.getMinutes()).padStart(2, "0"); // 確保分鐘有兩位數
        return `${taiwanYear}/${month}/${day} ${hours}:${minutes}`;
    } else {
        return `${taiwanYear}/${month}/${day}`;
    }
}

//input民國年
function setDatepicker() {
    var dateNative = new Date(),
        dateTW = new Date(
            dateNative.getFullYear() - 1911,
            dateNative.getMonth(),
            dateNative.getDate()
        );


    function leftPad(val, length) {
        var str = '' + val;
        while (str.length < length) {
            str = '0' + str;
        }
        return str;
    }

    // 應該有更好的做法
    var funcColle = {
        onSelect: {
            basic: function (dateText, inst) {
                /*
                var yearNative = inst.selectedYear < 1911
                    ? inst.selectedYear + 1911 : inst.selectedYear;*/
                dateNative = new Date(inst.selectedYear, inst.selectedMonth, inst.selectedDay);

                // 年分小於100會被補成19**, 要做例外處理
                var yearTW = inst.selectedYear > 1911
                    ? leftPad(inst.selectedYear - 1911, 4)
                    : inst.selectedYear;
                var monthTW = leftPad(inst.selectedMonth + 1, 2);
                var dayTW = leftPad(inst.selectedDay, 2);
                console.log(monthTW);
                dateTW = new Date(
                    yearTW + '-' +
                    monthTW + '-' +
                    dayTW + 'T00:00:00.000Z'
                );
                console.log(dateTW);
                return $.datepicker.formatDate(twSettings.dateFormat, dateTW);
            }
        }
    };

    var twSettings = {
        closeText: '關閉',
        prevText: '上個月',
        nextText: '下個月',
        currentText: '今天',
        monthNames: ['一月', '二月', '三月', '四月', '五月', '六月',
            '七月', '八月', '九月', '十月', '十一月', '十二月'],
        monthNamesShort: ['一月', '二月', '三月', '四月', '五月', '六月',
            '七月', '八月', '九月', '十月', '十一月', '十二月'],
        dayNames: ['星期日', '星期一', '星期二', '星期三', '星期四', '星期五', '星期六'],
        dayNamesShort: ['周日', '周一', '周二', '周三', '周四', '周五', '周六'],
        dayNamesMin: ['日', '一', '二', '三', '四', '五', '六'],
        weekHeader: '周',
        dateFormat: 'yy/mm/dd',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: true,
        yearSuffix: '年',

        onSelect: function (dateText, inst) {
            $(this).val(funcColle.onSelect.basic(dateText, inst));
            if (typeof funcColle.onSelect.newFunc === 'function') {
                funcColle.onSelect.newFunc(dateText, inst);
            }
        }
    };

    // 把yearText換成民國
    var replaceYearText = function () {
        var $yearText = $('.ui-datepicker-year');

        if (twSettings.changeYear !== true) {
            $yearText.text('民國' + dateTW.getFullYear());
        } else {
            // 下拉選單
            if ($yearText.prev('span.datepickerTW-yearPrefix').length === 0) {
                $yearText.before("<span class='datepickerTW-yearPrefix'>民國</span>");
            }
            $yearText.children().each(function () {
                if (parseInt($(this).text()) > 1911) {
                    $(this).text(parseInt($(this).text()) - 1911);
                }
            });
        }
    };

    $.fn.datepickerTW = function (options) {

        // setting on init,
        if (typeof options === 'object') {
            //onSelect例外處理, 避免覆蓋
            if (typeof options.onSelect === 'function') {
                funcColle.onSelect.newFunc = options.onSelect;
                options.onSelect = twSettings.onSelect;
            }
            // year range正規化成西元, 小於1911的數字都會被當成民國年
            if (options.yearRange) {
                var temp = options.yearRange.split(':');
                for (var i = 0; i < temp.length; i += 1) {
                    //民國前處理
                    if (parseInt(temp[i]) < 1) {
                        temp[i] = parseInt(temp[i]) + 1911;
                    } else {
                        temp[i] = parseInt(temp[i]) < 1911
                            ? parseInt(temp[i]) + 1911
                            : temp[i];
                    }
                }
                options.yearRange = temp[0] + ':' + temp[1];
            }
            // if input val not empty
            if ($(this).val() !== '') {
                options.defaultDate = $(this).val();
            }
        }

        // setting after init
        if (arguments.length > 1) {
            // 目前還沒想到正常的解法, 先用轉換成init setting obj的形式
            if (arguments[0] === 'option') {
                options = {};
                options[arguments[1]] = arguments[2];
            }
        }

        // override settings
        $.extend(twSettings, options);

        // init
        $(this).datepicker(twSettings);

        // beforeRender
        $(this).click(function () {
            var isFirstTime = ($(this).val() === '');

            // year range and default date

            if ((twSettings.defaultDate || twSettings.yearRange) && isFirstTime) {

                if (twSettings.defaultDate) {
                    $(this).datepicker('setDate', twSettings.defaultDate);
                }

                // 當有year range時, select初始化設成range的最末年
                if (twSettings.yearRange) {
                    var $yearSelect = $('.ui-datepicker-year'),
                        nowYear = twSettings.defaultDate
                            ? $(this).datepicker('getDate').getFullYear()
                            : dateNative.getFullYear();

                    $yearSelect.children(':selected').removeAttr('selected');
                    if ($yearSelect.children('[value=' + nowYear + ']').length > 0) {
                        $yearSelect.children('[value=' + nowYear + ']').attr('selected', 'selected');
                    } else {
                        $yearSelect.children().last().attr('selected', 'selected');
                    }
                }
            } else {
                $(this).datepicker('setDate', dateNative);
            }
            console.log(twSettings.dateFormat, $.datepicker.formatDate(twSettings.dateFormat, dateTW))
            $(this).val($.datepicker.formatDate(twSettings.dateFormat, dateTW));

            replaceYearText();

            if (isFirstTime) {
                $(this).val('');
            }
        });

        // afterRender
        $(this).focus(function () {
            replaceYearText();
        });

        return this;
    };

};
