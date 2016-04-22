'use strict';

var LoginPage = function () {
    var emailInput = element(by.id('Email'));
    var passwordInput = element(by.id('Password'));
    var loginButton = element(by.xpath('//*[@id="loginform"]/form/div[2]/div/div/button'));

    this.get = function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl + '/DockyardAccount');
    };

    this.setEmail = function (email) {
        emailInput.sendKeys(email);
    };

    this.setPassword = function (password) {
        passwordInput.sendKeys(password);
    };

    this.login = function () {
        return loginButton.click();
    };
};

module.exports = LoginPage;