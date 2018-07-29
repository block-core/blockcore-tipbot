#### Tipbot for Discord that works with Stratis

A Stratis node with RPC turned on also needs to be running.

See http://stratisdevelopmentfoundation.com/install_qt/ for more information about setting this up.

Sample stratis.conf, to be placed in qt node roaming folder:

```
# RPC Settings
# Activate RPC Server (default: 0)
server = 1
# Where the RPC Server connects (default: 127.0.0.1 and ::1)
rpcconnect = 127.0.0.1
# Ip address allowed to connect to RPC (default all: 0.0.0.0 and ::)
rpcallowedip = 127.0.0.1
rpcuser = user
rpcpassword = 4815162342
rpcport=23521
```

Make sure ports, passwords, etc match the ones found in Settings.cs

You must also create a discord bot and put the token in Settings.cs

## Creating a Discord Bot

Before you can begin writing your bot, it is necessary to create a bot
account on Discord.

1. Visit the [Discord Applications Portal].
2. Create a New Application.
3. Give the application a name (this will be the bot's initial
username).
4. Create the Application.
5. In the application review page, click **Create a Bot User**.
6. Confirm the popup.
7. If this bot will be public, check "Public Bot." **Do not tick any 
other options!**

[Discord Applications Portal]: https://discordapp.com/developers/applications/me

## Adding your bot to a server

Bots **cannot** use invite links, they must be explicitly invited
through the OAuth2 flow.

1. Open your bot's application on the [Discord Applications Portal].
2. Retrieve the app's **Client ID**.
3. Create an OAuth2 authorization URL
`https://discordapp.com/oauth2/authorize?client_id=<CLIENT ID>&scope=bot`
4. Open the authorization URL in your browser.
5. Select a server.
6. Click on authorize.
	
	>[!NOTE]
	Only servers where you have the `MANAGE_SERVER` permission will be
	present in this list.

## Testing the bot is responsive

When you see the bot in the online status on your discord server, you are ready to test it. if the bots name is "BOT", then you would enter:

`@bot about`
