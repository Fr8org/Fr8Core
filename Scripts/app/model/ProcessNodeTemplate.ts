module dockyard.model {
    export class ProcessNodeTemplate {
        public id: number;
        public isTempId: boolean;
        public processTemplateId: number;
        public name: string;
        public criteria: Criteria;
        public actions: Array<Action>;

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

        clone(): ProcessNodeTemplate {
            var result = new ProcessNodeTemplate(this.id, this.isTempId, this.processTemplateId, this.name);
            result.criteria = this.criteria !== null ? this.criteria.clone() : null;
            angular.forEach(this.actions, function (it) { result.actions.push(it.clone()); });

            return result;
        }
    }
}
 