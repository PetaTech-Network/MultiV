﻿using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;

using Lidgren.Network;

namespace CoopServer
{
    internal class Util
    {
        public static (byte, byte[]) GetBytesFromObject(object obj)
        {
            return obj switch
            {
                byte _ => (0x01, BitConverter.GetBytes((byte)obj)),
                short _ => (0x02, BitConverter.GetBytes((short)obj)),
                ushort _ => (0x03, BitConverter.GetBytes((ushort)obj)),
                int _ => (0x04, BitConverter.GetBytes((int)obj)),
                uint _ => (0x05, BitConverter.GetBytes((uint)obj)),
                long _ => (0x06, BitConverter.GetBytes((long)obj)),
                ulong _ => (0x07, BitConverter.GetBytes((ulong)obj)),
                float _ => (0x08, BitConverter.GetBytes((float)obj)),
                bool _ => (0x09, BitConverter.GetBytes((bool)obj)),
                _ => (0x0, null),
            };
        }

        public static Client GetClientByNetHandle(long netHandle)
        {
            Client result = Server.Clients.Find(x => x.NetHandle == netHandle);
            if (result == null)
            {
                NetConnection localConn = Server.MainNetServer.Connections.Find(x => netHandle == x.RemoteUniqueIdentifier);
                if (localConn != null)
                {
                    localConn.Disconnect("No data found!");
                }
                return null;
            }

            return result;
        }

        public static NetConnection GetConnectionByUsername(string username)
        {
            Client client = Server.Clients.Find(x => x.Player.Username.ToLower() == username.ToLower());
            if (client == null)
            {
                return null;
            }

            return Server.MainNetServer.Connections.Find(x => x.RemoteUniqueIdentifier == client.NetHandle);
        }

        // Return a list of all connections but not the local connection
        public static List<NetConnection> FilterAllLocal(NetConnection local)
        {
            return new(Server.MainNetServer.Connections.Where(e => e != local));
        }
        public static List<NetConnection> FilterAllLocal(long local)
        {
            return new(Server.MainNetServer.Connections.Where(e => e.RemoteUniqueIdentifier != local));
        }

        // Return a list of players within range of ...
        public static List<NetConnection> GetAllInRange(LVector3 position, float range)
        {
            return new(Server.MainNetServer.Connections.FindAll(e =>
            {
                Client client = Server.Clients.First(x => x.NetHandle == e.RemoteUniqueIdentifier);
                return client != null && client.Player.IsInRangeOf(position, range);
            }));
        }
        // Return a list of players within range of ... but not the local one
        public static List<NetConnection> GetAllInRange(LVector3 position, float range, NetConnection local)
        {
            return new(Server.MainNetServer.Connections.Where(e =>
            {
                Client client = Server.Clients.First(x => x.NetHandle == e.RemoteUniqueIdentifier);
                return e != local && client != null && client.Player.IsInRangeOf(position, range);
            }));
        }

        public static T Read<T>(string file) where T : new()
        {
            XmlSerializer ser = new(typeof(T));

            XmlWriterSettings settings = new()
            {
                Indent = true,
                IndentChars = ("\t"),
                OmitXmlDeclaration = true
            };

            string path = AppContext.BaseDirectory + file;
            T data;

            if (File.Exists(path))
            {
                using (XmlReader stream = XmlReader.Create(path))
                {
                    data = (T)ser.Deserialize(stream);
                }

                using (XmlWriter stream = XmlWriter.Create(path, settings))
                {
                    ser.Serialize(stream, data);
                }
            }
            else
            {
                using (XmlWriter stream = XmlWriter.Create(path, settings))
                {
                    ser.Serialize(stream, data = new T());
                }
            }

            return data;
        }
    }
}
