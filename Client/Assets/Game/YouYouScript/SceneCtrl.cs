using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class SceneCtrl : SingletonMono<SceneCtrl>
{
    void Start()
    {
        GameEntry.Log(LogCategory.ZhangSan, "�ĳ�è��ţ��");
        FormDialog formDialog = GameEntry.UI.OpenUIForm<FormDialog>();
        formDialog.SetUI("����ڲ�����ȫ���������, �Ѿ������¼����", "��¼����");
    }
}
