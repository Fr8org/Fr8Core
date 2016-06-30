import data
import hub
import manifests


class TerminalHandler(object):
    def __init__(self, terminal, hub_factory=None, authentication_handler=None, activity_store=None, activities=None):
        self.terminal = terminal

        if not hub_factory:
            self.hub_factory = hub.create_default_hub
        else:
            self.hub_factory = hub_factory

        if not activity_store:
            self.activity_store = ActivityStore()
        else:
            self.activity_store = activity_store

        if activities:
            for a in activities:
                self.activity_store.register_activity(a[0], a[1])

        self.authentication_handler = authentication_handler

    def discover(self):
        activity_templates = self.activity_store.get_activity_templates()
        discover_data = manifests.StandardFr8TerminalCM(terminal=self.terminal, activities=activity_templates)
        return discover_data.to_fr8_json()

    def auth_request_url(self):
        if not self.authentication_handler:
            raise 'No authentication handler registered for TerminalHandler'

        request_url_data = self.authentication_handler.get_request_url()
        return request_url_data.to_fr8_json()

    def auth_token(self, request):
        if not self.authentication_handler:
            raise 'No authentication handler registered for TerminalHandler'

        external_token_dto = data.ExternalAuthenticationDTO.from_fr8_json(request.json)
        auth_token_data = self.authentication_handler.extract_token(external_token_dto)
        return auth_token_data.to_fr8_json()

    def configure(self, request):
        fr8_data = data.Fr8DataDTO.from_fr8_json(request.json)
        activity_handler = self.activity_store.create_activity_handler(fr8_data.activity.activity_template)
        activity_handler.configure(fr8_data)
        return fr8_data.activity.to_fr8_json()

    def activate(self, request):
        fr8_data = data.Fr8DataDTO.from_fr8_json(request.json)
        activity_handler = self.activity_store.create_activity_handler(fr8_data.activity.activity_template)
        activity_handler.activate(fr8_data)
        return fr8_data.activity.to_fr8_json()

    def deactivate(self, request):
        fr8_data = data.Fr8DataDTO.from_fr8_json(request.json)
        activity_handler = self.activity_store.create_activity_handler(fr8_data.activity.activity_template)
        activity_handler.deactivate(fr8_data)
        return fr8_data.activity.to_fr8_json()

    def run(self, request):
        fr8_data = data.Fr8DataDTO.from_fr8_json(request.json)
        hub_url = request.headers.get('FR8HUBCALLBACKURL')
        terminal_secret = request.headers.get('FR8HUBCALLBACKSECRET')

        hub_communicator = self.hub_factory(
            hub_url,
            self.terminal.id,
            terminal_secret,
            fr8_data.container_id,
            fr8_data.activity.auth_token.user_id
        )

        payload = hub_communicator.get_payload()

        activity_handler = self.activity_store.create_activity_handler(fr8_data.activity.activity_template)
        activity_handler.deactivate(fr8_data)
        activity_handler.run(fr8_data, payload, hub_communicator)

        return payload.to_fr8_json()


class ActivityStore(object):
    def __init__(self):
        self.activity_templates = []
        self.activity_handlers = {}

    @staticmethod
    def create_activity_template_key(activity_template):
        return activity_template.name + '_v' + activity_template.version

    def register_activity(self, activity_template, activity_handler):
        self.activity_templates.append(activity_template)
        self.activity_handlers[ActivityStore.create_activity_template_key(activity_template)] = activity_handler

    def create_activity_handler(self, activity_template):
        return self.activity_handlers[ActivityStore.create_activity_template_key(activity_template)]()

    def get_activity_templates(self):
        return self.activity_templates
