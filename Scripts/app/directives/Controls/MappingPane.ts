/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    interface IMappingPaneScope extends ng.IScope {
        field: any;
        currentAction: model.ActionDesignDTO;
        dataItems: any;
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
                    var upStreamCrate = crateHelper.findByLabel(
                        $scope.currentAction.crateStorage,
                        'Upstream Plugin-Provided Fields'
                        );

                    var downStreamCrate = crateHelper.findByLabel(
                        $scope.currentAction.crateStorage,
                        'Downstream Plugin-Provided Fields'
                        );

                    var upStreamFields = [];
                    var downStreamFields = [];

                    if (upStreamCrate && upStreamCrate.contents) {
                        upStreamFields = angular.fromJson(upStreamCrate.contents).Fields || [];
                    }

                    if (downStreamCrate && downStreamCrate.contents) {
                        downStreamFields = angular.fromJson(downStreamCrate.contents).Fields || [];
                    }

                    $scope.dataItems = [];

                    if (upStreamFields.length && downStreamFields.length) {
                        var sources = [];
                        angular.forEach(upStreamFields, function (it) {
                            sources.push(it);
                        });

                        var targets = [];
                        angular.forEach(downStreamFields, function (it) {
                            targets.push(it);
                        });

                        var minCount = Math.min(upStreamFields.length, downStreamFields.length);
                        for (var i = 0; i < minCount; ++i) {
                            $scope.dataItems.push({
                                sources: sources,
                                targets: targets,
                                selectedSource: null,
                                selectedTarget: null
                            });
                        }
                    }

                    $scope.$watch('field', function (field: any) {
                        if (!field || !field.value) { return; }

                        var jsonValue = angular.fromJson(field.value);
                        for (var i = 0; i < $scope.dataItems.length; ++i) {
                            if (!jsonValue || jsonValue.length <= i) {
                                continue;
                            }

                            $scope.dataItems[i].selectedSource = jsonValue[i].source;
                            $scope.dataItems[i].selectedTarget = jsonValue[i].target;
                        }
                    });

                    $scope.$watch('dataItems', function (dataItem) {
                        var valueArr = [];
                        angular.forEach($scope.dataItems, function (it) {
                            valueArr.push({ source: it.selectedSource, target: it.selectedTarget });
                        });

                        $scope.field.value = angular.toJson(valueArr);
                    }, true);
                }]
        }
    }
} 

app.directive('mappingPane', dockyard.directives.MappingPane);