using UnityEngine;

// Unity MonoBehaviour Example
// MonoBehaviour is the base class from which every Unity script derives.
public class GrassComputer : MonoBehaviour
{
    public float Range;
    public int Density;

    public UnityEngine.Material GrassMaterial;
    public UnityEngine.Mesh GrassMesh;

    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;

    private Bounds bounds;

    private struct MeshProperties
    {
        public Matrix4x4 matrix;
        public Vector4 color;

        public static int Size()
        {
            return sizeof(float) * 16 + sizeof(float) * 4;
        }
    }

    public void Start()
    {
        var lineCount = this.Range * this.Density;
        var population = (int)(lineCount * lineCount);
        this.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(this.Range, 0, this.Range));

        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = (uint)this.GrassMesh.GetIndexCount(0);
        args[1] = (uint)(population);
        args[2] = (uint)this.GrassMesh.GetIndexStart(0);
        args[3] = (uint)this.GrassMesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        MeshProperties[] meshProperties = new MeshProperties[population];

        var start = this.Range / 2f;
        var step = this.Range / lineCount;
        var index = 0;

        for (var idx = 0; idx < lineCount; idx++)
        {
            for (var idy = 0; idy < lineCount; idy++)
            {
                var meshProperty = new MeshProperties();
                var position = new Vector3(-start + idx * step, 0f, -start + idy * step) + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                position.y = Terrain.activeTerrain.SampleHeight(position);
                var rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                var scaleNoise = 1f;
                var scaleAmplitude = 2f;
                var scaleFactor = Mathf.PerlinNoise(idx + 0.01f * scaleNoise, idy + 0.01f * scaleNoise) * scaleAmplitude;
                var scale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

                meshProperty.color = new Vector4(Random.Range(0f, 0.3f), 1f, Random.Range(0f, 0.3f), 1);
                meshProperty.matrix = Matrix4x4.TRS(position, rotation, scale);
                meshProperties[index] = meshProperty;

                index++;
            }
        }

        this.meshPropertiesBuffer = new ComputeBuffer(population, MeshProperties.Size());
        this.meshPropertiesBuffer.SetData(meshProperties);
        this.GrassMaterial.SetBuffer("_Properties", this.meshPropertiesBuffer);
    }

    public void Update()
    {
        Graphics.DrawMeshInstancedIndirect(this.GrassMesh, 0, this.GrassMaterial, this.bounds, this.argsBuffer);
    }

    public void OnDisabled()
    {
        this.meshPropertiesBuffer.Release();
        this.meshPropertiesBuffer = null;
    }
}