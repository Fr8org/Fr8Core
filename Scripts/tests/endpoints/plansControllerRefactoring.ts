/// <reference path="../../app/_all.ts" />

jasmine.DEFAULT_TIMEOUT_INTERVAL = 10000;

describe("Endpoints hit tests", () => {
    let $http: ng.IHttpService = angular.injector(["ng"]).get("$http");
    let config: ng.IRequestConfig = {
        url: "/api/v1/plans",
        method: ""
    };

    config.headers = { "Content-type": "application/json" };

    // do you understand what written down here ↓ :)
    let checkEndpoint: (config: ng.IRequestConfig, done: any) => void = (config, done) => {
        $http(config)
            .then((response) => {
                expect(response.data).not.toBeNull();
                done();
            }, (response) => {
                expect(response.status).not.toBe(404);
                //fail(response.status);
                done();
            });
    };


    it('should create solution by hit POST /api/v1/plans?solution_name= ', (done) => {
        config.method = "POST";
        config.params = { solution_name: "none" };

        checkEndpoint(config,done);
        
    });

    it('should copy plan by hit POST /api/v1/plans?source_id= ', (done) => {
        config.method = "POST";
        config.params = { source_id: "" };

        checkEndpoint(config, done);
    });

    it('should get plan by hit (old Get()) GET /api/v1/plans?id= ', (done) => {
        config.method = "GET";
        config.params = { id: "41788318-03B6-46F4-B2B6-5A8225A36314" };

        checkEndpoint(config, done);
    });

    it('should get full plan (old GetFullPlan) by hit GET /api/v1/plans?id=&include_children=true', (done) => {
        config.method = "GET";
        config.params = { id: "41788318-03B6-46F4-B2B6-5A8225A36314", include_children:true};

        checkEndpoint(config, done);
    });

    it('should get plan by Activity Id (old GetByActivity) hit GET /api/v1/plans?activity_id=', (done) => {    
        config.method = "GET";
        config.params = { activity_id: "2E435BA0-2A6F-4BBC-9924-5322464414BD"};

        checkEndpoint(config, done);
    });

    it('should get plan by name (old GetByName) hit GET /api/v1/plans?name=', (done) => {
        config.method = "GET";
        config.params = { name: "SOME DATA PLAN" };

        checkEndpoint(config, done);
    });

  

});
