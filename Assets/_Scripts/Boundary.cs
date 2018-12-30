using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boundary{

	public Vector3 min;
	public Vector3 max;

	private float limit;
	private Vector3 precision;

	public Boundary(float limit){
		this.limit = limit;
		float prec = 2;
		precision = new Vector3(prec,prec,prec);
	}

	public void update(List<Body> bodies){
		min = new Vector3(limit,limit,0);
		max = new Vector3(-limit,-limit,0);
		foreach(Body body in bodies){
			getLimit(body.position);
		}
		adjust();
	}

	private void getLimit(Vector3 vec){
		max = Vector3.Max(max,vec);
		min = Vector3.Min(min,vec);
	}

	private void adjust(){
		max+=precision;
		min -=precision;
	}
}
