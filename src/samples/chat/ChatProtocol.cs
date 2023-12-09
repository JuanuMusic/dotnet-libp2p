// SPDX-FileCopyrightText: 2023 Demerzel Solutions Limited
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Text;
using Nethermind.Libp2p.Core;

internal class ChatProtocol : SymmetricProtocol, IProtocol
{
    IChannel _channel;
    // On message received event
    public event Action<ReadOnlySequence<byte>>? OnMessageReceived;

    //private static readonly ConsoleReader Reader = new();
    public string Id => "/chat/1.0.0";
    

    protected override async Task ConnectAsync(IChannel channel, IChannelFactory? channelFactory,
        IPeerContext context, bool isListener)
    {
        Console.WriteLine("Connected to peer...");
        _channel = channel;
        
        while (!channel.Token.IsCancellationRequested)
        {
            ReadOnlySequence<byte> read =
                await channel.ReadAsync(0, ReadBlockingMode.WaitAny, channel.Token);
            
            if (OnMessageReceived != null)
                OnMessageReceived(read);
            
            //Console.Write(Encoding.UTF8.GetString(read).Replace("\n\n", "\n> "));
        }
    }

    public async Task SendMessage(string message)
    {
        if (!_channel.Token.IsCancellationRequested)
        {
            //Console.Write("> :) ");
            byte[] buf = Encoding.UTF8.GetBytes(message);
            await _channel.WriteAsync(new ReadOnlySequence<byte>(buf));
        }
    }
}
