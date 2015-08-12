/// <reference path="../../_all.ts" />
/// <reference path="../../../typings/angularjs/angular.d.ts"/>
/// <reference path="../../../typings/underscore/underscore.d.ts" />

module dockyard.directives.PaneFieldMapping {
    'use strict';

    export enum MessageType {
        PaneConfigureMapping_ActionUpdated,
        PaneConfigureMapping_Render,
        PaneConfigureMapping_Hide,
        PaneConfigureMapping_UpdateAction
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneFieldMapping implements ng.IDirective {
        public templateUrl = "/Views/AngularTemplate/PaneFieldParamMapper.html";
        public restrict = "E";

        // control works in two modes field (fields would be dropdown) or param (would be dropdown)
        // if nothing is set it works as field mapper
        public scope = {
            mode: "@"
        };

        public controller = ["$scope", "$resource", "urlPrefix", ($scope, $resource, urlPrefix) => {

            var mappedValue = <any>{
                Map: [
                ]
            };


            var transform = () => {
                if (!$scope.toBeMappedFrom)
                    return;
                if ($scope.toBeMappedFrom.constructor !== Array && $scope.toBeMappedFrom < 0)
                    return;
                mappedValue.Map = [];
                var includeOnly = ['Id', 'Name', 'type'];

                $scope.toBeMappedFrom.forEach((current) => {
                    mappedValue.Map.push({
                        from: _.pick(current, includeOnly), to: _.pick(current.mappedTo, includeOnly)
                    });
                });
            };

            function init() {


                var loadedActions = false;
                var loadedFields = false;
                $scope.mappedValue = mappedValue;

                var returnedParams = $resource(urlPrefix + "/actionparams").query(() => {
                    loadedActions = true;
                    returnedParams.forEach((actionParam) => {
                        actionParam.type = "actionparam";
                    });
                    if ($scope.mode === "param") {
                        $scope.toBeMappedTo = returnedParams;
                        $scope.HeadingRight = "Document Fields";
                        $scope.HeadingLeft = "Action Params";
                        return;
                    }
                    $scope.toBeMappedFrom = returnedParams;
                    transform();
                    $scope.HeadingLeft = "Document Fields";
                    $scope.HeadingRight = "Action Params";
                    return;
                });

                var docFields = $resource(urlPrefix + "/actionparams").query(() => {
                    loadedFields = true;
                    docFields.forEach((docField) => {
                        docField.type = "docusignfield";
                    });
                    if ($scope.mode === "param") {
                        $scope.toBeMappedFrom = docFields;
                        transform();
                        return;
                    }
                    $scope.toBeMappedTo = docFields;

                });

                $scope.doneLoading = () => loadedActions && loadedFields;

                $scope.showHeading = () => {
                    if (loadedActions && loadedFields) {
                        return $scope.toBeMappedTo.length > 0 && $scope.toBeMappedFrom.length > 0;
                    }
                    return false;
                }

                $scope.uiDropDownChanged = () => {
                    transform();
                };

            }

            var onRender = () => {
                init();
                transform();
            }

            var onHide = () => {
                $scope.doneLoading = () => { return false }
            };


            var onUpdate = () => { };

            //onRender();

            $scope.$on(MessageType[MessageType.PaneConfigureMapping_Render], onRender);
            $scope.$on(MessageType[MessageType.PaneConfigureMapping_Hide], onHide);
            $scope.$on(MessageType[MessageType.PaneConfigureMapping_UpdateAction], onUpdate);

        }];

        public static factory = () => new PaneFieldMapping();
    }

    app.run([
        "$httpBackend", "urlPrefix", (httpBackend, urlPrefix) => {

            var actions = [
                { Name: "Action Param1", Id: 11 },
                { Name: 'Action Param2', Id: 12 },
                { Name: 'Action Param3', Id: 13 },
                { Name: 'Action Param4', Id: 14 }
            ];

            var documentFields = [
                { Name: "Field1", Id: 21 },
                { Name: "Field2", Id: 22 },
                { Name: "Field3", Id: 23 },
                { Name: "Field4", Id: 24 },
                { Name: "Field5", Id: 25 },
                { Name: "Field6", Id: 26 }
            ];

            httpBackend
                .whenGET(urlPrefix + "/actionparams")
                .respond(actions);

            httpBackend
                .whenGET(urlPrefix + "/documentfields")
                .respond(documentFields);
        }
    ]);
    app.directive("paneFieldParamMapper", <any>PaneFieldMapping.factory);
}