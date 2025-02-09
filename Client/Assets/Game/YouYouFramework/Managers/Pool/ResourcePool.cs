using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// 资源池
    /// </summary>
    public class ResourcePool
    {
#if UNITY_EDITOR
        /// <summary>
        /// 在监视面板显示的信息
        /// </summary>
        public Dictionary<string, AssetReferenceEntity> InspectorDic = new Dictionary<string, AssetReferenceEntity>();
#endif

        /// <summary>
        /// 资源池名称
        /// </summary>
        public string PoolName { get; private set; }

        /// <summary>
        /// 资源池字典
        /// </summary>
        private Dictionary<string, AssetReferenceEntity> m_ResourceDic;

        /// <summary>
        /// 需要移除的Key链表
        /// </summary>
        private LinkedList<string> m_NeedRemoveKeyList;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="poolName">资源池名称</param>
        public ResourcePool(string poolName)
        {
            PoolName = poolName;
            m_ResourceDic = new Dictionary<string, AssetReferenceEntity>();
            m_NeedRemoveKeyList = new LinkedList<string>();
        }

        /// <summary>
        /// 注册到资源池
        /// </summary>
        public void Register(AssetReferenceEntity entity)
        {
#if UNITY_EDITOR
            InspectorDic.Add(entity.ResourceName, entity);
#endif
            m_ResourceDic.Add(entity.ResourceName, entity);
        }

        /// <summary>
        /// 资源取池
        /// </summary>
        public AssetReferenceEntity Spawn(string resourceName)
        {
            if (m_ResourceDic.TryGetValue(resourceName, out AssetReferenceEntity referenceEntity))
            {
                referenceEntity.Spawn(false);
            }
            return referenceEntity;
        }

        /// <summary>
        /// 释放资源池中可释放资源
        /// </summary>
        public void Release()
        {
            var enumerator = m_ResourceDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetReferenceEntity referenceEntity = enumerator.Current.Value;
                if (referenceEntity.GetCanRelease())
                {
#if UNITY_EDITOR
                    InspectorDic.Remove(referenceEntity.ResourceName);
#endif
                    m_NeedRemoveKeyList.AddFirst(referenceEntity.ResourceName);
                    referenceEntity.Release();
                }
            }

            //循环链表 从字典中移除制定的Key
            LinkedListNode<string> curr = m_NeedRemoveKeyList.First;
            while (curr != null)
            {
                string key = curr.Value;
                m_ResourceDic.Remove(key);

                LinkedListNode<string> next = curr.Next;
                m_NeedRemoveKeyList.Remove(curr);
                curr = next;
            }
        }

        /// <summary>
        /// 释放池内所有资源
        /// </summary>
        public void ReleaseAll()
        {
            var enumerator = m_ResourceDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetReferenceEntity referenceEntity = enumerator.Current.Value;
#if UNITY_EDITOR
                InspectorDic.Remove(referenceEntity.ResourceName);
#endif
                m_NeedRemoveKeyList.AddFirst(referenceEntity.ResourceName);
                referenceEntity.Release();
            }

            //循环链表 从字典中移除制定的Key
            LinkedListNode<string> curr = m_NeedRemoveKeyList.First;
            while (curr != null)
            {
                string key = curr.Value;
                m_ResourceDic.Remove(key);

                LinkedListNode<string> next = curr.Next;
                m_NeedRemoveKeyList.Remove(curr);
                curr = next;
            }
        }
    }
}