/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface ISelectActionPaneScope extends IPaneScope {
        onActionTypeChanged: (newValue: number, oldValue: number, scope: ISelectActionPaneScope) => void;
        actionTypeId: number;
    }
}