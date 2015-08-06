module dockyard.directives.paneConfigureAction {
    export enum MessageType {
        PaneConfigureAction_ActionUpdated,
        PaneConfigureAction_Render,
        PaneConfigureAction_Hide,
        PaneConfigureAction_Cancelled
    }

    export class ActionUpdatedEventArgs {
        public criteriaId: number;
        public actionId: number;
        public actionTempId: number;
        public processTemplateId: number;

        constructor(criteriaId: number, actionId: number, actionTempId: number, processTemplateId: number) {
            this.actionId = actionId;
            this.criteriaId = criteriaId;
            this.actionTempId = actionTempId;
            this.processTemplateId = processTemplateId
        }
    }

    export class RenderEventArgs {
        public criteriaId: number;
        public actionId: number;
        public isTempId: boolean;
        public processTemplateId: number;

        constructor(criteriaId: number, actionId: number, isTempId: boolean, processTemplateId: number) {
            this.actionId = actionId;
            this.criteriaId = criteriaId;
            this.isTempId = isTempId;
            this.processTemplateId = processTemplateId
        }
    }

    export class CancelledEventArgs {
        public criteriaId: number;
        public actionId: number;
        public isTempId: boolean;
        public processTemplateId: number;

        constructor(criteriaId: number, actionId: number, isTemp: boolean, processTemplateId: number) {
            this.actionId = actionId;
            this.criteriaId = criteriaId;
            this.isTempId = isTemp;
            this.processTemplateId = processTemplateId
        }
    }
}