var RegistrationPage = function(){
    var emailInput = element(by.id('Email'));
    var passwordInput = element(by.id('Password'));
    var confirmPasswordInput = element(by.id('ConfirmPassword'));
    var registrationButton = element(by.buttonText('Register'));
    var myAccountButton = element(by.xpath('//*[@id="main-nav"]/ul/li[7]/p/a'));
    var selectDropDownByName = element(by.xpath('/html/body/div[1]/div/div[2]/div[1]/div/div[2]/ul/li/a/span'));
    var logoutButton = element(by.xpath('/html/body/div[1]/div/div[2]/div[1]/div/div[2]/ul/li/ul/li[3]/a'));
    //var registrationButton = element(by.xpath('//*[@id="loginform"]/html/body/div/div/div/div/div[2]/div[2]/a'));

    this.get = function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl + '/DockyardAccount/');
    };

    this.myAccount = function () {
        return myAccountButton.click();
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

    this.selectDropDownByName = function () {
        selectDropDownByName.click();
        return logoutButton.click();
    };
}

module.exports = RegistrationPage;