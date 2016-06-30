import base64
import json
import hashlib
import hmac
import time
import urllib
import urllib2
import uuid

import data


def create_default_hub(hub_url, terminal_id, terminal_secret, container_id, user_id):
    return Hub(hub_url, terminal_id, terminal_secret, container_id, user_id)


class Hub(object):
    def __init__(self, hub_url, terminal_id, terminal_secret, container_id, user_id):
        self.hub_url = hub_url
        self.terminal_id = terminal_id
        self.terminal_secret = terminal_secret
        self.container_id = container_id
        self.user_id = user_id

    def get_payload(self):
        payload_url = self.hub_url + 'api/v1/containers/payload?id=' + self.container_id
        hmac_header = Hub.generate_hmac_header(
            urllib.quote(payload_url, safe=''),
            self.terminal_id,
            self.terminal_secret,
            self.user_id
        )

        headers = {
            "Authorization": "hmac " + hmac_header
        }
        request = urllib2.Request(payload_url, headers=headers)
        contents = urllib2.urlopen(request).read()
        return data.PayloadDTO.from_fr8_json(json.loads(contents))

    @staticmethod
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
