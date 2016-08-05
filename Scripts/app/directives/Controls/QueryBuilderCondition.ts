module dockyard.directives {
    'use strict';

    export interface IQueryBuilderConditionScope extends ng.IScope {
        currentAction: model.ActivityDTO;
        fields: Array<model.FieldDTO>;
        requestUpstream: boolean;
        operators: Array<IQueryOperator>;
        condition: IQueryCondition;
        isSingle: boolean;
        hasConfigurationControl: boolean;
        toggle: boolean;

        fieldSelected: () => void;
        onRemoveCondition: () => void;
        toggleDropDown: (select: any) => void;
        focusOutSet: (focusElem: any) => void;

        rootElem: any;
        isDisabled:boolean;
    }

    export function QueryBuilderCondition(): ng.IDirective {
        return {
            restrict: 'A',
            replace: true,
            templateUrl: '/AngularTemplate/QueryBuilderCondition',
            scope: {
                currentAction: '=',
                condition: '=',
                fields: '=',
                requestUpstream: '=?',
                operators: '=',
                isSingle: '=',
                onRemoveCondition: '&',
                isDisabled: '='
            },
            link: (scope: IQueryBuilderConditionScope,
                elem: ng.IAugmentedJQuery,
                attr: ng.IAttributes) => {

                scope.rootElem = elem;
            },
            controller: ['$rootScope', '$scope', '$compile', 'UpstreamExtractor',
                ($rootScope: ng.IRootScopeService,
                    $scope: IQueryBuilderConditionScope,
                    $compile: ng.ICompileService,
                    UpstreamExtractor: services.UpstreamExtractor) => {

                    var configurationControl = null;

                    var createControl = (condition: IQueryCondition): model.ControlDefinitionDTO => {
                        var control;

                        if ($scope.condition.field.fieldType === model.FieldType[model.FieldType.Date]) {
                            control = new model.DatePicker();
                            control.value = condition.value;
                        }
                        else if ($scope.condition.field.fieldType === model.FieldType[model.FieldType.PickList]
                            && $scope.condition.field.data['allowableValues']) {
                            control = new model.DropDownList();

                            var listItems: Array<model.DropDownListItem> = [];
                            angular.forEach(
                                $scope.condition.field.data['allowableValues'],
                                (item) => {
                                    listItems.push(new model.DropDownListItem(item.key, item.value));
                                }
                            );

                            control.listItems = listItems;
                        } else { // All other field types.
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

                    $scope.toggle = false;
                    $scope.toggleDropDown = $select => {
                        if (!$scope.focusOutSet) {
                            var focusElem = angular.element($select.focusInput);
                            $scope.focusOutSet = isFocusOutFunc;
                            $scope.focusOutSet(focusElem);
                        }


                            $select.open = !$scope.toggle;
                            $scope.toggle = !$scope.toggle;

                    };

                    var isFocusOutFunc = focusElem => {
                        focusElem.focusout(() => {
                            $scope.toggle = false;
                        });
                    }
                }
            ]
        };
    }
}

app.directive('queryBuilderCondition', dockyard.directives.QueryBuilderCondition);
