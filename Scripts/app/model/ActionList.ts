module dockyard.model {
    export class ActionList {
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
 