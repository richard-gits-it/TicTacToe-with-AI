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

namespace GameDemo
{
    public class Client
    {
        private TcpClient _client;
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
            //makes the connection, binds to the ip address and port
            this._client = new TcpClient(host, port);
            //just start receiving from the server
            Task.Run(() => Receive());

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

                if (msg == null)
                    return null;

                if (msg.ContentType == MessageType.Connected)
                {
                    //in case nothng is listening to the event, we wont call it
                    if (ReceivePacket != null && msg != null)
                    {
                        ReceivePacket(msg);
                    }

                }else if (msg.ContentType == MessageType.Disconnected)
                {
                    //in case nothng is listening to the event, we wont call it
                    if (ReceivePacket != null && msg != null)
                    {
                        ReceivePacket(msg);
                    }
                }else if (msg.ContentType == MessageType.Broadcast)
                {

                }else if (msg.ContentType == MessageType.Move)
                {
                    board = JsonConvert.DeserializeObject<int[,]>(msg.Payload); 
                    //in case nothng is listening to the event, we wont call it
                    if (ReceivePacket != null && msg != null)
                    {
                        ReceivePacket(msg);
                    }
                }else if (msg.ContentType == MessageType.Move || msg.ContentType == MessageType.Invite || msg.ContentType == MessageType.Accept ||
                        msg.ContentType == MessageType.Decline || msg.ContentType == MessageType.Win ||
                        msg.ContentType == MessageType.Lose || msg.ContentType == MessageType.Draw || msg.ContentType == MessageType.Error)
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

    }
}