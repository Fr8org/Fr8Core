var SalesForceAuthorizationPage = function () {
    var emailInput = element(by.id('Email'));
    var passwordInput = element(by.id('Password'));
    var loginButton = element(by.xpath('//*[@id="loginform"]/form/div[2]/div/div/button'));
    var myAccountButton = element(by.xpath('//*[@id="main-nav"]/ul/li[7]/p/a'));
    var addPlansButton = element(by.xpath('//*[@id="Myfr8lines"]/h3/a'));
    var planInput = element(by.xpath('//*[@id="tab_1_1"]/form/div[3]/div/input'));
    var saveChangesButton = element(by.xpath('//*[@id="tab_1_1"]/form/div[5]/button'));
    var monitorIconButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div/div/div[2]/div/div/div/div/div[2]/action-picker/div/div/div[1]/div[1]/i'));
    var salesForceActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div/div/div[2]/div/div/div/div/div[2]/action-picker/div/div/div[2]/div[1]/div[2]/div[4]/div/div[2]/h4'));
    var monitorSalesForceActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div/div/div[2]/div/div/div/div/div[2]/action-picker/div/div/div[2]/div[2]/div[2]/div/div/div[2]/h4'));
    var addAccountButton = element(by.xpath('/html/body/div[4]/div/div/div[2]/div[2]/div[2]/a')); 
    /* Popop input elements */
    var usernameInput = element(by.id('username'));
    var passwordPopupInput = element(by.xpath('//*[@id="password"]'));
    var loginPopupButton = element(by.id('Login'));
    var okButton = element(by.className('btn btn-primary pull-right'));

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

    this.addPlans = function () {
        return addPlansButton.click();
    };

    this.setPlanName = function (name) {
        planInput.sendKeys(name);
    };

    this.saveChanges = function () {
        return saveChangesButton.click();
    };

    this.monitorIcon = function () {
        return monitorIconButton.click();
    };
    
    this.salesForceActivity = function () {
        return salesForceActivityButton.click();
    };

    this.monitorSalesForce = function () {
        return monitorSalesForceActivityButton.click();
    };

    this.addAccount = function () {
        return addAccountButton.click();
    };

    this.setUsername = function (username) {
        return usernameInput.sendKeys(username);
    };

    this.setPasswordPopup = function (passwordPopup) {
        return passwordPopupInput.sendKeys(passwordPopup);
    };

    this.loginButtonPopup = function () {
        return loginPopupButton.click();
    };
    
    this.ok = function () {
        return okButton.click();
    };
};
module.exports = SalesForceAuthorizationPage;