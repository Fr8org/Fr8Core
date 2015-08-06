/// <reference path="../../_all.ts" />

module dockyard.directives.paneSelectTemplate {
    'use strict';

    export class RenderEventArgs {
        public processTemplateId: number;

        constructor(processTemplateId: number) {
            this.processTemplateId = processTemplateId
        }
    }
} 