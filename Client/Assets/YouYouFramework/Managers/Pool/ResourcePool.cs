using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// ��Դ��
    /// </summary>
    public class ResourcePool
    {
#if UNITY_EDITOR
        /// <summary>
        /// �ڼ��������ʾ����Ϣ
        /// </summary>
        public Dictionary<string, ResourceEntity> InspectorDic = new Dictionary<string, ResourceEntity>();
#endif

        /// <summary>
        /// ��Դ������
        /// </summary>
        public string PoolName { get; private set; }

        /// <summary>
        /// ��Դ���ֵ�
        /// </summary>
        private Dictionary<string, ResourceEntity> m_ResourceDic;
        /// <summary>
        /// ��Ҫ�Ƴ���Key����
        /// </summary>
        private LinkedList<string> m_NeedRemoveKeyList;

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="poolName">��Դ������</param>
        public ResourcePool(string poolName)
        {
            PoolName = poolName;
            m_ResourceDic = new Dictionary<string, ResourceEntity>();
            m_NeedRemoveKeyList = new LinkedList<string>();
        }

        /// <summary>
        /// ע�ᵽ��Դ��
        /// </summary>
        /// <param name="entity"></param>
        public void Register(ResourceEntity entity)
        {
            entity.Spawn();
#if UNITY_EDITOR
            InspectorDic[entity.ResourceName] = entity;
#endif
            m_ResourceDic.Add(entity.ResourceName, entity);
        }

        /// <summary>
        /// ��Դȡ��
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public ResourceEntity Spawn(string resourceName)
        {
            ResourceEntity resourceEntity = null;
            if (m_ResourceDic.TryGetValue(resourceName, out resourceEntity))
            {
                resourceEntity.Spawn();
#if UNITY_EDITOR
                if (InspectorDic.ContainsKey(resourceEntity.ResourceName))
                {
                    InspectorDic[resourceEntity.ResourceName] = resourceEntity;
                }
#endif
            }
            return resourceEntity;
        }

        /// <summary>
        /// ��Դ�س�
        /// </summary>
        /// <param name="resourceName"></param>
        public void Unspawn(string resourceName)
        {
            ResourceEntity resourceEntity = null;
            if (m_ResourceDic.TryGetValue(resourceName, out resourceEntity))
            {
                resourceEntity.Unspawn();
#if UNITY_EDITOR
                if (InspectorDic.ContainsKey(resourceEntity.ResourceName))
                {
                    InspectorDic[resourceEntity.ResourceName] = resourceEntity;
                }
#endif
            }
        }

        /// <summary>
        /// �ͷ���Դ���п��ͷ���Դ
        /// </summary>
        public void Release()
        {
            var enumerator = m_ResourceDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ResourceEntity resourceEntity = enumerator.Current.Value;
                if (resourceEntity.GetCanRelease())
                {
#if UNITY_EDITOR
                    if (InspectorDic.ContainsKey(resourceEntity.ResourceName))
                    {
                        InspectorDic.Remove(resourceEntity.ResourceName);
                    }
#endif
                    m_NeedRemoveKeyList.AddFirst(resourceEntity.ResourceName);
                    resourceEntity.Release();
                }
            }

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
        /// �ͷų���������Դ
        /// </summary>
        public void ReleaseAll()
        {
            var enumerator = m_ResourceDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ResourceEntity resourceEntity = enumerator.Current.Value;
                if (resourceEntity.GetCanRelease())
                {
#if UNITY_EDITOR
                    if (InspectorDic.ContainsKey(resourceEntity.ResourceName))
                    {
                        InspectorDic.Remove(resourceEntity.ResourceName);
                    }
#endif
                    m_NeedRemoveKeyList.AddFirst(resourceEntity.ResourceName);
                    resourceEntity.Release();
                }
            }

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