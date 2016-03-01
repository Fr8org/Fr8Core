module dockyard.model {
    export class ActionGroup {
        public activities: model.ActivityDTO[];

        public offsetTop: number = 0;
        public offsetLeft: number = 0;
        public height: number = 300;

        public arrowLength: number = 0;
        public arrowOffsetLeft: number = 0;

        public parentAction: model.ActivityDTO;
        public parentId: string;

        constructor(activities: model.ActivityDTO[], parentAction: model.ActivityDTO, parentId: string) {
            this.activities = activities;
            this.parentAction = parentAction;
            this.parentId = parentId;
        }
    }
}