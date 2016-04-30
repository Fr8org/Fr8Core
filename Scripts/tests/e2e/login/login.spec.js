var LoginPage = require('../pages/login.page.js');

describe('login page tests', function () {
    var loginPage;

    beforeEach(function () {
        loginPage = new LoginPage();
        loginPage.get();
    });

    it('should login', function () {
        loginPage.setEmail("integration_test_runner@fr8.company");
        loginPage.setPassword("fr8#s@lt!");

        loginPage.login();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('Welcome');
    });

});
 
   