app.run([
    '$httpBackend', httpBackend => {
        // Fake AppInsights request
        httpBackend.when('POST', 'https://dc.services.visualstudio.com/v2/track').respond({});
    }   
]);