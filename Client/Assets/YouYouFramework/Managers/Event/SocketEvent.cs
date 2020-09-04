using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// Socket�¼�
    /// </summary>
    public class SocketEvent : IDisposable
    {
        /// <summary>
        /// ����ԭ��       ���о������ⶼ���Ը�ԭ��Ϊί������
        /// </summary>
        /// <param name="P">�����������</param>
        public delegate void OnActionHandler(byte[] buffer);
        /// <summary>
        /// Key: �����б��Key   ͬ���б�������ⶼ��ͬһ��Key
        /// Value: �����б�
        /// </summary>
        private Dictionary<ushort, LinkedList<OnActionHandler>> dic = new Dictionary<ushort, LinkedList<OnActionHandler>>();

        #region AddEventListener �۲��߼����¼�
        /// <summary> 
        /// �۲��߼����¼�
        /// </summary>
        /// <param name="Key">�����б��Key</param>
        /// <param name="handler">����</param>
        public void AddEventListener(ushort key, OnActionHandler handler)
        {
            LinkedList<OnActionHandler> lstHandler = null;
            dic.TryGetValue(key, out lstHandler);
            if (lstHandler == null)
            {
                lstHandler = new LinkedList<OnActionHandler>();
                dic[key] = lstHandler;
            }
            lstHandler.AddLast(handler);
        }
        #endregion

        #region RemoveEventListener �۲����Ƴ������¼�
        /// <summary>
        /// �۲����Ƴ������¼�
        /// </summary>
        /// <param name="key">�����б��Key</param>
        /// <param name="handler">����</param>
        public void RemoveEventListener(ushort key, OnActionHandler handler)
        {
            LinkedList<OnActionHandler> lstHandler = null;
            dic.TryGetValue(key, out lstHandler);
            if (lstHandler != null)
            {
                lstHandler.Remove(handler);
                if (lstHandler.Count == 0)
                {
                    dic.Remove(key);
                }
            }
        }
        #endregion

        #region Dispatch �������ɷ��¼�
        /// <summary>
        /// �������ɷ��¼�
        /// </summary>
        /// <param name="btnKey">�����б��Key</param>
        /// <param name="buffer">�������</param>
        public void Dispatch(ushort key, byte[] buffer)
        {
            LinkedList<OnActionHandler> lstHandler = null;
            dic.TryGetValue(key, out lstHandler);

            if (lstHandler != null && lstHandler.Count > 0)
            {
                for (LinkedListNode<OnActionHandler> curr = lstHandler.First; curr != null; curr = curr.Next)
                {
                    curr.Value?.Invoke(buffer);
                }
            }
        }
        public void Dispatch(ushort key)
        {
            Dispatch(key, null);
        }
        #endregion

        public void Dispose()
        {
            dic.Clear();
        }
    }
}