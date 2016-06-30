import data
import manifests
import utility


ControlTypes = utility.enum(
    DROP_DOWN_LIST      = 'DropDownList',
    TEXT_SOURCE         = 'TextSource'
)


class ControlEvent(object):
    def __init__(self, **kwargs):
        self.name = kwargs.get('name')
        self.handler = kwargs.get('handler')

    def to_fr8_json(self):
        return {
            'name': self.name,
            'handler': self.handler
        }

    @staticmethod
    def from_fr8_json(json_data):
        return ControlEvent(name=json_data.get('name'), handler=json_data.get('handler'))


class ControlSource(object):
    def __init__(self, **kwargs):
        self.manifest_type = kwargs.get('manifest_type')
        self.label = kwargs.get('label')
        self.filter_by_tag = kwargs.get('filter_by_tag')
        self.request_upstream = kwargs.get('request_upstream')
        self.availability_type = kwargs.get('availability_type')

    def to_fr8_json(self):
        return {
            'manifestType': self.manifest_type,
            'label': self.label,
            'filterByTag': self.filter_by_tag,
            'requestUpstream': self.request_upstream,
            'availabilityType': self.availability_type
        }

    @staticmethod
    def from_fr8_json(json_data):
        if not json_data:
            return None

        return ControlSource(
            manifest_type=json_data.get('manifestType'),
            label=json_data.get('label'),
            filter_by_tag=json_data.get('filterByTag'),
            request_upstream=json_data.get('requestUpstream'),
            availability_type=json_data.get('availabilityType')
        )


class ControlDefinitionDTO(object):
    def __init__(self, **kwargs):
        self.name = kwargs.get('name')
        self.label = kwargs.get('label')
        self.required = kwargs.get('required', False)
        self.value = kwargs.get('value')
        self.type = kwargs.get('type')
        self.selected = kwargs.get('selected', False)
        self.events = kwargs.get('events', [])
        self.source = kwargs.get('source')
        self.is_hidden = kwargs.get('is_hidden', False)
        self.is_collapsed = kwargs.get('is_collapsed', False)

    def to_fr8_json(self):
        return {
            'name': self.name,
            'required': self.required,
            'value': self.value,
            'label': self.label,
            'type': self.type,
            'selected': self.selected,
            'events': [x.to_fr8_json() for x in self.events] if self.events else [],
            'source': self.source.to_fr8_json() if self.source is not None else None,
            'is_hidden': self.is_hidden,
            'is_collapsed': self.is_collapsed
        }

    @staticmethod
    def from_fr8_json(json_data, control=None):
        if not control:
            control = ControlDefinitionDTO()

        control.name = json_data.get('name')
        control.required = json_data.get('required', False)
        control.value = json_data.get('value')
        control.label = json_data.get('label')
        control.type = json_data.get('type')
        control.selected = json_data.get('selected', False)
        control.events = [ControlEvent.from_fr8_json(x) for x in json_data.get('events')]\
            if json_data.get('events') else []
        control.source = ControlSource.from_fr8_json(json_data.get('source'))
        control.is_hidden = json_data.get('is_hidden')
        control.is_collapsed = json_data.get('is_collapsed')


class DropDownList(ControlDefinitionDTO):
    def __init__(self, **kwargs):
        super(DropDownList, self).__init__(**kwargs)
        self.type = ControlTypes.DROP_DOWN_LIST
        self.list_items = kwargs.get('list_items', [])
        self.selected_key = kwargs.get('selected_key')
        self.has_refresh_button = kwargs.get('has_refresh_button', False)
        self.selected_item = kwargs.get('selected_item')

    def to_fr8_json(self):
        json_data = super(DropDownList, self).to_fr8_json()
        json_data['listItems'] = self.list_items
        json_data['selectedKey'] = self.selected_key
        json_data['hasRefreshButton'] = self.has_refresh_button
        json_data['selectedItem'] = self.selected_item.to_fr8_json() if self.selected_item is not None else None

        return json_data

    @staticmethod
    def from_fr8_json(json_data, control=None):
        if not control:
            control = DropDownList()

        ControlDefinitionDTO.from_fr8_json(json_data, control)
        control.list_items = json_data.get('listItems')
        control.selected_key = json_data.get('selectedKey')
        control.has_refresh_button = json_data.get('hasRefreshButton')
        control.selected_item = data.FieldDTO.from_fr8_json(json_data.get('selectedItem'))\
            if json_data.get('selectedItem') else None


class TextSource(DropDownList):
    def __init__(self, **kwargs):
        super(TextSource, self).__init__(**kwargs)
        self.type = ControlTypes.TEXT_SOURCE
        self.initial_label = kwargs.get('initial_label')
        self.upstream_source_label = kwargs.get('upstream_source_label')
        self.text_value = kwargs.get('text_value')
        self.value_source = kwargs.get('value_source')

    def get_value(self, crate_storage, field):
        crates = crate_storage.all_crates_of_type(manifests.ManifestType.STANDARD_PAYLOAD_DATA)
        for c in crates:
            for po in c.contents.payload_objects:
                for f in po.payload_object:
                    if f.key == field:
                        return f.value
        return None

    def to_fr8_json(self):
        json_data = super(TextSource, self).to_fr8_json()
        json_data['initialLabel'] = self.initial_label
        json_data['upstreamSourceLabel'] = self.upstream_source_label
        json_data['textValue'] = self.text_value
        json_data['valueSource'] = self.value_source

        return json_data

    @staticmethod
    def from_fr8_json(json_data, control=None):
        if not control:
            control = TextSource()
        DropDownList.from_fr8_json(json_data, control)
        control.initial_label = json_data.get('initialLabel')
        control.upstream_source_label = json_data.get('upstreamSourceLabel')
        control.text_value = json_data.get('textValue')
        control.value_source = json_data.get('valueSource')

        return control


control_extractors = {
    ControlTypes.DROP_DOWN_LIST: DropDownList.from_fr8_json,
    ControlTypes.TEXT_SOURCE: TextSource.from_fr8_json
}


def extract_control(json_data):
    control_type = json_data['type']
    if control_type not in control_extractors:
        raise 'Unsupported control type, data extraction failed.'

    return control_extractors[control_type](json_data)