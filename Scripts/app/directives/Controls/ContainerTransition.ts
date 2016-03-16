/// <reference path="../../_all.ts" />
module dockyard.directives.containerTransition {
    import ContainerTransitions = dockyard.model.ContainerTransitions;
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
        PCA: directives.paneConfigureAction.IPaneConfigureActionController;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function ContainerTransition(): ng.IDirective {

        
        var controller = ['$scope', '$timeout', 'RouteService', ($scope: IContainerTransitionScope, $timeout: ng.ITimeoutService, RouteService: services.IRouteService) => {

            var planOptions = new Array<model.DropDownListItem>();

            //let's load and keep all plans in cache
            //TODO think about this - maybe we need to request data from PCA or PB
            //this direct access will create unnecessary requests to server
            RouteService.getbystatus({ id: null, status: null, category: '' }).$promise.then((plans: Array<model.RouteDTO>) => {
                for (var i = 0; i < plans.length; i++) {
                    planOptions.push(new model.DropDownListItem(plans[i].name, plans[i].id));
                }
            });

            var operationList = [
                new model.DropDownListItem('Jump To Activity', ContainerTransitions.JumpToActivity.toString()),
                new model.DropDownListItem('Launch Plan', ContainerTransitions.JumpToPlan.toString()),
                new model.DropDownListItem('Jump To Subplan', ContainerTransitions.JumpToSubplan.toString()),
                new model.DropDownListItem('Stop Processing', ContainerTransitions.StopProcessing.toString()),
                //new model.DropDownListItem('Suspend Processing', ContainerTransitions.SuspendProcessing.toString()),
                //new model.DropDownListItem('Proceed To Next Activity', ContainerTransitions.ProceedToNextActivity.toString())
            ];


            //TODO remove this to a helper service
            var searchAction = (id: string, actionList: model.ActivityDTO[]): model.ActivityDTO => {
                for (var i = 0; i < actionList.length; i++) {
                    if (actionList[i].id === id) {
                        return actionList[i];
                    }
                    if (actionList[i].childrenActivities.length) {
                        var foundAction = searchAction(id, <model.ActivityDTO[]>actionList[i].childrenActivities);
                        if (foundAction !== null) {
                            return foundAction;
                        }
                    }
                }
                return null;
            }

            //TODO remove this to a helper service
            var findActionById = (id: string): model.ActivityDTO => {
                for (var subroute of $scope.route.subroutes) {
                    var foundAction = searchAction(id, subroute.activities);
                    if (foundAction !== null) {
                        return foundAction;
                    }
                }

                return null;
            }

            
            
            var findSubPlanById = (id: string) : model.SubrouteDTO =>  {
                for(var i = 0; i< $scope.route.subroutes.length; i++) {
                    if($scope.route.subroutes[i].id === id) {
                        return $scope.route.subroutes[i];
                    }
                }
                //it seems this id belongs to an action
                //lets look one level up
                var action = findActionById(id);
                if (action != null) {
                    return findSubPlanById(action.parentRouteNodeId);
                }
                return null;
            }

            

            //Alex requested that lower level of current activity
            //shouldn't be listed as a jump target
            //we are checking if this is the level we should stop
            //each level = ActivityGroup
            //each children creates a level
            var isThisCurrentLevel = (activity: model.ActivityDTO): boolean => {
                return $scope.currentAction.parentRouteNodeId === activity.parentRouteNodeId;
            }

            var getActivityTree = (activity: model.ActivityDTO) => {
                var childActivities = new Array<model.ActivityDTO>();
                if (activity.childrenActivities && activity.childrenActivities.length > 0) {
                    for (var i = 0; i < activity.childrenActivities.length; i++) {
                        childActivities.push(<model.ActivityDTO>activity.childrenActivities[i]);
                        if (!isThisCurrentLevel(<model.ActivityDTO>activity.childrenActivities[i])){
                            childActivities.concat(getActivityTree(<model.ActivityDTO>activity.childrenActivities[i]));
                        }
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
                    if (!isThisCurrentLevel(subplan.activities[i])){
                        var childActivityTree = getActivityTree(current);
                        for (var j = 0; j < childActivityTree.length; j++) {
                            subplanActivities.push(new model.DropDownListItem(childActivityTree[j].label, childActivityTree[j].id));
                        }
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

            var buildPlanDropdown = () => {
                var dd = new model.DropDownList();
                dd.label = "Select Target Plan";
                dd.listItems = planOptions;
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
                        
                        (<any>transition)._dummySecondaryOperationDD = buildActivityDropdown();
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
            var buildJumpTargets = (): Array<model.ActivityJumpTarget> => {
                var targets = new Array<model.ActivityJumpTarget>();
                for (var i = 0; i < $scope.field.transitions.length; i++) {
                    var target = new model.ActivityJumpTarget();
                    target.TransitionType = $scope.field.transitions[i].transition;
                    target.Target = '';
                    var dd = <model.DropDownList>(<any>$scope.field.transitions[i])._dummyOperationDD;
                    
                    target.Target = dd.selectedKey;
                    var dd2 = <model.DropDownList>(<any>$scope.field.transitions[i])._dummySecondaryOperationDD;
                    if (dd2 && dd2.selectedKey && dd2.selectedKey.length > 0) {
                        target.Target += ': ' + dd2.selectedKey;
                    }

                    targets.push(target);
                }
                return targets;
            };

            var informJumpTargets = () => {
                var targets = buildJumpTargets();
                $scope.PCA.setJumpTargets(targets);
            };

            var triggerChange = () => {
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }

                informJumpTargets();
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
                triggerChange();
            };

            //let's add initial transition
            if ($scope.field.transitions.length < 1) {
                $scope.addTransition();
            }

            $timeout(informJumpTargets);
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/ContainerTransition',
            require: '^paneConfigureAction',
            link: (scope: IContainerTransitionScope, element, attrs, PCA: directives.paneConfigureAction.IPaneConfigureActionController) => {
                scope.PCA = PCA;
            },
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