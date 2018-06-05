using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;

[ExecuteInEditMode]
public class PlanVertexVisual : MonoBehaviour
{
    [HideInInspector]
    public List<Transform> m_Nodes = new List<Transform>();
    public float fineness = 50f;

    public PlanVertexData m_PlanVertexData = new PlanVertexData();

  
    /// <summary>
    /// 获取模拟字段的大小
    /// </summary>
    public  Vector2 Size
    {
        get { return _size; }
        set
        {
            if (_size != value)
            {
                _size.x = Mathf.Clamp(Mathf.Abs(value.x), 0f, float.PositiveInfinity);
                _size.y = Mathf.Clamp(Mathf.Abs(value.y), 0f, float.PositiveInfinity);
                Debug.Log("_size : " + _size);
                UpdateCollider();
            }
        }
    }

    public float Depth
    {
        get { return _depth; }
        set
        {
            _depth = Mathf.Clamp(value, 0f, 10000f);
            
            UpdateCollider();
        }
    }

    /* Property fields */
    [SerializeField]
    private Vector2 _size = new Vector2(10f, 10f);
    [SerializeField]
    private float _depth = 10f;
    

    private BoxCollider _collider;
    
    public void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        if (_collider == null)
        {
            _collider = gameObject.AddComponent<BoxCollider>();
            UpdateCollider();
        }
       
    }



    public virtual void UpdateCollider()
    {
        Vector2 sizeScaled = new Vector2( _size.x * transform.lossyScale.x, _size.y * transform.lossyScale.y);
        //  _collider.center = new Vector3(sizeScaled.x / 2f, _depth / 2f, sizeScaled.y / 2f);
        //  _collider.center = new Vector3(transform.position.x - sizeScaled.x / 2f, transform.position.y, transform.position.z - sizeScaled.y / 2f);
        _collider.center = new Vector3(0, transform.position.y, 0);

        _collider.size = new Vector3(sizeScaled.x, _depth, sizeScaled.y);

        m_PlanVertexData._SpatialDomain = new Vector3(_collider.size.x, _collider.size.y, _collider.size.z);
    }

    public Vector3 GetDirection
    {
        get
        {
            if (m_Nodes.Count > 2)
            {
                return (m_Nodes[m_Nodes.Count - 1].position - m_Nodes[m_Nodes.Count - 2].position).normalized;
            }
            else
            {
                return Vector3.right;
            }
        }
    }

    public string ToJson()
    {
        m_PlanVertexData._Fineness = fineness;


        m_PlanVertexData._NodePoints = new Vector3[m_Nodes.Count];
        for (int i = 0; i < m_Nodes.Count; i++)
        {
            m_PlanVertexData._NodePoints[i] = m_Nodes[i].position;
        }
        m_PlanVertexData._Alpha = 0.5f;

        string str = JsonUtility.ToJson(m_PlanVertexData);
        return str;

    }

    public int Count
    {
        get
        {
            if (m_Nodes != null)
                return m_Nodes.Count;
            else
            {
                return 0;
            }
        }
    }

    public Transform Remove(Transform obj)
    {
        if (Contains(obj))
        {
            m_Nodes.Remove(obj);
            GameObject.DestroyImmediate(obj.gameObject);
            if (m_Nodes.Count >= 2)
            {
                return m_Nodes[m_Nodes.Count - 1];
            }
            else
            {
                return null;
            }
        }

        return null;
    }

    public Transform GetEndNode
    {
        get
        {
            if (m_Nodes.Count >= 1)
            {
                return m_Nodes[m_Nodes.Count - 1];
            }
            return null;
        }
    }

    public bool Contains(Transform obj)
    {
        return m_Nodes.Contains(obj);
    }

    void OnDrawGizmos()
    {
        if(m_PlanVertexData!= null)
        {
            m_PlanVertexData._SpatialPos = new Vector2(transform.position.x, transform.position.z);
        }

        if (m_Nodes == null && m_Nodes.Count < 1) return;

        Vector3[] m_CurvePoints = Curve.CatmulRomCurve(m_Nodes.ToArray(), fineness);


        if (m_CurvePoints == null) return;

        Gizmos.color = Color.red;
        foreach (Vector3 temp in m_CurvePoints)
        {
            Vector3 pos = new Vector3(temp.x, transform.position.y, temp.z);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pos, m_Nodes[0].localScale.x / 2);
        }

        Gizmos.color = new Color(0.5f,0.2f,0.2f,0.4f);
        if (_collider != null)
        {
            Gizmos.DrawCube(_collider.bounds.center, _collider.bounds.size);
        }
    }
}

public class PlanVertexEditor : EditorWindow
{
    [@MenuItem("RoadPlanVertex/打开Plan编辑界面")]
    static void main()
    {
        EditorWindow.GetWindowWithRect<PlanVertexEditor>(new Rect(0, 0, 350, 210), false, "Plan编辑器");
    }


    [@MenuItem("RoadPlanVertex/创建道路网格实例")]
    static void CreatRoadPlan()
    {
        PlanVertexControl planControl = FindObjectOfType<PlanVertexControl>();
        if (planControl == null)
        {
            GameObject obj = GameObject.Instantiate(Resources.Load("SimplePlan"), Vector3.zero,Quaternion.identity) as GameObject;

            planControl = obj.GetComponent<PlanVertexControl>();
        }
        else
        {
            Debug.Log("已經存在 SimplePlan 對象");
        }
    }

    [@MenuItem("RoadPlanVertex/最后保存的文件")]
    static void GetLastFilePath()
    {

    }

    public static string m_LastFile;

    public GameObject ParentTarget;
    public PlanVertexVisual m_PlanVertexVisual;
    private Vector2 scrollVec2Fineness;
    private string targetName;
    private float fineness;
    private float oldfineness;
    public Transform curr_Node;
    public TextAsset m_RoadPlanData;

    private void OnHierarchyChange()
    {

    }

    private void OnGUI()
    {
        GUI.backgroundColor = new Color(0.2f,0.9f,1f,0.9f);

        if (ParentTarget == null)
        {
            ParentTarget = new GameObject("PlanVertexEditor", typeof(PlanVertexVisual));
            m_PlanVertexVisual = ParentTarget.GetComponent<PlanVertexVisual>();
            Selection.activeGameObject = ParentTarget;
        }

        scrollVec2Fineness = EditorGUILayout.BeginScrollView(scrollVec2Fineness, GUILayout.Width(position.width), GUILayout.Height(position.height));

        //#region 打开网页
        //EditorGUILayout.BeginHorizontal("MeTransitionHead");
        //if (GUILayout.Button("访问CSDN博客", "toolbarbutton"))
        //{
        //    Help.BrowseURL(@"http://blog.csdn.net/qq992817263/article/details/51579913");
        //}
        //if (GUILayout.Button("访问github项目", "toolbarbutton"))
        //{
        //    Help.BrowseURL(@"https://github.com/coding2233/MeshEditor");
        //}
        //EditorGUILayout.EndHorizontal();
        //#endregion

        if (m_PlanVertexVisual.Count < 1)
        {
            #region Json数据
            EditorGUILayout.BeginHorizontal("HelpBox");
            GUILayout.Label("Json数据:");
            if (ParentTarget != null)
            {
                m_RoadPlanData = (TextAsset)EditorGUI.ObjectField(new Rect(2, 5, position.width * 0.5f, 20), m_RoadPlanData, typeof(TextAsset), false);
                if (m_RoadPlanData != null)
                {
                    if (GUILayout.Button("解析Json数据", GUILayout.Width(200)))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            PlanVertexData data = JsonUtility.FromJson<PlanVertexData>(m_RoadPlanData.text);
                            fineness = data._Fineness;
                            foreach (Vector3 v in data._NodePoints)
                            {
                                CreatNode(v);
                            }

                            Debug.Log("_SpatialDomain : " + data._SpatialDomain + " _SpatialPos : " + data._SpatialPos);
                            m_PlanVertexVisual.Size = new Vector2(data._SpatialDomain.x, data._SpatialDomain.z);
                            m_PlanVertexVisual.Depth = data._SpatialDomain.y;
                            m_PlanVertexVisual.transform.position = new Vector3(data._SpatialPos.x, 0, data._SpatialPos.y);

                        };
                    }
                }
            }
            else
            {
                GUILayout.Label("无", "ErrorLabel");
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            #endregion
        }
        if (m_PlanVertexVisual != null && m_PlanVertexVisual.m_Nodes != null && ParentTarget != null)
        {
            if (Selection.activeGameObject != null)
            {
                #region 当前编辑目标
                EditorGUILayout.BeginHorizontal("HelpBox");
                GUILayout.Label("当前编辑节点:");
                if (m_PlanVertexVisual.Contains(Selection.activeGameObject.transform))
                {
                    curr_Node = Selection.activeGameObject.transform;
                    targetName = curr_Node.name;
                    if (GUILayout.Button(targetName, GUILayout.Width(200)))
                    {
                        EditorApplication.delayCall += () =>
                        {
                            Selection.activeGameObject = curr_Node.gameObject;
                        };
                    }
                }
                else
                {
                    GUILayout.Label("无", "ErrorLabel");
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
                #endregion
            }


            #region 精细度
            EditorGUILayout.BeginHorizontal("HelpBox");
            GUILayout.Label("精细度: " + fineness);
            if (ParentTarget != null)
            {
                fineness = GUILayout.HorizontalSlider(fineness, 10, 100);
                if (fineness != oldfineness)
                {
                    m_PlanVertexVisual.fineness = fineness;
                    oldfineness = fineness;
                }
            }
            else
            {
                GUILayout.Label("无", "ErrorLabel");
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            #endregion


            #region 顶点数量
            EditorGUILayout.BeginHorizontal("HelpBox");
            GUILayout.Label("当前节点数量:");
            if (ParentTarget != null && m_PlanVertexVisual != null)
            {
                GUILayout.Label(m_PlanVertexVisual.Count.ToString());
            }
            else
            {
                GUILayout.Label("无", "ErrorLabel");
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            #endregion

        }

        GUI.backgroundColor = new Color(0.2f, 0.5f, 1f, 0.9f);

        #region 创建、删除
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("创建节点", "LargeButtonLeft"))
        {
            EditorApplication.delayCall += CreatNode;
        }
        if (GUILayout.Button("删除节点", "LargeButtonRight"))
        {
            EditorApplication.delayCall += () =>
            {
                if (Selection.activeGameObject != null)
                {
                    if (m_PlanVertexVisual.Contains(Selection.activeGameObject.transform))
                    {
                        curr_Node = Selection.activeGameObject.transform;
                        targetName = curr_Node.name;
                        Transform obj = m_PlanVertexVisual.Remove(Selection.activeGameObject.transform);

                        if (obj != null)
                        {
                            Selection.activeGameObject = obj.gameObject;
                        }
                        else
                        {
                            Selection.activeGameObject = ParentTarget;
                        }
                    }
                }

            };
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        //#region 复制、多个复制
        //EditorGUILayout.BeginHorizontal();
        //if (GUILayout.Button("复制", "LargeButtonLeft"))
        //{
        //    // EditorApplication.delayCall += CollapseOnTwoVertex;
        //}
        //if (GUILayout.Button("多个复制", "LargeButtonRight"))
        //{
        //    // EditorApplication.delayCall += CollapseOnMoreVertex;
        //}
        //EditorGUILayout.EndHorizontal();
        //#endregion

        #region 取消编辑

        GUI.color = Color.yellow;
        GUI.backgroundColor = Color.red;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("取消编辑", "LargeButton"))
        {
            Close();
        }
        EditorGUILayout.EndHorizontal();
        #endregion
        
        #region 保存数据

        GUI.color = Color.green;
        GUI.backgroundColor = Color.green;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("保存编辑数据", "LargeButton"))
        {
            EditorApplication.delayCall += Finish;
        }
        EditorGUILayout.EndHorizontal();
        #endregion

        EditorGUILayout.EndScrollView();
    }

    void Finish()
    {
        FileInfo file = new FileInfo(Application.dataPath + "/RoadPlanVertex" + System.DateTime.Now.Hour + "_" + System.DateTime.Now.Minute + "_" + System.DateTime.Now.Second + ".json");

        using (FileStream stream = file.Open(FileMode.Create, FileAccess.ReadWrite))
        {
            string str = m_PlanVertexVisual.ToJson();

            byte[] bytes = Encoding.UTF8.GetBytes(str);

            stream.Write(bytes, 0, bytes.Length);
        }
        EditorApplication.RepaintProjectWindow();

        Debug.Log("保存路径：" + file.Name);
    }

    void CreatNode()
    {
        if (ParentTarget == null) return;

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.parent = ParentTarget.transform;
        if (Selection.activeGameObject != null)
        {
            obj.transform.position = Selection.activeGameObject.transform.position + m_PlanVertexVisual.GetDirection * 8;
        }
        else if (m_PlanVertexVisual != null && m_PlanVertexVisual.GetEndNode != null)
        {
            obj.transform.position = m_PlanVertexVisual.GetEndNode.position + m_PlanVertexVisual.GetDirection * 8;
        }
        else
        {
            obj.transform.localPosition = Vector3.zero;
        }

        obj.transform.localScale = Vector3.one * 0.3f;
        Selection.activeGameObject = obj;
        m_PlanVertexVisual.m_Nodes.Add(obj.transform);

        obj.name = "Node" + m_PlanVertexVisual.Count;

    }
    void CreatNode(Vector3 pos)
    {
        if (ParentTarget == null) return;

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.parent = ParentTarget.transform;
        obj.transform.position = pos;

        obj.transform.localScale = Vector3.one * 0.3f;
        Selection.activeGameObject = obj;
        m_PlanVertexVisual.m_Nodes.Add(obj.transform);

        obj.name = "Node" + m_PlanVertexVisual.Count;

    }



    private void OnDestroy()
    {
        if (ParentTarget != null)
        {
            DestroyImmediate(ParentTarget);
        }
    }
}

public abstract class UndoEditor<T> : Editor where T : UnityEngine.Object
{
    protected T _object;

    private void OnEnable()
    {
        _object = target as T;
    }

    public override void OnInspectorGUI()
    {
        if (_object == null)
            return;
        
        OnInspectorGUIDraw();
    }

    public void OnSceneGUI()
    {
        OnSceneGUIDraw();

        if (Event.current.type == EventType.ValidateCommand)
        {
            switch (Event.current.commandName)
            {
                case "UndoRedoPerformed":
                    OnSceneGUIUndo();
                    break;
            }
        }
    }

    protected abstract void OnInspectorGUIDraw();
    protected abstract void OnSceneGUIDraw();
    protected abstract void OnSceneGUIUndo();
}


[CustomEditor(typeof(PlanVertexVisual))]
public class PlanVertexVisualEditor : UndoEditor<PlanVertexVisual>
{
    protected override void OnInspectorGUIDraw()
    {

        EditorGUILayout.HelpBox("道路大小控制", MessageType.None, true);

        //      public List<Transform> m_Nodes = new List<Transform>();
        //public float fineness = 50f;

        //public PlanVertexData m_PlanVertexData = new PlanVertexData();

        float sizeX = Mathf.Clamp(EditorGUILayout.FloatField("Length", _object.Size.x), 0f, float.PositiveInfinity);
        float sizeY = Mathf.Clamp(EditorGUILayout.FloatField("Width", _object.Size.y), 0f, float.PositiveInfinity);

        _object.Size = new Vector2(sizeX, sizeY);
        
        _object.Depth = Mathf.Clamp(EditorGUILayout.FloatField("Hight", _object.Depth), 0f, float.PositiveInfinity);

        EditorGUILayout.HelpBox("曲面道路参数", MessageType.None, true);
        if (_object.Count > 0)
        {
             EditorGUILayout.TextField(new GUIContent("节点数量"), _object.Count.ToString());

            _object.fineness = Mathf.Clamp(EditorGUILayout.FloatField("精细度", _object.fineness), 10f, 100);

            //if (_object.UseFakeNormals)
            //{
            //    EditorGUILayout.BeginVertical();
            //    _object.NormalizeFakeNormals = EditorGUILayout.Toggle(new GUIContent("Normalize normals",
            //        "Indicates whether to normalize fast approximate normals."), _object.NormalizeFakeNormals);
            //    EditorGUILayout.EndVertical();
            //}
        }

        //_object.SetTangents = EditorGUILayout.Toggle(new GUIContent("Set tangents", "Whether the tangents must be set (usually for bump-mapped shaders)" +
        //                                                                            "Enabling this may sometimes result in performance drop on high" +
        //                                                                            " Quality levels. It is better to turn it off if " +
        //                                                                            "your shader doesn't uses normals."), _object.SetTangents);
    }

    protected override void OnSceneGUIDraw()
    {
        Handles.color = Color.red * 10;
        Vector3 pos = _object.transform.position + _object.transform.right * _object.Size.x;
        Handles.DrawLine(_object.transform.position, pos);
        float sizeX = Mathf.Clamp(_object.transform.InverseTransformPoint(Handles.Slider(pos, _object.transform.right,
                                                                         HandleUtility.GetHandleSize(pos) * 0.15f, Handles.CubeCap,
                                                                         1f)).x, 0.001f, float.PositiveInfinity);

        Handles.color = Color.blue * 10;
        pos = _object.transform.position + _object.transform.forward * _object.Size.y;
        Handles.DrawLine(_object.transform.position, pos);
        float sizeY = Mathf.Clamp(_object.transform.InverseTransformPoint(Handles.Slider(pos, _object.transform.forward,
                                                                          HandleUtility.GetHandleSize(pos) * 0.15f, Handles.CubeCap,
                                                                          1f)).z, 0.001f, float.PositiveInfinity);

        _object.Size = new Vector2(sizeX, sizeY);
    }

    protected override void OnSceneGUIUndo()
    {
        Vector2 size = _object.Size;
        _object.Size = new Vector2(0f, 0f);
        _object.Size = size;
    }
}