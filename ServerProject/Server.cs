using Human;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TCP280Project;
using TicTacToe280Project;

namespace ServerProject
{
    public class Server
    {
        //server has to run and be listening
        private TcpListener _listener;
        List<TcpClient> _clients = new List<TcpClient>();
        private bool _running = false;
        public delegate void ServerMessage(Packet280 packet);
        public event ServerMessage? ServerMessageEvent;

        public delegate void LocalServerMessage(string message);
        public event LocalServerMessage? LocalServerMessageEvent;

        //initialize the game state
        GameState gameState = new GameState(new HumanPlayer(1), new HumanPlayer(-1));

        //construct the server and tell whih port to start on
        public Server(int port)
        {
            this._listener = new TcpListener(System.Net.IPAddress.Any, port);
        }
        public async Task Stop()
        {
            //set our bool that contols our loo[
            this._running = false;
            //stop the listener
            this._listener.Stop();
            //send a message to the UI
            if (LocalServerMessageEvent != null)
                LocalServerMessageEvent("Server has stopped");
        }
        public async Task Start()
        {
            this._listener.Start();
            //server has started
            if (LocalServerMessageEvent != null)
                LocalServerMessageEvent("Server has started");
            //keep track that we started
            this._running = true;
            while (this._running)
            {
                try
                {
                    TcpClient client = await this._listener.AcceptTcpClientAsync();
                    //add them to the list of clients
                    this._clients.Add(client);
                    //make a new packet
                    Packet280 packet = new Packet280();
                    //message type to forward to everyone
                    packet.ContentType = MessageType.Broadcast;
                    //payload will just have cclient info
                    packet.Payload = $"Client connected: {client.Client.RemoteEndPoint}";
                    //start handling the client
                    Task.Run(() => HandleClient(client));
                }
                catch (Exception ex)
                {

                    LocalServerMessageEvent(ex.Message);
                }
            }
        }

        private async Task BroadcastToAllClients(Packet280 packet)
        {
            //make a temp list for disconnected clients
            List<TcpClient> disconnectedClients = new List<TcpClient>();


            //lets turn message into a json string
            var msg = JsonConvert.SerializeObject(packet);
            //turn the string into bytes
            byte[] mMsg = Encoding.UTF8.GetBytes(msg);
            //loop through rach client and send the message
            foreach (var client in _clients)
            {
                try
                {
                    NetworkStream strm = client.GetStream();
                    await strm.WriteAsync(mMsg, 0, mMsg.Length);
                }
                catch
                {
                    //add disconnected clients to the list, for cleanup after the loop
                    disconnectedClients.Add(client);
                }
            }
            foreach (var client in disconnectedClients)
            {
                //clean up the broken connections
                _clients.Remove(client);
            }
        }
        public async void HandleClient(TcpClient client)
        {
            try
            {
                //setup a stream with a networked machine
                NetworkStream stream = client.GetStream();
                //setup a buffer to read the data
                byte[] buffer = new byte[4096];
                //loop and wait for bytes to arrive from the client
                int bytesRead;
                while((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    //read message as a string
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    //deserialize a json string back into a packet280
                    var tmpMessage = JsonConvert.DeserializeObject<Packet280>(message);
                    if (tmpMessage.ContentType == MessageType.Connected)
                    {
                        LocalServerMessageEvent("A client connected: " + client.Client.RemoteEndPoint);
                        await BroadcastToAllClients(tmpMessage);

                    }
                    else if (tmpMessage.ContentType == MessageType.Disconnected)
                    {
                        LocalServerMessageEvent("A client disconnected: " + client.Client.RemoteEndPoint);
                        await BroadcastToAllClients(tmpMessage);
                        //client has gracefully disconnected, so we remove them from the list
                        this._clients.Remove(client);

                    }
                    else if (tmpMessage.ContentType == MessageType.Broadcast)
                    {
                        //what broadcast means is that we should send it to everyone
                        await BroadcastToAllClients(tmpMessage);
                        //this line 
                        LocalServerMessageEvent(tmpMessage.Payload);
                    }
                    else if (tmpMessage.ContentType == MessageType.GameState)
                    {
                        // Assuming tmpMessage.Payload is a serialized GameState object
                        // Deserialize the GameState object
                        var board = JsonConvert.DeserializeObject<int[,]>(tmpMessage.Payload);
                        gameState.board = board;
                        // Send the GameState to this specific client
                        await BroadcastToAllClients(tmpMessage);
                    }
                }
                //connection is lost
                Packet280 packet = new Packet280();
                //client disconnects, so we broadcast to let others know
                packet.ContentType = MessageType.Broadcast;
                packet.Payload = $"Client disconnected: {client.Client.RemoteEndPoint}";

            }
            catch (Exception ex)
            {
                Packet280 packet = new Packet280();
                packet.ContentType = MessageType.Broadcast;
                packet.Payload = $"Client disconnected: {client.Client.RemoteEndPoint} :: {ex.Message}";
                //TODO: we neeed an event to actually invoke the message
            }
        }

        private async Task SendGameStateToClient(TcpClient client, GameState gameState)
        {
            // Serialize the GameState object to JSON
            var gameStateJson = JsonConvert.SerializeObject(gameState);
            // Create a new Packet280 object to send the serialized GameState
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
            NetworkStream stream = client.GetStream();
            // Send the data to the client
            await stream.WriteAsync(data, 0, data.Length);
        }


    }
}
