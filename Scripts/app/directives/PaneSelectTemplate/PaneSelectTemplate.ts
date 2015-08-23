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

        private _$scope: IPaneSelectTemplateScope;

        constructor(
            private $q: ng.IQService,
            private $timeout: ng.ITimeoutService,
            private $stateParams: ng.ui.IStateParamsService,
            private DocuSignTemplateService: services.IDocuSignTemplateService,
            private DocuSignTriggerService: services.IDocuSignTriggerService,
            private ProcessTemplateService: services.IProcessTemplateService) {

            PaneSelectTemplate.prototype.controller = (
                $scope: IPaneSelectTemplateScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {

                this._$scope = $scope;
                $scope.doneLoading = false;
                $scope.$on(MessageType[MessageType.PaneSelectTemplate_Render], this.onRender);
                $scope.$on(MessageType[MessageType.PaneSelectTemplate_Hide], this.onHide);

                $scope.cancel = <(curScope: IPaneSelectTemplateScope) => ng.IPromise<interfaces.IProcessTemplateVM>>
                    angular.bind(this, this.cancel);
                $scope.save = <(curScope: IPaneSelectTemplateScope) => ng.IPromise<interfaces.IProcessTemplateVM>>
                    angular.bind(this, this.save);
            };
        }

        private init(curScope: IPaneSelectTemplateScope) {

            curScope.loadingMessage = "Loading Templates .....";

            curScope.processTemplate = this.ProcessTemplateService.get({id: this.$stateParams["id"]});
            curScope.docuSignTemplates = this.DocuSignTemplateService.query();
            curScope.docuSignExternalEvents = this.DocuSignTriggerService.query();

            this.$q.all([
                curScope.docuSignTemplates.$promise,
                curScope.docuSignExternalEvents.$promise,
                curScope.processTemplate.$promise]
            ).then(
                () => {
                    console.log(curScope.processTemplate);
                    if (curScope.processTemplate && curScope.processTemplate.SubscribedDocuSignTemplates.length > 0)
                    {
                        curScope.docuSignTemplateId = curScope.processTemplate.SubscribedDocuSignTemplates[0];
                    }
                    curScope.doneLoading = true
                }
            );
        }

        private onRender = (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => {
            var curScope = (<IPaneSelectTemplateScope> event.currentScope)
            curScope.visible = true;
            this.init(curScope);
        }

        private onHide = (event: ng.IAngularEvent, eventArgs: RenderEventArgs) => {
            var curScope = <IPaneSelectTemplateScope> event.currentScope;
            curScope.visible = false;
            this.save(curScope);
        };

        public save(scope) {
            if (this._$scope.processTemplate != null) {
                console.log(this._$scope.processTemplate);
                //Add selected DocuSign template
                debugger;
                this._$scope.processTemplate.SubscribedDocuSignTemplates.splice(
                    0,
                    1,
                    $('#docuSignTemplate').val().replace('string:', '')); //a hack required since $scope contained old value when save is triggered by 'Hide' message
                return this.ProcessTemplateService.save(
                    {
                           updateRegistrations: true //update template and trigger registrations
                    },
                    this._$scope.processTemplate).$promise;
            }
        }

        public cancel(curScope: IPaneSelectTemplateScope) {
            curScope.visible = false;
        }

        public static Factory() {
            var directive = (
                $q: ng.IQService,
                $timeout: ng.ITimeoutService,
                $stateParams: ng.ui.IStateParamsService,
                DocuSignTemplateService: services.IDocuSignTemplateService,
                DocuSignTriggerService: services.IDocuSignTriggerService,
                ProcessTemplateService: services.IProcessTemplateService) =>
            {
                return new PaneSelectTemplate(
                    $q,
                    $timeout,
                    $stateParams,
                    DocuSignTemplateService,
                    DocuSignTriggerService,
                    ProcessTemplateService);
            };

            directive["$inject"] = ['$q', '$timeout', '$stateParams', 'DocuSignTemplateService', 'DocuSignTriggerService', 'ProcessTemplateService'];
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