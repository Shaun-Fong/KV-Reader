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

    private bool m_LoadNewFile;
    private bool m_IsDirty;
    private bool IsDirty
    {
        get
        {
            return m_IsDirty;
        }
        set
        {
            m_IsDirty = value;
            this.titleContent = new GUIContent("KV Reader" + (m_IsDirty ? " *" : ""));
        }
    }

    public string LastSavePath
    {
        get
        {
            return EditorPrefs.GetString(Application.companyName + "_" + Application.productName + "_LastSavePath", "");
        }
        set
        {
            EditorPrefs.SetString(Application.companyName + "_" + Application.productName + "_LastSavePath", value);
        }
    }

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
        IsDirty = false;
        m_LoadNewFile = false;

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
                if (textField.userData == null)
                    return;

                var i = (int)textField.userData;

                if (m_Reader.Data[i].Value != evt.newValue && m_LoadNewFile == false)
                {
                    IsDirty = true;
                }

                m_Reader.Data[i].Key = evt.newValue;
            });
            IsDirty = true;
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
            IsDirty = true;
        };

        Func<VisualElement> VALUE_MakeCell = () =>
        {
            var textField = new TextField();
            textField.RegisterValueChangedCallback(evt =>
            {
                if (textField.userData == null)
                    return;

                var i = (int)textField.userData;

                if (m_Reader.Data[i].Value != evt.newValue && m_LoadNewFile == false)
                {
                    IsDirty = true;
                }

                m_Reader.Data[i].Value = evt.newValue;
            });
            IsDirty = true;
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
            IsDirty = true;
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

    private void OnDestroy()
    {
        if (IsDirty == true)
        {
            if (EditorUtility.DisplayDialog("Save file", "File is changed, do you want to save before quit?", "YES", "NO"))
            {
                IsDirty = false;
                SaveFile();
            }
        }
    }

    private void OnGUI()
    {
        if (m_LoadNewFile == true)
        {
            m_LoadNewFile = false;
        }
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
        void newFile()
        {
            m_Reader.Data.Clear();
            listview.RefreshItems();

            m_SavePath = "";
            RefreshPath();
        }

        if (IsDirty)
        {
            if (EditorUtility.DisplayDialog("Create new file", "File will not be save, are you sure to create new file?", "YES", "NO"))
            {
                IsDirty = false;
                newFile();
            }
        }
        else
        {
            newFile();
        }
    }

    private void OpenFile()
    {
        void open()
        {
            m_LoadNewFile = true;
            string filePath = EditorUtility.OpenFilePanel("Open File", LastSavePath, "");
            LoadFile(filePath);
            IsDirty = false;
        }

        if (IsDirty == true)
        {
            if (EditorUtility.DisplayDialog("Open file", "File is changed, are you sure to load a new file without save?", "YES", "NO"))
            {
                open();
            }
        }
        else
        {
            open();
        }
    }

    private void LoadFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) == true || File.Exists(filePath) == false)
        {
            return;
        }

        LastSavePath = Path.GetDirectoryName(filePath);

        Debug.Log($"Loaded at '{filePath}'.");
        m_Reader.ParseFromPath(filePath);
        listview.RefreshItems();

        m_SavePath = filePath;
        RefreshPath();
        IsDirty = false;
    }

    private void SaveFile()
    {
        void save()
        {
            while (string.IsNullOrEmpty(m_SavePath))
            {
                m_SavePath = EditorUtility.SaveFilePanel("Save File", LastSavePath, "new file", "txt");
                LastSavePath = Path.GetDirectoryName(m_SavePath);
            }

            string content = m_Reader.ParseToString();

            File.WriteAllText(m_SavePath, content);
            Debug.Log($"Saved at '{m_SavePath}'.");

            listview.RefreshItems();
            RefreshPath();
            AssetDatabase.Refresh();
            IsDirty = false;
        }

        if (IsDirty == true)
        {
            if (EditorUtility.DisplayDialog("Save file", "Are you sure to save?", "YES", "NO"))
            {
                save();
            }
        }
        else
        {
            save();
        }
    }
}
