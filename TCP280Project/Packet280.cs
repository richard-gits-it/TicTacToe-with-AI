namespace TCP280Project;

public enum MessageType
{
    Broadcast,
    ServerOnly,
    GameState,
    Connected,
    Disconnected
}

public class Packet280
{
    public MessageType ContentType { get; set; }
    public string Payload { get; set; }
}