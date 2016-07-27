var LoginPage = require('../pages/login.page.js');
//var ContactPage = require('../pages/contact.page.js');
//var GoogleAuthorizationPage = require('../pages/googleAuthorization.page.js');
//var RegistrationPage = require('../pages/registration.page.js');
var UIHelpers = require('../shared/uiHelpers.js');

var AccountHelper = function () {

    var loginPage = new LoginPage();
    //var registrationPage = new RegistrationPage();
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

    //this.register = function () {
    //    return registrationPage.get().then(function () {
    //        registrationPage.setEmail("testuser_00074@fr8.co");
    //        registrationPage.setPassword("123qwe");
    //        registrationPage.setConfirmPassword("123qwe");
    //        registrationPage.setOrganization();
    //        return registrationPage.register();
    //    });
    //};

    //this.sendContact = function () {
    //    contactPage.setName("Fr8 Company");
    //    contactPage.setEmail("emre@fr8.co");
    //    contactPage.setSubject("Test subject");
    //    contactPage.setMessage("Test message");
    //    return contactPage.sendContact();
    //};

};
module.exports = AccountHelper;