(function (ns) {

    ns.WidgetConsts = {
        canvasPadding: 10,
        minSpaceBetweenObjects: 40,
        defaultSize: 130,
        startNodeHeight: 30,
        strokeWidth: 3
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
            this._redraw();
        },

        _redraw: function () {
            this._placeStartNode();
            this._placeAddCriteriaNode();
            this._placeAddCriteriaArrow();

            this._fabric.renderAll();
        },

        _predefinedObjects: function () {
            this._predefineStartNode();
            this._predefineAddCriteriaNode();
            this._predefineAddCriteriaArrow();
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
                fill: 'lightsalmon',
                stroke: 'coral',
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


        // ---------- region: AddCriteriaArrow routines. ----------

        _predefineAddCriteriaArrow: function () {
            var addCriteriaArrow = this._createAddCriteriaArrow();

            this._fabric.add(addCriteriaArrow);

            this._addCriteriaArrow = addCriteriaArrow;
        },

        _createAddCriteriaArrow: function () {
            var height = ns.WidgetConsts.minSpaceBetweenObjects;
            var pathDef = 'M 0 0 L 0 ' + height.toString()
                + 'M 0 ' + height.toString() + ' L -3 ' + (height - 3).toString()
                + 'M 0 ' + height.toString() + ' L 3 ' + (height - 3).toString();

            return new fabric.Path(pathDef, {
                stroke: 'black',
                strokeWidth: 1,
                fill: false,
                originX: 'left',
                originY: 'top'
            });
        },

        _placeAddCriteriaArrow: function () {
            var left = ns.WidgetConsts.canvasPadding + ns.WidgetConsts.defaultSize / 2 - 3;
            var top = this._getStartNodeBottomPoint() + 2;

            this._addCriteriaArrow.set('left', left);
            this._addCriteriaArrow.set('top', top);

            this._addCriteriaArrow.setCoords();
        },

        // ---------- endregion: AddCriteriaArrow routines. ----------


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
                fill: '#FFFF99',
                stroke: '#FFCC00',
                strokeWidth: ns.WidgetConsts.strokeWidth,
                angle: 45,
                selectable: false
            });
        },

        _placeAddCriteriaNode: function () {
            var size = Math.floor(Math.sqrt(ns.WidgetConsts.defaultSize * ns.WidgetConsts.defaultSize / 2));
            var left = ns.WidgetConsts.canvasPadding + ns.WidgetConsts.defaultSize / 2;
            var top = this._getStartNodeBottomPoint() + ns.WidgetConsts.minSpaceBetweenObjects;

            this._addCriteriaNode.set('left', left);
            this._addCriteriaNode.set('top', top);
            this._addCriteriaNode.set('width', size);
            this._addCriteriaNode.set('height', size);

            this._addCriteriaNode.setCoords();
        }

        // ---------- endregion: AddCriteriaNode routines. ----------
    });

})(Core.ns('ProcessBuilder'));