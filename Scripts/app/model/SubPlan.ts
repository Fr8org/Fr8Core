module dockyard.model {
    export class SubPlanDTO {
        public id: string;
        public isTempId: boolean;
        public planId: string;
        public parentPlanNodeId: string;
        public name: string;
        public criteria: CriteriaDTO;
        public activities: Array<ActivityDTO>;

        constructor(
            id: string,
            isTempId: boolean,
            planId: string,
            parentPlanNodeId: string,
            name: string
        ) {
            this.id = id;
            this.isTempId = isTempId;
            this.planId = planId;
            this.parentPlanNodeId = parentPlanNodeId;
            this.name = name;

            this.criteria = null;
            this.activities = [];
        }

        clone(): SubPlanDTO {
            var result = new SubPlanDTO(this.id, this.isTempId, this.planId, this.parentPlanNodeId, this.name);
            result.criteria = this.criteria !== null ? this.criteria.clone() : null;

            angular.forEach(this.activities, function (it) { result.activities.push(it.clone()); });

            return result;
        }

        // Create and return empty ProcessNodeTemplate object,
        // if user selects just newly created Criteria diamond on WorkflowDesigner pane.
        static create(
            planId,
            subPlanId,
            criteriaId): model.SubPlanDTO {

            // Create new ProcessNodeTemplate object with default name and provided temporary id.
            var subPlan = new model.SubPlanDTO(
                subPlanId,
                true,
                planId,
                planId,
                'New criteria'
            );

            // Create criteria with default conditions, and temporary criteria.id.
            var criteria = new model.CriteriaDTO(
                criteriaId,
                true,
                subPlan.id,
                model.CriteriaExecutionType.WithConditions
            );

            subPlan.criteria = criteria;

            return subPlan;
        };
    }
}
 