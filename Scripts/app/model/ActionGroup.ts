module dockyard.model {
    export class ActionGroup {
        public actions: ActionDTO[]
        public offsetTop: number = 0;
        public offsetLeft: number = 0;
        public height: number = 300;
        public arrowLength: number = 0;

        constructor(actions) {
            this.actions = actions;
        }
    }
}