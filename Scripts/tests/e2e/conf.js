exports.config = {
    directConnect: true,
    capabilities: {
        'browserName': 'chrome'
    },
    framework: 'jasmine',
    specs: ['login.js'],
    jasmineNodeOpts: {
        defaultTimeoutInterval: 30000
    },
    baseUrl: 'http://dev.fr8.co:30643'
}; 