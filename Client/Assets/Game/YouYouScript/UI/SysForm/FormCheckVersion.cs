using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;
using Main;

/// <summary>
/// 这个界面脚本不做热更, 所以不继承UIFormBase
/// </summary>
public class FormCheckVersion : MonoBehaviour
{
    [SerializeField]
    private Text txtTip;
    //[SerializeField]
    //private Text txtSize;
    //[SerializeField]
    //private Text txtVersion;

    [SerializeField]
    private Scrollbar scrollbar;

    private void OnDestroy()
    {
        MainEntry.ResourceManager.CheckVersionBeginDownload -= OnCheckVersionBeginDownload;
        MainEntry.ResourceManager.CheckVersionDownloadUpdate -= OnCheckVersionDownloadUpdate;
        MainEntry.ResourceManager.CheckVersionDownloadComplete -= OnCheckVersionDownloadComplete;

        MainEntry.Data.ActionPreloadBegin -= OnPreloadBegin;
        MainEntry.Data.ActionPreloadUpdate -= OnPreloadUpdate;
        MainEntry.Data.ActionPreloadComplete -= OnPreloadComplete;
    }
    private void Start()
    {
        MainEntry.ResourceManager.CheckVersionBeginDownload += OnCheckVersionBeginDownload;
        MainEntry.ResourceManager.CheckVersionDownloadUpdate += OnCheckVersionDownloadUpdate;
        MainEntry.ResourceManager.CheckVersionDownloadComplete += OnCheckVersionDownloadComplete;


        MainEntry.Data.ActionPreloadBegin += OnPreloadBegin;
        MainEntry.Data.ActionPreloadUpdate += OnPreloadUpdate;
        MainEntry.Data.ActionPreloadComplete += OnPreloadComplete;

        //if (txtSize != null) txtSize.gameObject.SetActive(false);
    }

    #region 检查更新进度
    private void OnCheckVersionBeginDownload()
    {
        //if (txtSize != null) txtSize.gameObject.SetActive(true);

        //txtVersion.text = string.Format("最新版本 {0}", GameEntry.Resource.ResourceManager.CDNVersion);
    }
    private void OnCheckVersionDownloadUpdate(BaseParams baseParams)
    {
        txtTip.text = string.Format("正在下载{0}/{1}", baseParams.IntParam1, baseParams.IntParam2);
        //if (txtSize != null) txtSize.text = string.Format("{0:f2}M/{1:f2}M", (float)baseParams.ULongParam1 / (1024 * 1024), (float)baseParams.ULongParam2 / (1024 * 1024));

        scrollbar.size = (float)baseParams.IntParam1 / baseParams.IntParam2;
    }
    private void OnCheckVersionDownloadComplete()
    {
        //Debug.Log("检查更新下载完毕!!!");
    }
    #endregion

    #region 预加载进度
    private void OnPreloadComplete()
    {
        Destroy(gameObject);
    }
    private void OnPreloadUpdate(float baseParams)
    {
        txtTip.text = string.Format("正在加载资源{0:f0}%", baseParams * 100);

        scrollbar.size = baseParams;
    }
    private void OnPreloadBegin()
    {
        //if (txtSize != null) txtSize.gameObject.SetActive(false);
        //txtVersion.text = string.Format("资源版本号 {0}", GameEntry.Resource.ResourceManager.CDNVersion);
    }
    #endregion
}
