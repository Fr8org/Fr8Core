/// <reference path="../../_all.ts" />

module dockyard.directives.paneSelectAction {
    'use strict';

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
} 