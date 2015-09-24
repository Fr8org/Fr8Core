/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    interface IMappingPaneScope extends ng.IScope {
        field: any;
        currentAction: model.ActionDTO;
        sourceData: any;
        targetData: any;
    }

    export function MappingPane(): ng.IDirective {
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/MappingPane',
            scope: {
                currentAction: '=',
                field: '='
            },
            controller: ['$scope', 'CrateHelper',
                function ($scope: IMappingPaneScope, crateHelper: services.CrateHelper) {
                    var upStreamFields = crateHelper.findByLabel(
                        $scope.currentAction.crateStorage,
                        'Upstream Plugin-Provided Fields'
                        );

                    if (upStreamFields && upStreamFields.contents) {
                        $scope.sourceData = [];
                        angular.forEach(angular.fromJson(upStreamFields.contents), function (it) {
                            $scope.sourceData.push(it);
                        });
                    }

                    var downStreamFields = crateHelper.findByLabel(
                        $scope.currentAction.crateStorage,
                        'Downstream Plugin-Provided Fields'
                        );

                    if (downStreamFields && downStreamFields.contents) {
                        $scope.targetData = [];
                        angular.forEach(angular.fromJson(downStreamFields.contents), function (it) {
                            $scope.targetData.push(it);
                        });
                    }

                    $scope.$watch('field', function (field: any) {
                        if (!field || !field.value) { return; }

                        var jsonValue = angular.fromJson(field.value);
                        angular.forEach(jsonValue, function (it) {
                            for (var i = 0; i < $scope.sourceData.length; ++i) {
                                if ($scope.sourceData[i].id == it.source) {
                                    $scope.sourceData[i].targetDataItem = it.target;
                                    break;
                                }
                            }
                        });
                    });

                    $scope.$watch('sourceData', function (source) {
                        var valueArr = [];
                        angular.forEach($scope.sourceData, function (it) {
                            valueArr.push({ source: it.id, target: it.targetDataItem });
                        });

                        $scope.field.value = angular.toJson(valueArr);
                    }, true);
                }]
        }
    }
} 

app.directive('mappingPane', dockyard.directives.MappingPane);