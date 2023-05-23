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

    ///// <summary>
    ///// Ѫ�������
    ///// </summary>
    //public Transform HeadBarPoint { get; private set; }

    ///// <summary>
    ///// ��ǰѪ��
    ///// </summary>
    //public UIGlobalHeadBarView HeadBarView { get; protected set; }

    ///// <summary>
    ///// ��ǰHUD
    ///// </summary>
    //[HideInInspector] public HUDText HUDText;


    protected virtual void OnDestroy()
    {
        //if (HeadBarView != null)
        //{
        //    GameEntry.Data.GlobalDataMgr.ReleaseHeadBarView(HeadBarView);
        //    HeadBarView = null;
        //}

        //if (HUDText != null)
        //{
        //    GameEntry.Data.GlobalDataMgr.ReleaseHudText(HUDText);
        //    HUDText = null;
        //}
    }
    protected virtual void Awake()
    {
        Agent = GetComponent<RoleAgentCompoent>();

        //��ʼ������ϵͳ
        AnimCompoent = GetComponent<RoleAnimCompoent>();
        AnimCompoent.InitAnim(GetComponentInChildren<Animator>());

    }
    //protected virtual void Update()
    //{
    //    if (HeadBarPoint == null) return;
    //    //�õ���Ļ����
    //    Vector2 screenPos = CameraCtrl.Instance.MainCamera.WorldToScreenPoint(HeadBarPoint.position);

    //    //���յ�UI��������
    //    Vector3 pos;

    //    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(GameEntry.Instance.UIRootRectTransform, screenPos, GameEntry.Instance.UICamera, out pos))
    //    {
    //        HeadBarView.transform.position = pos;
    //        HUDText.transform.position = pos + new Vector3(0, 50, 0);
    //    }
    //}
}
