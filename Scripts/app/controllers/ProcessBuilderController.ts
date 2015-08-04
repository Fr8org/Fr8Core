/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    class ProcessBuilderController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di


        public static $inject = [
            '$rootScope',
            '$scope',
            'StringService'
        ];
        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: interfaces.IProcessBuilderScope,
            private StringService: services.IStringService) {

            // BEGIN ProcessBuilder event handlers.
            var criteriaIdSeq = 0;
            var actionIdSeq = 0;

            $scope.criteria = [];

            $scope.fields = [
                { key: 'envelope.name', name: '[Envelope].Name' },
                { key: 'envelope.date', name: '[Envelope].Date' }
            ];

            $scope.selectedCriteria = null;

            $scope.isCriteriaSelected = function () {
                return $scope.selectedCriteria !== null;
            };

            $scope.addCriteria = function () {
                var id = ++criteriaIdSeq;

                $scope.criteria.push({
                    id: id,
                    name: 'New criteria #' + id.toString(),
                    actions: [],
                    conditions: [
                        {
                            field: 'envelope.name',
                            operator: 'gt',
                            value: ''
                        }
                    ],
                    executionMode: 'conditions'
                });
            };

            $scope.selectCriteria = function (criteriaId) {
                var i;
                for (i = 0; i < $scope.criteria.length; ++i) {
                    if ($scope.criteria[i].id === criteriaId) {
                        $scope.selectedCriteria = $scope.criteria[i];
                        break;
                    }
                }
            };

            $scope.removeCriteria = function () {
                if (!$scope.selectedCriteria) { return; }

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
                for (i = 0; i < $scope.criteria.length; ++i) {
                    if ($scope.criteria[i].id === criteriaId) {
                        $scope.criteria[i].actions.push({
                            id: id,
                            name: 'Action #' + id.toString()
                        });
                        break;
                    }
                }
            };

            $scope.selectAction = function (criteriaId, actionId) {
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
    }
    app.controller('ProcessBuilderController', ProcessBuilderController);
} 