module dockyard.model {
    export class Action {
        id: number;
        isTempId: boolean;
        criteriaId: number;
        name: string;
        actionTypeId: number;

        constructor(id: number, isTempId: boolean, criteriaId: number) {
            this.criteriaId = criteriaId;
            this.id = id;
            this.isTempId = isTempId;
        }

        clone(): Action {
            var result = new Action(this.id, this.isTempId, this.criteriaId);
            result.name = this.name;
            result.actionTypeId = this.actionTypeId;

            return result;
        }
    }
}