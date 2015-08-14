module dockyard.model {
    export class Action implements interfaces.IAction {
        id: number;
        tempId: number;
        name: string;
        criteriaId: number;
        actionType: string;
        actionListId: number
        configurationSettings: string;
        mappingSettigns: string;
        userLabel: string

        constructor(id: number, tempId: number) {
            this.id = id;
            this.tempId = tempId;
        }

        clone(): Action {
            var result = new Action(this.id, this.tempId);
            result.name = this.name;
            return result;
        }
    }
}