﻿using System.IO;

namespace Game
{
    /// <summary>
    /// 网络消息包头接口
    /// |Type|Length|Msg
    /// </summary>
    public class NetworkChannelHelper
    {
        public virtual int HeaderLength
        {
            get { return 8; }
        }

        /// <summary>
        /// 获取网络消息包长度。
        /// </summary>
        public int MsgLength
        {
            get;
            private set;
        }
        /// <summary>
        /// 网络消息类型
        /// </summary>
        public int MsgType
        {
            get;
            private set;
        }

        public NetworkChannelHelper() { }

        public virtual void DecodeHeader(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            MsgType = reader.ReadInt32();
            MsgLength = reader.ReadInt32();
        }
        /// <summary>
        /// 打包协议
        /// </summary>
        public virtual Protocol Decode(MemoryStream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            Protocol packet = new Protocol();
            packet.WriteInt(MsgType);
            packet.WriteBytes(reader.ReadBytes(MsgLength));
            return packet;
        }
    }
}
