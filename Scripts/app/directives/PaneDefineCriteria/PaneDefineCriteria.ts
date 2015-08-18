/// <reference path="../../_all.ts" />
 
module dockyard.directives.paneDefineCriteria {
    'use strict';

    export function PaneDefineCriteria(): ng.IDirective {

        var onRender = function (
            eventArgs: RenderEventArgs,
            scope: IPaneDefineCriteriaScope,
            LocalIdentityGenerator: services.LocalIdentityGenerator) {

            console.log('PaneDefineCriteria::onRender', eventArgs);

            var processNodeTemplate = eventArgs.processNodeTemplate.clone();
            if (processNodeTemplate.isTempId) {
                var criteria = new model.Criteria(
                    LocalIdentityGenerator.getNextId(),
                    true,
                    processNodeTemplate.id,
                    model.CriteriaExecutionType.WithConditions
                );

                processNodeTemplate.criteria = criteria;
            }

            scope.fields = eventArgs.fields;
            scope.processNodeTemplate = processNodeTemplate;
            scope.isVisible = true;

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

        var onHide = function (scope: IPaneDefineCriteriaScope) {
            scope.isVisible = false;
            scope.processNodeTemplate = null;
            scope.fields = [];
        };

        var removeCriteria = function (scope: IPaneDefineCriteriaScope) {
            var eventArgs = new ProcessNodeTemplateRemovingEventArgs(scope.processNodeTemplate.id, scope.processNodeTemplate.isTempId);
            scope.$emit(MessageType[MessageType.PaneDefineCriteria_ProcessNodeTemplateRemoving], eventArgs);
        };

        var onSave = function (
            scope: IPaneDefineCriteriaScope,
            http: ng.IHttpService,
            urlPrefix: string
            ) {

            var processNodeTemplateUrl = urlPrefix + '/processNodeTemplate';
            var getCriteriaIdUrlPrefix = urlPrefix + '/processNodeTemplate/criteria?id=';
            var criteriaUrl = urlPrefix + '/criteria';

            if (scope.processNodeTemplate.isTempId) {
                http.post(processNodeTemplateUrl, scope.processNodeTemplate)
                    .success(function (processNodeTemplateResult: any) {
                        var processNodeTemplateTempId: number = scope.processNodeTemplate.id;

                        scope.processNodeTemplate.isTempId = false;
                        scope.processNodeTemplate.id = processNodeTemplateResult.Id;

                        var url = getCriteriaIdUrlPrefix + processNodeTemplateResult.Id;
                        http.get(url).success(function (criteriaResult: any) {
                            scope.processNodeTemplate.criteria.id = criteriaResult.Id;
                            scope.processNodeTemplate.criteria.isTempId = false;

                            http.put(criteriaUrl, scope.processNodeTemplate.criteria)
                                .success(function () {
                                    scope.$emit(
                                        MessageType[MessageType.PaneDefineCriteria_ProcessNodeTemplateUpdating],
                                        new ProcessNodeTemplateUpdatingEventArgs(
                                            scope.processNodeTemplate.id,
                                            scope.processNodeTemplate.name,
                                            processNodeTemplateTempId
                                            )
                                        );
                                });
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
                    (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => onRender(eventArgs, $scope, LocalIdentityGenerator));

                $scope.$on(MessageType[MessageType.PaneDefineCriteria_Hide],
                    (event: ng.IAngularEvent) => onHide($scope));

                $scope.$on(MessageType[MessageType.PaneDefineCriteria_Save],
                    (event: ng.IAngularEvent) => onSave($scope, $http, urlPrefix));

                $scope.removeCriteria = function () {
                    removeCriteria($scope);
                };

                $scope.save = function () {
                    onSave($scope, $http, urlPrefix);
                };

                $scope.cancel = function () {
                    $scope.$emit(MessageType[MessageType.PaneDefineCriteria_Cancelling]);
                };
            }
        };
    }
}

app.directive('paneDefineCriteria', dockyard.directives.paneDefineCriteria.PaneDefineCriteria);
