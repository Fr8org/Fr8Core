/// <reference path="../../_all.ts" />

module dockyard.directives.paneConfigureAction {
    'use strict';

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
} 