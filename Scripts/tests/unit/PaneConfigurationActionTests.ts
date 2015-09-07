/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.controller {
    //Setup aliases
    import pca = dockyard.directives.paneConfigureAction;
    import fx = dockyard.tests.utils.fixtures;

    describe("PaneConfiguration onRender getCongiruationStore", () => {
        beforeEach(module("app"));

        app.run(['$httpBackend',
            function ($httpBackend) {
                $httpBackend.whenGET().passThrough();
            }
        ]);

        var _$controllerService: ng.IControllerService,
            _$scope: pca.IPaneConfigureActionScope,
            _controller: any,
            _$state: ng.ui.IState,
            _actionServiceMock: utils.ActionServiceMock,
            _$q: ng.IQService,
            _$http: ng.IHttpService,
            _urlPrefix: string;

        //TODO
        //beforeEach(() => {
        //    inject(($controller, $rootScope, $q, $http) => {
        //        _actionServiceMock = new utils.ActionServiceMock($q);
        //        _$q = $q;
        //        _$scope = fx.ActionDesignDTO.paneConfiguration;
        //        _$state = {
        //            data: {
        //                pageSubTitle: ""
        //            },
        //            params: {
        //                id: 1
        //            }
        //        };
        //        _$http = $http;
        //        _urlPrefix = '/api';

        //    });
        //    spyOn(_$scope, "$broadcast");
        //});


        //it("Get configuration settings from new action template", () => {
        //    var event = fx.ActionDesignDTO.PaneConfigureActionOnRender_Event;
        //    var event_args = fx.ActionDesignDTO.PaneConfigureActionOnRender_EventArgs;

        //    console.log(event);
        //    console.log(event_args);
        //    console.log("before call");
        //    _$scope.$emit(pca.MessageType[pca.MessageType.PaneConfigureAction_Render], event, event_args);
        //    //expect(_$scope.$broadcast).toHaveBeenCalledWith('PaneSelectTemplate_Render');
        //    console.log("after call");
        //    console.log(event);
        //    console.log(event_args);
        //});

    });
} 