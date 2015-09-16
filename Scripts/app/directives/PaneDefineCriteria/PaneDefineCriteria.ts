/// <reference path="../../_all.ts" />

module dockyard.directives.paneDefineCriteria {
    'use strict';

    export function PaneDefineCriteria(): ng.IDirective {
        var disposeActionListener: Function; // a function to deregister currentAction watch upon Hide()

        // Get url for ProcessNodeTemplate create, update, delete operations.
        var getProcessNodeTemplateUrl = function (urlPrefix) {
            return urlPrefix + '/processNodeTemplate';
        };

        // Get url for fetching single ProcessNodeTemplate entity.
        var getSingleProcessNodeTemplateUrl = function (urlPrefix, id) {
            return urlPrefix + '/processNodeTemplate/' + id;
        };

        // Get url for fetching criteria by ProcessNodeTemplate.Id.
        var getCriteriaIdUrl = function (urlPrefix, pntId) {
            return urlPrefix + '/criteria/byProcessNodeTemplate?id=' + pntId;
        };

        // Get url for criteria update operation.
        var getCriteriaUrl = function (urlPrefix) {
            return urlPrefix + '/criteria';
        };

        // Prepare pane scope for properly displaying data.
        var afterLoaded = function (scope: IPaneDefineCriteriaScope, eventArgs: RenderEventArgs) {
            scope.fields = eventArgs.fields;

            // Create conditions array, if it does not exists in criteria object.
            if (!scope.processNodeTemplate.criteria.conditions) {
                scope.processNodeTemplate.criteria.conditions = [];
            }

            // Create default condition line, if conditions array is empty.
            if (scope.processNodeTemplate.criteria.conditions.length === 0) {
                var fieldKey = '';
                if (scope.fields && scope.fields.length > 0) {
                    fieldKey = scope.fields[0].key;
                }

                var condition = new model.Condition(fieldKey, 'gt', '');
                condition.validate();

                scope.processNodeTemplate.criteria.conditions.push(condition);
            }
        };


        // Load existing ProcessNodeTemplate and Criteria objects from REST api.
        var loadProcessNodeTemplate = function (http: ng.IHttpService, urlPrefix: string, id, callback) {
            // Get ProcessNodeTemplate DTO from REST api.
            http.get(getSingleProcessNodeTemplateUrl(urlPrefix, id))
                .success(function (processNodeTemplateDTO: any) {
                    // Get Criteria DTO from REST api.
                    http.get(getCriteriaIdUrl(urlPrefix, id))
                        .success(function (criteriaDTO: any) {
                            // Construct ProcessNodeTemplate object from DTO.
                            var processNodeTemplate = new model.ProcessNodeTemplateDTO(
                                processNodeTemplateDTO.id,
                                false,
                                processNodeTemplateDTO.ProcessNodeTemplateId,
                                processNodeTemplateDTO.Name
                                );

                            // Construct Criteria object from DTO.
                            var criteria = new model.CriteriaDTO(
                                criteriaDTO.id,
                                false,
                                processNodeTemplate.id,
                                criteriaDTO.ExecutionType
                                );

                            angular.forEach(criteriaDTO.Conditions, function (it) {
                                criteria.conditions.push(<model.Condition>it);
                            });

                            processNodeTemplate.criteria = criteria;

                            // Call callback after asynchronous HTTP-operations are complete.
                            callback(processNodeTemplate);
                        });
                });
        };

        //var loadDatasources = function (
        //    eventArgs: RenderEventArgs,
        //    scope: IPaneDefineCriteriaScope,
        //    ActionService: services.IActionService) {
        //    ActionService.getFieldDataSources({}, scope.currentAction).$promise.then((data) => {
        //        scope.fields = [];
        //        data.forEach((value) => {
        //            scope.fields.push(new model.Field(value, value));
        //        });
        //    });
        //}

        // Callback for handling PaneDefineCriteria_Render message.
        var onRender = function (
            eventArgs: RenderEventArgs,
            scope: IPaneDefineCriteriaScope,
            http: ng.IHttpService,
            urlPrefix: string,
            LocalIdentityGenerator: services.LocalIdentityGenerator,
            ActionService: services.IActionService,
            CriteriaServiceWrapper: services.ICriteriaServiceWrapper) {

            console.log('PaneDefineCriteria::onRender', eventArgs);
            console.log('PaneDefineCriteria: currentAction: ' + scope.currentAction);

            //Clean up state from previous Criteria
            cleanUp(scope);

            // If we deal with newly created object (i.e. isTempId === true),
            // then create blank temporary object in the scope of DefineCriteria pane.
            if (eventArgs.isTempId) {
                scope.processNodeTemplate = model.ProcessNodeTemplateDTO.create(
                    eventArgs.processTemplateId,
                    eventArgs.id,
                    LocalIdentityGenerator.getNextId()
                    );

                afterLoaded(scope, eventArgs);
            }
            // If we deal with existing object, we load it asynchronously from REST api.
            else {
                CriteriaServiceWrapper.load(eventArgs.id)
                    .then((pnt: model.ProcessNodeTemplateDTO) => {
                        scope.processNodeTemplate = pnt;
                        afterLoaded(scope, eventArgs);
                    });
            }

            scope.isVisible = true;

            //Check if we have currentAction with the same criteriaId (processNodeTemplateId)
            //If yes, init the module. If no, wait for it and then init the module.  
            if (model.ActionDesignDTO.isActionValid(scope.currentAction)) {
                //loadDatasources(eventArgs, scope, ActionService); we no longer use this v1 approach
            }
            else {
                disposeActionListener = scope.$watch("currentAction", (newAction: interfaces.IActionVM) => {
                    //When user selected the current criteria's action, initialize the pane. 
                    if (model.ActionDesignDTO.isActionValid(newAction)) {
                        disposeActionListener(); //deregister the watch
                        //loadDatasources(eventArgs, scope, ActionService); we no longer use this v1 approach
                    }
                }, true);
            }
        };

        // Callback for handling PaneDefineCriteria_Hide.
        var onHide = function (scope: IPaneDefineCriteriaScope, CriteriaWrapperService: services.ICriteriaServiceWrapper) {
            cleanUp(scope);
            scope.isVisible = false;
        };

        var cleanUp = function (scope) {
            if (disposeActionListener) disposeActionListener(); //deregister currentAction watch
            scope.processNodeTemplate = null;
            scope.fields = [];
        }

        // Callback for handling "Remove criteria" button click event.
        var removeCriteria = function (
            scope: IPaneDefineCriteriaScope,
            http: ng.IHttpService,
            urlPrefix: string
            ) {
            // Call REST api to delete ProcessNodeTemplate object from server.
            http.delete(getSingleProcessNodeTemplateUrl(urlPrefix, scope.processNodeTemplate.id))
                .success(function () {
                    // Emit PaneDefineCriteria_ProcessNodeTemplateRemoving message,
                    // to remove criteria diamond from WorkflowDesigner pane.
                    var eventArgs = new ProcessNodeTemplateRemovingEventArgs(
                        scope.processNodeTemplate.id,
                        scope.processNodeTemplate.isTempId
                        );
                    scope.$emit(
                        MessageType[MessageType.PaneDefineCriteria_ProcessNodeTemplateRemoving],
                        eventArgs
                        );
                });
        };

        // Callback to handle PaneDefineCriteria_Save message,
        // or handle "Save" button click event.
        var save = function (
            scope: IPaneDefineCriteriaScope,
            callback: (args: SaveCallbackArgs) => void,
            CriteriaServiceWrapper: services.ICriteriaServiceWrapper
            ) {

            var curProcessNodeTemplate: model.ProcessNodeTemplateDTO = scope.processNodeTemplate;
            var tempId: number = curProcessNodeTemplate.id; //maybe not temp but then we won't use this value
            var operationResult = CriteriaServiceWrapper.addOrUpdate(curProcessNodeTemplate);

            if (operationResult.actionType == services.ActionTypeEnum.Add) {
                // In case of newly created object (i.e. isTempId === true).
                operationResult.promise.then(() => {
                    // Emit PaneDefineCriteria_ProcessNodeTemplateUpdating message,
                    // to replace temporary id with global id, and update name on WorkflowDesigner pane.
                    scope.$emit(
                        MessageType[MessageType.PaneDefineCriteria_ProcessNodeTemplateUpdated],
                        new ProcessNodeTemplateUpdatedEventArgs(
                            curProcessNodeTemplate.id,
                            curProcessNodeTemplate.name,
                            tempId
                            )
                        );
                    console.log('DefineCriteriaPane::save succeded');
                });
            }
            else if (operationResult.actionType == services.ActionTypeEnum.Update) {
                // In case of existing object with permanent id.
                operationResult.promise.then(() => {
                    // Emit PaneDefineCriteria_ProcessNodeTemplateUpdating message,
                    // to replace temporary id with global id, and update name on WorkflowDesigner pane.
                    scope.$emit(
                        MessageType[MessageType.PaneDefineCriteria_ProcessNodeTemplateUpdated],
                        new ProcessNodeTemplateUpdatedEventArgs(
                            scope.processNodeTemplate.id,
                            scope.processNodeTemplate.name,
                            null
                            )
                        );
                    console.log('DefineCriteriaPane update succeded');
                });
            }
            else {
                return; //ProcessNodeTemplate object was null, just exiting
            }
        }

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/PaneDefineCriteria',
            scope: {
                currentAction: '='
            },
            controller: (
                $scope: IPaneDefineCriteriaScope,
                $http: ng.IHttpService,
                urlPrefix: string,
                LocalIdentityGenerator: services.LocalIdentityGenerator,
                ActionService: services.IActionService,
                CriteriaServiceWrapper: services.ICriteriaServiceWrapper
                ): void => {

                $scope.operators = [
                    { text: 'Greater than', value: 'gt' },
                    { text: 'Greater than or equal', value: 'gte' },
                    { text: 'Less than', value: 'lt' },
                    { text: 'Less than or equal', value: 'lte' },
                    { text: 'Equal', value: 'eq' },
                    { text: 'Not equal', value: 'neq' }
                ];

                $scope.defaultOperator = 'gt';

                $scope.$on(MessageType[MessageType.PaneDefineCriteria_Render],
                    (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => onRender(eventArgs, $scope, $http, urlPrefix, LocalIdentityGenerator, ActionService, CriteriaServiceWrapper));

                $scope.$on(MessageType[MessageType.PaneDefineCriteria_Hide],
                    (event: ng.IAngularEvent) => onHide($scope, CriteriaServiceWrapper));

                $scope.$on(MessageType[MessageType.PaneDefineCriteria_Save],
                    (event: ng.IAngularEvent, eventArgs: SaveEventArgs) => save($scope, eventArgs.callback, CriteriaServiceWrapper));

                $scope.removeCriteria = function () {
                    removeCriteria($scope, $http, urlPrefix);
                };

                $scope.save = function () {
                    save($scope, function () { }, CriteriaServiceWrapper);
                };

                $scope.cancel = function () {
                    $scope.$emit(MessageType[MessageType.PaneDefineCriteria_Cancelling]);
                };

            }
        };
    }

    export enum MessageType {
        PaneDefineCriteria_Render,
        PaneDefineCriteria_Hide,
        PaneDefineCriteria_Save,
        PaneDefineCriteria_ProcessNodeTemplateRemoving,
        PaneDefineCriteria_ProcessNodeTemplateUpdated,
        PaneDefineCriteria_Cancelling
    }

    export class RenderEventArgs {
        public fields: Array<model.Field>;
        public processTemplateId: number;
        public id: number;
        public isTempId: boolean;

        constructor(fields: Array<model.Field>, processTemplateId: number,
            id: number, isTempId: boolean) {

            this.fields = fields;
            this.processTemplateId = processTemplateId;
            this.id = id;
            this.isTempId = isTempId;
        }
    }

    export class HideEventArgs  {
        public processTemplateId: number;
        public id: number;
        public isTempId: boolean;

        constructor(processTemplateId: number,
            id: number, isTempId: boolean) {

            this.processTemplateId = processTemplateId;
            this.id = id;
            this.isTempId = isTempId;
        }
    }

    export class SaveEventArgs {
        public callback: (args: SaveCallbackArgs) => void;

        constructor(callback: (args: SaveCallbackArgs) => void) {
            this.callback = callback;
        }
    }

    export class SaveCallbackArgs {
        public id: number;
        public tempId: number;

        constructor(id: number, tempId: number) {
            this.id = id;
            this.tempId = tempId;
        }
    }

    export class ProcessNodeTemplateRemovingEventArgs {
        public processNodeTemplateId: number;
        public isTempId: boolean;

        constructor(processNodeTemplateId: number, isTempId: boolean) {
            this.processNodeTemplateId = processNodeTemplateId;
            this.isTempId = isTempId;
        }
    }

    export class ProcessNodeTemplateUpdatedEventArgs {
        public processNodeTemplateId: number;
        public name: string;
        public processNodeTemplateTempId: number;

        constructor(
            processNodeTemplateId: number,
            name: string,
            processNodeTemplateTempId: number) {

            this.processNodeTemplateId = processNodeTemplateId;
            this.name = name;
            this.processNodeTemplateTempId = processNodeTemplateTempId;
        }
    }

    export interface IPaneDefineCriteriaScope extends ng.IScope {
        isVisible: boolean;
        removeCriteria: () => void;
        save: () => void;
        cancel: () => void;
        operators: Array<interfaces.IOperator>;
        defaultOperator: string;
        processNodeTemplate: model.ProcessNodeTemplateDTO;
        fields: Array<model.Field>;
        currentAction: interfaces.IActionVM;
        currentCriteria: model.CriteriaDTO;
    }
}

app.directive('paneDefineCriteria', dockyard.directives.paneDefineCriteria.PaneDefineCriteria);