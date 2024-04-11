using System.Net;
using TCP280Project;

namespace ClientProject
{
    public partial class ClientForm : Form
    {
        private Client client;
        public ClientForm()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            client = new Client(txtIpAddress.Text, int.Parse(txtPort.Text));
            client.ReceivePacket += Client_ReceivePacket;
            //we have just connected we need to notify the server
            Packet280 tmp = new Packet280();
            tmp.ContentType = MessageType.Connected;
            tmp.Payload = "Hello Server!";
            await client.SendMessage(tmp);
        }

        private void Client_ReceivePacket(Packet280 packet)
        {
            this.Invoke(() => lstClientMessages.Items.Add(packet.Payload));
        }

        private async void btnTestMessage_Click(object sender, EventArgs e)
        {
            string host = Dns.GetHostName();

            Packet280 packet = new Packet280();
            packet.ContentType = MessageType.Broadcast;
            packet.Payload = $"{host}: {txtMessage.Text}";
            //packet.Payload = "This is a test message from " + host;
            await client.SendMessage(packet);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            client.DisconnectClient();
        }

        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.DisconnectClient();
        }

        private async void btnSendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FileTransferObject obj = new FileTransferObject();
                obj.FileName = ofd.SafeFileName;
                //actually load the bytes of the file
                obj.FileBytes = File.ReadAllBytes(ofd.FileName);

                Packet280 packet = new Packet280();
                //important to know a file is coming across
                //packet.ContentType = MessageType.File;
                packet.Payload = obj.JsonSerialized();
                await client.SendMessage(packet);
            }
        }
    }
}