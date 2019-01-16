﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class NetworkManager : IManager
    {
        public void Init()
        {

        }

        /// <summary>
        /// 关闭并清理网络管理器。
        /// </summary>
        public void Dispose()
        {
            IEnumerator iter = m_NetworkChannels.GetEnumerator();
            while (iter.MoveNext())
            {
                NetworkChannel nc = iter.Current as NetworkChannel;
                nc.NetworkChannelConnected -= OnNetworkChannelConnected;
                nc.NetworkChannelClosed -= OnNetworkChannelClosed;
                nc.NetworkChannelMissHeartBeat -= OnNetworkChannelMissHeartBeat;
                nc.NetworkChannelError -= OnNetworkChannelError;
                nc.Destroy();
            }
            m_NetworkChannels.Clear();
        }


        private readonly Dictionary<string, NetworkChannel> m_NetworkChannels;

        private Action<NetworkChannel, object> m_NetworkConnectedEventHandler;
        private Action<NetworkChannel> m_NetworkClosedEventHandler;
        private Action<NetworkChannel, int> m_NetworkMissHeartBeatEventHandler;
        private Action<NetworkChannel, NetworkErrorCode, string> m_NetworkErrorEventHandler;

        /// <summary>
        /// 初始化网络管理器的新实例。
        /// </summary>
        public NetworkManager()
        {
            m_NetworkChannels = new Dictionary<string, NetworkChannel>();

            m_NetworkConnectedEventHandler = null;
            m_NetworkClosedEventHandler = null;
            m_NetworkMissHeartBeatEventHandler = null;
            m_NetworkErrorEventHandler = null;
        }

        /// <summary>
        /// 获取网络频道数量。
        /// </summary>
        public int NetworkChannelCount
        {
            get
            {
                return m_NetworkChannels.Count;
            }
        }

        /// <summary>
        /// 网络连接成功事件。
        /// </summary>
        public event Action<NetworkChannel, object> NetworkConnected
        {
            add
            {
                m_NetworkConnectedEventHandler += value;
            }
            remove
            {
                m_NetworkConnectedEventHandler -= value;
            }
        }

        /// <summary>
        /// 网络连接关闭事件。
        /// </summary>
        public event Action<NetworkChannel> NetworkClosed
        {
            add
            {
                m_NetworkClosedEventHandler += value;
            }
            remove
            {
                m_NetworkClosedEventHandler -= value;
            }
        }

        /// <summary>
        /// 网络心跳包丢失事件。
        /// </summary>
        public event Action<NetworkChannel, int> NetworkMissHeartBeat
        {
            add
            {
                m_NetworkMissHeartBeatEventHandler += value;
            }
            remove
            {
                m_NetworkMissHeartBeatEventHandler -= value;
            }
        }

        /// <summary>
        /// 网络错误事件。
        /// </summary>
        public event Action<NetworkChannel, NetworkErrorCode, string> NetworkError
        {
            add
            {
                m_NetworkErrorEventHandler += value;
            }
            remove
            {
                m_NetworkErrorEventHandler -= value;
            }
        }

        /// <summary>
        /// 网络管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (KeyValuePair<string, NetworkChannel> networkChannel in m_NetworkChannels)
            {
                networkChannel.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 检查是否存在网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>是否存在网络频道。</returns>
        public bool HasNetworkChannel(string name)
        {
            return m_NetworkChannels.ContainsKey(name ?? string.Empty);
        }

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>要获取的网络频道。</returns>
        public INetworkChannel GetNetworkChannel(string name)
        {
            NetworkChannel networkChannel = null;
            if (m_NetworkChannels.TryGetValue(name ?? string.Empty, out networkChannel))
            {
                return networkChannel;
            }

            return null;
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <returns>所有网络频道。</returns>
        public INetworkChannel[] GetAllNetworkChannels()
        {
            int index = 0;
            INetworkChannel[] results = new INetworkChannel[m_NetworkChannels.Count];
            foreach (KeyValuePair<string, NetworkChannel> networkChannel in m_NetworkChannels)
            {
                results[index++] = networkChannel.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <param name="results">所有网络频道。</param>
        public void GetAllNetworkChannels(List<INetworkChannel> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, NetworkChannel> networkChannel in m_NetworkChannels)
            {
                results.Add(networkChannel.Value);
            }
        }

        /// <summary>
        /// 创建网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <param name="networkChannelHelper">网络频道辅助器。</param>
        /// <returns>要创建的网络频道。</returns>
        public INetworkChannel CreateNetworkChannel(string name)
        {
            if (HasNetworkChannel(name))
            {
                throw new Exception(string.Format("Already exist network channel '{0}'.", name ?? string.Empty));
            }

            NetworkChannel networkChannel = new NetworkChannel(name);
            networkChannel.NetworkChannelConnected += OnNetworkChannelConnected;
            networkChannel.NetworkChannelClosed += OnNetworkChannelClosed;
            networkChannel.NetworkChannelMissHeartBeat += OnNetworkChannelMissHeartBeat;
            networkChannel.NetworkChannelError += OnNetworkChannelError;
            m_NetworkChannels.Add(name, networkChannel);
            return networkChannel;
        }

        /// <summary>
        /// 销毁网络频道。
        /// </summary>
        /// <param name="name">网络频道名称。</param>
        /// <returns>是否销毁网络频道成功。</returns>
        public bool DestroyNetworkChannel(string name)
        {
            NetworkChannel networkChannel = null;
            if (m_NetworkChannels.TryGetValue(name ?? string.Empty, out networkChannel))
            {
                networkChannel.NetworkChannelConnected -= OnNetworkChannelConnected;
                networkChannel.NetworkChannelClosed -= OnNetworkChannelClosed;
                networkChannel.NetworkChannelMissHeartBeat -= OnNetworkChannelMissHeartBeat;
                networkChannel.NetworkChannelError -= OnNetworkChannelError;
                networkChannel.Destroy();
                return m_NetworkChannels.Remove(name);
            }

            return false;
        }

        private void OnNetworkChannelConnected(NetworkChannel networkChannel, object userData)
        {
            if (m_NetworkConnectedEventHandler != null)
            {
                lock (m_NetworkConnectedEventHandler)
                {
                    m_NetworkConnectedEventHandler(networkChannel, userData);
                }
            }
        }

        private void OnNetworkChannelClosed(NetworkChannel networkChannel)
        {
            if (m_NetworkClosedEventHandler != null)
            {
                lock (m_NetworkClosedEventHandler)
                {
                    m_NetworkClosedEventHandler(networkChannel);
                }
            }
        }

        private void OnNetworkChannelMissHeartBeat(NetworkChannel networkChannel, int missHeartBeatCount)
        {
            if (m_NetworkMissHeartBeatEventHandler != null)
            {
                lock (m_NetworkMissHeartBeatEventHandler)
                {
                    m_NetworkMissHeartBeatEventHandler(networkChannel, missHeartBeatCount);
                }
            }
        }

        private void OnNetworkChannelError(NetworkChannel networkChannel, NetworkErrorCode errorCode, string errorMessage)
        {
            Debug.LogErrorFormat("ErrorCode:{0}.{1}", errorCode, errorMessage);
            //if (m_NetworkErrorEventHandler != null)
            //{
            //    lock (m_NetworkErrorEventHandler)
            //    {
            //        m_NetworkErrorEventHandler(networkChannel, errorCode, errorMessage);
            //    }
            //}
        }
    }
}


