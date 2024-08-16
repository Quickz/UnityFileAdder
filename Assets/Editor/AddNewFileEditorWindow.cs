using System.IO;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace UnityFileAdder
{
    public class AddNewFileEditorWindow : EditorWindow
    {
        public string path;
        private static Vector2 _size = new(500, 75);
        private string _input;
        private bool _initialFocusTriggered;

        [MenuItem("Custom/Add New File %E")]
        public static void AddNewFile()
        {
            var window = GetWindow<AddNewFileEditorWindow>();
            window.titleContent = new GUIContent("Add New File");
            window.minSize = _size;
            window.maxSize = _size;
            window.path = GetActiveFolderPath();
            window.Center();
        }

        public void OnGUI()
        {
            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
            {
                Close();
            }
            else if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
            {
                TrySubmit();
            }

            const int paddingValue = 15;

            var padding = new RectOffset(paddingValue, paddingValue, paddingValue, paddingValue);
            var area = new Rect(
                padding.right,
                padding.top,
                position.width - (padding.right + padding.left),
                position.height - (padding.top + padding.bottom));

            GUILayout.BeginArea(area);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{path}/", GUILayout.ExpandWidth(false));
        
            GUI.SetNextControlName("Input");
            _input = GUILayout.TextField(_input);

            if (!_initialFocusTriggered)
            {
                GUI.FocusControl("Input");
                _initialFocusTriggered = true;
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                TrySubmit();
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void TrySubmit()
        {
            if (string.IsNullOrWhiteSpace(_input))
            {
                return;
            }

            var finalPath = Path.Combine(path, _input);
            Debug.Log($"Final path: {finalPath}");

            var directoryPath = Path.GetDirectoryName(finalPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(finalPath));
            }

            if (!finalPath.EndsWith("\\") &&
                !finalPath.EndsWith("/") &&
                !finalPath.EndsWith(Path.PathSeparator))
            {
                // TODO: Fix issue
                // Submitting Blablabla/Something.cs won't work if Blablabla directory doesn't already exist

                if (File.Exists(finalPath))
                {
                    Debug.LogError($"File already exists at {finalPath}");
                    return;
                }

                if (Directory.Exists(finalPath))
                {
                    Debug.LogError($"Unable to create a file. Name reserved by a directory at {finalPath}");
                    return;
                }

                // TODO:
                // Introduce file templates.
                // A few ideas:
                // Default MonoBehaviour script.
                // C# Interface
                File.WriteAllText(finalPath, "");

                // TODO:
                // See if you can auto-focus the newly created asset
            }

            AssetDatabase.Refresh();

            Close();
        }

        private static string GetActiveFolderPath()
        {
            var getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod(
                "GetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic);
            return getActiveFolderPath.Invoke(null, null) as string;
        }
    }
}
