var ContactPage = function () {
    var nameInput = element(by.id('name'));
    var emailInput = element(by.id('email'));
    var subjectInput = element(by.id('subject'));
    var messageInput = element(by.id('message'));
    var contactButton = element(by.xpath('//*[@id="main-nav"]/ul/li[5]/a'));
    var sendMessageButton = element(by.id('submit-button'));

    this.get = function () {
        browser.ignoreSynchronization = true;
        browser.get(browser.baseUrl);
    };

    this.setName = function (name) {
        nameInput.sendKeys(name);
    };

    this.setEmail = function (email) {
        emailInput.sendKeys(email);
    };

    this.setSubject = function (subject) {
        subjectInput.sendKeys(subject);
    };

    this.setMessage = function (message) {
        messageInput.sendKeys(message);
    };

    this.contact = function () {
        return contactButton.click();
    };

    this.sendMessage = function () {
        return sendMessageButton.click();
    };

};

module.exports = ContactPage;