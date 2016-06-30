import uuid
import manifests
import utility


def extract_fr8_crate_contents(contents, manifest_id):
    manifest_id_str = str(manifest_id)
    if manifest_id_str not in manifests.manifest_extractors:
        raise 'Unsupported manifest_id, data extraction failed.'

    return manifests.manifest_extractors[manifest_id_str](contents)


TerminalStatus = utility.enum(ACTIVE=1, INACTIVE=0)

AuthenticationType = utility.enum(NONE=1, INTERNAL=2, EXTERNAL=3, INTERNAL_WITH_DOMAIN=4)

ActivityCategory = utility.enum(MONITORS=1, RECEIVERS=2, PROCESSORS=3, FORWARDERS=4, SOLUTION=5)

ActivityType = utility.enum(STANDARD=1, LOOP=2, SOLUTION=3)

AvailabilityType = utility.enum(NOTSET=0, CONFIGURATION=1, RUNTIME=2, ALWAYS=3)


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
        self.crate_storage = kwargs.get('crate_storage', CrateStorageDTO())
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
            'CrateStorage': self.crate_storage.to_fr8_json(),
            'ChildrenActivities': [x.to_fr8_json() for x in self.children_activities],
            'AuthTokenId': self.auth_token_id,
            'AuthToken': self.auth_token.to_fr8_json() if self.auth_token else None,
        }

    @staticmethod
    def from_fr8_json(json_data):
        a = ActivityDTO(
            name=json_data.get('Name'),
            label=json_data.get('Label'),
            activity_template=ActivityTemplateDTO.from_fr8_json(json_data.get('activityTemplate')),
            root_plan_node_id=json_data.get('RootPlanNodeId'),
            parent_plan_node_id=json_data.get('ParentPlanNodeId'),
            current_view=json_data.get('CurrentView'),
            ordering=json_data.get('Ordering'),
            id=json_data.get('Id'),
            crate_storage=CrateStorageDTO.from_fr8_json(json_data.get('CrateStorage')) if json_data.get('CrateStorage') else [],
            children_activities=[ActivityDTO.from_fr8_json(x) for x in json_data.get('ChildrenActivities')]\
                if json_data.get('ChildrenActivities') else [],
            auth_token_id = json_data.get('AuthTokenId'),
            auth_token = AuthorizationTokenDTO.from_fr8_json(json_data.get('AuthToken')) if json_data.get('AuthToken') else None
        )
        return a


class ActivityResponseDTO(object):
    def __init__(self, **kwargs):
        self.type = kwargs.get('type')
        self.body = kwargs.get('body')

    def to_fr8_json(self):
        return {
            'type': self.type,
            'body': self.body
        }

    @staticmethod
    def from_fr8_json(json_data):
        dto = ActivityResponseDTO(
            type=json_data.get('type'),
            body=json_data.get('body')
        )

        return dto


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

    @staticmethod
    def from_fr8_json(json_data):
        a = ActivityTemplateDTO(
            id=json_data.get('id'),
            name=json_data.get('name'),
            label=json_data.get('label'),
            version=json_data.get('version'),
            web_service=WebServiceDTO.from_fr8_json(json_data.get('webService')) if json_data.get('webService') else None,
            terminal=TerminalDTO.from_fr8_json(json_data.get('terminal')) if json_data.get('terminal') else None,
            activity_category=json_data.get('category'),
            activity_type=json_data.get('type'),
            min_pane_width=json_data.get('minPaneWidth'),
            needs_authentication=json_data.get('needsAuthentication')
        )
        return a


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

    @staticmethod
    def from_fr8_json(json_data):
        dto = AuthorizationTokenDTO(
            user_id=json_data.get('UserId'),
            token=json_data.get('Token'),
            external_account_id=json_data.get('ExternalAccountId'),
            external_state_token=json_data.get('ExternalStateToken')
        )
        return dto


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

    @staticmethod
    def from_fr8_json(json_data):
        c = CrateDTO(
            availability_type=json_data.get('availability'),
            contents=extract_fr8_crate_contents(json_data.get('contents'), json_data.get('manifestId')),
            create_time=json_data.get('createTime'),
            id=json_data.get('id'),
            label=json_data.get('label'),
            manifest_id=json_data.get('manifestId'),
            manifest_registrar=json_data.get('manifestRegistrar'),
            manifest_type=json_data.get('manifestType'),
            parent_crate_id=json_data.get('parentCrateId')
        )
        return c


class CrateStorageDTO(object):
    def __init__(self, **kwargs):
        self.crates = kwargs.get('crates', [])

    def first_crate_of_type(self, mt):
        for c in self.crates:
            if c.manifest_id == mt:
                return c
        return None

    def first_crate_contents_of_type(self, mt):
        crate = self.first_crate_of_type(mt)
        if not crate:
            return None

        return crate.contents

    def all_crates_of_type(self, mt):
        return [c for c in self.crates if c.manifest_id == mt]

    def to_fr8_json(self):
        return { 'crates': [x.to_fr8_json() for x in self.crates] }

    @staticmethod
    def from_fr8_json(json_data):
        dto = CrateStorageDTO(
            crates=[CrateDTO.from_fr8_json(c) for c in json_data.get('crates')]\
                if json_data.get('crates') else []
        )
        return dto


class ExternalAuthenticationDTO(object):
    def __init__(self, **kwargs):
        self.user_id = kwargs.get('user_id')
        self.parameters = kwargs.get('parameters')

    @staticmethod
    def from_fr8_json(json_data):
        ea = ExternalAuthenticationDTO()
        ea.user_id = json_data.get('Fr8UserId')
        ea.parameters = dict()
        for x in json_data.get('RequestQueryString', '').split("&"):
            t = x.split('=')
            ea.parameters[t[0]] = t[1]
        return ea


class ExternalAuthUrlDTO(object):
    def __init__(self, **kwargs):
        self.state_token = kwargs.get('state_token')
        self.url = kwargs.get('url')

    def to_fr8_json(self):
        return {
            'ExternalStateToken': self.state_token,
            'Url': self.url
        }


class FieldDTO(object):
    def __init__(self, **kwargs):
        self.name = kwargs.get('name')
        self.label = kwargs.get('label')
        self.field_type = kwargs.get('field_type')
        self.is_required = kwargs.get('is_required', False)
        self.tags = kwargs.get('tags')
        self.availability = kwargs.get('availability')
        self.data = kwargs.get('data', dict())

    def to_fr8_json(self):
        return {
            'key': self.name,
            'label': self.label,
            'fieldType': self.field_type,
            'isRequired': self.is_required,
            'tags': self.tags,
            'availability': self.availability,
            'data': self.data
        }

    @staticmethod
    def from_fr8_json(json_data):
        dto = FieldDTO(
            name=json_data.get('key'),
            label=json_data.get('label'),
            field_type=json_data.get('fieldType'),
            is_required=json_data.get('isRequired', False),
            tags=json_data.get('tags'),
            availability=json_data.get('availability', AvailabilityType.NOTSET),
            data=json_data.get('data', dict())
        )

        return dto


class Fr8DataDTO(object):
    def __init__(self, **kwargs):
        self.activity = kwargs.get('activity')
        self.container_id = kwargs.get('container_id')

    def get_user_id(self):
        return self.activity.auth_token.user_id

    def to_fr8_json(self):
        return {
            'ActivityDTO': self.activity.to_fr8_json() if self.activity else None,
            'ContainerId': self.container_id
        }

    @staticmethod
    def from_fr8_json(json_data):
        dto = Fr8DataDTO(
            activity=ActivityDTO.from_fr8_json(json_data.get('ActivityDTO')),
            container_id=json_data.get('ContainerId')
        )
        return dto


class KeyValueDTO(object):
    def __init__(self, **kwargs):
        self.key = kwargs.get('key')
        self.value = kwargs.get('value')
        self.tags = kwargs.get('tags')

    def to_fr8_json(self):
        return {
            'key': self.key,
            'value': self.value,
            'tags': self.tags
        }

    @staticmethod
    def from_fr8_json(json_data):
        dto = KeyValueDTO(
            key=json_data.get('key'),
            value=json_data.get('value'),
            tags=json_data.get('tags')
        )
        return dto


class PayloadDTO(object):
    def __init__(self, **kwargs):
        self.crate_storage = kwargs.get('crate_storage')
        self.container_id = kwargs.get('container_id')

    def success(self):
        crate = self.crate_storage.first_crate_of_type(manifests.ManifestType.OPERATIONAL_STATE)
        if not crate:
            cm = manifests.OperationalStateCM()
            crate = CrateDTO(contents=cm, availability_type=AvailabilityType.RUNTIME)
            self.crate_storage.crates.append(crate)

        crate.contents.set_success_response()

    def to_fr8_json(self):
        return {
            'container': self.crate_storage.to_fr8_json(),
            'containerId': self.container_id
        }

    @staticmethod
    def from_fr8_json(json_data):
        p = PayloadDTO(
            container_id=json_data.get('containerId'),
            crate_storage=CrateStorageDTO.from_fr8_json(json_data.get('container'))\
                if json_data.get('container') else []
        )
        return p


class PayloadObjectDTO(object):
    def __init__(self, **kwargs):
        self.payload_object = kwargs.get('payload_object', [])

    def to_fr8_json(self):
        return {
            'PayloadObject': [x.to_fr8_json() for x in self.payload_object]
        }

    @staticmethod
    def from_fr8_json(json_data):
        dto = PayloadObjectDTO(
            payload_object=[KeyValueDTO.from_fr8_json(x) for x in json_data.get('PayloadObject')]\
                if json_data.get('PayloadObject') else []
        )

        return dto


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

    @staticmethod
    def from_fr8_json(json_data):
        t = TerminalDTO(
            id=json_data.get('id'),
            name=json_data.get('name'),
            label=json_data.get('label'),
            version=json_data.get('version'),
            terminal_status=json_data.get('terminalStatus'),
            endpoint=json_data.get('endpoint'),
            description=json_data.get('description'),
            authentication_type=json_data.get('authenticationType', AuthenticationType.NONE)
        )
        return t


class WebServiceDTO(object):
    def __init__(self, **kwargs):
        self.name = kwargs.get('name')
        self.icon_path = kwargs.get('icon_path')

    def to_fr8_json(self):
        return {
            'name': self.name,
            'iconPath': self.icon_path
        }

    @staticmethod
    def from_fr8_json(json_data):
        dto = WebServiceDTO(
            name=json_data.get('name'),
            icon_path=json_data.get('iconPath')
        )
        return dto
