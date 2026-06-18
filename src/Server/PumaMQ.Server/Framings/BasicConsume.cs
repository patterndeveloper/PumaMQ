using System.Buffers.Binary;
using System.Text;

namespace PumaMQ.Server.Framings;

internal readonly struct BasicConsume : IRequestMethodFrame
{
    public string Queue { get; }


    public BasicConsume(L2Frame l2Frame)
    {
        int currentIndex = 0;

        ReadOnlySpan<byte> methodSpan = l2Frame.Method.Memory.Span;
        ushort ticket = BinaryPrimitives.ReadUInt16BigEndian(methodSpan);
        currentIndex += SpecMethodFrameLen.Ticket;

        byte queueLen = methodSpan[currentIndex];
        currentIndex += SpecMethodFrameLen.StringLen;

        if (queueLen == 0)
        {
            string message = $"Queue name can not be empty";
            throw new ArgumentOutOfRangeException(message);
        }
        if (queueLen > methodSpan.Length)
        {
            string message = $"Length of Queue is {queueLen}, but length of actual bytes is {methodSpan.Length}";
            throw new ArgumentOutOfRangeException(message);
        }
        Queue = Encoding.UTF8.GetString(methodSpan.Slice(currentIndex, queueLen));

        //Todo: pars client-side consumer-tag

        //Todo: pars bit-flags
    }
}