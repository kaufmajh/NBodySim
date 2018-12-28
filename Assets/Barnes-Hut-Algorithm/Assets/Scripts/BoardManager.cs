using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Assets.Barnes_Hut_Algorithm;
using NBodyPhysics;
using System.Threading;

public class BoardManager : MonoBehaviour
{
	public GameObject bodyPrefab;
    public SystemType systemType = SystemType.Stock;
    public int nodeCountLimit = 16;

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
	private List<Quad> quads = new List<Quad>();

	void Start ()
	{
		boundary = new Boundary (1000);
		compute = false;
		bruteForce = false;
		displayQuad = false;
		size = bodies.Count;
        systemType = SystemType.Stock;
        nodeCountLimit = 16;
        
    }

	// Update is called once per frame
	void Update ()
	{
        HandleUserInput();

		boundary.update (bodies);
		float sized = Mathf.Max ((boundary.max.x - boundary.min.x), (boundary.max.y - boundary.min.y));
		Vector3 center = new Vector3 ((boundary.max.x + boundary.min.x) / 2, (boundary.max.y + boundary.min.y) / 2, (boundary.max.z + boundary.min.z) / 2);
        //nodeTree = new NodeTree (1, center, sized);

		stopwatch.Start();
        Simulate();
        stopwatch.Stop();
		stopwatch.Reset();
	}

    /// <summary>
    /// Advances the simulation by one frame if it is active. 
    /// </summary>
    private void Simulate()
    {
        if (compute)
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
                    if (body != null)
                        nodeTree.Add(body);
                
                // Accelerate the bodies in parallel. 
                //Parallel.ForEach(_bodies, body => {
                //    if (body != null)
                //        _tree.Accelerate(body);
                //});
                foreach(var body in bodies)
                {
                    if(body != null)
                    {
                        nodeTree.Accelerate(body);
                    }
                }

                // Update frame counter. 
                if (nodeTree.bodyCount > 0)
                    framecount++;
            }

        // Update the camera. 
        //_cameraZ += _cameraZVelocity * _cameraZ;
        //_cameraZ = Math.Max(1, _cameraZ);
        //_cameraZVelocity *= CameraZEasing;
        //_renderer.Camera.Z = _cameraZ;

        // Sleep for the necessary time. 
        //int elapsed = (int)_stopwatch.ElapsedMilliseconds;
        //if (elapsed < FrameInterval)
        //{
        //    Thread.Sleep(FrameInterval - elapsed);
        //}

        // Update the simulation FPS.
        //_stopwatch.Stop();
        //Fps += (1000.0 / _stopwatch.Elapsed.TotalMilliseconds - Fps) * FpsEasing;
        //Fps = Math.Min(Fps, FpsMax);
        //_stopwatch.Reset();
        //_stopwatch.Start();
    }

	private void HandleUserInput ()
	{
		if (Input.GetKeyDown ("b"))
			displayQuad = !displayQuad;

		if (Input.GetKeyDown ("n"))
			bruteForce = !bruteForce;


		//if (Input.GetButtonDown ("Fire1")) {
		//	//circle = !circle;
		//	Vector3 mouse = Input.mousePosition;
		//	mouse = Camera.main.ScreenToWorldPoint (mouse);
		//	mouse.z = 0;
		//	GameObject dotGO = Instantiate (bodyPrefab, mouse, Quaternion.identity) as GameObject;
		//	bodies.Add (new Body (dotGO));
		//	size = bodies.Count;
		//}
		framecount++;
		//if (circle) {
		//	GameObject dotGO = Instantiate (bodyPrefab, new Vector3 (Mathf.Cos (Time.time) * dist, Mathf.Sin (Time.time) * dist, 0), Quaternion.identity) as GameObject;
		//	bodies.Add (new Body (dotGO));
		//	dist+=0.05f;
		//	size = bodies.Count;
		//}
	}

	public void OnDrawGizmos ()
	{
		if (displayQuad) {
			nodeTree.GetAllQuads(quads);
			foreach (Quad quad in quads) {
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
        Generate(systemType);
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
                            bodies.Add(new Body(dotGO,theMass));
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
                            double TotalMass = PseudoRandom.Double(1e6) + 3e4;
                            //bodies[i] = new Body(location, TotalMass, velocity);
                            var dotGO = GameObject.Instantiate(bodyPrefab, location, Quaternion.identity) as GameObject;
                            //dotGO.GetComponent<Rigidbody>().TotalMass = (float)TotalMass;
                            bodies[i] = new Body(dotGO);
                        }
                    }
                    break;
                // Generate orbital system. 
                case SystemType.OrbitalSystem:
                    {
                        var scale = .0001f;
                        var centerTotalMass = (float)1e10;
                        var centerBody = GameObject.Instantiate(bodyPrefab, new Vector3(0,0,0), Quaternion.identity) as GameObject;
                        //centerBody.GetComponent<Rigidbody>().TotalMass = centerTotalMass;
                        bodies.Add(new Body(centerBody,centerTotalMass));

                        for (int i = 1; i < nodeCountLimit; i++)
                        {
                            var distance = PseudoRandom.Float(1e6f) + bodies[0].Radius;
                            var angle = (float)PseudoRandom.Double(Math.PI * 2);
                            var location = new Vector3((float)(Math.Cos(angle) * distance) * scale, PseudoRandom.Float(-2e4f, 2e4f) * scale, (float)(Math.Sin(angle) * distance) * scale);
                            var TotalMass = PseudoRandom.Double(1e6) + 3e4;
                            var speed = Math.Sqrt(centerTotalMass * centerTotalMass * G / ((centerTotalMass + TotalMass) * distance));
                            //var velocity = Vector.Cross(location, Vector.YAxis).Unit() * speed;
                            //bodies[i] = new Body(location, TotalMass, velocity);
                            var dotGO = GameObject.Instantiate(bodyPrefab, location, Quaternion.identity) as GameObject;
                            //dotGO.GetComponent<Rigidbody>().TotalMass = (float)TotalMass;
                            bodies.Add(new Body(dotGO,(float)TotalMass));
                        }
                    }
                    break;
            }
        }
    }
}






