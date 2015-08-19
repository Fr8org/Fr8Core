(function (ns) {

    // ProcessBuilder widget class.
    // Fires events:
    //     'startNode:click' - when StartNode is clicked; function (mouseEvent) { ... }
    //     'addCriteriaNode:click' - when AddCriteriaNode is clicked; function (mouseEvent) { ... }
    //     'criteriaNode:click' - when CriteriaNode (criteria created by user) is clicked; function (mouseEvent, criteriaId) { ... }
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

            this._startNode = null;
            this._addCriteriaNode = null;
            this._addCriteriaArrow = null;
            this._criteria = [];
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

        // Add criteria to ProcessBuilder canvas.
        // Parameters:
        //     criteria - object to define criteria; minimum required set of properties: { id: 'someId' }
        addCriteria: function (criteria) {
            if (!criteria || !criteria.id) {
                throw 'Criteria must contain "id" property.';
            }

            var criteriaDescr = {
                id: criteria.id,
                isTempId: !!criteria.isTempId,
                data: criteria,
                actions: [],
                criteriaNode: null,
                criteriaArrow: null,
                actionsNode: null,
                actionsArrow: null,
                addActionNode: null
            };

            criteriaDescr.criteriaNode = this._factory.createCriteriaNode(
                criteria.name || ('Criteria #' + criteria.id.toString())
            );
            criteriaDescr.criteriaNode.on(
                'click',
                Core.delegate(function (e) {
                    this.fire('criteriaNode:click', e, criteriaDescr.id, criteriaDescr.isTempId);
                }, this)
            );

            criteriaDescr.actionsNode = this._factory.createActionsNode();

            criteriaDescr.addActionNode = this._factory.createAddActionNode();
            criteriaDescr.addActionNode.on(
                'click',
                Core.delegate(function (e) {
                    this.fire('addActionNode:click', e, criteriaDescr.id);
                }, this)
            );


            this._criteria.push(criteriaDescr);

            this._canvas.add(criteriaDescr.criteriaNode);
            this._canvas.add(criteriaDescr.actionsNode);
            this._canvas.add(criteriaDescr.addActionNode);

            this.relayout();
        },

        // Remove criteria from ProcessBuilder canvas.
        // Parameters:
        //     id - criteriaId
        removeCriteria: function (id, isTempId) {
            var i, j;
            for (i = 0; i < this._criteria.length; ++i) {
                if (this._criteria[i].id === id
                    && this._criteria[i].isTempId === isTempId) {

                    this._canvas.remove(this._criteria[i].addActionNode);
                    this._canvas.remove(this._criteria[i].actionsNode);
                    this._canvas.remove(this._criteria[i].criteriaNode);

                    if (this._criteria[i].criteriaArrow) {
                        this._canvas.remove(this._criteria[i].criteriaArrow);
                    }

                    if (this._criteria[i].actionsArrow) {
                        this._canvas.remove(this._criteria[i].actionsArrow);
                    }

                    for (j = 0; j < this._criteria[i].actions.length; ++j) {
                        this._canvas.remove(this._criteria[i].actions[j].actionNode);
                    }

                    this._criteria.splice(i, 1);

                    break;
                }
            }

            this.relayout();
        },

        // Rename criteria with global ID.
        renameCriteria: function (id, text) {
            var i;
            for (i = 0; i < this._criteria.length; ++i) {
                if (this._criteria[i].id == id
                    && !this._criteria[i].isTempId) {

                    this._criteria[i].criteriaNode.setText(text);

                    this.relayout();
                    return;
                }
            }
        },

        // Replace temporary ID with global ID.
        replaceCriteriaTempId: function (tempId, id) {
            var i;
            for (i = 0; i < this._criteria.length; ++i) {
                if (this._criteria[i].id == tempId
                    && this._criteria[i].isTempId) {
                    
                    this._criteria[i].isTempId = false;
                    this._criteria[i].id = id;

                    return;
                }
            }
        },

        // Search criteria by id.
        // Parameters:
        //     criteriaId - criteria id.
        // Returns:
        //     criteria descriptor or null if no criteria found.
        _findCriteria: function (criteriaId) {
            var criteria = null;
            var i;
            for (i = 0; i < this._criteria.length; ++i) {
                if (this._criteria[i].id === criteriaId) {
                    criteria = this._criteria[i];
                    break;
                }
            }

            return criteria;
        },

        // Add action to action list to specified criteria.
        // Parameters:
        //     criteriaId - id of criteria
        //     action - object to define action; minimum required set of properties: { id: 'someId' }
        addAction: function (criteriaId, action) {
            if (!action || !action.id) {
                throw 'Action must contain "id" property.';
            }

            var criteria = this._findCriteria(criteriaId);
            if (!criteria) { throw 'No criteria found with id = ' + criteriaId.toString(); }

            var actionDescr = {
                id: action.id,
                data: action,
                actionNode: null
            };

            
            actionDescr.actionNode = this._factory.createActionNode(
                action.name || ('Action #' + action.id.toString())
            );

            actionDescr.actionNode.on(
                'click',
                Core.delegate(function (e) {
                    this.fire('actionNode:click', e, criteria.id, action.id);
                }, this)
            );


            this._canvas.add(actionDescr.actionNode);
            criteria.actions.push(actionDescr);

            this.relayout();
        },

        // Remove action from specified criteria.
        // Parameters:
        //     criteriaId - id of criteria.
        //     actionId - id of action.
        removeAction: function (criteriaId, actionId) {
            var criteria = this._findCriteria(criteriaId);
            if (!criteria) { throw 'No criteria found with id = ' + criteriaId.toString(); }

            var i;
            for (i = 0; i < criteria.actions.length; ++i) {
                if (criteria.actions[i].id === actionId) {
                    this._canvas.remove(criteria.actions[i].actionNode);
                    criteria.actions.splice(i, 1);

                    break;
                }
            }

            this.relayout();
        },

        // Relayout StartNode.
        _relayoutStartNode: function () {
            this._placeStartNode();
        },

        // Relayout user defined criteria and criteria's action panel.
        _relayoutCriteria: function () {
            var arrowLeft = ns.WidgetConsts.canvasPadding
                + Math.floor(this._getVerticalSectionMaxWidth() / 2);

            var i, prevCriteria;
            var j, prevAction;

            var prevBottomPoint = this._getStartNodeBottomPoint();

            prevCriteria = null;
            for (i = 0; i < this._criteria.length; ++i) {
                this._placeCriteriaNode(this._criteria[i], prevCriteria);
                this._placeActionsNode(this._criteria[i]);
                this._placeAddActionNode(this._criteria[i]);

                prevAction = null;
                for (j = 0; j < this._criteria[i].actions.length; ++j) {
                    this._placeActionNode(this._criteria[i], this._criteria[i].actions[j], prevAction);
                    prevAction = this._criteria[i].actions[j];
                }

                this._criteria[i].actionsArrow = this._replaceRightArrow(
                    this._criteria[i].actionsArrow,
                    this._getCriteriaNodeTopPoint(this._criteria[i])
                        + Math.floor(this._getCriteriaNodeHeight(this._criteria[i]) / 2),
                    this._getCriteriaNodeRightPoint(this._criteria[i]),
                    this._getActionsNodeLeftPoint(this._criteria[i])
                );

                this._criteria[i].criteriaArrow = this._replaceDownArrow(
                    this._criteria[i].criteriaArrow,
                    arrowLeft,
                    prevBottomPoint,
                    this._getCriteriaNodeTopPoint(this._criteria[i])
                );

                prevBottomPoint = this._getCriteriaNodeBottomPoint(this._criteria[i]);
                prevCriteria = this._criteria[i];
            }
        },

        // Relayout button to add new criteria.
        _relayoutAddCriteriaNode: function () {
            var arrowLeft = ns.WidgetConsts.canvasPadding
                + Math.floor(this._getVerticalSectionMaxWidth() / 2);

            var prevBottomPoint;
            if (this._criteria.length > 0) {
                prevBottomPoint = this._getCriteriaNodeBottomPoint(this._criteria[this._criteria.length - 1]);
            }
            else {
                prevBottomPoint = this._getStartNodeBottomPoint();
            }

            this._placeAddCriteriaNode();
            this._addCriteriaArrow = this._replaceDownArrow(
                this._addCriteriaArrow,
                arrowLeft,
                prevBottomPoint,
                this._getAddCriteriaNodeTopPoint()
            );
        },

        // Resize canvas according to actual workflow size.
        _resizeCanvas: function () {
            var addCriteriaBottom = this._getAddCriteriaNodeBottomPoint();
            this._canvas.resize(
                this._canvas.getWidth(),
                addCriteriaBottom + ns.WidgetConsts.canvasPadding
            );
        },

        // Relayout whole canvas.
        relayout: function () {
            this._relayoutStartNode();
            this._relayoutCriteria();
            this._relayoutAddCriteriaNode();
            this._resizeCanvas();
        },

        // Create predefined objects.
        _predefinedObjects: function () {
            this._predefineStartNode();
            this._predefineAddCriteriaNode();
        },

        // Get maximum width among StartNode, AddCriteriaNode, CriteriaNode.
        _getVerticalSectionMaxWidth: function () {
            return Math.max(ns.WidgetConsts.startNodeWidth,
                ns.WidgetConsts.addCriteriaNodeWidth,
                ns.WidgetConsts.criteriaNodeWidth);
        },

        // ---------- region: StartNode routines. ----------

        // Create StartNode.
        _predefineStartNode: function () {
            var startNode = this._factory.createStartNode();
            startNode.on(
                'click',
                Core.delegate(function (e) { this.fire('startNode:click', e); }, this)
            );

            this._canvas.add(startNode);
            this._startNode = startNode;
        },

        // Set position on StartNode.
        _placeStartNode: function () {
            var left = Math.floor((this._getVerticalSectionMaxWidth() - ns.WidgetConsts.startNodeWidth) / 2);

            this._startNode.setLeft(ns.WidgetConsts.canvasPadding + left);
            this._startNode.setTop(ns.WidgetConsts.canvasPadding);

            this._startNode.relayout();
        },

        // Get bottom Y point of StartNode.
        _getStartNodeBottomPoint: function () {
            return this._startNode.getTop()
                + this._startNode.getHeight();
        },

        // ---------- endregion: StartNode routines. ----------


        // ---------- region: AddCriteriaNode routines. ----------

        // Create AddCriteriaNode.
        _predefineAddCriteriaNode: function () {
            var addCriteriaNode = this._factory.createAddCriteriaNode();
            addCriteriaNode.on(
                'click',
                Core.delegate(function (e) { this.fire('addCriteriaNode:click', e); }, this)
            );

            this._canvas.add(addCriteriaNode);
            this._addCriteriaNode = addCriteriaNode;
        },

        // Set position on AddCriteriaNode.
        _placeAddCriteriaNode: function () {
            var topOffset;
            if (this._criteria.length) {
                topOffset = this._getCriteriaSectionBottomPoint(this._criteria[this._criteria.length - 1]);
            }
            else {
                topOffset = this._getStartNodeBottomPoint();
            }

            var left = ns.WidgetConsts.canvasPadding + Math.floor((this._getVerticalSectionMaxWidth() - ns.WidgetConsts.addCriteriaNodeWidth) / 2);
            var top = topOffset + ns.WidgetConsts.minSpaceBetweenObjects;

            this._addCriteriaNode.setLeft(left);
            this._addCriteriaNode.setTop(top);

            this._addCriteriaNode.relayout();
        },

        // Get top Y point of AddCriteriaNode.
        _getAddCriteriaNodeTopPoint: function () {
            return this._addCriteriaNode.getTop();
        },

        // Get bottom Y point of AddCriteriaNode.
        _getAddCriteriaNodeBottomPoint: function () {
            return this._addCriteriaNode.getTop()
                + this._addCriteriaNode.getHeight();
        },

        // ---------- endregion: AddCriteriaNode routines. ----------

        // ---------- region: Arrows routines. ----------

        // Create or replace down-directed arrow.
        _replaceDownArrow: function (arrow, left, from, to) {
            if (arrow !== null) { this._canvas.remove(arrow); }
        
            var length = (to - from) - ns.WidgetConsts.arrowPadding * 2;
            
            var top = from + ns.WidgetConsts.arrowPadding;
            var arrow = this._factory.createDownArrow(left, top, length);
        
            this._canvas.add(arrow);
            return arrow;
        },

        // Create or replace right-directed arrow.
        _replaceRightArrow: function (arrow, top, from, to) {
            if (arrow !== null) { this._canvas.remove(arrow); }

            var length = (to - from) - ns.WidgetConsts.arrowPadding * 2;

            var left = from + ns.WidgetConsts.arrowPadding;
            var arrow = this._factory.createRightArrow(left, top, length);

            this._canvas.add(arrow);
            return arrow;
        },

        // ---------- endregion: Arrows routines. ----------


        // ---------- region: CriteriaNode routines. ----------

        // Set position of user defined criteria node.
        _placeCriteriaNode: function (criteria, prevCriteria) {
            var topOffset;
            if (prevCriteria) {
                topOffset = this._getCriteriaSectionBottomPoint(prevCriteria);
            }
            else {
                topOffset = this._getStartNodeBottomPoint();
            }

            var left = ns.WidgetConsts.canvasPadding + Math.floor((this._getVerticalSectionMaxWidth() - ns.WidgetConsts.criteriaNodeWidth) / 2);
            var top = topOffset + ns.WidgetConsts.minSpaceBetweenObjects;

            criteria.criteriaNode.setLeft(left);
            criteria.criteriaNode.setTop(top);
            criteria.criteriaNode.relayout();
        },

        // Get top Y point of user defined criteria node.
        _getCriteriaNodeTopPoint: function (criteria) {
            return criteria.criteriaNode.getTop();
        },

        // Get bottom Y point of user defined criteria node.
        _getCriteriaNodeBottomPoint: function (criteria) {
            return criteria.criteriaNode.getTop()
                + criteria.criteriaNode.getHeight();
        },

        // Get right X point of user defined criteria node.
        _getCriteriaNodeRightPoint: function (criteria) {
            return criteria.criteriaNode.getLeft()
                + criteria.criteriaNode.getWidth();
        },

        // Get height of user defined criteria node.
        _getCriteriaNodeHeight: function (criteria) {
            return criteria.criteriaNode.getHeight();
        },

        // Get height of user defined criteria node including height of criteria action panel.
        _getCriteriaSectionBottomPoint: function (criteria) {
            var actionsNodeHeight = this._getActionsNodeHeight(criteria);
            var criteriaTopPoint = this._getCriteriaNodeTopPoint(criteria);
            var criteriaBottomPoint = this._getCriteriaNodeBottomPoint(criteria);

            if ((criteriaTopPoint + actionsNodeHeight) > criteriaBottomPoint) {
                return criteriaTopPoint + actionsNodeHeight;
            }
            else {
                return criteriaBottomPoint;
            }
        },

        // ---------- endregion: CriteriaNode routines. ----------


        // ---------- region: ActionsNode routines. ----------

        // Set position of actions panel for specified criteria.
        _placeActionsNode: function (criteria) {
            var left = this._getVerticalSectionMaxWidth()
                + ns.WidgetConsts.minSpaceBetweenObjects;

            var top = this._getCriteriaNodeTopPoint(criteria);

            var width = ns.WidgetConsts.actionsNodeWidth;
            var height = ns.WidgetConsts.actionsNodeTopHeight
                + ns.WidgetConsts.addActionNodeHeight
                + criteria.actions.length * ns.WidgetConsts.actionNodeHeight
                + ns.WidgetConsts.actionsNodeBottomHeight;

            var criteriaHeight = this._getCriteriaNodeHeight(criteria);
            if (height < criteriaHeight) {
                top += Math.floor((criteriaHeight - height) / 2);
            }

            criteria.actionsNode.setLeft(left);
            criteria.actionsNode.setTop(top);
            criteria.actionsNode.setWidth(width);
            criteria.actionsNode.setHeight(height);
            criteria.actionsNode.relayout();
        },

        // Get actions panel top Y point.
        _getActionsNodeTopPoint: function (criteria) {
            return criteria.actionsNode.getTop();
        },

        // Get actions panel left X point.
        _getActionsNodeLeftPoint: function (criteria) {
            return criteria.actionsNode.getLeft();
        },

        // Get height of actions panel.
        _getActionsNodeHeight: function (criteria) {
            return criteria.actionsNode.getHeight();
        },

        // Set position of add action button for specified criteria.
        _placeAddActionNode: function (criteria) {
            var left = this._getActionsNodeLeftPoint(criteria)
                + ns.WidgetConsts.addActionNodePadding;

            var top = this._getActionsNodeTopPoint(criteria)
                + ns.WidgetConsts.actionsNodeTopHeight
                + ns.WidgetConsts.addActionNodePadding;

            criteria.addActionNode.setLeft(left);
            criteria.addActionNode.setTop(top);
            criteria.addActionNode.relayout();
        },

        // Get bottom Y point of add action button for specified criteria.
        _getAddActionNodeBottomPoint: function (criteria) {
            return criteria.addActionNode.getTop()
                + ns.WidgetConsts.addActionNodeHeight
                - ns.WidgetConsts.addActionNodePadding;
        },

        // Set position of user defined action.
        _placeActionNode: function (criteria, action, prevAction) {
            var topOffset;
            if (!prevAction) {
                topOffset = this._getAddActionNodeBottomPoint(criteria);
            }
            else {
                topOffset = this._getActionNodeBottomPoint(prevAction);
            }

            topOffset += ns.WidgetConsts.actionNodePadding;

            action.actionNode.setLeft(criteria.addActionNode.getLeft());
            action.actionNode.setTop(topOffset);
            action.actionNode.relayout();
        },

        // Get bottom Y point of user defined action.
        _getActionNodeBottomPoint: function (action) {
            return action.actionNode.getTop()
                + ns.WidgetConsts.actionNodeHeight
                - ns.WidgetConsts.actionNodePadding;
        }

        // ---------- endregion: ActionsNode routines. ----------
    });

})(Core.ns('ProcessBuilder'));