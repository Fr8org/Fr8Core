from flask import Flask
from flask import jsonify
from flask import request

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
    return jsonify(handler.activate(request))


# Deactivate end-point.
@app.route('/activities/deactivate', methods=['POST'])
def deactivate():
    return jsonify(handler.deactivate(request))


# Run end-point.
@app.route('/activities/Run', methods=['POST'])
def run():
    return jsonify(handler.run(request))


# Main app entry-point.
if __name__ == '__main__':
    app.run(port=terminal.terminal_port)