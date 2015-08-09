module dockyard.model {
    export class Action implements interfaces.IAction {
        id: number;
        tempId: number;
        criteriaId: number;
        name: string;
        actionTypeId: number;

        constructor(id: number, tempId: number, criteriaId: number) {
            this.criteriaId = criteriaId;
            this.id = id;
            this.tempId = tempId;
        }

        clone(): Action {
            var result = new Action(this.id, this.tempId, this.criteriaId);
            result.name = this.name;
            result.actionTypeId = this.actionTypeId;

            return result;
        }
    }
}