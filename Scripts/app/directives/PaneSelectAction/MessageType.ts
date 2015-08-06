module dockyard.directives.paneSelectAction {
    export enum MessageType {
        PaneSelectAction_ActionUpdated,
        PaneSelectAction_Render,
        PaneSelectAction_Hide,
        PaneSelectAction_UpdateAction,
        PaneSelectAction_ActionTypeSelected
    }

    export class ActionTypeSelectedEventArgs {
        public actionId: number;
        public criteriaId: number;
        public tempActionId: number;
        public actionTypeId: number;
        public processTemplateId: number;

        constructor(criteriaId: number, actionId: number, tempActionId: number, actionTypeId: number, processTemplateId: number) {
            this.actionId = actionId;
            this.criteriaId = criteriaId;
            this.tempActionId = tempActionId;
            this.actionTypeId = actionTypeId;
            this.processTemplateId = processTemplateId
        }
    }

    export class ActionUpdatedEventArgs {
        public actionId: number;
        public criteriaId: number;
        public tempActionId: number;
        public actionName: string;
        public processTemplateId: number;

        constructor(criteriaId: number, actionId: number, tempActionId: number, actionName: string, processTemplateId: number) {
            this.actionId = actionId;
            this.criteriaId = criteriaId;
            this.tempActionId = tempActionId;
            this.actionName = actionName;
            this.processTemplateId = processTemplateId
        }
    }

    export class RenderEventArgs {
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

    export class UpdateActionEventArgs {
        public actionId: number;
        public criteriaId: number;
        public actionTempId: number;
        public processTemplateId: number;

        constructor(criteriaId: number, actionId: number, actionTempId: number, processTemplateId: number) {
            this.actionId = actionId;
            this.criteriaId = criteriaId;
            this.actionTempId = actionTempId;
            this.processTemplateId = processTemplateId
        }
    }
}