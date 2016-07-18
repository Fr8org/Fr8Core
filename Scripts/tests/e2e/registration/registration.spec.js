var RegistrationPage = require('../pages/registration.page.js');
var AccountHelper = require('../shared/accountHelper.js');
var UIHelpers = require('../shared/uiHelpers.js');
//var ConsoleLog = require('../shared/consoleLog.js');

describe('registration page tests', function () {
    //PROPERTIES
    var registrationPage = new RegistrationPage();
    var uiHelpers = new UIHelpers();
    var accountHelper = new AccountHelper();
    //var consoleLog = new ConsoleLog();

    beforeEach(function (done) {
        registrationPage.get();
    });

    //afterEach(function (done) {
    //    consoleLog();
    //});

    it('should registration process to complete', function () {
        return uiHelpers.waitForElementToBePresent(registrationPage.emailInput).then(function () {
            //expect(browser.getCurrentUrl()).toContain('/Register');
            return accountHelper.register().then(function () {
                expect(browser.getCurrentUrl()).toContain('/Welcome');
            });
        }, function () { });
    });

});

//describe('logout test', function () {

//    afterEach(function () {
//        accountHelper.logout();
//    });

//});
