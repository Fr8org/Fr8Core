/// <reference path="../_all.ts" />
/*
    Detail (view/add/edit) controller
*/
var dockyard;
(function (dockyard) {
    var controllers;
    (function (controllers) {
        'use strict';
        var ProcessBuilderController = (function () {
            function ProcessBuilderController($rootScope, $scope, StringService) {
                this.$rootScope = $rootScope;
                this.$scope = $scope;
                this.StringService = StringService;
                // BEGIN ProcessBuilder event handlers.
                var criteriaIdSeq = 0;
                var actionIdSeq = 0;
                $scope.criteria = [];
                $scope.fields = [
                    { key: 'envelope.name', name: '[Envelope].Name' },
                    { key: 'envelope.date', name: '[Envelope].Date' }
                ];
                $scope.selectedCriteria = null;
                $scope.selectedAction = null;
                $scope.isCriteriaSelected = function () {
                    return $scope.selectedCriteria !== null;
                };
                $scope.isActionSelected = function () {
                    return $scope.selectedAction !== null;
                };
                $scope.addCriteria = function () {
                    var id = ++criteriaIdSeq;
                    var criteria = {
                        id: id,
                        name: 'New criteria #' + id.toString(),
                        actions: [],
                        conditions: [
                            {
                                field: 'envelope.name',
                                operator: 'gt',
                                value: '',
                                valueError: true
                            }
                        ],
                        executionMode: 'conditions'
                    };
                    $scope.criteria.push(criteria);
                    $scope.selectedCriteria = criteria;
                    $scope.selectedAction = null;
                };
                $scope.selectCriteria = function (criteriaId) {
                    $scope.selectedAction = null;
                    var i;
                    for (i = 0; i < $scope.criteria.length; ++i) {
                        if ($scope.criteria[i].id === criteriaId) {
                            $scope.selectedCriteria = $scope.criteria[i];
                            break;
                        }
                    }
                };
                $scope.removeCriteria = function () {
                    if (!$scope.selectedCriteria) {
                        return;
                    }
                    var i;
                    for (i = 0; i < $scope.criteria.length; ++i) {
                        if ($scope.criteria[i].id === $scope.selectedCriteria.id) {
                            $scope.criteria.splice(i, 1);
                            $scope.selectedCriteria = null;
                            break;
                        }
                    }
                };
                $scope.addAction = function (criteriaId) {
                    var id = ++actionIdSeq;
                    var i;
                    var action;
                    for (i = 0; i < $scope.criteria.length; ++i) {
                        if ($scope.criteria[i].id === criteriaId) {
                            action = {
                                id: id,
                                name: 'Action #' + id.toString()
                            };
                            $scope.criteria[i].actions.push(action);
                            $scope.selectedCriteria = null;
                            $scope.selectedAction = action;
                            break;
                        }
                    }
                };
                $scope.selectAction = function (criteriaId, actionId) {
                    $scope.selectedCriteria = null;
                    var i, j;
                    for (i = 0; i < $scope.criteria.length; ++i) {
                        if ($scope.criteria[i].id === criteriaId) {
                            for (j = 0; j < $scope.criteria[i].actions.length; ++j) {
                                if ($scope.criteria[i].actions[j].id === actionId) {
                                    $scope.selectedAction = $scope.criteria[i].actions[j];
                                    break;
                                }
                            }
                            break;
                        }
                    }
                };
                $scope.removeAction = function (criteriaId, actionId) {
                    var i, j;
                    for (i = 0; i < $scope.criteria.length; ++i) {
                        if ($scope.criteria[i].id === criteriaId) {
                            for (j = 0; j < $scope.criteria[i].actions.length; ++j) {
                                if ($scope.criteria[i].actions[j].id === actionId) {
                                    $scope.criteria[i].actions.splice(j, 1);
                                    break;
                                }
                            }
                            break;
                        }
                    }
                };
                // END CriteriaPane & ProcessBuilder routines.
                // END ProcessBuilder event handlers.
            }
            // $inject annotation.
            // It provides $injector with information about dependencies to be injected into constructor
            // it is better to have it close to the constructor, because the parameters must match in count and type.
            // See http://docs.angularjs.org/guide/di
            ProcessBuilderController.$inject = [
                '$rootScope',
                '$scope',
                'StringService'
            ];
            return ProcessBuilderController;
        })();
        app.controller('ProcessBuilderController', ProcessBuilderController);
    })(controllers = dockyard.controllers || (dockyard.controllers = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ProcessBuilderController.js.map