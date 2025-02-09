using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;

public class FormHollow : UIFormBase
{
    private LinkedListNode<Transform> CurrGuide;
    private LinkedList<Transform> CurrGuides = new LinkedList<Transform>();
    private Transform itemParent;

    public GuideState GuideState { get; private set; }

    private static FormHollow formHollow;

    protected override void Awake()
    {
        base.Awake();
        foreach (Transform item in transform)
        {
            item.gameObject.SetActive(false);
        }
    }
    public static void ShowDialog()
    {
        if (formHollow == null || !formHollow.IsActive) formHollow = GameEntry.UI.OpenUIForm<FormHollow>();
        formHollow.SetUI();
    }
    private void SetUI()
    {
        if (GuideState != GameEntry.Guide.CurrentState)
        {
            GuideState = GameEntry.Guide.CurrentState;

            if (itemParent != null)
            {
                itemParent.gameObject.SetActive(false);
                itemParent = null;
            }

            CurrGuides.Clear();
            itemParent = transform.Find(GuideState.ToString());
            if (itemParent == null)
            {
                GameEntry.LogError(LogCategory.Guide, "itemParent==null, descGroup==" + GuideState);
            }
            itemParent.gameObject.SetActive(true);
            foreach (Transform item in itemParent)
            {
                item.gameObject.SetActive(false);
                CurrGuides.AddLast(item);
            }
            CurrGuide = CurrGuides.First;
            ShowGuide();
        }
        else
        {
            NextGroup();
        }
    }

    private void NextGroup()
    {
        CurrGuide.Value.gameObject.SetActive(false);
        CurrGuide = CurrGuide.Next;
        if (CurrGuide != null)
        {
            ShowGuide();
        }
    }

    private void ShowGuide()
    {
        GameEntry.Log(LogCategory.Guide, "Enter=={0}=={1}=={2}", GameEntry.Guide.CurrentState, CurrGuide.Value.gameObject.name, GameEntry.Guide.GuideGroup.TaskGroup.CurrCount + 1);
        CurrGuide.Value.gameObject.SetActive(true);
    }

}
