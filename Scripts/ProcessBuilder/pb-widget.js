(function (ns) {

    ns.Widget = Core.class(Core.CoreObject, {
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

        init: function () {
            var canvas = $('<canvas width="' + this._pxWidth.toString() + '" height="' + this._pxHeight.toString() + '"></canvas>');

            this._jqEl.append(canvas);

            this._canvas = this._factory.createCanvas(canvas[0]);

            this._predefinedObjects();
            this.relayout();
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
                actionsArrow: null,
                addActionNode: null
            };

            criteriaDescr.criteriaNode = this._factory.createCriteriaNode(
                criteria.name || ('Criteria #' + criteria.id.toString())
            );
            criteriaDescr.criteriaNode.on(
                'click',
                Core.delegate(function (e) {
                    this.fire('criteriaNode:click', e, criteria.id);
                }, this)
            );

            criteriaDescr.actionsNode = this._factory.createActionsNode();

            criteriaDescr.addActionNode = this._factory.createAddActionNode();
            criteriaDescr.addActionNode.on(
                'click',
                Core.delegate(function (e) {
                    this.fire('addActionNode:click', e, criteria.id);
                }, this)
            );


            this._criteria.push(criteriaDescr);

            this._canvas.add(criteriaDescr.criteriaNode);
            this._canvas.add(criteriaDescr.actionsNode);
            this._canvas.add(criteriaDescr.addActionNode);

            this.relayout();
        },

        removeCriteria: function (id) {
            var i, j;
            for (i = 0; i < this._criteria.length; ++i) {
                if (this._criteria[i].id === id) {
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

        _relayoutStartNode: function () {
            this._placeStartNode();
        },

        _relayoutCriteria: function () {
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

                this._criteria[i].actionsArrow = this._replaceArrow(
                    this._criteria[i].actionsArrow,
                    ns.WidgetConsts.rightMode,
                    this._getCriteriaNodeTopPoint(this._criteria[i])
                        + Math.floor(this._getCriteriaNodeHeight(this._criteria[i]) / 2)
                        - ns.WidgetConsts.arrowSize,
                    ns.WidgetConsts.canvasPadding + ns.WidgetConsts.defaultSize,
                    ns.WidgetConsts.canvasPadding + ns.WidgetConsts.defaultSize + ns.WidgetConsts.minSpaceBetweenObjects
                );

                this._criteria[i].criteriaArrow = this._replaceArrow(
                    this._criteria[i].criteriaArrow,
                    ns.WidgetConsts.downMode,
                    ns.WidgetConsts.canvasPadding
                        + Math.floor(ns.WidgetConsts.defaultSize / 2)
                        - ns.WidgetConsts.arrowSize,
                    prevBottomPoint,
                    this._getCriteriaNodeTopPoint(this._criteria[i])
                );

                prevBottomPoint = this._getCriteriaNodeBottomPoint(this._criteria[i]);
                prevCriteria = this._criteria[i];
            }
        },

        _relayoutAddCriteriaNode: function () {
            var prevBottomPoint;
            if (this._criteria.length > 0) {
                prevBottomPoint = this._getCriteriaNodeBottomPoint(this._criteria[this._criteria.length - 1]);
            }
            else {
                prevBottomPoint = this._getStartNodeBottomPoint();
            }

            this._placeAddCriteriaNode();
            this._addCriteriaArrow = this._replaceArrow(
                this._addCriteriaArrow,
                ns.WidgetConsts.downMode,
                ns.WidgetConsts.canvasPadding + Math.floor(ns.WidgetConsts.defaultSize / 2) - ns.WidgetConsts.arrowSize,
                prevBottomPoint,
                this._getAddCriteriaNodeTopPoint()
            );
        },

        _resizeCanvas: function () {
            var addCriteriaBottom = this._getAddCriteriaNodeBottomPoint();
            this._canvas.resize(this._canvas.getWidth(), addCriteriaBottom);
        },

        relayout: function () {
            this._relayoutStartNode();
            this._relayoutCriteria();
            this._relayoutAddCriteriaNode();
            this._resizeCanvas();
        },

        _predefinedObjects: function () {
            this._predefineStartNode();
            this._predefineAddCriteriaNode();
        },

        // ---------- region: StartNode routines. ----------

        _predefineStartNode: function () {
            var startNode = this._factory.createStartNode();
            startNode.on(
                'click',
                Core.delegate(function (e) { this.fire('startNode:click', e); }, this)
            );

            this._canvas.add(startNode);
            this._startNode = startNode;
        },

        _placeStartNode: function () {
            this._startNode.setLeft(ns.WidgetConsts.canvasPadding);
            this._startNode.setTop(ns.WidgetConsts.canvasPadding);

            this._startNode.relayout();
        },

        _getStartNodeBottomPoint: function () {
            return this._startNode.getTop()
                + this._startNode.getHeight();
        },

        // ---------- endregion: StartNode routines. ----------


        // ---------- region: AddCriteriaNode routines. ----------

        _predefineAddCriteriaNode: function () {
            var addCriteriaNode = this._factory.createAddCriteriaNode();
            addCriteriaNode.on(
                'click',
                Core.delegate(function (e) { this.fire('addCriteriaNode:click', e); }, this)
            );

            this._canvas.add(addCriteriaNode);
            this._addCriteriaNode = addCriteriaNode;
        },

        _placeAddCriteriaNode: function () {
            var topOffset;
            if (this._criteria.length) {
                topOffset = this._getCriteriaSectionBottomPoint(this._criteria[this._criteria.length - 1]);
            }
            else {
                topOffset = this._getStartNodeBottomPoint();
            }

            var left = ns.WidgetConsts.canvasPadding;
            var top = topOffset + ns.WidgetConsts.minSpaceBetweenObjects;

            this._addCriteriaNode.setLeft(left);
            this._addCriteriaNode.setTop(top);

            this._addCriteriaNode.relayout();
        },

        _getAddCriteriaNodeTopPoint: function () {
            return this._addCriteriaNode.getTop();
        },

        _getAddCriteriaNodeBottomPoint: function () {
            return this._addCriteriaNode.getTop()
                + this._addCriteriaNode.getHeight();
        },

        // ---------- endregion: AddCriteriaNode routines. ----------

        // ---------- region: Arrows routines. ----------

        _replaceArrow: function (arrow, mode, pos, from, to) {
            if (arrow !== null) {
                this._canvas.remove(arrow);
            }
        
            var length = (to - from) - ns.WidgetConsts.arrowPadding * 2;
        
            var left, top, arrow;
            if (mode === ns.WidgetConsts.downMode) {
                left = pos;
                top = from + ns.WidgetConsts.arrowPadding;

                arrow = this._factory.createDownArrow(left, top, length);
            }
            else {
                left = from + ns.WidgetConsts.arrowPadding;
                top = pos;

                arrow = this._factory.createRightArrow(left, top, length);
            }
        
            this._canvas.add(arrow);
            return arrow;
        },

        // ---------- endregion: Arrows routines. ----------


        // ---------- region: CriteriaNode routines. ----------

        _placeCriteriaNode: function (criteria, prevCriteria) {
            var topOffset;
            if (prevCriteria) {
                topOffset = this._getCriteriaSectionBottomPoint(prevCriteria);
            }
            else {
                topOffset = this._getStartNodeBottomPoint();
            }

            var left = ns.WidgetConsts.canvasPadding;
            var top = topOffset + ns.WidgetConsts.minSpaceBetweenObjects;

            criteria.criteriaNode.setLeft(left);
            criteria.criteriaNode.setTop(top);
            criteria.criteriaNode.relayout();
        },

        _getCriteriaNodeTopPoint: function (criteria) {
            return criteria.criteriaNode.getTop();
        },

        _getCriteriaNodeBottomPoint: function (criteria) {
            return criteria.criteriaNode.getTop()
                + criteria.criteriaNode.getHeight();
        },

        _getCriteriaNodeHeight: function (criteria) {
            return criteria.criteriaNode.getHeight();
        },

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

        _placeActionsNode: function (criteria) {
            var left = ns.WidgetConsts.canvasPadding
                + ns.WidgetConsts.defaultSize
                + ns.WidgetConsts.minSpaceBetweenObjects;

            var top = this._getCriteriaNodeTopPoint(criteria);

            var width = ns.WidgetConsts.defaultSize;
            var height = ns.WidgetConsts.addActionNodeHeight
                + criteria.actions.length * ns.WidgetConsts.actionNodeHeight;

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

        _getActionsNodeTopPoint: function (criteria) {
            return criteria.actionsNode.getTop();
        },

        _getActionsNodeHeight: function (criteria) {
            return criteria.actionsNode.getHeight();
        },

        _placeAddActionNode: function (criteria) {
            var left = ns.WidgetConsts.canvasPadding
                + ns.WidgetConsts.defaultSize
                + ns.WidgetConsts.minSpaceBetweenObjects
                + ns.WidgetConsts.addActionNodePadding;

            var top = this._getActionsNodeTopPoint(criteria)
                + ns.WidgetConsts.addActionNodePadding;

            criteria.addActionNode.setLeft(left);
            criteria.addActionNode.setTop(top);
            criteria.addActionNode.relayout();
        },

        _getAddActionNodeBottomPoint: function (criteria) {
            return criteria.addActionNode.getTop()
                + ns.WidgetConsts.addActionNodeHeight
                - ns.WidgetConsts.addActionNodePadding;
        },

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

        _getActionNodeBottomPoint: function (action) {
            return action.actionNode.getTop()
                + ns.WidgetConsts.actionNodeHeight
                - ns.WidgetConsts.actionNodePadding;
        }

        // ---------- endregion: ActionsNode routines. ----------
    });

})(Core.ns('ProcessBuilder'));