/// <reference path="../../_all.ts" />

module dockyard.directives.paneWorkflowDesigner {
    'use strict';

    export class UpdateActionEventArgs {
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