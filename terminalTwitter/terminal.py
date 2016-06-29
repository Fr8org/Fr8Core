import fr8.data

twitter_consumer_key = 'j0CGin9tvfWEe5UrxsCcjC6fT'
twitter_consumer_secret = 'yjSWPAqLOC5ABG8Yz1uAvsGEAWNph5IRThfnwjciAvobXh8Gc2'
twitter_callback_url = 'http://localhost:30643/AuthenticationCallback/ProcessSuccessfulOAuthResponse?terminalName=terminalTwitter&terminalVersion=1'

web_service = fr8.data.WebServiceDTO(name='Twitter')

terminal_id = 'B9CC8E6D-69B0-4E41-BCBC-251F25E10E75'
terminal_endpoint = 'http://127.0.0.1:8080'
terminal = fr8.data.TerminalDTO(
    id = terminal_id,
    name = 'terminalTwitter',
    version = '1',
    endpoint = terminal_endpoint,
    label = 'Twitter Terminal',
    authentication_type = fr8.data.AuthenticationType.EXTERNAL
)

post_to_twitter_id = 'BA4BD581-F2D7-4A4A-B82A-0CF9C74BDD15'
post_to_twitter = fr8.data.ActivityTemplateDTO(
    id = post_to_twitter_id,
    name = 'Post_To_Twitter',
    version = '1',
    terminal = terminal,
    web_service = web_service,
    activity_category = fr8.data.ActivityCategory.FORWARDERS,
    needs_authentication = True,
    label = 'Post To Twitter'
)
