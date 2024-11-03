# Basic .NET 9 Console App

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/mitch-b/dotnet-basic-console)

Yes, there are much less verbose starting projects. No, you do not need all this stuff to play with .NET.

Quickly clone this repository to get a .NET 9 console app which:

1. Has dependency injection usage examples ([DemoService](./ConsoleApp/Services/DemoService.cs))
1. Docker container support
1. Has GitHub actions to build a container
1. Uses centralized package management
1. Has a VSCode launch profile
1. Has a devcontainer to launch in GitHub Codespaces or within VSCode itself as a remote container
1. Example getting AccessToken from Azure AD following client credentials flow (for daemons)
    * Built-in example assumes Azure App Registration has `Microsoft Graph : User.Read.All` application permission granted.

## Build & Run as Docker container

```bash
cd src/
docker build -t dotnet-basic-console:latest .
docker run --rm dotnet-basic-console:latest
```
