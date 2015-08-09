module dockyard.directives.paneSelectTemplate {
    export enum MessageType {
        PaneSelectTemplate_ProcessTemplateUpdated,
        PaneSelectTemplate_Render,
        PaneSelectTemplate_Hide,
    }

    export class RenderEventArgs extends EventArgsBase {
        constructor() {
            super ();
        }
    }

    export class ProcessTemplateUpdatedEventArgs extends EventArgsBase {
        public processTemplateId: number;
        public processTemplateName: string;

        constructor(processTemplateId: number, processTemplateName: string) {
            this.processTemplateId = processTemplateId;
            this.processTemplateName = processTemplateName;
            super ();
        }
    }

    export class HideEventArgs extends EventArgsBase {
        constructor() {
            super ();
        }
    }
}