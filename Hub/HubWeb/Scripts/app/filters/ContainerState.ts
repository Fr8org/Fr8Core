/// <reference path="../_all.ts" />

/*
    The filter converts numeric value to container state name
*/
module dockyard {
    'use strict';
    app.filter('State', () =>
        function (input: number): string {
            switch (input) {
                case model.State.Unstarted:
                    return "Unstarted";
                    
                case model.State.Executing:
                    return "Executing";
                    
                case model.State.WaitingForTerminal:
                    return "WaitingForTerminal";
                    
                case model.State.Completed:
                    return "Completed";
                    
                case model.State.Failed:
                    return "Failed";
                case model.State.Suspended:
                    return "Suspended";
                case model.State.Deleted:
                    return "Deleted";
                default:
                    return "";
            }
        });
} 