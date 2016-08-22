/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface ITerminalFormScope extends ng.IScope {
        terminal: interfaces.ITerminalVM;
        submit: (isValid: boolean) => void;
        cancel: () => void;
        errorMessage: string;
        processing: boolean;
    }

    class TerminalFormController {

        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'TerminalService',
            '$modalInstance',
            'StringService'
        ];

        constructor(
            private $scope: ITerminalFormScope,
            private TerminalService: services.ITerminalService,
            private $modalInstance: any,
            private StringService: dockyard.services.IStringService) {
                $scope.terminal = <interfaces.ITerminalVM>{};
                $scope.terminal.endpoint = "https://";
                $scope.cancel = <() => void>angular.bind(this, this.cancelForm);
                $scope.submit = isValid => {
                if (isValid) {
                    $scope.processing = true;
                    $scope.terminal.devUrl = $scope.terminal.endpoint;

                    var result = TerminalService.save($scope.terminal);
                    result.$promise
                        .then(terminal => {
                            this.$modalInstance.close(terminal);
                            $scope.processing = false;
                        })
                        .catch(e => {
                            debugger;
                            console.log('Terminal addition failed: ' + e.data.message || e.status);
                            $scope.processing = false;
                            switch (e.status) {
                                case 400:
                                    this.$scope.errorMessage = this.StringService.terminal["error400"];
                                    if (e.data.message) {
                                        this.$scope.errorMessage += " Additional information: " + e.data.message;
                                    }
                                    break;
                                case 404:
                                    this.$scope.errorMessage = this.StringService.terminal["error404"];
                                    break;
                                case 409:
                                    this.$scope.errorMessage = this.StringService.terminal["error409"];
                                    break;
                                default:
                                    this.$scope.errorMessage = this.StringService.terminal["error"];
                                    break;
                            }

                        });
                }
            };
        }

        private cancelForm() {
            this.$modalInstance.dismiss('cancel');
        }
    }

    app.controller('TerminalFormController', TerminalFormController);
}