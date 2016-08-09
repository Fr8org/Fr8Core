/// <reference path="../_all.ts" />
module dockyard.directives.UIBlocker {
    'use strict';
    import designHeaderEvents = dockyard.Fr8Events.DesignerHeader;

    export interface IUIBlockerScope extends ng.IScope {
        plan: model.PlanDTO;
        showPlanIsDisabledDialog: () => void;
        deactivatePlan: () => void;
        active: boolean;
    }

    export function UIBlocker(): ng.IDirective {
        var controller = ['$scope', '$rootScope', 'UIHelperService', 'PlanService', ($scope: IUIBlockerScope, $rootScope: interfaces.IAppRootScope, UIHelperService: services.IUIHelperService, PlanService: services.IPlanService) => {
            var alertMessage = new model.AlertDTO();
            alertMessage.title = "Plan is disabled";
            alertMessage.isOkCancelVisible = true;

            $scope.showPlanIsDisabledDialog = () => {
                if ($scope.plan.planState === model.PlanState.Active) {
                    alertMessage.body = "You can't modify this Plan while it's running. Would you like to stop it now?"
                    UIHelperService
                        .openConfirmationModal(alertMessage)
                        .then(() => {
                            $scope.deactivatePlan();
                        });
                }
                else {
                    alertMessage.body = "You can't modify this Plan while it's running."
                    UIHelperService.openConfirmationModal(alertMessage);
                }
            }

            $scope.deactivatePlan = () => {
                var result = PlanService.deactivate({ planId: $scope.plan.id });
                result.$promise.then((data) => {
                    $scope.plan.planState = model.PlanState.Inactive;
                    $rootScope.$broadcast(<any>designHeaderEvents.PLAN_IS_DEACTIVATED);
                });
            };
            
        }];

        return {
            transclude: true,
            template:  '<div ng-if="active" ng-click="showPlanIsDisabledDialog()" style="width:100%;height:100%;position:absolute;background:#505050;top:0;left:0;z-index:1000;opacity:0.2;"></div>' +
                       '<ng-transclude></ng-translude>',
            controller: controller,
            scope: {
                active: '=',
                plan: '='
            }
        };
    }
    app.directive('uiBlocker', UIBlocker);
}