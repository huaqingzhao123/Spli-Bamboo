using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace hbj
{
    public class MathfUtil
    {

        /// <summary>
        /// 叉乘(p1p2*p1p)  aXb=x1y2-x2y1
        /// </summary>
        public static float GetVector2Cross(Vector2 middleP, Vector2 leftP, Vector2 rigthP)
        {
            //  aXb=x1y2-x2y1
            return (leftP.x - middleP.x) * (rigthP.y - middleP.y) - (rigthP.x - middleP.x) * (leftP.y - middleP.y);
        }
        /// <summary>
        /// 判断点是否在矩形内
        /// </summary>
        /// <returns></returns>
        public static bool IsVector2PointInMatrix(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p)
        {
            return GetVector2Cross(p1, p2, p) * GetVector2Cross(p3, p4, p) >= 0 && GetVector2Cross(p2, p3, p) * GetVector2Cross(p4, p1, p) >= 0;
        }

        /// <summary>
        /// 判断点是否在三角形内
        /// </summary>
        /// <returns></returns>
        public static bool Vector2IsInTriangle(Vector2 p1, Vector2 p2, Vector2 p3,Vector2 judgeP)
        {
            var sum = CalculateTriangle(p1, p2, p3);
            var s1 = CalculateTriangle(judgeP, p2, p3);
            var s2 = CalculateTriangle(judgeP, p1, p3);
            var s3 = CalculateTriangle(judgeP, p1, p2);
            var res = s1 + s2 + s3;
            //误差0.5
            var inArea = res >= sum - 0.5f && res <= sum + 0.5f;
            //var inArea = (s1 + s2 + s3)==sum ;
            return inArea;
        }

        /// <summary>
        /// 计算三角形面积，传入三边长度,海伦计算法
        /// </summary>
        /// <returns></returns>
        public static float CalculateTriangle(float a,float b,float c)
        {
            //S= Mathf.Sqrt(p(p−a)(p−b)(p−c));
            var average = (a + b + c) / 2;
            return Mathf.Sqrt(average*(average-a) * (average-b) * (average-c));
        }
        /// <summary>
        /// 计算三角形面积，叉乘计算,传入三点
        /// </summary>
        /// <returns></returns>
        public static float CalculateTriangle(Vector2 p1,Vector2 p2,Vector2 p3)
        {
            //叉乘计算的是平行四边形面积所以除以2
            return Mathf.Abs(GetVector2Cross(p1,p2,p3)/2);
        }
    }
}

