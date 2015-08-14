// to enable disable, service hitting real backend comment this line and 
// uncomment second line
//app.constant('urlPrefix', '/apimocks');
app.constant('urlPrefix', '/api');
app.run([
    '$httpBackend', httpBackend => {
        var validation = (url) => {
            return url.indexOf("/apimocks") === -1;
        }
        httpBackend.whenGET(validation).passThrough();
        httpBackend.whenGET(validation).passThrough();
        httpBackend.whenGET(validation).passThrough();
        httpBackend.whenPOST(validation).passThrough();
        httpBackend.whenPUT(validation).passThrough();
        httpBackend.whenDELETE(validation).passThrough();
    }
]);