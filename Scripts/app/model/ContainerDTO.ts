module dockyard.model {
    export class ContainerDTO{
        id: number;
        name: string;
        routeId: number;
        containerState: number;
        currentRouteNodeId: string;
        nextRouteNodeId: string;
    }

    export enum ContainerState {
        Unstarted = 1,
        Executing = 2,
        WaitingForTerminal = 3,
        Completed = 4,
        Failed = 5
    }
}