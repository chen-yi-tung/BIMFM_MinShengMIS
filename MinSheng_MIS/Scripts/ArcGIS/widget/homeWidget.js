'use strict';
async function homeWidget(view, position = "top-left", index = -1) {
    await loadArcgis("esri/widgets/Home")
    const component = new Arcgis.Home({ view, id: "home" });
    view.ui.add({ component, position, index });
    return component;
}