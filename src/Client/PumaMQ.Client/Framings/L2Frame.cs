namespace PumaMQ.Client.Framings;

internal class L2Frame : IDisposable
{
    internal ClassMethod ClassMethod { get; set; }

    internal RentedMemory Method { get; set; }
    internal RentedMemory Header { get; set; }
    internal RentedMemory Body { get; set; }


    public void Dispose()
    {
        Method.Dispose();
        Header.Dispose();
        Body.Dispose();
    }
}