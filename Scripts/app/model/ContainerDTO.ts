module dockyard.model {
    export class ContainerDTO {
        id: number;
        Name: string;
        description: string;
        ProcessTemplateId: number;
        ProcessState: number;
        CurrentActivityId: string;
        NextActivityId: string;
    }

    export enum ContainerState {
        Inactive = 1,
        Active = 2
    }
}