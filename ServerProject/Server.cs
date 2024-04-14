using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;
using TCP280Project;

namespace ServerProject
{
    public class Server
    {
        //server has to run and be listening
        private TcpListener _listener;
        List<TcpClient> _clients = new List<TcpClient>();

        //list of games that are being played on the server with the players remote end points and bool for if the game is active
        List<Tuple<string, string>> _games = new List<Tuple<string, string>>();

        //tuple for players and their usernames
        List<Tuple<string, string>> _players = new List<Tuple<string, string>>();

        //get a list of usernames from _players
        public List<string> GetPlayerList()
        {
            List<string> tmp = new List<string>();
            foreach (var player in _players)
            {
                tmp.Add(player.Item2);
            }
            return tmp;
        }



        private bool _running = false;
        public delegate void ServerMessage(Packet280 packet);
        public event ServerMessage? ServerMessageEvent;

        public delegate void LocalServerMessage(string message);
        public event LocalServerMessage? LocalServerMessageEvent;

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

        //broadcast to specific client
        private async Task BroadcastToClient(Packet280 packet, TcpClient client)
        {
            //lets turn message into a json string
            var msg = JsonConvert.SerializeObject(packet);
            //turn the string into bytes
            byte[] mMsg = Encoding.UTF8.GetBytes(msg);
            //loop through rach client and send the message
            try
            {
                NetworkStream strm = client.GetStream();
                await strm.WriteAsync(mMsg, 0, mMsg.Length);
            }
            catch
            {
                //add disconnected clients to the list, for cleanup after the loop
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
                        //check if username is already taken from the list of players
                        if (_players.Exists(p => p.Item2 == tmpMessage.Payload))
                        {
                            //send a message to the client that the username is taken
                            Packet280 errorPacket = new Packet280();
                            errorPacket.ContentType = MessageType.Error;
                            errorPacket.Payload = "Username is already taken";
                            await BroadcastToClient(errorPacket, client);
                            //close the connection
                            client.Close();
                            return;
                        }

                        LocalServerMessageEvent("A client connected: " + client.Client.RemoteEndPoint);

                        //add to the list of players
                        _players.Add(new Tuple<string, string>(client.Client.RemoteEndPoint.ToString(), tmpMessage.Payload));

                        //broadcast to all clients the list of players
                        tmpMessage.Payload = JsonConvert.SerializeObject(GetPlayerList());
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
                    else if (tmpMessage.ContentType == MessageType.Move)
                    {
                        // Assuming tmpMessage.Payload is a serialized tuple of board and player object
                        // Deserialize the payload into a tuple
                        var payload = JsonConvert.DeserializeObject < Tuple<int[,], string>>(tmpMessage.Payload);
                        // Get the board and player object from the tuple
                        var board = payload.Item1;
                        var player = payload.Item2;

                        //check board for win
                        if (CheckForWinner(board))
                        {
                            await SendWinLose(client, board, player);
                        }
                        else if (CheckForDraw(board))
                        {
                            await SendDraw(client, board, player);
                        }


                        //create a new packet for the move
                        Packet280 movePacket = new Packet280();
                        movePacket.ContentType = MessageType.Move;
                        //serialize the board and send it as the payload
                        movePacket.Payload = JsonConvert.SerializeObject(board);

                        //send the move to the other player
                        var otherPlayer = _players.Find(p => p.Item2 == player);
                        var otherPlayerClient = _clients.Find(c => c.Client.RemoteEndPoint.ToString() == otherPlayer.Item1);

                        //send the move to the other player
                        await BroadcastToClient(movePacket, otherPlayerClient);

                    }
                    else if (tmpMessage.ContentType == MessageType.Invite || tmpMessage.ContentType == MessageType.Accept ||
                        tmpMessage.ContentType == MessageType.Decline || tmpMessage.ContentType == MessageType.Win ||
                        tmpMessage.ContentType == MessageType.Lose || tmpMessage.ContentType == MessageType.Draw)
                    {
                        var receiver = _players.Find(p => p.Item2 == tmpMessage.Payload);
                        //get sender from the client
                        var sender = _players.Find(p => p.Item1 == client.Client.RemoteEndPoint.ToString());
                        //create a tuple for sender and receiver
                        Tuple<string, string> tuple = new Tuple<string, string>(sender.Item2, receiver.Item2);
                        //pack the tuple into the payload
                        tmpMessage.Payload = JsonConvert.SerializeObject(tuple);

                        //if invite and receiver is in game, send a decline
                        if (tmpMessage.ContentType == MessageType.Invite && (_games.Exists(g => g.Item2 == receiver.Item2) || _games.Exists(g => g.Item1 == receiver.Item2)))
                        {
                            tmpMessage.Payload = "Player is currently in a game.";
                            tmpMessage.ContentType = MessageType.Decline;
                        }
                        else if (tmpMessage.ContentType == MessageType.Decline && !_games.Exists(g => g.Item2 == sender.Item2))
                        {
                            tmpMessage.Payload = "Challenge Declined";
                        }

                        //send the invite to the other player
                        await BroadcastToClient(tmpMessage, _clients.Find(c => c.Client.RemoteEndPoint.ToString() == receiver.Item1));

                        //if accept add the game to the list of games
                        if (tmpMessage.ContentType == MessageType.Accept)
                        {
                            _games.Add(new Tuple<string, string>(sender.Item2, receiver.Item2));
                        }
                        else if (tmpMessage.ContentType == MessageType.Win || tmpMessage.ContentType == MessageType.Lose || tmpMessage.ContentType == MessageType.Draw)
                        {
                            _games.Remove(new Tuple<string, string>(sender.Item2, receiver.Item2));
                        }
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

        private async Task SendWinLose(TcpClient client, int[,] board, string player)
        {
            //create a new packet for the win
            Packet280 winPacket = new Packet280();
            winPacket.ContentType = MessageType.Win;
            //serialize the board and send it as the payload
            winPacket.Payload = JsonConvert.SerializeObject(board);

            //send the win to the player
            await BroadcastToClient(winPacket, client);

            //create a new packet for the lose
            Packet280 losePacket = new Packet280();
            losePacket.ContentType = MessageType.Lose;
            //serialize the board and send it as the payload
            losePacket.Payload = JsonConvert.SerializeObject(board);

            //send the lose to the other player
            var otherPlayer = _players.Find(p => p.Item2 == player);
            var otherPlayerClient = _clients.Find(c => c.Client.RemoteEndPoint.ToString() == otherPlayer.Item1);

            //send the lose to the other player
            await BroadcastToClient(losePacket, otherPlayerClient);

            //remove the game from the list of games
            _games.Remove(new Tuple<string, string>(player, otherPlayer.Item2));
        }

        private async Task SendDraw(TcpClient client, int[,] board, string player)
        {
            //create a new packet for the draw
            Packet280 drawPacket = new Packet280();
            drawPacket.ContentType = MessageType.Draw;
            //serialize the board and send it as the payload
            drawPacket.Payload = JsonConvert.SerializeObject(board);

            //send the draw to the player
            await BroadcastToClient(drawPacket, client);

            //send the draw to the other player
            var otherPlayer = _players.Find(p => p.Item2 == player);
            var otherPlayerClient = _clients.Find(c => c.Client.RemoteEndPoint.ToString() == otherPlayer.Item1);

            //send the draw to the other player
            await BroadcastToClient(drawPacket, otherPlayerClient);

            //remove the game from the list of games
            _games.Remove(new Tuple<string, string>(player, otherPlayer.Item2));
        }

        private bool CheckForWinner(int[,] board)
        {
            // Check rows
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] != 0 && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2])
                {
                    return true;
                }
            }

            // Check columns
            for (int i = 0; i < 3; i++)
            {
                if (board[0, i] != 0 && board[0, i] == board[1, i] && board[1, i] == board[2, i])
                {
                    return true;
                }
            }

            // Check diagonals
            if (board[0, 0] != 0 && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
            {
                return true;
            }

            if (board[0, 2] != 0 && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
            {
                return true;
            }

            return false;
        }

        private bool CheckForDraw(int[,] board)
        {
            // Check for a draw
            bool draw = true;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i, j] == 0)
                    {
                        draw = false;
                        break;
                    }
                }
            }

            return draw;
        }   

    }
}
