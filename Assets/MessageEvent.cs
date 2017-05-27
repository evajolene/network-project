using System;

public class MessageEventArgs : EventArgs
{
    public int identity;
    public byte[] buffer;
    public int size;

    public MessageEventArgs(int Identity, byte[] Buffer, int Size)
    {
        identity = Identity;
        buffer = Buffer;
        size = Size;
    }
};

public class MessageEvent
{
    public event EventHandler<MessageEventArgs> eventHandler;
    public readonly byte messageType;

    public MessageEvent(byte MessageType)
    {
        messageType = MessageType;
    }

    public void Event(int Identity, byte[] Buffer, int Size)
    {
        EventHandler<MessageEventArgs> handler = eventHandler;
        if (handler != null)
        {
            eventHandler(this, new MessageEventArgs(Identity, Buffer, Size));
        }
    }
};