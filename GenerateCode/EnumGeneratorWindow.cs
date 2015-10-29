using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace GenerateCode
{
    public class EnumGeneratorWindow : EditorWindow
    {
        enum UseValues
        {
            Custom,
            UnityLayers,
            UnityTags
        }

        [MenuItem("Window/Generate Code/Enum Generator")]
        public static void ShowWindow()
        {
            var window = GetWindow<EnumGeneratorWindow>();
            window.Init();
        }

        private string codeNamespace;
        private bool withFlagsAttribute;
        private string codeEnumName;
        private List<string> codeEnumValues;
        private string folder;

        private int codeEnumValuesCount;
        private UseValues useValues;
        private bool enumValuesFoldout;

        void Init()
        {
            codeNamespace = "";
            withFlagsAttribute = true;
            codeEnumName = "NewEnum";
            codeEnumValues = new List<string> {"Value1", "Value2"};
            folder = Application.dataPath;
            
            codeEnumValuesCount = 2;
            useValues = UseValues.UnityTags;
            enumValuesFoldout = false;
        }



        void OnGUI()
        {
            // title label
            GUILayout.Label("Enum Generator settings:", EditorStyles.boldLabel);

            
            // namespace textfield
            codeNamespace = EditorGUILayout.TextField("namespace", codeNamespace);

            EditorGUI.indentLevel++;
            // flags toggle 
            withFlagsAttribute = EditorGUILayout.Toggle("[Flags]", withFlagsAttribute);

            // enum name textfield
            codeEnumName = EditorGUILayout.TextField("enum", codeEnumName);
            EditorGUI.indentLevel++;

            EditorGUILayout.Space();
            // use which values? popup
            EditorGUILayout.BeginHorizontal();
            enumValuesFoldout = EditorGUILayout.Foldout(enumValuesFoldout, new GUIContent("values"));
            var useValuesEnumNames = Enum.GetNames(typeof (UseValues));
            useValues = (UseValues) GUILayout.SelectionGrid((int)useValues, useValuesEnumNames, useValuesEnumNames.Length);//.EnumPopup(new GUIContent(), useValues);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if (enumValuesFoldout)
            {

                EditorGUI.indentLevel++;

                switch (useValues)
                {
                    case UseValues.Custom:
                        // use custom list of strings

                        // the array size setting
                        EditorGUILayout.BeginHorizontal();
                        codeEnumValuesCount = EditorGUILayout.IntField("Size", codeEnumValuesCount);
                        if (GUILayout.Button("Resize", GUILayout.Width(80f)))
                        {
                            if (codeEnumValuesCount < codeEnumValues.Count)
                                codeEnumValues.RemoveRange(codeEnumValuesCount, codeEnumValues.Count - codeEnumValuesCount);
                            else if (codeEnumValuesCount > codeEnumValues.Count) {
                                if (codeEnumValuesCount > codeEnumValues.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
                                    codeEnumValues.Capacity = codeEnumValuesCount;
                                codeEnumValues.AddRange(Enumerable.Repeat("NewValue", codeEnumValuesCount - codeEnumValues.Count));
                            }

                        }
                        EditorGUILayout.EndHorizontal();
                        
                        
                        // show the array elements textfields
                        for (int i = 0; i < codeEnumValues.Count; i++)
                        {
                            int index = withFlagsAttribute ? (1 << i) : i;
                            codeEnumValues[i] = EditorGUILayout.TextField(index + ".", codeEnumValues[i]);
                        }

                        break;
                    case UseValues.UnityLayers:
                        // use unity layers
                        var layers = InternalEditorUtility.layers;

                        // show uneditable list
                        for (int i = 0; i < layers.Length; i++)
                        {
                            int index = withFlagsAttribute ? (1 << i) : i;
                            EditorGUILayout.LabelField(index + ".", layers[i]);
                        }
                        break;
                    case UseValues.UnityTags:
                    default:
                        // use unity tags
                        var tags = InternalEditorUtility.tags;

                        // show uneditable list
                        for (int i = 0; i < tags.Length; i++)
                        {
                            int index = withFlagsAttribute ? (1 << i) : i;
                            EditorGUILayout.LabelField(index + ".", tags[i]);
                        }
                        break;
                }

                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
            // title label
            GUILayout.Space(2);
            GUILayout.Label("Select output folder:", EditorStyles.boldLabel);

            // select output folder
            EditorGUILayout.BeginHorizontal();
            // folder path textfield
            folder = EditorGUILayout.TextField("", folder);

            // folder path browse...
            if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
            {
                folder = EditorUtility.OpenFolderPanel("Select output folder", folder, "");
            }
            EditorGUILayout.EndHorizontal();


            // save button
            GUILayout.Space(10f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", GUILayout.MaxWidth(80f)))
            {
                string[] enumValues = null;
                switch (useValues)
                {
                    case UseValues.Custom:
                        // saving custom values
                        enumValues = codeEnumValues.ToArray();
                        break;
                    case UseValues.UnityLayers:
                        // saving unity layers
                        enumValues = InternalEditorUtility.layers;
                        break;
                    case UseValues.UnityTags:
                    default:
                        // saving tags
                        enumValues = InternalEditorUtility.tags;
                        break;
                }
                // write .cs file
                Write(codeNamespace, codeEnumName, withFlagsAttribute, enumValues, folder);
                Close();
            }

            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(80f)))
            {
                Close();
            }
            EditorGUILayout.EndHorizontal();
        }


        public static void Write(string ns, string typeName, bool withFlagsAttribute, string[] fields, string folder = null)
        {
            // get full path
            var path = Path.GetFullPath(folder + Path.DirectorySeparatorChar + typeName + ".cs");

            // write to file
            using (var writer = new StreamWriter(File.Open(path, FileMode.Create)))
            {
                EnumGenerator.WriteEnum(writer, ns, typeName, fields, withFlagsAttribute, null, null);
            }
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
        }
    }

}