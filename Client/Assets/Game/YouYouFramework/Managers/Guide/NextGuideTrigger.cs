using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

/// <summary>
/// ��������, ��һ��������
/// </summary>
public class NextGuideTrigger : MonoBehaviour
{
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
                //��������, ������һ��
                GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);

                TriggerEnter?.Invoke();
            }
        }
    }
}
