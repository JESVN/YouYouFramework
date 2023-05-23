using Main;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class GuideDataMgr : DataMgrBase<GuideDataMgr.EventName>
{
    public enum EventName
    {

    }
    //����������浵
    private bool b_native = true;

    private GuideEntity GuideEntity;

    public void Init()
    {
        if (b_native)
        {
            GuideEntity = GameEntry.Data.PlayerPrefsDataMgr.GetObject<GuideEntity>("GuideEntity");
        }
        else
        {
            //������Ըĳ�����浵
        }
    }
    public void SaveDataAll()
    {
        if (b_native)
        {
            GameEntry.Data.PlayerPrefsDataMgr.SetObject("GuideEntity", GuideEntity);
        }
        else
        {
            //������Ըĳ�����浵
        }
    }

    public GuideState NextGuide { get { return GuideEntity.CurrGuide + 1; } }


    /// <summary>
    /// �������� ���1��ģ�� �浵
    /// </summary>
    public void GuideCompleteOne(GuideState guideState)
    {
        //ֻ�ܱ�����������
        if (guideState >= GuideEntity.CurrGuide + 1)
        {
            GuideEntity.CurrGuide = guideState;
            GameEntry.Log(LogCategory.Guide, "GuideCompleteOne:" + guideState.ToString() + guideState.ToInt());
        }
    }
}

public class GuideEntity
{
    //��ǰ����ɵ���������
    public GuideState CurrGuide;
}