/// <reference path="../_all.ts" />

module dockyard.controllers {
    export interface ISelectActionScope extends ng.IScope {
        activityCategories: interfaces.IActivityCategoryDTO[];
        onSolutionSelected: (solution: interfaces.IActivityCategoryDTO) => void;
    }
    export class SolutionListController {

        public static $inject = [
            '$scope',
            'ActivityTemplateHelperService',
            '$modal',
            '$state'
        ];
        constructor(
            private $scope: ISelectActionScope,
            private ActivityTemplateHelperService: services.IActivityTemplateHelperService,
            private ActionService: services.IActionService,
            private $state: ng.ui.IStateService) {

            $scope.onSolutionSelected = <(solution: interfaces.IActivityCategoryDTO) => void>
                angular.bind(this, this.onSolutionSelected);

            $scope.activityCategories = ActivityTemplateHelperService.getAvailableActivityTemplatesInCategories();
        }

        private onSolutionSelected(solution: interfaces.IActivityTemplateVM) {
            this.$state.go('configureSolution', { solutionName: solution.name });
        }
    }

    app.controller('SolutionListController', SolutionListController);
}