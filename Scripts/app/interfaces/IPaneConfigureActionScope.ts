/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IPaneConfigureActionScope extends ng.IScope {
        onActionChanged: (newValue: model.Action, oldValue: model.Action, scope: IPaneConfigureActionScope) => void;
        action: model.Action;
        isVisible: boolean;
        cancel: (event: ng.IAngularEvent) => void;
        save: (event: ng.IAngularEvent) => void;
    }
}