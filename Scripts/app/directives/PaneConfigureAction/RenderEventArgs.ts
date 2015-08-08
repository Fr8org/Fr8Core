/// <reference path="../../_all.ts" />

module dockyard.directives.paneConfigureAction {
    'use strict';

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
} 