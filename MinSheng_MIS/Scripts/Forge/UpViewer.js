var viewer; //GuiViewer3D
var viewerUrl;
//var viewerUrl = "BimModels/Site01/Resource/3D View/林口(一)/林口(一).svf"; //svf檔路徑
async function initializeViewer(callback) {
    const myProfileSettings_up = {
        name: "mySettings_up",
        description: "My personal settings.",
        settings: {
            viewType: 1, //orthographic
        },
        persistent: [],
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
    Autodesk.Viewing.Private.InitParametersSetting.alpha = true;

    viewer = new Autodesk.Viewing.GuiViewer3D(
        document.getElementById('viewer3d'),
        { profileSettings: myProfileSettings_up }
    );
    const options = { 'env': "Local", 'document': viewerUrl };
    Autodesk.Viewing.Initializer(options, function () {
        viewer.start(options.document, options, onSuccess, onError);
        async function onSuccess() {
            console.log("onSuccess");
            viewer.setBackgroundOpacity(0);
            viewer.setBackgroundColor();
            viewer.setLightPreset(16);

            viewer.toolbar.removeControl("navTools");
            viewer.toolbar.removeControl("settingsTools");

            viewer.impl.controls.handleKeyDown = () => {}

            viewer.addEventListener(Autodesk.Viewing.GEOMETRY_LOADED_EVENT, ON_GEOMETRY_LOADED);
            viewer.addEventListener(Autodesk.Viewing.EXTENSION_LOADED_EVENT, ON_EXTENSION_LOADED);
        }
        function onError() { console.log("onError"); }
    });

    function ON_GEOMETRY_LOADED(e) {
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

    function ON_EXTENSION_LOADED(e) {
        console.log("EXTENSION_LOADED_EVENT = " + e.extensionId);
        switch (e.extensionId) {
            case "Autodesk.ViewCubeUi":
                const vcExt = viewer.getExtension(e.extensionId);
                vcExt.setVisible(false);
                let pos = new THREE.Vector3(0, 0, 160);
                let tg = new THREE.Vector3(0, 0, 0);
                viewer.navigation.setView(pos, tg);
                viewer.navigation.setCameraUpVector(new THREE.Vector3(-1, 0, 0));
                break;
            case 'Autodesk.Viewing.Wireframes':
                const wfExt = viewer.getExtension(e.extensionId);
                wfExt.activate();
                wfExt.showSolidMaterial(false);
                let lineMaterial = new THREE.LineBasicMaterial({
                    color: 0x000000,
                    linewidth: 0.2
                });
                wfExt.setLinesMaterial(lineMaterial);
                wfExt.setLightPreset(16);
                console.log("wfExt run end");
                break;
        }
    }
}