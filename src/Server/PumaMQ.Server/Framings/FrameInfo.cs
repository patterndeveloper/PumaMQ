namespace PumaMQ.Server.Framings;



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