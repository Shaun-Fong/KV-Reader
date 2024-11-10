using KVReader;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class KVEditorWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private Button btn_new;
    private Button btn_open;
    private Button btn_save;
    private Button btn_path;
    private MultiColumnListView listview;

    private KVReader.Reader m_Reader;
    private string m_SavePath;

    [MenuItem("Tools/KV Reader")]
    public static void ShowWindow()
    {
        KVEditorWindow instance = CreateWindow<KVEditorWindow>();
        instance.titleContent = new GUIContent("KV Reader");
        instance.Show(true);
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        VisualElement instance = m_VisualTreeAsset.Instantiate();
        m_Reader = new KVReader.Reader();

        btn_new = instance.Q<Button>("btn_new");
        btn_open = instance.Q<Button>("btn_open");
        btn_save = instance.Q<Button>("btn_save");
        btn_path = instance.Q<Button>("btn_path");
        listview = instance.Q<MultiColumnListView>("listview");

        Func<VisualElement> KEY_MakeCell = () =>
        {
            var textField = new TextField();
            textField.RegisterValueChangedCallback(evt =>
            {
                var i = (int)textField.userData;
                m_Reader.Data[i].Key = evt.newValue;
            });
            return textField;
        };

        Action<VisualElement, int> KEY_BindCell = (VisualElement element, int index) =>
        {
            if (m_Reader.Data[index] == null)
            {
                m_Reader.Data[index] = new KVData("", "");
            }

            TextField textField = element as TextField;

            textField.value = m_Reader.Data[index].Key;
            textField.userData = index;
        };

        Func<VisualElement> VALUE_MakeCell = () =>
        {
            var textField = new TextField();
            textField.RegisterValueChangedCallback(evt =>
            {
                var i = (int)textField.userData;
                m_Reader.Data[i].Value = evt.newValue;
            });
            return textField;
        };

        Action<VisualElement, int> VALUE_BindCell = (VisualElement element, int index) =>
        {
            if (m_Reader.Data[index] == null)
            {
                m_Reader.Data[index] = new KVData("", "");
            }

            TextField textField = element as TextField;

            textField.value = m_Reader.Data[index].Value;
            textField.userData = index;
        };

        listview.columns["KEY"].makeCell = KEY_MakeCell;
        listview.columns["KEY"].bindCell = KEY_BindCell;
        listview.columns["VALUE"].makeCell = VALUE_MakeCell;
        listview.columns["VALUE"].bindCell = VALUE_BindCell;

        listview.itemsSource = m_Reader.Data;

        btn_new.clicked += NewFile;
        btn_open.clicked += OpenFile;
        btn_save.clicked += SaveFile;
        RefreshPath();

        if (string.IsNullOrEmpty(m_SavePath) == false)
        {
            LoadFile(m_SavePath);
        }

        root.Add(instance);
    }

    private void RefreshPath()
    {
        if (string.IsNullOrEmpty(m_SavePath))
        {
            btn_path.text = "new file";
        }
        else
        {
            btn_path.text = m_SavePath;
        }
    }

    private void NewFile()
    {
        m_Reader.Data.Clear();
        listview.RefreshItems();

        m_SavePath = "";
        RefreshPath();
    }

    private void OpenFile()
    {
        string filePath = EditorUtility.OpenFilePanel("Open File", Application.dataPath, "");
        LoadFile(filePath);
    }

    private void LoadFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) == true || File.Exists(filePath) == false)
        {
            return;
        }

        m_Reader.ParseFromPath(filePath);
        listview.RefreshItems();

        m_SavePath = filePath;
        RefreshPath();
    }

    private void SaveFile()
    {
        while (string.IsNullOrEmpty(m_SavePath))
        {
            m_SavePath = EditorUtility.SaveFilePanel("Save File", Application.dataPath, "new file", "txt");
        }

        string content = m_Reader.ParseToString();

        File.WriteAllText(m_SavePath, content);

        listview.RefreshItems();
        RefreshPath();
        AssetDatabase.Refresh();
    }
}
