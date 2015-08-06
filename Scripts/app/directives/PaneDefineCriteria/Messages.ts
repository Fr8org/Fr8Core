module dockyard.directives.paneDefineCriteria {

    export enum MessageType {
        PaneDefineCriteria_Render,
        PaneDefineCriteria_Hide,
        PaneDefineCriteria_CriteriaRemoved,
        PaneDefineCriteria_CriteriaUpdated,
        PaneDefineCriteria_Cancelled
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

} 