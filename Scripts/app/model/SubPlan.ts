module dockyard.model {
    export class SubPlanDTO {
        public subPlanId: string;
        public isTempId: boolean;
        public planId: string;
        public parentId: string;
        public name: string;
        public criteria: CriteriaDTO;
        public activities: Array<ActivityDTO>;
        public runnable: boolean;

        constructor(
            id: string,
            isTempId: boolean,
            planId: string,
            parentId: string,
            name: string
        ) {
            this.subPlanId = id;
            this.isTempId = isTempId;
            this.planId = planId;
            this.parentId = parentId;
            this.name = name;

            this.criteria = null;
            this.activities = [];

            this.runnable = true;
        }

        clone(): SubPlanDTO {
            var result = new SubPlanDTO(this.subPlanId, this.isTempId, this.planId, this.parentId, this.name);
            result.criteria = this.criteria !== null ? this.criteria.clone() : null;
            result.runnable = this.runnable;

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
                subPlan.subPlanId,
                model.CriteriaExecutionType.WithConditions
            );

            subPlan.criteria = criteria;

            return subPlan;
        };
    }
}
 