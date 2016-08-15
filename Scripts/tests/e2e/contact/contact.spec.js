var ContactPage = require('../pages/contact.page.js');
var AccountHelper = require('../shared/accountHelper.js');
var UIHelpers = require('../shared/uiHelpers.js');

describe('contact page test', function () {
    //PROPERTIES
    var uiHelpers = new UIHelpers();
    var accountHelper = new AccountHelper();
    var contactPage = new ContactPage();

    it('should open contact Page', function () {
       return accountHelper.contact().then(function () {
           contactPage.setName("Fr8 Company");
           contactPage.setEmail("emre@fr8.co");
           contactPage.setSubject("Test subject");
           contactPage.setMessage("Test message");
           return browser.wait(uiHelpers.waitForElementToBePresent(contactPage.sendMessage()).then(function () {
               browser.sleep(1000);
               return contactPage.sendMessage().click().then(function () {
                   expect(element(by.css('.alert.alert-success')));
               });
           }));
        });
    });

});