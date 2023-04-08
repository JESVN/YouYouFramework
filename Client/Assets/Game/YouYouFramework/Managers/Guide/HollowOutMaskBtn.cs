using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;


[RequireComponent(typeof(Button))]
public class HollowOutMaskBtn : HollowOutMask
{
    private Button button;

    [Header("ǿ�ƹۿ�ʱ��")]
    [SerializeField] float DelayTime;

    protected override void Awake()
    {
        base.Awake();
        IsAcross = false;
        button = GetComponent<Button>();
        button.targetGraphic = this;
        button.onClick.AddListener(() =>
        {
            //������һ������
            GameEntry.Guide.NextGroup(GameEntry.Guide.CurrentState);
        });
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        //ǿ����ҿ�һ���
        if (DelayTime > 0)
        {
            button.enabled = false;
            GameEntry.Time.Create(delayTime: DelayTime, onStar: () =>
            {
                button.enabled = true;
            });
        }
    }
}
