using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Barnes_Hut_Algorithm
{
    public static class PseudoRandom
    {
        private static ulong seed = (ulong)DateTime.Now.ToFileTime();

        static PseudoRandom()
        {
            for (int index = 0; index < 10; ++index)
            {
                long num = (long)PseudoRandom.UInt64();
            }
        }

        public static ulong UInt64()
        {
            PseudoRandom.seed ^= PseudoRandom.seed << 13;
            PseudoRandom.seed ^= PseudoRandom.seed >> 7;
            PseudoRandom.seed ^= PseudoRandom.seed << 17;
            return PseudoRandom.seed;
        }

        public static double Double()
        {
            return (double)PseudoRandom.UInt64() * 5.42101086242752E-20;
        }

        public static double Double(double a, double b = 0.0)
        {
            return a + PseudoRandom.Double() * (b - a);
        }

        public static float Float()
        {
            return (float)(PseudoRandom.UInt64() * 5.42101086242752E-20);
        }

        public static float Float(float a, float b = 0.0f)
        {
            return a + PseudoRandom.Float() * (b - a);
        }

        public static int Int32(int a, int b = 0)
        {
            double num = 0.5 * (double)Math.Sign((double)a - (double)b);
            return (int)Math.Round(PseudoRandom.Double((double)a + num, (double)b - num));
        }

        public static bool Boolean()
        {
            return ((long)PseudoRandom.UInt64() & 1L) == 0L;
        }

        public static Vector3 Vector(float maximumMagnitude = 1.0f)
        {
            return PseudoRandom.Float(maximumMagnitude, 0.0f) * PseudoRandom.DirectionVector(1.0f);
        }

        public static Vector3 DirectionVector(float magnitude = 1.0f)
        {
            Vector3 vector;
            do
            {
                vector = new Vector3(PseudoRandom.Float(-1.0f, 1.0f), PseudoRandom.Float(-1.0f, 1.0f), PseudoRandom.Float(-1.0f, 1.0f));
            }
            while (vector.magnitude == 0.0);
            return magnitude / vector.magnitude * vector;
        }

        //public static Color Hue(Color baseColour, double randomness)
        //{
        //    double[] hsl = ColorConverter.ColorToHSL(baseColour);
        //    hsl[0] += PseudoRandom.Double(-randomness, randomness);
        //    hsl[0] = hsl[0] > 1.0 ? hsl[0] - 1.0 : (hsl[0] < 0.0 ? hsl[0] + 1.0 : hsl[0]);
        //    return ColorConverter.HSLToColor(hsl);
        //}
    }
}
