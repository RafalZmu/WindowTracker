using ChatServer;
using ChatServer.Net.IO;
using System;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
	class Program
	{
		static List<Client> _users;
		static TcpListener _listener;
		static void Main(string[] args)
		{
			_users = new List<Client>();
			_listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
			_listener.Start();

			while (true)
			{
				var client = new Client(_listener.AcceptTcpClient());
				_users.Add(client);
				BrodcastConnection();
				
			}
		}

		static void BrodcastConnection()
		{
			foreach (var user in _users)
			{
				foreach (var usr in _users)
				{
					var brodecastPacket = new PacketBuilder();
					brodecastPacket.WriteOpCode(1);
					brodecastPacket.WriteMessage(usr.Name);
					brodecastPacket.WriteMessage(usr.UID.ToString());
					user.ClientSocket.Client.Send(brodecastPacket.GetPacketBytes());

				}
			}
		}
		public static void BrodcastMessage(string message)
		{
			foreach (var user in _users)
			{
				var brodecastPacket = new PacketBuilder();
				brodecastPacket.WriteOpCode(5);
				brodecastPacket.WriteMessage(message);
				user.ClientSocket.Client.Send(brodecastPacket.GetPacketBytes());
			}
		}

        public static void BrodecastDisconnect(string uid)
		{
			var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
			_users.Remove(disconnectedUser);
			foreach (var user in _users)
			{
				var brodecastPacket = new PacketBuilder();
				brodecastPacket.WriteOpCode(10);
				brodecastPacket.WriteMessage(uid);
				user.ClientSocket.Client.Send(brodecastPacket.GetPacketBytes());
			};
		}
	}
}