/// <reference path="../../_all.ts" />
/// <reference path="../../../typings/angularjs/angular.d.ts"/>
/// <reference path="../../../typings/underscore/underscore.d.ts" />

module dockyard.directives.paneConfigureMapping {
    'use strict';

    export class RenderEventArgs extends RenderEventArgsBase { }
    export class HideEventArgs extends HideEventArgsBase { }

    export class CancelledEventArgs extends CancelledEventArgsBase { }

    export enum MessageType {
        PaneConfigureMapping_ActionUpdated,
        PaneConfigureMapping_Render,
        PaneConfigureMapping_Hide,
        PaneConfigureMapping_UpdateAction
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneConfigureMapping implements ng.IDirective {
        public templateUrl = "/Views/AngularTemplate/PaneConfigureMapping.html";
        public restrict = "E";

        // control works in two modes field (fields would be dropdown) or param (would be dropdown)
        // if nothing is set it works as field mapper
        public scope = {
            mode: "@"
        };



        public controller = ["$scope", "$http", "urlPrefix", ($scope, $http, urlPrefix) => {

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

                var actionDto = {
                    ConfigurationSettings: "{'connection_string':'Data Source= s79ifqsqga.database.windows.net; database = demodb_health; User ID= alexeddodb; Password = Thales89'}",
                    ParentPluginRegistration: "Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1, Core"
                }

                $http.post(urlPrefix + "/actions/field_mapping_targets/", actionDto).then((returnedParams) => {
                    loadedActions = true;
                    var tempToBeMapped = [];

                    returnedParams.data.forEach((actionParam) => {
                        tempToBeMapped.push({ 'type': "actionparam", 'Name': actionParam });
                    });

                    if ($scope.mode === "param") {
                        $scope.toBeMappedTo = tempToBeMapped;
                        $scope.HeadingRight = "Document Fields";
                        $scope.HeadingLeft = "Action Params";
                        return;
                    }
                    $scope.toBeMappedFrom = tempToBeMapped;
                    transform();
                    $scope.HeadingLeft = "Document Fields";
                    $scope.HeadingRight = "Action Params";
                    return;
                });


                var actionWithProcess = { DocuSignTemplateId: "b5abd63a-c12c-4856-b9f4-989200e41a6f" };


                $http.post(urlPrefix + "/actions/field_data_sources/", actionWithProcess )
                    .then((docFields) => {
                        loadedFields = true;
                        var tempToBeMapped = [];
                        docFields.data.forEach((docField) => {
                            tempToBeMapped.push({ 'type': "docusignfield", 'Name': docField });
                        });

                        if ($scope.mode === "param") {
                            $scope.toBeMappedFrom = tempToBeMapped;
                            transform();
                            return;
                        }
                        $scope.toBeMappedTo = tempToBeMapped;

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

            onRender();

            $scope.$on(MessageType[MessageType.PaneConfigureMapping_Render], onRender);
            $scope.$on(MessageType[MessageType.PaneConfigureMapping_Hide], onHide);
            $scope.$on(MessageType[MessageType.PaneConfigureMapping_UpdateAction], onUpdate);

        }];

        public static factory = () => new PaneConfigureMapping();
    }

    //app.run([
    //    "$httpBackend", "urlPrefix", (httpBackend, urlPrefix) => {

    //        var actions = [
    //            { Name: "Action Param1", Id: 11 },
    //            { Name: 'Action Param2', Id: 12 },
    //            { Name: 'Action Param3', Id: 13 },
    //            { Name: 'Action Param4', Id: 14 }
    //        ];

    //        var documentFields = [
    //            { Name: "Field1", Id: 21 },
    //            { Name: "Field2", Id: 22 },
    //            { Name: "Field3", Id: 23 },
    //            { Name: "Field4", Id: 24 },
    //            { Name: "Field5", Id: 25 },
    //            { Name: "Field6", Id: 26 }
    //        ];

    //        httpBackend
    //            .whenGET(urlPrefix + "/actionparams")
    //            .respond(actions);

    //        httpBackend
    //            .whenGET(urlPrefix + "/documentfields")
    //            .respond(documentFields);
    //    }
    //]);
    app.directive("paneConfigureMapping", <any>PaneConfigureMapping.factory);
}