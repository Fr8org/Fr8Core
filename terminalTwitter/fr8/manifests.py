import controls
import data
import utility


ManifestType = utility.enum(
    FIELD_DESCRIPTION                       = 3,
    FIELD_DESCRIPTION_NAME                  = 'Field Description',
    KEY_VALUE_LIST                          = 44,
    KEY_VALUE_LIST_NAME                     = 'Key-Value pairs list',
    OPERATIONAL_STATE                       = 27,
    OPERATIONAL_STATE_NAME                  = 'Operational State',
    STANDARD_CONFIGURATION_CONTROLS         = 6,
    STANDARD_CONFIGURATION_CONTROLS_NAME    = 'Standard UI Controls',
    STANDARD_FR8_TERMINAL                   = 23,
    STANDARD_FR8_TERMINAL_NAME              = 'Standard Fr8 Terminal',
    STANDARD_PAYLOAD_DATA                   = 5,
    STANDARD_PAYLOAD_DATA_NAME              = 'Standard Payload Data'
)


class Manifest(object):
    def __init__(self, **kwargs):
        self.manifest_type = kwargs.get('manifest_type')
        self.manifest_type_name = kwargs.get('manifest_type_name')


class OperationalStateCM(Manifest):
    def __init__(self, **kwargs):
        super(OperationalStateCM, self).__init__(
            manifest_type=ManifestType.OPERATIONAL_STATE,
            manifest_type_name=ManifestType.OPERATIONAL_STATE_NAME
        )
        self.activity_response = kwargs.get('activity_response')

    def set_success_response(self):
        if not self.activity_response:
            self.activity_response = data.ActivityResponseDTO()

        self.activity_response.type = 'Success'

    def to_fr8_json(self):
        return {
            'CurrentActivityResponse': self.activity_response.to_fr8_json() if self.activity_response else None
        }

    @staticmethod
    def from_fr8_json(json_data):
        cm = OperationalStateCM(
            activity_response=data.ActivityResponseDTO.from_fr8_json(json_data)
        )

        return cm


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

    @staticmethod
    def from_fr8_json(json_data):
        if json_data is None:
            return None

        cm = StandardConfigurationControlsCM()
        cm.controls = []
        for c in json_data['Controls']:
            cm.controls.append(controls.extract_control(c))

        return cm


class StandardPayloadDataCM(Manifest):
    def __init__(self, **kwargs):
        super(StandardPayloadDataCM, self).__init__(
            manifest_type=ManifestType.STANDARD_PAYLOAD_DATA,
            manifest_type_name=ManifestType.STANDARD_PAYLOAD_DATA_NAME
        )

        self.name = kwargs.get('name')
        self.object_type = kwargs.get('object_type')
        self.payload_objects = kwargs.get('payload_objects', [])

    def to_fr8_json(self):
        return {
            'Name': self.name,
            'ObjectType': self.object_type,
            'PayloadObjects': [x.to_fr8_json() for x in self.payload_objects]
        }

    @staticmethod
    def from_fr8_json(json_data):
        cm = StandardPayloadDataCM(
            name=json_data.get('Name'),
            object_type=json_data.get('ObjectType'),
            payload_objects=[data.PayloadObjectDTO.from_fr8_json(x) for x in json_data.get('PayloadObjects')]\
                if json_data.get('PayloadObjects') else []
        )
        return cm


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


manifest_extractors = {
    str(ManifestType.OPERATIONAL_STATE): OperationalStateCM.from_fr8_json,
    str(ManifestType.STANDARD_CONFIGURATION_CONTROLS): StandardConfigurationControlsCM.from_fr8_json,
    str(ManifestType.STANDARD_PAYLOAD_DATA): StandardPayloadDataCM.from_fr8_json
}
