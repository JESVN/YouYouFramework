using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace YouYou
{
/// <summary>
/// Assetʵ��
/// </summary>
public class AssetEntity
{
    /// <summary>
    /// ��Դ����
    /// </summary>
    public AssetCategory Category;

    /// <summary>
    /// ��Դ��������(·��)
    /// </summary>
    public string AssetFullName;

    /// <summary>
    /// ������Դ��(�����Դ����һ��Assetbundle��)
    /// </summary>
    public string AssetBundleName;
    
    /// <summary>
    /// ������Դ
    /// </summary>
    public List<AssetDependsEntity> DependsAssetList;
    }
}
