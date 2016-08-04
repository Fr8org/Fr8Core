/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface ITerminalListScope extends ng.IScope {
        terminals: Array<model.TerminalDTO>;
        openDetails(terminal: interfaces.ITerminalVM);
        showAddTerminalModal: () => void;
        showPublishTerminalModal: () => void;
    }

    class TerminalListController {

        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'TerminalService',
            '$state',
            '$modal'
        ];

        constructor(
            private $scope: ITerminalListScope,
            private TerminalService: services.ITerminalService,
            private $state: ng.ui.IStateService,
            private $modal: any) {

            $scope.showAddTerminalModal = <() => void>angular.bind(this, this.showAddTerminalModal);

            TerminalService.getAll().$promise.then(data => {
                $scope.terminals = data;
            }).catch(e => {
                console.log(e.statusText);
                });

            $scope.openDetails = terminal => {
                $state.go('terminalDetails', { id: terminal.internalId });
            }
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