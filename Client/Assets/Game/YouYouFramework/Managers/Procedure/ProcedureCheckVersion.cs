using Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// ����������
    /// </summary>
    public class ProcedureCheckVersion : ProcedureBase
    {
        internal override void OnEnter()
        {
            base.OnEnter();
            MainEntry.ResourceManager.LocalAssetsManager.SetResourceVersion(null);//�����汾��, ����ֱ�Ӽ��MD5
            MainEntry.ResourceManager.CheckVersionComplete = () => GameEntry.Procedure.ChangeState(ProcedureState.Preload);
            MainEntry.ResourceManager.InitStreamingAssetsBundleInfo();

        }
    }
}