var UIHelpers = require('../shared/uiHelpers.js');

var LoginPage = function () {
    var emailInput = element(by.id('Email'));
    var passwordInput = element(by.id('Password'));
    var loginButton = element(by.xpath('//*[@id="loginform"]/form/div[2]/div/div/button'));
    var logoutButton = element(by.xpath('/html/body/div[1]/div/div[2]/div[1]/div/div[2]/ul/li/ul/li[3]/a'));
    var accountMenu = element(by.xpath('/html/body/div[1]/div/div[2]/div[1]/div/div[2]/ul/li'));

    var uiHelpers = new UIHelpers();
    
    this.get = function () {
        browser.ignoreSynchronization = true;
        return browser.get(browser.baseUrl + '/Account');
    };

    this.setEmail = function (email) {
        emailInput.sendKeys(email);
    };

    this.setPassword = function (password) {
        passwordInput.sendKeys(password);
    };

    this.login = function () {       
        return loginButton;
    };

    this.accountMenuButton = function () {
        return accountMenu;
    };
    
    this.logout = function () {
        return logoutButton.click();
    };

};

module.exports = LoginPage;