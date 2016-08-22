module dockyard.directives {

    export class EventArgsBase {
        constructor() {

        }
    }

    export class CriteriaEventArgsBase extends EventArgsBase {
        public criteriaId: number;
        constructor(criteriaId: number) {
            super();
            this.criteriaId = criteriaId;
        }
    }

    export class ActionEventArgsBase extends CriteriaEventArgsBase {
        public criteriaId: number;
        public actionId: number;
        constructor(criteriaId: number, actionId: number) {
            super(criteriaId);
            this.actionId = actionId;
        }
    }

    export class RenderEventArgsBase extends ActionEventArgsBase {
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTempId: boolean) {
            super(criteriaId, actionId);
            this.isTempId = isTempId;
        }
    }

    export class CancelledEventArgsBase extends ActionEventArgsBase {
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTemp: boolean) {
            super(criteriaId, actionId);
            this.isTempId = isTemp;
        }
    }

    export class HideEventArgsBase extends ActionEventArgsBase {
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTemp: boolean) {
            super(criteriaId, actionId);
            this.isTempId = isTemp;
        }
    }

    export class ActionUpdatedEventArgsBase extends ActionEventArgsBase {
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTempId: boolean) {
            super(criteriaId, actionId);
            this.isTempId = isTempId;
        }
    }

    export class AlertEventArgs extends EventArgsBase { }
}