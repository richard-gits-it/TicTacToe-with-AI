namespace TCP280Project;

public enum MessageType
{
    Broadcast,
    ServerOnly,
    Move,
    Connected,
    Disconnected,
    Invite,
    Accept,
    Decline,
    Leave,
    Win,
    Lose,
    Draw,
    Error,
    AI
}

public class Packet280
{
    public MessageType ContentType { get; set; }
    public string Payload { get; set; }
}