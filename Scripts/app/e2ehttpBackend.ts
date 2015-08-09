app.run([
    '$httpBackend', httpBackend => {
        httpBackend.whenGET(/^\/AngularTemplate\//).passThrough();
        httpBackend.whenGET(/^\/Views\//).passThrough();
        httpBackend.whenGET(/^\/api\//).passThrough();
        httpBackend.whenPOST(/^\/api\//).passThrough();
        httpBackend.whenPUT(/^\/api\//).passThrough();
        httpBackend.whenDELETE(/^\/api\//).passThrough();
    }
]);