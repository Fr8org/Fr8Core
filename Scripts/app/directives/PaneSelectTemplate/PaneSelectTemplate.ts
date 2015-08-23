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

    export class HideEventArgs extends EventArgsBase { }

    export interface IPaneSelectTemplateScope extends ng.IScope {
        visible: boolean,
        processName: string,
        docuSignTemplates: ng.resource.IResourceArray<interfaces.IDocuSignTemplateVM>,
        docuSignTriggers: ng.resource.IResourceArray<interfaces.IDocuSignTriggerVM>,
        loadingMessage: string,
        doneLoading: () => boolean;
    }


    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneSelectTemplate implements ng.IDirective {
        public templateUrl = "/Views/AngularTemplate/PaneSelectTemplate.html";
        public restrict = "E";
        public scope = {};
        public controller: ($scope: IPaneSelectTemplateScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes) => void;

        constructor(
            private DocuSignTemplateService: ng.resource.IResourceClass<interfaces.IDocuSignTemplateVM>,
            private DocuSignTriggerService: ng.resource.IResourceClass<interfaces.IDocuSignTriggerVM>) {

            PaneSelectTemplate.prototype.controller = (
                $scope: IPaneSelectTemplateScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
                 
                $scope.$on(MessageType[MessageType.PaneSelectTemplate_Render], this.onRender);
                $scope.$on(MessageType[MessageType.PaneSelectTemplate_Hide], this.onHide);
            };
        }

        private init(scope: IPaneSelectTemplateScope) {
            var resetLoadingMessage = () => {
                if (scope.doneLoading)
                    scope.loadingMessage = null;
            };
            scope.doneLoading = () => scope.docuSignTemplates.$resolved && scope.docuSignTriggers.$resolved;
            scope.loadingMessage = "Loading Templates .....";
            scope.docuSignTemplates = this.DocuSignTemplateService.query();
            scope.docuSignTemplates.$promise.then(() => resetLoadingMessage());

            scope.docuSignTriggers = this.DocuSignTriggerService.query();
            scope.docuSignTriggers.$promise.then(() => resetLoadingMessage()); 
        }

        private onRender = (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => {
            var scope = (<IPaneSelectTemplateScope> event.currentScope)
            scope.visible = true;
            this.init(scope);
        }

        private onHide = (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => {
            (<IPaneSelectTemplateScope> event.currentScope).visible = false;
        };

        public static Factory() {
            var directive = (DocuSignTemplateService, DocuSignTriggerService) => {
                return new PaneSelectTemplate(DocuSignTemplateService, DocuSignTriggerService);
            };

            directive["$inject"] = ['DocuSignTemplateService', 'DocuSignTriggerService'];
            return directive;
        }
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
    app.directive("paneSelectTemplate", PaneSelectTemplate.Factory());
}