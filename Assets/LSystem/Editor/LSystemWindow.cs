using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Sayama.LSystem.Editor
{
	public enum LSystemTab
	{
		Parameters,
		Generation
	}
	public class LSystemWindow : EditorWindow
	{
		private LSystemStringParams LSystemParams = LSystemStringParams.CreateDefault();

		private string NewKey = "";
		private string NewRule = "";

		private string Result = "";
		
		// =====

		private LSystemTab tab = LSystemTab.Parameters;
		private string[] tabStrings = Enum.GetNames(typeof(LSystemTab));

		private Vector2 paramScroll = Vector2.zero;
		private bool paramDisplayResult = true;

		private Vector2 genScroll = Vector2.zero;
		private bool genDisplayResult = false;

		[MenuItem("Sayama/LSystem")]
		public static void ShowWindow()
		{
			var window = EditorWindow.GetWindow(typeof(LSystemWindow));
		}

		private void Init()
		{
		}

		private bool showRules = false;
		private void OnGUI()
		{
			GUILayout.Label ("LSystem Creator", EditorStyles.boldLabel);

			tab = (LSystemTab)GUILayout.Toolbar((int)tab, tabStrings);

			switch (tab)
			{
				case LSystemTab.Parameters:
					DrawParameterGui();
					break;
				case LSystemTab.Generation:
					DrawGenerationGui();
					break;
			}
			
		}

		private void DrawParameterGui()
		{
			LSystemParams.InitialString = EditorGUILayout.DelayedTextField("Initial String", LSystemParams.InitialString).Trim();
			LSystemParams.IterationCount = Mathf.Max(EditorGUILayout.IntField("Iteration Count",LSystemParams.IterationCount), 1);
			LSystemParams.RotationDegrees = EditorGUILayout.Slider("Rotation", LSystemParams.RotationDegrees, -180, +180);

			//TODO: explain the '+' and '-' and '[' and ']' signs in editor
			
			#region Add Rules

			EditorGUILayout.BeginHorizontal();
			NewKey = EditorGUILayout.TextField("Key", NewKey);
			NewKey = NewKey.Trim();
			if (NewKey.Length > 1) NewKey = NewKey[..1];
			NewRule = EditorGUILayout.TextField("Rule", NewRule).Trim();
			bool canAddRule = NewKey.Length == 1 && !LSystemParams.Rules.ContainsKey(NewKey[0]);
			EditorGUI.BeginDisabledGroup(!canAddRule);
			if (GUILayout.Button("Add") && canAddRule)
			{
				LSystemParams.Rules.Add(NewKey[0], NewRule);
				NewKey = "";
				NewRule = "";
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			#endregion
			
			EditorGUILayout.Separator();

			#region Edit Rules

			// Show the rules
			showRules = EditorGUILayout.BeginFoldoutHeaderGroup(showRules, "Rules");
			if (showRules)
			{
				HashSet<char> toRemove = new HashSet<char>();
				Dictionary<char, string> toEdit = new Dictionary<char, string>();
				
				foreach (KeyValuePair<char,string> rule in LSystemParams.Rules)
				{
					EditorGUILayout.BeginHorizontal();

					string value = EditorGUILayout.TextField(rule.Key.ToString(), rule.Value).Trim();
					if (value != rule.Value)
					{
						toEdit.Add(rule.Key, value);
					}

					if (GUILayout.Button("Delete"))
					{
						if (EditorUtility.DisplayDialog($"Remove Key '{rule.Key}'",
								$"Do you really want to delete the Key '{rule.Key}' ?", "YES", "NO"))
						{
							toRemove.Add(rule.Key);
						}
					}
					
					EditorGUILayout.EndVertical();
				}

				foreach (var pair in toEdit)
				{
					LSystemParams.Rules[pair.Key] = pair.Value;
				}

				foreach (char key in toRemove)
				{
					LSystemParams.Rules.Remove(key);
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			#endregion
			
			EditorGUILayout.Separator();

			#region Results

			GenerateResult();
			ClearResult();
			CopyResult();

			DisplayResult(ref paramDisplayResult, ref paramScroll);

			#endregion

		}

		private void CopyResult()
		{
			EditorGUI.BeginDisabledGroup(Result.Length == 0);
			if (GUILayout.Button("Copy"))
			{
				GUIUtility.systemCopyBuffer = Result;
			}
			EditorGUI.EndDisabledGroup();
		}

		private void ClearResult()
		{
			if (GUILayout.Button("Clear Result"))
			{
				Result = "";
			}
		}

		private void GenerateResult()
		{
			EditorGUI.BeginDisabledGroup(LSystemParams.InitialString.Length == 0 || LSystemParams.Rules.Count == 0);
			if (GUILayout.Button("Generate String"))
			{
				Result = LSystemCreator.GenerateString(LSystemParams);
			}
			EditorGUI.EndDisabledGroup();
		}

		private void DisplayResult(ref bool displayResult, ref Vector2 scroll)
		{
			displayResult = EditorGUILayout.BeginFoldoutHeaderGroup(displayResult, "Result");
			if(displayResult)
			{
				scroll = EditorGUILayout.BeginScrollView(scroll, EditorStyles.textArea);
				EditorGUILayout.LabelField(Result, EditorStyles.wordWrappedLabel);
				EditorGUILayout.EndScrollView();
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}


		private Transform selection = null;

		private void DrawGenerationGui()
		{
			EditorGUILayout.ObjectField("Root Tree", selection, typeof(Transform), true);
            
			bool hasValidSelection = Selection.activeTransform != null;
			EditorGUI.BeginDisabledGroup(!hasValidSelection);
			if (GUILayout.Button("Use Selected"))
			{
				selection = Selection.activeTransform;
			}
			EditorGUI.EndDisabledGroup();
			
			EditorGUILayout.Separator();
	
			bool hasSelection = selection != null;

			EditorGUI.BeginDisabledGroup(!hasSelection || Result.Length == 0);
			if (GUILayout.Button("Generate L-System"))
			{
				LSystemCreator.GenerateLSystem(selection, LSystemParams);
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!hasSelection || selection.childCount == 0);
			if (GUILayout.Button("Clear L-System"))
			{
				ClearChildren(selection, false);
			}
			EditorGUI.EndDisabledGroup();
		}

		private void ClearChildren(Transform transform, bool clearSelf = true)
		{
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				Transform child = transform.GetChild(i);
				ClearChildren(child, true);
			}

			if (clearSelf)
			{
				Object.DestroyImmediate(transform.gameObject, false);
			}
		}
	}
}