/// <reference path="../_all.ts" />

module dockyard.services {

    export interface ILayoutService {
        placeActions(actionGroups, startingId): model.ActionGroup[];
    }

    export class LayoutService implements ILayoutService {
        public ACTION_HEIGHT = 300;
        public ACTION_WIDTH = 330;
        public ACTION_PADDING = 70;
        public ARROW_WIDTH = 16;
        public ADD_ACTION_BUTTON_WIDTH = 150;

        constructor(private CrateHelper: services.CrateHelper) {
        }

        placeActions(actions: model.ActivityDTO[], startingId: string): model.ActionGroup[] {
            var processedGroups: model.ActionGroup[] = [];

            var actionGroups = _.toArray<model.ActivityDTO[]>(
                _.groupBy<model.ActivityDTO>(actions, (action) => action.parentPlanNodeId)
            );

            if (actions.length) {
                var startingGroup = this.findChildGroup(actionGroups, startingId);
                this.processGroup(actionGroups, new model.ActionGroup(startingGroup, null), processedGroups);
            } else {
                processedGroups.push(new model.ActionGroup([], null));
            }

            return processedGroups;
        }

        recalculateTop(actionGroups: model.ActionGroup[]) {
            for (var i = 1; i < actionGroups.length; i++) {
                var curGroup = actionGroups[i];
                curGroup.offsetTop = this.calculateOffsetTop(actionGroups[i - 1]);
                var parentGroup = this.findParentGroup(actionGroups, curGroup.parentAction.id);
                curGroup.arrowLength = this.calculateArrowLength(curGroup, parentGroup);
            }
        }
        
        // Depth first search on the ActionGroup tree going from last sibling to first.
        private processGroup(actionGroups: model.ActivityDTO[][], group: model.ActionGroup, processedGroups: model.ActionGroup[]) {
            if (group.parentAction && group.parentAction.activityTemplate) {
                if (group.parentAction.activityTemplate.tags
                    && group.parentAction.activityTemplate.tags.indexOf('HideChildren') !== -1) {
                    return;
                }
            }

            processedGroups.push(group);
            group.activities = _.sortBy(group.activities, (action: model.ActivityDTO) => action.ordering);

            for (var i = group.activities.length - 1; i > -1; i--) {
                //var childGroup = this.findChildGroup(actionGroups, group.actions[i].id);
                if (group.activities[i].childrenActivities.length) {
                    var newGroup = new model.ActionGroup(<model.ActivityDTO[]>group.activities[i].childrenActivities, group.activities[i]);
                    this.calculateGroupPosition(newGroup, group, processedGroups);
                    this.processGroup(actionGroups, newGroup, processedGroups);
                } else if (this.allowsChildren(group.activities[i])) { //has no children, but allows it. we should place an add action button below
                    var potentialGroup = new model.ActionGroup([], group.activities[i]);
                    this.calculateGroupPosition(potentialGroup, group, processedGroups);
                    this.processGroup(actionGroups, potentialGroup, processedGroups);
                }
            }
            
        }

        private allowsChildren(action: model.ActivityDTO) {
            return action.activityTemplate.type === 'Loop';
        }

        private calculateGroupPosition(group: model.ActionGroup, parentGroup: model.ActionGroup, processedGroups: model.ActionGroup[]): void {
            group.offsetLeft = this.calculateOffsetLeft(parentGroup, group);
            group.offsetTop = this.calculateOffsetTop(processedGroups[processedGroups.length - 1]);
            group.arrowLength = this.calculateArrowLength(group, parentGroup);
            group.arrowOffsetLeft = this.calculateArrowOffsetLeft(group);
        }

        private calculateOffsetLeft(parentGroup: model.ActionGroup, currentGroup: model.ActionGroup): number {

            var i = 0, offsetLeft = parentGroup.offsetLeft;
            //calculate left offset by summing existing elements
            while (parentGroup.activities[i].id != currentGroup.parentAction.id) {
                offsetLeft += (parentGroup.activities[i].activityTemplate.minPaneWidth || this.ACTION_WIDTH) + this.ACTION_PADDING;
                i++;
            }

            //offsetLeft += ((parentGroup.actions[i].activityTemplate.minPaneWidth || this.ACTION_WIDTH) - (action.activityTemplate.minPaneWidth || this.ACTION_WIDTH)) / 2;
            if (currentGroup.activities.length) {
                offsetLeft += ((parentGroup.activities[i].activityTemplate.minPaneWidth || this.ACTION_WIDTH) - (currentGroup.activities[0].activityTemplate.minPaneWidth || this.ACTION_WIDTH)) / 2;
            } else {
                offsetLeft += (this.ACTION_WIDTH - this.ADD_ACTION_BUTTON_WIDTH) / 2;
            }


            return offsetLeft;
        }

        private calculateOffsetTop(prevGroup: model.ActionGroup): number {
            return prevGroup.offsetTop + prevGroup.height + this.ACTION_PADDING;
        }

        private calculateArrowLength(group: model.ActionGroup, parentGroup: model.ActionGroup): number {
            var parentAction = this.findParentAction(group, parentGroup);
            return group.offsetTop - parentGroup.offsetTop - parentAction.height - this.ACTION_PADDING;
        }

        private calculateArrowOffsetLeft(group: model.ActionGroup): number {
            if (group.activities.length) {
                return ((group.activities[0].activityTemplate.minPaneWidth || this.ACTION_WIDTH) - this.ARROW_WIDTH) / 2;
            } else {
                return 70; //TODO calculate this for real //(this.ACTION_WIDTH - this.ARROW_WIDTH) / 2;
            }
        }

        private findParentAction(group: model.ActionGroup, parentGroup: model.ActionGroup): model.ActivityDTO {
            return _.find(parentGroup.activities, (action: model.ActivityDTO) => {
                return action.id === group.parentAction.id;
            });
        }      

        private findParentGroup(actionGroups: model.ActionGroup[], parentId: string): model.ActionGroup {
            return _.find(actionGroups, (group: model.ActionGroup) => {
                return group.activities.some((action: model.ActivityDTO) => {
                    return action.id === parentId;
                });
            });
        }        

        private findChildGroup(actionGroups: model.ActivityDTO[][], parentId: string): model.ActivityDTO[] {
            return _.find(actionGroups, (group: model.ActivityDTO[]) => {
                return group[0].parentPlanNodeId === parentId;
            });
        }        
    }

    app.service('LayoutService', ['CrateHelper', LayoutService]);
}
