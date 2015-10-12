/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IAppRootScope extends ng.IScope {
        lastResult: string;
        obtype: string;
        obdatefield: string;
        obdateoperator: string;
        obdatevalue: string;
        obidfield: string;
        obidoperator: string;
        obidvalue: string;
    }
}