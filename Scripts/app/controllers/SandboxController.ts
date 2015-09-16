/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/

module dockyard.controllers {
    'use strict';

    export interface ISandboxScope extends ng.IScope {
        
        //this is for demo only, should be deleted on production
        radioDemoField: model.RadioButtonGroupField;
        dropdownDemoField: model.DropDownListBoxField;
        routingControlGroup: model.RoutingControlGroup;
    }

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
            'urlPrefix',
            'ProcessTemplateService',
            '$timeout',
            'CriteriaServiceWrapper',
            'SandboxService',
            'ActionListService'
        ];

        private _scope: ISandboxScope;

        constructor() {
            //THIS IS FOR DEMO ONLY
            var radioDemoField = new model.RadioButtonGroupField();
            radioDemoField.fieldLabel = 'Demo Label';
            radioDemoField.groupName = 'Demo Group Name';
            radioDemoField.type = 'radioButtonGroup';
            var demoRadio1 = new model.RadioField();
            demoRadio1.value = "Selection 1";
            demoRadio1.selected = false;
            var demoRadio2 = new model.RadioField();
            demoRadio2.value = "Selection 2";
            demoRadio2.selected = false;
            var demoRadio3 = new model.RadioField();
            demoRadio3.value = "Selection 3";
            demoRadio3.selected = true;
            var radios = new Array<model.RadioField>();
            radios.push(demoRadio1);
            radios.push(demoRadio2);
            radios.push(demoRadio3);
            radioDemoField.radios = radios;
            this._scope.radioDemoField = radioDemoField;

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

    app.run([
        "$httpBackend", "urlPrefix", ($httpBackend, urlPrefix) => {
            var actions: interfaces.IActionDesignDTO =
                {
                    name: "test action type",
                    configurationControls: new model.ControlsList(),
                    crateStorage: new model.CrateStorage(),
                    processNodeTemplateId: 1,
                    actionTemplateId: 1,
                    id: 1,
                    isTempId: false,
                    fieldMappingSettings: new model.FieldMappingSettings(),
                    userLabel: "test",
                    tempId: 0,
                    actionListId: 0,
                    activityTemplate: new model.ActivityTemplate(1, "Write to SQL", "1","")
                };

            $httpBackend
                .whenGET(urlPrefix + "/Action/1")
                .respond(actions);

            $httpBackend
                .whenPOST(urlPrefix + "/Action/1")
                .respond(function (method, url, data) {
                    return data;
                })
        }
    ]);

    app.controller('SandboxController', SandboxController);
} 