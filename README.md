### Tipbot for Discord that works with Blockcore

The bot is added to the [Blockcore community discord](hhttps://discord.gg/5aJk8Vv). You can check it out there.

Or you can [add Blockcore tipbot](https://discord.com/api/oauth2/authorize?client_id=<CLIENT ID>&permissions=2048&scope=bot) to your server.

Don't forget to add `Manage Messages ` permission for the bot.



#### Guide: how to setup your own instance of the bot

A Blockcore node (you can get latest release [here](https://github.com/block-core/blockcore))

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
useSegwit - To use segwit addresses, or not. true/fase
token - The applications token given to you from Discord
connectionString - To configure the databse For example, MSSQL: "Data Source=COMPUTERNAME/IP;Initial Catalog=DATABASENAME;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
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

Type: `tipbot help`