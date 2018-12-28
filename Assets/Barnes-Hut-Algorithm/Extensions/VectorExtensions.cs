using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Barnes_Hut_Algorithm.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 Projection(Vector3 a, Vector3 b)
        {
            return Vector3.Dot(a, b) / Vector3.Dot(b, b) * b;
        }

        public static Vector3 Rejection(Vector3 a, Vector3 b)
        {
            return a - Projection(a, b);
        }
    }
}
