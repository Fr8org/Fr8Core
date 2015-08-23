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
        PaneWorkflowDesigner_ProcessNodeTemplateNameUpdated,
        PaneWorkflowDesigner_ActionAdding,
        PaneWorkflowDesigner_ActionAdded,
        PaneWorkflowDesigner_ActionSelecting,
        PaneWorkflowDesigner_ActionRemoved,
        // PaneWorkflowDesigner_RefreshElement,
        PaneWorkflowDesigner_UpdateAction,
        PaneWorkflowDesigner_ProcessNodeTemplateTempIdReplaced
    }

    export class RenderEventArgs {
    }

    export class ProcessNodeTemplateAddingEventArgs {
    }

    export class ProcessNodeTemplateAddedEventArgs {
        public id: number;
        public isTempId: boolean;
        public name: string;

        constructor(id: number, isTempId: boolean, name: string) {
            this.id = id;
            this.isTempId = isTempId;
            this.name = name;
        }
    }

    export class ProcessNodeTemplateSelectingEventArgs {
        public id: number;
        public isTempId: boolean;

        constructor(id: number, isTempId: boolean) {
            this.id = id;
            this.isTempId = isTempId;
        }
    }

    export class ProcessNodeTemplateRemovedEventArgs {
        public id: number;
        public isTempId: boolean;

        constructor(id: number, isTempId: boolean) {
            this.id = id;
            this.isTempId = isTempId;
        }
    }

    export class ProcessNodeTemplateNameUpdatedEventArgs {
        public id: number;
        public text: string;

        constructor(id: number, text: string) {
            this.id = id;
            this.text = text;
        }
    }

    export class ActionAddingEventArgs {
        public processNodeTemplateId: number;
        public actionListType: model.ActionListType;

        constructor(processNodeTemplateId: number, actionListType: model.ActionListType) {
            this.processNodeTemplateId = processNodeTemplateId;
            this.actionListType = actionListType;
        }
    }

    export class ActionAddedEventArgs {
        public criteriaId: number;
        public action: model.Action;
        public actionListType: model.ActionListType;

        constructor(criteriaId: number,
            action: model.Action,
            actionListType: model.ActionListType) {

            this.criteriaId = criteriaId;
            this.action = action;
            this.actionListType = actionListType;
        }
    }

    export class ActionSelectingEventArgs {
        public criteriaId: number;
        public actionId: number;
        public actionListType: model.ActionListType;

        constructor(criteriaId: number,
            actionId: number,
            actionListType: model.ActionListType) {

            this.criteriaId = criteriaId;
            this.actionId = actionId;
            this.actionListType = actionListType;
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