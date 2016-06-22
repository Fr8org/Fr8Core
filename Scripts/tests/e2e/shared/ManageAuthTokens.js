var ManageAuthTokens = function () {
 
    //this.get = function () {
    //    browser.ignoreSynchronization = true;
    //    browser.get(browser.baseUrl + '/dashboard/manageAuthTokens');
    //};
    this.revokeAuthTokens = function () {
        for(i=0; i<element(by.repeater('terminal in terminals')).length; i++){
            var terminal = element(by.repeater('terminal in terminals').row(i));

            for (j = 0; i < terminal(by.repeater('authToken in terminal.authTokens')).lenght; j++) {
                var account = terminal(by.repeater('authToken in terminal.authTokens').row(j));
                var revokeButton = account(by.css('btn btn-danger btn-sm')).click();
            }
            return;
        }
    }
};
module.exports = ManageAuthTokens;