/// <reference path="../_all.ts" />

/*
    The service implements centralized alert Dialog storage.
*/

module dockyard.services {

    export interface IUIHelperService {
        openConfirmationModal: (message: model.AlertDTO) => ng.IPromise<void>
    }

    // Use this for Showing alert mechanism 
    class UIHelperService implements IUIHelperService {
        constructor(private $modal: any) {
        }

        public openConfirmationModal(message: model.AlertDTO) {
            return this.$modal.open({
                animation: true,
                templateUrl: '/AngularTemplate/ConfirmationModal',
                //this is a simple modal controller, so i didn't have an urge to seperate this
                //but resolve is used to make future seperation easier
                controller: ['$modalInstance', '$scope', 'modalMessage', ($modalInstance: any, $modalScope: any, modalMessage: string) => {
                    $modalScope.message = message;
                    $modalScope.confirm = () => {
                        $modalInstance.close();
                    };
                    $modalScope.cancel = () => {
                        $modalInstance.dismiss();
                    };
                }],
                resolve: {
                    'modalMessage': () => message
                }
            }).result;
        }
    }

    app.factory('UIHelperService', ['$modal', ($modal: any): IUIHelperService => new UIHelperService($modal)]);
}