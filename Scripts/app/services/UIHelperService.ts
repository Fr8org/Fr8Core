/// <reference path="../_all.ts" />

/*
    The service implements centralized string storage.
*/

module dockyard.services {

    export interface IUIHelperService {
        openConfirmationModal: (message: string) => ng.IPromise<void>
    }

    class UIHelperService implements IUIHelperService {
        constructor(private $modal: any) {
            
        }

        public openConfirmationModal(message: string) {
            return this.$modal.open({
                animation: true,
                templateUrl: 'AngularTemplate/ConfirmationModal',
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