/// <reference path="../../_all.ts" />
module dockyard.directives.containerTransition {
    'use strict';

    interface IContainerTransitionScope extends ng.IScope {
        route: model.RouteDTO;
        field: model.ContainerTransition;
        addTransition: () => void;
        getOperationField: (transition: model.ContainerTransitionField) => model.DropDownList;
        onOperationChange: (transition: model.ContainerTransitionField) => void;
        onTargetChange: (transition: model.ContainerTransitionField) => void;
        change: () => (field: model.ControlDefinitionDTO) => void;
        currentAction: model.ActivityDTO;
        removeTransition: (index: number) => void;
    }

    export class ContainerTransitions {
        public static get JumpToActivity(): number { return 0; };
        public static get JumpToPlan(): number { return 1; };
        public static get JumpToSubplan(): number { return 2; };
        public static get StopProcessing(): number { return 3; };
        public static get SuspendProcessing(): number { return 4; };
        public static get ProceedToNextActivity(): number { return 5; };        
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function ContainerTransition(): ng.IDirective {

        
        var controller = ['$scope', '$element', '$attrs', ($scope: IContainerTransitionScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {

            var operationList = [
                new model.DropDownListItem('Jump To Activity', ContainerTransitions.JumpToActivity.toString()),
                //new model.DropDownListItem('Jump To Plan', ContainerTransitions.JumpToPlan.toString()),
                new model.DropDownListItem('Jump To Subplan', ContainerTransitions.JumpToSubplan.toString()),
                new model.DropDownListItem('Stop Processing', ContainerTransitions.StopProcessing.toString()),
                //new model.DropDownListItem('Suspend Processing', ContainerTransitions.SuspendProcessing.toString()),
                new model.DropDownListItem('Proceed To Next Activity', ContainerTransitions.ProceedToNextActivity.toString())
            ];

            var triggerChange = () => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
            };

            var findSubPlanById = (id: string) =>  {
                for(var i = 0; i< $scope.route.subroutes.length; i++) {
                    if($scope.route.subroutes[i].id === id) {
                        return $scope.route.subroutes[i];
                    }
                }
                return null;
            }

            var getActivityTree = (activity: model.ActivityDTO) => {
                var childActivities = new Array<model.ActivityDTO>();
                if (activity.childrenActivities && activity.childrenActivities.length > 0) {
                    for (var i = 0; i < activity.childrenActivities.length; i++) {
                        childActivities.push(<model.ActivityDTO>activity.childrenActivities[i]);
                        childActivities.concat(getActivityTree(<model.ActivityDTO>activity.childrenActivities[i]));
                    }
                }

                return childActivities;
            };

            var getCurrentSubplanActivities = () => {
                var currentSubplanId = $scope.currentAction.parentRouteNodeId;
                var subplan = findSubPlanById(currentSubplanId);
                if (subplan == null) {
                    throw 'Unable to find subplan with id ' + currentSubplanId;
                }

                var subplanActivities = new Array<model.DropDownListItem>();
                for (var i = 0; i < subplan.activities.length; i++) {
                    var current = subplan.activities[i];
                    subplanActivities.push(new model.DropDownListItem(current.label, current.id));
                    var childActivityTree = getActivityTree(current);
                    for (var j = 0; j < childActivityTree.length; j++) {
                        subplanActivities.push(new model.DropDownListItem(childActivityTree[j].label, childActivityTree[j].id));
                    }
                }

                //let's exclude current activity from that list
                for (var i = 0; i < subplanActivities.length; i++) {
                    if (subplanActivities[i].value === $scope.currentAction.id) {
                        subplanActivities.splice(i, 1);
                        break;
                    }
                }

                return subplanActivities;
            };

            var getCurrentSubplans = () => {
                var subplans = new Array<model.DropDownListItem>();
                for (var i = 0; i < $scope.route.subroutes.length; i++) {
                    subplans.push(new model.DropDownListItem($scope.route.subroutes[i].name, $scope.route.subroutes[i].id));
                }

                return subplans;
            };

            var buildActivityDropdown = () => {
                var dd = new model.DropDownList();
                dd.label = "Select Target Activity";
                dd.listItems = getCurrentSubplanActivities();
                dd.value = null;
                dd.selectedKey = null;
                return dd;
            };

            var buildSubplanDropdown = () => {
                var dd = new model.DropDownList();
                dd.label = "Select Target Subplan";
                dd.listItems = getCurrentSubplans();
                dd.value = null;
                dd.selectedKey = null;
                return dd;
            };

            var processTransition = (transition: model.ContainerTransitionField) => {
                switch (transition.transition) {
                    case ContainerTransitions.JumpToActivity:
                        (<any>transition)._dummySecondaryOperationDD = buildActivityDropdown();
                        break;
                    case ContainerTransitions.JumpToPlan:
                        //TODO implement this

                        break;
                    case ContainerTransitions.JumpToSubplan:
                        (<any>transition)._dummySecondaryOperationDD = buildSubplanDropdown();
                        break;
                    case ContainerTransitions.StopProcessing:
                    case ContainerTransitions.SuspendProcessing:
                    case ContainerTransitions.ProceedToNextActivity:
                    default:
                        delete (<any>transition)._dummySecondaryOperationDD;
                        break;
                }
            };

            $scope.addTransition = () => {
                $scope.field.transitions.push(new model.ContainerTransitionField());
            };

            $scope.onTargetChange = (transition: model.ContainerTransitionField) => {
                var dd = <model.DropDownList>(<any>transition)._dummySecondaryOperationDD;
                transition.targetNodeId = dd.value;
                triggerChange();
                return angular.noop;
            };

            $scope.onOperationChange = (transition: model.ContainerTransitionField) => {
                var dd = <model.DropDownList>(<any>transition)._dummyOperationDD;
                transition.transition = parseInt(dd.value);
                processTransition(transition);
                triggerChange();
                return angular.noop;
            };

            $scope.getOperationField = (transition: model.ContainerTransitionField) => {
                if (!(<any>transition)._dummyOperationDD) {
                    var dd = new model.DropDownList();
                    dd.listItems = operationList;
                    (<any>transition)._dummyOperationDD = dd;
                }
                
                return (<any>transition)._dummyOperationDD;
            };
            $scope.removeTransition = (index: number) => {
                $scope.field.transitions.splice(index, 1);
            };

            //let's add initial transition
            if ($scope.field.transitions.length < 1) {
                $scope.addTransition();
            }
        }];

        //The factory function returns Directive object as per Angular requirements
        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ContainerTransition',
            controller: controller,
            scope: {
                route: '=',
                field: '=',
                currentAction: '='
            }
        };
    }

    app.directive('containerTransition', ContainerTransition);
}