module dockyard.directives.paneSelectTemplate {
    export enum MessageType {
        PaneSelectTemplate_ProcessTemplateUpdated,
        PaneSelectTemplate_Render,
        PaneSelectTemplate_Hide,
    }

    export class RenderEventArgs {
        public processTemplateId: number;

        constructor(processTemplateId: number) {
            this.processTemplateId = processTemplateId
        }
    }
}