/// <reference path="../_all.ts" />
/*
'use strict';

app.directive('processBuilder', function () {
    var linkFn = function (scope, element, attrs) {
        var factory = new ProcessBuilder.FabricJsFactory();
        var widget = Core.create(ProcessBuilder.Widget,
            element, factory, attrs.width, attrs.height);

        widget.on('addCriteriaNode:click', function () {
            scope.$apply(function () {
                scope.addCriteria();
            });
        });

        widget.on('criteriaNode:click', function (e, criteriaId) {
            scope.$apply(function () {
                scope.selectCriteria({ criteriaId: criteriaId });
            });
        });

        widget.on('addActionNode:click', function (e, criteriaId) {
            scope.$apply(function () {
                scope.addAction({ criteriaId: criteriaId });
            });
        });

        widget.on('actionNode:click', function (e, criteriaId, actionId) {
            scope.$apply(function () {
                scope.selectAction({ criteriaId: criteriaId, actionId: actionId });
            });
        });

        var collectMissing = function (original, missing) {
            var result = [];
            var i, j, found;

            for (i = 0; i < original.length; ++i) {
                found = false;
                for (j = 0; j < missing.length; ++j) {
                    if (missing[j].id === original[i].id) {
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    result.push(original[i]);
                }
            }

            return result;
        };

        var forEachSameCriteria = function (first, second, callback) {
            var i, j;

            for (i = 0; i < first.length; ++i) {
                for (j = 0; j < second.length; ++j) {
                    if (first[i].id === second[j].id) {
                        callback(first[i], second[i]);
                        break;
                    }
                }
            }
        };

        scope.$watch('criteriaList',
            function (newList, oldList) {
                newList = newList || [];
                oldList = oldList || [];

                // Collecting added criteria.
                var addedCriteria = collectMissing(newList, oldList);
                addedCriteria.forEach(function (it) {
                    widget.addCriteria(it);
                });

                // Collecting removed criteria.
                var removedCriteria = collectMissing(oldList, newList);
                removedCriteria.forEach(function (it) {
                    widget.removeCriteria(it.id);
                });

                // Add or remove actions.
                forEachSameCriteria(newList, oldList,
                    function (newCriteria, oldCriteria) {
                        var newActions = newCriteria.actions || [];
                        var oldActions = oldCriteria.actions || [];

                        var addedActions = collectMissing(newActions, oldActions);
                        addedActions.forEach(function (it) {
                            widget.addAction(newCriteria.id, it);
                        });

                        var removedActions = collectMissing(oldActions, newActions);
                        removedActions.forEach(function (it) {
                            widget.removeAction(newCriteria.id, it.id);
                        });
                    });
            },
            true
            );
    };

    return {
        restrict: 'E',
        template: '<div></div>',
        link: linkFn,
        scope: {
            'criteriaList': '=',
            'addCriteria': '&onAddCriteria',
            'selectCriteria': '&onSelectCriteria',
            'addAction': '&onAddAction',
            'selectAction': '&onSelectAction'
        }
    };
});
*/
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var paneWorkflowDesigner;
        (function (paneWorkflowDesigner) {
            function PaneWorkflowDesigner() {
                return {
                    restrict: 'E',
                    template: '<div style="overflow: auto;"></div>',
                    scope: {
                        'criteriaList': '=',
                        'addCriteria': '&onAddCriteria',
                        'selectCriteria': '&onSelectCriteria',
                        'addAction': '&onAddAction',
                        'selectAction': '&onSelectAction'
                    },
                    link: function (scope, element, attrs) {
                        var factory = new ProcessBuilder.FabricJsFactory();
                        var widget = Core.create(ProcessBuilder.Widget, element.children()[0], factory, attrs.width, attrs.height);
                        widget.on('addCriteriaNode:click', function () {
                            scope.$apply(function () {
                                //If a new criteria becomes selected by default, 
                                //please create and send the WorkflowDesignerPane_CriteriaSelected message
                                scope.addCriteria();
                                var eventArgs = new paneWorkflowDesigner.CriteriaSelectedEventArgs();
                                //If a criteria is newly added, create an unique ID within the current User scope. 
                                //Otherwise, please use the permanent database-generated id. 
                                eventArgs.criteriaId = 1; //dummy value
                                //if a criteria is newly added but not saved to the DB, then true. Otherwise false.
                                eventArgs.isTempId = true;
                                //Send event message
                                console.log("sending event");
                                scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_CriteriaSelected], eventArgs);
                            });
                        });
                        widget.on('criteriaNode:click', function (e, criteriaId) {
                            scope.$apply(function () {
                                scope.selectCriteria({ criteriaId: criteriaId });
                                var eventArgs = new paneWorkflowDesigner.CriteriaSelectedEventArgs();
                                //If a criteria is newly added, create an unique ID within the current User scope. 
                                //Otherwise, please use the permanent database-generated id. 
                                eventArgs.criteriaId = criteriaId;
                                //if a criteria is newly added but not saved to the DB, then true. Otherwise false.
                                eventArgs.isTempId = true;
                                //Send event message
                                console.log("sending event");
                                scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_CriteriaSelected], eventArgs);
                            });
                        });
                        widget.on('addActionNode:click', function (e, criteriaId) {
                            scope.$apply(function () {
                                scope.addAction({ criteriaId: criteriaId });
                                //If a new action becomes selected by default, 
                                //need to create and send the WorkflowDesignerPane_ActionSelected message
                                var eventArgs = new paneWorkflowDesigner.ActionSelectedEventArgs();
                                //If a criteria is newly added, create an unique ID within the current User scope. 
                                //Otherwise, please use the permanent database-generated id. 
                                eventArgs.criteriaId = 1; // dummy value
                                //if a criteria is newly added but not saved to the DB, then true. Otherwise false.
                                eventArgs.isTempId = true;
                                //Send event message
                                console.log("sending event");
                                scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionSelected], eventArgs);
                            });
                        });
                        widget.on('actionNode:click', function (e, criteriaId, actionId) {
                            scope.$apply(function () {
                                scope.selectAction({ criteriaId: criteriaId, actionId: actionId });
                                var eventArgs = new paneWorkflowDesigner.ActionSelectedEventArgs();
                                //If a criteria is newly added, create an unique ID within the current User scope. 
                                //Otherwise, please use the permanent database-generated id. 
                                eventArgs.criteriaId = criteriaId;
                                //if a criteria is newly added but not saved to the DB, then true. Otherwise false.
                                eventArgs.isTempId = true;
                                //Send event message
                                console.log("sending event");
                                scope.$emit(paneWorkflowDesigner.MessageType[paneWorkflowDesigner.MessageType.PaneWorkflowDesigner_ActionSelected], eventArgs);
                            });
                        });
                        var collectMissing = function (original, missing) {
                            var result = [];
                            var i, j, found;
                            for (i = 0; i < original.length; ++i) {
                                found = false;
                                for (j = 0; j < missing.length; ++j) {
                                    if (missing[j].id === original[i].id) {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found) {
                                    result.push(original[i]);
                                }
                            }
                            return result;
                        };
                        var forEachSameCriteria = function (first, second, callback) {
                            var i, j;
                            for (i = 0; i < first.length; ++i) {
                                for (j = 0; j < second.length; ++j) {
                                    if (first[i].id === second[j].id) {
                                        callback(first[i], second[i]);
                                        break;
                                    }
                                }
                            }
                        };
                        scope.$watch('criteriaList', function (newList, oldList) {
                            newList = newList || [];
                            oldList = oldList || [];
                            // Collecting added criteria.
                            var addedCriteria = collectMissing(newList, oldList);
                            addedCriteria.forEach(function (it) {
                                widget.addCriteria(it);
                            });
                            // Collecting removed criteria.
                            var removedCriteria = collectMissing(oldList, newList);
                            removedCriteria.forEach(function (it) {
                                widget.removeCriteria(it.id);
                            });
                            // Add or remove actions.
                            forEachSameCriteria(newList, oldList, function (newCriteria, oldCriteria) {
                                var newActions = newCriteria.actions || [];
                                var oldActions = oldCriteria.actions || [];
                                var addedActions = collectMissing(newActions, oldActions);
                                addedActions.forEach(function (it) {
                                    widget.addAction(newCriteria.id, it);
                                });
                                var removedActions = collectMissing(oldActions, newActions);
                                removedActions.forEach(function (it) {
                                    widget.removeAction(newCriteria.id, it.id);
                                });
                            });
                        }, true);
                    }
                };
            }
            paneWorkflowDesigner.PaneWorkflowDesigner = PaneWorkflowDesigner;
        })(paneWorkflowDesigner = directives.paneWorkflowDesigner || (directives.paneWorkflowDesigner = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
app.directive('paneWorkflowDesigner', dockyard.directives.paneWorkflowDesigner.PaneWorkflowDesigner);
//# sourceMappingURL=PaneWorkflowDesigner.js.map