/// <reference path="../../app/_all.ts" />
/// <reference path="../../typings/angularjs/angular-mocks.d.ts" />

module dockyard.tests.controller {
    //Setup aliases
    import pwd = dockyard.directives.paneWorkflowDesigner;
    import pdc = dockyard.directives.paneDefineCriteria;
    import psa = dockyard.directives.paneSelectAction;
    import pca = dockyard.directives.paneConfigureAction;
    import pst = dockyard.directives.paneSelectTemplate;
    import pcm = dockyard.directives.paneConfigureMapping;

    describe("PaneConfiguration onRender getCongiruationStore", () => {
        beforeEach(module("app"));

        app.run(['$httpBackend',
            function ($httpBackend) {
                $httpBackend.whenGET().passThrough();
            }
        ]);

        var _$controllerService: ng.IControllerService,
            _$scope: dockyard.controllers.IPaneConfigureActionScope,
            _controller: any,
            _$state: ng.ui.IState,
            _actionServiceMock: utils.ActionServiceMock,
            _$q: ng.IQService,
            _$http: ng.IHttpService,
            _urlPrefix: string;

        beforeEach(() => {
            inject(($controller, $rootScope, $q, $http) => {
                _actionServiceMock = new utils.ActionServiceMock($q);
                _$q = $q;
                _$scope = tests.utils.Factory.GetProcessBuilderScope($rootScope);
                _$state = {
                    data: {
                        pageSubTitle: ""
                    },
                    params: {
                        id: 1
                    }
                };
                _$http = $http;
                _urlPrefix = '/api';

            });
            spyOn(_$scope, "$broadcast");
        });


        it("Get configuration settings from new action template", () => {
            var Event2Args = new pca.RenderEventArgs(new model.ActionDesignDTO(1, 2, false, 3));

            _$scope.$emit(MessageType[MessageType.PaneConfigureAction_Render], Event2Args);
            expect(_$scope.$broadcast).toHaveBeenCalledWith('PaneSelectTemplate_Render');
        });

    });
} 