'use strict';
async function fullscreenWidget(view, position = "top-left", index = -1) {
    await loadArcgis("esri/widgets/Fullscreen")
    const component = new Arcgis.Fullscreen({ view, id: "fullscreen" });
    view.ui.add({ component, position, index });
    return component;
}