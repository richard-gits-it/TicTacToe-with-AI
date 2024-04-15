using TCP280Project;

namespace ServerProject;

public partial class ServerForm : Form
{
    Server server = new Server(10000);
    public ServerForm()
    {
        InitializeComponent();
    }

    private async void btnStart_Click(object sender, EventArgs e)
    {
        //server.ServerMessageEvent += Server_OnMessageReceived;
        //server.LocalServerMessageEvent += Server_LocalServerMessageEvent;
        //Task serverTask = server.Start();
        //await serverTask;
    }

    private void Server_LocalServerMessageEvent(string message)
    {
        this.Invoke(() => lstMessages.Items.Add(message));
    }

    private void Server_OnMessageReceived(Packet280 packet)
    {
        this.Invoke(() => lstMessages.Items.Add(packet.Payload));
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        server.Stop();
    }

    private async void ServerForm_Load(object sender, EventArgs e)
    {
        server.ServerMessageEvent += Server_OnMessageReceived;
        server.LocalServerMessageEvent += Server_LocalServerMessageEvent;
        Task serverTask = server.Start();
        await serverTask;
    }

    private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        server.Stop();
    }
}