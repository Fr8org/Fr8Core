# Installing and running locally
#### Installing python virtual environment

Python virtual environtment should be used.
Run command prompt with python interpreter in PATH.
Then change directory to Fr8-Root/terminalTwitter

	> cd c:\workdir\fr8\terminalTwitter
	> pip install virtualenv
	> virtualenv venv
	> venv\scripts\activate

#### Next step we should install dependencies into virtual environment:

	> pip install Flask
	> pip install tweepy

#### Next we should run self-hosted Flask application:

	> python main.py


# Current SDK status
#### Supported features:
- Core DTOs for performing activity discovering, external authentication, configuration, activation, deactivation and running;
- Serializers and deserializers for core DTOs;
- Core controls:
	- Configuration
	- DropDownList
	- TextSource
- Core manifests:
	- OperationalStateCM
	- StandardConfigurationControlsCM
	- StandardPayloadDataCM
- Core Terminal-to-Hub communication mechanisms:
	- HMAC authentication support
	- External authentication support
	- Container payload retreiving


# SDK development roadmap
#### Development plan for nearest future:
- Add support for internal authentication;
- Add rest of supported manifest types;
- Add rest of supported control types;
- Add support for complex OperationalStatCM conditions;
- Add support for other Terminal-To-Hub calls;
- Add support for Bottle and Django frameworks.


# SDK features
### SDK packages
- **fr8.controls**

	Package contains controls classes and their serializers/deserializers.

- **fr8.data**

	Package contains core DTO classes and their serializers/deserializers.

- **fr8.hub**
	
	Package contains utility classes to provide Terminal-To-Hub communication. Usually user should not instantiate these classes manually, instances of these classes are provided to configure and run activy handlers.

- **fr8.manifests**

	Package contains core Crate Manifest (CM) classes and their serializers/deserializers.

- **fr8.terminal**

	Package contains TerminalHandler and ActivityStore classes. TerminalHandler should be instantiated once for whole terminal application. User usually should not instantiate ActivityStore class manually, as it is instantiated by default by TerminalHandler. More examples on TerminalHandler and ActivityStore are listed below in "Twitter terminal sample" section.

- **fr8.utility**
	
	Package contains classes and functions for SDK internal use.


### Terminal HTTP request handling
SDK was build on top of Flask framework, currently this is the only framework supported by SDK.

Before running Flask application instance user should register TerminalHandler instance, single instance per terminal application:

	# Terminal request handler.
	handler = fr8.terminal.TerminalHandler(
	    terminal_dto,
	    authentication_handler=authentication_handler,
	    activities=[
	        (activity_template_dto, activity_handler)
	    ]
	)

**terminal_dto** is an instance of fr8.data.TerminalDTO class, which should be created manually by user.

**authentication_handler** is an optional parameter, which assigns authentication handler class to TerminalHandler (see **Authentication Handler** section below)

**activities** is an optional parameter, which specifies a list of available activity templates and activity handlers for the terminal. 

In fact this parameter specifies a list of tuples, where for every tuple we specify activity_template_dto of type fr8.data.ActivityTemplateDTO class for the first element, and activity handler class for the second element (see Activity Handler section below).

After registering TerminalHandler, user should specify Flask endpoints, which will call TerminalHandler's methods:

	# Flask app instance.
	app = Flask(__name__)

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
	    app.run(port=terminal_port)


### Authentication Handler

Authentication handler class must contain following methods to meet TerminalHandler interface requirements:
- **def get_request_url(self)** - method which returns fr8.data.ExternalAuthUrlDTO structure;
- **def extract_token(self, external_token_dto)** - method which receives **external_token_dto** of fr8.data.ExternalAuthenticationDTO type. Method must return instance of fr8.data.AuthorizationTokenDTO class. For more info please see *authentication.py* file in Twitter terminal sample.

Full structure of Authentication Handler class:

	class Handler(object):	
	    def get_request_url(self):
			...
	        return fr8.data.ExternalAuthUrlDTO(state_token=oauth_token, url=redirect_url)
	
	    def extract_token(self, external_token_dto):
			...
	        return fr8.data.AuthorizationTokenDTO(
	            token=token,
	            external_state_token=state_token,
	            external_account_id=user_full_name
	        )


### Activity Handler

Activity handler class must contain following methods to meet TerminalHandler interface requirements:
- **def configure(self, fr8_data)** - method is called when TerminalHandler receives configure call. Method should not return any data. Configure method should modify activity's crate storage. TerminalHandler automatically captures all crate storage changes and serializes them back to Hub.

	**fr8_data** parameter is of fr8.data.Fr8DataDTO type.

- **def activate(self, fr8_data)** - method is called when TerminalHandler receives activate call. If activity does not contain any activation logic, simply specify *pass* statement. Method should not return any data. Similar to *configure* method, TerminalHandler automatically captures all crate storage changes and serializes them back to Hub.

	**fr8_data** parameter is of fr8.data.Fr8DataDTO type.

- **def deactivate(self, fr8_data)** - method is called when TerminalHandler receives deactivate call. If activity does not contain any deactivation logic, simply specify *pass* statement. Method should not return any data. Similar to *configure* method, TerminalHandler automatically captures all crate storage changes and serializes them back to Hub.
	
	**fr8_data** parameter is of fr8.data.Fr8DataDTO type.

- **def run(self, fr8_data, payload, hub_communicator):** - method is called when TerminalHandler receives run call.

	**fr8_data** parameter is of fr8.data.Fr8DataDTO type, contains all information regarding activity configuration.

	**payload** parameter is of fr8.data.PayloadDTO type, contains container's runtime information and crate storage.

	**hub_communicator** parameter is of fr8.hub.Hub type, the instance of default Hub Communicator implementation, which provides support for Terminal-To-Hub communication.

Full structure of Activity Handler class:

	class Handler(object):
	    def configure(self, fr8_data):
			...
			pass
	
	    def activate(self, fr8_data):
			...
	        pass
	
	    def deactivate(self, fr8_data):
			...
	        pass
	
	    def run(self, fr8_data, payload, hub_communicator):
			...
	        payload.success()


# Twitter terminal sample

We're providing Twitter terminal sample to show how to integrate all pieces together in complete solution.

Sample contains following files:
- **main.py** - entry point for Flask self-hosted web-application, TerminalHandler registration.
- **terminal.py** - terminal configuration (like twitter app keys), TerminalDTO and ActivityTemplateDTO instances creation.
- **authentication.py** - Authentication Handler class, which shows how to implement external authentication flow.
- **post_to_twitter.py** - Activity Handler class, which was designed to post a message to twitter channel and provides handling for both *configure* and *run* calls.
