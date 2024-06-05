'use strict';
(async function (A) {
    await loadArcgis([
        "esri/core/reactiveUtils",
        "esri/rest/support/Query",
        "esri/geometry/Extent",
    ])
    Object.defineProperty(A, "sceneLayerUtils", {
        value: {
            pointToExtent(pointGeometry, distance = 6) {
                return new Arcgis.Extent({ xmin: 1, ymin: 1, xmax: distance, ymax: distance, spatialReference: pointGeometry.spatialReference }).centerAt(pointGeometry);
            },
            async fitByObjectId({ view, layer, objectId, popup = true }) {
                let [layerView, e, f] = await Promise.all([
                    view.whenLayerView(layer),
                    layer.queryExtent(new Arcgis.Query({
                        objectIds: [objectId],
                        returnGeometry: true,
                    })),
                    layer.queryFeatures(new Arcgis.Query({
                        objectIds: [objectId],
                        outFields: ['*'],
                    }))
                ])

                view.goTo(this.pointToExtent(e.extent.center).expand(10)).then(async () => {
                    if (!popup) { return }
                    let { extent } = await layerView.queryExtent(new Arcgis.Query({
                        objectIds: [objectId],
                        returnGeometry: true,
                        returnZ: true
                    }));
                    view.openPopup({
                        features: f.features,
                        //updateLocationEnabled: true,
                        location: extent.center
                    })
                })
                return [e, f];
            },
            async fitByWhere({ view, layer, where = "1=1", popup = true, highlight = false, callback = () => { } }) {
                let [layerView, e, f] = await Promise.all([
                    view.whenLayerView(layer),
                    layer.queryExtent(new Arcgis.Query({
                        returnGeometry: true,
                        where,
                    })),
                    layer.queryFeatures(new Arcgis.Query({
                        outFields: ['*'],
                        where,
                    }))
                ])

                view.goTo(this.pointToExtent(e.extent.center).expand(10)).then(async () => {
                    if (popup) {
                        let { extent } = await layerView.queryExtent(new Arcgis.Query({
                            returnGeometry: true,
                            returnZ: true,
                            where
                        }));
                        view.openPopup({
                            features: f.features,
                            //updateLocationEnabled: true,
                            location: extent.center
                        })
                    }
                    if (highlight) {
                        let highlightHandle = layerView.highlight(f.features.map(feature => feature.attributes.objectid));

                        Arcgis.reactiveUtils.when(
                            () => view.popup.visible == false,
                            () => {
                                highlightHandle && highlightHandle.remove();
                            },
                            { once: true }
                        )
                    }
                    callback(layerView, f)
                })

                return [e, f];
            }
        }
    })
})(window.Arcgis)