var RegistrationPage = require('../pages/registration.page.js');
var AccountHelper = require('../shared/accountHelper.js');
var UIHelpers = require('../shared/uiHelpers.js');
//var ConsoleLog = require('../shared/consoleLog.js');

describe('registration page tests', function () {
    //PROPERTIES
    var uiHelpers = new UIHelpers();
    var accountHelper = new AccountHelper();
    var registrationPage = new RegistrationPage();
    //var consoleLog = new ConsoleLog();

    it('should register', function () {
        return accountHelper.register().then(function () {
            registrationPage.setEmail(browser.params.username);
            registrationPage.setPassword(browser.params.password);
            registrationPage.setConfirmPassword(browser.params.password);
            registrationPage.setOrganization();
            return registrationPage.register().click().then(function () {
                expect(browser.getCurrentUrl()).toContain('/Welcome');
            });
        });
    });

    describe('should logout', function () {

        var registrationPage = new RegistrationPage();

        it('should logout', function () {
            return accountHelper.logout().then(function () {
                expect(browser.getCurrentUrl()).toContain('/Account');
            });
        });
    });



});
