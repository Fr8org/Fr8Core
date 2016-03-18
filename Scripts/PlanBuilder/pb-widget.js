(function (ns) {

    // ProcessBuilder widget class.
    // Fires events:
    //     'addActionNode:click' - when AddActionNode (add action button) is clicked; function (mouseEvent, criteriaId) { ... }
    //     'actionNode:click' - when ActionNode (add created by user) is clicked; function (mouseEvent, criteriaId, actionId) { ... }
    ns.Widget = Core.class(Core.CoreObject, {
        // Parameters:
        //     element - div element on HTML page, which serves as container for canvas element.
        //     factory - ProcessBuilder.BaseFactory instance.
        //     initPixelWidth - initial canvas width (in pixels).
        //     initPixelHeight - initial canvas height (in pixels).
        constructor: function (element, factory, initPixelWidth, initPixelHeight) {
            ns.Widget.super.constructor.call(this);

            this._el = Core.element(element);
            this._jqEl = $(this._el);

            this._pxWidth = initPixelWidth;
            this._pxHeight = initPixelHeight;

            this._factory = factory;
            this._canvas = null;

            this._actionsNode = null;
            this._addActionNode = null;
            this._actions = [];
        },

        // Creating canvas element, and populating canvas with predefined elements.
        init: function () {
            ns.ImageLoader.instance.loadImages(
                Core.delegate(
                    function () {
                        var canvas = $('<canvas width="' + this._pxWidth.toString() + '" height="' + this._pxHeight.toString() + '"></canvas>');

                        this._jqEl.append(canvas);

                        this._canvas = this._factory.createCanvas(canvas[0]);

                        this._predefinedObjects();
                        this.relayout();
                    },
                    this
                )
            );
        },

        // Add action to action list to specified criteria.
        // Parameters:
        //     criteriaId - id of criteria
        //     action - object to define action; minimum required set of properties: { id: 'someId' }
        addAction: function (criteriaId, action, actionType) {

            if (!action || !action.id) {
                throw 'Action must contain "id" property.';
            }

            if (actionType !== ns.ActionType.immediate) {
                throw 'Only immediate action types are supported so far.';
            }

            var actionDescr = {
                id: action.id,
                actionType: actionType,
                data: action,
                actionNode: null,
                activityTemplateId: 0
            };

            actionDescr.actionNode = this._factory.createActionNode(
                action.name
            );

            actionDescr.actionNode.on(
                'click',
                Core.delegate(function (e) {
                    this.fire('actionNode:click', e, null, actionDescr.id, actionDescr.actionType, actionDescr.activityTemplateId);
                }, this)
            );

            this._canvas.add(actionDescr.actionNode);
            this._actions.push(actionDescr);

            this.relayout();
        },

        // Remove action from specified criteria.
        // Parameters:
        //     actionId - id of action.
        //     isTempId - flag.
        removeAction: function (actionId) {
            for (var i = 0; i < this._actions.length; ++i) {
                if (this._actions[i].id == actionId) {
                    this._canvas.remove(this._actions[i].actionNode);
                    this._actions.splice(i, 1);

                    break;
                }
            }

            this.relayout();
        },

        
        // Rename action with global ID.
        renameAction: function (id, text) {
            for (var i = 0; i < this._actions.length; ++i) {
                if (this._actions[i].id == id) {

                    this._actions[i].actionNode.setText(text);

                    this.relayout();
                    return;
                }
            }
        },

        updateActivityTemplateId: function (id, activityTemplateId) {
            for (var i = 0; i < this._actions.length; ++i) {
                if (this._actions[i].id == id) {
                    this._actions[i].activityTemplateId = activityTemplateId;
                    return;
                }
            }
        },

        // Resize canvas according to actual workflow size.
        _resizeCanvas: function () {
            var addActionNodeBottom = this._getAddActionNodeBottomPoint();
            this._canvas.resize(
                this._canvas.getWidth(),
                addActionNodeBottom + ns.WidgetConsts.canvasPadding
            );
        },

        // Relayout actions panel.
        _relayoutActionsNode: function () {
            this._placeActionsNode();
        },

        // Relayout add action button.
        _relayoutAddActionNode: function () {
            this._placeAddActionNode();
        },

        // Relayout action nodes.
        _relayoutActionNodes: function () {
            var prevAction = null;
            for (i = 0; i < this._actions.length; ++i) {
                this._placeActionNode(this._actions[i], prevAction);
                prevAction = this._actions[i];
            }
        },

        // Relayout whole canvas.
        relayout: function () {
            this._relayoutActionsNode();
            this._relayoutAddActionNode();
            this._relayoutActionNodes();
            this._resizeCanvas();
        },

        // Create predefined objects.
        _predefinedObjects: function () {
            //this._predefineStartNode();
            //this._predefineAddCriteriaNode();
            this._predefineActionsNode();
            this._predefineAddActionNode();
        },

        // ---------- region: ActionsNode routines. ----------

        // Create actions panel.
        _predefineActionsNode: function () {
            var actionsNode = this._factory.createActionsNode();
            this._canvas.add(actionsNode);
            this._actionsNode = actionsNode;
        },

        // Set position of actions panel for specified criteria.
        _placeActionsNode: function () {
            this._actionsNode.setLeft(ns.WidgetConsts.canvasPadding);
            this._actionsNode.setTop(ns.WidgetConsts.canvasPadding);

            var width = ns.WidgetConsts.actionsNodeWidth;
            var height = ns.WidgetConsts.actionsNodeTopHeight
                + ns.WidgetConsts.addActionNodeHeight
                + this._actions.length * ns.WidgetConsts.actionNodeHeight
                + ns.WidgetConsts.actionsNodeBottomHeight;

            this._actionsNode.setWidth(width);
            this._actionsNode.setHeight(height);
            this._actionsNode.relayout();
        },

        // Get actions panel top Y point.
        _getActionsNodeTopPoint: function () {
            return this._actionsNode.getTop();
        },

        // Get actions panel left X point.
        _getActionsNodeLeftPoint: function () {
            return this._actionsNode.getLeft();
        },

        // Create add action button.
        _predefineAddActionNode: function () {
            var addActionNode = this._factory.createAddActionNode();
            addActionNode.on(
                'click',
                Core.delegate(function (e) {
                    this.fire('addActionNode:click', e, 1, ns.ActionType.immediate);
                }, this)
            );

            this._canvas.add(addActionNode);
            this._addActionNode = addActionNode;
        },

        // Set position of add action button.
        _placeAddActionNode: function () {
            var left = this._getActionsNodeLeftPoint()
                + ns.WidgetConsts.addActionNodePadding;

            var top = this._getActionsNodeTopPoint()
                + ns.WidgetConsts.actionsNodeTopHeight
                + ns.WidgetConsts.addActionNodePadding;

            this._addActionNode.setLeft(left);
            this._addActionNode.setTop(top);
            this._addActionNode.relayout();
        },

        // Get bottom Y point of add action button for specified criteria.
        _getAddActionNodeBottomPoint: function () {
            return this._addActionNode.getTop()
                + ns.WidgetConsts.addActionNodeHeight
                - ns.WidgetConsts.addActionNodePadding;
        },

        // Set position of user defined action.
        _placeActionNode: function (action, prevAction) {
            var topOffset;
            if (!prevAction) {
                topOffset = this._addActionNode.getTop();
            }
            else {
                topOffset = this._getActionNodeBottomPoint(prevAction);
            }

            topOffset += ns.WidgetConsts.actionNodePadding;

            action.actionNode.setLeft(this._addActionNode.getLeft());
            action.actionNode.setTop(topOffset);
            action.actionNode.relayout();

            this._addActionNode.setTop(topOffset + action.actionNode.getHeight() + ns.WidgetConsts.actionNodePadding);
            this._addActionNode.relayout();
        },

        // Get bottom Y point of user defined action.
        _getActionNodeBottomPoint: function (action) {
            return action.actionNode.getTop()
                + ns.WidgetConsts.actionNodeHeight
                - ns.WidgetConsts.actionNodePadding;
        }

        // ---------- endregion: ActionsNode routines. ----------
    });

})(Core.ns('PlanBuilder'));