module dockyard.model {
    export class Action {
        id: number;
        isTempId: boolean;
        criteriaId: number;
        actionType: string;
        actionListId: number
        configurationSettings: string;
        fieldMappingSettings: string;
        userLabel: string;

        constructor(id: number, isTempId: boolean, criteriaId: number) {
            this.criteriaId = criteriaId;
            this.id = id;
            this.isTempId = isTempId;
        }

        clone(): Action {
            var result = new Action(this.id, this.isTempId, this.criteriaId);
            result.userLabel = this.userLabel;
            result.actionType = this.actionType;

            result.userLabel = this.userLabel;
            return result;
        }
    }
}