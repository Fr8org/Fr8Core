module dockyard.model {
    export class ActionListDTO {
        id: number;
        name: string;
        actionListType: ActionListType;
        actions: Array<ActionDTO>
    }

    export enum ActionListType {
        Immediate = 1,
        Scheduled = 2
    }
}
 