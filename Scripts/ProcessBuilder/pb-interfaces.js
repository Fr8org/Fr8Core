(function (ns) {

    ns.BaseFactory = Core.class({
        constructor:            function () {},

        createCanvas:           function (canvas) { throw 'Not implemented'; },
        createStartNode:        function () { throw 'Not implemented'; },
        createAddCriteriaNode:  function () { throw 'Not implemented'; },
        createCriteriaNode:     function (criteriaName) { throw 'Not implemented'; },
        createActionsNode:      function () { throw 'Not implemented'; },
        createAddActionNode:    function () { throw 'Not implemented'; },
        createActionNode:       function (actionNode) { throw 'Not implemented'; },
        createRightArrow:       function (left, top, length) { throw 'Not implemented'; },
        createDownArrow:        function (left, top, length) { throw 'Not implemented'; }
    });


    ns.BaseCanvas = Core.class(Core.CoreObject, {
        constructor: function (canvas) {
            ns.BaseCanvas.super.constructor.call(this);
            this._canvas = canvas;
        },

        getCanvas: function () {
            return this._canvas;
        },

        add: function (baseCanvasObject) {
            throw 'Not implemented';
        },

        remove: function (baseCanvasObject) {
            throw 'Not implemented';
        },

        getWidth: function () {
            throw 'Not implemented';
        },

        getHeight: function () {
            throw 'Not implemented';
        },

        resize: function (width, height) {
            throw 'Not implemented';
        },

        redraw: function () {
            throw 'Not implemented';
        }
    });


    ns.BaseCanvasObject = Core.class(Core.CoreObject, {
        constructor: function () {
            ns.BaseCanvasObject.super.constructor.call(this);
        },

        setLeft: function (left) {
            throw 'Not implemented';
        },

        getLeft: function () {
            throw 'Not implemented';
        },

        setTop: function (top) {
            throw 'Not implemented';
        },

        getTop: function () {
            throw 'Not implemented';
        },

        setWidth: function (width) {
            throw 'Not implemented';
        },

        getWidth: function () {
            throw 'Not implemented';
        },

        setHeight: function (height) {
            throw 'Not implemented';
        },

        getHeight: function () {
            throw 'Not implemented';
        },

        relayout: function () {
            throw 'Not implemented';
        }
    });

})(Core.ns('ProcessBuilder'));