module dockyard.directives {
    'use strict';

    export interface IQueryBuilderCondition2Scope extends ng.IScope {
        currentAction: model.ActivityDTO;
        fields: Array<model.FieldDTO>;
        operators: Array<IQueryOperator2>;
        condition: IQueryCondition2;
        isSingle: boolean;
        hasConfigurationControl: boolean;

        fieldSelected: () => void;
        onRemoveCondition: () => void;

        rootElem: any;
    }

    export function QueryBuilderCondition2(): ng.IDirective {
        return {
            restrict: 'A',
            replace: true,
            templateUrl: '/AngularTemplate/QueryBuilderCondition2',
            scope: {
                currentAction: '=',
                condition: '=',
                fields: '=',
                operators: '=',
                isSingle: '=',
                onRemoveCondition: '&'
            },
            link: (scope: IQueryBuilderCondition2Scope,
                elem: ng.IAugmentedJQuery,
                attr: ng.IAttributes) => {

                scope.rootElem = elem;
            },
            controller: ['$rootScope', '$scope', '$compile',
                ($rootScope: ng.IRootScopeService,
                    $scope: IQueryBuilderCondition2Scope,
                    $compile: ng.ICompileService) => {

                    var configurationControl = null;

                    var createControl = (condition: IQueryCondition2): model.ControlDefinitionDTO => {
                        var control;

                        if ($scope.condition.field.fieldType === 'Date') {
                            control = new model.DatePicker();
                            control.value = condition.value;
                        }
                        // All other field types.
                        else {
                            control = new model.TextBox();
                            control.value = condition.value;
                        }

                        return control;
                    };

                    var attachControl = () => {
                        if (configurationControl) {
                            configurationControl.scope.$destroy();
                            configurationControl.markup.remove();

                            configurationControl = null;
                            $scope.hasConfigurationControl = false;
                        }

                        if (!$scope.condition.field) {
                            return;
                        }

                        var configurationControlScope = $scope.$new();
                        (<any>configurationControlScope).control = createControl($scope.condition);
                        (<any>configurationControlScope).currentAction = $scope.currentAction;

                        $scope.hasConfigurationControl = true; 

                        $compile('<configuration-control current-action="currentAction" field="control" />')
                            (configurationControlScope, (markup, scope) => {
                                $('.condition-control', $scope.rootElem).append(markup);

                                scope.$watch('control', function (newValue: any) {
                                    $scope.condition.value = newValue.value;
                                }, true);

                                configurationControl = {
                                    scope: scope,
                                    markup: markup
                                };
                            });
                    };

                    $scope.fieldSelected = () => {
                        attachControl();
                    };

                    $scope.$watch('condition', function () {
                        attachControl();
                    });

                    $scope.$watch('condition.value', function (value) {
                        if (configurationControl) {
                            configurationControl.scope.control.value = value;
                        }
                    });

                    // attachControl();
                }
            ]
        };
    }
}

app.directive('queryBuilderConditionTwo', dockyard.directives.QueryBuilderCondition2);
