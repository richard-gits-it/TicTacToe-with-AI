using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCP280Project;
using TicTacToe280Project;

namespace ClientProject;

public class Client
{
    private TcpClient _client;
    public delegate void ReceivePacketMessage(Packet280 packet);
    public event ReceivePacketMessage? ReceivePacket;

    public Client(string host, int port)
    {
        //makes the connection, binds to the ip address and port
        this._client = new TcpClient(host, port);
        //just start receiving from the server
        Task.Run(() => Receive());

        //just start receiving from the server game state
        Task.Run(() => ReceiveGameState());
    
    }

    public async Task SendMessage(Packet280 packet)
    {
        NetworkStream stream = this._client.GetStream();
        var tmp = JsonConvert.SerializeObject(packet);
        byte[] buffer = Encoding.UTF8.GetBytes(tmp);
        await stream.WriteAsync(buffer, 0, buffer.Length);
    }

    public async Task<string> Receive()
    {
        NetworkStream stream = this._client.GetStream();
        while (true)
        {
            byte[] buffer;
            buffer = new byte[4096];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var stringMgs = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var msg = JsonConvert.DeserializeObject<Packet280>(stringMgs);
            if (msg.ContentType == MessageType.Connected ||
                msg.ContentType == MessageType.Disconnected ||
                msg.ContentType == MessageType.Broadcast)
            {
                //in case nothng is listening to the event, we wont call it
                if (ReceivePacket != null && msg != null)
                {
                    ReceivePacket(msg);
                }
                
            }
        }
    }

    public async void DisconnectClient()
    {
        if (this._client != null && this._client.Connected)
        {

            Packet280 tmp = new Packet280();
            tmp.ContentType = MessageType.Disconnected;
            tmp.Payload = "Client Disconnected " + Dns.GetHostName();
            await SendMessage(tmp);

            //make sure you only close the connection after you have sent the message
            this._client.Close();
        }
    }

    public async Task SendGameState(GameState gameState)
    {
        try
        {
            //serialize the GameState object to JSON
            var gameStateJson = JsonConvert.SerializeObject(gameState);
            //create a new Packet280 object with MessageType.GameState and the serialized GameState as payload
            Packet280 packet = new Packet280
            {
                ContentType = MessageType.GameState,
                Payload = gameStateJson
            };
            // Serialize the Packet280 object to JSON
            var packetJson = JsonConvert.SerializeObject(packet);
            // Convert the JSON string to bytes
            byte[] data = Encoding.UTF8.GetBytes(packetJson);
            // Get the network stream for sending data
            NetworkStream stream = this._client.GetStream();
            // Send the data to the server
            await stream.WriteAsync(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            // Exception handling
        }
    }
    public async Task ReceiveGameState()
    {
        try
        {
            NetworkStream stream = this._client.GetStream();
            byte[] buffer = new byte[4096];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var stringMsg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var msg = JsonConvert.DeserializeObject<Packet280>(stringMsg);
            if (msg.ContentType == MessageType.GameState)
            {
                // Deserialize the GameState object from the payload
                var gameState = JsonConvert.DeserializeObject<GameState>(msg.Payload);
                // Raise an event or do whatever is necessary with the received GameState
                OnReceiveGameState(gameState);
            }
            // Handle other message types if needed
        }
        catch (Exception ex)
        {
            // Exception handling
        }
    }

    // Event handler for receiving GameState
    public event Action<GameState> ReceiveGameStateEvent;

    protected virtual void OnReceiveGameState(GameState gameState)
    {
        ReceiveGameStateEvent?.Invoke(gameState);
    }

}
