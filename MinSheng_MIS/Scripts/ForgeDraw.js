//import "https://pixijs.download/release/pixi.js";

var ForgeDraw = (function (e) {
    var app;
    var view;
    var stage;
    var lineData = [];
    var points = [];
    var lines = [];
    var devices = [];

    var forgeViewer;
    var selectPos = {};
    var layer = {
        stage: new PIXI.Container(),
        line: new PIXI.Container(),
        point: new PIXI.Container(),
        device: new PIXI.Container(),
    }

    /* control */
    const Control = Object.freeze({
        NONE: 0,
        DRAW: 1,
        ERASER: 2,
        DEVICE: 3,
    })
    var currentControl = Control.DRAW;

    /* event */
    class LineDataChangeEvent {
        constructor(detail) {
            return new CustomEvent('fd.linedata.change', { 'detail': detail })
        }
    }
    class LineDataDetailEvent {
        constructor(detail) {
            return new CustomEvent('fd.linedata.detail', { 'detail': detail })
        }
    }
    class LineDataRemoveAllEvent {
        constructor(detail) {
            return new CustomEvent('fd.linedata.removeall', { 'detail': detail })
        }
    }
    class DevicePointUserCreateEvent {
        constructor(detail) {
            return new CustomEvent('fd.devicepoint.usercreate', { 'detail': detail })
        }
    }
    class DevicePointIgnoreEvent {
        constructor(detail) {
            return new CustomEvent('fd.devicepoint.ignore', { 'detail': detail })
        }
    }

    /* setting */
    const Colors = {
        Start: 0xEC6767,
        Middle: 0xFE9292,
        Bool: 0x00f5d4,
        End: 0xFF9559,
        Line: 0xFCC7C7,
        BlueTooth: 0x2750B9,
        DefaultDevice: 0x3ac0ff,
    };

    const drawSetting = {
        point: {
            width: 8,
            hoverWidth: 8,
            strokeWeight: 2,
            color: Colors.Middle,
            interactive: true,
            contextMenu: {
                html: `<div class="contextMenu"><ul></ul></div>`,
                button: [
                    {
                        name: "詳細",
                        onClick: function (event, point) {
                            console.log(`point ${point.index} => 詳細`);
                            view.dispatchEvent(new LineDataDetailEvent(lineData[point.index]));
                        }
                    },
                    {
                        name: "刪除",
                        onClick: function (event, point) {
                            console.log(`point ${point.index} => 刪除`);
                            point.remove();
                        }
                    },
                ]
            }
        },
        devicePoint: {
            width: 8,
            hoverWidth: 8,
            strokeWeight: 3,
            strokeColor: Colors.Line,
            color: Colors.BlueTooth,
            interactive: true,
            nearestLine: false,
            label: true,
            contextMenu: {
                html: `<div class="contextMenu"><ul></ul></div>`,
                button: [
                    {
                        name: "詳細",
                        onClick: function (event, point) {
                            console.log(`devicePoint ${point.dbId} => 詳細`);
                            view.dispatchEvent(new LineDataDetailEvent(point));
                        }
                    },
                    {
                        name: "忽略",
                        onClick: function (event, point) {
                            console.log(`devicePoint ${point.dbId} => 忽略`);
                            point.isUpdate = !point.isUpdate;
                            event.target.innerHTML = point.isUpdate ? "忽略" : "取消忽略";
                            point.update();
                            view.dispatchEvent(new DevicePointIgnoreEvent(point));
                        }
                    },
                ]
            }
        },
        line: {
            strokeWeight: 6,
            color: Colors.Line,
            interactive: true
        }
    };

    /* function */
    function init(canvas, forgeGuiViewer3D, callback = function () { }) {
        view = canvas;
        let rect = view.getBoundingClientRect();
        app = new PIXI.Application({
            view: view,
            width: rect.width,
            height: rect.height,
            backgroundAlpha: 0,
            antialias: true,
        });

        app.stage.addChild(...Object.values(layer));

        view.addEventListener("contextmenu", preventDefaultEvent);
        $(document.body).on("contextmenu", ".contextMenu", preventDefaultEvent);

        forgeViewer = forgeGuiViewer3D;

        forgeViewer.addEventListener(
            Autodesk.Viewing.SELECTION_CHANGED_EVENT,
            getDeviceCenterPosition
        );

        stage = new ForgeDraw.Stage();
        exports.stage = stage;

        callback();
        return app;

        function getDeviceCenterPosition(e) {
            let dbId = e.dbIdArray[0]; //target dbId
            let box = viewer.utilities.getBoundingBox(); //THREE.Box3
            selectPos[dbId] = box.center(); //獲取 target 中心點座標並記錄於selectPos
            forgeViewer.clearSelection();
        }
    }

    function preventDefaultEvent(event) {
        event.preventDefault();
    }

    function updatePoints() {
        lineData.forEach((e, i, arr) => {
            if (i == 0) {
                points[i].color = Colors.Start;
                points[i].graphics.alpha = 1;
                lines[i] && lines[i].redraw(e.position, arr[i + 1].position);
            }
            else if (i == arr.length - 1) {
                points[i].color = Colors.End;
                points[i].graphics.alpha = 1;
            }
            else {
                points[i].color = Colors.Middle;
                points[i].graphics.alpha = 0;
                lines[i].redraw(e.position, arr[i + 1].position);
            }
            points[i].position = e.position;
        })
    }

    function updateLine(i, pos) {
        if (i !== 0) {
            lines[i - 1].redraw(lineData[i - 1].position, pos);
        }

        if (i !== lineData.length - 1) {
            lines[i].redraw(pos, lineData[i + 1].position);
        }
    }

    function removeAllData() {
        lineData.length = 0;
        points.length = 0;
        lines.length = 0;

        layer.line.removeChildren();
        layer.point.removeChildren();

        devices.forEach(e => e.update());

        view.dispatchEvent(new LineDataRemoveAllEvent());
    }

    function getRoute() {
        return devices
            .filter(e => e.result != undefined)
            .sort((a, b) => {
                if (a.result.i == b.result.i) {
                    return a.result.distanceToA - b.result.distanceToA;
                }
                return a.result.i - b.result.i;
            });
    }

    function getForgeLineData() {
        return lineData.map(e => { return forgeViewer.clientToWorld(e.position.x, e.position.y).point; });
    }

    function getControl() {
        return Object.values(Control).find(e => e == currentControl);
    }

    function setControl(control) {
        currentControl = control;
    }

    /* class */
    class drawObject {
        constructor() {
            this.name = "drawObject";
            this.container = new PIXI.Container();
            this.onOverEvent = function () { }
            this.onOutEvent = function () { }
            this.onDownEvent = function () { }
            this.onMoveEvent = function () { }
            this.onUpEvent = function () { }
            //this.create();
        }
        create() { }
        redraw() { }
        on(eventKey, callback) {
            this.container.on(eventKey, callback);
        }
        off(eventKey, callback) {
            this.container.off(eventKey, callback);
        }
    }

    class Line extends drawObject {
        constructor(start, end, options = {}) {
            super();
            this.name = "Line";
            this.options = Object.assign({}, drawSetting.line, options);
            this.lineStyle = {
                width: this.options.strokeWeight,
                color: this.options.color,
                alpha: 1,
                cap: PIXI.LINE_CAP.ROUND,
                join: PIXI.LINE_JOIN.ROUND,
            };
            this.graphics;
            this.start = start;
            this.end = end;
            this.create();
        }
        /**
         * @param {Boolean} b
         */
        set interactive(b) {
            this.options.interactive = b;
            this.container.interactive = this.options.interactive;
        }
        get interactive() { return this.container.interactive }

        get index() { return lines.indexOf(this) }

        create() {
            this.graphics = new PIXI.Graphics()
                .lineStyle(this.lineStyle)
                .moveTo(this.start.x, this.start.y)
                .lineTo(this.end.x, this.end.y);

            this.container.addChild(this.graphics);
            this.container.interactive = this.options.interactive;

            layer.line.addChild(this.container);
        }
        redraw(newStart, newEnd) {
            this.start = newStart;
            this.end = newEnd;
            //console.log(this.start, this.end);
            this.graphics
                .clear()
                .lineStyle(this.lineStyle)
                .moveTo(this.start.x, this.start.y)
                .lineTo(this.end.x, this.end.y);
        }
    }

    class Point extends drawObject {
        constructor(pos, options = {}) {
            super();
            this.name = "Point";
            this.options = Object.assign({}, drawSetting.point, options);
            this.position = new PIXI.Point(pos.x, pos.y);
            this.graphics;
            this.over;
            this.hitAreaVisible = false;
            this.create();
        }
        /**
         * @param {number} c
         */
        set color(c) {
            this.options.color = c;
            this.graphics.tint = this.options.color;
        }
        get color() { return this.options.color }

        /**
         * @param {PIXI.Point} pos
         */
        set position(pos) {
            this.container.position = pos;
        }
        get position() { return this.container.position }

        /**
         * @param {Boolean} b
         */
        set interactive(b) {
            this.options.interactive = b;
            this.container.interactive = this.options.interactive;
        }
        get interactive() { return this.container.interactive }

        get index() { return points.indexOf(this); }

        calcHitArea() {
            return new PIXI.Rectangle(
                -this.options.hoverWidth,
                -this.options.hoverWidth,
                this.options.hoverWidth * 2,
                this.options.hoverWidth * 2
            )
        }

        create() {
            const self = this;
            this.graphics = new PIXI.Graphics()
                .lineStyle(0)
                .beginFill(0xffffff, 1)
                .drawCircle(0, 0, this.options.width)
                .endFill();

            this.over = new PIXI.Graphics()
                .lineStyle(this.options.strokeWeight, 0xffffff, 0.8)
                .beginFill(0xffffff, 0)
                .drawCircle(0, 0, this.options.width)
                .endFill();

            this.hitAreaRect = new PIXI.Graphics()
                .lineStyle(1, 0x00ff00, 0.8)
                .beginFill(0xffffff, 0)
                .drawRect(-this.options.hoverWidth,
                    -this.options.hoverWidth,
                    this.options.hoverWidth * 2,
                    this.options.hoverWidth * 2)
                .endFill();

            this.graphics.tint = this.options.color;

            this.container.interactive = this.options.interactive;
            //this.container.hitArea = this.calcHitArea();
            this.container.position = this.position;
            this.container.addChild(this.graphics, this.over, this.hitAreaRect);
            this.over.visible = true;
            this.hitAreaRect.visible = this.hitAreaVisible;

            layer.point.addChild(this.container);

            if (this.options.contextMenu) {
                this.contextMenu = new ContextMenu(this, this.options.contextMenu);
            }

            this.onOverEvent = function (event) {
                console.log(`${self.name} ${self.index} => onOverEvent`);
                !(self.index == 0 || self.index == points.length - 1) && (
                    self.graphics.alpha = 1
                )
                self.over.visible = true;
            }
            this.onOutEvent = function (event) {
                console.log(`${self.name} ${self.index} => onOutEvent`);
                !(self.index == 0 || self.index == points.length - 1) && (
                    self.graphics.alpha = 0
                )
                self.over.visible = false;
            }
            this.onRightDownEvent = function (event) {
                console.log(`${self.name} ${self.index} => onRightDownEvent`);
                self.off("pointerup", self.onUpEvent);
                self.off("pointerout", self.onDownOutEvent);
                stage.off("pointerdown", stage.onDownEvent);
                stage.on("pointerdown", self.onRightUpEvent);
                document.body.addEventListener("click", self.onRightUpEvent);

                if (self.contextMenu) self.contextMenu.show();
            }
            this.onRightUpEvent = function (event) {
                console.log(`${self.name} ${self.index} => onRightUpEvent`);
                stage.off("pointerdown", self.onRightUpEvent);
                self.on("pointerout", self.onOutEvent);
                stage.on("pointerdown", stage.onDownEvent);
                document.body.removeEventListener("click", self.onRightUpEvent);
                self.onOutEvent();

                if (self.contextMenu) self.contextMenu.hide();
            }
            this.onDownEvent = function (event) {
                console.log(`${self.name} ${self.index} => onDownEvent`);
                self.off("pointerout", self.onOutEvent);
                self.on("pointerup", self.onUpEvent);
                self.on("pointerout", self.onDownOutEvent);
            }
            this.onDownOutEvent = function (event) {
                console.log(`${self.name} ${self.index} => onDownOutEvent`);
                self.off("pointerout", self.onDownOutEvent);
                self.off("pointerup", self.onUpEvent);

                stage.on("pointermove", self.onMoveEvent);
                stage.on("pointerup", self.onMoveUpEvent);
                self.on("pointermove", self.onMoveEvent);
                self.on("pointerup", self.onMoveUpEvent);
            }
            this.onMoveEvent = function (event) {
                console.log(`${self.name} ${self.index} => onMoveEvent`);
                let pos = this.parent.toLocal(event.global, null);
                self.position = pos;

                lineData[self.index].position = pos;

                view.dispatchEvent(new LineDataChangeEvent(lineData[self.index]));

                updateLine(self.index, pos);
            }
            this.onMoveUpEvent = function (event) {
                console.log(`${self.name} ${self.index} => onMoveUpEvent`);
                stage.off("pointermove", self.onMoveEvent);
                stage.off("pointerup", self.onMoveUpEvent);
                self.off("pointermove", self.onMoveEvent);
                self.off("pointerup", self.onMoveUpEvent);

                self.on("pointerout", self.onOutEvent);

                self.interactive = true;
            }
            this.onUpEvent = function (event) {
                console.log(`${self.name} ${self.index} => onUpEvent`);
                switch (getControl()) {
                    case Control.ERASER:
                        self.remove();
                        break;
                    default:
                        self.off("pointerout", self.onDownOutEvent);
                        self.off("pointerup", self.onUpEvent);
                        self.on("pointerout", self.onOutEvent);
                        break;
                }
            }

            this.on("pointerdown", this.onDownEvent);

            this.on("pointerover", this.onOverEvent);
            this.on("pointerout", this.onOutEvent);

            this.on("rightdown", this.onRightDownEvent);
        }

        remove() {
            let i = this.index;
            if (lines.length != 0) {
                if (i != lineData.length - 1) {
                    lines.splice(i, 1);
                    layer.line.removeChildAt(i);
                }
                else {
                    lines.splice(i - 1, 1);
                    layer.line.removeChildAt(i - 1);
                }
            }
            layer.point.removeChildAt(i);
            points.splice(i, 1);
            lineData.splice(i, 1);

            updatePoints();

            view.dispatchEvent(new LineDataChangeEvent(null));

            this.contextMenu.remove();

            delete this;
        }
    }

    class DevicePoint extends drawObject {
        constructor(data, options = {}) {
            super();
            this.options = Object.assign({}, drawSetting.devicePoint, options);
            this.name = data.deviceName;
            this.dbId = data.dbId;
            this.type = data.deviceType;
            this.sprite = this.options.sprite;
            this.position = data.position ?? new PIXI.Point(0, 0);
            this.isUpdate = true;
            this.result = undefined;
            this.create();
        }

        /**
         * @param {Boolean} b
         */
        set interactive(b) {
            this.options.interactive = b;
            this.graphics.interactive = this.options.interactive;
        }
        get interactive() { return this.graphics.interactive }

        get index() { return devices.indexOf(this); }

        setPiovtCenter(c) {
            c.pivot.x = c.width / 2;
            c.pivot.y = c.height / 2;
        }
        drawText(text = this.name) {
            let padding = 4;
            this.text = new PIXI.Container();
            let t = new PIXI.Text(text, new PIXI.TextStyle({
                fontSize: 16,
                fill: "#ffffff",
                wordWrap: true,
                align: "center",
            }));
            let b = new PIXI.Graphics()
                .lineStyle(0)
                .beginFill(this.options.color, 0.8)
                .drawRect(-padding, -padding, t.width + (padding * 2), t.height + (padding * 2))
                .endFill();
            this.text.addChild(b, t);
            this.setPiovtCenter(this.text);

            this.text.position = this.position;
            this.text.position.x += (this.options.strokeWeight + this.options.width) / 2;
            this.text.position.y += 32;

            this.text.visible = this.options.label;
            this.container.addChild(this.text);
        }
        create() {
            const self = this;
            if (this.dbId) {
                forgeViewer.select(this.dbId);
                this.position = forgeViewer.worldToClient(selectPos[this.dbId]);
            }
            console.log(`${this.name} => create`, this.position);

            if (this.sprite !== undefined) {
                this.graphics = this.sprite;
            }
            else {
                this.graphics = new PIXI.Graphics()
                    .lineStyle(this.options.strokeWeight, 0xffffff)
                    .beginFill(this.options.color, 1)
                    .drawCircle(0, 0, this.options.width)
                    .endFill();
            }

            this.graphics.position = this.position;

            this.line = new PIXI.Graphics();
            this.graphics.interactive = this.options.interactive;
            this.container.addChild(this.line, this.graphics);

            if (this.options.label && this.name) {
                this.drawText();
            }

            layer.device.addChild(this.container);

            devices.push(this);

            if (this.options.contextMenu) {
                this.contextMenu = new ContextMenu(this, this.options.contextMenu);
            }

            this.onOverEvent = function (event) {
                console.log(`${self.name} ${self.index} => onOverEvent`);
                !self.isUpdate && self.text && (self.text.visible = true);
            }
            this.onOutEvent = function (event) {
                console.log(`${self.name} ${self.index} => onOutEvent`);
                !self.isUpdate && self.text && (self.text.visible = false);
            }
            this.onRightDownEvent = function (event) {
                console.log(`${self.name} ${self.index} => onRightDownEvent`);
                self.off("pointerout", self.onOutEvent);
                stage.off("pointerdown", stage.onDownEvent);
                stage.on("pointerdown", self.onRightUpEvent);
                document.body.addEventListener("click", self.onRightUpEvent);

                if (self.contextMenu) self.contextMenu.show();
            }
            this.onRightUpEvent = function (event) {
                console.log(`${self.name} ${self.index} => onRightUpEvent`);
                stage.off("pointerdown", self.onRightUpEvent);
                self.on("pointerout", self.onOutEvent);
                stage.on("pointerdown", stage.onDownEvent);
                document.body.removeEventListener("click", self.onRightUpEvent);
                self.onOutEvent();

                if (self.contextMenu) self.contextMenu.hide();
            }

            this.on("pointerover", this.onOverEvent);
            this.on("pointerout", this.onOutEvent);

            this.on("rightdown", this.onRightDownEvent);

        }
        update() {
            this.text && (this.text.visible = this.isUpdate);
            this.graphics.alpha = this.isUpdate ? 1 : 0.5;
            if (!this.isUpdate) {
                this.result = undefined;
                this.line.clear();
                return;
            };
            let result = { r: app.view.width };
            lineData.forEach((e, i, arr) => {
                if (arr.length - 1 == i) { return; }
                let a = e.position;
                let b = arr[i + 1].position;
                let p = this.findNearest(this.position, a, b);
                let r = this.twoPointDistance(this.position, p);
                if (result.r > r && r < 50) {
                    result = {
                        i: i,
                        r: r,
                        x: p.x,
                        y: p.y,
                        distanceToA: this.twoPointDistance(this.position, a),
                    }
                }
            })
            if (result.i != undefined) {
                this.result = result;
                this.options.nearestLine && this.line
                    .clear()
                    .lineStyle(this.options.strokeWeight, this.options.strokeColor, 1)
                    .moveTo(this.position.x, this.position.y)
                    .lineTo(this.result.x, this.result.y);
            }
            else {
                this.result = undefined;
                this.line.clear();
            }
        }
        remove() {
            let i = this.index;

            layer.device.removeChildAt(i);
            devices.splice(i, 1);

            view.dispatchEvent(new LineDataChangeEvent(null));

            this.contextMenu.remove();

            delete this;
        }
        /**
         * find the point nearest p on line by a and b
         * @param {PIXI.Point} p 
         * @param {PIXI.Point} a 
         * @param {PIXI.Point} b 
         * @returns {PIXI.Point} nearest point
         */
        findNearest(p, a, b) {
            let atob = { x: b.x - a.x, y: b.y - a.y };
            let atop = { x: p.x - a.x, y: p.y - a.y };
            let len = atob.x * atob.x + atob.y * atob.y;
            let dot = atop.x * atob.x + atop.y * atob.y;
            let t = Math.min(1, Math.max(0, dot / len));
            dot = (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
            return { x: a.x + atob.x * t, y: a.y + atob.y * t };
        }
        /**
         * get distance by p1 to p2
         * @param {PIXI.Point} p1 
         * @param {PIXI.Point} p2 
         * @returns {number} distance
         */
        twoPointDistance(p1, p2) {
            return Math.sqrt(Math.pow((p1.x - p2.x), 2) + Math.pow((p1.y - p2.y), 2));
        }
        on(eventKey, callback) {
            this.graphics.on(eventKey, callback);
        }
        off(eventKey, callback) {
            this.graphics.off(eventKey, callback);
        }
    }

    class Stage extends drawObject {
        constructor() {
            super();
            this.name = "Stage";
            this.create();
        }
        create() {
            const self = this;
            layer.stage.addChild(this.container);

            let g = new PIXI.Graphics()
                .lineStyle(0)
                .beginFill(0x000000, 0)
                .drawRect(0, 0, app.view.width, app.view.height)
                .endFill();
            this.container.addChild(g);

            this.container.interactive = true;
            this.container.hitArea = new PIXI.Rectangle(0, 0, app.view.width, app.view.height);
            let movingPoint = undefined;
            let movingLine = undefined;

            this.onDownEvent = function (event) {
                let pos = this.parent.toLocal(event.global, null);

                console.log(`${self.name} => onDownEvent`, pos);
                switch (getControl()) {
                    case Control.DRAW:
                        movingPoint = new Point(pos, { interactive: false });
                        if (lineData.length == 0) {
                            movingPoint.color = Colors.Start;
                        }
                        else {
                            movingLine = new Line(lineData.at(-1).position, pos, { interactive: false });
                            movingPoint.color = Colors.End;
                        }

                        if (lineData.length >= 2) {
                            points.at(-1).graphics.alpha = 0;
                            points.at(-1).color = Colors.Middle;
                        }

                        self.on("pointermove", self.onMoveEvent);
                        self.on("pointerup", self.onUpEvent);
                        break;
                    case Control.DEVICE:
                        //todo
                        movingPoint = new Point(pos, {
                            color: Colors.DefaultDevice,
                            interactive: false,
                            label: false,
                        });
                        self.on("pointermove", self.onDeviceMoveEvent);
                        self.on("pointerup", self.onDeviceUpEvent);
                        break;
                }
            }

            this.onMoveEvent = function (event) {
                let pos = this.parent.toLocal(event.global, null);
                console.log(`${self.name} => onMoveEvent`, pos);
                movingPoint.position = pos;
                if (lineData.length !== 0) {
                    movingLine.redraw(lineData.at(-1).position, pos);
                }
            }

            this.onDeviceMoveEvent = function (event) {
                let pos = this.parent.toLocal(event.global, null);
                console.log(`${self.name} => onDeviceMoveEvent`, pos);
                movingPoint.position = pos;
            }

            this.onUpEvent = function (event) {
                console.log(`${self.name} => onUpEvent`);
                let data = {
                    position: new PIXI.Point(movingPoint.position.x, movingPoint.position.y)
                }
                lineData.push(data);
                view.dispatchEvent(new LineDataChangeEvent(data));
                movingPoint.interactive = true;
                points.push(movingPoint);

                if (lineData.length !== 1) {
                    movingLine.interactive = true;
                    lines.push(movingLine);
                }

                self.off("pointermove", self.onMoveEvent);
                self.off("pointerup", self.onUpEvent);

            }

            this.onDeviceUpEvent = function (event) {
                console.log(`${self.name} => onDeviceUpEvent`);
                let data = {
                    position: new PIXI.Point(movingPoint.position.x, movingPoint.position.y),
                    callback: () => { layer.point.removeChild(movingPoint.container); }
                }
                view.dispatchEvent(new DevicePointUserCreateEvent(data));
                self.off("pointermove", self.onDeviceMoveEvent);
                self.off("pointerup", self.onDeviceUpEvent);
            }

            this.on("pointerdown", this.onDownEvent);
        }
        redraw() {
            this.container.hitArea = new PIXI.Rectangle(0, 0, app.view.width, app.view.height);
        }

    }

    class ContextMenu {
        constructor(point, options = {}) {
            this.options = Object.assign({}, drawSetting.point.contextMenu, options);
            this.point = point;
            this.html = $(options.html);
            this.position = point.position;
            this.create();
        }
        create() {
            const self = this;
            let contextMenu = this.html;
            this.options.button.forEach((e) => {
                let btn = $(`<li><button class="btn">${e.name}</button></li>`);
                contextMenu.children("ul").append(btn);
                btn.on("click", function (event) {
                    self.point.onRightUpEvent();
                    e.onClick(event, self.point);
                    self.hide();
                });
            });
            contextMenu.detach();
        }
        show() {
            this.position = this.point.position;
            this.html.css({
                left: this.position.x,
                top: this.position.y
            });
            $(view).parent().append(this.html);
        }
        hide() {
            this.html.detach();
        }
        remove() {
            this.html.remove();
        }
    }

    /* exports */
    var exports = {
        "Line": Line,
        "Point": Point,
        "DevicePoint": DevicePoint,
        "Stage": Stage,
        "init": init,
        "removeAllData": removeAllData,
        "getRoute": getRoute,
        "getForgeLineData": getForgeLineData,
        "getControl": getControl,
        "setControl": setControl,
        "Control": Control,
        "drawSetting": drawSetting,
        "layer": layer,
        "lineData": lineData,
        "points": points,
        "devices": devices,
        "lines": lines,
        "stage": stage
    }
    return exports;
})()