// Implementation of drawing abstraction with FabricJS.
(function (ns) {

    // FabricJS-related factory.
    ns.FabricJsFactory = Core.class(ns.BaseFactory, {
        constructor: function () {
            ns.FabricJsFactory.super.constructor.call(this);
        },

        // Create FabricJsCanvas object.
        createCanvas: function (canvas) {
            return Core.create(ns.FabricJsCanvas, canvas);
        },

        // Create FabricJsStartNode for StartNode.
        createStartNode: function () {
            return Core.create(ns.FabricJsStartNode);
        },

        // Create FabricJsAddCriteriaNode for AddCriteriaNode.
        createAddCriteriaNode: function () {
            return Core.create(ns.FabricJsAddCriteriaNode);
        },

        // Create FabricJsCriteriaNode for CriteriaNode.
        createCriteriaNode: function (criteriaName) {
            return Core.create(ns.FabricJsCriteriaNode, criteriaName);
        },

        // Create FabricJsActionsNode for ActionsNode.
        createActionsNode: function () {
            return Core.create(ns.FabricJsActionsNode);
        },

        // Create FabricJsAddActionNode for AddActionNode.
        createAddActionNode: function () {
            return Core.create(ns.FabricJsAddActionNode);
        },

        // Create FabricJsActionNode for ActionNode.
        createActionNode: function (actionName) {
            return Core.create(ns.FabricJsActionNode, actionName);
        },

        // Create FabricJsRightArrow for right-directed arrow.
        createRightArrow: function (left, top, length) {
            return Core.create(ns.FabricJsRightArrow, left, top, length);
        },

        // Create FabricJsDownArrow for down-directed arrow.
        createDownArrow: function (left, top, length) {
            return Core.create(ns.FabricJsDownArrow, left, top, length);
        }
    });


    // FabricJS-related canvas.
    ns.FabricJsCanvas = Core.class(ns.BaseCanvas, {
        constructor: function (canvas) {
            ns.FabricJsCanvas.super.constructor.call(this, canvas);
            this._fabric = null;
        },

        init: function () {
            // Initializing FabricJS canvas and disable multiple object selection.
            this._fabric = new fabric.Canvas(this.getCanvas(), {
                backgroundColor: ns.WidgetConsts.canvasBgFill,
                selection: false
            });
        },

        add: function (fabricCanvasObject) {
            this._fabric.add(fabricCanvasObject.getFabricObject());
        },

        remove: function (fabricCanvasObject) {
            this._fabric.remove(fabricCanvasObject.getFabricObject());
        },

        getWidth: function () {
            return this._fabric.getWidth();
        },

        getHeight: function () {
            return this._fabric.getHeight();
        },

        resize: function (width, height) {
            this._fabric.setWidth(width);
            this._fabric.setHeight(height);
            this._fabric.calcOffset();
        },

        redraw: function () {
            this._fabric.renderAll();
        }
    });


    // Base rectangle-bounded FabricJS-related canvas node.
    ns.BaseFabricJsObject = Core.class(ns.BaseCanvasObject, {
        constructor: function () {
            ns.BaseFabricJsObject.super.constructor.call(this);
        },

        // Get underlying FabricJS object.
        getFabricObject: function () {
            throw 'Not implemented';
        },

        setLeft: function (left) {
            this.getFabricObject().set('left', left);
        },

        getLeft: function () {
            return this.getFabricObject().get('left');
        },

        setTop: function (top) {
            this.getFabricObject().set('top', top);
        },

        getTop: function () {
            return this.getFabricObject().get('top');
        },

        setWidth: function (width) {
            this.getFabricObject().set('width', width);
        },

        getWidth: function () {
            return this.getFabricObject().get('width');
        },

        setHeight: function (height) {
            this.getFabricObject().set('height', height);
        },

        getHeight: function () {
            return this.getFabricObject().get('height');
        },

        relayout: function () {
            this.getFabricObject().setCoords();
        }
    });


    // FabricJS implementation of StartNode.
    ns.FabricJsStartNode = Core.class(ns.BaseFabricJsObject, {
        constructor: function () {
            ns.BaseFabricJsObject.super.constructor.call(this);

            this._object = null;
        },

        init: function () {
            var rect = new fabric.Rect({
                rx: ns.WidgetConsts.startNodeCornerRadius,
                ry: ns.WidgetConsts.startNodeCornerRadius,
                fill: ns.WidgetConsts.startNodeFill,
                stroke: ns.WidgetConsts.startNodeStroke,
                strokeWidth: ns.WidgetConsts.strokeWidth,
                selectable: false,
                originX: 'center',
                originY: 'center',
                width: ns.WidgetConsts.startNodeWidth,
                height: ns.WidgetConsts.startNodeHeight
            });
            
            var label = new fabric.Text('START', {
                fontSize: ns.WidgetConsts.startNodeTextSize,
                fontFamily: ns.WidgetConsts.startNodeTextFont,
                fill: ns.WidgetConsts.startNodeTextFill,
                selectable: false,
                originX: 'center',
                originY: 'center',
                top: ns.WidgetConsts.startNodeTextOffsetY
            });
            
            var group = new fabric.Group([rect, label], {
                selectable: false
            });

            group.on('mousedown', Core.delegate(function (e) { this.fire('click', e); }, this));
            
            this._object = group;
        },

        getFabricObject: function () {
            return this._object;
        }
    });


    // FabricJS implementation of AddCriteriaNode.
    ns.FabricJsAddCriteriaNode = Core.class(ns.BaseFabricJsObject, {
        constructor: function () {
            ns.BaseFabricJsObject.super.constructor.call(this);

            this._object = null;
        },

        init: function () {
            var halfWidth = Math.floor(ns.WidgetConsts.addCriteriaNodeWidth / 2);
            var halfHeight = Math.floor(ns.WidgetConsts.addCriteriaNodeHeight / 2);

            var diamond = new fabric.Path([
                    [ 'M', 0, -halfHeight ],
                    [ 'L', halfWidth, 0 ],
                    ['L', 0, halfHeight],
                    [ 'L', -halfWidth, 0 ]
                ],
                {
                    fill: ns.WidgetConsts.addCriteriaNodeFill,
                    stroke: ns.WidgetConsts.addCriteriaStroke,
                    strokeWidth: ns.WidgetConsts.strokeWidth,
                    originX: 'center',
                    originY: 'center'
                });

            var label = new fabric.Text('Add criteria', {
                fontSize: ns.WidgetConsts.addCriteriaNodeTextSize,
                fontFamily: ns.WidgetConsts.addCriteriaNodeTextFont,
                fill: ns.WidgetConsts.addCriteriaNodeTextFill,
                selectable: false,
                originX: 'center',
                originY: 'center'
            });

            var group = new fabric.Group([diamond, label], {
                selectable: false
            });

            group.on('mousedown', Core.delegate(function (e) { this.fire('click', e); }, this));

            this._object = group;
        },

        getFabricObject: function () {
            return this._object;
        }
    });


    // FabricJS implementation of CriteriaNode (criteria created by user).
    ns.FabricJsCriteriaNode = Core.class(ns.BaseFabricJsObject, {
        constructor: function (criteriaName) {
            ns.FabricJsCriteriaNode.super.constructor.call(this);

            this._criteriaName = criteriaName;
            this._object = this;
        },

        init: function () {
            var halfWidth = Math.floor(ns.WidgetConsts.criteriaNodeWidth / 2);
            var halfHeight = Math.floor(ns.WidgetConsts.criteriaNodeHeight / 2);

            var diamond = new fabric.Path([
                    ['M', 0, -halfHeight],
                    ['L', halfWidth, 0],
                    ['L', 0, halfHeight],
                    ['L', -halfWidth, 0]
                ],
                {
                    fill: ns.WidgetConsts.criteriaNodeFill,
                    stroke: ns.WidgetConsts.criteriaNodeStroke,
                    strokeWidth: ns.WidgetConsts.strokeWidth,
                    originX: 'center',
                    originY: 'center'
                });

            var label = new fabric.Text(this._criteriaName, {
                fontSize: ns.WidgetConsts.criteriaNodeTextSize,
                fontFamily: ns.WidgetConsts.criteriaNodeTextFont,
                fill: ns.WidgetConsts.criteriaNodeTextFill,
                selectable: false,
                originX: 'center',
                originY: 'center'
            });

            var group = new fabric.Group([diamond, label], {
                selectable: false
            });

            group.on('mousedown', Core.delegate(function (e) { this.fire('click', e); }, this));

            this._object = group;
        },

        getFabricObject: function () {
            return this._object;
        }
    });


    // FabricJS implementation of ActionsNode (actions panel).
    ns.FabricJsActionsNode = Core.class(ns.BaseFabricJsObject, {
        constructor: function () {
            ns.FabricJsActionsNode.super.constructor.call(this);

            this._object = this;
        },

        init: function () {
            var rect = new fabric.Rect({
                rx: ns.WidgetConsts.actionsNodeCornerRadius,
                ry: ns.WidgetConsts.actionsNodeCornerRadius,
                fill: ns.WidgetConsts.actionsNodeFill,
                stroke: ns.WidgetConsts.actionsNodeStroke,
                strokeWidth: ns.WidgetConsts.strokeWidth,
                selectable: false
            });

            this._object = rect;
        },

        getFabricObject: function () {
            return this._object;
        }
    });


    // FabricJS implementation of AddActionNode (add action button).
    ns.FabricJsAddActionNode = Core.class(ns.BaseFabricJsObject, {
        constructor: function () {
            ns.FabricJsAddActionNode.super.constructor.call(this);

            this._object = this;
        },

        init: function () {
            var label = new fabric.Text('Add Action', {
                fontSize: ns.WidgetConsts.addActionNodeTextSize,
                fontFamily: ns.WidgetConsts.addActionNodeTextFont,
                fill: ns.WidgetConsts.addActionNodeTextFill,
                selectable: false
            });

            label.on('mousedown', Core.delegate(function (e) { this.fire('click', e); }, this));

            this._object = label;
        },

        getFabricObject: function () {
            return this._object;
        }
    });


    // FabricJS implementation of ActionNode (action created by user).
    ns.FabricJsActionNode = Core.class(ns.BaseFabricJsObject, {
        constructor: function (actionName) {
            ns.FabricJsActionNode.super.constructor.call(this);

            this._actionName = actionName;
            this._object = this;
        },

        init: function () {
            var label = new fabric.Text(this._actionName, {
                fontSize: ns.WidgetConsts.actionNodeTextSize,
                fontFamily: ns.WidgetConsts.actionNodeTextFont,
                fill: ns.WidgetConsts.actionNodeTextFill,
                selectable: false
            });

            label.on('mousedown', Core.delegate(function (e) { this.fire('click', e); }, this));

            this._object = label;
        },

        getFabricObject: function () {
            return this._object;
        }
    });


    // Right-directed FabricJS arrow.
    ns.FabricJsRightArrow = Core.class(ns.BaseFabricJsObject, {
        constructor: function (left, top, length) {
            ns.BaseFabricJsObject.super.constructor.call(this);

            this._left = left;
            this._top = top;
            this._length = length;
            this._object = null;
        },

        init: function () {
            var path = [
                    ['M', 0, 0],
                    ['L', this._length, 0],
                    ['M', this._length, 0],
                    ['L', this._length - ns.WidgetConsts.arrowSize, -ns.WidgetConsts.arrowSize],
                    ['M', this._length, 0],
                    ['L', this._length - ns.WidgetConsts.arrowSize, ns.WidgetConsts.arrowSize]
                ];

            var arrow = new fabric.Path(path, {
                stroke: ns.WidgetConsts.arrowStroke,
                strokeWidth: ns.WidgetConsts.arrowStrokeWidth,
                fill: false,
                selectable: false,
                originX: 'left',
                originY: 'center',
                left: this._left,
                top: this._top
            });

            this._object = arrow;
        },

        getFabricObject: function () {
            return this._object;
        }
    });


    // Down-directed FabricJS arrow.
    ns.FabricJsDownArrow = Core.class(ns.BaseFabricJsObject, {
        constructor: function (left, top, length) {
            ns.BaseFabricJsObject.super.constructor.call(this);

            this._left = left;
            this._top = top;
            this._length = length;
            this._object = null;
        },

        init: function () {
            var path = [
                    ['M', 0, 0],
                    ['L', 0, this._length],
                    ['M', 0, this._length],
                    ['L', -ns.WidgetConsts.arrowSize, this._length - ns.WidgetConsts.arrowSize],
                    ['M', 0, this._length],
                    ['L', ns.WidgetConsts.arrowSize, this._length - ns.WidgetConsts.arrowSize]
                ];

            var arrow = new fabric.Path(path, {
                stroke: ns.WidgetConsts.arrowStroke,
                strokeWidth: ns.WidgetConsts.arrowStrokeWidth,
                fill: false,
                selectable: false,
                originX: 'center',
                originY: 'top',
                left: this._left,
                top: this._top
            });

            this._object = arrow;
        },

        getFabricObject: function () {
            return this._object;
        }
    });


})(Core.ns('PlanBuilder'));