(function ($) {
	$.datepick.regional['zh-TW'] = {
		showAnim:'slideDown',
		clearText: '清除',
		clearStatus: '清除已選日期',
		closeText: '確定',
		closeStatus: '不改變目前的選擇',
		prevText: '&#x3c;上月',
		prevStatus: '顯示上月',
		prevBigText: '&#x3c;&#x3c;',
		prevBigStatus: '顯示上一年',
		nextText: '下月&#x3e;',
		nextStatus: '顯示下月',
		nextBigText: '&#x3e;&#x3e;',
		nextBigStatus: '顯示下一年',
		currentText: '今天',
		currentStatus: '顯示本月',
		monthNames: ['1月', '2月', '3月', '4月', '5月', '6月',
			'7月', '8月', '9月', '10月', '11月', '12月'
		],
		monthNamesShort: ['1', '2', '3', '4', '5', '6',
			'7', '8', '9', '10', '11', '12'
		],
		monthStatus: '選擇月份',
		yearStatus: '選擇年份',
		weekHeader: '周',
		weekStatus: '年內周次',
		dayNames: ['星期日', '星期一', '星期二', '星期三', '星期四', '星期五', '星期六'],
		dayNamesShort: ['日', '一', '二', '三', '四', '五', '六'],
		dayNamesMin: ['日', '一', '二', '三', '四', '五', '六'],
		dayStatus: '設定 DD 為一周起始',
		dateStatus: '選擇 m月 d日, DD',
		dateFormat: 'yy/mm/dd',
		firstDay: 1,
		initStatus: '請選擇日期',
		isRTL: false,
		showMonthAfterYear: false,
		yearSuffix: '',
		
	};
	$.datepick.setDefaults($.datepick.regional['zh-TW']);

	$.extend($.datepick, {
		formatDate: function (format, date, settings) {
			var d = date.getDate();
			var m = date.getMonth() + 1;
			var y = date.getFullYear();
			var fm = function (v) {
				return (v < 10 ? '0' : '') + v;
			};
			return (y - 1911) + '-' + fm(m) + '-' + fm(d);
		},
		parseDate: function (format, value, settings) {
			var v = new String(value);
			var Y, M, D;
			if (v.length == 7) {
				/*1001215*/
				Y = v.substring(0, 3) - 0 + 1911;
				M = v.substring(3, 5) - 0 - 1;
				D = v.substring(5, 7) - 0;
				return (new Date(Y, M, D));
			} else if (v.length == 6) {
				/*981215*/
				Y = v.substring(0, 2) - 0 + 1911;
				M = v.substring(2, 4) - 0 - 1;
				D = v.substring(4, 6) - 0;
				return (new Date(Y, M, D));
			}
			return (new Date());
		},
		formatYear: function (v) {
			return '民國' + (v - 1911) + '年';
		},
		

	});

})($);