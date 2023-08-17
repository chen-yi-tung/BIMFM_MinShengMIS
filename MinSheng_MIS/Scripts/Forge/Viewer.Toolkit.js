AutodeskNamespace("Autodesk.Viewer.Extension");
Autodesk.Viewer.Extension.Toolkit = function (viewer, options) {
    Autodesk.Viewing.Extension.call(this, viewer, options);
    this.name = "Viewer.Toolkit";

    /**
     * 根據rootId獲得所有子dbId
     * @param {Autodesk.Viewing.Private.InstanceTree} it 
     * @param {number} rootId 任意dbId
     * @param {boolean} [last=true] 只取得最底層dbId，預設為true
     * @returns {Promise<Number[]>} 子dbId
     */
    this.getAlldbIds = async function (it, rootId, last = true) {
        return new Promise((resolve) => {
            const alldbId = [];
            if (!rootId) { return alldbId; }
            //若已為最底層則回傳rootId
            if (it.getChildCount(rootId) == 0) { alldbId.push(rootId); return alldbId; }
            const queue = [];
            queue.push(rootId);
            while (queue.length > 0) {
                const node = queue.shift();

                if (last == false) { alldbId.push(node); }
                else if (last && it.getChildCount(node) == 0) { alldbId.push(node); }

                it.enumNodeChildren(node, (childNode) => {
                    childNode && queue.push(childNode);
                });
            }
            //console.log("alldbId", alldbId)
            resolve(alldbId);
        })
    }
    /**
     * 變更模型材質顏色和透明度
     * @param {Autodesk.Viewing.Model} model 
     * @param {number} dbid 任意dbId
     * @param {number} color 0x000000
     * @param {number} [alpha=1] 透明度值，範圍應該在 0 到 1 之間
     * @returns {Promise<boolean>} return true is success
     */
    this.changeColor = async function (model, dbid, color, alpha = 1) {
        return new Promise((resolve) => {
            model.getObjectTree(async (it) => {
                const fl = it.fragList
                const alldbId = await this.getAlldbIds(it, dbid)
                alldbId.forEach((node) => {
                    it.enumNodeFragments(node, (fragId) => {
                        var mat = null
                        var matName = 'model:' + model.id.toString() + '|frag:' + fragId.toString();

                        if (matName in viewer.impl.matman()._materials) {
                            mat = viewer.impl.matman()._materials[matName];
                            mat = changeMat(mat);
                        } else {
                            mat = fl.getMaterial(fragId).clone();
                            mat = changeMat(mat);
                            viewer.impl.matman().addMaterial(matName, mat, true);
                        }
                        fl.setMaterial(fragId, mat);
                        viewer.impl.invalidate(true, true, true);
                    })
                })
                resolve(true)
            })
        })
        function changeMat(m) {
            m.opacity = alpha;
            m.transparent = alpha < 1 ? true : false;
            m.color = new THREE.Color(color);
            m.depthWrite = alpha < 1 ? false : true;
            m.needsUpdate = true;
            return m;
        }
    }
    /**
     * 變更模型材質
     * @param {Autodesk.Viewing.Model} model 
     * @param {number} dbid 
     * @param {THREE.MeshBasicMaterial | THREE.MeshPhongMaterial | THREE.MeshStandardMaterial} material 
     * @returns {Promise<boolean>} return true is success
     */
    this.changeMaterial = async function (model, dbid, material) {
        return new Promise((resolve) => {
            model.getObjectTree(async (it) => {
                const fl = it.fragList
                const alldbId = await this.getAlldbIds(it, dbid)
                alldbId.forEach((node) => {
                    it.enumNodeFragments(node, (fragId) => {
                        var matName = 'model:' + model.id.toString() + '|frag:' + fragId.toString();

                        if (matName in viewer.impl.matman()._materials) {
                            viewer.impl.matman()._materials[matName] = material;
                        } else {
                            viewer.impl.matman().addMaterial(matName, material, true);
                        }
                        fl.setMaterial(fragId, material);
                        viewer.impl.invalidate(true, true, true);
                    }, true)
                })
                resolve(true)
            })
        })
    }
    /**
     * 返回指定dbid模型的邊界
     * @param {Autodesk.Viewing.Model} model 
     * @param {number} dbid 
     * @returns {Promise<THREE.Box3>}
     */
    this.getBoundingBox = async function (model, dbid) {
        return new Promise((resolve) => {
            const bounds = new THREE.Box3()
            model.getObjectTree((it) => {
                const fl = it.fragList
                it.enumNodeFragments(dbid, (fragId) => {
                    let box = new THREE.Box3();
                    fragList.getWorldBounds(fragId, box);
                    bounds.union(box);
                }, true);
                resolve(bounds)
            })
        })
    }
    /**
     * 指定縮放距離的FitToView
     * @param {number} dbId 
     * @param {number} [scale=1] 縮放距離，1為最近
     * @param {boolean} [immediate=false] 立即縮放
     */
    this.fitToView = async function (dbId, scale = 1, immediate = false) {
        dbId = parseInt(input.value);
        // THREE.Box3.expandByScalar(scale) => 縮放距離，1為最近
        let bounds = await getBoundingBox(dbId);
        bounds.expandByScalar(scale)
        await viewer.navigation.fitBounds(immediate, bounds, true, false)
    }
}
Autodesk.Viewer.Extension.Toolkit.prototype = Object.create(Autodesk.Viewing.Extension.prototype);
Autodesk.Viewer.Extension.Toolkit.prototype.constructor = Autodesk.Viewer.Extension.Toolkit;
Autodesk.Viewer.Extension.Toolkit.prototype.load = function () {
    delete this.activate;
    delete this.activeStatus;
    delete this.deactivate;
    delete this.mode;
    delete this.modes;
    this.viewer.toolkit = this;
    return this.viewer && !0;
};
Autodesk.Viewing.theExtensionManager.registerExtension('Viewer.Toolkit', Autodesk.Viewer.Extension.Toolkit);