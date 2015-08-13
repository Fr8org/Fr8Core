/// <reference path="../../_all.ts" />
/// <reference path="../../../typings/angularjs/angular.d.ts"/>
/// <reference path="../../../typings/underscore/underscore.d.ts" />

module dockyard.directives.paneSelectTemplate {
    'use strict';

    export enum MessageType {
        PaneSelectTemplate_ProcessTemplateUpdated,
        PaneSelectTemplate_Render,
        PaneSelectTemplate_Hide,
    }

    export class RenderEventArgs extends EventArgsBase {
        constructor() {
            super();
        }
    }

    export class ProcessTemplateUpdatedEventArgs extends EventArgsBase {
        public processTemplateId: number;
        public processTemplateName: string;

        constructor(processTemplateId: number, processTemplateName: string) {
            this.processTemplateId = processTemplateId;
            this.processTemplateName = processTemplateName;
            super();
        }
    }

    export class HideEventArgs extends EventArgsBase {
        constructor() {
            super();
        }
    }


    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneSelectTemplate implements ng.IDirective {
        public templateUrl = "/Views/AngularTemplate/PaneSelectTemplate.html";
        public restrict = "E";

        public controller = ["$scope", "$resource", "urlPrefix", ($scope, $resource, urlPrefix) => {
            $scope.Visible = true;
            $scope.DataModel = {};
            $scope.DataModel.ProcessName = "My Process";
            function init() {

                var loadedDocuTemplates = false;
                var loadedTriggers = false;

                var resetLoadingMessage = () => {
                    if (loadedDocuTemplates && loadedTriggers)
                        $scope.loadingMessage = null;
                };


                var docusignTemplates = $resource(urlPrefix + "/docusigntemplates").query(() => {
                    loadedDocuTemplates = true;
                    $scope.docusignTemplates = docusignTemplates;
                    $scope.DataModel.SelectedDocuSignTemplate = _.sample(docusignTemplates);
                    resetLoadingMessage();
                    return;
                });

                var triggers = $resource(urlPrefix + "/processtemplate/triggersettings").query(() => {
                    loadedTriggers = true;
                    resetLoadingMessage();
                    $scope.triggers = triggers;

                });

                $scope.doneLoading = () => loadedDocuTemplates && loadedTriggers;

                $scope.loadingMessage = "Loading Templates .....";

            }

            var onRender = () => {
                init();

            }

            var onHide = () => {
                $scope.Visible = false;
            };

            onRender();

            $scope.$on(MessageType[MessageType.PaneSelectTemplate_Render], onRender);
            $scope.$on(MessageType[MessageType.PaneSelectTemplate_Hide], onHide);


        }];

        public static factory = () => new PaneSelectTemplate();
    }

    app.run([
        "$httpBackend", "urlPrefix", (httpBackend, urlPrefix) => {

            var triggerSettings = [
                { Name: "Envelope Sent", Id: 11 },
                { Name: "Envelope Delivered", Id: 12 },
                { Name: "Envelope Signed", Id: 13 },
                { Name: "Envelope Completed", Id: 14 }
            ];

            var docuSignTemplates = [
                { Name: "Contract", Id: 21 },
                { Name: "Invoice", Id: 22 },
                { Name: "Letter of Approval", Id: 23 },
                { Name: "Deed", Id: 24 },
                { Name: "Lease", Id: 25 },
                { Name: "Aggreement", Id: 26 }
            ];
            
            httpBackend
                .whenGET(urlPrefix + "/processtemplate/triggersettings")
                .respond(triggerSettings);

            httpBackend
                .whenGET(urlPrefix + "/docusigntemplates")
                .respond(docuSignTemplates);
        }
    ]);
    app.directive("paneSelectTemplate", <any>PaneSelectTemplate.factory);
}