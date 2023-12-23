// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nethermind.Libp2p.Stack;
using Nethermind.Libp2p.Core;
using Multiformats.Address;
using Multiformats.Address.Protocols;
using Chat;
using Spectre.Console;

ChatCommandApp app = new ChatCommandApp();
app.Build();

ServiceProvider serviceProvider = new ServiceCollection()
    .AddLibp2p(builder => {
        var chatProtocol = new ChatProtocol(app.AddChatHistory);
        return builder.AddAppLayerProtocol(chatProtocol);
    })
    .BuildServiceProvider();

IPeerFactory peerFactory = serviceProvider.GetService<IPeerFactory>()!;

//ILogger logger = serviceProvider.GetService<ILoggerFactory>()!.CreateLogger("Chat");
CancellationTokenSource ts = new();



if (args.Length > 0 && args[0] == "-d")
{
    Multiaddress remoteAddr;
    if (args.Length > 1)
        remoteAddr = args[1];
    else
    {
        var remote = AnsiConsole.Ask<string>("Enter [green]remote address[/]?");
        remoteAddr = remote;
    }

    string addrTemplate = remoteAddr.Has<QUICv1>() ?
       "/ip4/0.0.0.0/udp/0/quic-v1" :
       "/ip4/0.0.0.0/tcp/0";

    ILocalPeer localPeer = peerFactory.Create(localAddr: addrTemplate);

    app.AddDebugMessage($"Dialing {remoteAddr}");
    IRemotePeer remotePeer = await localPeer.DialAsync(remoteAddr, ts.Token);
    // Add self
    app.AddUser(localPeer.Address.ToString() + "*");

    await remotePeer.DialAsync<ChatProtocol>(ts.Token);
    await remotePeer.DisconnectAsync();
}
else
{
    Identity optionalFixedIdentity = new(Enumerable.Repeat((byte)42, 32).ToArray());
    ILocalPeer peer = peerFactory.Create(optionalFixedIdentity);

    // Add self
    app.AddUser(peer.Address.Get<P2P>().ToString() + "*");

    // Add self
    string addrTemplate = args.Contains("-quic") ?
        "/ip4/0.0.0.0/udp/{0}/quic-v1/p2p/{1}" :
        "/ip4/0.0.0.0/tcp/{0}/p2p/{1}";

    IListener listener = await peer.ListenAsync(
        string.Format(addrTemplate, args.Length > 0 && args[0] == "-sp" ? args[1] : "0", peer.Identity.PeerId),
        ts.Token);
    // logger.LogInformation($"Listener started at {listener.Address}");
    app.AddDebugMessage($"Listener started at {listener.Address}");

    listener.OnConnection += async remotePeer => {
        app.AddDebugMessage($"A peer connected {remotePeer.Address}");
        app.AddUser(remotePeer.Address.ToString());
    };
    Console.CancelKeyPress += delegate { listener.DisconnectAsync(); };

    await listener;
}
