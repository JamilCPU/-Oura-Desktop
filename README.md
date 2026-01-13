#Currently In-Progress

##Can attempt to run the project by cloning -> navgiating to AvaloniaSidebar directory -> calling "dotnet run"

Expect to be prompted to connect to Oura. You'll need to login in and retrieve an API token from them for use with the application.

Additionally, so your token can be used for oura api calls you'll need to create a config file at backend/api/config

Name it oauth-config.json

The contents of mine(minus the actual tokens) look something like this.

{
    "client_id": "",
    "client_secret": "",
    "redirect_uri": "http://localhost:8080/callback"
  }

I'll be adding so the user doesn't have to go through manually to get things setup eventually.
