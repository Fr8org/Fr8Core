/// <reference path="../_all.ts" />

module dockyard.controllers {
    export interface ISelectActionScope extends ng.IScope {
        activityCategories: ng.resource.IResource<interfaces.IActivityCategoryDTO[]>;
        onSolutionSelected: (solution: interfaces.IActivityCategoryDTO) => void;
    }
    export class SolutionListController {

        public static $inject = [
            '$scope',
            'ActivityTemplateService',
            '$modal',
            '$state'
        ];
        constructor(
            private $scope: ISelectActionScope,
            private ActivityTemplateService: services.IActivityTemplateService,
            private ActionService: services.IActionService,
            private $state: ng.ui.IStateService) {

            $scope.onSolutionSelected = <(solution: interfaces.IActivityCategoryDTO) => void>
                angular.bind(this, this.onSolutionSelected);

            $scope.activityCategories = ActivityTemplateService.getAvailableActivities();
        }

        private onSolutionSelected(solution: interfaces.IActivityTemplateVM) {
            this.$state.go('configureSolution', { solutionName: solution.name });
        }
    }

    app.controller('SolutionListController', SolutionListController);
}