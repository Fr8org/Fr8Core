module dockyard.model {
    export class ActionGroup {
        public actions: model.ActionDTO[]

        public offsetTop: number = 0;
        public offsetLeft: number = 0;
        public height: number = 300;

        public arrowLength: number = 0;
        public arrowOffsetLeft: number = 0;

        public parentAction: model.ActionDTO;

        constructor(actions: model.ActionDTO[], parentAction: model.ActionDTO) {
            this.actions = actions;
            this.parentAction = parentAction;
        }
    }
}