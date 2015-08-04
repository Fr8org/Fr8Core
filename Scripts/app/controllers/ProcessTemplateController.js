'use strict';

/*
    Detail (view/add/edit) controller
*/
app.controller('ProcessTemplateController',
    ['$rootScope', '$scope', '$resource', 'ProcessTemplateService', '$stateParams', 'StringService',
    function ($rootScope, $scope, $resource, $ProcessTemplateService, $stateParams, $StringService) {

        $scope.$on('$viewContentLoaded', function () {
            // initialize core components
            Metronic.initAjax();
        });

        //Load detailed information
        var id = $stateParams.id;
        if (/^[0-9]+$/.test(id) && parseInt(id) > 1) {
            $scope.ptvm = $ProcessTemplateService.get({ id: $stateParams.id });
        }

        //Save button
        $scope.submit = function (isValid) {
            if (isValid) {
                $ProcessTemplateService.save($scope.ptvm).$promise
                    .then(function () {
                        $rootScope.lastResult = "success";
                        window.location.href = '#processes';
                    })
                    .catch(function (e) {
                        switch (e.status) {
                            case 404:
                                $scope.errorMessage = $StringService.processTemplate.error404;
                                break;
                            case 400:
                                $scope.errorMessage = $StringService.processTemplate.error400;
                                break;
                            default:
                                $scope.errorMessage = $StringService.processTemplate.error;
                                break;
                        }
                    });
            }
        };

        // BEGIN CriteriaPane & ProcessBuilder event routines.

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
    }]);

/*
    List controller
*/
app.controller('ProcessTemplatesController', 
    ['$rootScope', '$scope', '$resource', 'ProcessTemplateService', '$modal', '$filter',
        function ($rootScope, $scope, $resource, $ProcessTemplateService, $modal, $filter) {

            //Clear the last result value (but still allow time for the confirmation message to show up)
            setTimeout(function () {
                delete $rootScope.lastResult;
            }, 300);

            $scope.$on('$viewContentLoaded', function () {

                // initialize core components
                Metronic.initAjax();
            });

            //Load Process Templates view model
            $scope.ptvms = $ProcessTemplateService.query();

            //Detail/edit link
            $scope.nav = function (pt) {
                window.location.href = '#processes/' + pt.Id;
            }

            //Delete link
            $scope.remove = function (pt) {
                $modal.open({
                    animation: true,
                    templateUrl: 'modalDeleteConfirmation',
                    controller: 'ProcessTemplatesController__DeleteConfirmation',
                    size: null

                }).result.then(function (selectedItem) {
                    //Deletion confirmed
                    $ProcessTemplateService.delete({ id: pt.Id }).$promise.then(function () {
                        $rootScope.lastResult = "success";
                        $scope.ptvms
                        window.location.href = '#processes';
                    });

                    //Remove from local collection
                    $scope.ptvms = $filter('filter')($scope.ptvms, function (value, index) { return value.Id !== pt.Id; })

                }, function () {
                    //Deletion cancelled
                });
            };
}]);

app.controller('ProcessTemplatesController__DeleteConfirmation', function ($scope, $modalInstance) {


    $scope.ok = function () {
        $modalInstance.close();
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };
});