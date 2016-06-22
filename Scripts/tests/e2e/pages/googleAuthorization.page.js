var GoogleAuthorizationPage = function () {
    var emailInput = element(by.id('Email'));
    var passwordInput = element(by.id('Password'));
    var loginButton = element(by.xpath('//*[@id="loginform"]/form/div[2]/div/div/button'));
    var myAccountButton = element(by.xpath('//*[@id="main-nav"]/ul/li[7]/p/a'));
    var addPlansButton = element(by.xpath('//*[@id="Myfr8lines"]/h3/a'));
    var planInput = element(by.xpath('//*[@id="tab_1_1"]/form/div[3]/div/input'));
    var saveChangesButton = element(by.xpath('//*[@id="tab_1_1"]/form/div[5]/button'));
    var getDataGoogleButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div/div/div[2]/div/div/div/div/div[2]/action-picker/div/div/div[1]/div[2]/i'));
    var googleActivityButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div/div/div[2]/div/div/div/div/div[2]/action-picker/div/div/div[2]/div[1]/div[2]/div[7]/div/div[2]/h4'));
    var getGoogleSheetDataButton = element(by.xpath('/html/body/div[2]/div[2]/div/div/div/div/div[1]/div[1]/div/div/div[2]/div/div/div/div/div[2]/action-picker/div/div/div[2]/div[2]/div[2]/div/div/div[2]/h4'));
    var addAccountButton = element(by.xpath('/html/body/div[4]/div/div/div[2]/div[2]/div[2]/a'));
    /* Popop input elements */
    var emailPopupInput = element(by.id('Email'));
    var continuePopupButton = element(by.id('next'));
    var passwordPopupInput = element(by.id('Passwd'));
    var signInPopupButton = element(by.id('signIn'));
    var allowButton = element(by.id('submit_approve_access'));

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

    this.getDataGoogle = function () {
        return getDataGoogleButton.click();
    };

    this.googleActivity = function () {
        return googleActivityButton.click();
    };

    this.getGoogleSheetData = function () {
        return getGoogleSheetDataButton.click();
    };

    this.addAccount = function () {
        return addAccountButton.click();
    };

    this.setEmailPopupInput = function (emailPopup) {
        emailPopupInput.sendKeys(emailPopup);
    };

    this.continuePopup = function () {
        return continuePopupButton.click();
    };

    this.setPasswordPopupInput = function (passwordPopup) {
        passwordPopupInput.sendKeys(passwordPopup);
    };

    this.signInPopup = function () {
        return signInPopupButton.click();
    };

    this.allow = function () {
        return allowButton.click();
    };

};
module.exports = GoogleAuthorizationPage;