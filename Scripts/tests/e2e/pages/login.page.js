var LoginPage = function () {
    var emailInput = element(by.id('Email'));
    var passwordInput = element(by.id('Password'));
    var loginButton = element(by.xpath('//*[@id="loginform"]/form/div[2]/div/div/button'));
    var myAccountButton = element(by.xpath('//*[@id="main-nav"]/ul/li[7]/p/a'));
    var selectDropDownByName = element(by.xpath('/html/body/div[1]/div/div[2]/div[1]/div/div[2]/ul/li/a/span'));
    var logoutButton = element(by.xpath('/html/body/div[1]/div/div[2]/div[1]/div/div[2]/ul/li/ul/li[3]/a'));
    
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

    this.myAccount = function () {
        return myAccountButton.click();
    };

    this.selectDropDownByName = function () {
        selectDropDownByName.click();
        return logoutButton.click();
    };

};

module.exports = LoginPage;