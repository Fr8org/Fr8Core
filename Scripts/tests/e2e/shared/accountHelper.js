var LoginPage = require('../pages/login.page.js');
//var ContactPage = require('../pages/contact.page.js');
//var GoogleAuthorizationPage = require('../pages/googleAuthorization.page.js');
var RegistrationPage = require('../pages/registration.page.js');
var MyAccountPage = require('../pages/myAccount.page.js');

var AccountHelper = function () {

    var loginPage = new LoginPage();
    var logoutPage = new LoginPage();
    var registrationPage = new RegistrationPage();
    var myAccountPage = new MyAccountPage();
    //var contactPage = new ContactPage();

    this.login = function () {
        loginPage.setEmail("integration_test_runner@fr8.company");
        loginPage.setPassword("fr8#s@lt!");
        return loginPage.login();
    };

    this.register = function () {
        registrationPage.setEmail("testuser_00040@fr8.co");
        registrationPage.setPassword("123qwe");
        registrationPage.setConfirmPassword("123qwe");
        registrationPage.setOrganization();
        return registrationPage.register();
    };

    this.logout = function () {
        MyAccountPage();
        //return myAccountPage.selectDropDownByName();
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