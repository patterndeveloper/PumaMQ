using PumaMQ.Client.Exceptions;

namespace PumaMQ.Client.Services;

internal class ChannelNoManager
{
    private ushort _nextChannelNo;
    private readonly Queue<ushort> _retiredChannelNo = [];
    private readonly HashSet<ushort> _usedChannelNo = [];
    private readonly object _syncObj;

    internal ChannelNoManager()
    {
        _syncObj = new object();
    }

    internal ushort GetNext()
    {
        ushort channelNo;

        if (_retiredChannelNo.Any())
        {
            channelNo = _retiredChannelNo.Dequeue();
        }
        else
        {
            if (_nextChannelNo == ushort.MaxValue)
            {
                string message = $"The is no available Channel";
                throw new NoAvailableChannelException(message);
            }
            channelNo = ++_nextChannelNo;
        }
        _usedChannelNo.Add(channelNo);
        return channelNo;
    }
}