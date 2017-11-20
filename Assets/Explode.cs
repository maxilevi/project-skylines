/* Written by Maxi Levi <maxilevi@live.com>, November 2017
 */

 
using UnityEngine;
using System.Collections;

public class Explode : MonoBehaviour
{
	public AudioSource ExplosionAudio;
	private GameObject Debris;
	void Start(){
		Debris = GameObject.FindGameObjectWithTag ("Debris");
		SplitMesh();
	}

	IEnumerator DestroyCoroutine(GameObject GO){
		yield return new WaitForSeconds (2 + Random.Range(0.0f, 5.0f));
		GO.GetComponent<MeshFilter> ().sharedMesh.Clear ();
		Destroy(GO);
	}

	void SplitMesh ()
	{
		MeshFilter MF = GetComponent<MeshFilter>();
		MeshRenderer MR = GetComponent<MeshRenderer>();
		Mesh M = MF.mesh;
		Vector3[] verts = M.vertices;
		Vector3[] normals = M.normals;
		Vector2[] uvs = M.uv;
		for (int submesh = 0; submesh < M.subMeshCount; submesh++)
		{
			int[] indices = M.GetTriangles(submesh);
			for (int i = 0; i < indices.Length; i += 3)
			{
				Vector3[] newVerts = new Vector3[3];
				Vector3[] newNormals = new Vector3[3];
				Vector2[] newUvs = new Vector2[3];
				for (int n = 0; n < 3; n++)
				{
					int index = indices[i + n];
					newVerts[n] = verts[index];
					newUvs[n] = uvs[index];
					newNormals[n] = normals[index];
				}
				Mesh mesh = new Mesh();
				mesh.vertices = newVerts;
				mesh.normals = newNormals;
				mesh.uv = newUvs;

				mesh.triangles = new int[] { 0, 1, 2, 2, 1, 0 };

				GameObject GO = new GameObject("Triangle " + (i / 3));
				if (Debris != null)
					GO.transform.parent = Debris.transform;
				GO.transform.position = transform.position;
				GO.transform.rotation = transform.rotation;
				GO.AddComponent<MeshRenderer>().material = MR.materials[submesh];
				GO.AddComponent<MeshFilter>().mesh = mesh;
				GO.AddComponent<BoxCollider>();
				GO.AddComponent<Rigidbody>().AddExplosionForce(20, transform.position, 30);

				StartCoroutine (DestroyCoroutine (GO));
			}
		}
		MR.enabled = false;
		if (ExplosionAudio != null) {
			AudioSource au = Instantiate<AudioSource> (ExplosionAudio, this.transform.position, Quaternion.identity);
			Destroy (au, au.time + .5f);
		}
		Destroy(this.gameObject);
	}
}