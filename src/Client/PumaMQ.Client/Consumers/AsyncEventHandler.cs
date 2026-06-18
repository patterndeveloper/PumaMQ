namespace PumaMQ.Client.Consumers;

public delegate Task AsyncEventHandler<TArgs>(object sender, TArgs args) where TArgs : AsyncEventArgs;
