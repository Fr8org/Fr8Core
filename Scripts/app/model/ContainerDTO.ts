module dockyard.model {
    export class ContainerDTO {
        id: number;
        name: string;
        processTemplateId: number;
        containerState: number;
        currentActivityId: string;
        nextActivityId: string;
    }

    export enum ContainerState {
        Unstarted = 1,
        Executing = 2,
        WaitingForPlugin = 3,
        Completed = 4,
        Failed = 5
    }
}