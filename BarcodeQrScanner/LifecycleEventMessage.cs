namespace BarcodeQrScanner;

public class LifecycleEventMessage
{
    public string EventName { get; }

    public LifecycleEventMessage(string eventName)
    {
        EventName = eventName;
    }
}