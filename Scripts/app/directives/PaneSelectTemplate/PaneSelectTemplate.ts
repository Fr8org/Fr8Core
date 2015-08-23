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
        docuSignTemplates: ng.resource.IResourceArray<interfaces.IDocuSignTemplateVM>;
        docuSignExternalEvents: ng.resource.IResourceArray<interfaces.IDocuSignExternalEventVM>;
        loadingMessage: string;
        doneLoading: boolean;
        processTemplate: interfaces.IProcessTemplateVM;
        save: (curScope: IPaneSelectTemplateScope) => ng.IPromise<interfaces.IProcessTemplateVM>;
        cancel: (curScope: IPaneSelectTemplateScope) => void;
        docuSignTemplateId: string;            
    }
    
    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class PaneSelectTemplate implements ng.IDirective {
        public templateUrl = "/Views/AngularTemplate/PaneSelectTemplate.html";
        public restrict = "E";
        public scope = {};
        public controller: ($scope: IPaneSelectTemplateScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes) => void;

        constructor(
            private $q: ng.IQService,
            private $stateParams: ng.ui.IStateParamsService,
            private DocuSignTemplateService: services.IDocuSignTemplateService,
            private DocuSignTriggerService: services.IDocuSignTriggerService,
            private ProcessTemplateService: services.IProcessTemplateService) {

            PaneSelectTemplate.prototype.controller = (
                $scope: IPaneSelectTemplateScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {

                $scope.doneLoading = false;
                $scope.$on(MessageType[MessageType.PaneSelectTemplate_Render], this.onRender);
                $scope.$on(MessageType[MessageType.PaneSelectTemplate_Hide], this.onHide);

                $scope.cancel = <(curScope: IPaneSelectTemplateScope) => ng.IPromise<interfaces.IProcessTemplateVM>>
                    angular.bind(this, this.cancel);
                $scope.save = <(curScope: IPaneSelectTemplateScope) => ng.IPromise<interfaces.IProcessTemplateVM>>
                    angular.bind(this, this.save);
            };
        }

        private init(scope: IPaneSelectTemplateScope) {

            scope.loadingMessage = "Loading Templates .....";

            scope.processTemplate = this.ProcessTemplateService.get({id: this.$stateParams["id"]});
            scope.docuSignTemplates = this.DocuSignTemplateService.query();
            scope.docuSignExternalEvents = this.DocuSignTriggerService.query();

            this.$q.all([
                scope.docuSignTemplates.$promise,
                scope.docuSignExternalEvents.$promise,
                scope.processTemplate.$promise]
            ).then(
                () => {
                    console.log(scope.processTemplate);
                    if (scope.processTemplate && scope.processTemplate.SubscribedDocuSignTemplates.length > 0)
                    {
                        scope.docuSignTemplateId = scope.processTemplate.SubscribedDocuSignTemplates[0];
                    }
                    scope.doneLoading = true
                }
            );
        }

        private onRender = (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => {
            var scope = (<IPaneSelectTemplateScope> event.currentScope)
            scope.visible = true;
            this.init(scope);
        }

        private onHide = (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => {
            (<IPaneSelectTemplateScope> event.currentScope).visible = false;
        };

        public save(curScope: IPaneSelectTemplateScope) {
            if (curScope.processTemplate != null) {
                console.log(curScope.processTemplate);
                //Add selected DocuSign template
                curScope.processTemplate.SubscribedDocuSignTemplates.splice(0, 1, curScope.docuSignTemplateId);
                return this.ProcessTemplateService.save(
                    {
                           updateRegistrations: true //update template and trigger registrations
                    },
                    curScope.processTemplate).$promise;
            }
        }

        public cancel(curScope: IPaneSelectTemplateScope) {
            curScope.visible = false;
        }

        public static Factory() {
            var directive = (
                $q: ng.IQService,
                $stateParams: ng.ui.IStateParamsService,
                DocuSignTemplateService: services.IDocuSignTemplateService,
                DocuSignTriggerService: services.IDocuSignTriggerService,
                ProcessTemplateService: services.IProcessTemplateService) =>
            {
                return new PaneSelectTemplate(
                    $q,
                    $stateParams,
                    DocuSignTemplateService,
                    DocuSignTriggerService,
                    ProcessTemplateService);
            };

            directive["$inject"] = ['$q', '$stateParams', 'DocuSignTemplateService', 'DocuSignTriggerService', 'ProcessTemplateService'];
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