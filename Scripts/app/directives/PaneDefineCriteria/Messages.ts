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
        public processTemplateId: number;
        public id: number;
        public isTempId: boolean;

        constructor(fields: Array<model.Field>, processTemplateId: number,
            id: number, isTempId: boolean) {

            this.fields = fields;
            this.processTemplateId = processTemplateId;
            this.id = id;
            this.isTempId = isTempId;
        }
    }

    export class SaveEventArgs {
        public callback: (args: SaveCallbackArgs) => void;

        constructor(callback: (args: SaveCallbackArgs) => void) {
            this.callback = callback;
        }
    }

    export class SaveCallbackArgs {
        public id: number;
        public tempId: number;

        constructor(id: number, tempId: number) {
            this.id = id;
            this.tempId = tempId;
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