var LoginPage = require('../pages/login.page.js');
var AccountHelper = require('../shared/accountHelper.js');
var UIHelpers = require('../shared/uiHelpers.js');

describe('login page tests', function () {

    //PROPERTIES
    var uiHelpers = new UIHelpers();
    var accountHelper = new AccountHelper();
    var loginPage = new LoginPage();

    it('should login', function () {
        return accountHelper.login().then(function () {
            loginPage.setEmail(browser.params.username);
            loginPage.setPassword(browser.params.password);
            return loginPage.login().click().then(function () {
                expect(browser.getCurrentUrl()).toContain('/Welcome');
            });
        });
    });

    describe('should logout', function () {

        var loginPage = new LoginPage();

        it('should logout', function () {
            return accountHelper.logout().then(function () {
                expect(browser.getCurrentUrl()).toContain('/Account');
            });
        });
    });


});