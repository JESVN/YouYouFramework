using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using YouYou;

namespace YouYou
{
    [System.Serializable]
    public class PlayResourceEventArgs
    {
#if UNITY_EDITOR
        [OnValueChanged("OnCurrResourceChanged")]
        public GameObject CurrResource;
        private void OnCurrResourceChanged()
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(CurrResource);
            PrefabPath = path;
            PrefabName = CurrResource.name;
        }
#endif

        [Header("Ŀ���")]
        public DynamicTarget Target;

        [Header("Ԥ��·��")]
        public string PrefabPath;

        [Header("Ԥ������")]
        public string PrefabName;

        [Header("ƫ��")]
        public Vector3 Offset;

        [Header("��ת")]
        public Vector3 Rotation;

        [Header("����")]
        public Vector3 Scale = Vector3.one;

    }
    public class PlayResourcePlayable : BasePlayableAsset<PlayResourcePlayableBehaviour, PlayResourceEventArgs>
    {
    }
    public class PlayResourcePlayableBehaviour : BasePlayableBehaviour<PlayResourceEventArgs>
    {
        protected override void OnYouYouBehaviourPlay(Playable playable, FrameData info)
        {
            if (CurrArgs.Target == DynamicTarget.OurOne)
            {
                PoolObj poolObj = GameEntry.Pool.GameObjectPool.Spawn(CurrArgs.PrefabName);
                poolObj.transform.SetParent(CurrTimelineCtrl.RoleCtrl.transform);
                poolObj.transform.localPosition = CurrArgs.Offset;
                poolObj.transform.localEulerAngles = CurrArgs.Rotation;
                poolObj.transform.localScale = CurrArgs.Scale;
                poolObj.SetDelayTimeDespawn((float)End);
            }
        }

        protected override void OnYouYouBehaviourStop(Playable playable, FrameData info)
        {

        }
    }
}