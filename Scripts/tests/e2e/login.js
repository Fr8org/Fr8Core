describe('login', function () {
    it('Should login', function () {
        var EC = protractor.ExpectedConditions;
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl);

        var button = element(by.xpath('//*[@id="main-nav"]/ul/li[6]/button'));
        var isClickable = EC.elementToBeClickable(button);
        browser.wait(isClickable, 5000);
        element(by.xpath('//*[@id="main-nav"]/ul/li[6]/button')).click();
        element(by.id('Email')).sendKeys("integration_test_runner@fr8.company");
        element(by.id('Password')).sendKeys("fr8#s@lt!");
        element(by.xpath('//*[@id="loginform"]/form/div[2]/div/div/button')).click();
        expect(element(by.xpath('//*[@id="main-nav"]/ul/li[6]/button/span[1]')).getText()).toBe('MY ACCOUNT');
    });

});
 
   