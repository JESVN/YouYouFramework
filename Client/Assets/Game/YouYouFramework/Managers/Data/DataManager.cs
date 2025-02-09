using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 数据组件
    /// </summary>
    public class DataManager 
    {
        public GuideDataMgr GuideDataMgr { get; private set; }
        public PlayerPrefsDataMgr PlayerPrefsDataMgr { get; private set; }
        internal DataManager()
        {
            GuideDataMgr = new GuideDataMgr();
            PlayerPrefsDataMgr = new PlayerPrefsDataMgr();
        }
        public void Init()
        {
            GuideDataMgr.Init();
            PlayerPrefsDataMgr.Init();
        }
        public void SaveDataAll()
        {
            GuideDataMgr.SaveDataAll();
            PlayerPrefsDataMgr.SaveDataAll();
        }
    }
}