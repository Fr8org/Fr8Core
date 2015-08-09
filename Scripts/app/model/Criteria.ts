module dockyard.model {
    export enum CriteriaExecutionMode {
        WithConditions = 1,
        WithoutConditions = 2
    }

    export class Criteria {
        public id: number;
        public isTempId: boolean;
        public name: string;
        public actions: Array<Action>;
        public conditions: Array<Condition>;
        public executionMode: CriteriaExecutionMode;

        constructor(id: number, isTempId: boolean, name: string, executionMode: CriteriaExecutionMode) {
            this.id = id;
            this.isTempId = isTempId;
            this.name = name;
            this.executionMode = executionMode;

            this.actions = [];
            this.conditions = [];
        }

        clone(): Criteria {
            var result = new Criteria(this.id, this.isTempId, this.name, this.executionMode);
            this.actions.forEach(function (it) { result.actions.push(it.clone()); });
            this.conditions.forEach(function (it) { result.conditions.push(it.clone()); });

            return result;
        }
    }
}
 