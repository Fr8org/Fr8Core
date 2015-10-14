module dockyard.model {
    export class ContainerDTO {
        id: number;
        name: string;
        processTemplateId: number;
        processState: number;
        currentActivityId: string;
        nextActivityId: string;
    }

    export enum ContainerState {
        Inactive = 1,
        Active = 2
    }
}