(function (ns) {

    ns.WidgetConsts = {
        downMode: 1,
        rightMode: 2,

        canvasPadding: 10,
        minSpaceBetweenObjects: 40,
        defaultSize: 130,
        strokeWidth: 1,

        startNodeHeight: 30,
        startNodeStroke: 'coral',
        startNodeFill: 'lightsalmon',
        startNodeTextSize: 15,
        startNodeTextFill: 'white',
        startNodeTextFont: 'Tahoma',
        startNodeTextOffsetY: 3,

        addCriteriaNodeStroke: '#FFCC00',
        addCriteriaNodeFill: '#FFFF99',
        addCriteriaNodeTextSize: 15,
        addCriteriaNodeTextFill: '#303030',
        addCriteriaNodeTextFont: 'Tahoma',
        addCriteriaNodeTextOffsetY: 3,

        criteriaNodeStroke: '#99FF00',
        criteriaNodeFill: '#CCFF99',
        criteriaNodeTextSize: 15,
        criteriaNodeTextFill: 'black',
        criteriaNodeTextFont: 'Tahoma',

        actionsNodeFill: 'white',
        actionsNodeStroke: 'red',
        actionNodePadding: 5,
        actionNodeHeight: 30,
        addActionNodePadding: 5,
        addActionNodeHeight: 30,
        addActionNodeTextSize: 15,
        addActionNodeTextFill: 'black',
        addActionNodeTextFont: 'Tahoma',

        arrowStroke: '#303030',
        arrowStrokeWidth: 1,
        arrowSize: 4,
        arrowPadding: 4
    };


    ns.Widget = Core.class(Core.CoreObject, {
        constructor: function (element, initPixelWidth, initPixelHeight) {
            ns.Widget.super.constructor.call(this);

            this._el = Core.element(element);
            this._jqEl = $(this._el);
            this._canvas = null;

            this._pxWidth = initPixelWidth;
            this._pxHeight = initPixelHeight;

            this._fabric = null;

            this._startNode = null;
            this._addCriteriaNode = null;
            this._addCriteriaArrow = null;
            this._criteria = [];
        },

        init: function () {
            var canvas = $('<canvas width="' + this._pxWidth.toString() + '" height="' + this._pxHeight.toString() + '"></canvas>');

            this._canvas = canvas;
            this._jqEl.append(canvas);

            this._fabric = new fabric.Canvas(canvas[0]);
            this._fabric.selection = false;

            this._predefinedObjects();
            this.redraw();
        },

        addCriteria: function (criteria) {
            if (!criteria || !criteria.id) {
                throw 'Criteria must contain "id" property.';
            }

            var criteriaDescr = {
                id: criteria.id,
                data: criteria,
                actions: [],
                criteriaNode: null,
                criteriaArrow: null,
                actionsNode: null,
                addActionNode: null,
                actionsArrow: null,
                bottomNodePoint: 0,
                bottomSectionPoint: 0
            };

            criteriaDescr.criteriaNode = this._createCriteriaNode(
                criteria.name || ('Criteria #' + criteria.id.toString())
            );
            criteriaDescr.criteriaNode.on(
                'mousedown',
                Core.delegate(function (e) {
                    this.fire('criteriaNode:click', e, criteria.id);
                }, this)
            );

            criteriaDescr.actionsNode = this._createActionsNode();

            criteriaDescr.addActionNode = this._createAddActionNode();
            criteriaDescr.addActionNode.on(
                'mousedown',
                Core.delegate(function (e) {
                    this.fire('addActionNode:click', e, criteria.id);
                }, this)
            );


            this._criteria.push(criteriaDescr);

            this._fabric.add(criteriaDescr.criteriaNode);
            this._fabric.add(criteriaDescr.actionsNode);
            this._fabric.add(criteriaDescr.addActionNode);

            this.redraw();
        },

        removeCriteria: function (id) {
            var i;
            for (i = 0; i < this._criteria.length; ++i) {
                if (this._criteria[i].id === id) {
                    this._fabric.remove(this._criteria[i].addActionNode);
                    this._fabric.remove(this._criteria[i].actionsNode);
                    this._fabric.remove(this._criteria[i].criteriaNode);

                    if (this._criteria[i].criteriaArrow) {
                        this._fabric.remove(this._criteria[i].criteriaArrow);
                    }

                    this._criteria.splice(i, 1);

                    break;
                }
            }

            this.redraw();
        },

        addAction: function (criteriaId, action) {
            if (!action || !action.id) {
                throw 'Action must contain "id" property.';
            }

            var criteria = null;
            var i;
            for (i = 0; i < this._criteria.length; ++i) {
                if (this._criteria[i].id === criteriaId) {
                    criteria = this._criteria[i];
                    break;
                }
            }

            if (!criteria) { throw 'No criteria found with id = ' + criteriaId.toString(); }

            var actionDescr = {
                id: action.id,
                data: action,
                actionNode: null,
                bottomNodePoint: 0
            };

            // TODO: finish this method.
            this.redraw();
        },

        removeAction: function (criteriaId, actionId) {
            // TODO: finish this method.
            this.redraw();
        },

        redraw: function () {
            var i, prevCriteria;
            var j, prevAction;

            this._placeStartNode();

            var prevBottomPoint = this._getStartNodeBottomPoint();

            prevCriteria = null;
            for (i = 0; i < this._criteria.length; ++i) {
                this._placeCriteriaNode(this._criteria[i], prevCriteria);
                this._placeActionsNode(this._criteria[i]);
                this._placeAddActionNode(this._criteria[i]);

                prevAction = null;
                for (j = 0; j < this._criteria[i].actions.length; ++j) {
                    this._placeActionNode(this._criteria[i].actions[j], prevAction);
                    prevAction = this._criteria[i].actions[j];
                }

                this._criteria[i].criteriaArrow = this._replaceArrow(
                    this._criteria[i].criteriaArrow,
                    ns.WidgetConsts.downMode,
                    prevBottomPoint,
                    this._getCriteriaNodeTopPoint(this._criteria[i])
                );

                prevBottomPoint = this._getCriteriaNodeBottomPoint(this._criteria[i]);
                prevCriteria = this._criteria[i];
            }

            if (this._criteria.length > 0) {
                prevBottomPoint = this._getCriteriaNodeBottomPoint(this._criteria[this._criteria.length - 1]);
            }

            this._placeAddCriteriaNode();
            this._addCriteriaArrow = this._replaceArrow(
                this._addCriteriaArrow,
                ns.WidgetConsts.downMode,
                prevBottomPoint,
                this._getAddCriteriaNodeTopPoint()
            );

            this._fabric.renderAll();
        },

        _predefinedObjects: function () {
            this._predefineStartNode();
            this._predefineAddCriteriaNode();
        },

        // ---------- region: StartNode routines. ----------

        _predefineStartNode: function() {
            var startNode = this._createStartNode();

            startNode.on(
                'mousedown',
                Core.delegate(function (e) { this.fire('startNode:click', e); }, this)
            );

            this._fabric.add(startNode);

            this._startNode = startNode;
        },

        _createStartNode: function () {
            var rect = new fabric.Rect({
                rx: 10,
                ry: 10,
                fill: ns.WidgetConsts.startNodeFill,
                stroke: ns.WidgetConsts.startNodeStroke,
                strokeWidth: ns.WidgetConsts.strokeWidth,
                selectable: false,
                originX: 'center',
                originY: 'center',
                width: ns.WidgetConsts.defaultSize,
                height: ns.WidgetConsts.startNodeHeight
            });

            var label = new fabric.Text('Start', {
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

            return group;
        },

        _placeStartNode: function () {
            this._startNode.set('left', ns.WidgetConsts.canvasPadding);
            this._startNode.set('top', ns.WidgetConsts.canvasPadding);

            this._startNode.setCoords();
        },

        _getStartNodeBottomPoint: function () {
            return this._startNode.get('top')
                + this._startNode.get('height');
        },

        // ---------- endregion: StartNode routines. ----------


        // ---------- region: AddCriteriaNode routines. ----------

        _predefineAddCriteriaNode: function () {
            var addCriteriaNode = this._createAddCriteriaNode();

            addCriteriaNode.on(
                'mousedown',
                Core.delegate(function (e) { this.fire('addCriteriaNode:click', e); }, this)
            );

            this._fabric.add(addCriteriaNode);

            this._addCriteriaNode = addCriteriaNode;
        },

        _createAddCriteriaNode: function () {
            var size = Math.floor(Math.sqrt(ns.WidgetConsts.defaultSize * ns.WidgetConsts.defaultSize / 2));

            var rect = new fabric.Rect({
                fill: ns.WidgetConsts.addCriteriaNodeFill,
                stroke: ns.WidgetConsts.addCriteriaNodeStroke,
                strokeWidth: ns.WidgetConsts.strokeWidth,
                angle: 45,
                selectable: false,
                width: size,
                height: size,
                originX: 'center',
                originY: 'center'
            });

            var label = new fabric.Text('Add criteria...', {
                fontSize: ns.WidgetConsts.addCriteriaNodeTextSize,
                fontFamily: ns.WidgetConsts.addCriteriaNodeTextFont,
                fill: ns.WidgetConsts.addCriteriaNodeTextFill,
                selectable: false,
                originX: 'center',
                originY: 'center'
            });

            var group = new fabric.Group([rect, label], {
                selectable: false
            });

            return group;
        },

        _placeAddCriteriaNode: function () {
            var topOffset;
            if (this._criteria.length) {
                topOffset = this._getCriteriaNodeBottomPoint(this._criteria[this._criteria.length - 1]);
            }
            else {
                topOffset = this._getStartNodeBottomPoint();
            }

            var left = ns.WidgetConsts.canvasPadding;
            var top = topOffset + ns.WidgetConsts.minSpaceBetweenObjects;

            this._addCriteriaNode.set('left', left);
            this._addCriteriaNode.set('top', top);

            this._addCriteriaNode.setCoords();
        },

        _getAddCriteriaNodeTopPoint: function () {
            return this._addCriteriaNode.get('top');
        },

        // ---------- endregion: AddCriteriaNode routines. ----------

        // ---------- region: Arrows routines. ----------

        _replaceArrow: function (arrow, mode, from, to) {
            if (arrow !== null) {
                this._fabric.remove(arrow);
            }

            var height = (to - from) - ns.WidgetConsts.arrowPadding * 2;

            var left = ns.WidgetConsts.canvasPadding
                + ns.WidgetConsts.defaultSize / 2
                - ns.WidgetConsts.arrowSize;
            var top = from + ns.WidgetConsts.arrowPadding;

            var path = [
                ['M', 0, 0],
                ['L', 0, height],
                ['M', 0, height],
                ['L', -ns.WidgetConsts.arrowSize, height - ns.WidgetConsts.arrowSize],
                ['M', 0, height],
                ['L', ns.WidgetConsts.arrowSize, height - ns.WidgetConsts.arrowSize]
            ];

            arrow = new fabric.Path(path, {
                stroke: ns.WidgetConsts.arrowStroke,
                strokeWidth: ns.WidgetConsts.arrowStrokeWidth,
                fill: false,
                selectable: false,
                originX: 'left',
                originY: 'top',
                left: left,
                top: top
            });

            this._fabric.add(arrow);

            return arrow;
        },

        // ---------- endregion: Arrows routines. ----------


        // ---------- region: CriteriaNode routines. ----------

        _placeCriteriaNode: function (criteria, prevCriteria) {
            var topOffset;
            if (prevCriteria) {
                topOffset = this._getCriteriaNodeBottomPoint(prevCriteria);
            }
            else {
                topOffset = this._getStartNodeBottomPoint();
            }

            var left = ns.WidgetConsts.canvasPadding;
            var top = topOffset + ns.WidgetConsts.minSpaceBetweenObjects;

            criteria.criteriaNode.set('left', left);
            criteria.criteriaNode.set('top', top);

            criteria.criteriaNode.setCoords();

            criteria.bottomNodePoint = top + ns.WidgetConsts.defaultSize;
        },

        _createCriteriaNode: function (criteriaName) {
            var size = Math.floor(Math.sqrt(ns.WidgetConsts.defaultSize * ns.WidgetConsts.defaultSize / 2));

            var rect = new fabric.Rect({
                fill: ns.WidgetConsts.criteriaNodeFill,
                stroke: ns.WidgetConsts.criteriaNodeStroke,
                strokeWidth: ns.WidgetConsts.strokeWidth,
                angle: 45,
                selectable: false,
                width: size,
                height: size,
                originX: 'center',
                originY: 'center'
            });

            var label = new fabric.Text(criteriaName, {
                fontSize: ns.WidgetConsts.criteriaNodeTextSize,
                fontFamily: ns.WidgetConsts.criteriaNodeTextFont,
                fill: ns.WidgetConsts.criteriaNodeTextFill,
                selectable: false,
                originX: 'center',
                originY: 'center'
            });

            var group = new fabric.Group([rect, label], {
                selectable: false
            });

            return group;
        },

        _getCriteriaNodeTopPoint: function (criteria) {
            return criteria.criteriaNode.get('top');
        },

        _getCriteriaNodeBottomPoint: function (criteria) {
            return criteria.bottomNodePoint;
        },

        // ---------- endregion: CriteriaNode routines. ----------


        // ---------- region: ActionsNode routines. ----------

        _createActionsNode: function () {
            var rect = new fabric.Rect({
                rx: 10,
                ry: 10,
                fill: ns.WidgetConsts.actionsNodeFill,
                stroke: ns.WidgetConsts.actionsNodeStroke,
                strokeWidth: ns.WidgetConsts.strokeWidth,
                selectable: false
            });

            return rect;
        },

        _placeActionsNode: function (criteria) {
            var left = ns.WidgetConsts.canvasPadding
                + ns.WidgetConsts.defaultSize
                + ns.WidgetConsts.minSpaceBetweenObjects;

            var top = this._getCriteriaNodeTopPoint(criteria);

            var width = ns.WidgetConsts.defaultSize;
            var height = ns.WidgetConsts.addActionNodeHeight
                + criteria.actions.length * ns.WidgetConsts.actionNodeHeight;

            criteria.actionsNode.set('left', left);
            criteria.actionsNode.set('top', top);
            criteria.actionsNode.set('width', width);
            criteria.actionsNode.set('height', height);

            criteria.actionsNode.setCoords();
        },

        _createAddActionNode: function () {
            var label = new fabric.Text('Add action...', {
                fontSize: ns.WidgetConsts.addActionNodeTextSize,
                fontFamily: ns.WidgetConsts.addActionNodeTextFont,
                fill: ns.WidgetConsts.addActionNodeTextFill,
                selectable: false
            });

            return label;
        },

        _placeAddActionNode: function (criteria) {
            var left = ns.WidgetConsts.canvasPadding
                + ns.WidgetConsts.defaultSize
                + ns.WidgetConsts.minSpaceBetweenObjects
                + ns.WidgetConsts.addActionNodePadding;

            var top = this._getCriteriaNodeTopPoint(criteria)
                + ns.WidgetConsts.addActionNodePadding;

            criteria.addActionNode.set('left', left);
            criteria.addActionNode.set('top', top);

            criteria.addActionNode.setCoords();
        }

        // ---------- endregion: ActionsNode routines. ----------
    });

})(Core.ns('ProcessBuilder'));