/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IProcessTemplatesScope extends ng.IScope {
        ptvms: angular.resource.IResourceArray<dockyard.interfaces.IProcessTemplateVM>;
        nav: (pt: IProcessTemplateVM) => void,
        remove: (pt: IProcessTemplateVM) => void
    }
} 