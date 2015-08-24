/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IProcessBuilderScope extends ng.IScope {
        processTemplateId: number,
        processNodeTemplates: Array<model.ProcessNodeTemplate>,
        fields: Array<model.Field>,

        // Identity of currently selected processNodeTemplate.
        curNodeId: number,
        // Flag, that indicates if currently selected processNodeTemplate has temporary identity.
        curNodeIsTempId: boolean,

        currentAction: IActionVM,
        Save: Function,
        Cancel: Function
    }
} 