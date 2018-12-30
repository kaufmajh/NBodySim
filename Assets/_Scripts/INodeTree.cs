using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Barnes_Hut_Algorithm
{
    interface INodeTree
    {
        void Add(Body body);
        void AddToSubtree(Body body);
        void Accelerate(Body body);
        void GetAllQuads(List<Quadrant> quads);
    }
}
