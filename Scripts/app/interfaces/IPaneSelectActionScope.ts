/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IPaneSelectActionScope extends ng.IScope {
        onActionChanged: (newValue: model.ActionDesignDTO, oldValue: model.ActionDesignDTO, scope: IPaneSelectActionScope) => void;
        action: model.ActionDesignDTO;
        isVisible: boolean;
        actionTypes: Array<model.ActionTemplate>;
        ActionTypeSelected: () => void;
        RemoveAction: () => void;
    }
}