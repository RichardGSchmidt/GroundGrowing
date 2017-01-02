	using UnityEngine;

	[RequireComponent(typeof(MeshFilter))]
	public class OctahedronSphereTester : MonoBehaviour {

		public int subdivisions = 5;
		
		public float radius = 1f;
		
		private void Awake () {
			GetComponent<MeshFilter>().mesh = OctahedronSphereCreator.Create(subdivisions, radius);
		}
	}