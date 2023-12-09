// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nethermind.Libp2p.Stack;
using Nethermind.Libp2p.Core;
using Multiformats.Address;
using Multiformats.Address.Protocols;
using Terminal.Gui;
using Samples.UI;
using System.Text;

// Initialize chat protocol.
Console.WriteLine("Initializing chat protocol...");
ChatProtocol chatProtocol = new ChatProtocol();

// Initialize chat window.
Console.WriteLine("Initializing chat window...");



var chatWindow = new ChatWindow(new ChatWindowConfig{
    Title = "Chat",
    LocalUser = "Juanu",
    OnSendMessage = async (msg) => {
        await chatProtocol.SendMessage(msg);
    }
});



ServiceProvider serviceProvider = new ServiceCollection()
    .AddSingleton<ILoggerFactory>(new LoggerFactory(chatWindow.AddDebug))
    .AddLibp2p(builder => builder.AddAppLayerProtocol(chatProtocol))
    .AddLogging(builder =>
        builder.SetMinimumLevel(args.Contains("--trace") ? LogLevel.Trace : LogLevel.Information)
            .AddSimpleConsole(l =>
            {
                l.SingleLine = true;
                l.TimestampFormat = "[HH:mm:ss.FFF]";
            }))
    .BuildServiceProvider();

chatProtocol.OnMessageReceived += (msg) =>
    chatWindow.AddChatHistory("Juanu", Encoding.UTF8.GetString(msg));

IPeerFactory peerFactory = serviceProvider.GetService<IPeerFactory>()!;

ILogger logger = new ChatLogger(chatWindow.AddDebug);
CancellationTokenSource ts = new();


if (args.Length > 0 && args[0] == "-d")
{
    Multiaddress remoteAddr = args[1];

    string addrTemplate = remoteAddr.Has<QUICv1>() ?
       "/ip4/0.0.0.0/udp/0/quic-v1" :
       "/ip4/0.0.0.0/tcp/0";

    ILocalPeer localPeer = peerFactory.Create(localAddr: addrTemplate);
    
    logger.LogInformation($"Dialing {remoteAddr}");
    IRemotePeer remotePeer = await localPeer.DialAsync(remoteAddr, ts.Token);
    chatWindow.AddPeer(remotePeer.Address.ToString());
    await remotePeer.DialAsync<ChatProtocol>(ts.Token);
    await remotePeer.DisconnectAsync();
}
else
{
    Identity optionalFixedIdentity = new(Enumerable.Repeat((byte)42, 32).ToArray());
    ILocalPeer peer = peerFactory.Create(optionalFixedIdentity);

    string addrTemplate = args.Contains("-quic") ?
        "/ip4/0.0.0.0/udp/{0}/quic-v1/p2p/{1}" :
        "/ip4/0.0.0.0/tcp/{0}/p2p/{1}";

    Console.WriteLine("Starting to listen...");
    IListener listener = await peer.ListenAsync(
        string.Format(addrTemplate, args.Length > 0 && args[0] == "-sp" ? args[1] : "0", peer.Identity.PeerId),
        ts.Token);
    Console.WriteLine("Listener started...");
    logger.LogInformation($"Listener started at {listener.Address}");
    
    listener.OnConnection += remotePeer => {
        return Task.Run(() =>{
            logger.LogInformation($"A peer connected {remotePeer.Address}");
            chatWindow.AddPeer(remotePeer.Address.ToString());
        });
    };

    Console.CancelKeyPress += delegate { listener.DisconnectAsync(); };

    await listener;
}

Application.Init();
Application.Top.Add(chatWindow);
Application.Run(Application.Top);