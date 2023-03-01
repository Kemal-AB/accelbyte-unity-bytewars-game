using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

public class AssetNameEnumGenerator : AssetModificationProcessor
{
    private const string EnumPath = "/Scripts/AssetManager/AssetEnum.cs";
    private const string AssetFolder = "Assets/Resources/"+AssetManager.ModuleFolder;
    private const string MetaExtension = ".meta";
    static string[] OnWillSaveAssets(string[] paths)
    {
        List<string> newAssetNames = new List<string>();
        foreach (string path in paths)
        {
            //don't process meta file updates
            if (path.EndsWith(MetaExtension))
                return paths;
            if (path.StartsWith(AssetFolder))
            {
                newAssetNames.Add(Path.GetFileNameWithoutExtension(path));
                break;
            }
        }

        if (newAssetNames.Count==0) return paths;
        UpdateAssetEnum(newAssetNames);
        return paths;
    }

    private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
    {
        try
        {
            if (String.IsNullOrEmpty(Path.GetExtension(assetPath)))
            {
                Directory.Delete(assetPath, true);
                File.Delete(assetPath+MetaExtension);
            }
            else
            {
                File.Delete(assetPath);
                File.Delete(assetPath + MetaExtension);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        if (assetPath.StartsWith(AssetFolder))
        {
            UpdateAssetEnum();
        }
        return AssetDeleteResult.DidDelete;
    }

    private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
    {
        if (String.IsNullOrEmpty(Path.GetExtension(sourcePath)))
        {
            Directory.Move(sourcePath, destinationPath);
            File.Move(sourcePath+MetaExtension, destinationPath+MetaExtension);
            return AssetMoveResult.DidMove;
        }
        File.Move(sourcePath, destinationPath);
        File.Move(sourcePath+MetaExtension, destinationPath+MetaExtension);
        bool isDestinationAssetFolder = destinationPath.StartsWith(AssetFolder);
        bool isSourceAssetFolder = sourcePath.StartsWith(AssetFolder);
        if ((isDestinationAssetFolder && !isSourceAssetFolder) ||
            (isSourceAssetFolder && !isDestinationAssetFolder)  )
        {
            //Debug.Log("update on move");
            UpdateAssetEnum();
        }
        return AssetMoveResult.DidMove;
    }

    private static void UpdateAssetEnum(List<string> newAssetNames = null)
    {
        List<String> enumNames = new List<string>();
        if (newAssetNames != null)
        {
            foreach (var newAssetName in newAssetNames)
            {
                enumNames.Add(newAssetName);
            }
        }
        String[] assetNames = Directory.GetFiles(AssetFolder, "*", SearchOption.AllDirectories);
        foreach (var file in assetNames)
        {
            string fullFileName = Path.GetFileName(file);
            if (!fullFileName.EndsWith(MetaExtension))
            {
                string fileName = Path.GetFileNameWithoutExtension(fullFileName);
                if (enumNames.Contains(fileName))
                {
                    Debug.Log("Aseet name: "+fileName+" already exist");
                    continue;
                }
                else
                {
                    enumNames.Add(fileName);
                }
            }
        }
        string enumScript = @"//auto generated from AssetNameEnumGenerator
public enum AssetEnum 
{
";
        foreach (var enumName in enumNames)
        {
            enumScript += "      " + enumName + ",\n";
        }
        enumScript += "}";
        try
        {
            using var fs = File.Create(Application.dataPath + EnumPath);
            var content = new UTF8Encoding(true).GetBytes(enumScript);
            fs.Write(content, 0, content.Length);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
#endif