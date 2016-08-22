module dockyard.model {
    export enum CriteriaExecutionType {
        NoSet = 0,
        WithConditions = 1,
        WithoutConditions = 2
    }

    export class CriteriaDTO implements interfaces.ICriteriaDTO {
        public id: string;
        public isTempId: boolean;
        public processNodeTemplateId: string;
        public conditions: Array<Condition>;
        public executionType: CriteriaExecutionType;
        public userLabel: string;
        public activities: Array<ActivityDTO>;

        constructor(
            id: string,
            isTempId: boolean,
            processNodeTemplateId: string,
            executionType: CriteriaExecutionType
        ) {
            this.id = id;
            this.isTempId = isTempId;
            this.processNodeTemplateId = processNodeTemplateId;
            this.executionType = executionType;

            this.conditions = [];
        }

        clone(): CriteriaDTO {
            var result = new CriteriaDTO(this.id, this.isTempId, this.processNodeTemplateId, this.executionType);
            angular.forEach(this.conditions, function (it) { result.conditions.push(it.clone()); });

            return result;
        }
    }
}
