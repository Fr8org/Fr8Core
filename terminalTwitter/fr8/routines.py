import base64
import hashlib
import hmac
import json
import time
import urllib
import urllib2
import uuid

import controls

from data import ActivityDTO
from data import ActivityTemplateDTO
from data import AuthorizationTokenDTO
from data import CrateDTO
from data import ExternalAuthenticationDTO
from data import Fr8DataDTO
from data import ManifestType
from data import PayloadDTO
from data import StandardConfigurationControlsCM
from data import TerminalDTO
from data import WebServiceDTO


def extract_controls_crate_manifest(json_data):
    if json_data is None:
        return None

    cm = StandardConfigurationControlsCM()
    cm.controls = []
    for c in json_data['Controls']:
        cm.controls.append(controls.extract_control(c))

    return cm


manifest_extractors = {
    str(ManifestType.STANDARD_CONFIGURATION_CONTROLS): extract_controls_crate_manifest
}


def extract_fr8_auth_token(json):
    a = AuthorizationTokenDTO()
    a.user_id = json["UserId"]
    a.token = json["Token"]
    return a


def extract_fr8_web_service(json):
    ws = WebServiceDTO()
    ws.name = json["name"]
    return ws


def extract_fr8_terminal(json):
    t = TerminalDTO()
    t.id = json["id"] if "id" in json else None
    t.name = json["name"] if "name" in json else None
    t.label = json["label"] if "label" in json else None
    t.version = json["version"] if "version" in json else None
    t.terminal_status = json["terminalStatus"] if "terminalStatus" in json else None
    t.endpoint = json["endpoint"] if "endpoint" in json else None
    t.description = json["description"] if "description" in json else None
    t.authentication_type = json["authenticationType"] if "authenticationType" in json else None
    return t


def extract_fr8_activity_template(json):
    a = ActivityTemplateDTO()
    a.id = json["id"]
    a.name = json["name"]
    a.label = json["label"]
    a.version = json["version"]
    a.webService = extract_fr8_web_service(json["webService"]) if "webService" in json else None
    a.terminal = extract_fr8_terminal(json["terminal"]) if "terminal" in json else None
    a.activity_category = json["category"]
    a.activity_type = json["type"]
    a.min_pane_width = json["minPaneWidth"]
    a.needs_authentication = json["needsAuthentication"]
    return a


def extract_fr8_crate_contents(contents, manifest_id):
    manifest_id_str = str(manifest_id)
    if manifest_id_str not in manifest_extractors:
        raise 'Unsupported manifest_id, data extraction failed.'

    return manifest_extractors[manifest_id_str](contents)


def extract_fr8_crate(json_data):
    c = CrateDTO()
    c.availability_type = json_data['availability']
    c.contents = extract_fr8_crate_contents(json_data['contents'], json_data['manifestId'])
    c.create_time = json_data['createTime']
    c.id = json_data['id']
    c.label = json_data['label']
    c.manifest_id = json_data['manifestId']
    c.manifest_registrar = json_data['manifestRegistrar']
    c.manifest_type = json_data['manifestType']
    c.parent_crate_id = json_data['parentCrateId']

    return c


def extract_fr8_crate_storage(json):
    r = []
    for c in json['crates']:
        r.append(extract_fr8_crate(c))
    return r


def extract_fr8_activity(json):
    a = ActivityDTO()
    a.name = json["Name"]
    a.label = json["Label"]
    a.activity_template = extract_fr8_activity_template(json["activityTemplate"])
    a.root_plan_node_id = json["RootPlanNodeId"]
    a.parent_plan_node_id = json["ParentPlanNodeId"]
    a.current_view = json["CurrentView"]
    a.ordering = json["Ordering"]
    a.id = json["Id"]
    a.crate_storage = extract_fr8_crate_storage(json["CrateStorage"])\
        if "CrateStorage" in json and json["CrateStorage"] is not None else []
    a.children_activities = map(lambda x: extract_fr8_activity(x), json["ChildrenActivities"])\
        if "ChildrenActivities" in json and json["ChildrenActivities"] is not None else []
    a.auth_token_id = json["AuthTokenId"]
    a.auth_token = extract_fr8_auth_token(json["AuthToken"])
    return a


def extract_fr8_payload(json):
    p = PayloadDTO()
    p.container_id = json["containerId"]
    p.crate_storage = extract_fr8_crate_storage(json["container"])\
        if "container" in json and json["container"] is not None else []

    return p


def extract_fr8_data(json):
    data = Fr8DataDTO()
    data.activity = extract_fr8_activity(json["ActivityDTO"])
    data.container_id = json["ContainerId"] if "ContainerId" in json else None
    return data


def extract_fr8_external_token_data(json_data):
    ea = ExternalAuthenticationDTO()
    ea.user_id = json_data['Fr8UserId']
    ea.parameters = dict()
    for x in json_data['RequestQueryString'].split("&"):
        t = x.split('=')
        ea.parameters[t[0]] = t[1]
    return ea


def first_crate_of_type(crate_storage, mt):
    for c in crate_storage:
        if c.manifest_id == mt:
            return c
    return None


def all_crates_of_type(crate_storage, mt):
    r = []
    for c in crate_storage:
        if c.manifest_id == mt:
            r.append(c)
    return r


def generate_hmac_header(url, terminal_id, terminal_secret, user_id, content = bytearray()):
    timestamp = str(int(time.time()))
    nonce = uuid.uuid4()

    m = hashlib.md5()
    m.update(content)
    md5_digest = m.digest()
    md5_base64 = base64.b64encode(md5_digest)

    raw = terminal_id + url + timestamp + str(nonce) + md5_base64 + user_id
    key_bytes = bytearray(str(terminal_secret))
    message_bytes = bytearray(raw, 'utf-8')

    hmac_digest = hmac.new(key_bytes, message_bytes, hashlib.sha512).digest()
    hmac_base64 = base64.b64encode(hmac_digest)

    result = terminal_id + ":" + hmac_base64 + ":" + str(nonce) + ":" + timestamp + ":" + user_id
    return result


def get_container_payload(hub_url, container_id, terminal_id, terminal_secret, user_id):
    payload_url = hub_url + "api/v1/containers/payload?id=" + container_id

    hmac = generate_hmac_header(urllib.quote(payload_url, safe=''), terminal_id, terminal_secret, user_id)

    headers = {
        "Authorization": "hmac " + hmac
    }
    request = urllib2.Request(payload_url, headers = headers)
    contents = urllib2.urlopen(request).read()
    return json.loads(contents)


def set_success_state(payload):
    os_crate = first_crate_of_type(payload.crate_storage, ManifestType.operational_state)
    os_crate.contents["CurrentActivityResponse"] = {
        "type": "Success",
        "body": None
    }


def first_field_value(crate_storage, field):
    crates = all_crates_of_type(crate_storage, ManifestType.standard_payload_data)
    for c in crates:
        for po in c.contents["PayloadObjects"]:
            for f in po["PayloadObject"]:
                if f["key"] == field:
                    return f["value"]
    return None
