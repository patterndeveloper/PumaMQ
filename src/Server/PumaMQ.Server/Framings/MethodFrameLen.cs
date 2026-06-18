namespace PumaMQ.Server.Framings;

internal static class FrameLen
{
    internal const byte Type = 1;
    internal const byte Channel = 2;
    internal const byte Length = 4;
    internal const byte Marker = 1;

    internal const byte TotalFixed = Type
                                     + Channel
                                     + Length
                                     + Marker;
}


internal static class MethodFrameLen
{
    internal const byte Class = 2;
    internal const byte Method = 2;
    //Reserved
    internal const byte Ticket = 2;
    internal const byte ExchangeLen = 1;
    internal const byte RoutingKeyLen = 1;
    internal const byte Flags = 1;

    internal const byte TotalFixed = Class
                                     + Method
                                     + Ticket
                                     + Flags;
}


internal static class SpecMethodFrameLen
{
    //Connection.Start
    internal const byte VersionMajor = 1;
    internal const byte VersionMinor = 1;

    //Exchange.Declare   Queue.Declare   Queue.Bind    Basic.Publish    Basic.Consume
    internal const byte Ticket = 2;
    internal const byte Flags = 1;
    internal const byte Args = 4;
    internal const byte StringLen = 1;
}


internal static class HeaderFrameLen
{
    internal const byte Class = 2;
    internal const byte Weight = 2;
    internal const byte BodySize = 8;
    internal const byte PropertyFlags = 2;

    internal const byte Total = Class
                                + Weight
                                + BodySize
                                + PropertyFlags;
}