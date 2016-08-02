/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface ITerminalListScope extends ng.IScope {
        terminals: Array<model.TerminalRegistrationDTO>;
        showAddTerminalModal: () => void;
    }

    class TerminalListController {

        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'TerminalService',
            '$modal'
        ];

        constructor(
            private $scope: ITerminalListScope,
            private TerminalService: services.ITerminalService,
            private $modal: any) {

            $scope.showAddTerminalModal = <() => void>angular.bind(this, this.showAddTerminalModal);

            TerminalService.getRegistrations().$promise.then(data => {
                $scope.terminals = data;
            }).catch(e => {
                console.log(e.statusText);
            });
        }

        private showAddTerminalModal() {
            this.$modal.open({
                animation: true,
                templateUrl: 'terminalFormModal',
                controller: 'TerminalFormController'
            })
                .result.then(terminal => {
                    this.$scope.terminals.push(terminal);
                });
        }
    }

    app.controller('TerminalListController', TerminalListController);
}