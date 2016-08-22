var ContactPage = function () {
    var nameInput = element(by.id('name'));
    var emailInput = element(by.id('email'));
    var subjectInput = element(by.id('subject'));
    var messageInput = element(by.id('message'));
    var contactButton = element(by.xpath('//*[@id="main-nav"]/ul/li[5]/a'));
    var sendMessageButton = element(by.id('submit-button'));
    //var sendMessageButton = element(by.buttonText('Send Message'));

    this.get = function () {
        browser.ignoreSynchronization = true;
        return browser.get(browser.baseUrl + '/Support');
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

    this.sendMessage = function () {
        return sendMessageButton;
    };


};

module.exports = ContactPage;