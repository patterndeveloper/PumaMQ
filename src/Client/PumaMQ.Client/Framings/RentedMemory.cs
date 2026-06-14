using System.Buffers;

namespace PumaMQ.Client.Framings;

internal struct RentedMemory : IDisposable
{
    private bool _disposed;
    private bool _takeOvered;

    private byte[] _rentedBuffer;
    internal ReadOnlyMemory<byte> Memory { get; private set; }

    internal RentedMemory(byte[] array, ReadOnlyMemory<byte> memory) : this()
    {
        _rentedBuffer = array ?? throw new ArgumentNullException(nameof(array));
        Memory = memory;
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