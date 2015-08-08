/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IPaneConfigureActionScope extends ng.IScope {
        onActionChanged: (newValue: IAction, oldValue: IAction, scope: IPaneConfigureActionScope) => void;
        action: IAction;
        isVisible: boolean;
        cancel: (event: ng.IAngularEvent) => void;
        save: (event: ng.IAngularEvent) => void;
    }
}