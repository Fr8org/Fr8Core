module dockyard.model {
    export class ActionList {
        id: number;
        name: string;
        actionListType: ActionListType;
    }

    export enum ActionListType {
        Immediate = 1,
        Scheduled = 2
    }
}
 