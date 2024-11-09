using KVReader;
using System;
using System.IO;
using Unity.Properties;
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

        listview.columns["KEY"].makeCell = () => new TextField();
        listview.columns["VALUE"].makeCell = () => new TextField();
        listview.columns["KEY"].bindCell = (VisualElement element, int index) =>
        {
            TextField textField = element as TextField;

            if (m_Reader.Data[index] == null)
            {
                m_Reader.Data[index] = new KVData("", "");
            }

            textField.value = m_Reader.Data[index].Key;
            textField.RegisterCallback((ChangeEvent<string> evt) =>
            {
                m_Reader.Data[index].Key = evt.newValue;
            });
        };
        listview.columns["VALUE"].bindCell = (VisualElement element, int index) =>
        {
            TextField textField = element as TextField;

            if (m_Reader.Data[index] == null)
            {
                m_Reader.Data[index] = new KVData("", "");
            }

            textField.value = m_Reader.Data[index].Value;
            textField.RegisterCallback((ChangeEvent<string> evt) =>
            {
                m_Reader.Data[index].Value = evt.newValue;
            });
        };
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
        if(string.IsNullOrEmpty(m_SavePath))
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
