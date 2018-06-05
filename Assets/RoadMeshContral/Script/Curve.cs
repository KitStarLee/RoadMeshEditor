using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Curve  {

    private static List<Vector3> m_CurvePoints = new List<Vector3>();

    /// <summary>
    /// 卡特罗姆曲线
    /// </summary>
	public static Vector3[] CatmulRomCurve(Vector3[] Nodes, float fineness = 50f, float alpha = 0.5f)
    {
        m_CurvePoints.Clear();
        int index = 0;
        
        while (index < Nodes.Length - 3)
        {
            Vector3 p0 = Nodes[index]; // Vector3 has an implicit conversion to Vector2
            Vector3 p1 = Nodes[index + 1];
            Vector3 p2 = Nodes[index + 2];
            Vector3 p3 = Nodes[index + 3];

            float t0 = 0.0f;
            float t1 = CatmulRomT(t0, p0, p1, alpha);
            float t2 = CatmulRomT(t1, p1, p2, alpha);
            float t3 = CatmulRomT(t2, p2, p3, alpha);

            for (float t = t1; t < t2; t += ((t2 - t1) / fineness))
            {
                Vector3 A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
                Vector3 A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
                Vector3 A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

                Vector3 B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2;
                Vector3 B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3;

                Vector3 C = (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2;

                m_CurvePoints.Add(C);
            }
            index++;
        }

        return m_CurvePoints.ToArray();
    }

    /// <summary>
    /// 卡特罗姆曲线
    /// </summary>
    public static Vector3[] CatmulRomCurve(Transform[] Nodes, float fineness = 50f, float alpha = 0.5f)
    {
        m_CurvePoints.Clear();
        int index = 0;

        while (index < Nodes.Length - 3)
        {
            Vector3 p0 = Nodes[index].position; // Vector3 has an implicit conversion to Vector2
            Vector3 p1 = Nodes[index + 1].position;
            Vector3 p2 = Nodes[index + 2].position;
            Vector3 p3 = Nodes[index + 3].position;

            float t0 = 0.0f;
            float t1 = CatmulRomT(t0, p0, p1, alpha);
            float t2 = CatmulRomT(t1, p1, p2, alpha);
            float t3 = CatmulRomT(t2, p2, p3, alpha);

            for (float t = t1; t < t2; t += ((t2 - t1) / fineness))
            {
                Vector3 A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
                Vector3 A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
                Vector3 A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;

                Vector3 B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2;
                Vector3 B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3;

                Vector3 C = (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2;

                m_CurvePoints.Add(C);
            }
            index++;
        }

        return m_CurvePoints.ToArray();
    }
    private static float CatmulRomT(float t, Vector3 p0, Vector3 p1, float alpha = 0.5f)
    {
        float a = Mathf.Pow((p1.x - p0.x), 2.0f) + Mathf.Pow((p1.y - p0.y), 2.0f) + Mathf.Pow((p1.z - p0.z), 2.0f);
        float b = Mathf.Pow(a, 0.5f);
        float c = Mathf.Pow(b, alpha);

        return (c + t);
    }
}
