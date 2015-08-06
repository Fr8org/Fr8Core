/// <reference path="../../_all.ts" />

module dockyard.directives.paneSelectAction {
    'use strict';

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