// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Text;
using Multiformats.Address;
using Nethermind.Libp2p.Core;

internal class ChatProtocol : SymmetricProtocol, IProtocol
{
    Action<Multiaddress, string> _onMessage;
    private static readonly ConsoleReader Reader = new();
    public string Id => "/chat/1.0.0";

    public ChatProtocol(Action<Multiaddress, string> onMessage) : base()
    {
        _onMessage = onMessage;
    }


    protected override async Task ConnectAsync(IChannel channel, IChannelFactory channelFactory,
        IPeerContext context, bool isListener)
    {
        Console.Write("> ");
        _ = Task.Run(async () =>
        {
            while (!channel.Token.IsCancellationRequested)
            {
                ReadOnlySequence<byte> read =
                    await channel.ReadAsync(0, ReadBlockingMode.WaitAny, channel.Token);
                _onMessage(context.RemoteEndpoint, Encoding.UTF8.GetString(read).Replace("\n\n", "\n> "));
            }
        }, channel.Token);
        while (!channel.Token.IsCancellationRequested)
        {
            string line = await Reader.ReadLineAsync(channel.Token);
            Console.Write("> ");
            byte[] buf = Encoding.UTF8.GetBytes(line + "\n\n");
            await channel.WriteAsync(new ReadOnlySequence<byte>(buf));
        }
    }
}
