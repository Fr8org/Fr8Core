var RegistrationPage = function(){
    var emailInput = element(by.id('Email'));
    var passwordInput = element(by.id('Password'));
    var confirmPasswordInput = element(by.id('ConfirmPassword'));
    var registrationButton = element(by.buttonText('Register'));
    //var registrationButton = element(by.xpath('//*[@id="loginform"]/html/body/div/div/div/div/div[2]/div[2]/a'));

    this.get = function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl + '/DockyardAccount/');
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

    this.register = function () {
        return registrationButton.click();
    };
}

module.exports = RegistrationPage;