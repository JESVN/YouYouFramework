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
        /// ����ȡ��(reference==true�����ü���+1)
        /// </summary>
        public void Spawn(bool reference)
        {
            LastUseTime = Time.time;

            //�����������Դ�� ���ͷ�
            if (GameEntry.Pool.CheckAssetBundleIsLock(ResourceName))
            {
                ReferenceCount = 1;
            }
        }

        /// <summary>
        /// ����س�(reference==true�����ü���-1)
        /// </summary>
        public void Unspawn(bool reference)
        {
#if ASSETBUNDLE
            LastUseTime = Time.time;
#else
            Target = null;
            MainEntry.ClassObjectPool.Enqueue(this);
#endif
        }

        /// <summary>
        /// �����Ƿ�����ͷ�
        /// </summary>
        /// <returns></returns>
        public bool GetCanRelease()
        {
            return ReferenceCount == 0 && Time.time - LastUseTime > GameEntry.Pool.ReleaseAssetBundleInterval;
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