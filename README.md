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
