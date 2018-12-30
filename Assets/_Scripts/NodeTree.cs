using NBodyUniverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Barnes_Hut_Algorithm
{
    public class NodeTree : INodeTree
    {
        private Body averageBody = null;
        public int bodyCount = 0;

        private Vector3 center;
        private float size;
        private int level;

        private int MAX_OBJECTS = 1;
        private int MAX_LEVELS = 15;

        /// <summary>
        /// The width of the tree's bounds. 
        /// </summary>
        private double _width = 0;

        /// <summary>
        /// The location of the center of TotalMass of the bodies contained in the tree. 
        /// </summary>
        private Vector3 _centerOfMass = Vector3.zero;

        /// <summary>
        /// The tolerance of the TotalMass grouping approximation in the simulation. A 
        /// body is only accelerated when the ratio of the tree's width to the 
        /// distance (from the tree's center of TotalMass to the body) is less than this.
        /// </summary>
        private const double Tolerance = 0.5;

        /// <summary>
        /// The softening factor for the acceleration equation. This dampens the 
        /// the slingshot effect during close encounters of bodies. 
        /// </summary>
        private const double Epsilon = 700;

        /// <summary>
        /// The minimum width of a tree. Subtrees are not created when if their width 
        /// would be smaller than this value. 
        /// </summary>
        private const double MinimumWidth = 1;

        /// <summary>
        /// The total TotalMass of the bodies contained in the tree. 
        /// </summary>
        public double TotalMass = 0;

        /// <summary>
        /// The first body added to the tree. This is used when the first Body must 
        /// be added to subtrees at a later time. 
        /// </summary>
        private Body _firstBody = null;

        private NodeTree[] subTrees;

        public NodeTree()
        {

        }
        /// <summary>
        /// Constructs a tree with the given width located about the origin.
        /// </summary>
        /// <param name="width">The width of the new tree.</param>
        public NodeTree(double width)
        {
            _width = width;
        }

        /// <summary>
        /// Constructs a tree with the given location and width.
        /// </summary>
        /// <param name="location">The location of the center of the new tree.</param>
        /// <param name="width">The width of the new tree.</param>
        public NodeTree(Vector3 location, double width)
            : this(width)
        {
            center = _centerOfMass = location;
        }

        public void Accelerate(Body body)
        {
            double dx = _centerOfMass.x - body.position.x;
            double dy = _centerOfMass.y - body.position.y;
            double dz = _centerOfMass.z - body.position.z;
            double dSquared = dx * dx + dy * dy + dz * dz;

            // Case 1. The tree contains only one body and it is not the one in the 
            //         tree so we can perform the acceleration. 
            //
            // Case 2. The width to distance ratio is within the defined tolerance so 
            //         we consider the tree to be effectively a single TotalMassive body and 
            //         perform the acceleration. 
            if ((bodyCount == 1 && body != _firstBody) || (_width * _width < Tolerance * Tolerance * dSquared))
            {

                // Calculate a normalized acceleration value and multiply it with the 
                // displacement in each coordinate to get that coordinate's acceleration 
                // component. 
                double distance = Math.Sqrt(dSquared + Epsilon * Epsilon);
                double normAcc = Universe.G * TotalMass / (distance * distance * distance);

                body.acceleration.x += (float)(normAcc * dx);
                body.acceleration.y += (float)(normAcc * dy);
                body.acceleration.z += (float)(normAcc * dz);
            }

            // Case 3. More granularity is needed so we accelerate at the subtrees. 
            else if (subTrees != null)
            {
                foreach (NodeTree subtree in subTrees)
                {
                    if (subtree != null)
                    {
                        subtree.Accelerate(body);
                    }
                }
            }
        }

        public void Add(Body body)
        {
            _centerOfMass = ((float)TotalMass * _centerOfMass + body.mass * body.position) / (float)(TotalMass + body.mass);
            TotalMass += body.mass;
            bodyCount++;
            if (bodyCount == 1)
                _firstBody = body;
            else
            {
                AddToSubtree(body);
                if (bodyCount == 2)
                    AddToSubtree(_firstBody);
            }
        }

        public void AddToSubtree(Body body)
        {
            double subtreeWidth = _width / 2;

            // Don't create subtrees if it violates the width limit.
            if (subtreeWidth < MinimumWidth)
                return;

            if (subTrees == null)
                subTrees = new NodeTree[8];

            // Determine which subtree the body belongs in and add it to that subtree. 
            int subtreeIndex = 0;
            for (int i = -1; i <= 1; i += 2)
                for (int j = -1; j <= 1; j += 2)
                    for (int k = -1; k <= 1; k += 2)
                    {
                        Vector3 subtreeLocation = center + (float)(subtreeWidth / 2) * new Vector3(i, j, k);

                        // Determine if the body is contained within the bounds of the subtree under 
                        // consideration. 
                        if (Math.Abs(subtreeLocation.x - body.position.x) <= subtreeWidth / 2
                         && Math.Abs(subtreeLocation.y - body.position.y) <= subtreeWidth / 2
                         && Math.Abs(subtreeLocation.z - body.position.z) <= subtreeWidth / 2)
                        {

                            if (subTrees[subtreeIndex] == null)
                                subTrees[subtreeIndex] = new NodeTree(subtreeLocation, subtreeWidth);
                            subTrees[subtreeIndex].Add(body);
                            return;
                        }
                        subtreeIndex++;
                    }
        }

        // convert to generic oct/quad, maybe just rename?
        public void GetAllQuads(List<Quadrant> quads)
        {
            if (averageBody == null)
            {
                quads.Add(new Quadrant(center, size, level, Vector3.zero, 0));
            }
            else
            {
                quads.Add(new Quadrant(center, size, level, averageBody.position, averageBody.mass));
            }

            if (subTrees[0] == null)
            {
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                subTrees[i].GetAllQuads(quads);
            }
        }
    }
}
