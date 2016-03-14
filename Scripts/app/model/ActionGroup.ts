module dockyard.model {
    export class ActionGroup {
        public envelopes: model.ActivityEnvelope[];

        public offsetTop: number = 0;
        public offsetLeft: number = 0;
        public height: number = 300;

        public arrowLength: number = 0;
        public arrowOffsetLeft: number = 0;

        public parentAction: model.ActivityDTO;
        public parentId: string;

        constructor(activities: model.ActivityDTO[], parentAction: model.ActivityDTO, parentId: string) {
            this.envelopes = this.packActivities(activities);
            this.parentAction = parentAction;
            this.parentId = parentId;
        }

        packActivities(activities: model.ActivityDTO[]) {
            var envelopeList = new Array<model.ActivityEnvelope>();
            var sortedActivities = _.sortBy(activities, (activity: model.ActivityDTO) => activity.ordering);
            for (var i = 0; i < sortedActivities.length; i++) {
                envelopeList.push(new  ActivityEnvelope(sortedActivities[i]));
            }
            return envelopeList;
        }
    }

    export class ActivityEnvelope {
        public activity: model.ActivityDTO;
        public allowsSiblings: boolean;

        constructor(activity: model.ActivityDTO) {
            this.activity = activity;
            this.allowsSiblings = true;
        }

    }
}