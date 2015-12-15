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

        constructor() {
        }

        placeActions(actions: model.ActionDTO[], startingId: string): model.ActionGroup[] {
            var processedGroups: model.ActionGroup[] = [];

            var actionGroups = _.toArray<model.ActionDTO[]>(
                _.groupBy<model.ActionDTO>(actions, (action) => action.parentRouteNodeId)
            );

            if (actions.length) {
                var startingGroup = this.findChildGroup(actionGroups, startingId);
                this.processGroup(actionGroups, new model.ActionGroup(startingGroup), processedGroups);
            } else {
                processedGroups.push(new model.ActionGroup([]));
            }

            return processedGroups;
        }

        recalculateTop(actionGroups: model.ActionGroup[]) {
            var processedGroups: model.ActionGroup[] = [actionGroups[0]];
            for (var i = 1; i < actionGroups.length; i++) {
                var curGroup = actionGroups[i];
                curGroup.offsetTop = this.calculateOffsetTop(actionGroups[i - 1]);

                var parentGroup = this.findParentGroup(actionGroups, curGroup.actions[0].parentRouteNodeId);
                curGroup.arrowLength = this.calculateArrowLength(curGroup, parentGroup);
            }
        }
        
        // Depth first search on the ActionGroup tree going from last sibling to first.
        private processGroup(actionGroups: model.ActionDTO[][], group: model.ActionGroup, processedGroups: model.ActionGroup[]) {
            processedGroups.push(group);
            group.actions = _.sortBy(group.actions, (action: model.ActionDTO) => action.ordering);
            for (var i = group.actions.length - 1; i > -1; i--) {
                //var childGroup = this.findChildGroup(actionGroups, group.actions[i].id);
                if (group.actions[i].childrenActions.length) {
                    var newGroup = new model.ActionGroup(group.actions[i].childrenActions);
                    this.calculateGroupPosition(newGroup, group, processedGroups);
                    this.processGroup(actionGroups, newGroup, processedGroups);
                }
            }
        }

        private calculateGroupPosition(group: model.ActionGroup, parentGroup: model.ActionGroup, processedGroups: model.ActionGroup[]): void {
            group.offsetLeft = this.calculateOffsetLeft(parentGroup, group.actions[0]);
            group.offsetTop = this.calculateOffsetTop(processedGroups[processedGroups.length - 1]);
            group.arrowLength = this.calculateArrowLength(group, parentGroup);
            group.arrowOffsetLeft = this.calculateArrowOffsetLeft(group);
        }

        private calculateOffsetLeft(parentGroup: model.ActionGroup, action: model.ActionDTO): number {
            var i = 0,
                offsetLeft = parentGroup.offsetLeft;
            while (parentGroup.actions[i].id != action.parentRouteNodeId) {
                offsetLeft += (parentGroup.actions[i].activityTemplate.minPaneWidth || this.ACTION_WIDTH) + this.ACTION_PADDING;
                i++;
            }
            offsetLeft += ((parentGroup.actions[i].activityTemplate.minPaneWidth || this.ACTION_WIDTH) - (action.activityTemplate.minPaneWidth || this.ACTION_WIDTH)) / 2;

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
            return ((group.actions[0].activityTemplate.minPaneWidth || this.ACTION_WIDTH) - this.ARROW_WIDTH) / 2;
        }

        private findParentAction(group: model.ActionGroup, parentGroup: model.ActionGroup): model.ActionDTO {
            return _.find(parentGroup.actions, (action: model.ActionDTO) => {
                return action.id === group.actions[0].parentRouteNodeId;
            });
        }      

        private findParentGroup(actionGroups: model.ActionGroup[], parentId: string): model.ActionGroup {
            return _.find(actionGroups, (group: model.ActionGroup) => {
                return group.actions.some((action: model.ActionDTO) => {
                    return action.id === parentId;
                });
            });
        }        

        private findChildGroup(actionGroups: model.ActionDTO[][], parentId: string): model.ActionDTO[] {
            return _.find(actionGroups, (group: model.ActionDTO[]) => {
                return group[0].parentRouteNodeId === parentId;
            });
        }        
    }

    app.service('LayoutService', LayoutService);
}
