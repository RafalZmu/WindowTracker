using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WindowTracker.Net.IO;

namespace WindowTracker.Net
{
    class Server
    {
        public TcpClient _client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectedEvent;
        public Server() 
        {
            _client= new TcpClient();
        }

        public void ConnectToServer(string Name)
        {
            if(!_client.Connected) 
            {

                if(!string.IsNullOrEmpty(Name))
                {
                    _client.Connect("127.0.0.1", 7891);
                    PacketReader = new PacketReader(_client.GetStream());
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(Name);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }
                else
                {
                    MessageBox.Show("Provide valid name");
                }
                ReadPackets();

                
            }
        }
        private void ReadPackets()

        {
            Task.Run(() =>
            {
                while (true)
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
            _client.Client.Send(messagePacket.GetPacketBytes());
        }
    }
}
