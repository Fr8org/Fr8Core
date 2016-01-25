module dockyard.model {
    export class ActionGroup {
        public actions: model.ActivityDTO[]

        public offsetTop: number = 0;
        public offsetLeft: number = 0;
        public height: number = 300;

        public arrowLength: number = 0;
        public arrowOffsetLeft: number = 0;

        public parentAction: model.ActivityDTO;

        constructor(actions: model.ActivityDTO[], parentAction: model.ActivityDTO) {
            this.actions = actions;
            this.parentAction = parentAction;
        }
    }
}