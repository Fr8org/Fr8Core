/// <reference path="../_all.ts" />
module dockyard.directives.ActivityHeader {
    'use strict';

    export interface IActivityHeaderScope extends ng.IScope {
        envelope: model.ActivityEnvelope;
        openMenu: ($mdOpenMenu: any, ev: any) => void;
        reConfigureAction: (action: model.ActivityDTO) => void;
        openAddLabelModal: (action: model.ActivityDTO) => void;
        hasHelpMenuItem: (activity: model.ActivityDTO) => boolean;
        showActivityHelpDocumentation: (activity: model.ActivityDTO) => void;
        deleteAction: (action: model.ActivityDTO) => void;
        chooseAuthToken: (action: model.ActivityDTO) => void;
        planState: number;
        finishedLoading: () => void;
        myActivityTemplate: interfaces.IActivityTemplateVM;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function ActivityHeader(): ng.IDirective {
        var controller = ['$scope', 'ActionService', '$modal', 'SolutionDocumentationService', ($scope: IActivityHeaderScope, ActionService: services.IActionService, $modal: any, documentationService: services.ISolutionDocumentationService) => {
            $scope.openMenu = ($mdOpenMenu, ev) => {
                $mdOpenMenu(ev);
            };
            $scope.openAddLabelModal = (action: model.ActivityDTO) => {
                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: '/AngularTemplate/ActivityLabelModal',
                    controller: 'ActivityLabelModalController',
                    resolve: {
                        label: () => action.label
                    }
                });
                modalInstance.result.then(function (label: string) {
                    action.label = label;
                    ActionService.save(action);
                });
            }
            $scope.hasHelpMenuItem = (activity) => {
                var at = $scope.envelope.activityTemplate;
                if (at.showDocumentation != null) {
                    if (at.showDocumentation.body.displayMechanism != undefined &&
                        at.showDocumentation.body.displayMechanism.contains("HelpMenu")) {
                        return true;
                    }
                }
                return false;
            }
            $scope.showActivityHelpDocumentation = (activity) => {
                var activityDTO = new model.ActivityDTO("", "", "");
                activityDTO.toActionVM();
                activityDTO.documentation = "HelpMenu";
                activityDTO.activityTemplate = activity.activityTemplate;

                documentationService.getDocumentationResponseDTO(activityDTO).$promise.then(data => {
                    if (data) {
                        var newWindow = this.$window.open();
                        newWindow.document.writeln(data.body);
                    }
                });
            }

            angular.element(document.querySelector('.col-sm-8.ellipsis')).ready(function () {
                window.setTimeout(function () {
                    $scope.$emit('titleLoadingFinished');
                }, 500);
            });          

        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ActivityHeader',
            require: '^PlanBuilderController',
            controller: controller,
            replace: true,
            scope: {
                envelope: '=',
                reConfigureAction: '=',
                deleteAction: '=',
                chooseAuthToken: '=',
                allowDrag: '=',
                planState: '='
            }
        };
    }

    app.directive('activityHeader', ActivityHeader);
}