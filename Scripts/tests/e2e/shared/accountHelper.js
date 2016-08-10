var LoginPage = require('../pages/login.page.js');
//var ContactPage = require('../pages/contact.page.js');
var PlansPage = require('../pages/plans.page.js');
var RegistrationPage = require('../pages/registration.page.js');
var UIHelpers = require('../shared/uiHelpers.js');

var AccountHelper = function () {

    var loginPage = new LoginPage();
    var plansPage = new PlansPage();
    var registrationPage = new RegistrationPage();
    var uiHelpers = new UIHelpers();
    //var contactPage = new ContactPage();

    this.login = function () {
        return loginPage.get();
    };

    this.logout = function () {
        return browser.get(browser.baseUrl + '/DockyardAccount/Logoff');
    };

    this.register = function () {
        return registrationPage.get();
    };

    this.google = function () {
        return plansPage.get();
    };

    //this.sendContact = function () {
    //    contactPage.setName("Fr8 Company");
    //    contactPage.setEmail("emre@fr8.co");
    //    contactPage.setSubject("Test subject");
    //    contactPage.setMessage("Test message");
    //    return contactPage.sendContact();
    //};

};
module.exports = AccountHelper;