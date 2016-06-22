var ContactPage = require('../pages/contact.page.js');

describe('contact page test', function () {
    var contactPage;
    contactPage = new ContactPage();
    contactPage.get();

    /* For browser console error logs*/
    //afterEach(function (done) {
    //    browser.manage().logs().get('browser').then(function (browserLog) {
    //        var i = 0,
    //        severWarnings = false;
    //        for (i; i <= browserLog.length - 1; i++) {
    //            if (browserLog[i].level.name === 'SEVERE') {
    //                console.log('\n' + browserLog[i].level.name);
    //                console.log('(Possibly exception) \n' + browserLog[i].message)
    //                severWarnings = true;
    //            }
    //        }
    //        expect(severWarnings).toBe(true);
    //        done();
    //    });
    //});


    it('should open contact page', function () {
        browser.sleep(3000);
        contactPage.contact();
        browser.waitForAngular();
        expect(browser.getCurrentUrl()).toContain('/Support');
    });

    it('should contact message has been successfully sent', function () {
        contactPage.setName("Fr8 Company");
        contactPage.setEmail("emre@fr8.co");
        contactPage.setSubject("Test subject");
        contactPage.setMessage("Test message");
        browser.sleep(1000);
        contactPage.sendMessage();
        browser.sleep(3000);
        expect(element(by.css('.alert.alert-success')));
    });

});