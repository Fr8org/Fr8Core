import json
import uuid


def enum(**enums):
    return type('Enum', (object,), enums)

TerminalStatus = enum(ACTIVE=1, INACTIVE=0)

AuthenticationType = enum(NONE=1, INTERNAL=2, EXTERNAL=3, INTERNAL_WITH_DOMAIN=4)

ActivityCategory = enum(MONITORS=1, RECEIVERS=2, PROCESSORS=3, FORWARDERS=4, SOLUTION=5)

ActivityType = enum(STANDARD=1, LOOP=2, SOLUTION=3)

AvailabilityType = enum(NOTSET=0, CONFIGURATION=1, RUNTIME=2, ALWAYS=3)

ManifestType = enum(
    FIELD_DESCRIPTION                       = 3,
    FIELD_DESCRIPTION_NAME                  = 'Field Description',
    STANDARD_PAYLOAD_DATA                   = 5,
    STANDARD_PAYLOAD_DATA_NAME              = 'Standard Payload Data',
    STANDARD_CONFIGURATION_CONTROLS         = 6,
    STANDARD_CONFIGURATION_CONTROLS_NAME    = 'Standard UI Controls',
    STANDARD_FR8_TERMINAL                   = 23,
    STANDARD_FR8_TERMINAL_NAME              = 'Standard Fr8 Terminal',
    OPERATIONAL_STATE                       = 27,
    OPERATIONAL_STATE_NAME                  = 'Operational State'
)


class Manifest(object):
    def __init__(self, **kwargs):
        self.manifest_type = kwargs.get('manifest_type')
        self.manifest_type_name = kwargs.get('manifest_type_name')


class StandardFr8TerminalCM(Manifest):
    def __init__(self, **kwargs):
        super(StandardFr8TerminalCM, self).__init__(
            manifest_type=ManifestType.STANDARD_FR8_TERMINAL,
            manifest_type_name=ManifestType.STANDARD_FR8_TERMINAL_NAME
        )

        self.terminal = kwargs.get('terminal')
        self.activities = kwargs.get('activities', [])

    def to_fr8_json(self):
        return {
            'definition': self.terminal.to_fr8_json() if self.terminal else None,
            'activities': [x.to_fr8_json() for x in self.activities] if self.activities else None
        }


class TerminalDTO:
    def __init__(self, **kwargs):
        self.id = kwargs.get('id')
        self.name = kwargs.get('name')
        self.label = kwargs.get('label')
        self.version = kwargs.get('version')
        self.terminal_status = kwargs.get('terminal_status', TerminalStatus.ACTIVE)
        self.endpoint = kwargs.get('endpoint')
        self.description = kwargs.get('description')
        self.authentication_type = kwargs.get('authentication_type', AuthenticationType.NONE)

    def to_fr8_json(self):
        return {
            'id': self.id,
            'name': self.name,
            'label': self.label,
            'version': self.version,
            'terminalStatus': self.terminal_status,
            'endpoint': self.endpoint,
            'description': self.description,
            'authenticationType': self.authentication_type
        }


class WebServiceDTO(object):
    def __init__(self, **kwargs):
        self.name = kwargs.get('name')

    def to_fr8_json(self):
        return {
            'name': self.name
        }


class ActivityTemplateDTO(object):
    def __init__(self, **kwargs):
        self.id = kwargs.get('id')
        self.name = kwargs.get('name')
        self.version = kwargs.get('version')
        self.terminal = kwargs.get('terminal')
        self.web_service = kwargs.get('web_service')
        self.activity_category = kwargs.get('activity_category')
        self.needs_authentication = kwargs.get('needs_authentication', False)
        self.label = kwargs.get('label', '')
        self.activity_type = kwargs.get('activity_type', ActivityType.STANDARD)
        self.min_pane_width = kwargs.get('min_pane_width', 380)

    def to_fr8_json(self):
        return {
            'id': self.id,
            'name': self.name,
            'version': self.version,
            'terminal': self.terminal.to_fr8_json(),
            'webService': self.web_service.to_fr8_json() if self.web_service is not None else None,
            'category': self.activity_category,
            'needsAuthentication': self.needs_authentication,
            'label': self.label,
            'type': self.activity_type,
            'minPaneWidth': self.min_pane_width
        }


class AuthorizationTokenDTO(object):
    def __init__(self, **kwargs):
        self.user_id = kwargs.get('user_id')
        self.token = kwargs.get('token')
        self.external_account_id = kwargs.get('external_account_id')
        self.external_state_token = kwargs.get('external_state_token')

    def to_fr8_json(self):
        return {
            'UserId': self.user_id,
            'Token': self.token,
            'ExternalAccountId': self.external_account_id,
            'ExternalStateToken': self.external_state_token
        }


class ActivityDTO(object):
    def __init__(self, **kwargs):
        self.name = kwargs.get('name')
        self.label = kwargs.get('label')
        self.activity_template = kwargs.get('activity_template')
        self.root_plan_node_id = kwargs.get('root_plan_node_id')
        self.parent_plan_node_id = kwargs.get('parent_plan_node_id')
        self.current_view = kwargs.get('current_view')
        self.ordering = kwargs.get('ordering')
        self.id = kwargs.get('id')
        self.crate_storage = kwargs.get('crate_storage', [])
        self.children_activities = kwargs.get('children_activities', [])
        self.auth_token_id = kwargs.get('auth_token_id')
        self.auth_token = kwargs.get('auth_token')

    def to_fr8_json(self):
        return {
            'Name': self.name,
            'Label': self.label,
            'ActivityTemplate': self.activity_template.to_fr8_json(),
            'RootPlanNodeId': self.root_plan_node_id,
            'ParentPlanNodeId': self.parent_plan_node_id,
            'CurrentView': self.current_view,
            'Ordering': self.ordering,
            'Id': self.id,
            'CrateStorage': {
                'Crates': [x.to_fr8_json() for x in self.crate_storage]
            },
            'ChildrenActivities': [x.to_fr8_json() for x in self.children_activities],
            'AuthTokenId': self.auth_token_id,
            'AuthToken': self.auth_token.to_fr8_json() if self.auth_token else None,
        }


class CrateDTO(object):
    def __init__(self, **kwargs):
        self.manifest_type = kwargs.get('manifest_type')
        self.manifest_id = kwargs.get('manifest_id')
        self.manifest_registrar = kwargs.get('manifest_registrar')
        self.id = kwargs.get('id', uuid.uuid4())
        self.label = kwargs.get('label')
        self.contents = kwargs.get('contents')
        self.parent_crate_id = kwargs.get('parent_crate_id')
        self.create_time = kwargs.get('create_time', '')
        self.availability_type = kwargs.get('availability_type', AvailabilityType.NOTSET)

    def to_fr8_json(self):
        return {
            'manifestType': self.manifest_type,
            'manifestId': self.manifest_id,
            'manifestRegistrar': self.manifest_registrar,
            'id': self.id,
            'label': self.label,
            'contents': self.contents.to_fr8_json(),
            'createTime': self.create_time,
            'availability': self.availability_type
        }


class Fr8DataDTO(object):
    def __init__(self, **kwargs):
        self.activity = kwargs.get('activity')
        self.container_id = kwargs.get('container_id')

    def get_user_id(self):
        return self.activity.auth_token.user_id


class FieldDTO(object):
    def __init__(self, **kwargs):
        self.key = kwargs.get('key')
        self.value = kwargs.get('value')
        self.availability_type = kwargs.get('availability_type')

    def to_fr8_json(self):
        return {
            'key': self.key,
            'value': self.value,
            'availability': self.availability_type
        }


class StandardConfigurationControlsCM(Manifest):
    def __init__(self, **kwargs):
        super(StandardConfigurationControlsCM, self).__init__(
            manifest_type=ManifestType.STANDARD_CONFIGURATION_CONTROLS,
            manifest_type_name=ManifestType.STANDARD_CONFIGURATION_CONTROLS_NAME
        )
        self.controls = kwargs.get('controls', [])

    def to_fr8_json(self):
        return {
            'Controls': [x.to_fr8_json() for x in self.controls]
        }


class ExternalAuthenticationDTO(object):
    def __init__(self, **kwargs):
        self.user_id = kwargs.get('user_id')
        self.parameters = kwargs.get('parameters')


class PayloadDTO(object):
    def __init__(self, **kwargs):
        self.crate_storage = kwargs.get('crate_storage')
        self.container_id = kwargs.get('container_id')

    def to_fr8_json(self):
        return {
            'container': {
                'crates': [x.to_fr8_json() for x in self.crate_storage]
            },
            'containerId': self.container_id
        }


class ExternalAuthUrlDTO(object):
    def __init__(self, **kwargs):
        self.state_token = kwargs.get('state_token')
        self.url = kwargs.get('url')

    def to_fr8_json(self):
        return {
            'ExternalStateToken': self.state_token,
            'Url': self.url
        }
