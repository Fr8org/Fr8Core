/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface ISolutionDocumentationScope extends ng.IScope {
        solutionDTO: ng.resource.IResource<model.SolutionDTO>;
        solutionDTOList: Array<model.SolutionDTO>;
        solutionNameList: Array<string>;
        terminalName: string;
    }

    class SolutionDocumentationController {

        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'SolutionDocumentationService',
        ];

        constructor(
            private $scope: ISolutionDocumentationScope,
            private SolutionDocumentationService: services.ISolutionDocumentationService
        ) {

            $scope.solutionDTOList = Array<model.SolutionDTO>();

            SolutionDocumentationService.getSolutionDocumentationList({ terminalName: $scope.terminalName }).$promise.then(function (data) {
                $scope.solutionNameList = data;

                $scope.solutionNameList.forEach( (solutionName: string) => {

                    var activityTemplate = new model.ActivityTemplate("", solutionName, "", "", "");

                    var activityDTO = new model.ActivityDTO("", "", "");
                    activityDTO.toActionVM();
                    activityDTO.documentation = "MainPage";
                    activityDTO.activityTemplate = activityTemplate;

                    SolutionDocumentationService.getSolutionDTO(activityDTO).$promise.then(data=> {
                        var activityName = activityDTO.activityTemplate.name;

                        if (data) {
                            var solutionDTO = new dockyard.model.SolutionDTO(data.name, data.version, data.terminal, data.body, activityName);
                            $scope.solutionDTOList.push(solutionDTO);
                        }
                    });

                });
            });
        }
    }

    app.controller('SolutionDocumentationController', SolutionDocumentationController);
}