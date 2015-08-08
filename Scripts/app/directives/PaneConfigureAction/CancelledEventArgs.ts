/// <reference path="../../_all.ts" />

module dockyard.directives.paneConfigureAction {
    'use strict';

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