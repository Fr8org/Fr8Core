/// <reference path="../../_all.ts" />

module dockyard.directives.paneWorkflowDesigner {
    'use strict';

    export enum MessageType {
        PaneWorkflowDesigner_Render,
        PaneWorkflowDesigner_TemplateSelected,
        PaneWorkflowDesigner_ProcessNodeTemplateAdding,
        PaneWorkflowDesigner_AddCriteria,
        PaneWorkflowDesigner_CriteriaSelected,
        PaneWorkflowDesigner_RemoveCriteria,
        PaneWorkflowDesigner_UpdateProcessNodeTemplateName,
        PaneWorkflowDesigner_ActionAdding,
        PaneWorkflowDesigner_AddAction,
        PaneWorkflowDesigner_ActionSelected,
        PaneWorkflowDesigner_ActionRemoved,
        PaneWorkflowDesigner_ActionNameUpdated,
        PaneWorkflowDesigner_ReplaceTempIdForProcessNodeTemplate,
        PaneWorkflowDesigner_ActionTempIdReplaced
    }

    export class RenderEventArgs {
    }

    export class CriteriaAddingEventArgs {
    }

    export class AddProcessNodeTemplateEventArgs {
        public id: number;
        public isTempId: boolean;
        public name: string;

        constructor(id: number, isTempId: boolean, name: string) {
            this.id = id;
            this.isTempId = isTempId;
            this.name = name;
        }
    }

    export class CriteriaSelectedEventArgs {
        public id: number;
        public isTempId: boolean;

        constructor(id: number, isTempId: boolean) {
            this.id = id;
            this.isTempId = isTempId;
        }
    }

    export class RemoveCriteriaEventArgs {
        public id: number;
        public isTempId: boolean;

        constructor(id: number, isTempId: boolean) {
            this.id = id;
            this.isTempId = isTempId;
        }
    }

    export class UpdateProcessNodeTemplateNameEventArgs {
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

    export class AddActionEventArgs {
        public criteriaId: number;
        public action: model.ActionDesignDTO;
        public actionListType: model.ActionListType;

        constructor(criteriaId: number,
            action: model.ActionDesignDTO,
            actionListType: model.ActionListType) {

            this.criteriaId = criteriaId;
            this.action = action;
            this.actionListType = actionListType;
        }
    }

    export class ActionSelectedEventArgs {
        public processNodeTemplateId: number;
        public actionId: number;
        public actionListId: number;

        constructor(
            processNodeTemplateId: number,
            actionId: number,
            actionListId: number) {

            this.processNodeTemplateId = processNodeTemplateId;
            this.actionId = actionId;
            this.actionListId = actionListId;
        }
    }

    export class ActionRemovedEventArgs {
        public id: number;
        public isTempId: boolean;

        constructor(id: number, isTempId: boolean) {
            this.id = id;
            this.isTempId = isTempId;
        }
    }

    export class TemplateSelectedEventArgs {
    }

    export class ActionNameUpdatedEventArgs {
        public id: number;
        public name: string;

        constructor(id: number, name: string) {
            this.id = id;
            this.name = name;
        }
    }

    export class ReplaceTempIdForProcessNodeTemplateEventArgs {
        public tempId: number;
        public id: number;

        constructor(tempId: number, id: number) {
            this.tempId = tempId;
            this.id = id;
        }
    }

    export class ActionTempIdReplacedEventArgs {
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