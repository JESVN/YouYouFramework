using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;
public class NextGuideTrigger : MonoBehaviour
{
    //public string nextGuide;

    /// <summary>
    /// ʲô���ֽ����ᴥ��
    /// </summary>
    public string[] triggerNames;

    public Action TriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        foreach (string name in triggerNames)
        {
            if (name == other.name)
            {
                //GuideState state = nextGuide.ToEnum<GuideState>();
                //bool isNext = GameEntry.Guide.NextGroup(state);
                //if (!isNext)
                //{
                //    Debug.LogError("������һ��ʧ��, ���ö��==" + state);
                //}

                //������һ������
                GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);

                TriggerEnter?.Invoke();
            }
        }
    }


}
