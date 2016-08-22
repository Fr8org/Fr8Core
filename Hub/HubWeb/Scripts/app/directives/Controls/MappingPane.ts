/// <reference path="../../_all.ts" />

module dockyard.directives {
    'use strict';

    interface IMappingPaneScope extends ng.IScope {
        field: any;
        currentAction: model.ActivityDTO;
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
                        'Upstream Terminal-Provided Fields'
                        );

                    var downStreamCrate = crateHelper.findByLabel(
                        $scope.currentAction.crateStorage,
                        'Downstream Terminal-Provided Fields'
                        );

                    var upStreamFields = [];
                    var downStreamFields = [];

                    if (upStreamCrate && upStreamCrate.contents) {
                        upStreamFields =(<any>upStreamCrate.contents).Fields || [];
                    }

                    if (downStreamCrate && downStreamCrate.contents) {
                        downStreamFields = (<any>downStreamCrate.contents).Fields || [];
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

                            $scope.dataItems[i].selectedSource = jsonValue[i].Key;
                            $scope.dataItems[i].selectedTarget = jsonValue[i].Value;
                            }
                        });

                    $scope.$watch('dataItems', function (dataItem) {
                        var valueArr = [];
                        angular.forEach($scope.dataItems, function (it) {
                            valueArr.push({ Key: it.selectedSource, Value: it.selectedTarget });
                        });

                        $scope.field.value = angular.toJson(valueArr);
                    }, true);
                }]
        }
    }
} 

app.directive('mappingPane', dockyard.directives.MappingPane);