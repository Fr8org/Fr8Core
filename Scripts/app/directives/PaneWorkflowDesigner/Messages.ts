/// <reference path="../../_all.ts" />

module dockyard.directives.paneWorkflowDesigner {
    'use strict';

    export enum MessageType {
        PaneWorkflowDesigner_Render,
        PaneWorkflowDesigner_TemplateSelecting,
        PaneWorkflowDesigner_ProcessNodeTemplateAdding,
        PaneWorkflowDesigner_ProcessNodeTemplateAdded,
        PaneWorkflowDesigner_ProcessNodeTemplateSelecting,
        PaneWorkflowDesigner_ProcessNodeTemplateRemoved,
        PaneWorkflowDesigner_ActionAdding,
        PaneWorkflowDesigner_ActionAdded,
        PaneWorkflowDesigner_ActionSelecting,
        PaneWorkflowDesigner_ActionRemoved,
        // PaneWorkflowDesigner_RefreshElement,
        PaneWorkflowDesigner_UpdateAction,
        PaneWorkflowDesigner_UpdateCriteriaName,
        PaneWorkflowDesigner_ProcessNodeTemplateTempIdReplaced
    }

    export class RenderEventArgs {
    }

    export class ProcessNodeTemplateAddingEventArgs {
    }

    export class ProcessNodeTemplateAddedEventArgs {
        public processNodeTemplate: model.ProcessNodeTemplate;

        constructor(processNodeTemplate: model.ProcessNodeTemplate) {
            this.processNodeTemplate = processNodeTemplate;
        }
    }

    export class ProcessNodeTemplateSelectingEventArgs {
        public processNodeTemplateId: number;

        constructor(processNodeTemplateId: number) {
            this.processNodeTemplateId = processNodeTemplateId;
        }
    }

    export class ProcessNodeTemplateRemovedEventArgs {
        public criteriaId: number;
        public isTempId: boolean;

        constructor(criteriaId: number, isTempId: boolean) {
            this.criteriaId = criteriaId;
            this.isTempId = isTempId;
        }
    }

    export class UpdateCriteriaNameEventArgs extends CriteriaEventArgsBase {
        public criteriaId: number;

        constructor(criteriaId: number) {
            super(criteriaId);
        }
    }

    export class ActionAddingEventArgs {
        public criteriaId: number;

        constructor(criteriaId: number) {
            this.criteriaId = criteriaId;
        }
    }

    export class ActionAddedEventArgs {
        public criteriaId: number;
        public action: model.Action;

        constructor(criteriaId: number, action: model.Action) {
            this.criteriaId = criteriaId;
            this.action = action;
        }
    }

    export class ActionSelectingEventArgs {
        public criteriaId: number;
        public actionId: number;

        constructor(criteriaId: number, actionId: number) {
            this.criteriaId = criteriaId;
            this.actionId = actionId;
        }
    }

    export class ActionRemovedEventArgs {
        public criteriaId: number;
        public actionId: number;

        constructor(criteriaId: number, actionId: number) {
            this.criteriaId = criteriaId;
            this.actionId = actionId;
        }
    }

    export class TemplateSelectingEventArgs {
    }

    export class UpdateActionEventArgs extends ActionEventArgsBase {
        public userLabel: string;
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTempId: boolean, userLabel: string) {
            super(criteriaId, actionId);
            this.isTempId = isTempId;
            this.userLabel = userLabel;
        }
    }

    export class ProcessNodeTemplateTempIdReplacedEventArgs {
        public tempId: number;
        public id: number;

        constructor(tempId: number, id: number) {
            this.tempId = tempId;
            this.id = id;
        }
    }

    // export class UpdateActionEventArgs {
    //     public criteriaId: number;
    //     public actionId: number;
    //     public actionTempId: number;
    //     public processTemplateId: number;
    // }
} 