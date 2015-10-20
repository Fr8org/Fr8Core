/// <reference path="../_all.ts" />

module dockyard.services {

    export interface ILayoutService {
        placeActions(actionGroups, startingId): model.ActionGroup[];
    }

    export class LayoutService implements ILayoutService {
        private ACTION_HEIGHT = 300;
        private ACTION_WIDTH = 330;
        private ACTION_PADDING = 70;

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

            var offsetTop = parentGroup.offsetTop + this.ACTION_HEIGHT + this.ACTION_PADDING;
            for (var processedGroup of processedGroups) {
                if (offsetTop <= processedGroup.offsetTop) {
                    offsetTop = processedGroup.offsetTop + this.ACTION_HEIGHT + this.ACTION_PADDING;
                }
            }
            group.offsetTop = offsetTop;
            group.arrowLength = offsetTop - parentGroup.offsetTop - this.ACTION_HEIGHT - this.ACTION_PADDING;
        }

        private findChildGroup(actionGroups: model.ActionDTO[][], parentId: number) {
            return _.find(actionGroups, (group: model.ActionDTO[]) => {
                return group[0].parentRouteNodeId === parentId;
            });
        }        
    }

    app.service('LayoutService', LayoutService);
}
