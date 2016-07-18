var RegistrationPage = function () {
    var emailInput = element(by.id('Email'));
    var passwordInput = element(by.id('Password'));
    var confirmPasswordInput = element(by.id('ConfirmPassword'));
    var registrationButton = element(by.buttonText('Register'));
    var organizationCheckBox = element(by.id('HasOrganization'));

    this.get = function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl + '/DockyardAccount/Register');
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
        return registrationButton.click();
    };

};
module.exports = RegistrationPage;