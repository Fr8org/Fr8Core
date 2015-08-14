/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IPaneSelectActionScope extends ng.IScope {
        onActionChanged: (newValue: model.Action, oldValue: model.Action, scope: IPaneSelectActionScope) => void;
        action: model.Action;
        isVisible: boolean;
        sampleActionTypes: Array<{ name: string, value: string }>;
        ActionTypeSelected: () => void;
    }
}