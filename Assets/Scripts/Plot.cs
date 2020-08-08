using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UI;
using System.Security.Cryptography;

[RequireComponent(typeof(RectTransform))]
public class Plot : MonoBehaviour
{
    [System.Serializable]
    public class DataSource
    {
        public Component target;
        public string name;
        public Color color;

        public float GetValue()
        {
            string[] methodNames = name.Split('.');
            object obj = target;
            for (int i = 0; i < methodNames.Length; i++)
            {
                Type t = obj.GetType();
                BindingFlags bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance
                    | BindingFlags.Public | BindingFlags.NonPublic
                    | BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty;
                obj = t.InvokeMember(methodNames[i], bindingFlags, null, obj, null);
            }
            return (float)obj;
        }
    }

    public DataSource[] dataSources;
    public float xScale = 10;
    public float yScale = 10;
    public float lineWidth = 1;

    private Queue<GameObject> lineSegments = new Queue<GameObject>();
    private float[] prevY = null;
    private float canvasWidth;

    private void Start()
    {
        prevY = new float[dataSources.Length];
        canvasWidth = ((RectTransform)transform).rect.width;
    }

    private void FixedUpdate()
    {
        // Remove out-of-range line segments
        while (lineSegments.Count > 0)
        {
            GameObject lineSeg = lineSegments.Peek();
            if (((RectTransform)lineSeg.transform).rect.x < -canvasWidth / 2)
            {
                Destroy(lineSeg);
                lineSegments.Dequeue();
            }
            else
                break;
        }

        // Shift all line segments
        foreach (GameObject lineSeg in lineSegments)
        {
            lineSeg.transform.position += new Vector3(-xScale * Time.fixedDeltaTime, 0, 0);
        }

        for (int i = 0; i < dataSources.Length; i++)
        {
            float y = dataSources[i].GetValue();
            DrawLineSeg(
                new Vector2(canvasWidth / 2 - xScale, prevY[i] * yScale),
                new Vector2(canvasWidth / 2, y * yScale),
                lineWidth,
                dataSources[i].color);
            prevY[i] = y;
        }
    }

    private void DrawLineSeg(Vector2 src, Vector2 dst, float width, Color c)
    {
        GameObject lineSeg = new GameObject("lineSeg", typeof(RectTransform));
        lineSegments.Enqueue(lineSeg);

        float angle = Vector2.SignedAngle(Vector2.right, dst - src);
        float length = Vector2.Distance(src, dst);

        RectTransform rectTrans = (RectTransform)lineSeg.transform;
        rectTrans.SetParent(this.transform);
        rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, length);
        rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, width);
        rectTrans.Rotate(0, 0, angle);
        rectTrans.anchoredPosition = (src + dst) / 2;

        Image image = lineSeg.AddComponent<Image>();
        image.color = c;
    }
}