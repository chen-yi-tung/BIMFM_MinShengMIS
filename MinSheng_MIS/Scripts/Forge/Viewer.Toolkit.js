(() => {
    class Toolkit extends Autodesk.Viewing.Extension {
        #controls = {};
        constructor(viewer, options) {
            super(viewer, options);
            this.name = "Viewer.Toolkit";
            this.#controls.handleKeyDown = this.viewer.impl.controls.handleKeyDown.bind(this.viewer.impl.controls);
            this.#controls.handleKeyUp = this.viewer.impl.controls.handleKeyUp.bind(this.viewer.impl.controls);
        }
        load() {
            delete this.activate;
            delete this.activeStatus;
            delete this.deactivate;
            delete this.mode;
            delete this.modes;
            this.viewer.toolkit = this;
            return this.viewer && !0;
        }
        unload() {
            delete this.viewer.toolkit;
            return true;
        }
        setColor(dbid, color, alpha, invalidate = false) {
            const tree = this.viewer.model.getInstanceTree();
            const matMng = this.viewer.impl.matman();
            const fragList = tree.fragList;
            let mat = null;
            let matName = null;

            tree.enumNodeFragments(dbid, (fragId) => {
                matName = "model:" + this.viewer.model.id + "|frag:" + fragId;
                mat = matMng._materials?.[matName] ?? fragList.getMaterial(fragId)?.clone?.();
                if (!mat) {
                    return false;
                }
                mat = changeMat(mat, color, alpha);
                !(matName in matMng._materials) && matMng.addMaterial(matName, mat, true);
                fragList.setMaterial(fragId, mat);
            });
            invalidate && this.viewer.impl.invalidate(true, true, true);

            return;

            function changeMat(m, c, a) {
                m.opacity = a;
                m.transparent = a < 1 ? true : false;
                m.color = new THREE.Color(c);
                m.depthWrite = a < 1 ? false : true;
                m.needsUpdate = true;
                return m;
            }
        }
        getBoundingBox(dbId, model) {
            if (!model) {
                model = this.viewer.model;
            }
            const it = model.getInstanceTree();
            const fragList = model.getFragmentList();
            let bounds = new THREE.Box3();

            it.enumNodeFragments(
                dbId,
                (fragId) => {
                    let box = new THREE.Box3();
                    fragList.getWorldBounds(fragId, box);
                    bounds.union(box);
                },
                true
            );

            return bounds;
        }
        isBox3Valid(box) {
            if (!box) return false;
            if (!box?.min || !box?.max) return false;
            const { min, max } = box;
            return [min.x, min.y, min.z, max.x, max.y, max.z].some(isFinite);
        }
        getCenter(dbId, model) {
            const b = this.getBoundingBox(dbId, model);

            if (!this.isBox3Valid(b)) {
                return null;
            }
            return b.getCenter();
        }
        isCameraView(v) {
            const camera = this.viewer.getCamera();
            if (camera.isOrthographicCamera && camera.up.z === 0) {
                return true;
            }
            const vector = new THREE.Vector3(v.x, v.y, v.z);
            const frustum = new THREE.Frustum();
            frustum.setFromMatrix(new THREE.Matrix4().multiplyMatrices(camera.projectionMatrix, camera.matrixWorldInverse));
            return frustum.containsPoint(vector);
        }
        getDistance(v) {
            const vector = new THREE.Vector3(v.x, v.y, v.z);
            return vector.distanceTo(this.viewer.getCamera().position);
        }
        get2DPosition(v) {
            return this.viewer.worldToClient(new THREE.Vector3(v.x, v.y, v.z));
        }
        getWorldDirection() {
            const camera = this.viewer.getCamera();
            return camera.getWorldDirection();
        }
        planeDirectionToRotation(dir, pivot = new THREE.Vector3(1, 0, 0), plane = new THREE.Plane(new THREE.Vector3(0, 0, 1), 0)) {
            const pdir = plane.projectPoint(dir).normalize();
            const rot = (pdir.angleTo(pivot) * 180) / Math.PI;
            if (dir.y >= 0) {
                return -rot;
            }
            return rot;
        }
        getTopPlaneView(v) {
            const position = v.toolkit.get2DPosition(this.viewer.getCamera().position);
            const rotation = v.toolkit.planeDirectionToRotation(this.viewer.toolkit.getWorldDirection());
            return { position, rotation };
        }
        getModelsBoundingBox(models = []) {
            if (!Array.isArray(models) || models.length == 0) {
                models = this.viewer.getAllModels();
            }
            const box = new THREE.Box3();
            models.forEach((model) => {
                const b = model.getBoundingBox();
                box.union(b);
            });
            return box;
        }
        autoFitModelsTop(models = [], scale = 2, immediate = true, reorient = false) {
            return new Promise((resolve, reject) => {
                if (!Array.isArray(models) || models.length == 0) {
                    reject();
                }

                const box = this.getModelsBoundingBox(models);

                const w = box.getCenter();
                this.viewer.navigation.setView(new THREE.Vector3(w.x, w.y, w.z + box.getSize().z ** 2), new THREE.Vector3(w.x, w.y, 0));
                this.viewer.navigation.setCameraUpVector(new THREE.Vector3(0, 1, 0));
                this.viewer.navigation.fitBounds(immediate, box.clone().expandByScalar(scale), reorient, false);

                if (immediate == true) {
                    this.viewer.navigation.toOrthographic();
                    resolve(true);
                } else {
                    this.viewer.addEventListener(
                        Autodesk.Viewing.CAMERA_TRANSITION_COMPLETED,
                        () => {
                            this.viewer.navigation.toOrthographic();
                            resolve(true);
                        },
                        { once: true }
                    );
                }
            });
        }
        setKeyControls(v) {
            if (v) {
                this.viewer.impl.controls.handleKeyDown = this.#controls.handleKeyDown;
                this.viewer.impl.controls.handleKeyUp = this.#controls.handleKeyUp;
            }
            else {
                this.viewer.impl.controls.handleKeyDown = function (e) { };
                this.viewer.impl.controls.handleKeyUp = function (e) { };
            }
        }
        setPointerControls(v) {
            const handleClick = () => {
                this.viewer.clearSelection()
            }
            const checkCursor = () => {
                this.viewer.container.style.cursor = this.viewer.canvas.style.cursor;
            }
            if (v) {
                this.viewer.canvas.style.pointerEvents = null;
                this.viewer.removeEventListener(Autodesk.Viewing.AGGREGATE_SELECTION_CHANGED_EVENT, handleClick);
                this.viewer.removeEventListener(Autodesk.Viewing.CAMERA_CHANGE_EVENT, checkCursor);
            }
            else {
                this.viewer.canvas.style.pointerEvents = 'none';
                this.viewer.addEventListener(Autodesk.Viewing.AGGREGATE_SELECTION_CHANGED_EVENT, handleClick);
                this.viewer.addEventListener(Autodesk.Viewing.CAMERA_CHANGE_EVENT, checkCursor);
            }
        }
        #map(n, start1, stop1, start2, stop2) {
            return ((n - start1) / (stop1 - start1)) * (stop2 - start2) + start2;
        }
        normalizePosition(v) {
            const bbox = this.viewer.model.getBoundingBox();
            return new THREE.Vector3(
                this.#map(v.x, bbox.min.x, bbox.max.x, -1, 1),
                this.#map(v.y, bbox.min.y, bbox.max.y, -1, 1),
                this.#map(v.z, bbox.min.z, bbox.max.z, -1, 1)
            );
        }
        unnormalizePosition(v) {
            const bbox = this.viewer.model.getBoundingBox();
            return new THREE.Vector3(
                this.#map(v.x, -1, 1, bbox.min.x, bbox.max.x),
                this.#map(v.y, -1, 1, bbox.min.y, bbox.max.y),
                this.#map(v.z, -1, 1, bbox.min.z, bbox.max.z)
            );
        }
    }
    Autodesk.Viewing.theExtensionManager.registerExtension("Viewer.Toolkit", Toolkit);
})();
