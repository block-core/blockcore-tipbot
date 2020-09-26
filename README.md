
<p align="center">
  <p align="center">
    <img src="https://user-images.githubusercontent.com/5221349/72841405-93c2ce80-3c96-11ea-844b-3e1ff782b1ae.png" height="100" alt="Blockcore" />
  </p>
  <h3 align="center">
    About Blockcore TipBot
  </h3>
  <p align="center">
    Tipbot for Discord that works with Blockcore based blockchains
  </p>
</p>


# Introduction

The bot is added to the [Blockcore community discord](hhttps://discord.gg/5aJk8Vv). You can check it out there.

Or you can [add Blockcore tipbot](https://discord.com/api/oauth2/authorize?client_id=<CLIENT ID>&permissions=2048&scope=bot) to your server.

Don't forget to add `Manage Messages ` permission for the bot.


#### Guide: how to setup your own instance of the bot

A Blockcore Reference Node (you can get latest release [here](https://github.com/block-core/blockcore-nodes/releases))

```
blockcore-desired-chain.exe
```

You will also need [.NET Core SDK](https://www.microsoft.com/net/download) installed.

Clone the repository, open powershell in the repo folder and run `dotnet publish` from it. That will compile the project and produce `..\TipBot\bin\Debug\netcoreapp3.1\publish` folder with `TipBot.dll` and other files in it. 

Now you can run the bot using powershell or cmd:

```
dotnet exec TipBot.dll -apiUrl=http://127.0.0.1:PORT/ -walletName=WALLETNAME -walletPassword=WALLETPASSWORD -networkFee=0.01 -useSegwit=true -token=DISCORDBOTTOKEN
```

Bot configuration parameters: 

```
apiUrl - URL of API server. Usually http://127.0.0.1:<API port>/
walletName - the wallet name
walletPassword - the wallet password
networkFee - The network fee for withdraws
useSegwit - To use segwit addresses, or not. true/false
token - The applications token given to you from Discord
connectionString - To configure the database For example, MSSQL: "Data Source=COMPUTERNAME/IP;Initial Catalog=DATABASENAME;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
```


#### Creating a Discord Bot

Before you can begin writing your bot, it is necessary to create a bot
account on Discord.

1. Visit the [Discord Applications Portal](https://discord.com/developers).
1. Create a New Application.
1. Give the application a name (this will be the bot's initial
username).
1. Select the "Bot" menu option under "Settings" on the left.
1. Click the "Add Bot" button and confirm the dialog.
1. Uncheck the "Public Bot" checkbox to ensure only you can add your tipping bot to selected Discord servers.

#### Adding your bot to a server

1. Open your bot's application on the Discord Applications Portal.
1. Retrieve the app's **Client ID**.
1. Create an OAuth2 authorization URL
  `https://discordapp.com/oauth2/authorize?client_id=<CLIENT ID>&scope=bot`
1. Open the authorization URL in your browser.
1. Select a server.
1. Click on authorize.


#### Testing the bot is responsive

When you see the bot in the online status on your discord server, you are ready to test it. 

Type: `tipbot help`


## User Secrets

The Blockcore TipBot utilizes the latest configuration APIs in .NET Core, and allows you to securely configure 
user secrets with the token for your Discord bot and SQL Connection string. Right-click on the project in Visual Studio 2019 
and choose "Manage User Secrets".

## Microsoft SQL Server

You can utilize the container for Microsoft SQL Server to quickly run an instance of MSSQL used by the TipBot.

```sh
sudo docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=MyVeryOwnPassword!23" -p 14330:1433 -d mcr.microsoft.com/mssql/server:2019-latest
```

Connection string can then be set to:

```
Server=127.0.0.1,14330;Database=TipBot;User Id=SA;Password=MyVeryOwnPassword!23
```

## Docker Cleanup

Docker provides a single command that will clean up any resources — images, containers, volumes, and networks — that are dangling (not associated with a container):

```
docker system prune
```

To additionally remove any stopped containers and all unused images (not just dangling images), add the -a flag to the command:

```
docker system prune -a
```