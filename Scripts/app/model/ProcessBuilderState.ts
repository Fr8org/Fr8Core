module dockyard.model {    
    /*
        Current state is moved to this class
    */
    export class ProcessBuilderState {
        processTemplate: model.ProcessTemplateDTO;

        // ProcessNodeTemplate for currently edited Crteria. Unlike criteria property, 
        // it is set null as soon as Criteria is done editing.
        processNodeTemplate: model.ProcessNodeTemplateDTO;

        // Either currently edited Criteria or context for currently edited Action.
        // Unlike ProcessNodeTemplate, it is not set null when an Action is selected. 
        criteria: model.CriteriaDTO;

        // Current action. This variable is set null as soon as action is done editing.
        action: model.ActionDesignDTO;

        constructor() {
            this.criteria = <model.CriteriaDTO> {};
            this.action = null;
            this.processNodeTemplate = null;
            this.processTemplate = null;
        }
    }
}