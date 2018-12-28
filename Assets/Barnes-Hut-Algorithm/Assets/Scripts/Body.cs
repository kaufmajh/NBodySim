using UnityEngine;
using System.Collections;
using System;
using Assets.Barnes_Hut_Algorithm.Extensions;

public class Body  {

	private GameObject dot;
	public float mass;
	public Vector3 position;
	private Vector3 velocity;
	public Vector3 acceleration;
	private float G = 0.04f;
	public Guid InstanceID {get; private set;}

    /// <summary>
    /// The previous locations of the body in a circular queue. 
    /// </summary>
    private Vector3[] _locationHistory = new Vector3[20];

    /// <summary>
    /// The current index in the location history queue. 
    /// </summary>
    private int _locationHistoryIndex = 0;

    /// <summary>
    /// The radius of the body. 
    /// </summary>
    public double Radius
    {
        get
        {
            return GetRadius(mass);
        }
    }

    public Body(GameObject _dot, float _mass = 2f){
		this.InstanceID = Guid.NewGuid();
		mass = _mass;
		dot = _dot;
		velocity = Vector3.zero;
		acceleration = Vector3.zero;
		if(_dot != null)
			position = CopyVector(_dot.transform.position);
		 else 
			position = Vector3.zero;

        for (int i = 0; i < _locationHistory.Length; i++)
        {
            _locationHistory[i] = position;
        }
    }

	//public void update(){
		//velocity += acceleration;
		//position += velocity;
		//acceleration = Vector3.zero;

		//dot.transform.position = CopyVector(position);
	//}

	public void interact(Body b){
		this.applyForce(b.attract(this));
	}

    /// <summary>
    /// Updates the properties of the body such as location, velocity, and 
    /// applied acceleration. This method should be invoked at each time step. 
    /// </summary>
    public void Update()
    {
        _locationHistory[_locationHistoryIndex] = position;
        _locationHistoryIndex = ++_locationHistoryIndex % _locationHistory.Length;

        double speed = velocity.magnitude;
        if (speed > Universe.C)
        {
            velocity = (float)Universe.C * (velocity / velocity.magnitude);
            speed = Universe.C;
        }

        if (speed == 0)
        {
            velocity += acceleration;
        }
        else
        {

            // Apply relativistic velocity addition. 
            var parallelAcc = VectorExtensions.Projection(acceleration, velocity);
            var orthogonalAcc = VectorExtensions.Rejection(acceleration, velocity);
            double alpha = Math.Sqrt(1 - Math.Pow(speed / Universe.C, 2));
            velocity = (velocity + parallelAcc + (float)alpha * orthogonalAcc) / (1 + Vector3.Dot(velocity, acceleration) / (float)(Universe.C * Universe.C));
        }

        position += velocity;
        acceleration = Vector3.zero;
        dot.transform.position = CopyVector(position);
    }

    public void applyForce(Vector3 force){
		acceleration += new Vector3(force.x/mass,force.y/mass,force.z/mass);
	}

	public Vector3 attract(Body b){
		Vector3 forc = position - b.position;
		float distance = forc.magnitude;
		distance = Mathf.Clamp(distance,50f,250f);

		forc.Normalize();
		var strength = (G*mass*mass)/(distance*distance);
		return new Vector3(forc.x* strength, forc.y* strength, forc.z* strength);

	}

	public void addBody(Body body){
		float m = mass + body.mass;
		float x = (position.x * mass + body.position.x * body.mass) / m ;
		float y = (position.y * mass + body.position.y * body.mass) / m ;
		mass = m;
		position = new Vector3(x,y,0);
	}
	
	private Vector3 CopyVector(Vector3 vec) {
		return new Vector3(vec.x,vec.y,vec.z);
	}

    /// <summary>
    /// Returns the radius defined for the given mass value. 
    /// </summary>
    /// <param name="mass">The mass to calculate a radius for.</param>
    /// <returns>The radius defined for the given mass value.</returns>
    public double GetRadius(double mass)
    {
        // We assume all bodies have the same density so volume is directly 
        // proportion to mass. Then we use the inverse of the equation for the 
        // volume of a sphere to solve for the radius. The end result is arbitrarily 
        // scaled and added to a constant so the Body is generally visible. 
        return 10 * Math.Pow(3 * mass / (4 * Math.PI), 1 / 3.0) + 10;
    }
}
