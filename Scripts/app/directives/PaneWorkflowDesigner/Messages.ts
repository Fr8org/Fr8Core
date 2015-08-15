/// <reference path="../../_all.ts" />

module dockyard.directives.paneWorkflowDesigner {
    'use strict';

    export enum MessageType {
        PaneWorkflowDesigner_Render,
        PaneWorkflowDesigner_TemplateSelected,
        PaneWorkflowDesigner_CriteriaAdding,
        PaneWorkflowDesigner_CriteriaAdded,
        PaneWorkflowDesigner_CriteriaSelected,
        PaneWorkflowDesigner_CriteriaRemoved,
        PaneWorkflowDesigner_ActionAdding,
        PaneWorkflowDesigner_ActionAdded,
        PaneWorkflowDesigner_ActionSelected,
        PaneWorkflowDesigner_ActionRemoved,
        // PaneWorkflowDesigner_RefreshElement,
        PaneWorkflowDesigner_UpdateAction,
        PaneWorkflowDesigner_UpdateCriteriaName
    }

    export class RenderEventArgs {
    }

    export class CriteriaAddingEventArgs {
    }

    export class CriteriaAddedEventArgs {
        public criteria: model.Criteria;

        constructor(criteria: model.Criteria) {
            this.criteria = criteria;
        }
    }

    export class CriteriaSelectedEventArgs {
        public criteriaId: number;

        constructor(criteriaId: number) {
            this.criteriaId = criteriaId;
        }
    }

    export class CriteriaRemovedEventArgs {
        public criteriaId: number;

        constructor(criteriaId: number) {
            this.criteriaId = criteriaId;
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

    export class ActionSelectedEventArgs {
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

    export class TemplateSelectedEventArgs {
    }

    export class UpdateActionEventArgs extends ActionEventArgsBase {
        public actionName: string;
        public isTempId: boolean;

        constructor(criteriaId: number, actionId: number, isTempId: boolean, actionName: string) {
            super(criteriaId, actionId);
            this.isTempId = isTempId;
            this.actionName = actionName;
        }
    }

    // export class UpdateActionEventArgs {
    //     public criteriaId: number;
    //     public actionId: number;
    //     public actionTempId: number;
    //     public processTemplateId: number;
    // }
} 