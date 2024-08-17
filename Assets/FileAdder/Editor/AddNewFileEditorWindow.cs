using System.IO;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace UnityFileAdder
{
    public class AddNewFileEditorWindow : EditorWindow
    {
        public string currentFolderPath;
        private static Vector2 _size = new(500, 75);
        private string _input;
        private bool _initialFocusTriggered;

        [MenuItem("Window/Add New File %E")]
        public static void AddNewFile()
        {
            var window = GetWindow<AddNewFileEditorWindow>();
            window.titleContent = new GUIContent("Add New File");
            window.minSize = _size;
            window.maxSize = _size;
            window.currentFolderPath = GetActiveFolderPath();
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

            var currentFolder = Path.GetFileName(currentFolderPath);
            GUILayout.Label(currentFolder + PathHelper.PathSeparator, GUILayout.ExpandWidth(false));
        
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

            var assetPath = Path.Combine(currentFolderPath, _input);
            var directoryPath = Path.GetDirectoryName(assetPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            }

            var isFilePath = PathHelper.IsFilePath(assetPath);

            if (isFilePath)
            {
                if (File.Exists(assetPath))
                {
                    Debug.LogError($"File already exists at {assetPath}");
                    return;
                }

                if (Directory.Exists(assetPath))
                {
                    Debug.LogError($"Unable to create a file. Name reserved by a directory at {assetPath}");
                    return;
                }

                var fileName = Path.GetFileName(assetPath);
                var assetContent = GetTemplate(fileName);
                File.WriteAllText(assetPath, assetContent);
            }

            AssetDatabase.Refresh();

            if (!isFilePath)
            {
                // Necessary for focusing on a folder to work
                assetPath = PathHelper.TrimPathSeparators(assetPath);
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            ProjectWindowUtil.ShowCreatedAsset(asset);

            Close();
        }

        private static string GetTemplate(string fileNameWithExtension)
        {
            var format = Path.GetExtension(fileNameWithExtension).Trim('.');
            var fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension);
            const string TemplateFolder = "Assets/FileAdder/Editor/Templates/";
            var templateLocation = Path.Combine(TemplateFolder, $"{format}.txt");

            if (!File.Exists(templateLocation))
            {
                return "";
            }

            var template = File.ReadAllText(templateLocation);
            template = template.Replace("{fileName}", fileName);

            return template;
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
