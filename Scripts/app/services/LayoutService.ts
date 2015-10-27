/// <reference path="../_all.ts" />

module dockyard.services {

    export interface ILayoutService {
        placeActions(actionGroups, startingId): model.ActionGroup[];
    }

    export class LayoutService implements ILayoutService {
        public ACTION_HEIGHT = 300;
        public ACTION_WIDTH = 330;
        public ACTION_PADDING = 70;

        constructor() {
        }

        placeActions(actions: model.ActionDTO[], startingId: number): model.ActionGroup[] {
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
            for (var i = group.actions.length - 1; i > -1; i--) {
                var childGroup = this.findChildGroup(actionGroups, group.actions[i].id);
                if (childGroup) {
                    var newGroup = new model.ActionGroup(childGroup) 
                    this.calculateGroupPosition(newGroup, group, processedGroups);
                    this.processGroup(actionGroups, newGroup, processedGroups);
                }
            }
        }

        private calculateGroupPosition(group: model.ActionGroup, parentGroup: model.ActionGroup, processedGroups: model.ActionGroup[]): void {
            var parentActionIdx = _.findIndex(parentGroup.actions, (action: model.ActionDTO) => {
                return action.id === group.actions[0].parentRouteNodeId;
            });
            group.offsetLeft = parentGroup.offsetLeft + parentActionIdx * (this.ACTION_WIDTH + this.ACTION_PADDING);
            group.offsetTop = this.calculateOffsetTop(processedGroups[processedGroups.length - 1]);
            group.arrowLength = this.calculateArrowLength(group, parentGroup);
        }

        private calculateOffsetTop(prevGroup: model.ActionGroup) {
            return prevGroup.offsetTop + prevGroup.height + this.ACTION_PADDING;
        }

        private calculateArrowLength(group: model.ActionGroup, parentGroup: model.ActionGroup) {
            return group.offsetTop - parentGroup.offsetTop - parentGroup.height - this.ACTION_PADDING;
        }

        private findParentGroup(actionGroups: model.ActionGroup[], parentId: number) {
            return _.find(actionGroups, (group: model.ActionGroup) => {
                return group.actions.some((action: model.ActionDTO) => {
                    return action.id === parentId;
                });
            });
        }        

        private findChildGroup(actionGroups: model.ActionDTO[][], parentId: number) {
            return _.find(actionGroups, (group: model.ActionDTO[]) => {
                return group[0].parentRouteNodeId === parentId;
            });
        }        
    }

    app.service('LayoutService', LayoutService);
}
