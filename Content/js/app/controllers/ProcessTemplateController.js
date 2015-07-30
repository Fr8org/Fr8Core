'use strict';

/*
    Detail (view/add/edit) controller
*/
MetronicApp.controller('ProcessTemplateController',
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

        // BEGIN ProcessBuilder event handlers.

        var criteriaIdSeq = 0;
        var actionIdSeq = 0;

        $scope.pbAddCriteriaClick = function () {
            $scope.processBuilder.addCriteria({ id: ++criteriaIdSeq })
        };

        $scope.pbCriteriaClick = function (criteriaId) {
            $scope.processBuilder.removeCriteria(criteriaId);
        };

        $scope.pbAddActionClick = function (criteriaId) {
            $scope.processBuilder.addAction(criteriaId, { id: ++actionIdSeq });
        };

        $scope.pbActionClick = function (criteriaId, actionId) {
            $scope.processBuilder.removeAction(criteriaId, actionId);
        };

        // END ProcessBuilder event handlers.
    }]);

/*
    List controller
*/
MetronicApp.controller('ProcessTemplatesController', 
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

MetronicApp.controller('ProcessTemplatesController__DeleteConfirmation', function ($scope, $modalInstance) {


    $scope.ok = function () {
        $modalInstance.close();
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };
});