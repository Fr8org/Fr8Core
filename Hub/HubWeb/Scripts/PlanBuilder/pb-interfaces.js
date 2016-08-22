// Interfaces for abstracting drawing routines.
(function (ns) {

    // Factory that creates canvas and nodes for visual editor.
    ns.BaseFactory = Core.class({
        constructor: function () {},

        // Create BaseCanvas object.
        createCanvas: function (canvas) { throw 'Not implemented'; },

        // Create BaseCanvasObject for StartNode.
        createStartNode: function () { throw 'Not implemented'; },

        // Create BaseCanvasObject for AddCriteriaNode.
        createAddCriteriaNode: function () { throw 'Not implemented'; },

        // Create BaseCanvasObject for CriteriaNode (criteria added by user).
        createCriteriaNode: function (criteriaName) { throw 'Not implemented'; },

        // Create BaseCanvasObject for ActionsNode (actions panel).
        createActionsNode: function () { throw 'Not implemented'; },

        // Create BaseCanvasObject for AddActionNode (add action button).
        createAddActionNode: function () { throw 'Not implemented'; },

        // Create BaseCanvasObject for ActionNode (action added by user).
        createActionNode: function (actionNode) { throw 'Not implemented'; },

        // Create BaseCanvasObject for right-directed arrow.
        createRightArrow: function (left, top, length) { throw 'Not implemented'; },

        // Create BaseCanvasObject for down-directed arrow.
        createDownArrow: function (left, top, length) { throw 'Not implemented'; }
    });


    // Base canvas object. We pass canvas HTML element to constructor of the class.
    ns.BaseCanvas = Core.class(Core.CoreObject, {
        constructor: function (canvas) {
            ns.BaseCanvas.super.constructor.call(this);
            this._canvas = canvas;
        },

        // Get undelying canvas HTML element.
        getCanvas: function () {
            return this._canvas;
        },

        // Add node to canvas.
        add: function (baseCanvasObject) {
            throw 'Not implemented';
        },

        // Remove node from canvas.
        remove: function (baseCanvasObject) {
            throw 'Not implemented';
        },

        // Get canvas width.
        getWidth: function () {
            throw 'Not implemented';
        },

        // Get canvas height.
        getHeight: function () {
            throw 'Not implemented';
        },

        // Resize canvas.
        resize: function (width, height) {
            throw 'Not implemented';
        },

        // Repaint canvas.
        redraw: function () {
            throw 'Not implemented';
        }
    });


    // Base rectangle-bounded canvas node.
    ns.BaseCanvasObject = Core.class(Core.CoreObject, {
        constructor: function () {
            ns.BaseCanvasObject.super.constructor.call(this);
        },

        // Set left position of node.
        setLeft: function (left) {
            throw 'Not implemented';
        },

        // Get left position of node.
        getLeft: function () {
            throw 'Not implemented';
        },

        // Set top position of node.
        setTop: function (top) {
            throw 'Not implemented';
        },

        // Get top position of node.
        getTop: function () {
            throw 'Not implemented';
        },

        // Set node's width.
        setWidth: function (width) {
            throw 'Not implemented';
        },

        // Get node's width.
        getWidth: function () {
            throw 'Not implemented';
        },

        // Set node's height.
        setHeight: function (height) {
            throw 'Not implemented';
        },

        // Get node's height.
        getHeight: function () {
            throw 'Not implemented';
        },

        // Relayout node after position or size has changed.
        relayout: function () {
            throw 'Not implemented';
        }
    });

})(Core.ns('PlanBuilder'));