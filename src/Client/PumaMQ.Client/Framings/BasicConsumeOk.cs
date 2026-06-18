using System.Text;

namespace PumaMQ.Client.Framings;

internal readonly struct BasicConsumeOk
{
    public readonly string ConsumerTag { get; }


    public BasicConsumeOk(L2Frame l2Frame)
    {
        ReadOnlySpan<byte> methodSpan = l2Frame.Method.Memory.Span;
        byte tagLen = methodSpan[0];

        if(tagLen == 0)
        {
            ConsumerTag = string.Empty;
        }
        if(methodSpan.Length > tagLen)
        {
            ConsumerTag = Encoding.UTF8.GetString(methodSpan.Slice(1, tagLen));
        }
        else
        {
            string message = $"Length of ConsumerTag is {tagLen}, but length of actual bytes is {methodSpan.Length - 1}";
            throw new ArgumentOutOfRangeException(message);
        }
    }
}