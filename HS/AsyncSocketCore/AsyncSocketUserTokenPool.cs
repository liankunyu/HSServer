using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace HS
{
    public class AsyncSocketUserTokenPool
    {
        private Stack<AsyncSocketUserToken> m_pool;

        public AsyncSocketUserTokenPool(int capacity)
        {
            m_pool = new Stack<AsyncSocketUserToken>(capacity);
        }

        public void Push(AsyncSocketUserToken item)
        {
            if (item == null)
            {
                throw new ArgumentException("Items added to a AsyncSocketUserToken cannot be null");
            }
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }

        public AsyncSocketUserToken Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }

        public int Count
        {
            get { return m_pool.Count; }
        }
    }

    public class AsyncSocketUserTokenList : Object
    {
        private List<AsyncSocketUserToken> m_list;

        public AsyncSocketUserTokenList()
        {
            m_list = new List<AsyncSocketUserToken>();
        }

        public void Add(AsyncSocketUserToken userToken)
        {
            lock(m_list)
            {
                m_list.Add(userToken);
            }
        }

        public void Remove(AsyncSocketUserToken userToken)
        {
            lock (m_list)
            {
                m_list.Remove(userToken);
            }
        }
        public bool ContainKey(int gateWay)
        {
            lock (m_list)
            {
                for (int i = 0; i < m_list.Count; i++)
                {
                    if (m_list[i].GateWayId == gateWay)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public AsyncSocketUserToken UseKey(int gateWay)
        {
            AsyncSocketUserToken UserToken = null;
            lock (m_list)
            {
                for (int i = 0; i < m_list.Count; i++)
                {
                    if (m_list[i].GateWayId == gateWay)
                    {
                        UserToken = m_list[i];
                    }
                }
                return UserToken;
            }
        }
        public void CopyList(ref AsyncSocketUserToken[] array)
        {
            lock (m_list)
            {
                array = new AsyncSocketUserToken[m_list.Count];
                m_list.CopyTo(array);
            }
        }
        public int Count()
        {
            return m_list.Count;
        }
        public Socket UserEndPoint(string ep)
        {
            lock (m_list)
            {
                for (int i = 0; i < m_list.Count; i++)
                {
                    if (m_list[i].ConnectSocket.RemoteEndPoint.ToString() == ep)
                    {
                        return m_list[i].ConnectSocket;
                    }
                }
                return null;
            }
        }
    }
}
