using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NBodyUniverse
{
    public enum SystemType { None, SlowParticles, FastParticles, MassiveBody, OrbitalSystem, BinarySystem, PlanetarySystem, DistributionTest, Stock };

    public static class Universe
    {
        /// <summary>
        /// The gravitational constant. 
        /// </summary>
        public static double G = 67;

        /// <summary>
        /// The maximum speed. 
        /// </summary>
        public static double C = 1e4;

        
    }
}

