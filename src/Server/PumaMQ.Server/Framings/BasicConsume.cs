using System.Buffers.Binary;
using System.Text;

namespace PumaMQ.Server.Framings;

internal readonly struct BasicConsume : IClientRequestMethodFrame
{
    public string Queue { get; }
    public string Tag { get; }


    public BasicConsume(L2Frame l2Frame)
    {
        int currentIndex = 0;

        ReadOnlySpan<byte> methodSpan = l2Frame.Method.Memory.Span;
        ushort ticket = BinaryPrimitives.ReadUInt16BigEndian(methodSpan);
        currentIndex += SpecMethodFrameLen.Ticket;

        byte queueLen = methodSpan[currentIndex];
        currentIndex += SpecMethodFrameLen.StringLen;
        ReadOnlySpan<byte> queueSpan = methodSpan.Slice(currentIndex);

        if (queueLen == 0)
        {
            string message = $"Queue name can not be empty";
            throw new ArgumentOutOfRangeException(message);
        }
        if (queueLen > queueSpan.Length)
        {
            string message = $"Length of Queue is {queueLen}, but length of actual bytes is {queueSpan.Length}";
            throw new ArgumentOutOfRangeException(message);
        }
        queueSpan = methodSpan.Slice(currentIndex, queueLen);
        Queue = Encoding.UTF8.GetString(queueSpan);
        currentIndex += queueLen;

        //Todo: pars client-side consumer-tag
        byte tagLen = methodSpan[currentIndex];
        currentIndex += SpecMethodFrameLen.StringLen;
        ReadOnlySpan<byte> tagSpan = methodSpan.Slice(currentIndex);

        if(tagLen == 0)
        {
            string message = $"Tag name can not be empty";
            throw new ArgumentOutOfRangeException(message);
        }
        if(tagLen > tagSpan.Length)
        {
            string message = $"Length of Tag is {tagLen}, but length of actual bytes is {tagSpan.Length}";
            throw new ArgumentOutOfRangeException(message);
        }
        tagSpan = methodSpan.Slice(currentIndex, tagLen);
        Tag = Encoding.UTF8.GetString(tagSpan);
        currentIndex += tagLen;

        //Todo: pars bit-flags

        //Todo: pars Args
    }
}