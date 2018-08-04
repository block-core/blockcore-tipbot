#### Tipbot for Discord that works with Stratis

A Stratis node with RPC turned on needs to be running.

```
stratis-qt.exe -rpcuser=<username> -rpcpassword=<password> -rpcport=<port> -server=1
```



Bot configuration: 

```
daemonUrl - URL of RPC server. Usually http://127.0.0.1:<RPC port>/
rpcUsername - rpc user name
rpcPassword - rpc password
walletPassword - wallet's password. Dont specify if wallet is not encrypted

token - discord bot token
```



## Creating a Discord Bot

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



## Adding your bot to a server

1. Open your bot's application on the Discord Applications Portal.
2. Retrieve the app's **Client ID**.
3. Create an OAuth2 authorization URL
  `https://discordapp.com/oauth2/authorize?client_id=<CLIENT ID>&scope=bot`
4. Open the authorization URL in your browser.
5. Select a server.
6. Click on authorize.


## Testing the bot is responsive

When you see the bot in the online status on your discord server, you are ready to test it. if the bots name is "BOT", then you would enter:

`@bot help`
