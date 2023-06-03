using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using YouYou;


public class RoleView : MonoBehaviour
{
    /// <summary>
    /// �Զ��嶯�����
    /// </summary>
    public RoleAnimCompoent AnimCompoent { get; private set; }
    /// <summary>
    /// �Զ��嵼�����
    /// </summary>
    public RoleAgentCompoent Agent { get; private set; }

    protected virtual void OnDestroy()
    {
    }
    protected virtual void Awake()
    {
        Agent = GetComponent<RoleAgentCompoent>();

        //��ʼ������ϵͳ
        AnimCompoent = GetComponent<RoleAnimCompoent>();
        AnimCompoent.InitAnim(GetComponentInChildren<Animator>());

    }
}
