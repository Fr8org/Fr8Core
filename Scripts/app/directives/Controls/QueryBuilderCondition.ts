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
                onRemoveCondition: '&'
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

                    var loadUpstreamFields = () => {
                        var availabilityType = 'NotSet';

                        return UpstreamExtractor
                            .getAvailableData($scope.currentAction.id, availabilityType)
                            .then((data: model.IncomingCratesDTO) => {
                                var fields: Array<model.FieldDTO> = [];

                                angular.forEach(data.availableCrates, (ct) => {
                                    angular.forEach(ct.fields, (f) => {
                                        var i, j;
                                        var found = false;
                                        for (i = 0; i < fields.length; ++i) {
                                            if (fields[i].key === f.key) {
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (!found) {
                                            fields.push(f);
                                        }
                                    });
                                });

                                fields.sort((x, y) => {
                                    if (x.key < y.key) {
                                        return -1;
                                    }
                                    else if (x.key > y.key) {
                                        return 1;
                                    }
                                    else {
                                        return 0;
                                    }
                                });

                                $scope.fields = fields;
                            });
                    };

                    $scope.toggle = false;
                    $scope.toggleDropDown = $select => {
                        if (!$scope.focusOutSet) {
                            var focusElem = angular.element($select.focusInput);
                            $scope.focusOutSet = isFocusOutFunc;
                            $scope.focusOutSet(focusElem);
                        }

                        if (!$scope.toggle
                            && $scope.requestUpstream) {

                            $select.open = false;

                            loadUpstreamFields().then(() => { //parameter isSilent false, since we want to see error messages
                                $select.open = !$scope.toggle;
                                $scope.toggle = !$scope.toggle;
                            });
                        }
                        else {
                            $select.open = !$scope.toggle;
                            $scope.toggle = !$scope.toggle;
                        }
                    };

                    var isFocusOutFunc = focusElem => {
                        focusElem.focusout(() => {
                            $scope.toggle = false;
                        });
                    }

                    if ($scope.requestUpstream) {
                        $scope.fields = [];
                    }
                }
            ]
        };
    }
}

app.directive('queryBuilderCondition', dockyard.directives.QueryBuilderCondition);
