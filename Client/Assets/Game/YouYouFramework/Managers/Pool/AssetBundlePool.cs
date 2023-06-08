using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    public class AssetBundlePool : MonoBehaviour
    {
#if UNITY_EDITOR
        /// <summary>
        /// �ڼ��������ʾ����Ϣ
        /// </summary>
        public Dictionary<string, AssetBundleEntity> InspectorDic = new Dictionary<string, AssetBundleEntity>();
#endif

        /// <summary>
        /// ��Դ������
        /// </summary>
        public string PoolName { get; private set; }

        /// <summary>
        /// ��Դ���ֵ�
        /// </summary>
        private Dictionary<string, AssetBundleEntity> m_ResourceDic;

        /// <summary>
        /// ��Ҫ�Ƴ���Key����
        /// </summary>
        private LinkedList<string> m_NeedRemoveKeyList;

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="poolName">��Դ������</param>
        public AssetBundlePool(string poolName)
        {
            PoolName = poolName;
            m_ResourceDic = new Dictionary<string, AssetBundleEntity>();
            m_NeedRemoveKeyList = new LinkedList<string>();
        }

        /// <summary>
        /// ע�ᵽ��Դ��
        /// </summary>
        public void Register(AssetBundleEntity entity)
        {
#if UNITY_EDITOR
            InspectorDic.Add(entity.ResourceName, entity);
#endif
            m_ResourceDic.Add(entity.ResourceName, entity);
        }

        /// <summary>
        /// ��Դȡ��
        /// </summary>
        public AssetBundleEntity Spawn(string resourceName)
        {
            if (m_ResourceDic.TryGetValue(resourceName, out AssetBundleEntity resourceEntity))
            {
                resourceEntity.Spawn();
            }
            return resourceEntity;
        }

        /// <summary>
        /// �ͷ���Դ���п��ͷ���Դ
        /// </summary>
        public void Release()
        {
            var enumerator = m_ResourceDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetBundleEntity resourceEntity = enumerator.Current.Value;
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

            //ѭ������ ���ֵ����Ƴ��ƶ���Key
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
                AssetBundleEntity resourceEntity = enumerator.Current.Value;
#if UNITY_EDITOR
                if (InspectorDic.ContainsKey(resourceEntity.ResourceName))
                {
                    InspectorDic.Remove(resourceEntity.ResourceName);
                }
#endif
                m_NeedRemoveKeyList.AddFirst(resourceEntity.ResourceName);
                resourceEntity.Release();
            }

            //ѭ������ ���ֵ����Ƴ��ƶ���Key
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