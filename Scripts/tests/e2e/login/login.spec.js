var LoginPage = require('../pages/login.page.js');

describe('login page tests', function () {
    var loginPage;

    //beforeEach(function () {
        loginPage = new LoginPage();
        loginPage.get();
    //});

    it('should login', function () {
        loginPage.setEmail("integration_test_runner@fr8.company");
        loginPage.setPassword("fr8#s@lt!");

        loginPage.login();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('Welcome');
    });

    it('should open my plans page', function () {
        browser.sleep(2000);
        loginPage.myAccount();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/myaccount');
    });

    it('should be logout', function () {
        browser.sleep(4000);
        loginPage.selectDropDownByName();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/DockyardAccount');
    });

});
 
   