'use strict';
(() => {
    if (window.Arcgis == undefined) { window.Arcgis = {} }
    loadWidgetScripts([
        "homeWidget",
        "fullscreenWidget",
        "basemapGalleryWidget",
        "searchWidget",
        "daylightWidget",
        "elevationProfileWidget",
        "measureExpandWidget",
        "featureTableWidget",
        "selectWidget",
        "settingWidget"
    ])

    function loadWidgetScripts(urls) {
        return new Promise((resolve) => {
            urls.forEach(url => {
                const script = document.createElement("script");
                script.type = "text/javascript";
                script.setAttribute("defer", '')
                script.onload = function (event) {
                    //console.log("load " + url)
                    resolve(event);
                };
                script.src = "/Scripts/ArcGIS/widget/" + url + ".js";
                document.getElementsByTagName("head")[0].appendChild(script);
            });
        })
    }
})()