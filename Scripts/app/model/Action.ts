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
    }
}