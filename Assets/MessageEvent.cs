using System;

public class MessageEventArgs : EventArgs
{
    public byte[] buffer;
    public int size;

    public MessageEventArgs(byte[] Buffer, int Size)
    {
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

    public void Event(byte[] Buffer, int Size)
    {
        EventHandler<MessageEventArgs> handler = eventHandler;
        if (handler != null)
        {
            eventHandler(this, new MessageEventArgs(Buffer, Size));
        }
    }
};