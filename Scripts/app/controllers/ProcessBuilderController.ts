/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    //Setup aliases
    import pwd = dockyard.directives.paneWorkflowDesigner;
    import psa = dockyard.directives.paneSelectAction;
    import pca = dockyard.directives.paneConfigureAction;
    import pst = dockyard.directives.paneSelectTemplate;

    class ProcessBuilderController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di


        public static $inject = [
            '$rootScope',
            '$scope',
            'StringService'
        ];

        private _scope: interfaces.IProcessBuilderScope;

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: interfaces.IProcessBuilderScope,
            private StringService: services.IStringService) {

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
                if (!$scope.selectedCriteria) { return; }

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
        private setupMessageProcessing() {

            //Process Designer Pane events
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_CriteriaSelected],
                (event: ng.IAngularEvent, eventArgs: pwd.CriteriaSelectedEventArgs) => this.PaneWorkflowDesigner_CriteriaSelected(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_ActionSelected],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionSelectedEventArgs) => this.PaneWorkflowDesigner_ActionSelected(eventArgs));
            this._scope.$on(pwd.MessageType[pwd.MessageType.PaneWorkflowDesigner_TemplateSelected],
                (event: ng.IAngularEvent, eventArgs: pwd.ActionSelectedEventArgs) => this.PaneWorkflowDesigner_TemplateSelected(eventArgs));

            //Process Configure Action Pane events
            this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_Cancelled],
                (event: ng.IAngularEvent, eventArgs: pca.CancelledEventArgs) => this.PaneConfigureAction_Cancelled(eventArgs));
            this._scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ActionUpdated],
                (event: ng.IAngularEvent, eventArgs: pca.ActionUpdatedEventArgs) => this.PaneConfigureAction_ActionUpdated(eventArgs));
        }

        /*
            Handles message 'WorkflowDesignerPane_CriteriaSelected'
        */
        private PaneWorkflowDesigner_CriteriaSelected(eventArgs: pwd.CriteriaSelectedEventArgs) {
            console.log("ProcessBuilderController: criteria selected");

            //Hide Select Action Pane
            this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
                
            //Hide Configure Action Pane
            this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Hide]);
        }

        /*
            Handles message 'WorkflowDesignerPane_ActionSelected'
        */
        private PaneWorkflowDesigner_ActionSelected(eventArgs: pwd.ActionSelectedEventArgs) { 
            console.log("ProcessBuilderController: action selected");

            //Render Select Action Pane
            var eArgs = new psa.RenderEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId,
                eventArgs.isTempId,
                eventArgs.processTemplateId);
            this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Render], eArgs);

            //Render Configure Action Pane
            var eArgs = new psa.RenderEventArgs(
                eventArgs.criteriaId,
                eventArgs.actionId,
                eventArgs.isTempId,
                eventArgs.processTemplateId);
            this._scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], eArgs);
        }

        /*
            Handles message 'WorkflowDesignerPane_TemplateSelected'
        */
        private PaneWorkflowDesigner_TemplateSelected(eventArgs: pwd.TemplateSelectedEventArgs) {
            console.log("ProcessBuilderController: template selected");

            //Show Select Template Pane
            var eArgs = new directives.paneSelectTemplate.RenderEventArgs(eventArgs.processTemplateId);
            this._scope.$broadcast(pst.MessageType[pst.MessageType.PaneSelectTemplate_Render]);       
        }


        /*
            Handles message 'ConfigureActionPane_ActionUpdated'
        */
        private PaneConfigureAction_ActionUpdated(eventArgs: pca.ActionUpdatedEventArgs) {

            //Force update on Select Action Pane 
            var eArgs = new directives.paneSelectAction.UpdateActionEventArgs(
                eventArgs.criteriaId, eventArgs.actionId, eventArgs.actionTempId, 0);
            this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_UpdateAction], eArgs);

            //Update Action on Designer
            eArgs = new directives.paneWorkflowDesigner.UpdateActionEventArgs(
                eventArgs.criteriaId, eventArgs.actionId, eventArgs.actionTempId, 0);
            this._scope.$broadcast(psa.MessageType[pwd.MessageType.PaneWorkflowDesigner_UpdateAction], eArgs);
        }

        /*
            Handles message 'ConfigureActionPane_Cancelled'
        */
        private PaneConfigureAction_Cancelled(eventArgs: pca.CancelledEventArgs) {
            //Hide Select Action Pane
            this._scope.$broadcast(psa.MessageType[psa.MessageType.PaneSelectAction_Hide]);
        }
    }
    app.controller('ProcessBuilderController', ProcessBuilderController);
} 