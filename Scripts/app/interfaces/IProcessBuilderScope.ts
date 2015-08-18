/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IProcessBuilderScope extends ng.IScope {
        processTemplateId: number,
        processNodeTemplates: Array<model.ProcessNodeTemplate>,
        fields: Array<model.Field>,
        currentProcessNodeTemplate: model.ProcessNodeTemplate,
        currentAction: IActionVM,
        Save: Function,
        Cancel: Function
    }
} 