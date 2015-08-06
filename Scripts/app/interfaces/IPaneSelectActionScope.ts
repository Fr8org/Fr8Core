/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IPaneSelectActionScope extends ng.IScope {
        onActionChanged: (newValue: IAction, oldValue: IAction, scope: IPaneSelectActionScope) => void;
        action: IAction;
        isVisible: boolean;
        sampleActionTypes: Array<{ name: string, value: string }>;
    }
}