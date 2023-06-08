using Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class AssetBundleEntity
    {
        /// <summary>
        /// ��Դ����
        /// </summary>
        public string ResourceName;

        /// <summary>
        /// ����Ŀ��
        /// </summary>
        public AssetBundle Target;

        /// <summary>
        /// �ϴ�ʹ��ʱ��
        /// </summary>
        public float LastUseTime { get; private set; }

        /// <summary>
        /// ���ü���
        /// </summary>
        public int ReferenceCount { get; private set; }


        /// <summary>
        /// ����ȡ��
        /// </summary>
        public void Spawn()
        {
            LastUseTime = Time.time;

            //�����������Դ�� ���ͷ�
            if (GameEntry.Pool.CheckAssetBundleIsLock(ResourceName))
            {
                ReferenceCount = 1;
            }
        }

        /// <summary>
        /// ����س�
        /// </summary>
        public void Unspawn()
        {
            LastUseTime = Time.time;
        }

        /// <summary>
        /// �����Ƿ�����ͷ�
        /// </summary>
        /// <returns></returns>
        public bool GetCanRelease()
        {
            return ReferenceCount == 0 && Time.time - LastUseTime > GameEntry.Pool.ReleaseAssetBundleInterval;
        }

        public static AssetBundleEntity Create(string name, AssetBundle target)
        {
            AssetBundleEntity assetBundleEntity = MainEntry.ClassObjectPool.Dequeue<AssetBundleEntity>();
            assetBundleEntity.ResourceName = name;
            assetBundleEntity.Target = target;
            assetBundleEntity.Spawn();
            return assetBundleEntity;
        }
        /// <summary>
        /// �ͷ���Դ
        /// </summary>
        public void Release()
        {
            AssetBundle bundle = Target;
            bundle.Unload(false);

            ResourceName = null;
            ReferenceCount = 0;
            Target = null;

            MainEntry.ClassObjectPool.Enqueue(this); //�������Դʵ��س�
        }
    }
}