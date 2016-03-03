module dockyard.model {    
    /*
        Current state is moved to this class
    */
    export class PlanBuilderState {
        plan: model.PlanDTO;

        // ProcessNodeTemplate for currently edited Crteria. Unlike criteria property, 
        // it is set null as soon as Criteria is done editing.
        subPlan: model.SubPlanDTO;

        // Either currently edited Criteria or context for currently edited Action.
        // Unlike ProcessNodeTemplate, it is not set null when an Action is selected. 
        criteria: model.CriteriaDTO;

        // Current action. This variable is set null as soon as action is done editing.
        activities: model.ActivityDTO;

        constructor() {
            this.criteria = <model.CriteriaDTO>{};
            this.activities = null;
            this.subPlan = null;
            this.plan = null;
        }
    }
}