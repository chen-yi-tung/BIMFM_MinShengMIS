'use strict';
async function basemapGalleryWidget(view, position = "top-left", index = -1) {
    await loadArcgis("esri/widgets/BasemapGallery")
    const component = new Arcgis.BasemapGallery({ view });
    const expand = await Arcgis.customWidgetUtils.expandWidget(view, component, position, { id: "basemapGallery", mode: "auto" })
    view.ui.add({ component: expand, position, index });
    return [component, expand];
}