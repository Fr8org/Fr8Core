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

        // control works in two modes field (fields would be dropdown) or param (would be dropdown) REWRITE THIS
        // if nothing is set it works as field mapper
        public scope = {
            mode: "@",
            currentAction: "="
        };



        public controller = ["$scope", "$http", "urlPrefix", ($scope, $http, urlPrefix) => {

            var updateScopedAction = () => {
                var fields = [];

                if ($scope.mode === 'param') {
                    $scope.toBeMappedFrom.forEach((it) => {
                        if (it.mappedTo && it.mappedTo.name) {
                            fields.push(new model.FieldMapping(it.mappedTo.name, it.name));
                        }
                    });
                }
                else {
                    $scope.toBeMappedFrom.forEach((it) => {
                        if (it.mappedTo && it.mappedTo.name) {
                            fields.push(new model.FieldMapping(it.name, it.mappedTo.name));
                        }
                    });
                }

                if (!$scope.currentAction.fieldMappingSettings) {
                    $scope.currentAction.fieldMappingSettings = new model.FieldMappingSettings();
                }

                $scope.currentAction.fieldMappingSettings.fields = fields;
            };

            function render() {
                var loadedActions = false;
                var loadedFields = false;

                $http
                    .post("/actions/field_mapping_targets/", $scope.currentAction)
                    .then((returnedParams) => {
                        loadedActions = true;

                        var tempToBeMapped = [];

                        returnedParams.data.forEach((actionParam) => {
                            tempToBeMapped.push({ 'type': "actionparam", 'Name': actionParam });
                        });

                        if ($scope.mode === "param") {
                            $scope.toBeMappedTo = tempToBeMapped;
                            $scope.HeadingRight = "Target Data";
                            $scope.HeadingLeft = "Source Data";
                            return;
                        }

                        $scope.toBeMappedFrom = tempToBeMapped;
                        $scope.HeadingLeft = "Source Data";
                        $scope.HeadingRight = "Target Data";

                        return;
                    });

                $http.post("/actions/field_data_sources/", $scope.currentAction)
                    .then((dataSources) => {
                        loadedFields = true;

                        var tempToBeMapped = [];
                        dataSources.data.forEach((docField) => {
                            tempToBeMapped.push({ 'type': "docusignfield", 'Name': docField }); //should be renamed from docusignField to 'data source'
                        });

                        if ($scope.mode === "param") {
                            $scope.toBeMappedFrom = tempToBeMapped;
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
                    updateScopedAction();
                };
            }

            var onRender = () => {
                render();
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