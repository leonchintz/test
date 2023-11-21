using System.Collections.Generic;
using UnityEngine;

namespace TestT
{
    public static class WaterGenerator
    {
        public static void Init()
        {
            // 顶点列表  
            List<Vector2> vertices = new List<Vector2>()
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            // 回调函数  
            void OnTriangle(List<Vector2> triangle)
            {
                Debug.Log("Found Triangle: " + triangle);
            }

            // 调用三角剖分方法  
            Triangulate(vertices, OnTriangle);
        }


        // 三角剖分的回调函数  
        public delegate void TriangulationCallback(List<Vector2> triangle);

        // 执行Bowyer-Watson算法进行三角剖分  
        public static void Triangulate(List<Vector2> vertices, TriangulationCallback callback)
        {
            int numVertices = vertices.Count;

            // 构造超级三角形  
            List<Vector2> superTriangle = new List<Vector2>();
            float maxX = float.MinValue, maxY = float.MinValue, minX = float.MaxValue, minY = float.MaxValue;
            for (int i = 0; i < numVertices; i++)
            {
                maxX = Mathf.Max(maxX, vertices[i].x);
                maxY = Mathf.Max(maxY, vertices[i].y);
                minX = Mathf.Min(minX, vertices[i].x);
                minY = Mathf.Min(minY, vertices[i].y);
            }
            float deltaX = maxX - minX;
            float deltaY = maxY - minY;
            superTriangle.Add(new Vector2(minX - deltaX, minY - deltaY));
            superTriangle.Add(new Vector2(minX - deltaX, maxY + deltaY));
            superTriangle.Add(new Vector2(maxX + deltaX, minY - deltaY));

            // 将顶点添加到超级三角形中  
            List<int> superTriangleIndices = new List<int>();
            for (int i = 0; i < numVertices; i++)
            {
                superTriangleIndices.Add(i);
            }
            for (int i = 0; i < 3; i++)
            {
                superTriangleIndices.Add(numVertices + i);
            }
            vertices.AddRange(superTriangle);

            // 执行Bowyer-Watson算法  
            List<int> triangleIndices = new List<int>();
            for (int i = 0; i < superTriangleIndices.Count; i++)
            {
                int index = superTriangleIndices[i];
                int nextIndex = (index + 1) % superTriangleIndices.Count;
                int prevIndex = (index - 1 + superTriangleIndices.Count) % superTriangleIndices.Count;
                triangleIndices.Clear();
                GetTriangleIndices(vertices, superTriangleIndices, index, ref triangleIndices);
                if (triangleIndices.Count > 3)
                {
                    triangleIndices.RemoveAt(triangleIndices.Count - 1);
                    Refine(vertices, superTriangleIndices, triangleIndices, ref callback);
                }
                else if (triangleIndices.Count == 3)
                {
                    callback(GetTriangle(vertices, triangleIndices));
                }
                triangleIndices.Clear();
                triangleIndices.Add(prevIndex);
                triangleIndices.Add(index);
                triangleIndices.Add(nextIndex);
                if (triangleIndices.Count > 3)
                {
                    triangleIndices.RemoveAt(triangleIndices.Count - 1);
                    Refine(vertices, superTriangleIndices, triangleIndices, ref callback);
                }
                else if (triangleIndices.Count == 3)
                {
                    callback(GetTriangle(vertices, triangleIndices));
                }
            }
        }

        // 获取三角形顶点的索引列表  
        private static void GetTriangleIndices(List<Vector2> vertices, List<int> indices, int startIndex, ref List<int> triangleIndices)
        {
            int index = startIndex;
            do
            {
                triangleIndices.Add(indices[index]);
                index = (index + 1) % indices.Count;
            } while (index != startIndex && indices[index] != -1);
        }

        // 获取三角形的顶点列表  
        private static List<Vector2> GetTriangle(List<Vector2> vertices, List<int> triangleIndices)
        {
            List<Vector2> triangle = new List<Vector2>();
            for (int i = 0; i < triangleIndices.Count; i++)
            {
                triangle.Add(vertices[triangleIndices[i]]);
            }
            return triangle;
        }

        // 细化三角形  
        private static void Refine(List<Vector2> vertices, List<int> indices, List<int> triangleIndices, ref TriangulationCallback callback)
        {
            int index = triangleIndices[0];
            int oppositeIndex = -1;
            for (int i = 1; i < triangleIndices.Count; i++)
            {
                if (triangleIndices[i] != index)
                {
                    oppositeIndex = triangleIndices[i];
                    break;
                }
            }
            indices[index] = -1;
            for (int i = 0; i < indices.Count; i++)
            {
                if (indices[i] == oppositeIndex)
                {
                    indices[i] = index;
                }
                else if (indices[i] > index)
                {
                    indices[i]--;
                }
            }
            List<int> newIndices = new List<int>();
            for (int i = 0; i < indices.Count; i++)
            {
                if (indices[i] != -1)
                {
                    newIndices.Add(indices[i]);
                }
            }
            if (newIndices.Count >= 3)
            {
                triangleIndices.Clear();
                GetTriangleIndices(vertices, newIndices, oppositeIndex, ref triangleIndices);
                if (triangleIndices.Count > 3)
                {
                    triangleIndices.RemoveAt(triangleIndices.Count - 1);
                    Refine(vertices, newIndices, triangleIndices, ref callback);
                }
                else if (triangleIndices.Count == 3)
                {
                    callback(GetTriangle(vertices, triangleIndices));
                }
            }
        }
    }
}
