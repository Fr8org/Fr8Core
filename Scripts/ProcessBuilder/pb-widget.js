(function (ns) {

    ns.WidgetConsts = {
        downMode: 1,
        rightMode: 2,

        canvasPadding: 10,
        minSpaceBetweenObjects: 40,
        defaultSize: 130,
        startNodeHeight: 30,
        strokeWidth: 2,

        startNodeStroke: 'coral',
        startNodeFill: 'lightsalmon',

        addCriteriaNodeStroke: '#FFCC00',
        addCriteriaNodeFill: '#FFFF99',

        criteriaNodeStroke: '#99FF00',
        criteriaNodeFill: '#CCFF99',

        arrowStroke: 'red',
        arrowStrokeWidth: 2,
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
                node: null,
                arrow: null,
                bottomNodePoint: 0
            };

            criteriaDescr.node = new fabric.Rect({
                fill: ns.WidgetConsts.criteriaNodeFill,
                stroke: ns.WidgetConsts.criteriaNodeStroke,
                strokeWidth: ns.WidgetConsts.strokeWidth,
                angle: 45,
                selectable: false
            });

            this._criteria.push(criteriaDescr);

            this._fabric.add(criteriaDescr.node);
            this.redraw();
        },

        removeCriteria: function (id) {
            var i;
            for (i = 0; i < this._criteria.length; ++i) {
                if (this._criteria[i].id === id) {
                    this._fabric.remove(this._criteria[i].node);

                    if (this._criteria[i].path) {
                        this._fabric.remove(this._criteria[i].path);
                    }

                    this._criteria.splice(i, 1);

                    return;
                }
            }
        },

        redraw: function () {
            var i, prevCriteria;

            this._placeStartNode();

            var prevBottomPoint = this._getStartNodeBottomPoint();

            for (i = 0; i < this._criteria.length; ++i) {
                if (i === 0) { prevCriteria = null; }
                else { prevCriteria = this._criteria[i - 1]; }

                this._placeCriteriaNode(this._criteria[i], prevCriteria);
                this._criteria[i].arrow = this._replaceArrow(
                    this._criteria[i].arrow,
                    ns.WidgetConsts.downMode,
                    prevBottomPoint,
                    this._getCriteriaNodeTopPoint(this._criteria[i])
                );

                prevBottomPoint = this._getCriteriaNodeBottomPoint(this._criteria[i]);
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
                Core.delegate(function () { this.fire('startNode:click'); }, this)
            );

            this._fabric.add(startNode);

            this._startNode = startNode;
        },

        _createStartNode: function () {
            return new fabric.Rect({
                rx: 10,
                ry: 10,
                fill: ns.WidgetConsts.startNodeFill,
                stroke: ns.WidgetConsts.startNodeStroke,
                strokeWidth: ns.WidgetConsts.strokeWidth,
                selectable: false
            });
        },

        _placeStartNode: function () {
            this._startNode.set('left', ns.WidgetConsts.canvasPadding);
            this._startNode.set('top', ns.WidgetConsts.canvasPadding);
            this._startNode.set('width', ns.WidgetConsts.defaultSize);
            this._startNode.set('height', ns.WidgetConsts.startNodeHeight);

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
                Core.delegate(function () { this.fire('addCriteriaNode:click'); }, this)
            );

            this._fabric.add(addCriteriaNode);

            this._addCriteriaNode = addCriteriaNode;
        },

        _createAddCriteriaNode: function () {
            return new fabric.Rect({
                fill: ns.WidgetConsts.addCriteriaNodeFill,
                stroke: ns.WidgetConsts.addCriteriaNodeStroke,
                strokeWidth: ns.WidgetConsts.strokeWidth,
                angle: 45,
                selectable: false
            });
        },

        _placeAddCriteriaNode: function () {
            var topOffset;
            if (this._criteria.length) {
                topOffset = this._getCriteriaNodeBottomPoint(this._criteria[this._criteria.length - 1]);
            }
            else {
                topOffset = this._getStartNodeBottomPoint();
            }

            var size = Math.floor(Math.sqrt(ns.WidgetConsts.defaultSize * ns.WidgetConsts.defaultSize / 2));
            var left = ns.WidgetConsts.canvasPadding + ns.WidgetConsts.defaultSize / 2;
            var top = topOffset + ns.WidgetConsts.minSpaceBetweenObjects;

            this._addCriteriaNode.set('left', left);
            this._addCriteriaNode.set('top', top);
            this._addCriteriaNode.set('width', size);
            this._addCriteriaNode.set('height', size);

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

            var height = ns.WidgetConsts.minSpaceBetweenObjects - ns.WidgetConsts.arrowPadding * 2;

            var left = ns.WidgetConsts.canvasPadding
                + ns.WidgetConsts.defaultSize / 2
                - ns.WidgetConsts.arrowSize;
            var top = this._getStartNodeBottomPoint() + ns.WidgetConsts.arrowPadding;

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

            var size = Math.floor(Math.sqrt(ns.WidgetConsts.defaultSize * ns.WidgetConsts.defaultSize / 2));
            var left = ns.WidgetConsts.canvasPadding + ns.WidgetConsts.defaultSize / 2;
            var top = topOffset + ns.WidgetConsts.minSpaceBetweenObjects;

            criteria.node.set('left', left);
            criteria.node.set('top', top);
            criteria.node.set('width', size);
            criteria.node.set('height', size);

            criteria.node.setCoords();

            criteria.bottomNodePoint = top + ns.WidgetConsts.defaultSize;
        },

        _getCriteriaNodeTopPoint: function (criteria) {
            return criteria.node.get('top');
        },

        _getCriteriaNodeBottomPoint: function (criteria) {
            return criteria.bottomNodePoint;
        }

        // ---------- endregion: CriteriaNode routines. ----------
    });

})(Core.ns('ProcessBuilder'));