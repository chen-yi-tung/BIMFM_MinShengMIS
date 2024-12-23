var viewer; //GuiViewer3D
async function initializeViewer({
    BIMPath,
    BeaconPath,
    callback
}) {
    const profileSettings = {
        name: "mySettings_up",
        description: "My personal settings.",
        settings: {
            viewType: 1, //orthographic
        },
        persistent: ["Viewer.Toolkit"],
        extensions: {
            load: [],
            unload: [
                //減少toolbar物件
                "Autodesk.Section",
                "Autodesk.Measure",
                "Autodesk.Explode",
                "Autodesk.BimWalk",
                "Autodesk.Viewing.FusionOrbit"
            ]
        }
    }
    viewer = new Autodesk.Viewing.GuiViewer3D(document.getElementById('viewer3d'), { profileSettings });
    viewer.loadExtension("Viewer.Loading", {
        loader: `<div class="lds-default"><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div><div></div></div>`
    })

    Autodesk.Viewing.Private.InitParametersSetting.alpha = true;
    const options = {
        keepCurrentModels: true,
        globalOffset: { x: 0, y: 0, z: 0 }
    };
    const urns = [BIMPath, BeaconPath];

    Autodesk.Viewing.Initializer({ env: 'Local' }, async function () {
        viewer.start();

        await viewer.loadExtension("Viewer.Toolkit")

        //load urns
        const models = await Promise.all(urns.map((url) => {
            return new Promise(async (resolve, reject) => {
                if (url == null || url == undefined || url == "") {
                    resolve(null)
                    return
                };
                viewer.loadModel(window.location.origin + url, options,
                    async (model) => { await viewer.waitForLoadDone(); resolve(model); })
            })
        }))
        console.log(models)

        //setting 3d view env
        viewer.setGroundShadow(false);
        viewer.setBackgroundOpacity(0);
        viewer.setBackgroundColor();
        viewer.setLightPreset(16); //設定環境光源 = 雪地
        viewer.toolkit.removeAllToolbarControl()

        const ViewCubeUi = await viewer.loadExtension("Autodesk.ViewCubeUi")
        ViewCubeUi.setVisible(false);

        viewer.toolkit.autoFitModelsTop(models.filter(e=>e), 2, true)

        for (const model of models) {
            await onLoadDone(model)
        }
        viewer.loading.hide()

        setTimeout(() => {
            callback()
        }, 500)

        /* const Wireframes = await viewer.loadExtension("Autodesk.Viewing.Wireframes")
        Wireframes.activate();
        Wireframes.showSolidMaterial(false);
        Wireframes.setLinesMaterial(new THREE.LineBasicMaterial({ color: 0x000000, linewidth: 0.2 }));
        Wireframes.setLightPreset(16);
        console.log("wfExt run end"); */
    });

    async function onLoadDone(model) {
        if (model == null) return
        console.log("Model LoadDone", model.loader.basePath.split("/").at(-2));

        if (model.loader.basePath.includes("Beacon")) {
            const newmat = new THREE.MeshPhongMaterial({
                color: 0x1010ff,
                emissive: 0x001061,
                reflectivity: 0,
                shininess: 0
            })
            await viewer.toolkit.changeMaterial(model, model.getRootId(), newmat)
            console.log("顏色變更完畢")
        }
    }

}