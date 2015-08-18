module dockyard.model {
    export enum CriteriaExecutionType {
        WithConditions = 1,
        WithoutConditions = 2
    }

    export class Criteria {
        public id: number;
        public isTempId: boolean;
        public processNodeTemplateId: number;
        public conditions: Array<Condition>;
        public executionType: CriteriaExecutionType;

        constructor(
            id: number,
            isTempId: boolean,
            processNodeTemplateId: number,
            executionType: CriteriaExecutionType
        ) {
            this.id = id;
            this.isTempId = isTempId;
            this.processNodeTemplateId = processNodeTemplateId;
            this.executionType = executionType;

            this.conditions = [];
        }

        clone(): Criteria {
            var result = new Criteria(this.id, this.isTempId, this.processNodeTemplateId, this.executionType);
            angular.forEach(this.conditions, function (it) { result.conditions.push(it.clone()); });

            return result;
        }
    }
}
