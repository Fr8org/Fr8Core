/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IPlanDetailsScope extends ng.IScope {
        ptvm: interfaces.IPlanFullDTO;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        planBuilder: any,
        id: string,

        digestFlag:boolean;

        sharePlan: () => void;
        unpublishPlan: () => void;
        download: ($event: Event) => void;
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
            "$filter",
            "PusherNotifierService"
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IPlanDetailsScope,
            private PlanService: services.IPlanService,
            private $stateParams: any,
            private $filter: ng.IFilterService,
            private PusherNotifierService: dockyard.services.IPusherNotifierService) {
            
            //Load detailed information
            $scope.id = $stateParams.id;
            if (this.isValidGUID($scope.id)) {
                $scope.ptvm = PlanService.getFull({ id: $stateParams.id });
            }

            $scope.sharePlan = () => {
                PlanService.share($stateParams.id)
                    .then(() => {
                        console.log('sharePlan: Success');
                        PusherNotifierService.frontendSuccess("Plan " + $scope.ptvm.plan.name + " shared");
                    })
                    .catch((exp) => {
                        console.log('sharePlan: Failure');
                        exp.data = exp.data ? exp.data : "";
                        PusherNotifierService.frontendFailure("Plan sharing faliure: "+exp.data);
                    });
            };

            $scope.unpublishPlan = () => {
                PlanService.unpublish($stateParams.id)
                    .then(() => {
                        console.log('unpublishPlan: Success');
                        PusherNotifierService.frontendSuccess("Plan " + $scope.ptvm.plan.name + " unpublished");
                    })
                    .catch((exp) => {
                        console.log('unpublishPlan: Failure');
                        exp.data = exp.data ? exp.data : "";
                        PusherNotifierService.frontendFailure("Plan unpublished faliure: " + exp.data);
                    });
            };

            $scope.download = ($event: Event) => {
                
                if (!$scope.digestFlag) {
                    $scope.digestFlag = true;

                    var promise = PlanService.createTemplate($scope.ptvm.plan.id);
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
                            },
                            100);

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