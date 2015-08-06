/// <reference path="../_all.ts" />
/*
    Detail (view/add/edit) controller
*/
var dockyard;
(function (dockyard) {
    var controllers;
    (function (controllers) {
        'use strict';
        //Setup aliases
        var pwd = dockyard.directives.paneWorkflowDesigner;
        var psa = dockyard.directives.paneSelectAction;
        var pca = dockyard.directives.paneConfigureAction;
        var pst = dockyard.directives.paneSelectTemplate;
        var ProcessBuilderController = (function () {
            function ProcessBuilderController($rootScope, $scope, StringService) {
                this.$rootScope = $rootScope;
                this.$scope = $scope;
                this.StringService = StringService;
                this._scope = $scope;
                this.setupMessageProcessing();
                // BEGIN ProcessBuilder event handlers.
                var criteriaIdSeq = 0;
                var actionIdSeq = 0;
                $scope.criteria = [];
                $scope.fields = [
                    { key: 'envelope.name', name: '[Envelope].Name' },
                    { key: 'envelope.date', name: '[Envelope].Date' }
                ];
                $scope.selectedCriteria = null;
                $scope.selectedAction = null;
                $scope.isCriteriaSelected = function () {
                    return $scope.selectedCriteria !== null;
                };
                $scope.isActionSelected = function () {
                    return $scope.selectedAction !== null;
                };
                $scope.addCriteria = function () {
                    var id = ++criteriaIdSeq;
                    var criteria = {
                        id: id,
                        isTempId: false,
                        name: 'New criteria #' + id.toString(),
                        actions: [],
                        conditions: [
                            {
                                field: 'envelope.name',
                                operator: 'gt',
                                value: '',
                                valueError: true
                            }
                        ],
                        executionMode: 'conditions'
                    };
                    $scope.criteria.push(criteria);
                    $scope.selectedCriteria = criteria;
                    $scope.selectedAction = null;
                };
                $scope.selectCriteria = function (criteriaId) {
                    $scope.selectedAction = null;
                    var i;
                    for (i = 0; i < $scope.criteria.length; ++i) {
                        if ($scope.criteria[i].id === criteriaId) {
                            $scope.selectedCriteria = $scope.criteria[i];
                            break;
                        }
                    }
                };
                $scope.removeCriteria = function () {
                    if (!$scope.selectedCriteria) {
                        return;
                    }
                    var i;
                    for (i = 0; i < $scope.criteria.length; ++i) {
                        if ($scope.criteria[i].id === $scope.selectedCriteria.id) {
                            $scope.criteria.splice(i, 1);
                            $scope.selectedCriteria = null;
                            break;
                        }
                    }
                };
                $scope.addAction = function (criteriaId) {
                    var id = ++actionIdSeq;
                    var i;
                    var action;
                    for (i = 0; i < $scope.criteria.length; ++i) {
                        if ($scope.criteria[i].id === criteriaId) {
                            action = {
                                id: id,
                                name: 'Action #' + id.toString()
                            };
                            $scope.criteria[i].actions.push(action);
                            $scope.selectedCriteria = null;
                            $scope.selectedAction = action;
                            break;
                        }
                    }
                };
                $scope.selectAction = function (criteriaId, actionId) {
                    $scope.selectedCriteria = null;
                    var i, j;
                    for (i = 0; i < $scope.criteria.length; ++i) {
                        if ($scope.criteria[i].id === criteriaId) {
                            for (j = 0; j < $scope.criteria[i].actions.length; ++j) {
                                if ($scope.criteria[i].actions[j].id === actionId) {
                                    $scope.selectedAction = $scope.criteria[i].actions[j];
                                    break;
                                }
                            }
                            break;
                        }
                    }
                };
                $scope.removeAction = function (criteriaId, actionId) {
                    var i, j;
                    for (i = 0; i < $scope.criteria.length; ++i) {
                        if ($scope.criteria[i].id === criteriaId) {
                            for (j = 0; j < $scope.criteria[i].actions.length; ++j) {
                                if ($scope.criteria[i].actions[j].id === actionId) {
                                    $scope.criteria[i].actions.splice(j, 1);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                };
                // END CriteriaPane & ProcessBuilder routines.
                // END ProcessBuilder event handlers.
            }
            /*
                Mapping of incoming messages to handlers
            */
            ProcessBuilderController.prototype.setupMessageProcessing = function () {
                var _this = this;
                //Process Designer Pane events
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaSelected], function (event, eventArgs) { return _this.PaneWorkflowDesigner_CriteriaSelected(eventArgs); });
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelected], function (event, eventArgs) { return _this.PaneWorkflowDesigner_ActionSelected(eventArgs); });
                this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected], function (event, eventArgs) { return _this.PaneWorkflowDesigner_TemplateSelected(eventArgs); });
                //Process Configure Action Pane events
                this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_Cancelled], function (event, eventArgs) { return _this.PaneConfigureAction_Cancelled(eventArgs); });
                this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionUpdated], function (event, eventArgs) { return _this.PaneConfigureAction_ActionUpdated(eventArgs); });
                //Process Select Action Pane events
                this._scope.$on(psa.MessageType[psa.MessageType.PaneSelectAction_ActionTypeSelected], function (event, eventArgs) { return _this.PaneSelectAction_ActionTypeSelected(eventArgs); });
            };
            /*
                Handles message 'WorkflowDesignerPane_CriteriaSelected'
            */
            ProcessBuilderController.prototype.PaneWorkflowDesigner_CriteriaSelected = function (eventArgs) {
                console.log("ProcessBuilderController: criteria selected");
                //Hide Select Action Pane
                this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
                //Hide Configure Action Pane
                this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
            };
            /*
                Handles message 'WorkflowDesignerPane_ActionSelected'
            */
            ProcessBuilderController.prototype.PaneWorkflowDesigner_ActionSelected = function (eventArgs) {
                console.log("ProcessBuilderController: action selected");
                //Render Select Action Pane
                var eArgs = new psa.RenderEventArgs(eventArgs.criteriaId, eventArgs.actionId, eventArgs.isTempId, eventArgs.processTemplateId);
                this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Render], eArgs);
            };
            /*
                Handles message 'WorkflowDesignerPane_TemplateSelected'
            */
            ProcessBuilderController.prototype.PaneWorkflowDesigner_TemplateSelected = function (eventArgs) {
                console.log("ProcessBuilderController: template selected");
                //Show Select Template Pane
                var eArgs = new dockyard.directives.paneSelectTemplate.RenderEventArgs(eventArgs.processTemplateId);
                this._scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Render]);
            };
            /*
                Handles message 'ConfigureActionPane_ActionUpdated'
            */
            ProcessBuilderController.prototype.PaneConfigureAction_ActionUpdated = function (eventArgs) {
                //Force update on Select Action Pane 
                var eArgs = new dockyard.directives.paneSelectAction.UpdateActionEventArgs(eventArgs.criteriaId, eventArgs.actionId, eventArgs.actionTempId, 0);
                this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_UpdateAction], eArgs);
                //Update Action on Designer
                eArgs = new dockyard.directives.paneWorkflowDesigner.UpdateActionEventArgs(eventArgs.criteriaId, eventArgs.actionId, eventArgs.actionTempId, 0);
                this._scope.$broadcast(psa.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateAction], eArgs);
            };
            /*
                Handles message 'ConfigureActionPane_Cancelled'
            */
            ProcessBuilderController.prototype.PaneConfigureAction_Cancelled = function (eventArgs) {
                //Hide Select Action Pane
                this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
            };
            /*
                Handles message 'SelectActionPane_ActionTypeSelected'
            */
            ProcessBuilderController.prototype.PaneSelectAction_ActionTypeSelected = function (eventArgs) {
                console.log("action type selected");
                //Render Configure Action Pane
                var eArgs = new psa.RenderEventArgs(eventArgs.criteriaId, eventArgs.actionId > 0 ? eventArgs.actionId : eventArgs.tempActionId, eventArgs.actionId < 0, eventArgs.processTemplateId);
                this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], eArgs);
            };
            // $inject annotation.
            // It provides $injector with information about dependencies to be injected into constructor
            // it is better to have it close to the constructor, because the parameters must match in count and type.
            // See http://docs.angularjs.org/guide/di
            ProcessBuilderController.$inject = [
                '$rootScope',
                '$scope',
                'StringService'
            ];
            return ProcessBuilderController;
        })();
        app.controller('ProcessBuilderController', ProcessBuilderController);
    })(controllers = dockyard.controllers || (dockyard.controllers = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ProcessBuilderController.js.map