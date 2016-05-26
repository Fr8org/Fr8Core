/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface ISelectActionScope extends ng.IScope {
        activityCategories: ng.resource.IResource<interfaces.IActivityCategoryDTO[]>;
        onSolutionSelected: (solution: interfaces.IActivityCategoryDTO, $window) => void;
    }

    export class SolutionListController {

        public static $inject = [
            '$scope',
            'ActivityTemplateService',
            '$modal',
            '$state',
            '$window'
        ];
        constructor(
            private $scope: ISelectActionScope,
            private ActivityTemplateService: services.IActivityTemplateService,
            private ActionService: services.IActionService,
            private $state: ng.ui.IStateService,
            private $window: Window) {

            $scope.onSolutionSelected = <(solution: interfaces.IActivityCategoryDTO, $window) => void>
                angular.bind(this, this.onSolutionSelected);

            $scope.activityCategories = ActivityTemplateService.getAvailableActivities();
        }

        private onSolutionSelected(solution: interfaces.IActivityTemplateVM, $window) {
            this.$state.go('configureSolution', { solutionName: solution.name });
            $window.analytic.track("Loaded Solution", { "Solution Name": solution.name });
        }
    }

    app.controller('SolutionListController', SolutionListController);
}