using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseAlgo : MonoBehaviour
{

    //Random seed
    public int randomSeed = 0;

    //3D dimensions of the sky
    [Min(0)]
    public int x = 0;
    [Min(0)]
    public int y = 0;
    [Min(0)]
    public int z = 0;

    //Cloudiness
    [Range(0f, 1f)]
    public float cloudiness = 1f;

    //Restrict to sky
    public bool normalize = true;
    public bool scale = true;

    //Roughen lateral sides
    public bool sideOffset = false;

    //Cloud asset
    public GameObject cloud;

    //Initialize random vertices on sky's unit cubes vertices
    private Vector3[,,] initRandomVertices(Vector3[,,] perlinGrid)
    {
        for (int i = 0; i < perlinGrid.GetLength(0); i++)
        {
            for (int j = 0; j < perlinGrid.GetLength(1); j++)
            {
                for (int k = 0; k < perlinGrid.GetLength(2); k++)
                {
                    perlinGrid[i, j, k] = perlinGrid[i, j, k] + Random.insideUnitSphere;
                }
            }
        }

        return perlinGrid;

    }

    //Compute Perlin noise
    private float perlinNoise(int i, int j, int k, Vector3[,,] perlinGrid)
    {

        //Phase 1
        //Find surrounding unit cube vertices
        int X0 = Mathf.Clamp(Mathf.FloorToInt(perlinGrid[i, j, k].x), 0, x);
        int X1 = Mathf.Clamp(Mathf.CeilToInt(perlinGrid[i, j, k].x), 0, x);

        int Y0 = Mathf.Clamp(Mathf.FloorToInt(perlinGrid[i, j, k].y), 0, y);
        int Y1 = Mathf.Clamp(Mathf.CeilToInt(perlinGrid[i, j, k].y), 0, y);

        int Z0 = Mathf.Clamp(Mathf.FloorToInt(perlinGrid[i, j, k].z), 0, z);
        int Z1 = Mathf.Clamp(Mathf.CeilToInt(perlinGrid[i, j, k].z), 0, z);

        //Phase 2
        //Start permutations

        //Fix X0 and permute Y and Z
        float X0Y0Z0 = Vector3.Dot(perlinGrid[X0, Y0, Z0], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X0, Z0, Y0));
        float X0Y0Z1 = Vector3.Dot(perlinGrid[X0, Y0, Z1], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X0, Z1, Y0));
        float X0Y1Z0 = Vector3.Dot(perlinGrid[X0, Y1, Z0], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X0, Z0, Y1));
        float X0Y1Z1 = Vector3.Dot(perlinGrid[X0, Y1, Z1], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X0, Z1, Y1));

        //Fix X1 and permute Y and Z
        float X1Y0Z0 = Vector3.Dot(perlinGrid[X1, Y0, Z0], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X1, Z0, Y0));
        float X1Y0Z1 = Vector3.Dot(perlinGrid[X1, Y0, Z1], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X1, Z1, Y0));
        float X1Y1Z0 = Vector3.Dot(perlinGrid[X1, Y1, Z0], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X1, Z0, Y1));
        float X1Y1Z1 = Vector3.Dot(perlinGrid[X1, Y1, Z1], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X1, Z1, Y1));

        //Phase 3
        //Use Perlin function to get interpolation values
        float perlinX = fadeByPerlin((perlinGrid[i, j, k].x - X0));
        float perlinY = fadeByPerlin((perlinGrid[i, j, k].y - Y0));
        float perlinZ = fadeByPerlin((perlinGrid[i, j, k].z - Z0));

        //Phase 4
        //Start interpolating to get the vertices weighted contributions

        //Fix X0 and Y then interpolate along Z
        float X0Y0lerpedZ = Mathf.Lerp(X0Y0Z0, X0Y0Z1, perlinZ);
        float X0Y1lerpedZ = Mathf.Lerp(X0Y1Z0, X0Y1Z1, perlinZ);
        //Fix X0 then lerp along Y
        float X0lerpedYZ = Mathf.Lerp(X0Y0lerpedZ, X0Y1lerpedZ, perlinY);

        //Fix X1 and Y and interpolate along Z
        float X1Y0lerpedZ = Mathf.Lerp(X1Y0Z0, X1Y0Z1, perlinZ);
        float X1Y1lerpedZ = Mathf.Lerp(X1Y1Z0, X1Y1Z1, perlinZ);
        //Then X1 then interpolate along Y
        float X1lerpedYZ = Mathf.Lerp(X1Y0lerpedZ, X1Y1lerpedZ, perlinY);

        //Finally interpolate along X
        float lerpedXYZ = Mathf.Lerp(X0lerpedYZ, X1lerpedYZ, perlinX);
        //This is the resulting value -> Perlin noise

        //Return noise value
        return lerpedXYZ;

    }

    //Perlin noise interpolation function by Perlin himself
    private float fadeByPerlin(float t)
    {
        return 6 * Mathf.Pow(t, 5) - 15 * Mathf.Pow(t, 4) + 10 * Mathf.Pow(t, 3);
    }

    //Normalization function
    private float normalizeNoise(float noiseToNormalize, float min, float max)
    {
        return (noiseToNormalize - min) / (max - min);
    }

    //Scaling function
    private float scaleNoise(float noiseToScale, float min, float max)
    {
        return (noiseToScale * (max - min) + min);
    }

    //Main function
    void Start()
    {

        if (randomSeed == 0) randomSeed = (int)System.DateTime.Now.Ticks;
        Random.InitState(randomSeed);

        Vector3[,,] perlinGrid = new Vector3[x, y, z];
        float[,,] noiseValues = new float[x, y, z];

        perlinGrid = initRandomVertices(perlinGrid);

        float maxNoise = 0f;
        float minNoise = 0f;

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {

                    float noise = perlinNoise(i, j, k, perlinGrid);

                    if (noise < minNoise) minNoise = noise;
                    if (noise > maxNoise) maxNoise = noise;

                    //Noise is the new height!
                    noiseValues[i, j, k] = noise;

                }
            }
        }

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {

                    if (Random.value > cloudiness) continue;

                    float finalNoise = noiseValues[i, j, k];

                    if (normalize == true) finalNoise = normalizeNoise(finalNoise, minNoise, (maxNoise));

                    if (scale == true) finalNoise = scaleNoise(finalNoise, 0, y);

                    if (sideOffset == true && (i == 0 || i == x - 1 || k == 0 || k == z - 1))
                    {
                        Vector2 offset = Random.insideUnitCircle;
                        Instantiate(cloud, new Vector3(i + offset.x, finalNoise, k + offset.y), Quaternion.identity);
                    } else 
                        Instantiate(cloud, new Vector3(i, finalNoise, k), Quaternion.identity);

                }
            }
        }

    }

}

