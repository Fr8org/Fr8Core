/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/

module dockyard.controllers {
    'use strict';

    export interface ISandboxScope extends ng.IScope {
        routeId: number;
        processNodeTemplates: Array<model.SubrouteDTO>,
        fields: Array<model.Field>;

        // Identity of currently edited processNodeTemplate.
        //curNodeId: number;
        //// Flag, that indicates if currently edited processNodeTemplate has temporary identity.
        //curNodeIsTempId: boolean;
        current: model.RouteBuilderState,
        save: Function;
        cancel: Function;

        //this is for demo only, should be deleted on production
        radioDemoField: model.RadioButtonGroup;
        dropdownDemoField: model.DropDownList;
        textBlockDemoField: model.TextBlockField;
        routingControlGroup: model.RoutingControlGroup;
    }

    //Setup aliases
    import pwd = dockyard.directives.paneWorkflowDesigner;
    import pca = dockyard.directives.paneConfigureAction;

    class SandboxController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di


        public static $inject = [
            '$rootScope',
            '$scope',
            'StringService',
            'LocalIdentityGenerator',
            '$state',
            'ActionService',
            '$q',
            '$http',
            'RouteService',
            '$timeout',
            'CriteriaServiceWrapper',
            'RouteBuilderService',
            'ActionListService',
            'CrateHelper',
            'ActivityTemplateService'
        ];

        private _scope: ISandboxScope;

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: ISandboxScope,
            private StringService: services.IStringService,
            private LocalIdentityGenerator: services.ILocalIdentityGenerator,
            private $state: ng.ui.IState,
            private ActionService: services.IActionService,
            private $q: ng.IQService,
            private $http: ng.IHttpService,
            private RouteService: services.IRouteService,
            private $timeout: ng.ITimeoutService,
            private CriteriaServiceWrapper: services.ICriteriaServiceWrapper,
            private RouteBuilderService: services.IRouteBuilderService,
            
            private CrateHelper: services.CrateHelper,
            private ActivityTemplateService: services.IActivityTemplateService
            ) {
            this._scope = $scope;
            this._scope.routeId = $state.params.id;


            this._scope.processNodeTemplates = [];
            this._scope.fields = [];
            this._scope.current = new model.RouteBuilderState();

            //THIS IS FOR DEMO ONLY
            var radioDemoField = new model.RadioButtonGroup();
            radioDemoField.fieldLabel = 'Demo Label';
            radioDemoField.groupName = 'Demo Group Name';
            radioDemoField.type = 'radioButtonGroup';
            var demoRadio1 = new model.RadioButtonOption();
            demoRadio1.value = "Selection 1";
            demoRadio1.selected = false;
            var demoRadio2 = new model.RadioButtonOption();
            demoRadio2.value = "Selection 2";
            demoRadio2.selected = false;
            var demoRadio3 = new model.RadioButtonOption();
            demoRadio3.value = "Selection 3";
            demoRadio3.selected = true;
            var radios = new Array<model.RadioButtonOption>();
            radios.push(demoRadio1);
            radios.push(demoRadio2);
            radios.push(demoRadio3);
            radioDemoField.radios = radios;
            this._scope.radioDemoField = radioDemoField;

            var dropdownDemoField = new model.DropDownList();
            var demoSelectItem1 = new model.DropDownListItem();

            demoSelectItem1.key = "Operation 1";
            demoSelectItem1.value = "operation_1";
            var demoSelectItem2 = new model.DropDownListItem();

            demoSelectItem2.key = "Operation 2";
            demoSelectItem2.value = "operation_2";
            var demoSelectItem3 = new model.DropDownListItem();

            demoSelectItem3.key = "Operation 3";
            demoSelectItem3.value = "operation_3";
            var demoSelectItem4 = new model.DropDownListItem();

            demoSelectItem4.key = "Operation 4";
            demoSelectItem4.value = "operation_4";
            dropdownDemoField.fieldLabel = "Operation List";
            dropdownDemoField.listItems = new Array<model.DropDownListItem>();
            dropdownDemoField.listItems.push(demoSelectItem1);
            dropdownDemoField.listItems.push(demoSelectItem2);
            dropdownDemoField.listItems.push(demoSelectItem3);
            dropdownDemoField.listItems.push(demoSelectItem4);

            dropdownDemoField.value = "operation_4";
            dropdownDemoField.name = "demoDropDown";
            this._scope.dropdownDemoField = dropdownDemoField;

            var textBlockDemoField = new model.TextBlockField();
            textBlockDemoField.class = 'well well-lg';
            textBlockDemoField.value = 'Some description about action which is styled with class attribute using "well well-lg"';
            this._scope.textBlockDemoField = textBlockDemoField;

            var routingControlGroup = new model.RoutingControlGroup();
            routingControlGroup.fieldLabel = "routing";
            routingControlGroup.name = "routing";
            routingControlGroup.sourceField = "test_criteria_1";
            routingControlGroup.type = "routing";
            var routes = new Array<model.Route>();
            var routeTruthy = new model.Route();
            routeTruthy.measurementValue = "TRUE";
            routeTruthy.selection = "none";
            var routeActionList = new model.RouteActionList();
            var choices = new Array<model.Choice>();
            var choice = new model.Choice();
            choice.Id = "34";
            choice.Label = "Write Email";
            choices.push(choice);
            choice = new model.Choice();
            choice.Id = "50";
            choice.Label = "Extract Foo From Bar";
            choices.push(choice);
            routeActionList.choices = choices;
            routeTruthy.previousActionList = routeActionList;
            routeTruthy.previousActionSelectedId = "";
            routeTruthy.availableProcessNode = "";
            var routeFalsy = new model.Route();
            routeFalsy.measurementValue = "FALSE";
            routeFalsy.selection = "none";
            var routeActionList2 = new model.RouteActionList();
            var choices2 = new Array<model.Choice>();
            var choice2 = new model.Choice();
            choice2.Id = "341";
            choice2.Label = "Write Email (Falsy)";
            choices2.push(choice2);
            choice2 = new model.Choice();
            choice2.Id = "501";
            choice2.Label = "Extract Foo From Bar (Falsy)";
            choices2.push(choice2);
            routeActionList2.choices = choices2;
            routeFalsy.previousActionList = routeActionList2;
            routeFalsy.previousActionSelectedId = "";
            routeFalsy.availableProcessNode = "";
            routes.push(routeTruthy);
            routes.push(routeFalsy);
            routingControlGroup.routes = routes;
            this._scope.routingControlGroup = routingControlGroup;
            //END OF DEMO CODE
        }
    }

    app.controller('SandboxController', SandboxController);
} 