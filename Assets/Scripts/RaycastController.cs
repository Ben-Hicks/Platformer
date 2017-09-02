using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;
	public LayerMask collisionMask;
	public const float skinWidth = 0.015f;
	public float raySpacing = 0.25f;

	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;
	[HideInInspector]
	public BoxCollider2D collider;
	[HideInInspector]
	public RaycastOrigins raycastOrigins;

	public struct RaycastOrigins{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	// Use this for initialization
	public virtual void Awake () {
		collider = GetComponent<BoxCollider2D> ();
		SpaceRays ();
	}

	public virtual void Start(){
		CalculateRaySpacing ();
	}

	public void UpdateRaycastOrigins(){
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);//so that it shrinks by skinwidth on each side

		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}


	public void SpaceRays(){
		Bounds bounds = collider.bounds;
		verticalRayCount = (int)(bounds.size.x / raySpacing);
		horizontalRayCount = (int)(bounds.size.y / raySpacing);
	}

	public void CalculateRaySpacing(){
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}
}
