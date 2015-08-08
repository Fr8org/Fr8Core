module dockyard.directives.paneDefineCriteria {

    export enum MessageType {
        PaneDefineCriteria_Render,
        PaneDefineCriteria_Hide,
        PaneDefineCriteria_CriteriaRemoving,
        PaneDefineCriteria_CriteriaUpdating,
        PaneDefineCriteria_Cancelling
    }

    export class RenderEventArgs {
        public fields: Array<model.Field>;
        public criteria: model.Criteria;

        constructor(fields: Array<model.Field>, criteria: model.Criteria) {
            this.fields = fields;
            this.criteria = criteria;
        }
    }

    export class CriteriaRemovingEventArgs {
        public criteriaId: number;

        constructor(criteriaId: number) {
            this.criteriaId = criteriaId;
        }
    }
    export class CriteriaUpdated {
        public criteriaId: number;

        constructor(criteriaId: number) {
            this.criteriaId = criteriaId;
        }
    }
} 