import json
import tweepy

from fr8.data import CrateDTO
from fr8.controls import ControlSource
from fr8.controls import TextSource
from fr8.manifests import ManifestType
from fr8.manifests import StandardConfigurationControlsCM

import terminal


class Handler(object):
    def configure(self, fr8_data):
        controls_crate = fr8_data.activity.crate_storage.first_crate_of_type(
            ManifestType.STANDARD_CONFIGURATION_CONTROLS
        )

        if not controls_crate:
            message = TextSource(
                name='TwitterMessage',
                initial_label='Please enter message value',
                message_source=ControlSource(
                    manifest_type=ManifestType.FIELD_DESCRIPTION_NAME,
                    request_upstream=True
                )
            )

            controls = StandardConfigurationControlsCM(controls=[message])

            controls_crate = CrateDTO(
                manifest_id=ManifestType.STANDARD_CONFIGURATION_CONTROLS,
                manifest_type=ManifestType.STANDARD_CONFIGURATION_CONTROLS_NAME,
                contents=controls
            )

            fr8_data.activity.crate_storage.crates.append(controls_crate)

    def activate(self, fr8_data):
        pass

    def deactivate(self, fr8_data):
        pass

    def run(self, fr8_data, payload, hub_communicator):
        twitter_tokens = json.loads(fr8_data.activity.auth_token.token)
        auth = tweepy.OAuthHandler(terminal.twitter_consumer_key, terminal.twitter_consumer_secret)
        auth.set_access_token(twitter_tokens[0], twitter_tokens[1])

        controls_cm = fr8_data.activity.crate_storage.first_crate_contents_of_type(
            ManifestType.STANDARD_CONFIGURATION_CONTROLS
        )

        text_source = controls_cm.controls[0]
        value_source = text_source.value_source
        if value_source == 'specific':
            message = controls_cm.controls[0].text_value
        else:
            message = text_source.get_value(
                payload.crate_storage,
                controls_cm.controls[0].value
            )

        api = tweepy.API(auth)
        api.update_status(message)

        payload.success()
