var RegistrationPage = require('../pages/registration.page.js');

describe('registration page tests', function () {
    var registrationPage;

    registrationPage = new RegistrationPage();
    registrationPage.get();

    it('should set url as /register', function () {
        element(by.xpath('/html/body/div/div/div/div/div[2]/div[2]/a')).click();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Register');
    });

    it('should registration complete', function () {
        registrationPage.setEmail("testuser_00010@fr8.co");
        registrationPage.setPassword("123qwe");
        registrationPage.setConfirmPassword("123qwe");

        registrationPage.register();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('Welcome');
    });

});