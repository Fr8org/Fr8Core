import fr8.controls
import fr8.data
import fr8.routines


class Handler(object):
    def configure(self, fr8_data):
        controls = fr8.routines.first_crate_of_type(
            fr8_data.activity.crate_storage,
            fr8.data.ManifestType.STANDARD_CONFIGURATION_CONTROLS
        )

        if controls is None:
            message = fr8.controls.TextSource(
                name='TwitterMessage',
                initial_label='Please enter message value',
                message_source=fr8.controls.ControlSource(
                    manifest_type=fr8.data.ManifestType.FIELD_DESCRIPTION_NAME,
                    request_upstream=True
                )
            )

            controls = fr8.data.StandardConfigurationControlsCM(controls=[message])

            crate = fr8.data.CrateDTO(
                manifest_id=fr8.data.ManifestType.STANDARD_CONFIGURATION_CONTROLS,
                manifest_type=fr8.data.ManifestType.STANDARD_CONFIGURATION_CONTROLS_NAME,
                contents=controls
            )

            fr8_data.activity.crate_storage.append(crate)

        return fr8_data.activity
