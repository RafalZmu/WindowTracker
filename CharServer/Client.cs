using ChatServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    internal class Client
    {
        public string Name { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }
        PacketReader _packetReader;
        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = _packetReader.ReadByte();
            Name = _packetReader.ReadMessage();

            Console.WriteLine($"[{DateTime.Now}]: Client connected as {Name} ID:{UID}");

            Task.Run(() =>Process());
        }

        void Process()
        {
            while (true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    Console.WriteLine(opcode);

                    switch (opcode)
                    {
                        case 5:
                            var message = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}] {Name} Using: {message}");
                            Program.BrodcastMessage($"{UID}:{message}");
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{UID.ToString()}]: Disconnected");
                    Program.BrodecastDisconnect(UID.ToString());
                    ClientSocket.Close();
                    break;
                    
                }
            }
        }
    }
}
