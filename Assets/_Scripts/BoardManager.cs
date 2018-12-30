using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Assets.Barnes_Hut_Algorithm;
using System.Threading;
using Assets.Barnes_Hut_Algorithm.Extensions;
using NBodyUniverse;

public class BoardManager : MonoBehaviour
{
    public GameObject bodyPrefab;
    public SystemType systemType = SystemType.Stock;
    public int nodeCountLimit = 16;
    public bool showCenterMass = false;
    public GameObject centerMassNode;

    /// <summary>
    /// The gravitational constant. 
    /// </summary>
    public static double G = 67;
    /// <summary>
    /// The maximum speed. 
    /// </summary>
    public static double C = 1e4;

    private List<Body> bodies = new List<Body> ();
	private bool compute;
	private bool displayQuad;
	private bool bruteForce;
	private int size;
	private double framecount = 0;
	private Boundary boundary;
	private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch ();
	private NodeTree nodeTree;
    private List<Quadrant> quads = new List<Quadrant>();
    private CameraScript cameraScript;


    void Start ()
	{
        ResetSim();
    }

	// Update is called once per frame
	void Update ()
	{
        HandleUserInput();

		boundary.update (bodies);

		stopwatch.Start();
        Simulate();
        stopwatch.Stop();
		stopwatch.Reset();
	}

    public void ResetSim()
    {
        boundary = new Boundary(nodeCountLimit);
        compute = false;
        bruteForce = false;
        displayQuad = false;
        size = bodies.Count;
        systemType = SystemType.Stock;
        nodeCountLimit = 16;
        cameraScript = Camera.main.GetComponent<CameraScript>();
        bodies.ForEach(b => b.RemoveObject());
        centerMassNode = GameObject.Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity);
        centerMassNode.GetComponent<SpriteRenderer>().color = Color.grey;
        centerMassNode.SetActive(false);
        centerMassNode.GetComponent<TrailRenderer>().enabled = false;
    }

    /// <summary>
    /// Advances the simulation by one frame if it is active. 
    /// </summary>
    private void Simulate()
    {
        if (compute)
        {
            lock (_bodyLock)
            {

                // Update the bodies and determine the required tree width. 
                double halfWidth = 0;
                foreach (Body body in bodies)
                {
                    if (body != null)
                    {
                        body.Update();
                        halfWidth = Math.Max(Math.Abs(body.position.x), halfWidth);
                        halfWidth = Math.Max(Math.Abs(body.position.y), halfWidth);
                        halfWidth = Math.Max(Math.Abs(body.position.z), halfWidth);
                    }
                }

                // Initialize the root tree and add the bodies. The root tree needs to be 
                // slightly larger than twice the determined half width. 
                nodeTree = new NodeTree(2.1 * halfWidth);
                foreach (Body body in bodies)
                {
                    if (body != null)
                    {
                        nodeTree.Add(body);
                    }
                }

                // Accelerate the bodies
                foreach (var body in bodies)
                {
                    if (body != null)
                    {
                        nodeTree.Accelerate(body);
                    }
                }

                // Update frame counter. 
                if (nodeTree.bodyCount > 0)
                {
                    centerMassNode.transform.position = nodeTree._centerOfMass;
                    
                    framecount++;
                }
            }
        }
    }

	private void HandleUserInput ()
	{
		
		//framecount++;
	}

	public void OnDrawGizmos ()
	{
		if (displayQuad) {
			nodeTree.GetAllQuads(quads);
			foreach (Quadrant quad in quads) {
				Gizmos.color = quad.color;
				Gizmos.DrawWireCube (quad.position, quad.size);
				if(quad.gravityCenter != Vector3.zero)
		 			Gizmos.DrawSphere(quad.gravityCenter,quad.mass);
			}
			quads.Clear ();
		}
	}

    private readonly System.Object _bodyLock = new System.Object();

    public void HandleButtonGenerate()
    {
        ResetSim();
        var selectedType = GetComponent<UIController>().SystemType;
        Generate(selectedType);
    }
    public void HandleButtonStatus()
    {
        compute = !compute;
    }
    /// <summary>
    /// Generates the specified gravitational system. 
    /// </summary>
    /// <param name="type">The system type to generate.</param>
    public void Generate(SystemType type)
    {
        // Reset frames elapsed. 
        framecount = 0;
        cameraScript.followObject = null;
        centerMassNode.SetActive(false);

        lock (_bodyLock)
        {
            bodies.Clear();
            switch (type)
            {

                // Clear bodies. 
                case SystemType.None:
                    bodies.Clear();
                    break;
                case SystemType.Stock:
                    var rowLimit = (int)Math.Ceiling(Math.Sqrt(nodeCountLimit))/2;
                    for (int i = -1*rowLimit; i < rowLimit; i++)
                    {
                        for (int j = -1* rowLimit; j < rowLimit; j++)
                        {
                            var theMass = UnityEngine.Random.Range(1f, 8f);
                            var dotGO = Instantiate(bodyPrefab, new Vector3(i * 2, j * 2, 0), Quaternion.identity) as GameObject;
                            dotGO.transform.localScale = new Vector3(1 / theMass, 1 / theMass, 1 / theMass);
                            bodies.Add(new Body(dotGO,Vector3.zero,theMass));
                        }
                    }
                    break;

                // Generate slow particles. 
                case SystemType.SlowParticles:
                    {
                        for (int i = 0; i < bodies.Count; i++)
                        {
                            var distance = PseudoRandom.Float(1e6f);
                            double angle = PseudoRandom.Double(Math.PI * 2);
                            var location = new Vector3((float)Math.Cos(angle) * distance, PseudoRandom.Float(-2e5f, 2e5f), (float)Math.Sin(angle) * distance);
                            var dotGO = GameObject.Instantiate(bodyPrefab, location, Quaternion.identity) as GameObject;
                            bodies[i] = new Body(dotGO, Vector3.zero);
                        }
                    }
                    break;

                // Generate fast particles. 
                case SystemType.FastParticles:
                    {
                        for (int i = 0; i < bodies.Count; i++)
                        {
                            var distance = PseudoRandom.Float(1e6f);
                            double angle = PseudoRandom.Double(Math.PI * 2);
                            var location = new Vector3((float)Math.Cos(angle) * distance, PseudoRandom.Float(-2e5f, 2e5f), (float)Math.Sin(angle) * distance);
                            double mass = PseudoRandom.Double(1e6) + 3e4;
                            var velocity = PseudoRandom.Vector(5e3f);
                            var dotGO = GameObject.Instantiate(bodyPrefab, location, Quaternion.identity) as GameObject;
                            bodies[i] = new Body(dotGO, velocity, (float)mass);
                        }
                    }
                    break;

                // Generate massive body demonstration. 
                case SystemType.MassiveBody:
                    {
                        bodies[0] = new Body(GameObject.Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity), Vector3.zero, 1e10f);

                        var location1 = PseudoRandom.Vector(8e3f) + new Vector3(-3e5f, 1e5f + (float)bodies[0].Radius, 0);
                        double mass1 = 1e6;
                        var velocity1 = new Vector3(2e3f, 0, 0);
                        var dotGO = GameObject.Instantiate(bodyPrefab, location1, Quaternion.identity) as GameObject;
                        bodies[1] = new Body(dotGO, velocity1, (float)mass1);

                        for (int i = 2; i < bodies.Count; i++)
                        {
                            var distance = PseudoRandom.Float(2e5f) + bodies[1].Radius;
                            double angle = PseudoRandom.Double(Math.PI * 2);
                            var vertical = (float)Math.Min(2e8 / distance, 2e4);
                            var location = (new Vector3((float)(Math.Cos(angle) * distance), PseudoRandom.Float(-vertical, vertical), (float)(Math.Sin(angle) * distance)) + bodies[1].position);
                            double mass = PseudoRandom.Double(5e5) + 1e5;
                            double speed = Math.Sqrt(bodies[1].mass * bodies[1].mass * G / ((bodies[1].mass + mass) * distance));
                            var velocity = Vector3.Cross(location, new Vector3(0f, 0f, 1f)) / Vector3.Cross(location, new Vector3(0f, 0f, 1f)).magnitude * (float)speed + velocity1;
                            location = location.Rotate(0, 0, 0, 1, 1, 1, (float)(Math.PI * 0.1));
                            velocity = velocity.Rotate(0, 0, 0, 1, 1, 1, (float)(Math.PI * 0.1));
                            dotGO = GameObject.Instantiate(bodyPrefab, location1, Quaternion.identity) as GameObject;
                            bodies[i] = new Body(dotGO, velocity, (float)mass);
                        }
                    }
                    break;

                // Generate orbital system. 
                case SystemType.OrbitalSystem:
                    {
                        var mainBodyScale = new Vector3(5, 5, 5);
                        var centerTotalMass = 3000;
                        var centerBody = GameObject.Instantiate(bodyPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                        centerBody.transform.localScale = mainBodyScale;
                        bodies.Add(new Body(centerBody, Vector3.zero, centerTotalMass));
                        cameraScript.followObject = centerBody.transform;

                        for (int i = 1; i < nodeCountLimit; i++)
                        {
                            var distance = (PseudoRandom.Float(20f) + bodies[0].Radius);
                            var angle = (float)PseudoRandom.Double(Math.PI * 2);
                            var location = new Vector3((float)(Math.Cos(angle) * distance), PseudoRandom.Float(-20f, 20f), (float)(Math.Sin(angle) * distance));
                            var mass = UnityEngine.Random.Range(15f, 50f);
                            var velocity = Vector3.Cross(location, new Vector3(0f, 0f, 1f)) / Vector3.Cross(location, new Vector3(0f, 0f, 1f)).magnitude;
                            var dotGO = GameObject.Instantiate(bodyPrefab, location, Quaternion.identity) as GameObject;
                            var goScale = mass / 50;
                            dotGO.transform.localScale = new Vector3(goScale,goScale,goScale);
                            
                            bodies.Add(new Body(dotGO,velocity,(float)mass));
                        }
                    }
                    break;

                // Generate binary system. 
                case SystemType.BinarySystem:
                    {
                        centerMassNode.SetActive(true);
                        cameraScript.followObject = centerMassNode.transform;
                        var mainBodyScale = new Vector3(3, 3, 3);
                        var mass1 = PseudoRandom.Float(900) + 100;
                        var mass2 = PseudoRandom.Float(900) + 100;
                        var angle0 = (float)PseudoRandom.Double(Math.PI * 2);
                        var distance0 = PseudoRandom.Float(10) + 30;
                        var distance1 = distance0 / 2;
                        var distance2 = distance0 / 2;
                        var location1 = new Vector3((float)Math.Cos(angle0) * distance1, 0, (float)Math.Sin(angle0) * distance1);
                        var location2 = new Vector3((float)-Math.Cos(angle0) * distance2, 0, (float)-Math.Sin(angle0) * distance2);
                        var velocity1 = Vector3.Cross(location1, new Vector3(0f, 0f, 1f)) / Vector3.Cross(location1, new Vector3(0f, 0f, 1f)).magnitude;
                        var velocity2 = Vector3.Cross(location2, new Vector3(0f, 0f, 1f)) / Vector3.Cross(location2, new Vector3(0f, 0f, 1f)).magnitude;
                        var dotGO = GameObject.Instantiate(bodyPrefab, location1, Quaternion.identity) as GameObject;
                        dotGO.transform.localScale = mainBodyScale;
                        bodies.Add(new Body(dotGO, velocity1, mass1));
                        dotGO = GameObject.Instantiate(bodyPrefab, location2, Quaternion.identity) as GameObject;
                        dotGO.transform.localScale = mainBodyScale;
                        bodies.Add(new Body(dotGO, velocity2, mass2));

                        for (int i = 2; i < nodeCountLimit; i++)
                        {
                            var distance = PseudoRandom.Float(100);
                            var angle = (float)PseudoRandom.Double(Math.PI * 2);
                            var location = new Vector3((float)Math.Cos(angle) * distance, PseudoRandom.Float(-20f, 20f), (float)Math.Sin(angle) * distance);
                            var mass = PseudoRandom.Float(10f) + 5;
                            var speed = Math.Sqrt((mass1 + mass2) * (mass1 + mass2) * G / ((mass1 + mass2 + mass) * distance));
                            speed /= distance >= distance0 / 2 ? 1 : (distance0 / 2 / distance);
                            var velocity = Vector3.Cross(location, new Vector3(0f, 0f, 1f)) / Vector3.Cross(location, new Vector3(0f, 0f, 1f)).magnitude;
                            dotGO = GameObject.Instantiate(bodyPrefab, location, Quaternion.identity) as GameObject;
                            var goScale = mass / 15;
                            dotGO.transform.localScale = new Vector3(goScale, goScale, goScale);
                            bodies.Add(new Body(dotGO, velocity, mass));
                        }
                    }
                    break;
            }
        }
    }
}






