# TRYING OUT HUB API

## LOCATION

Follow [this link](http://fr8.co/swagger/ui/index) to find out more about available endpoints and sample values for both input parameters and output results

## GENERAL ENDPOINTS

Most endpoints don't contain sensitive or user-specific data so you can try them out without any limitations - just provide required input parameters. You can use sample data provided for every complex object

## ENDPOINTS THAT REQUIRE AUTHENTICATION

For some endpoints you may see HTTP code `403 (Unathorized)` among possible responses. It means that just providing required parameters is not enough. In order to perform necessary authentication you should use authentication endpoint `POST https://fr8.co/api/v1/authentication/login` with the following body:

    {
        "username" : "YOUR_USERNAME",
        "password" : "YOUR_PASSWORD"
    }

You can do it manually via your favourite tool or you can navigate to this endpoint documentation on [the same page](http://dev.fr8.co/swagger/ui/index#!/Authentication/Authentication_Login). If you specifiy proper details you will receive HTTP code `200 (OK)` and you can then try out all endpoints that require authentication. If specified username of password are incorrect you will receive HTTP code `403 (Forbidden)` and you will need to repeat your authentication attempt.

*NOTE*: even though you successfully authenticate with Fr8 Hub, some endpoints may still be unaivailable to your user because they require requestor to have specific security privileges
