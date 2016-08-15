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
            '$modalInstance'
        ];

        constructor(
            private $scope: ITerminalFormScope,
            private TerminalService: services.ITerminalService,
            private $modalInstance: any) {
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
                                $scope.errorMessage = "Terminal create error";
                                $scope.processing = false;
                                console.log(e.statusText);
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