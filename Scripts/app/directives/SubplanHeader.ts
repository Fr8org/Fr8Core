/// <reference path="../_all.ts" />
module dockyard.directives.SubplanHeader {
    'use strict';

    import planEvents = dockyard.Fr8Events.Plan;

    export interface ISubplanHeaderScope extends ng.IScope {
        subplan: model.SubPlanDTO;
        editing: boolean;
        editTitle(): void;
        onTitleChange(): void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function SubplanHeader(): ng.IDirective {
        var controller = ['$scope', 'SubPlanService', '$rootScope',($scope: ISubplanHeaderScope, subPlanService: services.ISubPlanService, $rootScope: interfaces.IAppRootScope) => {

            $scope.editTitle = () => {
                $scope.editing = true;
            };

            $scope.onTitleChange = () => {
                $scope.editing = false;
                subPlanService.update($scope.subplan);
                $rootScope.$broadcast(<any>planEvents.SUB_PLAN_MODIFICATION);
            };
            
        }];

        return {
            restrict: 'E',
            replace: true,
            templateUrl: '/AngularTemplate/SubplanHeader',
            controller: controller,
            scope: {
                subplan: '='
            }
        };
    }

    app.directive('subplanHeader', SubplanHeader);
}