/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IProcessBuilderScope extends ng.IScope {
        processTemplateId: number,
        criteria: Array<model.Criteria>,
        fields: Array<model.Field>
    }
} 