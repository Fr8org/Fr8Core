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
                    
                case model.ContainerState.Executing:
                    return "Executing";
                    
                case model.ContainerState.WaitingForTerminal:
                    return "WaitingForTerminal";
                    
                case model.ContainerState.Completed:
                    return "Completed";
                    
                case model.ContainerState.Failed:
                    return "Failed";
                    
                default:
                    return "";
            }
        });
} 