/// <reference path="../../_all.ts" />

module dockyard.directives.paneSelectAction {
    'use strict';

    export interface IPaneSelectActionScope extends ng.IScope {
        onActionChanged: (newValue: model.ActionDesignDTO, oldValue: model.ActionDesignDTO, scope: IPaneSelectActionScope) => void;
        currentAction: model.ActionDesignDTO;
        isVisible: boolean;
        actionTypes: Array<model.ActivityTemplate>;
        ActionTypeSelected: () => void;
        RemoveAction: () => void;
        componentActivities: string[];
        ChildActivityTypeSelected: (actionTemplateId: number) => void;
        childActivityStepId: number;
        childActivity: model.ActionDesignDTO;
    }

    export enum MessageType {
        PaneSelectAction_ActionUpdated,
        PaneSelectAction_Render,
        PaneSelectAction_Hide,
        PaneSelectAction_UpdateAction,
        PaneSelectAction_ActionTypeSelected,
        PaneSelectAction_ActionRemoved,
        PaneSelectAction_InitiateSaveAction
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
            currentAction: '='
        };
        public restrict = 'E';

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private ActionService: services.IActionService
            ) {

            PaneSelectAction.prototype.link = (
                scope: IPaneSelectActionScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            PaneSelectAction.prototype.controller = (
                $scope: IPaneSelectActionScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes,
                $http: ng.IHttpService) => {

                this.PopulateData($scope, $http);

                $scope.$watch<model.ActionDesignDTO>(
                    (scope: IPaneSelectActionScope) => scope.currentAction, this.onActionChanged, true);

                $scope.ActionTypeSelected = () => {
                    //debugger;
                    var currentSelectedActivity: model.ActivityTemplate;
                    var activities = $scope.actionTypes;
                    //find the selected activity
                    currentSelectedActivity = activities.filter(function (e) { return e.id == $scope.currentAction.activityTemplateId })[0];

                    if (currentSelectedActivity != null || currentSelectedActivity != undefined) {
                        // Ensure that we do not send CrateStorage of previously selected storage to server.
                        $scope.currentAction.crateStorage = new model.CrateStorage();

                        //Check for component activity
                        if (currentSelectedActivity.componentActivities != null) {
                            var componentActivities = angular.fromJson(currentSelectedActivity.componentActivities);
                            $scope.componentActivities = componentActivities;                           
                            //Default configuration for the first child component activity will be shown
                            $scope.childActivityStepId = componentActivities[0].id;
                            $scope.childActivity = angular.extend({}, $scope.currentAction);
                            $scope.childActivity.activityTemplateId = $scope.childActivityStepId;
                            var eventArgs = new ActionTypeSelectedEventArgs($scope.childActivity);
                            $scope.$emit(MessageType[MessageType.PaneSelectAction_ActionTypeSelected], eventArgs);
                        }
                        else {
                            $scope.componentActivities = null;
                            var eventArgs = new ActionTypeSelectedEventArgs($scope.currentAction);
                            $scope.$emit(MessageType[MessageType.PaneSelectAction_ActionTypeSelected], eventArgs);
                        }
                    }
                    else {
                        $scope.componentActivities = null;                     
                    }
                }

                $scope.RemoveAction = () => {
                    $scope.$emit(
                        MessageType[MessageType.PaneSelectAction_ActionRemoved],
                        new ActionRemovedEventArgs($scope.currentAction.id, $scope.currentAction.isTempId)
                    );

                    if (!$scope.currentAction.isTempId) {
                        this.ActionService.delete({
                            id: $scope.currentAction.id
                        }); 
                    }

                    $scope.currentAction = null;
                    $scope.isVisible = false;
                };

                $scope.ChildActivityTypeSelected = (childActionTemplateId) => {
                    if (childActionTemplateId != null) {
                        $scope.$emit(MessageType[MessageType.PaneSelectAction_InitiateSaveAction], eventArgs);
                        $scope.childActivity.activityTemplateId = childActionTemplateId;
                        var eventArgs = new ActionTypeSelectedEventArgs($scope.childActivity);
                        $scope.$emit(MessageType[MessageType.PaneSelectAction_ActionTypeSelected], eventArgs);

                    }
                }

                $scope.$on(MessageType[MessageType.PaneSelectAction_Render], this.onRender);
                $scope.$on(MessageType[MessageType.PaneSelectAction_Hide], this.onHide);
                $scope.$on(MessageType[MessageType.PaneSelectAction_UpdateAction], this.onUpdate);
            };
        }

        private onActionChanged(newValue: model.ActionDesignDTO, oldValue: model.ActionDesignDTO, scope: IPaneSelectActionScope) {

        }

        private onRender(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            var scope = (<IPaneSelectActionScope> event.currentScope);
            scope.isVisible = true;
        }

        private onHide(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<IPaneSelectActionScope>event.currentScope).isVisible = false;
        }

        private onUpdate(event: ng.IAngularEvent, eventArgs: RenderEventArgs) {
            (<any>$).notify("Greetings from Select Action Pane. I've got a message about my neighbor saving its data so I saved my data, too.", "success");
        }

        private PopulateData(
            $scope: IPaneSelectActionScope,
            $http: ng.IHttpService) {

            $scope.actionTypes = [];

            $http.get('/activities/available')
                .then(function (resp) {
                    angular.forEach(resp.data, function (it) {
                        console.log(it);
                        $scope.actionTypes.push(
                            new model.ActivityTemplate(
                                it.id,
                                it.name,
                                it.version,
                                it.componentActivities
                                )
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