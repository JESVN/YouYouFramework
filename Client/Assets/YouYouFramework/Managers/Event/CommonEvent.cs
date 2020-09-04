using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// ͨ���¼�
    /// </summary>
    public class CommonEvent : IDisposable
    {
        public delegate void OnActionHandler(object userData);
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

        #region Dispatch �ɷ�
        /// <summary>
        /// �ɷ�
        /// </summary>
        /// <param name="key"></param>
        /// <param name="p"></param>
        public void Dispatch(ushort key, object userData)
        {
            LinkedList<OnActionHandler> lstHandler = null;
            dic.TryGetValue(key, out lstHandler);

            if (lstHandler != null && lstHandler.Count > 0)
            {
                for (LinkedListNode<OnActionHandler> curr = lstHandler.First; curr != null; curr = curr.Next)
                {
                    curr.Value?.Invoke(userData);
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