/// <reference path="../_all.ts" />

module dockyard.services {

    export interface ILayoutService {
        placeActions(actionGroups, startingId): model.ActionGroup[];
        setSiblingStatus(action: model.ActivityDTO, allowSiblings: boolean): void;
        setJumpTargets(action: model.ActivityDTO, targets: Array<model.ActivityJumpTarget>): void;
        resetLayout(): void;
        addEmptyProcessedGroup(startingId): model.ActionGroup[];
    }

    export class LayoutService implements ILayoutService {
        public ACTION_HEIGHT = 300;
        public ACTION_WIDTH = 330;
        public ACTION_PADDING = 70;
        public ARROW_WIDTH = 16;
        public ADD_ACTION_BUTTON_WIDTH = 150;

        public subplans: Array<Array<model.ActionGroup>> = [];
        private activityTemplates: Array<interfaces.IActivityTemplateVM> = null;

        constructor(private CrateHelper: services.CrateHelper, private ActivityTemplateHelperService: services.IActivityTemplateHelperService) {
            
        }

        addEmptyProcessedGroup(parentId: string) {
            var processedGroups: model.ActionGroup[] = [];
            processedGroups.push(new model.ActionGroup([], null, parentId));
            this.subplans.push(processedGroups);
            return processedGroups;
        }

        setJumpTargets(action: model.ActivityDTO, targets: Array<model.ActivityJumpTarget>) {
            var env = this.findEnvelope(action.id);
            env.jumpTargets = targets;
        }

        resetLayout() {
            this.subplans = [];
        }

        placeActions(actions: model.ActivityDTO[], parentId: string): model.ActionGroup[] {

            var processedGroups: model.ActionGroup[] = [];

            var actionGroups = _.toArray<model.ActivityDTO[]>(
                _.groupBy<model.ActivityDTO>(actions, (action) => action.parentPlanNodeId)
            );

            if (actions.length) {
                var startingGroup = this.findChildGroup(actionGroups, parentId);
                this.processGroup(actionGroups, new model.ActionGroup(this.packActivities(startingGroup), null, parentId), processedGroups);
            } else {
                processedGroups.push(new model.ActionGroup([], null, parentId));
            }
            this.subplans.push(processedGroups);
            return processedGroups;
        }

        recalculateTop(actionGroups: model.ActionGroup[]) {
            for (var i = 1; i < actionGroups.length; i++) {
                var curGroup = actionGroups[i];
                curGroup.offsetTop = this.calculateOffsetTop(actionGroups[i - 1]);
                var parentGroup = this.findParentGroup(actionGroups, curGroup.parentEnvelope.activity.id);
                curGroup.arrowLength = this.calculateArrowLength(curGroup, parentGroup);
            }
        }

        private packActivities(activities: model.ActivityDTO[]) {
            var envelopeList = new Array<model.ActivityEnvelope>();
            var sortedActivities = <Array<model.ActivityDTO>>_.sortBy(activities, (activity: model.ActivityDTO) => activity.ordering);
            for (var i = 0; i < sortedActivities.length; i++) {
                var at = this.ActivityTemplateHelperService.getActivityTemplate(sortedActivities[i]);
                envelopeList.push(new model.ActivityEnvelope(sortedActivities[i], at));
            }
            return envelopeList;
        }
        
        // Depth first search on the ActionGroup tree going from last sibling to first.
        private processGroup(actionGroups: model.ActivityDTO[][], group: model.ActionGroup, processedGroups: model.ActionGroup[]) {
            if (group.parentEnvelope && group.parentEnvelope.activityTemplate) {
                if (group.parentEnvelope.activityTemplate.tags
                    && group.parentEnvelope.activityTemplate.tags.indexOf('HideChildren') !== -1) {
                    return;
                }
            }

            processedGroups.push(group);
            
            for (var i = group.envelopes.length - 1; i > -1; i--) {
                //var childGroup = this.findChildGroup(actionGroups, group.actions[i].id);
                if (group.envelopes[i].activity.childrenActivities.length) {
                    var newGroup = new model.ActionGroup(this.packActivities(<model.ActivityDTO[]>(group.envelopes[i].activity.childrenActivities)), group.envelopes[i], group.envelopes[i].activity.id);
                    this.calculateGroupPosition(newGroup, group, processedGroups);
                    this.processGroup(actionGroups, newGroup, processedGroups);
                } else if (this.allowsChildren(group.envelopes[i])) { //has no children, but allows it. we should place an add action button below
                    var potentialGroup = new model.ActionGroup([], group.envelopes[i], group.envelopes[i].activity.id);
                    this.calculateGroupPosition(potentialGroup, group, processedGroups);
                    this.processGroup(actionGroups, potentialGroup, processedGroups);
                }
            }
            
        }

        private allowsChildren(envelope: model.ActivityEnvelope) {
            return envelope.activityTemplate.type === 'Loop';
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
            while (parentGroup.envelopes[i].activity.id != currentGroup.parentEnvelope.activity.id) {
                offsetLeft += (parentGroup.envelopes[i].activityTemplate.minPaneWidth || this.ACTION_WIDTH) + this.ACTION_PADDING;
                i++;
            }

            //offsetLeft += ((parentGroup.actions[i].activityTemplate.minPaneWidth || this.ACTION_WIDTH) - (action.activityTemplate.minPaneWidth || this.ACTION_WIDTH)) / 2;
            if (currentGroup.envelopes.length) {
                offsetLeft += ((parentGroup.envelopes[i].activityTemplate.minPaneWidth || this.ACTION_WIDTH) - (currentGroup.envelopes[0].activityTemplate.minPaneWidth || this.ACTION_WIDTH)) / 2;
            } else {
                offsetLeft += (this.ACTION_WIDTH - this.ADD_ACTION_BUTTON_WIDTH) / 2;
            }

            // if offsetLeft negative, return 0
            offsetLeft = offsetLeft < 0 ? 0 : offsetLeft;


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
            if (group.envelopes.length) {
                return ((group.envelopes[0].activityTemplate.minPaneWidth || this.ACTION_WIDTH) - this.ARROW_WIDTH) / 2;
            } else {
                return 70; //TODO calculate this for real //(this.ACTION_WIDTH - this.ARROW_WIDTH) / 2;
            }
        }

        private findParentAction(group: model.ActionGroup, parentGroup: model.ActionGroup): interfaces.IActivityDTO {
            var envelope = _.find(parentGroup.envelopes, (envelope: model.ActivityEnvelope) => {
                return envelope.activity.id === group.parentEnvelope.activity.id;
            });
            return envelope.activity;
        }      

        private findParentGroup(actionGroups: model.ActionGroup[], parentId: string): model.ActionGroup {
            return _.find(actionGroups, (group: model.ActionGroup) => {
                return group.envelopes.some((envelope: model.ActivityEnvelope) => {
                    return envelope.activity.id === parentId;
                });
            });
        }        

        private findChildGroup(actionGroups: model.ActivityDTO[][], parentId: string): model.ActivityDTO[] {
            return _.find(actionGroups, (group: model.ActivityDTO[]) => {
                return group[0].parentPlanNodeId === parentId;
            });
        }

        private findEnvelope(activityId: string): model.ActivityEnvelope {
            for (var k = 0; k < this.subplans.length; k++) {
                var pGroup = this.subplans[k];
                for (var i = 0; i < pGroup.length; i++) {
                    for (var j = 0; j < pGroup[i].envelopes.length; j++) {
                        if (pGroup[i].envelopes[j].activity.id === activityId) {
                            return pGroup[i].envelopes[j];
                        }
                    }
                }
            }
            

            return null;
        }

        setSiblingStatus(action: model.ActivityDTO, allowSiblings: boolean): void {
            var env = this.findEnvelope(action.id);
            env.allowsSiblings = allowSiblings;
        }
    }

    app.service('LayoutService', ['CrateHelper', 'ActivityTemplateHelperService', LayoutService]);
}
