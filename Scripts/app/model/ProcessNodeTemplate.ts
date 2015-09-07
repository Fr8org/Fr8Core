module dockyard.model {
    export class ProcessNodeTemplateDTO {
        public id: number;
        public isTempId: boolean;
        public processTemplateId: number;
        public name: string;
        public criteria: CriteriaDTO;
        public actions: Array<ActionDesignDTO>;

        constructor(
            id: number,
            isTempId: boolean,
            processTemplateId: number,
            name: string
        ) {
            this.id = id;
            this.isTempId = isTempId;
            this.processTemplateId = processTemplateId;
            this.name = name;

            this.criteria = null;
            this.actions = [];
        }

        clone(): ProcessNodeTemplateDTO {
            var result = new ProcessNodeTemplateDTO(this.id, this.isTempId, this.processTemplateId, this.name);
            result.criteria = this.criteria !== null ? this.criteria.clone() : null;
            angular.forEach(this.actions, function (it) { result.actions.push(it.clone()); });

            return result;
        }
    }
}
 