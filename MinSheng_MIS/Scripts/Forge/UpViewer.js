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
    Autodesk.Viewing.Private.InitParametersSetting.alpha = true;
    const options = {
        keepCurrentModels: true,
        globalOffset: { x: 0, y: 0, z: 0 }
    };
    const urns = [BIMPath, BeaconPath];

    Autodesk.Viewing.Initializer(options, async function () {
        viewer.start();
        viewer.setBackgroundOpacity(0);
        viewer.setBackgroundColor();
        viewer.setLightPreset(16);

        viewer.toolbar.removeControl("navTools");
        viewer.toolbar.removeControl("settingsTools");
        viewer.impl.controls.handleKeyDown = () => { }

        await viewer.loadExtension("Viewer.Toolkit")

        //load urns
        urns.forEach(async (url) => {
            if (url == null || url == undefined || url == "") return;
            viewer.loadModel(
                window.location.origin + url,
                options,
                async (model) => { await viewer.waitForLoadDone(); onLoadDone(model); }
            )
        })

        const ViewCubeUi = await viewer.loadExtension("Autodesk.ViewCubeUi")
        ViewCubeUi.setVisible(false);
        let pos = new THREE.Vector3(0, 0, 160);
        let tg = new THREE.Vector3(0, 0, 0);
        viewer.navigation.setView(pos, tg);
        viewer.navigation.setCameraUpVector(new THREE.Vector3(-1, 0, 0));

        const Wireframes = await viewer.loadExtension("Autodesk.Viewing.Wireframes")
        Wireframes.activate();
        Wireframes.showSolidMaterial(false);
        Wireframes.setLinesMaterial(new THREE.LineBasicMaterial({ color: 0x000000, linewidth: 0.2 }));
        Wireframes.setLightPreset(16);
        console.log("wfExt run end");
    });

    async function onLoadDone(model) {
        console.log("Model LoadDone", model.loader.basePath.split("/").at(-1));

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

    function ON_GEOMETRY_LOADED() {
        console.log("GEOMETRY_LOADED_EVENT");
        let eventName = Autodesk.Viewing.FINAL_FRAME_RENDERED_CHANGED_EVENT;
        let extName = 'Autodesk.Viewing.Wireframes';
        viewer.addEventListener(eventName, loadWireFrame);

        function loadWireFrame() {
            console.log("FINAL_FRAME_RENDERED_CHANGED_EVENT");
            if (!viewer.isExtensionLoaded(extName)) {
                viewer.loadExtension(extName).then(callback);
                viewer.removeEventListener(eventName, loadWireFrame);
            }
        }
    }
}