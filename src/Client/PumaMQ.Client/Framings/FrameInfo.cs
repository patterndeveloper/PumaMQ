namespace PumaMQ.Client.Framings;


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




//      Method Frame (Fixed bytes = 12)
//
//      +---------------+--------------+--------------+----------------------------------+
//      | Frame Type    | Channel      | Size         | Payload                          |
//      | 1 byte        | 2 bytes      | 4 bytes      | ClassId + MethodId + Arguments   |
//      +---------------+--------------+--------------+----------------------------------+
//      | Frame End     |
//      | 1 byte        |
//      +---------------+
//
//      +---------------+---------------+----------------------+
//      | Class Id      | Method Id     | Method Arguments     |
//      | 2 bytes       | 2 bytes       | Variable             |
//      +---------------+---------------+----------------------+





//      Content-Header Frame (Fixed bytes = 20)
//
//      +---------------+--------------+--------------+-------------------------------+
//      | Frame Type    | Channel      | Size         | Payload                       |
//      | 1 byte        | 2 bytes      | 4 bytes      | Header Information            |
//      +---------------+--------------+--------------+-------------------------------+
//      | Frame End     |
//      | 1 byte        |
//      +---------------+
//
//      +---------------+---------------+---------------+---------------+
//      | Class Id      | Weight        | Body Size     | Properties    |
//      | 2 bytes       | 2 bytes       | 8 bytes       | Variable      |
//      +---------------+---------------+---------------+---------------+
//
//      Payload
//      |
//      +-- Class Id                             2 bytes
//      +-- Weight                               2 bytes(always 0)
//      +-- Body Size                            8 bytes
//      +-- Property Flags                       2+ bytes
//      +-- ContentType                          if present
//      +-- ContentEncoding                      if present
//      +-- Headers                              if present
//      +-- DeliveryMode                         if present
//      +-- Priority                             if present
//      +-- CorrelationId                        if present
//      +-- ReplyTo                              if present
//      +-- Expiration                           if present
//      +-- MessageId                            if present
//      +-- Timestamp                            if present
//      +-- Type                                 if present
//      +-- UserId                               if present
//      +-- AppId                                if present
//      +-- ClusterId                            if present




//      Body Frame (Fixed bytes = 8)
//
//      +---------------+--------------+--------------+------------------------------+
//      | Frame Type    | Channel      | Size         | Payload                      |
//      | 1 byte        | 2 bytes      | 4 bytes      | Message Body Bytes           |
//      +---------------+--------------+--------------+------------------------------+
//      | Frame End     |
//      | 1 byte        |
//      +---------------+
//
//      +-------------------------------------------+
//      | Raw Message Body Segment                  |
//      | Variable                                  |
//      +-------------------------------------------+
