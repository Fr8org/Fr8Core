module dockyard.model {
    export class Action implements interfaces.IAction {
        id: number;
        isTempId: boolean;
        criteriaId: number;
        actionType: string;
        actionListId: number
        configurationSettings: string;
        fieldMappingSettings: string;
        userLabel: string

        constructor(id: number, isTempId: boolean) {
            this.id = id;
            this.isTempId = isTempId;
        }

        clone(): Action {
            var result = new Action(this.id, this.isTempId);
            result.userLabel = this.userLabel;
            return result;
        }
    }
}