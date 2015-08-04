/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IProcessTemplateScope extends ng.IScope {
        ptvm: interfaces.IProcessTemplateVM;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        processBuilder: any
    }
} 