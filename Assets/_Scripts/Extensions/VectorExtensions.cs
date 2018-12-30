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

        public static Vector3 Rotate(this Vector3 vector,
          float pointX,
          float pointY,
          float pointZ,
          float directionX,
          float directionY,
          float directionZ,
          float angle)
        {
            var num1 = (float)(1.0 / Math.Sqrt(directionX * directionX + directionY * directionY + directionZ * directionZ));
            directionX *= num1;
            directionY *= num1;
            directionZ *= num1;
            var num2 = (float)Math.Cos(angle);
            var num3 = (float)Math.Sin(angle);
            return new Vector3((pointX * (directionY * directionY + directionZ * directionZ) - directionX * (pointY * directionY + pointZ * directionZ - directionX * vector.x - directionY * vector.y - directionZ * vector.z)) * (1.0f - num2) + vector.x * num2 + (-pointZ * directionY + pointY * directionZ - directionZ * vector.y + directionY * vector.z) * num3, 
                (pointY * (directionX * directionX + directionZ * directionZ) - directionY * (pointX * directionX + pointZ * directionZ - directionX * vector.x - directionY * vector.y - directionZ * vector.z)) * (1.0f - num2) + vector.y * num2 + (pointZ * directionX - pointX * directionZ + directionZ * vector.x - directionX * vector.z) * num3, 
                (pointZ * (directionX * directionX + directionY * directionY) - directionZ * (pointX * directionX + pointY * directionY - directionX * vector.x - directionY * vector.y - directionZ * vector.z)) * (1.0f - num2) + vector.z * num2 + (-pointY * directionX + pointX * directionY - directionY * vector.x + directionX * vector.y) * num3);
        }

        public static Vector3 Rotate(this Vector3 vector, Vector3 point, Vector3 direction, float angle)
        {
            return vector.Rotate(point.x, point.y, point.z, direction.x, direction.y, direction.z, angle);
        }
    }
}
