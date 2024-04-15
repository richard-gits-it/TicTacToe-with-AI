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

namespace _280Final
{
    public class Client
    {
        public TcpClient _client;
        public delegate void ReceivePacketMessage(Packet280 packet);
        public event ReceivePacketMessage? ReceivePacket;

        public int[,] board = new int[3, 3];
        List<Tuple<int, int>> availableMoves = new List<Tuple<int, int>>();

        //check available moves from the board
        public List<Tuple<int, int>> CheckAvailableMoves()
        {
            availableMoves.Clear();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == 0)
                    {
                        availableMoves.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            return availableMoves;
        }

        public Client(string host, int port)
        {
            try
            {
                this._client = new TcpClient();
                this._client.Connect(host, port);
                Task.Run(() => Receive());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server: {ex.Message}");
            }
        }

        public async Task SendMessage(Packet280 packet)
        {
            try
            {
                NetworkStream stream = this._client.GetStream();
                var tmp = JsonConvert.SerializeObject(packet);
                byte[] buffer = Encoding.UTF8.GetBytes(tmp);
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync(); // Ensure data is sent immediately
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }


        public async Task<string> Receive()
        {
            try
            {
                NetworkStream stream = this._client.GetStream();
                while (true)
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    var stringMsg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var msg = JsonConvert.DeserializeObject<Packet280>(stringMsg);

                    if (msg == null)
                        return null;

                    // Notify subscribers about received message
                    NotifySubscribers(msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving message: {ex.Message}");
                return null;
            }
        }

        private void NotifySubscribers(Packet280 msg)
        {
            try
            {
                // Check if there are any subscribers and the message is not null
                if (ReceivePacket == null || msg == null)
                    return;

                // Handle specific message types
                switch (msg.ContentType)
                {
                    case MessageType.Connected:
                    case MessageType.Disconnected:
                    case MessageType.Broadcast:
                        // No specific handling required for these types
                        break;

                    case MessageType.Move:
                    case MessageType.Win:
                    case MessageType.Lose:
                    case MessageType.Draw:
                        board = JsonConvert.DeserializeObject<int[,]>(msg.Payload);
                        break;

                    case MessageType.Invite:
                    case MessageType.Accept:
                    case MessageType.Decline:
                    case MessageType.Error:
                        board = new int[3, 3];
                        break;

                    default:
                        // Handle unknown message types
                        break;
                }

                // Notify subscribers about received message
                ReceivePacket(msg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling received message: {ex.Message}");
            }
        }

        public async void DisconnectClient()
        {
            try
            {
                if (this._client != null && this._client.Connected)
                {

                    Packet280 tmp = new Packet280();
                    tmp.ContentType = MessageType.Disconnected;
                    tmp.Payload = "Client Disconnected " + Dns.GetHostName();
                    await SendMessage(tmp);

                    // Make sure you only close the connection after you have sent the message
                    this._client.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disconnecting client: {ex.Message}");
            }
        }

    }
}