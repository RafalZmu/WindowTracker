using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using WindowTracker.Net.IO;

namespace WindowTracker.Net
{
    class Server
    {
        public TcpClient client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectedEvent;
        public Server() 
        {
            client = new TcpClient();
        }

        public void ConnectToServer(string Name)
        {
            if (!client.Connected)
            {

                if(!string.IsNullOrEmpty(Name))
                {
                    client.Connect("127.0.0.1", 7891);
                    PacketReader = new PacketReader(client.GetStream());
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(Name);
                    client.Client.Send(connectPacket.GetPacketBytes());
                    ReadPackets();
                }
                else
                {
                    MessageBox.Show("Provide valid name");
                }

                
            }
            else
            {
                var connectPacket = new PacketBuilder();
                connectPacket.WriteOpCode(10);
                client.Client.Send(connectPacket.GetPacketBytes());
                client.Client.Close();
                client = new TcpClient();
                
            }
        }
        private void ReadPackets()

        {
            
            var task =  Task.Run(() =>
            {
                while (client.Connected)
                {
                    var opcode = PacketReader.ReadByte();
                    switch (opcode)
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;
                        case 5:
                            msgReceivedEvent?.Invoke();
                            break;
                        case 10:
                            userDisconnectedEvent?.Invoke();
                            break;
                        default:

                            break;
                    }
                }
            });

        }
        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(message);
            client.Client.Send(messagePacket.GetPacketBytes());
        }
    }
}
