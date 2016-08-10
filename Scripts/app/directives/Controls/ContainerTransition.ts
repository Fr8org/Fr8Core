/// <reference path="../../_all.ts" />
module dockyard.directives.containerTransition {
    import ContainerTransitions = dockyard.model.ContainerTransitions;
    import pca = dockyard.directives.paneConfigureAction;
    import planEvents = dockyard.Fr8Events.Plan;
    'use strict';

    interface IContainerTransitionScope extends ng.IScope {
        plan: model.PlanDTO;
        field: model.ContainerTransition;
        addTransition: () => void;
        getOperationField: (transition: model.ContainerTransitionField) => model.DropDownList;
        onOperationChange: (transition: model.ContainerTransitionField) => void;
        onTargetChange: (transition: model.ContainerTransitionField) => void;
        change: () => (field: model.ControlDefinitionDTO) => void;
        subPlan: model.SubPlanDTO;
        currentAction: model.ActivityDTO;
        removeTransition: (index: number) => void;
        PCA: directives.paneConfigureAction.IPaneConfigureActionController;
        isDisabled: boolean;
        reconfigure: () => void;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function ContainerTransition(): ng.IDirective {

        var warningMessageTemplate =
            '<div class="modal-body">\
                <p>\
                    <div>Be careful!</div>\
                    <div>It\'s very easy to create an infinite loop when you jump back to the beginning of the subplan that you\'re part of. Make sure that you have a way to break out of this loop. Otherwise this Plan will likely get shut down for overconsumption of Fr8 resources.</div>\
                </p>\
            </div>\
            <div class="modal-footer">\
                <button class="btn btn-default" ng-click="$dismiss()">Okay</button>\
            </div>';

        var controller = ['$scope', '$timeout', 'PlanService', '$modal', 'ActivityTemplateHelperService', ($scope: IContainerTransitionScope, $timeout: ng.ITimeoutService, PlanService: services.IPlanService, $modal: any, ActivityTemplateHelperService: services.IActivityTemplateHelperService) => {

            var planOptions = new Array<model.DropDownListItem>();

            $scope.reconfigure = () => {
                $scope.$emit(pca.MessageType[pca.MessageType.PaneConfigureAction_Reconfigure], new pca.ActionReconfigureEventArgs($scope.currentAction));
            };

            //let's load and keep all plans in cache
            //TODO think about this - maybe we need to request data from PCA or PB
            //this direct access will create unnecessary requests to server
            PlanService.getbystatus({ id: null, status: null, category: '', orderBy: "name" }).$promise.then((result: any) => {
                var plans = result.plans;
                for (var i = 0; i < plans.length; i++) {
                    var plan = plans[i];
                    if (plan.id === $scope.plan.id) {
                        plan.name = 'Jump back to the start of this plan';
                        planOptions.unshift(new model.DropDownListItem(plan.name, plan.id));
                    }
                    else {
                        planOptions.push(new model.DropDownListItem(plan.name, plan.id));
                    }                    
                }
            });

            var operationList = [
                new model.DropDownListItem('Jump To Activity', ContainerTransitions.JumpToActivity.toString()),
                new model.DropDownListItem('Launch Additional Plan', ContainerTransitions.LaunchAdditionalPlan.toString()),
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
                for (var subPlan of $scope.plan.subPlans) {
                    var foundAction = searchAction(id, subPlan.activities);
                    if (foundAction !== null) {
                        return foundAction;
                    }
                }

                return null;
            }

            var findSubPlanById = (id: string): model.SubPlanDTO => {
                for (var i = 0; i < $scope.plan.subPlans.length; i++) {
                    if ($scope.plan.subPlans[i].id === id) {
                        return $scope.plan.subPlans[i];
                    }
                }
                //it seems this id belongs to an action
                //lets look one level up
                var action = findActionById(id);
                if (action != null) {
                    return findSubPlanById(action.parentPlanNodeId);
                }
                return null;
            }

            //Alex requested that lower level of current activity
            //shouldn't be listed as a jump target
            //we are checking if this is the level we should stop
            //each level = ActivityGroup
            //each children creates a level
            var isThisCurrentLevel = (activity: model.ActivityDTO): boolean => {
                return $scope.currentAction.parentPlanNodeId === activity.parentPlanNodeId;
            }

            var getActivityTree = (activity: model.ActivityDTO) => {
                var childActivities = new Array<model.ActivityDTO>();
                if (activity.childrenActivities && activity.childrenActivities.length > 0) {
                    for (var i = 0; i < activity.childrenActivities.length; i++) {
                        childActivities.push(<model.ActivityDTO>activity.childrenActivities[i]);
                        if (!isThisCurrentLevel(<model.ActivityDTO>activity.childrenActivities[i])) {
                            childActivities.concat(getActivityTree(<model.ActivityDTO>activity.childrenActivities[i]));
                        }
                    }
                }

                return childActivities;
            };


            var getCurrentSubplanActivities = () => {
                var currentSubplanId = $scope.currentAction.parentPlanNodeId;
                var subplan = findSubPlanById(currentSubplanId);
                if (subplan == null) {
                    throw 'Unable to find subplan with id ' + currentSubplanId;
                }

                var subplanActivities = new Array<model.DropDownListItem>();
                for (var i = 0; i < subplan.activities.length; i++) {
                    var current = subplan.activities[i];
                    var currentAt = ActivityTemplateHelperService.getActivityTemplate(current);
                    subplanActivities.push(new model.DropDownListItem(currentAt.label, current.id));
                    if (!isThisCurrentLevel(subplan.activities[i])) {
                        var childActivityTree = getActivityTree(current);
                        for (var j = 0; j < childActivityTree.length; j++) {
                            var childAT = ActivityTemplateHelperService.getActivityTemplate(childActivityTree[j]);
                            subplanActivities.push(new model.DropDownListItem(childAT.label, childActivityTree[j].id));
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
                for (var i = 0; i < $scope.plan.subPlans.length; i++) {
                    var subPlanName;

                    if ($scope.plan.subPlans[i].id === $scope.subPlan.id) {
                        subPlanName = 'Jump back to the start of this subplan';
                    }
                    else {
                        subPlanName = $scope.plan.subPlans[i].name;
                    }

                    subplans.push(new model.DropDownListItem(subPlanName, $scope.plan.subPlans[i].id));
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
                    case ContainerTransitions.LaunchAdditionalPlan:
                        (<any>transition)._dummySecondaryOperationDD = buildPlanDropdown();
                        break;
                    case ContainerTransitions.JumpToSubplan:
                        (<any>transition)._dummySecondaryOperationDD = buildSubplanDropdown();
                        break;
                    case ContainerTransitions.StopProcessing:
                    case ContainerTransitions.SuspendProcessing:
                    case ContainerTransitions.ProceedToNextActivity:
                    default:
                        delete (<any>transition)._dummySecondaryOperationDD;
                        return;
                }

                if (transition.targetNodeId != null) {
                    var dd = <model.DropDownList>(<any>transition)._dummySecondaryOperationDD;
                    for (var i = 0; i < dd.listItems.length; i++) {
                        if (dd.listItems[i].value === transition.targetNodeId) {
                            dd.value = dd.listItems[i].value;
                            dd.selectedKey = dd.listItems[i].key;
                        }
                    }
                }
            };

            $scope.addTransition = () => {
                var newTransition = new model.ContainerTransitionField();
                newTransition.name = "transition_" + $scope.field.transitions.length;
                $scope.field.transitions.push(newTransition);
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

            $scope.$on(<any>planEvents.SUB_PLAN_MODIFICATION, function () {
                for (var i = 0; i < $scope.field.transitions.length; i++) {
                    if ($scope.field.transitions[i].transition == ContainerTransitions.JumpToSubplan) {
                        (<any>$scope.field.transitions[i])._dummySecondaryOperationDD.listItems = getCurrentSubplans();
                    };
                };
            });

            $scope.onTargetChange = (transition: model.ContainerTransitionField) => {
                var dd = <model.DropDownList>(<any>transition)._dummySecondaryOperationDD;
                transition.targetNodeId = dd.value;
                transition.errorMessage = null;
                triggerChange();

                if ((<any>transition)._dummyOperationDD.value === ContainerTransitions.JumpToSubplan.toString()
                    && dd.value === $scope.subPlan.id) {

                    $modal.open({
                        template: warningMessageTemplate
                    });
                }
                return angular.noop;
            };

            $scope.onOperationChange = (transition: model.ContainerTransitionField) => {
                var dd = <model.DropDownList>(<any>transition)._dummyOperationDD;
                var targetNodeDd = <model.DropDownList>(<any>transition)._dummySecondaryOperationDD;
                targetNodeDd.value = null;
                targetNodeDd.selectedKey = null;
                transition.targetNodeId = null;
                transition.transition = parseInt(dd.value);
                transition.errorMessage = "";
                processTransition(transition);
                triggerChange();
                $scope.reconfigure();
                return angular.noop;
            };

            var getTransitionKey = (transition: string) : string => {
                for (var i = 0; i < operationList.length; i++) {
                    if (transition === operationList[i].value) {
                        return operationList[i].key;
                    }
                }
                return null;
            };

            $scope.getOperationField = (transition: model.ContainerTransitionField) => {
                if (!(<any>transition)._dummyOperationDD) {
                    var dd = new model.DropDownList();
                    dd.listItems = operationList;

                    if (transition.transition != null) {
                        dd.value = transition.transition.toString();
                        dd.selectedKey = getTransitionKey(transition.transition.toString());
                    }

                    (<any>transition)._dummyOperationDD = dd;
                }

                $timeout(() => {
                    processTransition(transition);
                    informJumpTargets();
                });
                

                return (<any>transition)._dummyOperationDD;
            };
            $scope.removeTransition = (index: number) => {
                $scope.field.transitions.splice(index, 1);
                $scope.field.transitions.forEach((tran, index) => {
                    tran.name = "transition_" + index;
                });
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
                plan: '=',
                subPlan: '=',
                field: '=',
                currentAction: '=',
                isDisabled: '=',
                change: '&'
            }
        };
    }

    app.directive('containerTransition', ContainerTransition);
}