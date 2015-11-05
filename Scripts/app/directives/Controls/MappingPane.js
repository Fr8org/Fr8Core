/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        'use strict';
        function MappingPane() {
            return {
                restrict: 'E',
                templateUrl: '/AngularTemplate/MappingPane',
                scope: {
                    currentAction: '=',
                    field: '='
                },
                controller: ['$scope', 'CrateHelper',
                    function ($scope, crateHelper) {
                        var upStreamCrate = crateHelper.findByLabel($scope.currentAction.crateStorage, 'Upstream Plugin-Provided Fields');
                        var downStreamCrate = crateHelper.findByLabel($scope.currentAction.crateStorage, 'Downstream Plugin-Provided Fields');
                        var upStreamFields = [];
                        var downStreamFields = [];
                        if (upStreamCrate && upStreamCrate.contents) {
                            upStreamFields = upStreamCrate.contents.Fields || [];
                        }
                        if (downStreamCrate && downStreamCrate.contents) {
                            downStreamFields = downStreamCrate.contents.Fields || [];
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
                        $scope.$watch('field', function (field) {
                            if (!field || !field.value) {
                                return;
                            }
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
            };
        }
        directives.MappingPane = MappingPane;
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
app.directive('mappingPane', dockyard.directives.MappingPane);
//# sourceMappingURL=MappingPane.js.map