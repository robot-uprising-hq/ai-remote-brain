using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Barracuda;
using Unity.MLAgents.Policies;

// This script 
public class BrainFileAutoMount : MonoBehaviour
{
    public RemoteAgent m_RemoteAgent;
    public string m_BehaviourName = "PushBlock";
    public string m_BrainFileFolderName = "Assets/BrainFileToUse";

    public Text m_WarningText;
    // public string m_BrainFileName = "BrainInUse.nn";
    
    string m_OverrideExtension = "nn";
    Dictionary<string, string> m_BehaviorNameOverrides = new Dictionary<string, string>();
    Dictionary<string, NNModel> m_CachedModels = new Dictionary<string, NNModel>();

    void Awake()
    {
        DirectoryInfo dir = new DirectoryInfo( m_BrainFileFolderName );
        FileInfo[] files = dir.GetFiles( "*.nn" );

        if (files.Length > 1)
        {
            m_WarningText.text = "FATAL ERROR: \nFound more than one brain file in folder " + m_BrainFileFolderName;
        }

        else if (files.Length == 0)
        {
            m_WarningText.text = "FATAL ERROR: \nCould not find a brain file in folder " + m_BrainFileFolderName;
        }

        m_BehaviorNameOverrides.Clear();
        m_BehaviorNameOverrides[m_BehaviourName] = m_BrainFileFolderName + "/" + files[0].Name;

        NNModel nnModel = GetModelForBehaviorName(m_BehaviourName);
        if (nnModel != null)
        {
            Debug.Log("On file");
        }

        var name = GetOverrideBehaviorName(m_BehaviourName);
        Debug.Log("name: " + name);

        m_RemoteAgent.LazyInitialize();
        // Need to give the sensors some data before setting up a new model
        // because the process of setting a new model reads the sensors once.
        float[] lowerObservations = new float[155];
        float[] upperObservations = new float[155];
        m_RemoteAgent.SetObservations(lowerObservations, upperObservations);
        m_RemoteAgent.SetModel(name, nnModel);

        m_WarningText.text = "Brain file '" + files[0].Name + "' found and taken into use";
    }

    // Source https://github.com/Unity-Technologies/ml-agents/blob/release_6/Project/Assets/ML-Agents/Examples/SharedAssets/Scripts/ModelOverrider.cs
    private static string GetOverrideBehaviorName(string originalBehaviorName)
    {
        return $"Override_{originalBehaviorName}";
    }

    // Source https://github.com/Unity-Technologies/ml-agents/blob/release_6/Project/Assets/ML-Agents/Examples/SharedAssets/Scripts/ModelOverrider.cs
    private NNModel GetModelForBehaviorName(string behaviorName)
    {
        if (m_CachedModels.ContainsKey(behaviorName))
        {
            return m_CachedModels[behaviorName];
        }

        string assetPath = null;
        if (m_BehaviorNameOverrides.ContainsKey(behaviorName))
        {
            assetPath = m_BehaviorNameOverrides[behaviorName];
        }
        else if(!string.IsNullOrEmpty(m_BrainFileFolderName))
        {
            assetPath = Path.Combine(m_BrainFileFolderName, $"{behaviorName}.{m_OverrideExtension}");
        }

        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.Log($"No override for BehaviorName {behaviorName}, and no directory set.");
            return null;
        }

        byte[] model = null;
        try
        {
            model = File.ReadAllBytes(assetPath);
        }
        catch(IOException)
        {
            Debug.Log($"Couldn't load file {assetPath} at full path {Path.GetFullPath(assetPath)}", this);
            // Cache the null so we don't repeatedly try to load a missing file
            m_CachedModels[behaviorName] = null;
            return null;
        }

        // Note - this approach doesn't work for onnx files. Need to replace with
        // the equivalent of ONNXModelImporter.OnImportAsset()
        var asset = ScriptableObject.CreateInstance<NNModel>();
        asset.modelData = ScriptableObject.CreateInstance<NNModelData>();
        asset.modelData.Value = model;

        asset.name = "Override - " + Path.GetFileName(assetPath);
        m_CachedModels[behaviorName] = asset;
        return asset;
    }
}
