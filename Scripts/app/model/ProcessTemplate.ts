module dockyard.model {
    export class ProcessTemplateDTO {
        id: number;
        isTempId: boolean;
        name: string;
        description: string;
        processTemplateState: ProcessState;
        subscribedDocuSignTemplates: Array<string>;
        externalEventSubscription: Array<number>; 
        startingProcessNodeTemplateId: number;
        subroutes: Array<ProcessNodeTemplateDTO>
    }

    export enum ProcessState {
        Inactive = 1,
        Active = 2
    }
}