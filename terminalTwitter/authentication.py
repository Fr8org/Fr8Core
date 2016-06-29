import json
import tweepy
import fr8.data
import terminal


class Handler(object):
    def __init__(self):
        self.auth_data = {}

    def get_request_url(self):
        auth = tweepy.OAuthHandler(
            terminal.twitter_consumer_key,
            terminal.twitter_consumer_secret,
            terminal.twitter_callback_url
        )

        redirect_url = auth.get_authorization_url()
        oauth_token = auth.request_token['oauth_token']
        self.auth_data[oauth_token] = auth.request_token

        return fr8.data.ExternalAuthUrlDTO(state_token=oauth_token, url=redirect_url)

    def extract_token(self, external_token_dto):
        oauth_verifier = external_token_dto.parameters['oauth_verifier']
        oauth_token = external_token_dto.parameters['oauth_token']
        auth = tweepy.OAuthHandler(terminal.twitter_consumer_key, terminal.twitter_consumer_secret)
        auth.request_token = self.auth_data[oauth_token]
        del self.auth_data[oauth_token]

        twitter_tokens = auth.get_access_token(oauth_verifier)
        token_json = json.dumps(twitter_tokens)

        auth = tweepy.OAuthHandler(terminal.twitter_consumer_key, terminal.twitter_consumer_secret)
        auth.set_access_token(twitter_tokens[0], twitter_tokens[1])

        api = tweepy.API(auth)
        user = api.me()

        return fr8.data.AuthorizationTokenDTO(
            token=token_json,
            external_state_token=oauth_token,
            external_account_id=user.screen_name
        )
