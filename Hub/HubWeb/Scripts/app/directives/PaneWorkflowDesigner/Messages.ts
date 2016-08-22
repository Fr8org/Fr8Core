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
        PaneWorkflowDesigner_ActionTempIdReplaced,
        PaneWorkflowDesigner_UpdateActivityTemplateId,
        PaneWorkflowDesigner_LongRunningOperation,
    }

    export enum LongRunningOperationFlag {
        Started,
        Stopped
    }

    export class LongRunningOperationEventArgs {
        public flag: LongRunningOperationFlag;

        constructor(flag: LongRunningOperationFlag) {
            this.flag = flag;
        }
    }

    export class RenderEventArgs {
    }

    export class CriteriaAddingEventArgs {
    }

    export class AddCriteriaEventArgs {
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

        constructor(processNodeTemplateId: number) {
            this.processNodeTemplateId = processNodeTemplateId;
        }
    }

    export class AddActionEventArgs {
        public criteriaId: string;
        public action: model.ActivityDTO;
        public doNotRaiseSelectedEvent: boolean;

        constructor(criteriaId: string,
            action: model.ActivityDTO,
            doNotRaiseSelectedEvent?: boolean) {

            this.criteriaId = criteriaId;
            this.action = action;
            this.doNotRaiseSelectedEvent = doNotRaiseSelectedEvent;
        }
    }

    export class ActionSelectedEventArgs {
        public processNodeTemplateId: string;
        public actionId: string;
        public activityTemplateId: number;

        constructor(
            processNodeTemplateId: string,
            actionId: string,
            activityTemplateId: number) {

            this.processNodeTemplateId = processNodeTemplateId;
            this.actionId = actionId;
            this.activityTemplateId = activityTemplateId;
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

    export class ReplaceTempIdForProcessNodeTemplateEventArgs {
        public tempId: number;
        public id: number;

        constructor(tempId: number, id: number) {
            this.tempId = tempId;
            this.id = id;
        }
    }

    export class ActionTempIdReplacedEventArgs {
        public tempId: string;
        public id: string;

        constructor(tempId: string, id: string) {
            this.tempId = tempId;
            this.id = id;
        }
    }

    export class UpdateActivityTemplateIdEventArgs {
        public id: string;
        public activityTemplateId: string;

        constructor(id: string, activityTemplateId: string) {
            this.id = id;
            this.activityTemplateId = activityTemplateId;
        }
    }

    // export class UpdateActionEventArgs {
    //     public criteriaId: number;
    //     public actionId: number;
    //     public actionTempId: number;
    //     public processTemplateId: number;
    // }
} 