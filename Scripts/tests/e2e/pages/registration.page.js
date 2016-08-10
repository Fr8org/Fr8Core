var UIHelpers = require('../shared/uiHelpers.js');

var RegistrationPage = function () {
    var emailInput = element(by.id('Email'));
    var passwordInput = element(by.id('Password'));
    var confirmPasswordInput = element(by.id('ConfirmPassword'));
    var registrationButton = element(by.buttonText('Register'));
    var organizationCheckBox = element(by.id('HasOrganization'));
    var accountMenu = element(by.xpath('/html/body/div[1]/div/div[2]/div[1]/div/div[2]/ul/li'));
    var logoutButton = element(by.xpath('/html/body/div[1]/div/div[2]/div[1]/div/div[2]/ul/li/ul/li[3]/a'));

    var uiHelpers = new UIHelpers();

    this.get = function () {
        browser.ignoreSynchronization = true;
        return browser.get(browser.baseUrl + '/Account/Register');
    };

    this.setEmail = function (email) {
        emailInput.sendKeys(email);
    };

    this.setPassword = function (password) {
        passwordInput.sendKeys(password);
    };

    this.setConfirmPassword = function (confirmPassword) {
        confirmPasswordInput.sendKeys(confirmPassword);
    };

    this.setOrganization = function () {
        return organizationCheckBox.click();
    };

    this.register = function () {
        return registrationButton;
    };

    this.accountMenuButton = function () {
        return accountMenu;
    };

    this.logout = function () {
        return logoutButton.click();
    };

};
module.exports = RegistrationPage;