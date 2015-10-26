/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';
    import pca = dockyard.directives.paneConfigureAction;

    export interface ISolutionScope extends ng.IScope {
        action: model.ActionDTO;
        route: model.RouteDTO;
        onSolutionSelected: (solution: interfaces.IActivityCategoryDTO) => void;
        processing: boolean;
    }

    export class SolutionController {

        public static $inject = [
            '$scope',
            'ActionService',
            '$state',
            '$stateParams'
        ];
        constructor(
            private $scope: ISolutionScope,
            private ActionService: services.IActionService,
            private $state: ng.ui.IStateService,
            private $stateParams: ng.ui.IStateParamsService) {

            $scope.$on(pca.MessageType[pca.MessageType.PaneConfigureAction_ChildActionsDetected], () => {
                this.onChildActionsDetected();
            });

            $scope.action = new model.ActionDTO(0, 0, false);

            var solutionName = $stateParams["solutionName"];
            if (!solutionName) {
                alert('No solution name is specified');
                return;
            }

            $scope.processing = true;
            var route = ActionService.createSolution({
                solutionName: solutionName
            });
            route.$promise.then((result) => {
                console.log(result);
                $scope.route = result;
                $scope.action = result.subroutes[0].actions[0]; // get the only first action
                $scope.$broadcast(pca.MessageType[pca.MessageType.PaneConfigureAction_RenderConfiguration]);
            })
                .finally(() => {
                    $scope.processing = false;
                });
        }

        private onChildActionsDetected() {
            this.$state.transitionTo('processBuilder', { id: this.$scope.route.id });
        }

    }

    app.controller('SolutionController', SolutionController);
} 