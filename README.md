# Avalonia Sidebar

This project is currently **under active development**. Expect rough edges and manual setup steps for now.

---

## Running the Project

You can run the application locally by following these steps:

1. **Clone the repository**
2. Navigate to the sidebar project directory:
Run the application:

dotnet run

## Oura API Setup (Required)

On first run, you will be prompted to connect to Oura.

To enable Oura API access, you must manually provide OAuth credentials.

Step 1: Obtain an Oura API Token

Log in to your Oura developer account

Create an application

Retrieve your client ID and client secret

Step 2: Create the Config File

Create the following directory (assuming you didn't create one already):

backend/api/config


Inside that directory, create a file named:

oauth-config.json

Example oauth-config.json
{
  "client_id": "",
  "client_secret": "",
  "redirect_uri": "http://localhost:8080/callback"
}

### This manual setup is temporary

### Future updates will automate OAuth setup and reduce required configuration

## Expect breaking changes while the project evolves
