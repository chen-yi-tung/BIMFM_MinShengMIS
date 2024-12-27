class UpViewer {
    constructor(container) {
        Autodesk.Viewing.Private.InitParametersSetting.alpha = true; //設定透明背景可用
        this.container = container;
        this.viewer = null;
        this.models = [];
        this.profileSettings = {
            name: "customSettings",
            description: "My personal settings.",
            settings: {
                lightPreset: 16,
            }, //API:https://aps.autodesk.com/en/docs/viewer/v7/reference/globals/TypeDefs/Settings/
            persistent: [],
            extensions: {
                load: ["Viewer.Toolkit"],
                unload: [],
            },
        };
        this.modelOptions = {
            keepCurrentModels: true,
            globalOffset: { x: 0, y: 0, z: 0 },
        };
    }
    async init() {
        if (this.viewer !== null) {
            return true;
        }
        let resolve, reject;
        const promise = new Promise((res, rej) => {
            resolve = res;
            reject = rej;
        });

        this.viewer = new Autodesk.Viewing.Viewer3D(this.container, { profileSettings: this.profileSettings });
        this.viewer.loadExtension("Viewer.Loading", { loader: `<div class="lds-default">${Array(12).fill("<div></div>").join("")}</div>` });

        Autodesk.Viewing.Initializer({ env: "Local" }, async () => {
            this.viewer.start();
            this.viewer.impl.controls.handleKeyDown = function (e) { };
            this.viewer.impl.controls.handleKeyUp = function (e) { };
            await this.viewer.loadExtension("Viewer.Toolkit");

            //setting 3d view env
            this.viewer.setBackgroundOpacity(0);
            this.viewer.setBackgroundColor();
            this.viewer.loading.hide();
            console.log("Viewer init done");

            resolve(true);
        });
        return promise;
    }
    async loadModels(urls, { onModelLoadDone = () => { }, onLoadDone = () => { } } = {}) {
        this.viewer.loading.show();
        const models = await Promise.all(
            urls.map(({ url, type }) => {
                return new Promise(async (resolve, reject) => {
                    this.viewer.loadModel(
                        window.location.origin + url,
                        {
                            ...this.modelOptions,
                            modelOverrideName: type,
                        },
                        async (model) => {
                            await this.viewer.waitForLoadDone();
                            this.models.push({
                                type,
                                model,
                            });
                            resolve(model);
                        },
                        async (model) => {
                            reject(model);
                        }
                    );
                });
            })
        );

        for (const model of models) {
            await onModelLoadDone(model);
        }
        await onLoadDone(models);
        this.viewer.setLightPreset(this.profileSettings.settings.lightPreset || 16);
        this.viewer.toolkit.autoFitModelsTop(models.filter(e => e), 10, true)
        this.viewer.loading.hide();
    }
    dispose() {
        if (this.viewer == null) {
            return;
        }
        this.viewer.tearDown();
        this.viewer.finish();
        this.viewer = null;
        this.container.replaceChildren();
    }
    createBeaconPoint(data = []) {
        const model = this.models.find((model) => model.type === "BT").model;
        const pins = [];
        data.forEach((d) => {
            let position = this.viewer.toolkit.getBoundingBox(model, d.dbId).getCenter();
            let pin = new ForgePin({
                viewer: this.viewer,
                position,
                data: d,
                img: "/Content/img/bluetooth.svg",
                id: `bluetooth-${d.dbId}`,
            });

            d.deviceName && $(pin.e).append(`<div class="bluetooth-name">${d.deviceName}</div>`);

            pin.show();
            pin.update();
            pins.push(pin);
        });
        this.viewer.addEventListener(Autodesk.Viewing.CAMERA_CHANGE_EVENT, () => {
            pins.forEach((pin) => {
                pin.update();
            });
        });
    }
    createEquipmentPoint(position) {
        let pin = new ForgePin({
            viewer: this.viewer,
            position: position,
            data: {},
            img: "/Content/img/gis-marker.svg",
            id: `equipment-${d.dbId}`,
        });
        this.viewer.addEventListener(Autodesk.Viewing.CAMERA_CHANGE_EVENT, () => {
            pin.update();
        });
    }
}

