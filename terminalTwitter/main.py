import json
import uuid

from flask import Flask
from flask import jsonify
from flask import request

import tweepy

import fr8.data
import fr8.routines
import fr8.terminal

import authentication
import post_to_twitter
import terminal

# Flask app instance.
app = Flask(__name__)

# Terminal request handler.
handler = fr8.terminal.TerminalHandler(
    terminal.terminal,
    authentication_handler=authentication.Handler(),
    activities=[
        (terminal.post_to_twitter, post_to_twitter.Handler)
    ]
)


# Discover end-point.
@app.route('/discover')
def discover():
    return jsonify(handler.discover())


# Authentication Request-Url end-point.
@app.route('/authentication/request_url', methods=['POST'])
def request_url():
    return jsonify(handler.auth_request_url())


# Authentication token gathering end-point.
@app.route('/authentication/token', methods=['POST'])
def token():
    return jsonify(handler.auth_token(request))


# Configure end-point.
@app.route('/activities/configure', methods=['POST'])
def configure():
    return jsonify(handler.configure(request))


# Activate end-point.
@app.route('/activities/activate', methods=['POST'])
def activate():
    data = fr8.routines.extract_fr8_data(request.json)
    return jsonify(data.activity.to_fr8_json())


# Deactivate end-point.
@app.route('/activities/deactivate', methods=['POST'])
def deactivate():
    data = fr8.routines.extract_fr8_data(request.json)
    return jsonify(data.activity.to_fr8_json())


# Run end-point.
@app.route('/activities/Run', methods=['POST'])
def run():
    data = fr8.routines.extract_fr8_data(request.json)

    terminal_secret = request.headers.get('FR8HUBCALLBACKSECRET')
    hub_url = request.headers.get('FR8HUBCALLBACKURL')
    payload = fr8.routines.extract_fr8_payload(
        fr8.routines.get_container_payload(
            hub_url, data.container_id, terminal.terminal_id,
            terminal_secret, data.activity.auth_token.user_id
        )
    )

    twitter_tokens = json.loads(data.activity.auth_token.token)
    auth = tweepy.OAuthHandler(terminal.twitter_consumer_key, terminal.twitter_consumer_secret)
    auth.set_access_token(twitter_tokens[0], twitter_tokens[1])

    controls_cm = fr8.routines.first_crate_of_type(
        data.activity.crate_storage,
        fr8.data.ManifestType.STANDARD_CONFIGURATION_CONTROLS
    )
    value_source = controls_cm.contents['Controls'][0]['valueSource']
    if value_source == 'specific':
        message = controls_cm.contents['Controls'][0]['textValue']
    else:
        message = fr8.routines.first_field_value(
            payload.crate_storage,
            controls_cm.contents['Controls'][0]['value']
        )

    api = tweepy.API(auth)
    api.update_status(message)

    fr8.routines.set_success_state(payload)
    json_data = payload.to_fr8_json()

    return jsonify(json_data)


# Main app entry-point.
if __name__ == '__main__':
    app.run(port=8080)