using UnityEditor;
using UnityEngine;

namespace CodeSuggester
{
    public class CodeSuggesterWindow : EditorWindow
    {
        private static bool isInitialized;
        private Vector2 scrollPos;
        private Rect cachedPosition;
        private string displayText;
        private CodeSuggestionRequester requester;
        private string apiKey;
        private float? requestStartTime;

        private bool IsRequesting => requestStartTime.HasValue;

        [MenuItem("Window/CodeSuggester")]
        private static void ShowWindow()
        {
            isInitialized = false;
            var window = GetWindow<CodeSuggesterWindow>();
            window.titleContent = new GUIContent("CodeSuggester");
            window.Show();
        }

        private void OnGUI()
        {
            if (!isInitialized)
            {
                requester = new CodeSuggestionRequester(OnRequestCompleted);
                Selection.selectionChanged = OnSelectionChanged;
                requestStartTime = null;
                isInitialized = true;
            }

            cachedPosition = position;
            DrawApiKeyField();
            DrawTextArea();
            DrawButton();
        }

        private void OnSelectionChanged()
        {
            Repaint();
        }
        
        private void OnRequestCompleted(string response)
        {
            requestStartTime = null;
            SetTextArea(response);
        }

        private void DrawApiKeyField()
        {
            apiKey = EditorGUILayout.PasswordField("API key", apiKey);
        }
        
        private void DrawTextArea()
        {
            var style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
            var heightLayout = GUILayout.ExpandHeight(true);
            var widthLayout = GUILayout.MaxWidth(cachedPosition.width - 6);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.TextArea(displayText, style, heightLayout, widthLayout);
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawButton()
        {
            var selectedTextAsset = GetSelectedTextAsset();
            var isSelectedTextAsset = selectedTextAsset != null;
            var buttonText = IsRequesting
                ? "Requesting suggestion..."
                : isSelectedTextAsset
                    ? "Request suggestion"
                    : "Select script asset";
            var isButtonDisabled = !isSelectedTextAsset || IsRequesting;

            EditorGUI.BeginDisabledGroup(isButtonDisabled);
            if (GUILayout.Button(buttonText))
            {
                RequestCodeSuggestion();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void RequestCodeSuggestion()
        {
            requestStartTime = Time.realtimeSinceStartup;
            SetTextArea(string.Empty);
            requester.Request(apiKey, GetSelectedTextAsset().text);
        }

        private static TextAsset GetSelectedTextAsset()
        {
            var instanceId = Selection.activeInstanceID;
            var assetPath = AssetDatabase.GetAssetPath(instanceId);
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            return textAsset;
        }

        private void SetTextArea(string text)
        {
            displayText = text;
            Repaint();
        }
    }
}