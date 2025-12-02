class UpViewer {
    #scale = 2;
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

            //setting 3d view env
            this.viewer.setBackgroundOpacity(0);
            this.viewer.setBackgroundColor();
            this.viewer.loading.hide();
            console.log("Viewer init done");

            resolve(true);

            this.addHomeToggle();
        });
        return promise;
    }
    async loadModels(urls, { onModelLoadDone = () => { }, onLoadDone = () => { } } = {}) {
        console.log('loadModels', urls);

        this.viewer.loading.show();
        const models = await Promise.all(
            urls.map(({ url, type }) => {
                return new Promise(async (resolve, reject) => {
                    console.log('loadModel', type, url);
                    this.viewer.loadModel(
                        window.location.origin + url,
                        {
                            ...this.modelOptions,
                            modelOverrideName: type,
                        },
                        async (model) => {
                            console.log('loadModel waiting', type, url);
                            await this.viewer.waitForLoadDone();
                            console.log('loadModel done', type, url);
                            this.models.push({
                                type,
                                model,
                            });
                            resolve(model);
                        },
                        async (model) => {
                            console.log('loadModel failed', type, url, model);
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

        if (!this.equipmentPointTool) {
            this.equipmentPointTool = new EquipmentPointTool(this.viewer);
            this.viewer.toolController.registerTool(this.equipmentPointTool)
        }

        this.viewer.toolkit.setKeyControls(false);
        this.viewer.toolkit.setPointerControls(false);

        this.viewer.setLightPreset(this.profileSettings.settings.lightPreset || 16);

        this.viewer.toolController.deactivateTool("hotkeys");
        this.viewer.toolController.deactivateTool("gestures");
        this.viewer.toolController.activateTool("pan");

        this.viewer.toolkit.autoFitModelsTop(
            models.filter((e) => e),
            this.#scale,
            true
        );
        this.viewer.loading.hide();
    }
    unloadModels() {
        this.models.forEach(({ model }) => {
            this.viewer.unloadModel(model)
        })
        this.models = [];
    }
    dispose() {
        if (this.viewer == null) {
            return;
        }
        this.models = [];
        this.viewer.tearDown();
        this.viewer.finish();
        this.viewer = null;
        this.equipmentPointTool = null;
        this.container.replaceChildren();
    }
    updateBeaconPoint() {
        if (!this.beaconPoint) return;
        this.beaconPoint.forEach((pin) => {
            pin.update();
        })
    }
    createBeaconPoint(data = []) {
        const model = this.models.find((model) => model.type === "BT").model;
        const pins = [];
        data.forEach((d) => {
            const pin = new ForgePin({
                viewer: this.viewer,
                dbId: d.DBID,
                position: !!d?.Position?.length ? {
                    x: d.Position?.[0] ?? 0,
                    y: d.Position?.[1] ?? 0,
                    z: d.Position?.[2] ?? 0,
                } : undefined,
                data: d,
                model,
                img: "/Content/img/bluetooth.svg",
                id: `bluetooth-${d.DBID}`,
            });

            d.DeviceName && $(pin.element).append(`<div class="bluetooth-name">${d.DeviceName}</div>`);
            pins.push(pin);

            pin.show();
            pin.update();
        });
        this.beaconPoint = pins;
        this.viewer.addEventListener(Autodesk.Viewing.CAMERA_CHANGE_EVENT, this.updateBeaconPoint.bind(this))
        return pins;
    }
    destroyBeaconPoint() {
        this.viewer.removeEventListener(Autodesk.Viewing.CAMERA_CHANGE_EVENT, this.updateBeaconPoint.bind(this))
        if (!this.beaconPoint) return;
        this.beaconPoint.forEach((pin) => {
            pin.destroy();
        });
        this.beaconPoint = null;
    }
    activateEquipmentPointTool(position = new THREE.Vector3(0, 0, 0), interactive = false) {
        if (position.z === 0) {
            const box = new THREE.Box3();
            this.viewer.getAllModels().forEach((model) => {
                const b = model.getBoundingBox();
                box.union(b);
            });
            position.z = box.getCenter().z;
        }
        this.viewer.toolController.activateTool(this.equipmentPointTool.getName());
        this.equipmentPointTool.setPosition(position);
        this.equipmentPointTool.interactive = interactive;
        return this.equipmentPointTool;
    }
    getModelsUrl(ViewName) {
        const ModelTypeList = ["AR", "BT", "E", "EL", "F", "PP", "PPO", "VE", "WW"];
        return ModelTypeList.map((type) => {
            return {
                type,
                url: `/BimModels/TopView/${type}/Resource/3D 視圖/${ViewName}/${ViewName}.svf`,
            };
        });
    }
    addHomeToggle() {
        this.viewer.container.insertAdjacentHTML("beforeend", `<div class="home-wrapper"><button type="button" class="home-toggle"><i class="home-toggle-icon"></i>回到預設視角</button></div>`);
        const toggle = this.viewer.container.querySelector(".home-toggle");
        toggle.onclick = () => {
            this.viewer.toolkit.autoFitModelsTop(this.viewer.getAllModels(), this.#scale, false);
        };
    }
}

class EquipmentPointTool {
    #name = 'Viewer.Tool.EquipmentPoint'
    #active = false;
    #pinOptions = {
        position: new THREE.Vector3(0, 0, 0),
        data: {},
        img: "/Content/img/equipment.svg",
        offset: ["-50%", "-100%"],
        id: `equipment`,
    }
    #interactive = false;
    #dragging = false;
    #panTool = null;
    #panToolEvent = {};
    #buttonNumber = 0;
    constructor(viewer) {
        this.viewer = viewer;
        this.pin = new ForgePin(Object.assign({ viewer: this.viewer }, this.#pinOptions))
    }
    get interactive() {
        return this.#interactive;
    }
    set interactive(v) {
        this.#interactive = v;
        this.#dragging = false;
    }
    getNames() {
        return [this.#name];
    }
    getName() {
        return this.#name;
    }
    register() { }
    deregister() {
        this.pin.destroy()
    }
    isActive() {
        return this.#active;
    }
    activate() {
        if (this.#active) return;
        this.#active = true;
        this.pin.show().update();

        this.#panTool = this.viewer.toolController.getTool("pan");
        this.#panToolEvent['handleButtonDown'] = this.#panTool.handleButtonDown.bind(this.#panTool)
        this.#panTool.handleButtonDown = (event, button) => {
            if (button === this.#buttonNumber) {
                return false;
            }
            this.#panToolEvent['handleButtonDown'](event, button)
        }
    }
    deactivate() {
        if (!this.#active) return;
        this.interactive = false;
        this.#active = false;
        this.pin.hide()

        Object.entries(this.#panToolEvent).forEach((eventName, func) => {
            this.#panTool[eventName] = func;
        })
    }
    update(highResTimestamp) {
        requestAnimationFrame(() => {
            this.pin.update();
        })
    }
    handleSingleClick(e, b) {
        if (!this.#interactive) return;
        if (b !== this.#buttonNumber) return;
        const hit = this.viewer.clientToWorld(e.canvasX, e.canvasY);
        if (!hit) {
            return;
        }
        this.pin.position = hit.point;
        this.pin.show().update();
        //console.log("click: ", e, hit?.point);
    }
    handleButtonDown(e, b) {
        if (!this.#interactive) return;
        if (b !== this.#buttonNumber) return;
        this.#dragging = true;
        this.handleSingleClick(e, b);
    }
    handleMouseMove(e) {
        if (!this.#interactive) return;
        this.#dragging && this.handleSingleClick(e, this.#buttonNumber);
    }
    handleButtonUp(e) {
        if (!this.#interactive) return;
        if (this.#dragging) {
            this.#dragging = false;
            this.handleSingleClick(e, this.#buttonNumber);
        }
    }
    getPosition() {
        return this.pin.position.clone();
    }
    setPosition(v) {
        this.pin.position = v;
    }
}
