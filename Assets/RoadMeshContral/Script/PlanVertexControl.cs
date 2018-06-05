using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlanVertexData
{
    /// <summary>
    /// 节点坐标
    /// </summary>
    public Vector3[] _NodePoints;

    /// <summary>
    /// 曲线的节点
    /// </summary>
    public Vector3[] _CurvePoints { get; private set; }

    /// <summary>
    /// 面片宽度
    /// </summary>
    public int _PlanWidth = 2;

    /// <summary>
    /// 精细度
    /// </summary>
    public float _Fineness = 50;

    public float _Alpha = 0.5f;


    //空间位置
    public Vector2 _SpatialPos;
    //空间范围
    public Vector3 _SpatialDomain;


    public PlanVertexData()
    {

    }
    public PlanVertexData(Vector3[] Nodes, float fineness = 50f, float alpha = 0.5f)
    {
        _NodePoints = Nodes;
        _Fineness = fineness;
        _Alpha = alpha;
        _CurvePoints = Curve.CatmulRomCurve(Nodes, _Fineness, _Alpha);
    }
    public PlanVertexData(Transform[] Nodes, float fineness = 50f, float alpha = 0.5f)
    {
        _NodePoints = new Vector3[Nodes.Length];
        for(int i = 0; i < _NodePoints.Length; i++)
        {
            _NodePoints[i] = Nodes[i].position;
        }

        _Fineness = fineness;
        _Alpha = alpha;
        _CurvePoints = Curve.CatmulRomCurve(Nodes, _Fineness, _Alpha);
    }

    public PlanVertexData(string data)
    {
        PlanVertexData Data = JsonUtility.FromJson<PlanVertexData>(data);
        
        _NodePoints = Data._NodePoints;

        _Fineness = Data._Fineness;
        _Alpha = Data._Alpha;
        _SpatialPos = Data._SpatialPos;
        _SpatialDomain = Data._SpatialDomain;
        _CurvePoints = Curve.CatmulRomCurve(_NodePoints, _Fineness, _Alpha);
    }
}

[DisallowMultipleComponent,RequireComponent(typeof(MeshFilter))]
public class PlanVertexControl : MonoBehaviour {
    
    //面宽度
    [SerializeField, Header("Plan宽度"), Range(0, 10)]
    public int _PlanWidth = 2;
    
    [SerializeField, Header("Json数据")]
    public TextAsset m_RoadPlanData;

    //物体网格
    private Mesh _Mesh;
    private MeshRenderer m_MeshRenderer;
    private Vector3[][] _GridVerticesList;
    public RenderTexture m_RenderTexture;

    public PlanVertexData m_PlanVertexData;
    


    #region 曲线使用的变量
    private Vector3[] m_CurvePoints;
    //set from 0-1
    public float alpha = 0.5f;
    //How many points you want on the curve
    [SerializeField, Header("精细度"), Range(10, 100)]
    float amountOfPoints = 50.0f;
    #endregion

   // private Transform[] _VerticesObjList;
    private Vector3[] _SelfEditVerticesList;
    private TextMesh m_TextMesh;
    

    // Use this for initialization
    void Start () {
        #region Error
        if (GetComponent<MeshFilter>() == null)
        {
            gameObject.SetActive(false);
            Debug.LogError("警告" + "游戏物体缺少组件 MeshFilter！");
            return;
        }
        if (GetComponent<MeshRenderer>() == null)
        {
            gameObject.SetActive(false);
            Debug.LogError("警告" + "游戏物体缺少组件 MeshRenderer！");
            return;
        }
        //if (transform.localScale != Vector3.one)
        //{
        //    transform.localScale = Vector3.one;
        //    Debug.Log("游戏物体的缩放已归为初始值");
        //}
        #endregion

       _Mesh = GetComponent<MeshFilter>().mesh;
        m_MeshRenderer = GetComponent<MeshRenderer>();
       m_TextMesh = GetComponentInChildren<TextMesh>();
        
        m_PlanVertexData = new PlanVertexData(m_RoadPlanData.text);

        m_CurvePoints = m_PlanVertexData._CurvePoints;


        InitCamera();

        m_MeshRenderer.material.SetVector("_CameraParams", new Vector4(m_PlanVertexData._SpatialPos.x, m_PlanVertexData._SpatialPos.y, Mathf.Abs(m_PlanVertexData._SpatialDomain.x), Mathf.Abs(m_PlanVertexData._SpatialDomain.x)));

        Debug.Log(m_MeshRenderer.material.GetVector("_CameraParams"));


        GenerateVertexNode();
       dt = Time.time;
    }

    void InitCamera()
    {
        int camLay = LayerMask.NameToLayer("PlanCamera");

        Camera[] Cameras = Camera.allCameras;
        Camera planCamera = null;
        foreach (Camera c in Cameras)
        {
            if (c.gameObject.layer == camLay)
            {
                planCamera = c;
                break;
            }

        }

        Debug.Log("_SpatialPos : " + m_PlanVertexData._SpatialPos + "  _SpatialDomain : " + m_PlanVertexData._SpatialDomain);

        if (planCamera == null)
        {
            planCamera = new GameObject("PlanCamera").AddComponent<Camera>();
        }
        planCamera.transform.position = new Vector3(m_PlanVertexData._SpatialPos.x, m_PlanVertexData._SpatialDomain.y, m_PlanVertexData._SpatialPos.y);
        planCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        planCamera.clearFlags = CameraClearFlags.Nothing;
        int maskCamera = 1 << LayerMask.NameToLayer("Car"); 
        planCamera.cullingMask = maskCamera;
        planCamera.gameObject.layer = camLay;
        planCamera.orthographic = true;
        planCamera.orthographicSize = Mathf.Abs(m_PlanVertexData._SpatialDomain.x / 2) ;
        planCamera.farClipPlane = m_PlanVertexData._SpatialDomain.y;
        planCamera.targetTexture = m_RenderTexture;
        planCamera.useOcclusionCulling = false;
        planCamera.allowHDR = false;
        planCamera.allowMSAA = false;
        planCamera.allowDynamicResolution = false;
    }

    float timer = 0;
    float needTimer = 1f;
    float dt = 0;
    int m_IndexUp = 0;
    // Update is called once per frame
    void Update()
    {
        if (_Mesh != null)
        {
            //for(int i  = 0; i < _VerticesObjList.Length; i++)
            //{
            //    _SelfEditVerticesList[i] = transform.worldToLocalMatrix.MultiplyPoint3x4(_VerticesObjList[i].position);
            //}

            //_Mesh.vertices = _SelfEditVerticesList;
            //_Mesh.RecalculateNormals();
        }
        
    }
    


    Vector3 oldPos;
    /// <summary>
    /// 生成曲线模型
    /// </summary>
    /// <param name="minceValue">细分值</param>
    void GenerateVertexNode()
    {
        oldPos = transform.position;
        ///到时候要去掉
       // _VerticesObjList = new Transform[m_CurvePoints.Length * 2];
        ///

        if (m_CurvePoints.Length < 3) return;
        
        _GridVerticesList = new Vector3[m_CurvePoints.Length][];
        _SelfEditVerticesList = new Vector3[m_CurvePoints.Length * 2];
        
        for (int i = 0; i < _GridVerticesList.Length; i++)
        {
            if (_GridVerticesList[i] == null) _GridVerticesList[i] = new Vector3[2];

            for (int j = 0; j < 2; j++)
            {
                Vector3 newPos = m_CurvePoints[i] + Vector3.Cross(Vector3.up.normalized, (m_CurvePoints[i] - oldPos).normalized) * ((j == 0) ? _PlanWidth  : -_PlanWidth);

                oldPos = m_CurvePoints[i];
                _GridVerticesList[i][j] = newPos;
                _SelfEditVerticesList[i * 2 + j] = newPos;

                //GameObject obj = Instantiate(m_TextMesh.gameObject, transform);
                //obj.transform.localPosition = _SelfEditVerticesList[i * 2 + j];
                //obj.transform.localScale = Vector3.one * 0.05f;
                //obj.GetComponent<TextMesh>().text = (i * 2 + j).ToString();
                //_VerticesObjList[i * 2 + j] = obj.transform;
            }
        }
        

        int currentIndex = 0;
        int[] Alltriangles = new int[(_SelfEditVerticesList.Length - 2) * 3];
        for (int i = 0, j = 0; i < Alltriangles.Length / 2; i++, j += 2)
        {
            if (currentIndex >= Alltriangles.Length) break;

            Alltriangles[currentIndex++] = j;
            Alltriangles[currentIndex++] = j + 1;
            Alltriangles[currentIndex++] = j + 3;

            Alltriangles[currentIndex++] = j;
            Alltriangles[currentIndex++] = j + 3;
            Alltriangles[currentIndex++] = j + 2;
        }

        
        Vector2[] AllUV = new Vector2[_SelfEditVerticesList.Length];
        float zInterval = 4.0f / (_GridVerticesList.Length - 1);
        float xInterval = 1.0f / (_GridVerticesList[0].Length - 1);

        float Zvalue = 0;
        float Xvalue = 0;
        for (int i = 0; i < _GridVerticesList.Length; i++)
        {
            Zvalue = ((float)i) * zInterval;

            for (int j = 0; j < _GridVerticesList[i].Length; j++)
            {
                Xvalue = ((float)j) * xInterval;
                AllUV[i * 2 + j] = new Vector2(Zvalue, Xvalue);
            }
        }
        
        _Mesh.Clear();
        _Mesh.vertices = _SelfEditVerticesList;
        _Mesh.triangles = Alltriangles;
        _Mesh.uv = AllUV;
        _Mesh.RecalculateNormals();
        _Mesh.RecalculateBounds();
    }
    

    void OnDrawGizmos()
    {
        //if (m_CurvePoints == null) return;

        //Gizmos.color = Color.red;
        //foreach (Vector3 temp in m_CurvePoints)
        //{
        //    Vector3 pos = new Vector3(temp.x, transform.position.y, temp.z);

        //    //Gizmos.color = Color.green;
        //    //Vector3 nor = Vector3.Cross(Vector3.up.normalized, pos.normalized);
        //    //Gizmos.DrawLine(pos, nor * 0.1f);

        //    //Gizmos.color = Color.blue;
        //    //Vector3 nor2 = Vector3.Cross(Vector3.forward.normalized, pos.normalized);
        //    //Gizmos.DrawLine(pos, nor2 * 0.2f);

        //    Gizmos.color = Color.red;
        //    Gizmos.DrawSphere(pos, 0.3f);
        //}
    }
}

