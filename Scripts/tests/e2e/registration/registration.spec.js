var RegistrationPage = require('../pages/registration.page.js');

describe('registration page tests', function () {
    var registrationPage;

    registrationPage = new RegistrationPage();
    registrationPage.get();

    /* For browser console error logs*/
    afterEach(function (done) {
        browser.manage().logs().get('browser').then(function (browserLog) {
            var i = 0,
            severWarnings = false;
            for (i; i <= browserLog.length - 1; i++) {
                if (browserLog[i].level.name === 'SEVERE') {
                    console.log('\n' + browserLog[i].level.name);
                    console.log('(Possibly exception) \n' + browserLog[i].message)
                    severWarnings = true;
                }
            }
            expect(severWarnings).toBe(true);
            done();
        });
    });


    it('should set url as /register', function () {
        element(by.xpath('/html/body/div/div/div/div/div[2]/div[2]/a')).click();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Register');
    });

    it('should registration complete', function () {
        registrationPage.setEmail("testuser_00014@fr8.co");
        registrationPage.setPassword("123qwe");
        registrationPage.setConfirmPassword("123qwe");
        registrationPage.register();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('Welcome');
    });

    it('should open my plans page', function () {
        browser.sleep(2000);
        registrationPage.myAccount();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/myaccount');
    });

    it('should be logout', function () {
        browser.sleep(4000);
        registrationPage.selectDropDownByName();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/DockyardAccount');
    });

});