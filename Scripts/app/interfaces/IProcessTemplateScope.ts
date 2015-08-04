/// <reference path="../_all.ts" />

module dockyard.interfaces {
    export interface IProcessTemplateScope extends ng.IScope {
        ptvm: interfaces.IProcessTemplateVM;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        pbAddCriteriaClick: () => void,
        pbCriteriaClick: (criteriaId: number) => void,
        pbAddActionClick: (criteriaId: number) => void,
        pbActionClick: (criteriaId: number, actionId: number) => void,
        processBuilder: any
    }
} 