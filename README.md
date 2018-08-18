### Tipbot for Discord that works with Stratis

The bot is added to the [stratis community discord](https://discord.gg/9tDyfZs). You can check it out there.

Or you can [add stratis tipbot](https://discordapp.com/oauth2/authorize?client_id=468025834519658496&scope=bot) to your server.



#### Guide: how to setup your own instance of the bot

A Stratis node (you can get latest release [here](https://github.com/stratisproject/stratisX/releases)) with RPC turned on needs to be running.

```
stratis-qt.exe -rpcuser=<username> -rpcpassword=<password> -rpcport=<port> -server=1
```



You will also need [.NET Core SDK](https://www.microsoft.com/net/download) installed.



Clone the repository, open powershell in the repo folder and run `dotnet publish` from it. That will compile the project and produce `..\TipBot\bin\Debug\netcoreapp2.1\publish` folder with `TipBot.dll` and other files in it. 

Now you can run the bot using powershell or cmd:

```
dotnet exec TipBot.dll -daemonUrl=http://127.0.0.1:23521/ -rpcUsername=USRNAME -rpcPassword=RPCPASS -walletPassword=WALLETPASS -token=BOTTOKEN
```



Bot configuration parameters: 

```
daemonUrl - URL of RPC server. Usually http://127.0.0.1:<RPC port>/
rpcUsername - rpc user name
rpcPassword - rpc password
walletPassword - wallet's password. Dont specify if wallet is not encrypted
token - discord bot token
```



#### Creating a Discord Bot

Before you can begin writing your bot, it is necessary to create a bot
account on Discord.

1. Visit the Discord Applications Portal.
2. Create a New Application.
3. Give the application a name (this will be the bot's initial
username).
4. Create the Application.
5. In the application review page, click **Create a Bot User**.
6. Confirm the popup.
7. If this bot will be public, check "Public Bot." **Do not tick any 
other options!**



#### Adding your bot to a server

1. Open your bot's application on the Discord Applications Portal.
2. Retrieve the app's **Client ID**.
3. Create an OAuth2 authorization URL
  `https://discordapp.com/oauth2/authorize?client_id=<CLIENT ID>&scope=bot`
4. Open the authorization URL in your browser.
5. Select a server.
6. Click on authorize.



#### Testing the bot is responsive

When you see the bot in the online status on your discord server, you are ready to test it. 

Type: `@tipbot help`