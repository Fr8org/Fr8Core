/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface ITerminalDetailsScope extends ng.IScope {
        terminal: model.TerminalDTO;
        canEditAllTerminals: boolean;
        approved: boolean;
        participationStateText: string;
        openPermissionsSetterModal: (terminal: model.TerminalDTO) => void;
        save: ($event: MouseEvent, terminal: model.TerminalDTO) => void;
        prodUrlChanged: (terminal: model.TerminalDTO) => void;
        showPublishTerminalModal: () => void;
        submit: ($event: MouseEvent, isValid: boolean) => void;
        cancel: () => void;
        errorMessage: string;
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
            'TerminalService',
            'UserService',
            '$mdDialog',
            '$http',
            'StringService'
        ];

        constructor(
            private $scope: ITerminalDetailsScope,
            private $state: ng.ui.IStateService,
            private $modal: any,
            private TerminalService: services.ITerminalService,
            private UserService: services.IUserService,
            private $mdDialog: ng.material.IDialogService,
            private $http: ng.IHttpService,
            private StringService: dockyard.services.IStringService) {

            TerminalService.get({ id: $state.params['id'] }).$promise.then(function (terminal) {
                $scope.terminal = terminal;
                $scope.approved = terminal.participationState === enums.ParticipationState.Approved;
                $scope.participationStateText = dockyard.enums.ParticipationState[terminal.participationState];
            });

            // Whether user has terminal administration priviledges, show additional UI elements
            $http.get('/api/users/checkpermission', { params: { userId: (<any>window).userId, permissionType: dockyard.enums.PermissionType.EditAllObjects, objectType: 'TerminalDO' } })
                .then(function (resp) {
                    $scope.canEditAllTerminals = <boolean>resp.data;
                });

            $scope.prodUrlChanged = (terminal: model.TerminalDTO) => {
                if (terminal.prodUrl && terminal.prodUrl.length > 0) {
                    $scope.approved = true;
                }
                else {
                    $scope.approved = false;
                }
            }

            $scope.cancel = () => {
                $state.go('terminals');
            };

            $scope.submit = ($event: MouseEvent, isValid: boolean) => {
                if (!isValid) {
                    return;
                }

                if (!$scope.terminal.isFr8OwnTerminal
                    && $scope.terminal.devUrl
                    && $scope.terminal.devUrl.indexOf('localhost') >= 0) {

                    let msg: string = "";
                    if ($scope.canEditAllTerminals) {
                        msg += "For non-Fr8 own terminals ";
                    }
                    msg += 'Development URL' + this.StringService.terminal["localhost_dev"];
                    $scope.errorMessage = msg;
                    return;
                }

                if ($scope.terminal.prodUrl && $scope.terminal.prodUrl.indexOf('localhost') >= 0) {
                    $scope.errorMessage = 'Production URL' + this.StringService.terminal["localhost_prod"];
                    return;
                }
                //if (!$scope.canEditAllTerminals) {
                //    if ($scope.terminal.devUrl.indexOf('localhost') >= 0) {
                //        let confirmDialog = this.$mdDialog.alert()
                //            .title('Input error')
                //            .textContent('Endpoint URL cannot contain the string "localhost".')
                //            .targetEvent($event)
                //            .ok('Ok');
                //        this.$mdDialog.show(confirmDialog);
                //        return;
                //    }
                //}
                if ($scope.approved) {
                    if ($scope.terminal.participationState != enums.ParticipationState.Approved) {
                        this.showConfigurationDialog($event, "approve", $scope.terminal.name).then(() => {
                            $scope.terminal.participationState = enums.ParticipationState.Approved;
                            this.saveTerminal($scope.terminal);
                        });
                    }
                    else {
                        this.saveTerminal($scope.terminal);
                    }
                }
                else {
                    if ($scope.terminal.participationState == enums.ParticipationState.Approved) {
                        this.showConfigurationDialog($event, "recall your approval of", $scope.terminal.name).then(() => {
                            $scope.terminal.participationState = enums.ParticipationState.Unapproved;
                            this.saveTerminal($scope.terminal);
                        });
                    }
                    else {
                        this.saveTerminal($scope.terminal);
                    }
                }
            }

            $scope.showPublishTerminalModal = () => {
                $modal.open({
                    animation: true,
                    templateUrl: '/AngularTemplate/TerminalPublishForm',
                    controller: ['$scope', '$modalInstance', function ($scope, $modalInstance) {
                        $scope.cancel = () => { $modalInstance.dismiss('cancel'); }
                    }]
                })
            }
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

        private saveTerminal(terminal: model.TerminalDTO) {
            let that = this;
            this.TerminalService.save(terminal).$promise.then(() => {
                this.$state.go('terminals');
            })
                .catch((e) => {
                    console.log('Terminal update failed: ' + e.data.message);
                    switch (e.status) {
                        case 400:
                            that.$scope.errorMessage = that.StringService.terminal["error400"];
                            if (e.data.message) {
                                that.$scope.errorMessage += " Additional information: " + e.data.message;
                            }
                            break;
                        case 404:
                            that.$scope.errorMessage = that.StringService.terminal["error404"];
                            break;
                        case 409:
                            that.$scope.errorMessage = that.StringService.terminal["error409"];
                            break;
                        default:
                            that.$scope.errorMessage = that.StringService.terminal["error"];
                            break;
                    }
                });
        }

        private showConfigurationDialog(event: MouseEvent, action: string, terminalName: string) {
            // Appending dialog to document.body to cover sidenav in docs app
            let confirmDialog = this.$mdDialog.confirm()
                .title('Terminal participation state change')
                .textContent('Are you sure that you wish to ' + action + ' ' + terminalName + '?')
                .targetEvent(event)
                .ok('Yes')
                .cancel('No');
            return this.$mdDialog.show(confirmDialog);
        }
    }

    app.controller('TerminalDetailsController', TerminalDetailsController);
    app.controller('PermissionsSetterModalController', ['$scope', '$modalInstance', 'terminal', ($scope: any, $modalInstance: any, terminal: model.TerminalDTO): void => {

        $scope.terminal = terminal;

        $scope.submitForm = () => {
            $modalInstance.close($scope.label);
        };

        $scope.cancel = () => {
            $modalInstance.dismiss();
        };

    }]);
}