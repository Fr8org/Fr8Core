module dockyard.model {
    export class SubrouteDTO {
        public id: string;
        public isTempId: boolean;
        public routeId: string;
        public name: string;
        public criteria: CriteriaDTO;
        public actions: Array<ActionDTO>;

        constructor(
            id: string,
            isTempId: boolean,
            routeId: string,
            name: string
        ) {
            this.id = id;
            this.isTempId = isTempId;
            this.routeId = routeId;
            this.name = name;

            this.criteria = null;
            this.actions = [];
        }

        clone(): SubrouteDTO {
            var result = new SubrouteDTO(this.id, this.isTempId, this.routeId, this.name);
            result.criteria = this.criteria !== null ? this.criteria.clone() : null;
            angular.forEach(this.actions, function (it) { result.actions.push(it.clone()); });

            return result;
        }

        // Create and return empty ProcessNodeTemplate object,
        // if user selects just newly created Criteria diamond on WorkflowDesigner pane.
        static create(
            routeId,
            subrouteId,
            criteriaId): model.SubrouteDTO {

            // Create new ProcessNodeTemplate object with default name and provided temporary id.
            var subroute = new model.SubrouteDTO(
                subrouteId,
                true,
                routeId,
                'New criteria'
                );

            // Create criteria with default conditions, and temporary criteria.id.
            var criteria = new model.CriteriaDTO(
                criteriaId,
                true,
                subroute.id,
                model.CriteriaExecutionType.WithConditions
                );

            subroute.criteria = criteria;

            return subroute;
        };
    }
}
 