/// <reference path="../../_all.ts" />
 
module dockyard.directives.paneDefineCriteria {
    'use strict';

    export function PaneDefineCriteria(): ng.IDirective {

        var getProcessNodeTemplateUrl = function (urlPrefix) {
            return urlPrefix + '/processNodeTemplate';
        };

        var getSingleProcessNodeTemplateUrl = function (urlPrefix, id) {
            return urlPrefix + '/processNodeTemplate/' + id;
        };

        var getCriteriaIdUrl = function (urlPrefix, pntId) {
            return urlPrefix + '/processNodeTemplate/criteria?id=' + pntId;
        };

        var getCriteriaUrl = function (urlPrefix) {
            return urlPrefix + '/criteria';
        };

        var afterLoaded = function (scope: IPaneDefineCriteriaScope, eventArgs: RenderEventArgs) {
            scope.fields = eventArgs.fields;

            if (!scope.processNodeTemplate.criteria.conditions) {
                scope.processNodeTemplate.criteria.conditions = [];
            }

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

        var createEmptyProcessNodeTemplate = function (
            processTemplateId,
            processNodeTemplateId,
            criteriaId): model.ProcessNodeTemplate {

            var processNodeTemplate = new model.ProcessNodeTemplate(
                processNodeTemplateId,
                true,
                processTemplateId,
                'New criteria'
                );

            var criteria = new model.Criteria(
                criteriaId,
                true,
                processNodeTemplate.id,
                model.CriteriaExecutionType.WithConditions
                );

            processNodeTemplate.criteria = criteria;

            return processNodeTemplate;
        };

        var loadProcessNodeTemplate = function (http: ng.IHttpService, urlPrefix: string, id, callback) {
            http.get(getSingleProcessNodeTemplateUrl(urlPrefix, id))
                .success(function (processNodeTemplateResult: any) {
                    http.get(getCriteriaIdUrl(urlPrefix, id))
                        .success(function (criteriaResult: any) {
                            var processNodeTemplate = new model.ProcessNodeTemplate(
                                processNodeTemplateResult.Id,
                                false,
                                processNodeTemplateResult.ProcessNodeTemplateId,
                                processNodeTemplateResult.Name
                                );

                            var criteria = new model.Criteria(
                                criteriaResult.Id,
                                false,
                                processNodeTemplate.id,
                                criteriaResult.ExecutionType
                                );

                            angular.forEach(criteriaResult.Conditions, function (it) {
                                criteria.conditions.push(<model.Condition>it);
                            });

                            processNodeTemplate.criteria = criteria;

                            callback(processNodeTemplate);
                        });
                });
        };

        var onRender = function (
            eventArgs: RenderEventArgs,
            scope: IPaneDefineCriteriaScope,
            http: ng.IHttpService,
            urlPrefix: string,
            LocalIdentityGenerator: services.LocalIdentityGenerator) {

            console.log('PaneDefineCriteria::onRender', eventArgs);

            if (eventArgs.isTempId) {
                scope.processNodeTemplate = createEmptyProcessNodeTemplate(
                    eventArgs.processTemplateId,
                    eventArgs.id,
                    LocalIdentityGenerator.getNextId()
                    );

                afterLoaded(scope, eventArgs);
            }
            else {
                loadProcessNodeTemplate(http, urlPrefix, eventArgs.id,
                    function (pnt: model.ProcessNodeTemplate) {
                        scope.processNodeTemplate = pnt;
                        afterLoaded(scope, eventArgs);
                    });
            }

            scope.isVisible = true;
        };

        var onHide = function (scope: IPaneDefineCriteriaScope) {
            scope.isVisible = false;
            scope.processNodeTemplate = null;
            scope.fields = [];
        };

        var removeCriteria = function (
            scope: IPaneDefineCriteriaScope,
            http: ng.IHttpService,
            urlPrefix: string
        ) {
            http.delete(getSingleProcessNodeTemplateUrl(urlPrefix, scope.processNodeTemplate.id))
                .success(function () {
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

        var onSave = function (
            scope: IPaneDefineCriteriaScope,
            http: ng.IHttpService,
            urlPrefix: string,
            callback: () => void
            ) {

            if (scope.processNodeTemplate.isTempId) {
                http.post(getProcessNodeTemplateUrl(urlPrefix), scope.processNodeTemplate)
                    .success(function (processNodeTemplateResult: any) {
                        var processNodeTemplateTempId: number = scope.processNodeTemplate.id;

                        scope.processNodeTemplate.isTempId = false;
                        scope.processNodeTemplate.id = processNodeTemplateResult.Id;

                        var url = getCriteriaIdUrl(urlPrefix, processNodeTemplateResult.Id);
                        http.get(url).success(function (criteriaResult: any) {
                            scope.processNodeTemplate.criteria.id = criteriaResult.Id;
                            scope.processNodeTemplate.criteria.isTempId = false;

                            http.put(getCriteriaUrl(urlPrefix), scope.processNodeTemplate.criteria)
                                .success(function () {
                                    scope.$emit(
                                        MessageType[MessageType.PaneDefineCriteria_ProcessNodeTemplateUpdating],
                                        new ProcessNodeTemplateUpdatingEventArgs(
                                            scope.processNodeTemplate.id,
                                            scope.processNodeTemplate.name,
                                            processNodeTemplateTempId
                                            )
                                    );

                                    console.log('DefineCriteriaPane::save succeded');
                                    callback();
                                });
                        });
                    });
            }

            else {
                http.put(getProcessNodeTemplateUrl(urlPrefix), scope.processNodeTemplate)
                    .success(function () {
                        http.put(getCriteriaUrl(urlPrefix), scope.processNodeTemplate.criteria)
                            .success(function () {
                                scope.$emit(
                                    MessageType[MessageType.PaneDefineCriteria_ProcessNodeTemplateUpdating],
                                    new ProcessNodeTemplateUpdatingEventArgs(
                                        scope.processNodeTemplate.id,
                                        scope.processNodeTemplate.name,
                                        null
                                        )
                                    );

                                console.log('DefineCriteriaPane update succeded');
                                callback();
                            });
                    });
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
