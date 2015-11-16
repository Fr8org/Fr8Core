/// <reference path="../_all.ts" />

/*
    The filter converts numeric value to container state name
*/
module dockyard {
    'use strict';
    app.filter('ContainerState', () =>
        function (input: number): string {
            switch (input) {
                case model.ContainerState.Unstarted:
                    return "Unstarted";
                    break;
                case model.ContainerState.Executing:
                    return "Executing";
                    break;
                case model.ContainerState.WaitingForTerminal:
                    return "WaitingForTerminal";
                    break;
                case model.ContainerState.Completed:
                    return "Completed";
                    break;
                case model.ContainerState.Failed:
                    return "Failed";
                    break;
                default:
                    return "";
            }
        });
} 