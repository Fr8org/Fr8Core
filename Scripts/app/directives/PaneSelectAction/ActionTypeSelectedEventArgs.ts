/// <reference path="../../_all.ts" />

module dockyard.directives.paneSelectAction {
    'use strict';

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
} 