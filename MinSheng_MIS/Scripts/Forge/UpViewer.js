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
            await this.viewer.loadExtension("Viewer.Toolkit");
            this.viewer.toolkit.removeKeyControls()

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
            const pin = new ForgePin({
                viewer: this.viewer,
                dbId: d.dbId,
                data: d,
                model,
                img: "/Content/img/bluetooth.svg",
                id: `bluetooth-${d.dbId}`,
            });

            d.deviceName && $(pin.element).append(`<div class="bluetooth-name">${d.deviceName}</div>`);

            pin.show();
            pin.update();
            pins.push(pin);
        });
    }
    createEquipmentPoint(position = new THREE.Vector3(0, 0, 0), interactive = false) {
        this.equipmentPoint = new EquipmentPoint({ viewer: this.viewer, position });
        this.equipmentPoint.interactive = interactive;
        return this.equipmentPoint;
    }
    getModelsUrl(ViewName) {
        const ModelTypeList = [
            "AR",
            "BT",
            "E",
            "EL",
            "F",
            "PP",
            "PPO",
            "VE",
            "WW"
        ]
        return ModelTypeList.map((type) => {
            return {
                type,
                url: `/BimModels/TopView/${type}/Resource/3D 視圖/${ViewName}/${ViewName}.svf`
            }
        })
    }
}

class EquipmentPoint extends ForgePin {
    #interactive = false;
    #dragging = false;
    constructor(options = {}) {
        super(Object.assign({
            position: new THREE.Vector3(0, 0, 0),
            data: {},
            img: "/Content/img/equipment.svg",
            offset: ["-50%", "-100%"],
            id: `equipment`,
        }, options));
    }
    get interactive() {
        return this.#interactive;
    }
    set interactive(v) {
        this.#interactive = v;
        this.#dragging = false;
        const rect = this.viewer.container;
        if (v) {
            const update = (event) => {
                const hit = this.viewer.clientToWorld(event.offsetX, event.offsetY);
                if (!hit) { return }
                this.position = hit.point;
                this.show().update();
            }

            rect.onclick = update;
            rect.onpointerdown  = (event) => {
                this.#dragging = true;
                update(event)
            }
            rect.onpointermove  = (event) => {
                this.#dragging && update(event);
            }
            rect.onpointerup  = (event) => {
                if (this.#dragging) {
                    this.#dragging = false;
                    update(event)
                }
            }
            rect.onpointerleave  = (event) => {
                if (this.#dragging) {
                    this.#dragging = false;
                    update(event)
                }
            }
        }
        else {
            this.#dragging = false;
            rect.onclick = null;
            rect.onpointerdown  = null;
            rect.onpointermove  = null;
            rect.onpointerup  = null;
            rect.onpointerleave  = null;
        }
    }
}

