/// <reference path="../../_all.ts" />
 
module dockyard.directives.paneDefineCriteria {
    'use strict';

    export function PaneDefineCriteria(): ng.IDirective {

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
            return urlPrefix + '/processNodeTemplate/criteria?id=' + pntId;
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

        // Create and return empty ProcessNodeTemplate object,
        // if user selects just newly created Criteria diamond on WorkflowDesigner pane.
        var createEmptyProcessNodeTemplate = function (
            processTemplateId,
            processNodeTemplateId,
            criteriaId): model.ProcessNodeTemplate {

            // Create new ProcessNodeTemplate object with default name and provided temporary id.
            var processNodeTemplate = new model.ProcessNodeTemplate(
                processNodeTemplateId,
                true,
                processTemplateId,
                'New criteria'
                );

            // Create criteria with default conditions, and temporary criteria.id.
            var criteria = new model.Criteria(
                criteriaId,
                true,
                processNodeTemplate.id,
                model.CriteriaExecutionType.WithConditions
                );

            processNodeTemplate.criteria = criteria;

            return processNodeTemplate;
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
                            var processNodeTemplate = new model.ProcessNodeTemplate(
                                processNodeTemplateDTO.Id,
                                false,
                                processNodeTemplateDTO.ProcessNodeTemplateId,
                                processNodeTemplateDTO.Name
                                );

                            // Construct Criteria object from DTO.
                            var criteria = new model.Criteria(
                                criteriaDTO.Id,
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

        // Callback for handling PaneDefineCriteria_Render message.
        var onRender = function (
            eventArgs: RenderEventArgs,
            scope: IPaneDefineCriteriaScope,
            http: ng.IHttpService,
            urlPrefix: string,
            LocalIdentityGenerator: services.LocalIdentityGenerator) {
            console.log('PaneDefineCriteria::onRender', eventArgs);

            // If we deal with newly created object (i.e. isTempId === true),
            // then create blank temporary object in the scope of DefineCriteria pane.
            if (eventArgs.isTempId) {
                scope.processNodeTemplate = createEmptyProcessNodeTemplate(
                    eventArgs.processTemplateId,
                    eventArgs.id,
                    LocalIdentityGenerator.getNextId()
                    );

                afterLoaded(scope, eventArgs);
            }
            // If we deal with existing object, we load it asynchronously from REST api.
            else {
                loadProcessNodeTemplate(http, urlPrefix, eventArgs.id,
                    function (pnt: model.ProcessNodeTemplate) {
                        scope.processNodeTemplate = pnt;
                        afterLoaded(scope, eventArgs);
                    });
            }

            scope.isVisible = true;
        };

        // Callback for handling PaneDefineCriteria_Hide.
        var onHide = function (scope: IPaneDefineCriteriaScope) {
            scope.isVisible = false;
            scope.processNodeTemplate = null;
            scope.fields = [];
        };

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

        // Save newly created object on server.
        var saveNew = function (
            scope: IPaneDefineCriteriaScope,
            http: ng.IHttpService,
            urlPrefix: string,
            callback: (args: SaveCallbackArgs) => void
            ) {
            // Save ProcessNodeTemplate object to server.
            // Server automatically creates empty criteria node.
            http.post(getProcessNodeTemplateUrl(urlPrefix), scope.processNodeTemplate)
                .success(function (processNodeTemplateResult: any) {
                    var processNodeTemplateTempId: number = scope.processNodeTemplate.id;

                    scope.processNodeTemplate.isTempId = false;
                    scope.processNodeTemplate.id = processNodeTemplateResult.Id;

                    // Fetch criteria object from server by ProcessNodeTemplate global ID.
                    var url = getCriteriaIdUrl(urlPrefix, processNodeTemplateResult.Id);
                    http.get(url).success(function (criteriaResult: any) {
                        scope.processNodeTemplate.criteria.id = criteriaResult.Id;
                        scope.processNodeTemplate.criteria.isTempId = false;

                        // Update criteria object on server.
                        http.put(getCriteriaUrl(urlPrefix), scope.processNodeTemplate.criteria)
                            .success(function () {
                                // Emit PaneDefineCriteria_ProcessNodeTemplateUpdating message,
                                // to replace temporary id with global id, and update name on WorkflowDesigner pane.
                                scope.$emit(
                                    MessageType[MessageType.PaneDefineCriteria_ProcessNodeTemplateUpdating],
                                    new ProcessNodeTemplateUpdatingEventArgs(
                                        scope.processNodeTemplate.id,
                                        scope.processNodeTemplate.name,
                                        processNodeTemplateTempId
                                        )
                                    );

                                console.log('DefineCriteriaPane::save succeded');

                                // Invoke callback, after all asynchronous HTTP operations were completed.
                                callback(new SaveCallbackArgs(processNodeTemplateResult.Id, processNodeTemplateTempId));
                            });
                    });
                });
        };

        // Update existing object on server.
        var saveExisting = function (
            scope: IPaneDefineCriteriaScope,
            http: ng.IHttpService,
            urlPrefix: string,
            callback: (args: SaveCallbackArgs) => void
            ) {
            // Call REST api to update ProcessNodeTemplate entity on server.
            http.put(getProcessNodeTemplateUrl(urlPrefix), scope.processNodeTemplate)
                .success(function () {
                    // Call REST api to update Criteria entity on server.
                    http.put(getCriteriaUrl(urlPrefix), scope.processNodeTemplate.criteria)
                        .success(function () {
                            // Emit PaneDefineCriteria_ProcessNodeTemplateUpdating message,
                            // to replace temporary id with global id, and update name on WorkflowDesigner pane.
                            scope.$emit(
                                MessageType[MessageType.PaneDefineCriteria_ProcessNodeTemplateUpdating],
                                new ProcessNodeTemplateUpdatingEventArgs(
                                    scope.processNodeTemplate.id,
                                    scope.processNodeTemplate.name,
                                    null
                                    )
                                );

                            console.log('DefineCriteriaPane update succeded');

                            // Invoke callback, after all asynchronous HTTP operations were completed.
                            callback(new SaveCallbackArgs(scope.processNodeTemplate.id, null));
                        });
                });
        };

        // Callback to handle PaneDefineCriteria_Save message,
        // or handle "Save" button click event.
        var onSave = function (
            scope: IPaneDefineCriteriaScope,
            http: ng.IHttpService,
            urlPrefix: string,
            callback: (args: SaveCallbackArgs) => void
            ) {

            //don't save anything if there is no criteria is selected
            if (!scope.processNodeTemplate) return;

            // In case of newly created object (i.e. isTempId === true).
            if (scope.processNodeTemplate.isTempId) {
                saveNew(scope, http, urlPrefix, callback);
            }
            // In case of existing object with global ID.
            else {
                saveExisting(scope, http, urlPrefix, callback);
            }
        };


        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/PaneDefineCriteria',
            scope: {},
            controller: (
                $scope: IPaneDefineCriteriaScope,
                $http: ng.IHttpService,
                urlPrefix: string, 
                LocalIdentityGenerator: services.LocalIdentityGenerator
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
                    (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => onRender(eventArgs, $scope, $http, urlPrefix, LocalIdentityGenerator));

                $scope.$on(MessageType[MessageType.PaneDefineCriteria_Hide],
                    (event: ng.IAngularEvent) => onHide($scope));

                $scope.$on(MessageType[MessageType.PaneDefineCriteria_Save],
                    (event: ng.IAngularEvent, eventArgs: SaveEventArgs) => onSave($scope, $http, urlPrefix, eventArgs.callback));

                $scope.removeCriteria = function () {
                    removeCriteria($scope, $http, urlPrefix);
                };

                $scope.save = function () {
                    onSave($scope, $http, urlPrefix, function () { });
                };

                $scope.cancel = function () {
                    $scope.$emit(MessageType[MessageType.PaneDefineCriteria_Cancelling]);
                };
            }
        };
    }
}

app.directive('paneDefineCriteria', dockyard.directives.paneDefineCriteria.PaneDefineCriteria);
