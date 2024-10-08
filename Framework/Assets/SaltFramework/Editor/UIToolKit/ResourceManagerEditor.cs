using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceManagerEditor : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    public TreeView treeView;
    private ListView contentListView;
    private VisualTreeAsset itemTemplate;
    private List<string> SceneListsoureData;
    private List<string> soureData;
    private List<VisualElement> UilistVeData;
    private VisualElement lastUIVe;
    private VisualElement currentUIVe;
    private string lastTreeVe;
    private string currentTreeVe;
    private string selcectUIname;
    private Label titleLabel;
    private Label countLabel;
    private double lastClickTime = 0;
    private const double DoubleClickThreshold = 0.3d;
    private string currentTree;
    private string currentItemName;

    private const string uiPath = "Assets/Resources/UI";
    private const string modelPath = "Assets/Resources/Prefabs/Model";
    private const string npcPath = "Assets/Resources/Prefabs/Npc";
    private const string effectPath = "Assets/Resources/Audio/Effect";
    private const string musicPath = "Assets/Resources/Audio/Music";
    private const string configJsonPath = "Assets/Resources/Config/Table";
    private const string configExcelPath = "D:/GameProject/Client/All-Doe-s-Life-Res/All-Doe-s-Life-Tool/ExcelTool/ExcelConfig";

    [MenuItem("▷ SaltFramework/资源管理器")]
    public static void ShowExample()
    {
        ResourceManagerEditor wnd = GetWindow<ResourceManagerEditor>();
        wnd.titleContent = new GUIContent("资源管理器");

       /* Rect _rect = new Rect(0, 0, 500, 500);
        ResourceManagerEditor window = (ResourceManagerEditor)EditorWindow.GetWindowWithRect(typeof(ResourceManagerEditor), _rect, true, "Window2 name");
        window.Show();*/
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        // 加载模板文件
        itemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UIToolKit/UIItemEditor.uxml");
        root.Add(labelFromUXML);
        treeView = root.Q<TreeView>("TreeView");
        var refreshBtn = root.Q<Button>("refreshBtn");
        refreshBtn.clicked += () =>
        {
            ReLoadRes();
        };
        titleLabel = root.Q<Label>("title");
        countLabel = root.Q<Label>("count");

        treeView.SetRootItems(treeRoots);
        treeView.makeItem = MakeItem();
        treeView.bindItem = BindTreeItem(); 
        treeView.selectionChanged += OnTreeSelect();
        contentListView = root.Q<ListView>("contentListView");
    }

    private Action<VisualElement, int> BindTreeItem()
    {
        return (VisualElement element, int index) =>
        {
            (element as Label).text = treeView.GetItemDataForIndex<IPlanetOrGroup>(index).name;
        };
    }

    private Action<IEnumerable<int>> OnListSelcetChange()
    {
        return (val) =>
        {
            foreach (var index in val)
            {
                if (currentTree == "Scene")
                {
                    var data = SceneListsoureData[index];
                    if (data != null)
                    {
                        ListOnclick(index, data);
                    }
                }
                else 
                {
                    var data = soureData[index];
                    if (data != null)
                    {
                        ListOnclick(index, data);
                    }
                }
               
            }
        };
    }

    public void ReLoadRes()
    {
        if (contentListView == null)
            return;
        RecycleTreeData();
        if (currentTree == "Scene")
            LoadSceneRes();
        else
            LoadRes();
    }


    public void LoadSceneRes()
    {
        SceneListsoureData = new List<string>();
        foreach (string sceneGuid in AssetDatabase.FindAssets("t:Scene", new string[] { "Assets" }))
        {
            string sceneFilename = AssetDatabase.GUIDToAssetPath(sceneGuid);
            SceneListsoureData.Add(sceneFilename);
        }
        contentListView.makeItem += MakeUIListItem();
        contentListView.bindItem += BindSceneListItem();
        contentListView.itemsSource = SceneListsoureData;
        contentListView.selectedIndicesChanged += OnListSelcetChange();
        contentListView.RegisterCallback<MouseDownEvent>(OnDoubleClick);
    }

    public void LoadRes()
    {
        string[] prefabPaths = new string[0];
        if (currentTree == "UI")
            prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { uiPath });
        if (currentTree == "模型")
            prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] {modelPath});
        else if (currentTree == "人物")
            prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { npcPath });
        else if(currentTree == "音效")
            prefabPaths = AssetDatabase.FindAssets("t:AudioClip", new[] { effectPath });
        else if (currentTree == "音乐")
            prefabPaths = AssetDatabase.FindAssets("t:AudioClip", new[] { musicPath });
        else if (currentTree == "Json")
            prefabPaths = AssetDatabase.FindAssets("t:TextAsset", new[] { configJsonPath });
        else if (currentTree == "Excel")
            prefabPaths = Directory.GetFiles(configExcelPath, "*.xlsx");
        soureData = new List<string>();
        string prefabName = "";
        foreach (string path in prefabPaths)
        {
            if (currentTree == "音乐" || currentTree == "音效" || currentTree == "Json")
            {
                var temp = AssetDatabase.GUIDToAssetPath(path);
                prefabName = Application.dataPath + "/" + temp.Remove(0, "Assets/".Length);
            }
            else if (currentTree == "Excel")
            {
                prefabName = path;
            }
            else
            {
                prefabName = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(path)).name;
            }
            soureData.Add(prefabName);
        }
        contentListView.makeItem += MakeUIListItem();
        contentListView.bindItem += BindListItem();
        contentListView.itemsSource = soureData;
        contentListView.selectedIndicesChanged += OnListSelcetChange();
        contentListView.RegisterCallback<MouseDownEvent>(OnDoubleClick);
    }

    private void OnDoubleClick(MouseDownEvent evt)
    {
        float currentTime = Time.realtimeSinceStartup;

        if (currentTime - lastClickTime < DoubleClickThreshold)
        {
            if (currentTree == "Scene")
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(currentItemName);
            }
            else
            {
                var path = "";
                if (currentTree == "UI")
                    path = uiPath + "/" + currentItemName + ".prefab";
                else if (currentTree == "模型")
                    path = modelPath + "/" + currentItemName + ".prefab";
                else if (currentTree == "人物")
                    path = npcPath + "/" + currentItemName + ".prefab";
                else if (currentTree == "Json" ||currentTree == "Excel")
                {
                    path = currentItemName;
                    if (!string.IsNullOrEmpty(path))
                        System.Diagnostics.Process.Start(path);
                    return;
                }
                else if (currentTree == "音效")
                {
                    path = currentItemName;
                    if (!string.IsNullOrEmpty(path))
                        System.Diagnostics.Process.Start(path);
                    return;
                }
                else if (currentTree == "音乐")
                {
                    path = currentItemName;
                    if (!string.IsNullOrEmpty(path))
                        System.Diagnostics.Process.Start(path);
                    return;
                }
                   
                OpenPerfet(path);
            }
        }
        lastClickTime = currentTime;
    }

    public void ListOnclick(int index,string name)
    {
        if (lastUIVe != null)
        {
            var last = lastUIVe.Q<VisualElement>("menu");
            last.style.display = DisplayStyle.None;
        }
        var ve = UilistVeData[index];
        if (currentTree == "Scene")
        {
            var pathname = SceneListsoureData[index];
            currentItemName = pathname;
        }
        else {
            var pathname = soureData[index];
            currentItemName = pathname;
        }
        
        var menuItem = ve.Q<VisualElement>("menu");
        menuItem.style.display = DisplayStyle.Flex;

        currentUIVe = ve;
        lastUIVe = currentUIVe;

    }

    private Action ListItemOpenClick(string name)
    {
        return () =>
        {
            if (currentTree == "Scene")
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(currentItemName);
            }
            else if (currentTree == "音乐" || currentTree == "音效" || currentTree == "Json" || currentTree == "Excel")
            {
                var path = currentItemName;
                if (!string.IsNullOrEmpty(path))
                    System.Diagnostics.Process.Start(path);
            }
            else
            {
                var perfetpath = "";
                Transform parent = null;
                if (currentTree == "模型")
                {
                    perfetpath = modelPath + "/" + name + ".prefab";
                    parent = GameObject.Find("MapConfig/SceneModel").transform;
                }
                else if (currentTree == "人物")
                {
                    perfetpath = npcPath + "/" + name + ".prefab";
                    parent = GameObject.Find("MapConfig/Npc").transform;
                }
                else if (currentTree == "UI")
                {
                    perfetpath = uiPath + "/" + name + ".prefab";
                    parent = GameObject.Find("Canvas").transform;
                }
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(perfetpath);
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
                Selection.activeGameObject = instance;
            }
            
        };
    }

    public void OpenPerfet(string path)
    {
        if (path == "")
            return;
        PrefabStage prefabStage = PrefabStageUtility.OpenPrefab(path);
        if (prefabStage != null)
        {
            // 成功打开 Prefab，可以在这里执行其他逻辑
            //Debug.Log("Prefab 已成功打开编辑器模式：" + path);
            DebugEX.LogFrameworkMsg("打开预制体：" + path);
        }
        else
        {
            // 打开 Prefab 失败
            Debug.LogWarning("无法打开 Prefab 编辑器模式：" + path);
        }
    }

    private Func<VisualElement> MakeUIListItem()
    {
        return () =>
        {
            VisualElement element = itemTemplate.CloneTree();  
            return element;
        };
    }

   
    
    private Action<VisualElement, int> BindListItem()
    {
        UilistVeData = new List<VisualElement>();
        return (ve, index) =>
        {
            var label = ve.Q<Label>("uiname");
            var menuItem = ve.Q<VisualElement>("menu");
            var openBtn = ve.Q<Button>("openBtn");
            string prefabName = (string)soureData[index];
            openBtn.clicked += ListItemOpenClick(prefabName);
            var iconItem = ve.Q<VisualElement>("Icon");
            var deleteBtn = ve.Q<Button>("deleteBtn");
            deleteBtn.clicked += ListItemDeleteClick(prefabName);
            Texture2D texture = null;
            if (currentTree == "UI")
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/UIToolKit/Icon/edit12.png");
            else if (currentTree == "模型")
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/UIToolKit/Icon/edit3.png");
            else if (currentTree == "人物")
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/UIToolKit/Icon/edit3.png");
            else if (currentTree == "音效")
            {
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/UIToolKit/Icon/edit4.png");
                prefabName = Path.GetFileName(prefabName);
            }
            else if (currentTree == "音乐")
            {
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/UIToolKit/Icon/edit4.png");
                prefabName = Path.GetFileName(prefabName);
            }
            else if (currentTree == "Json")
            {
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/UIToolKit/Icon/edit5.png");
                prefabName = Path.GetFileName(prefabName);
            }
            else if (currentTree == "Excel")
            {
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/UIToolKit/Icon/edit6.png");
                prefabName = Path.GetFileName(prefabName);
            }

            iconItem.style.backgroundImage = texture;
            label.text = prefabName;
            menuItem.style.display = DisplayStyle.None;
            UilistVeData.Add(ve);
        };
    }

    private Action<VisualElement, int> BindSceneListItem()
    {
        UilistVeData = new List<VisualElement>();
        return (ve, index) =>
        {
            var label = ve.Q<Label>("uiname");
            var menuItem = ve.Q<VisualElement>("menu");
            var openBtn = ve.Q<Button>("openBtn");
            var iconItem = ve.Q<VisualElement>("Icon");
            string prefabName = (string)SceneListsoureData[index];
            openBtn.clicked += ListItemOpenClick(prefabName);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/UIToolKit/Icon/edit2.png");
            iconItem.style.backgroundImage = texture;
            var deleteBtn = ve.Q<Button>("deleteBtn");
            deleteBtn.clicked += ListItemDeleteClick(prefabName);
            string name = Path.GetFileName(prefabName).Replace(".unity", "");
            label.text = name;
            menuItem.style.display = DisplayStyle.None;
            UilistVeData.Add(ve);
        };
    }

    private Action ListItemDeleteClick(string name)
    {
        return () =>
        {
            if (currentTree == "Scene")
            {
                var perfetpath = name;
                if (!EditorUtility.DisplayDialog("删除场景", "是否删除场景 : " + name, "是", "否"))
                {
                    return;
                }
                AssetDatabase.DeleteAsset(perfetpath);
            }
            else {
                var path = "";
                if (currentTree == "模型")
                    path = "Assets/Resources/Prefabs/Model/" + name + ".prefab";
                else if (currentTree == "人物")
                    path = "Assets/Resources/Prefabs/Npc/" + name + ".prefab";
                else if(currentTree == "UI")
                    path = uiPath+"/"+name+ ".prefab";
                else if (currentTree == "音效")
                    path = Path.GetFileName(name);
                else if (currentTree == "音乐")
                    path = Path.GetFileName(name);
                else if (currentTree == "Json")
                    path = Path.GetFileName(name);
                else if (currentTree == "Excel")
                    path = Path.GetFileName(name);

                if (!EditorUtility.DisplayDialog("删除场景", "是否删除物体 : " + name, "是", "否"))
                {
                    return;
                }
               
                if (currentTree == "音效"|| currentTree == "音乐"|| currentTree == "Json" || currentTree == "Excel")
                   File.Delete(name);
                else
                   AssetDatabase.DeleteAsset(path);
            }

            ReLoadRes();
        };
    }

    private Action<IEnumerable<object>> OnTreeSelect()
    {
        return (val) =>
        {
            foreach (var item in val)
            {
                var selected = item as Planet;
                if (selected != null)
                {
                    TreeOnClick(selected.name);
                }
            }
        };
    }

    public void TreeOnClick(string treename)
    {
        RecycleTreeData();
        currentTree = treename;
        titleLabel.text = treename;
        currentTreeVe = treename;
        if (treename == "Scene")
        {
            LoadSceneRes();
            countLabel.text = "count: " + SceneListsoureData.Count;
        }
        else {
            LoadRes();
            countLabel.text = "count: " + soureData.Count;
        }
        lastTreeVe = currentTreeVe;
    }

    void OnEnable()
    {
        EditorApplication.projectChanged += OnProjectChanged;
    }

    void OnDisable()
    {
        EditorApplication.projectChanged -= OnProjectChanged;
    }

    void OnProjectChanged()
    {
        AssetDatabase.Refresh();
        ReLoadRes();
    }

    public void RecycleTreeData()
    {
        SceneListsoureData = new List<string>();
        soureData = new List<string>();
        contentListView.makeItem -= MakeUIListItem();
        contentListView.bindItem -= BindSceneListItem();
        contentListView.bindItem -= BindListItem();
        contentListView.itemsSource = null;
        contentListView.selectedIndicesChanged -= OnListSelcetChange();
        contentListView.ClearClassList();
        contentListView.ClearSelection();
        contentListView.Clear();
        lastClickTime = 0;
    }

    private Func<VisualElement> MakeItem()
    {
        return () =>
        {
            Label label = new Label();
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.marginLeft = 5;
            return label;
        };
    }

    // Nested interface that can be either a single planet or a group of planets.
    protected interface IPlanetOrGroup
    {
        public string name
        {
            get;
        }

        public bool populated
        {
            get;
        }
    }
    // Nested class that represents a planet.
    protected class Planet : IPlanetOrGroup
    {
        public string name
        {
            get;
        }

        public bool populated
        {
            get;
        }

        public Planet(string name, bool populated = false)
        {
            this.name = name;
            this.populated = populated;
        }
    }
    
    protected class PlanetGroup : IPlanetOrGroup
    {
        public string name
        {
            get;
        }

        public bool populated
        {
            get
            {
                var anyPlanetPopulated = false;
                foreach (Planet planet in planets)
                {
                    anyPlanetPopulated = anyPlanetPopulated || planet.populated;
                }
                return anyPlanetPopulated;
            }
        }

        public readonly IReadOnlyList<Planet> planets;

        public PlanetGroup(string name, IReadOnlyList<Planet> planets)
        {
            this.name = name;
            this.planets = planets;
        }
    }

    // Data about planets in our solar system.
    protected static readonly List<PlanetGroup> planetGroups = new List<PlanetGroup>
    {
        new PlanetGroup("场景资源", new List<Planet>
        {
            new Planet("UI"),
            new Planet("Scene", true),
            new Planet("模型"),
            new Planet("人物")
        }),
        new PlanetGroup("外部资源", new List<Planet>
        {
            new Planet("音效"),
            new Planet("音乐")
        }),
        new PlanetGroup("配置表", new List<Planet>
        {
            new Planet("Json"),
            new Planet("Excel")
        })
    };

    // Expresses planet data as a list of the planets themselves. Needed for ListView and MultiColumnListView.
    protected static List<Planet> planets
    {
        get
        {
            var retVal = new List<Planet>(8);
            foreach (var group in planetGroups)
            {
                retVal.AddRange(group.planets);
            }
            return retVal;
        }
    }


    // Expresses planet data as a list of TreeViewItemData objects. Needed for TreeView and MultiColumnTreeView.
    protected static IList<TreeViewItemData<IPlanetOrGroup>> treeRoots
    {
        get
        {
            int id = 0;
            var roots = new List<TreeViewItemData<IPlanetOrGroup>>(planetGroups.Count);
            foreach (var group in planetGroups)
            {
                var planetsInGroup = new List<TreeViewItemData<IPlanetOrGroup>>(group.planets.Count);
                foreach (var planet in group.planets)
                {
                    planetsInGroup.Add(new TreeViewItemData<IPlanetOrGroup>(id++, planet));
                }

                roots.Add(new TreeViewItemData<IPlanetOrGroup>(id++, group, planetsInGroup));
            }
            return roots;
        }
    }
}
