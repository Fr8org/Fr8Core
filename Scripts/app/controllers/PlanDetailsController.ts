/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPlanDetailsScope extends IMainPlanScope {
        ptvm: interfaces.IPlanVM;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        planBuilder: any,
        id: string,

        digestFlag:boolean;

        sharePlan: () => void;
        unpublishPlan: () => void;
        download: ($event: Event) => void;
        descriptionEditing: boolean;
        nameEditing: boolean;
        onTitleChange(): void;
    }

    class PlanDetailsController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$rootScope',
            '$scope',
            'PlanService',
            '$stateParams',
            '$filter',
            'UINotificationService'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IPlanDetailsScope,
            private PlanService: services.IPlanService,
            private $stateParams: any,
            private $filter: ng.IFilterService,
            private uiNotificationService: dockyard.interfaces.IUINotificationService) {

            $scope.descriptionEditing = false;
            $scope.nameEditing = false;
            // Load detailed information
            $scope.id = $stateParams.id;

            if (this.isValidGUID($scope.id)) {
                PlanService.getFull({ id: $stateParams.id }).$promise.then(function (plan) {
                    $scope.current.plan = (<any>plan);
                });
            }

            $scope.sharePlan = () => {
                if (!$scope.current.plan.visibility.public) {
                    PlanService.share($stateParams.id)
                    .catch((exp) => {
                        exp.data = exp.data ? exp.data : "";
                    });
                }
            };

            $scope.onTitleChange = () => {
                $scope.descriptionEditing = false;
                $scope.nameEditing = false;
                var result = PlanService.update({ id: $scope.current.plan.id, name: $scope.current.plan.name, description: $scope.current.plan.description });
                result.$promise.then(() => { });
            };

            $scope.unpublishPlan = () => {
                //tony.yakovets: temporary crutch
                if (!$scope.current.plan.visibility.hidden) {
                    PlanService.unpublish($stateParams.id).catch((exp) =>
                    {
                        exp.data = exp.data ? exp.data : "";
                    });
                }
            };

            $scope.download = ($event: Event) => {
                
                if (!$scope.digestFlag) {
                    $scope.digestFlag = true;

                    var promise = PlanService.createTemplate($scope.current.plan.id);
                    var element = $event.target;

                    promise.then((template) => {
                        let json = $filter('json')(template);
                        let data = new Blob([json]);
                        let href = URL.createObjectURL(data);

                        (element as HTMLAnchorElement).href = href;
                        window.setTimeout(() => {
                                (element as HTMLAnchorElement).click();
                                $scope.digestFlag = false;
                                (element as HTMLAnchorElement).removeAttribute("href");
                        }, 100);
                    });
                }

                $event.stopPropagation = null;
                $event.preventDefault = () => { };
                $event.cancelBubble = false;
                $event.returnValue = true;    
            };
        }

        // Regular Expression reference link
        // https://lostechies.com/gabrielschenker/2009/03/10/how-to-add-a-custom-validation-method-to-the-jquery-validator-plug-in/
        private isValidGUID(GUID) {
            var validGuid = /^({|()?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}(}|))?$/;
            var emptyGuid = /^({|()?0{8}-(0{4}-){3}0{12}(}|))?$/;
            return validGuid.test(GUID) && !emptyGuid.test(GUID);
        }
    }

    app.controller('PlanDetailsController', PlanDetailsController);
}