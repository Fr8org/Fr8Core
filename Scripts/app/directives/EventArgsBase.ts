module dockyard.directives {

    export class EventArgsBase {
        constructor() {

        }
    }

    export class CriteriaEventArgs extends EventArgsBase {
        public criteriaId: number;
        constructor(criteriaId: number) {
            super();
            this.criteriaId = criteriaId;
        }
    }

    export class ActionEventArgs extends CriteriaEventArgs {
        public criteriaId: number;
        public actionId: number;
        constructor(criteriaId: number, actionId: number) {
            super(criteriaId);
            this.actionId = actionId;
        }
    }

    export class RenderEventArgsBase extends ActionEventArgs {
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTempId: boolean) {
            super(criteriaId, actionId);
            this.isTempId = isTempId;
        }
    }

    export class CancelledEventArgsBase extends ActionEventArgs{
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTemp: boolean) {
            super(criteriaId, actionId);
            this.isTempId = isTemp;
        }
    }

    export class ActionUpdatedEventArgsBase extends ActionEventArgs{
        public actionTempId: number;

        constructor(criteriaId: number, actionId: number, actionTempId: number) {
            super(criteriaId, actionId);
            this.actionTempId = actionTempId;
        }
    }
}