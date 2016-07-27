/// <reference path="../../app/_all.ts" />

jasmine.DEFAULT_TIMEOUT_INTERVAL = 10000;

describe("Endpoints hit tests", () => {
    let $http: ng.IHttpService = angular.injector(["ng"]).get("$http");
    let config: ng.IRequestConfig = {
        url: "/api/v1/plans",
        method: ""
    };

    config.headers = { "Content-type": "application/json" };
    
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


    it('should CREATE SOLUTION by hit POST /api/v1/plans?solutionName= ', (done) => {
        config.method = "POST";
        config.params = { solutionName: "none" };

        checkEndpoint(config,done);
        
    });

    it('should COPY plan by hit POST /api/v1/plans?source_id= ', (done) => {
        config.method = "POST";
        config.params = { source_id: "" };

        checkEndpoint(config, done);
    });

    it('should GET plan BY ID hit (old Get()) GET /api/v1/plans?id= ', (done) => {
        config.method = "GET";
        config.params = { id: "41788318-03B6-46F4-B2B6-5A8225A36314" };

        checkEndpoint(config, done);
    });

    it('should GET FULL PLAN (old GetFullPlan) by hit GET /api/v1/plans?id=&include_children=true', (done) => {
        config.method = "GET";
        config.params = { id: "41788318-03B6-46F4-B2B6-5A8225A36314", include_children:true};

        checkEndpoint(config, done);
    });

    it('should GET PLAN BY aCTIVITY iD (old GetByActivity) hit GET /api/v1/plans?activity_id=', (done) => {    
        config.method = "GET";
        config.params = { activity_id: "2E435BA0-2A6F-4BBC-9924-5322464414BD"};

        checkEndpoint(config, done);
    });

    it('should GET PLAN BY NAME (old GetByName) hit GET /api/v1/plans?name=', (done) => {
        config.method = "GET";
        config.params = { name: "SOME DATA PLAN", visibility:'1'};

        checkEndpoint(config, done);
    });

  

});
