'use strict';
(() => {
	if (window.Arcgis == undefined) { window.Arcgis = {} }
	loadScript("/Scripts/ArcGIS/utils/sceneLayerUtils.js")
	loadScript("/Scripts/ArcGIS/utils/customWidgetUtils.js")

	function loadScript(url) {
		return new Promise((resolve) => {
			const script = document.createElement("script");
			script.type = "text/javascript";
			script.setAttribute("defer", '')
			script.onload = function (event) {
				resolve(event);
			};
			script.src = url;
			document.getElementsByTagName("head")[0].appendChild(script);
		})
	}
})()
function loadArcgis(srcs) {
	return new Promise((resolve, reject) => {
		try {
			if (Array.isArray(srcs)) {
				require(srcs, function () {
					srcs.forEach((src, i) => {
						Object.defineProperty(window.Arcgis, /\/([^/]+)$/.exec(src)[1], { value: arguments[i] });
					});
					resolve(window.Arcgis);
				});
			} else {
				switch (typeof srcs) {
					case 'object':
						require(Object.values(srcs), function () {
							Object.entries(srcs).forEach(([k,], i) => {
								Object.defineProperty(window.Arcgis, k, { value: arguments[i] });
							});
							resolve(window.Arcgis);
						});
						break;
					case "string":
						require([srcs], function () {
							Object.defineProperty(window.Arcgis, /\/([^/]+)$/.exec(srcs)[1], { value: arguments[0] });
							resolve(window.Arcgis);
						});
						break;
				}
			}
		} catch { reject(); }
	})
}