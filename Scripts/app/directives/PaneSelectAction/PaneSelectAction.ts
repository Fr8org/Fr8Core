/// <reference path="../../_all.ts" />

module dockyard.directives.paneSelectAction {
    'use strict';

    export enum MessageType {
        PaneSelectAction_ActionUpdated,
        PaneSelectAction_Render,
        PaneSelectAction_Hide,
        PaneSelectAction_UpdateAction,
        PaneSelectAction_ActionTypeSelected,
        PaneSelectAction_ActionRemoved
    }

    export class ActionTypeSelectedEventArgs {
        public action: interfaces.IActionDesignDTO

        constructor(action: interfaces.IActionDesignDTO) {
            // Clone Action to prevent any issues due to possible mutation of source object
            this.action = angular.extend({}, action);
        }
    }

    export class ActionUpdatedEventArgs extends ActionEventArgsBase {
        public isTempId: boolean;
        public actionName: string;

        constructor(criteriaId: number, actionId: number, isTempId: boolean, actionName: string) {
            super(criteriaId, actionId);
            this.isTempId = isTempId;
            this.actionName = actionName;
        }
    }

    export class RenderEventArgs {
        public processNodeTemplateId: number;
        public id: number;
        public isTempId: boolean;
        public actionListId: number;

        constructor(
            processNodeTemplateId: number,
            id: number,
            isTemp: boolean,
            actionListId: number) {

            this.processNodeTemplateId = processNodeTemplateId;
            this.id = id;
            this.isTempId = isTemp;
            this.actionListId = actionListId;
        }
    }

    export class UpdateActionEventArgs extends ActionEventArgsBase {
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTempId: boolean) {
            super(criteriaId, actionId);
            this.isTempId = isTempId;
        }
    }

    export class ActionRemovedEventArgs {
        public id: number;
        public isTempId: boolean;

        constructor(id: number, isTempId: boolean) {
            this.id = id;
            this.isTempId = isTempId;
        }
    }


    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneSelectAction implements ng.IDirective {
        public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/PaneSelectAction';
        public controller: ($scope: ng.IScope, element: ng.IAugmentedJQuery,
            attrs: ng.IAttributes, $http: ng.IHttpService, urlPrefix: string) => void;
        public scope = {
            action: '='
        };
        public restrict = 'E';

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private ActionService: services.IActionService
            ) {

            PaneSelectAction.prototype.link = (
                scope: interfaces.IPaneSelectActionScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            PaneSelectAction.prototype.controller = (
                $scope: interfaces.IPaneSelectActionScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes,
                $http: ng.IHttpService) => {

                this.PopulateData($scope, $http);

                $scope.$watch<model.ActionDesignDTO>(
                    (scope: interfaces.IPaneSelectActionScope) => scope.action, this.onActionChanged, true);

                $scope.ActionTypeSelected = () => {
                    var eventArgs = new ActionTypeSelectedEventArgs($scope.action);
                    $scope.$emit(MessageType[MessageType.PaneSelectAction_ActionTypeSelected], eventArgs);
                }

                $scope.RemoveAction = () => {

                    $scope.$emit(
                        MessageType[MessageType.PaneSelectAction_ActionRemoved],
                        new ActionRemovedEventArgs($scope.action.id, $scope.action.isTempId)
                    );

                    if (!$scope.action.isTempId) {
                        this.ActionService.delete({
                            id: $scope.action.id
                        }); 
                    }

                    $scope.action = null;
                    $scope.isVisible = false;
                };

                $scope.$on(MessageType[MessageType.PaneSelectAction_Render], this.onRender);
                $scope.$on(MessageType[MessageType.PaneSelectAction_Hide], this.onHide);
                $scope.$on(MessageType[MessageType.PaneSelectAction_UpdateAction], this.onUpdate);
            };
        }

        private onActionChanged(newValue: model.ActionDesignDTO, oldValue: model.ActionDesignDTO, scope: interfaces.IPaneSelectActionScope) {

        }

        private onRender(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            var scope = (<interfaces.IPaneSelectActionScope> event.currentScope);
            scope.isVisible = true;
        }

        private onHide(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<interfaces.IPaneSelectActionScope>event.currentScope).isVisible = false;
        }

        private onUpdate(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<any>$).notify("Greetings from Select Action Pane. I've got a message about my neighbor saving its data so I saved my data, too.", "success");
        }

        private PopulateData(
            $scope: interfaces.IPaneSelectActionScope,
            $http: ng.IHttpService) {

            $scope.actionTypes = [];

            $http.get('/actions/available')
                .then(function (resp) {
                    angular.forEach(resp.data, function (it) {
                        console.log(it);
                        $scope.actionTypes.push(
                            new model.ActionTemplate(
                                it.id,
                                it.actionType,
                                it.version)
                            );
                    });
                });
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = (
                $rootScope: interfaces.IAppRootScope,
                ActionService: services.IActionService) => {

                return new PaneSelectAction($rootScope, ActionService);
            };

            directive['$inject'] = ['$rootScope', 'ActionService'];
            return directive;
        }
    }
    app.directive('paneSelectAction', PaneSelectAction.Factory());
}