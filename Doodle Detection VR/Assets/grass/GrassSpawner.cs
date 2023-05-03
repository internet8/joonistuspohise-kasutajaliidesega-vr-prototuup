using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public Mesh grassMesh;
    public Transform ground;
    public Material grassMaterial;
    public int grassCount = 100000;
    public Vector2 windDir = new Vector2(1, 0);
    public float windSpeed = 10;

    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;

    private void Start()
    {
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(new uint[] { (uint)grassMesh.GetIndexCount(0), (uint)grassCount, 0, 0, 0 });

        Vector3[] randomPositions = new Vector3[grassCount];
        for (int i = 0; i < grassCount; i++)
        {
            //randomPositions[i] = new Vector3(Random.Range(-50f, 50f) * ground.localScale.x, 0.0001f, Random.Range(-50f, 50f) * ground.localScale.z);
            randomPositions[i] = new Vector3(Random.Range(0f, 1f), 0, Random.Range(0f, 1f));
        }
        positionBuffer = new ComputeBuffer(grassCount, sizeof(float) * 3);
        positionBuffer.SetData(randomPositions);
        grassMaterial.SetBuffer("PositionBuffer", positionBuffer);
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, new Bounds(Vector3.zero, new Vector3(-500.0f, 200.0f, 500.0f)), argsBuffer);
        grassMaterial.SetFloat("_Rotation", Camera.main.transform.eulerAngles.y);
        grassMaterial.SetTextureOffset("_WindNoise", windDir * Time.time * windSpeed);
    }

    private void OnDestroy()
    {
        positionBuffer.Release();
        argsBuffer.Release();
    }
}
