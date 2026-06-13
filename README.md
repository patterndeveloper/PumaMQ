# PumaMQ
A lightweight message broker and .NET client inspired by RabbitMQ, implementing core messaging concepts found in modern brokers.

PumaMQ provides asynchronous messaging, channel-based communication, binary protocol framing, and memory-efficient networking built on top of .NET's low-level performance primitives.

---

## Highlights

- Custom AMQP-inspired binary protocol
- Dedicated .NET client library
- Channel multiplexing over a single TCP connection
- Asynchronous producer/consumer messaging
- Request/Reply (RPC) communication using dot net awaitable infrastructure
- High-throughput networking with `System.IO.Pipelines` and `System.Threading.Channels`
- Low-allocation message processing using `Span<T>`
- Buffer reuse via `ArrayPool<T>`
- Concurrent connection and channel management
- Extensible broker architecture
- Multi-threaded background services
- Cancellation token orchestration including timeout-based and linked cancellation tokes
