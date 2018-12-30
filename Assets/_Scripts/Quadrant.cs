using UnityEngine;
using System.Collections;

public class Quadrant
{
	public Vector3 position;
	public Vector3 size;
	public Color color;
	public Vector3 gravityCenter;
	public float mass;

	public Quadrant(Vector3 position, float size, float index,Vector3 gravCenter,float mass){
		this.position = position ;
	 	this.size = new Vector3(size,size,size);
		this.color = new Color(1-(index*0.066f),0f,index*0.066f,0.1f);	
		this.gravityCenter =  gravCenter;
		this.mass = mass;
	}


}
