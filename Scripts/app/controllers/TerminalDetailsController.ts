/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface ITerminalDetailsScope extends ng.IScope {
        terminal: model.TerminalDTO;
        openPermissionsSetterModal: (terminal: model.TerminalDTO) => void;
        submit: (isValid: boolean) => void;
        cancel: () => void;
    }

    class TerminalDetailsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            '$state',
            '$modal',
            'TerminalService'
        ];

        constructor(
            private $scope: ITerminalDetailsScope,
            private $state: ng.ui.IStateService,
            private $modal: any,
            private TerminalService: services.ITerminalService) {

            TerminalService.get({ id: $state.params['id'] }).$promise.then(function (data) {
                debugger;
                $scope.terminal = data;
            });

            $scope.cancel = function () {
                $state.go('terminals');
            };

            $scope.openPermissionsSetterModal = (terminal: model.TerminalDTO) => {
                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: '/AngularTemplate/PermissionsSetterModal',
                    controller: 'PermissionsSetterModalController',
                    resolve: {
                        terminal: () => terminal
                    }
                });
                modalInstance.result.then(function (terminal: model.TerminalDTO) {
                    //TerminalServire.setPermissions(terminal);
                });
            }
        }
    }

    app.controller('TerminalDetailsController', TerminalDetailsController);
    app.controller('PermissionsSetterModalController', ['$scope', '$modalInstance','terminal' , ($scope: any, $modalInstance: any, terminal : model.TerminalDTO): void => {

        $scope.terminal = terminal;

        $scope.submitForm = () => {
            $modalInstance.close($scope.label);
        };

        $scope.cancel = () => {
            $modalInstance.dismiss();
        };

    }]);
}