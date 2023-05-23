using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class RoleInfo
{
    public RoleView CurrRole { get; private set; }

    public int CurrHP;
    public int MaxHP;

    public List<RoleInfoSkill> SkillList = new List<RoleInfoSkill>();

    public LinkedList<RoleInfoSkill> AttackList = new LinkedList<RoleInfoSkill>();

    //���һ���ͷŵ��չ�
    public LinkedListNode<RoleInfoSkill> CurrAttack;

    /// <summary>
    /// ��ʼ����ǰ��ɫ��Ϣ
    /// </summary>
    public void InitCurrPlayerInfo(RoleView roleCtrl, int maxHP)
    {
        CurrRole = roleCtrl;

        MaxHP = maxHP;
        CurrHP = MaxHP;
    }

    internal int GetCanUsedSkillId()
    {
        //���Ȼ�ȡ����ID
        for (int i = 0; i < SkillList.Count; i++)
        {
            if (SkillList[i].IsActive)
            {
                return SkillList[i].SkillId;
            }
        }

        //����IDû����, �����չ�ID
        return GetCanUsedAttackId();
    }
    internal int GetCanUsedAttackId()
    {
        //2���������ͷ��չ���Ϊ�չ�����
        if (CurrAttack.Previous != null && Time.time - CurrAttack.Previous.Value.SkillCDBegTime > 3)
        {
            CurrAttack = AttackList.First;
        }
        return CurrAttack.Value.SkillId;
    }

    public void BegSkillCD(int skillId)
    {
        RoleInfoSkill roleInfoSkill = SkillList.Find(x => x.SkillId == skillId);
        if (roleInfoSkill == null)
        {
            for (LinkedListNode<RoleInfoSkill> node = AttackList.First; node != null; node = node.Next)
            {
                if (node.Value.SkillId == skillId) roleInfoSkill = node.Value;
            }
        }
        //roleInfoSkill.BegSkillCD();

        if (skillId == CurrAttack.Value.SkillId)
        {
            CurrAttack = CurrAttack.Next;
            if (CurrAttack == null) CurrAttack = AttackList.First;
        }
    }
}

public class RoleInfoSkill
{
    //���ܱ��
    public int SkillId;

    //�������һ�ο�ʼ�ͷŵ�ʱ��
    public float SkillCDBegTime { get; private set; }
    //����CD����ʱ��
    public float SkillCDEndTime { get; private set; }

    //��ǰ�����Ƿ����,  ������ȴ�Ƿ���� && MP�Ƿ��㹻
    public bool IsActive { get { return Time.time > SkillCDEndTime; } }//&& CurrMP >= SkillList[i].SpendMP

    //private SkillEntity SkillEntity;


    //public void BegSkillCD()
    //{
    //    if (SkillEntity == null) SkillEntity = GameEntry.DataTable.SkillDBModel.GetDic(SkillId);

    //    SkillCDBegTime = Time.time;
    //    SkillCDEndTime = SkillCDBegTime + SkillEntity.CDTime;
    //}
}
