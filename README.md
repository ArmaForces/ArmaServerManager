# ArmaForces Server Manager

![.NET Core Tests](https://github.com/ArmaForces/ArmaServerManager/workflows/.NET%20Core%20Tests/badge.svg)

<p align="center">
    <img src="https://avatars2.githubusercontent.com/u/50863181">
</p>

C# .NET Core Arma 3 Server Manager for ArmaForces unit.

~~Expected release around Dec 2020.~~ In internal tests since Jan 2021. Public Alpha version coming soon™, see [Alpha milestone](https://github.com/ArmaForces/ArmaServerManager/milestone/1).

## Main Features

### REST API

Manager is fully based on API interaction, everything can be done through the API, starting servers*, adding/removing Headless Clients, updating mods and changing the configuration.

_* It's possible to start multiple servers, but validateSignatures=3 option is not fully supported._

### Jobs scheduling

Starting server or updating mods can be scheduled to be done at some other time. This allows you to schedule server preparations before the mission and it will happen automatically, so you won't forget to update the mods. Each job you schedule has its unique id that can be used to query job status or cancel it if it's not needed anymore.

### Automatic jobs repeating on failure

Sometimes mods update may fail due to Workshop being Workshop. Or maybe there was some internet failure that caused download to break? Manager can handle that and will retry the update after waiting for some time, and will try up to 10 times over several hours before giving up. The same goes for other jobs (tasks) like server startup, which may fail if someone is on the server. Manager will wait until the server is empty (or not running) and then start your server.

### Server status from your browser

Ever wondering if your server is already running? Maybe you have been trying to join it but you don't know which mods you should load? Fear no more, Manager contains simple API to retrieve server status. You can check the server name, loaded modlist or even number of players connected without even starting Arma!

### Mods download from Workshop

Manager can download and update mods directly from workshop, saving you the time needed to copy the files or download the mods manually. Additionally Manager always performs mods integrity check and fixes broken mods for you!

### Missions and Modsets API integration

Manager integrates with other services for retrieving incoming missions details and contents of modlists, which are then used to automatically prepare server for missions.

### Freshly updated mods every morning

Every morning, Manager will retrieve incoming missions, gather all mods which will be used on these missions and update them to ensure you don't have any delay due to non-updated mods!*

_* Does not include handling CUP updates 15 minutes before the mission._

### Full clientside mods support

Did your guys ever cried to add keys for some mod? Or maybe you don't use signatures verification to allow people to load clientside mods? Manager comes to your needs, with full clientside mods support. See the table below for types of mods supported:

|  ModType   | Server loads? | Client must load? | Client can load? |
| :--------: | :-----------: | :---------------: | :--------------: |
| Serverside |      Yes      |        No         |       No         |
|  Required  |      Yes      |        Yes        |       No         |
|  Optional  |      Yes      |        No         |       Yes        |
| Clientside |      No       |        No         |       Yes        |

### Two-layer server configuration

Ever wanted to change some server settings like name or password in `server.cfg` only for one mission? Or maybe you have a modlist which has some unsigned mods and you need to toggle `verifySignatures` every time? Lucky for you, Manager supports two-layer configuration. Global server configuration can be overridden by modset specific server configuration, which prevents you from forgetting to change settings back e.g. after testing the mission. This way you can disable signatures checking when modlist with unsigned mods is loaded and it will automatically revert when other modlist is loaded!

## Missing features

- Running more than one server is not supported (unless only one of them has signatures verification enabled).
- Arma installation
- Linux support (only Windows Arma server is supported)
