using System.Buffers;

namespace PumaMQ.Server.Framings;

internal struct RentedMemory : IDisposable
{
    private bool _disposed;
    private bool _takeOvered;

    private byte[]? _rentedBuffer;
    internal ReadOnlyMemory<byte> Memory { get; private set; }


    public RentedMemory(byte[]? rentedBuffer, ReadOnlyMemory<byte> rentedMemory) : this()
    {
        _rentedBuffer = rentedBuffer;
        Memory = rentedMemory;
    }


    public RentedMemory Takeover(int offset)
    {
        if (_takeOvered)
            throw new InvalidOperationException("Already takeovered");

        RentedMemory newRentedMemory = new RentedMemory(_rentedBuffer, Memory.Slice(offset));

        _takeOvered = true;
        _rentedBuffer = null;
        Memory = ReadOnlyMemory<byte>.Empty;

        return newRentedMemory;
    }


    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_rentedBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(_rentedBuffer);
            _rentedBuffer = Array.Empty<byte>();
        }

        Memory = default;
    }
}
