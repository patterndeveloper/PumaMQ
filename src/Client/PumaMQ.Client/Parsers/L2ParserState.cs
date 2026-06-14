namespace PumaMQ.Client.Parsers;

internal enum L2ParserState : byte
{
    None = 0,
    ExpectingMethod,
    ExpectingHeader,
    ExpectingBody,
    Complete
}