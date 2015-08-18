module dockyard.directives.paneDefineCriteria {

    export enum MessageType {
        PaneDefineCriteria_Render,
        PaneDefineCriteria_Hide,
        PaneDefineCriteria_Save,
        PaneDefineCriteria_ProcessNodeTemplateRemoving,
        PaneDefineCriteria_ProcessNodeTemplateUpdating,
        PaneDefineCriteria_Cancelling
    }

    export class RenderEventArgs {
        public fields: Array<model.Field>;
        public processNodeTemplate: model.ProcessNodeTemplate;

        constructor(fields: Array<model.Field>, processNodeTemplate: model.ProcessNodeTemplate) {
            this.fields = fields;
            this.processNodeTemplate = processNodeTemplate;
        }
    }

    export class ProcessNodeTemplateRemovingEventArgs {
        public processNodeTemplateId: number;
        public isTempId: boolean;

        constructor(processNodeTemplateId: number, isTempId: boolean) {
            this.processNodeTemplateId = processNodeTemplateId;
            this.isTempId = isTempId;
        }
    }

    export class ProcessNodeTemplateUpdatingEventArgs {
        public processNodeTemplateId: number;
        public name: string;
        public processNodeTemplateTempId: number;

        constructor(
            processNodeTemplateId: number,
            name: string,
            processNodeTemplateTempId: number) {

            this.processNodeTemplateId = processNodeTemplateId;
            this.name = name;
            this.processNodeTemplateTempId = processNodeTemplateTempId;
        }
    }
} 