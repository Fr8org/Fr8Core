/// <reference path="../../_all.ts" />

module dockyard.directives.paneSelectAction {
    'use strict';

    export class ActionUpdatedEventArgs {
        public actionId: number;
        public criteriaId: number;
        public isTempId: boolean;
        public actionName: string;
        public processTemplateId: number;

        constructor(criteriaId: number, actionId: number, isTemp: boolean, actionName: string, processTemplateId: number) {
            this.actionId = actionId;
            this.criteriaId = criteriaId;
            this.isTempId = isTemp;
            this.actionName = actionName;
            this.processTemplateId = processTemplateId
        }
    }
} 