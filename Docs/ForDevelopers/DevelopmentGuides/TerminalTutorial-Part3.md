
### Ok, if you feel yourself warmed up enough, do the second attempt. 
## Second terminal: Asana.com - helps you organize your todo list into projects.
If external service has SDK (and NuGet packages) it will be much easier to create Activities and handle authentication. But if not, you should do all work by yourself.
This terminal is little bit complicated, so most work will be done inside services using interfaces. You can mimic the codebase but always free to implement all the steps in way you like.

## Step 1 - Same as in first terminal, create new terminal project

## Step 2 - Fill terminal information
Here we will have one difference - *AuthenticationType* property is set to *External*. That means hub will handle authentication callbacks and going to interact with our terminal during that process.
 
    namespace terminalAsana
    {
        public static class TerminalData
        {
            public static WebServiceDTO WebServiceDTO = new WebServiceDTO
            {
                Name = "Asana",
                IconPath = "https://asana.com/favicon.ico"
            };

            public static TerminalDTO TerminalDTO = new TerminalDTO
            {
                Endpoint = CloudConfigurationManager.GetSetting("terminalAsana.TerminalEndpoint"),
                TerminalStatus = TerminalStatus.Active,
                AuthenticationType = AuthenticationType.External,
                Name = "terminalAsana",
                Label = "Asana",
                Version = "1"
            };
        }
    }
