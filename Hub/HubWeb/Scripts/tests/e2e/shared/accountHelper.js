var LoginPage = require('../pages/login.page.js');
var ContactPage = require('../pages/contact.page.js');
var PlansPage = require('../pages/plans.page.js');
var RegistrationPage = require('../pages/registration.page.js');
var UIHelpers = require('../shared/uiHelpers.js');

var AccountHelper = function () {

    var loginPage = new LoginPage();
    var plansPage = new PlansPage();
    var registrationPage = new RegistrationPage();
    var uiHelpers = new UIHelpers();
    var contactPage = new ContactPage();

    this.login = function () {
        return loginPage.get();
    };

    this.logout = function () {
        return browser.get(browser.baseUrl + '/Account/Logoff');
    };

    this.register = function () {
        return registrationPage.get();
    };

    this.google = function () {
        return plansPage.get();
    };

    this.contact = function () {
        return contactPage.get();
    };

};
module.exports = AccountHelper;