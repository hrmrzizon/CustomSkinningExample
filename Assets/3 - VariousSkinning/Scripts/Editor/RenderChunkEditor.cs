﻿namespace Example.VariousSkinning.Editor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Runtime.InteropServices;

    [CustomEditor(typeof(RenderChunk))]
    public class RenderChunkEditor : Editor
    {
        private RenderChunk targetAs { get { return target as RenderChunk; } }

        private string overrideTitle = "Override UnityEngine.Mesh to RenderChunk Data";
        private string addTitle = "Add UnityEngine.Mesh to RenderChunk Data";
        private string fillTitle = "Fill RenderChunk Data from UnityEngine.Mesh";

        private SerializedProperty meshProperty;

        private void OnEnable()
        {
            meshProperty = serializedObject.FindProperty("mesh");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RenderChunk chunk = targetAs;

            EditorGUILayout.PropertyField(meshProperty);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Origin data");
            EditorGUI.indentLevel++;

            if (chunk.mesh != null)
            {
                EditorGUILayout.LabelField("VertexCount", chunk.mesh.vertexCount.ToString());
                EditorGUILayout.LabelField("IndexCount", chunk.mesh.triangles.Length.ToString());
                EditorGUILayout.LabelField("BoneWeightCount", chunk.mesh.boneWeights != null ? chunk.mesh.boneWeights.Length.ToString() : "null");
            }
            else
            {
                EditorGUILayout.HelpBox("Mesh is null.. please insert data above", MessageType.Error);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Converted Data");
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("VertexCount", chunk.meshData != null? chunk.meshData.Length.ToString(): "null");

            EditorGUI.BeginDisabledGroup(chunk.meshData == null);

            if (GUILayout.Button("Clear"))
            {
                chunk.meshData = null;

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("IndexCount", chunk.indices != null ? chunk.indices.Length.ToString() : "null");

            EditorGUI.BeginDisabledGroup(chunk.indices == null);

            if (GUILayout.Button("Clear"))
            {
                chunk.indices = null;

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("BoneWeightCount", chunk.boneWeights != null ? chunk.boneWeights.Length.ToString() : "null");

            EditorGUI.BeginDisabledGroup(chunk.boneWeights == null);

            if (GUILayout.Button("Clear"))
            {
                chunk.boneWeights = null;

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(chunk.mesh == null);

            if (GUILayout.Button(addTitle))
            {
                AddData();

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button(overrideTitle))
            {
                OverrideData();

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button(fillTitle))
            {
                FillData();

                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUI.EndDisabledGroup();
        }

        public void AddData()
        {
            long time = DateTime.Now.Ticks;

            try
            {
                RenderChunk chunk = targetAs;

                Mesh mesh = chunk.mesh;

                mesh.RecalculateNormals();

                chunk.vertexCount = mesh.vertexCount;

                EditorUtility.DisplayProgressBar(addTitle, "Get Mesh Data", 0.25f);

                if (EditorUtility.DisplayDialog(addTitle, "override mesh.vertcies, mesh.triangles to RenderChunk.MeshData\nthis process has been long time, are sure this process?", "Yes", "No"))
                {
                    MeshDataInfo[] meshData = new MeshDataInfo[mesh.vertexCount];

                    // This code very long time
                    for (int i = 0; i < meshData.Length; i++)
                    {
                        meshData[i].position = mesh.vertices[i];
                        meshData[i].normal = mesh.normals[i];
                        meshData[i].uv = mesh.uv[i];
                    }

                    chunk.meshData = meshData;
                }

                EditorUtility.DisplayProgressBar(addTitle, "Get Index Data", 0.5f);

                {
                    chunk.indices = new int[mesh.triangles.Length];
                    Array.Copy(mesh.triangles, chunk.indices, chunk.indices.Length);
                }

                EditorUtility.DisplayProgressBar(addTitle, "Get Index Count Data", 0.75f);

                {
                    chunk.indexCounts = new uint[mesh.subMeshCount];
                    for (int i = 0; i < chunk.indexCounts.Length; i++)
                        chunk.indexCounts[i] = mesh.GetIndexStart(i) + mesh.GetIndexCount(i);
                }

                EditorUtility.DisplayProgressBar(addTitle, "Get boneWeight Data", 1f);

                if (EditorUtility.DisplayDialog(addTitle, "override mesh.boneWeights to RenderChunk.MeshData\nthis process has been long time, are sure this process?", "Yes", "No"))
                {
                    chunk.boneWeights = new CustomBoneWeight[mesh.boneWeights.Length];

                    for (int i = 0; i < mesh.boneWeights.Length; i++)
                        chunk.boneWeights[i] = new CustomBoneWeight(mesh.boneWeights[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                float measuredTime = (float)((DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond) / 1000;
                Debug.Log("Measured time : " + measuredTime);
            }

            EditorUtility.ClearProgressBar();
        }

        public void OverrideData()
        {
            long time = DateTime.Now.Ticks;

            try
            {
                RenderChunk chunk = targetAs;

                Mesh mesh = chunk.mesh;

                mesh.RecalculateNormals();

                chunk.vertexCount = mesh.vertexCount;

                EditorUtility.DisplayProgressBar(overrideTitle, "Get Mesh Data", 0.25f);

                {
                    MeshDataInfo[] meshData = new MeshDataInfo[mesh.vertexCount];

                    // This code very long time
                    for (int i = 0; i < meshData.Length; i++)
                    {
                        meshData[i].position = mesh.vertices[i];
                        meshData[i].normal = mesh.normals[i];
                        meshData[i].uv = mesh.uv[i];
                    }

                    chunk.meshData = meshData;
                }

                EditorUtility.DisplayProgressBar(overrideTitle, "Get Index Data", 0.5f);

                {
                    chunk.indices = new int[mesh.triangles.Length];
                    Array.Copy(mesh.triangles, chunk.indices, chunk.indices.Length);
                }

                EditorUtility.DisplayProgressBar(overrideTitle, "Get Index Count Data", 0.75f);

                {
                    chunk.indexCounts = new uint[mesh.subMeshCount];
                    for (int i = 0; i < chunk.indexCounts.Length; i++)
                        chunk.indexCounts[i] = mesh.GetIndexStart(i) + mesh.GetIndexCount(i);
                }

                EditorUtility.DisplayProgressBar(overrideTitle, "Get boneWeight Data", 1f);

                {
                    chunk.boneWeights = new CustomBoneWeight[mesh.boneWeights.Length];

                    for (int i = 0; i < mesh.boneWeights.Length; i++)
                        chunk.boneWeights[i] = new CustomBoneWeight(mesh.boneWeights[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                float measuredTime = (float)((DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond) / 1000;
                Debug.Log("Measured time : " + measuredTime);
            }

            EditorUtility.ClearProgressBar();
        }

        public void FillData()
        {
            long time = DateTime.Now.Ticks;

            try
            {
                RenderChunk chunk = targetAs;
                Mesh mesh = chunk.mesh;

                chunk.vertexCount = mesh.vertexCount;

                if (chunk.meshData == null || chunk.meshData.Length != mesh.vertexCount)
                    if (EditorUtility.DisplayDialog(fillTitle, "override mesh.vertcies, mesh.triangles to RenderChunk.MeshData\nthis process has been long time, are sure this process?", "Yes", "No"))
                    {
                        EditorUtility.DisplayProgressBar(fillTitle, "Get Mesh Data", 0.25f);

                        chunk.meshData = new MeshDataInfo[mesh.vertexCount];
                        for (int i = 0; i < chunk.meshData.Length; i++)
                            chunk.meshData[i] = new MeshDataInfo() { position = mesh.vertices[i], normal = mesh.normals[i], uv = mesh.uv[i] };
                    }

                if (chunk.indices == null || chunk.indices.Length != mesh.triangles.Length)
                {
                    EditorUtility.DisplayProgressBar(fillTitle, "Get Index Data", 0.5f);

                    chunk.indices = new int[mesh.triangles.Length];
                    Array.Copy(mesh.triangles, chunk.indices, chunk.indices.Length);
                }

                EditorUtility.DisplayProgressBar(fillTitle, "Get Index Count Data", 0.75f);

                chunk.indexCounts = new uint[mesh.subMeshCount];
                for (int i = 0; i < chunk.indexCounts.Length; i++)
                    chunk.indexCounts[i] = mesh.GetIndexCount(i);

                if (chunk.boneWeights == null || chunk.boneWeights.Length != mesh.boneWeights.Length)
                    if (EditorUtility.DisplayDialog(fillTitle, "override mesh.boneWeights to RenderChunk.MeshData\nthis process has been long time, are sure this process?", "Yes", "No"))
                    {
                        EditorUtility.DisplayProgressBar(overrideTitle, "Get boneWeight Data", 1f);

                        chunk.boneWeights = new CustomBoneWeight[mesh.boneWeights.Length];

                        for (int i = 0; i < mesh.boneWeights.Length; i++)
                            chunk.boneWeights[i] = new CustomBoneWeight(mesh.boneWeights[i]);
                    }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                float measuredTime = (float)((DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond) / 1000;
                Debug.Log("Measured time : " + measuredTime);
            }

            EditorUtility.ClearProgressBar();
        }
    }
}