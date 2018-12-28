using Assets.Barnes_Hut_Algorithm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NBodyPhysics
{
    // types additions
    /// <summary>
    /// Specifies the system type to generate. 
    /// </summary>
    public enum SystemType { None, SlowParticles, FastParticles, MassiveBody, OrbitalSystem, BinarySystem, PlanetarySystem, DistributionTest, Stock };

    public class World
    {
        /// <summary>
        /// The number of frames elapsed in the simulation. 
        /// </summary>
        public long Frames
        {
            get;
            private set;
        }

        /// <summary>
        /// The lock for modifying the bodies collection. 
        /// </summary>
        private readonly System.Object _bodyLock = new System.Object();

        public GameObject bodyPrefab;

        private List<Body> bodies = new List<Body>();

        public World(GameObject prefab)
        {
            bodyPrefab = prefab;
        }

        /// <summary>
        /// Generates the specified gravitational system. 
        /// </summary>
        /// <param name="type">The system type to generate.</param>
        public void Generate(SystemType type)
        {
            // Reset frames elapsed. 
            Frames = 0;

            lock (_bodyLock)
            {
                switch (type)
                {

                    // Clear bodies. 
                    case SystemType.None:
                        //Array.Clear(bodies, 0, bodies.Length);
                        bodies.Clear();
                        break;

                    // Generate slow particles. 
                    case SystemType.SlowParticles:
                        {
                            for (int i = 0; i < bodies.Count; i++)
                            {
                                var distance = PseudoRandom.Float(1e6f);
                                double angle = PseudoRandom.Double(Math.PI * 2);
                                var location = new Vector3((float)Math.Cos(angle) * distance, PseudoRandom.Float(-2e5f, 2e5f), (float)Math.Sin(angle) * distance);
                                double mass = PseudoRandom.Double(1e6) + 3e4;
                                //bodies[i] = new Body(location, mass, velocity);
                                var dotGO = GameObject.Instantiate(bodyPrefab, location, Quaternion.identity) as GameObject;
                                dotGO.GetComponent<Rigidbody>().mass = (float)mass;
                                bodies[i] = new Body(dotGO);
                            }
                        }
                        break;

                    // Generate fast particles. 
                    //case SystemType.FastParticles:
                    //    {
                    //        for (int i = 0; i < bodies.Count; i++)
                    //        {
                    //            var distance = (float)PseudoRandom.Double(1e6);
                    //            var angle = (float)PseudoRandom.Double(Math.PI * 2);
                    //            var location = new Vector3(Math.Cos(angle) * distance, PseudoRandom.Double(-2e5, 2e5), Math.Sin(angle) * distance);
                    //            double mass = PseudoRandom.Double(1e6) + 3e4;
                    //            var velocity = PseudoRandom.Vector(5e3);
                    //            bodies[i] = new Body(location, mass, velocity);
                    //        }
                    //    }
                    //    break;

                    // Generate massive body demonstration. 
                    //case SystemType.MassiveBody:
                    //    {
                    //        bodies[0] = new Body(Vector3.zero, 1e10);

                    //        var location1 = PseudoRandom.Vector(8e3) + new Vector3(-3e5, 1e5 + bodies[0].Radius, 0);
                    //        double mass1 = 1e6;
                    //        Vector velocity1 = new Vector(2e3, 0, 0);
                    //        bodies[1] = new Body(location1, mass1, velocity1);

                    //        for (int i = 2; i < bodies.Count; i++)
                    //        {
                    //            double distance = PseudoRandom.Double(2e5) + bodies[1].Radius;
                    //            double angle = PseudoRandom.Double(Math.PI * 2);
                    //            double vertical = Math.Min(2e8 / distance, 2e4);
                    //            Vector location = (new Vector(Math.Cos(angle) * distance, PseudoRandom.Double(-vertical, vertical), Math.Sin(angle) * distance) + bodies[1].Location);
                    //            double mass = PseudoRandom.Double(5e5) + 1e5;
                    //            double speed = Math.Sqrt(bodies[1].Mass * bodies[1].Mass * G / ((bodies[1].Mass + mass) * distance));
                    //            Vector velocity = Vector.Cross(location, Vector.YAxis).Unit() * speed + velocity1;
                    //            location = location.Rotate(0, 0, 0, 1, 1, 1, Math.PI * 0.1);
                    //            velocity = velocity.Rotate(0, 0, 0, 1, 1, 1, Math.PI * 0.1);
                    //            bodies[i] = new Body(location, mass, velocity);
                    //        }
                    //    }
                    //    break;

                    // Generate orbital system. 
                    //case SystemType.OrbitalSystem:
                    //    {
                    //        bodies[0] = new Body(1e10);

                    //        for (int i = 1; i < bodies.Count; i++)
                    //        {
                    //            double distance = PseudoRandom.Double(1e6) + bodies[0].Radius;
                    //            double angle = PseudoRandom.Double(Math.PI * 2);
                    //            var location = new Vector(Math.Cos(angle) * distance, PseudoRandom.Double(-2e4, 2e4), Math.Sin(angle) * distance);
                    //            double mass = PseudoRandom.Double(1e6) + 3e4;
                    //            double speed = Math.Sqrt(bodies[0].Mass * bodies[0].Mass * G / ((bodies[0].Mass + mass) * distance));
                    //            var velocity = Vector.Cross(location, Vector.YAxis).Unit() * speed;
                    //            bodies[i] = new Body(location, mass, velocity);
                    //        }
                    //    }
                    //    break;

                    // Generate binary system. 
                    //case SystemType.BinarySystem:
                    //    {
                    //        double mass1 = PseudoRandom.Double(9e9) + 1e9;
                    //        double mass2 = PseudoRandom.Double(9e9) + 1e9;
                    //        double angle0 = PseudoRandom.Double(Math.PI * 2);
                    //        double distance0 = PseudoRandom.Double(1e5) + 3e4;
                    //        double distance1 = distance0 / 2;
                    //        double distance2 = distance0 / 2;
                    //        Vector location1 = new Vector(Math.Cos(angle0) * distance1, 0, Math.Sin(angle0) * distance1);
                    //        Vector location2 = new Vector(-Math.Cos(angle0) * distance2, 0, -Math.Sin(angle0) * distance2);
                    //        double speed1 = Math.Sqrt(mass2 * mass2 * G / ((mass1 + mass2) * distance0));
                    //        double speed2 = Math.Sqrt(mass1 * mass1 * G / ((mass1 + mass2) * distance0));
                    //        Vector velocity1 = Vector.Cross(location1, Vector.YAxis).Unit() * speed1;
                    //        Vector velocity2 = Vector.Cross(location2, Vector.YAxis).Unit() * speed2;
                    //        bodies[0] = new Body(location1, mass1, velocity1);
                    //        bodies[1] = new Body(location2, mass2, velocity2);

                    //        for (int i = 2; i < bodies.Length; i++)
                    //        {
                    //            double distance = PseudoRandom.Double(1e6);
                    //            double angle = PseudoRandom.Double(Math.PI * 2);
                    //            Vector location = new Vector(Math.Cos(angle) * distance, PseudoRandom.Double(-2e4, 2e4), Math.Sin(angle) * distance);
                    //            double mass = PseudoRandom.Double(1e6) + 3e4;
                    //            double speed = Math.Sqrt((mass1 + mass2) * (mass1 + mass2) * G / ((mass1 + mass2 + mass) * distance));
                    //            speed /= distance >= distance0 / 2 ? 1 : (distance0 / 2 / distance);
                    //            Vector velocity = Vector.Cross(location, Vector.YAxis).Unit() * speed;
                    //            bodies[i] = new Body(location, mass, velocity);
                    //        }
                    //    }
                    //    break;

                    // Generate planetary system. 
                    //case SystemType.PlanetarySystem:
                    //    {
                    //        bodies[0] = new Body(1e10);
                    //        int planets = PseudoRandom.Int32(10) + 5;
                    //        int planetsWithRings = PseudoRandom.Int32(1) + 1;
                    //        int k = 1;
                    //        for (int i = 1; i < planets + 1 && k < bodies.Length; i++)
                    //        {
                    //            int planetK = k;
                    //            double distance = PseudoRandom.Double(2e6) + 1e5 + bodies[0].Radius;
                    //            double angle = PseudoRandom.Double(Math.PI * 2);
                    //            Vector location = new Vector(Math.Cos(angle) * distance, PseudoRandom.Double(-2e4, 2e4), Math.Sin(angle) * distance);
                    //            double mass = PseudoRandom.Double(1e8) + 1e7;
                    //            double speed = Math.Sqrt(bodies[0].Mass * bodies[0].Mass * G / ((bodies[0].Mass + mass) * distance));
                    //            Vector velocity = Vector.Cross(location, Vector.YAxis).Unit() * speed;
                    //            bodies[k++] = new Body(location, mass, velocity);

                    //            // Generate rings.
                    //            const int RingParticles = 100;
                    //            if (--planetsWithRings >= 0 && k < bodies.Length - RingParticles)
                    //            {
                    //                for (int j = 0; j < RingParticles; j++)
                    //                {
                    //                    double ringDistance = PseudoRandom.Double(1e1) + 1e4 + bodies[planetK].Radius;
                    //                    double ringAngle = PseudoRandom.Double(Math.PI * 2);
                    //                    Vector ringLocation = location + new Vector(Math.Cos(ringAngle) * ringDistance, 0, Math.Sin(ringAngle) * ringDistance);
                    //                    double ringMass = PseudoRandom.Double(1e3) + 1e3;
                    //                    double ringSpeed = Math.Sqrt(bodies[planetK].Mass * bodies[planetK].Mass * G / ((bodies[planetK].Mass + ringMass) * ringDistance));
                    //                    Vector ringVelocity = Vector.Cross(location - ringLocation, Vector.YAxis).Unit() * ringSpeed + velocity;
                    //                    bodies[k++] = new Body(ringLocation, ringMass, ringVelocity);
                    //                }
                    //                continue;
                    //            }

                    //            // Generate moons. 
                    //            int moons = PseudoRandom.Int32(4);
                    //            while (moons-- > 0 && k < bodies.Length)
                    //            {
                    //                double moonDistance = PseudoRandom.Double(1e4) + 5e3 + bodies[planetK].Radius;
                    //                double moonAngle = PseudoRandom.Double(Math.PI * 2);
                    //                Vector moonLocation = location + new Vector(Math.Cos(moonAngle) * moonDistance, PseudoRandom.Double(-2e3, 2e3), Math.Sin(moonAngle) * moonDistance);
                    //                double moonMass = PseudoRandom.Double(1e6) + 1e5;
                    //                double moonSpeed = Math.Sqrt(bodies[planetK].Mass * bodies[planetK].Mass * G / ((bodies[planetK].Mass + moonMass) * moonDistance));
                    //                Vector moonVelocity = Vector.Cross(moonLocation - location, Vector.YAxis).Unit() * moonSpeed + velocity;
                    //                bodies[k++] = new Body(moonLocation, moonMass, moonVelocity);
                    //            }
                    //        }

                    //    // Generate asteroid belt.
                    //    while (k < bodies.Length)
                    //    {
                    //        double asteroidDistance = PseudoRandom.Double(4e5) + 1e6;
                    //        double asteroidAngle = PseudoRandom.Double(Math.PI * 2);
                    //        Vector asteroidLocation = new Vector(Math.Cos(asteroidAngle) * asteroidDistance, PseudoRandom.Double(-1e3, 1e3), Math.Sin(asteroidAngle) * asteroidDistance);
                    //        double asteroidMass = PseudoRandom.Double(1e6) + 3e4;
                    //        double asteroidSpeed = Math.Sqrt(bodies[0].Mass * bodies[0].Mass * G / ((bodies[0].Mass + asteroidMass) * asteroidDistance));
                    //        Vector asteroidVelocity = Vector.Cross(asteroidLocation, Vector.YAxis).Unit() * asteroidSpeed;
                    //        bodies[k++] = new Body(asteroidLocation, asteroidMass, asteroidVelocity);
                    //    }
                    //}
                    //break;

                    // Generate distribution test. 
                    case SystemType.DistributionTest:
                        {
                            bodies.Clear();
                            double distance = 4e4;
                            double mass = 5e6;

                            int side = (int)Math.Pow(bodies.Count, 1.0 / 3);
                            int k = 0;
                            for (int a = 0; a < side; a++)
                                for (int b = 0; b < side; b++)
                                    for (int c = 0; c < side; c++)
                                    {
                                        var location = new Vector3((float)distance * (a - side / 2), (float)distance * (b - side / 2), (float)distance * (c - side / 2));
                                        var dotGO = GameObject.Instantiate(bodyPrefab, location, Quaternion.identity) as GameObject;
                                        dotGO.GetComponent<Rigidbody>().mass = (float)mass;
                                        bodies[k++] = new Body(dotGO);
                                    }
                        }
                        break;
                }
            }
        }
    }

}
