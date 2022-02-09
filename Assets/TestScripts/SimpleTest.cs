using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTest : MonoBehaviour
{
    //Random init
    public int randomSeed = 0;

    //3D dimensions
    [Min(0)]
    public int x = 0;
    [Min(0)]
    public int y = 0;
    [Min(0)]
    public int z = 0;

    //[Range(0f, 1f)]
    //public float cloudiness = 1f;

    //Restrict to sky box
    //public bool normalize = false;

    //public bool borderOffset = false;
    //[Min(0f)]
    //public float maximumOffset = 0f;

    //public bool fractals = false;
    //[Min(0)]
    //public int octaves = 0;
    //[Min(0)]
    //public float persistence = 0f;

    public GameObject cloud;

    //OK
    //Initialize random arrays on sky's unit cubes vertices
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

    //OK
    //Calculate perlin noise for current point in the cube
    private float perlinNoise(int i, int j, int k, Vector3[,,] perlinGrid)
    {

        //if !unitCubeExists(i, j, k, perlinGrid) return;

        //Phase 1
        //Find current cube vertices
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
        //Debug.Log("X0Y0Z0 " + X0Y0Z0);
        float X0Y0Z1 = Vector3.Dot(perlinGrid[X0, Y0, Z1], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X0, Z1, Y0));
        //Debug.Log("X0Y0Z1 " + X0Y0Z1);
        float X0Y1Z0 = Vector3.Dot(perlinGrid[X0, Y1, Z0], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X0, Z0, Y1));
        //Debug.Log("X0Y1Z0 " + X0Y1Z0);
        float X0Y1Z1 = Vector3.Dot(perlinGrid[X0, Y1, Z1], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X0, Z1, Y1));
        //Debug.Log("X0Y1Z1 " + X0Y1Z1);

        //Fix X1 and permute Y and Z
        float X1Y0Z0 = Vector3.Dot(perlinGrid[X1, Y0, Z0], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X1, Z0, Y0));
        //Debug.Log("X1Y0Z0 " + X1Y0Z0);
        float X1Y0Z1 = Vector3.Dot(perlinGrid[X1, Y0, Z1], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X1, Z1, Y0));
        //Debug.Log("X1Y0Z1 " + X1Y0Z1);
        float X1Y1Z0 = Vector3.Dot(perlinGrid[X1, Y1, Z0], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X1, Z0, Y1));
        //Debug.Log("X1Y1Z0 " + X1Y1Z0);
        float X1Y1Z1 = Vector3.Dot(perlinGrid[X1, Y1, Z1], new Vector3(perlinGrid[i, j, k].x, perlinGrid[i, j, k].y, perlinGrid[i, j, k].z) - new Vector3(X1, Z1, Y1));

        //Phase 3
        //Use perlin function to smooth values
        float perlinX = fadeByPerlin((perlinGrid[i, j, k].x - X0));
        float perlinY = fadeByPerlin((perlinGrid[i, j, k].y - Y0));
        float perlinZ = fadeByPerlin((perlinGrid[i, j, k].z - Z0));

        //Debug.Log("perlinX: " + perlinX);
        //Debug.Log("perlinY: " + perlinY);
        //Debug.Log("perlinZ: " + perlinZ);

        //Phase 4
        //Start lerping to get the vertices weighted average

        //Fix X0 and Y then lerp along Z
        float X0Y0lerpedZ = Mathf.Lerp(X0Y0Z0, X0Y0Z1, perlinZ);
        float X0Y1lerpedZ = Mathf.Lerp(X0Y1Z0, X0Y1Z1, perlinZ);
        //Fix X0 then lerp along Y
        float X0lerpedYZ = Mathf.Lerp(X0Y0lerpedZ, X0Y1lerpedZ, perlinY);

        //Fix X1 and Y and lerp along Z
        float X1Y0lerpedZ = Mathf.Lerp(X1Y0Z0, X1Y0Z1, perlinZ);
        float X1Y1lerpedZ = Mathf.Lerp(X1Y1Z0, X1Y1Z1, perlinZ);
        //Then X1 then lerp along Y
        float X1lerpedYZ = Mathf.Lerp(X1Y0lerpedZ, X1Y1lerpedZ, perlinY);

        //Finally lerp along X
        float lerpedXYZ = Mathf.Lerp(X0lerpedYZ, X1lerpedYZ, perlinX);
        //I receive the combined height -> Perlin Noise

        //Debug.Log("final lerp: " + lerpedXYZ);

        //Return noise value inside the unit cube
        return lerpedXYZ;

    }

    //OK
    //Perlin noise generator function by Perlin himself
    private float fadeByPerlin(float side)
    {
        float final = 6 * Mathf.Pow(side, 5) - 15 * Mathf.Pow(side, 4) + 10 * Mathf.Pow(side, 3);
        return final;

    }

    //OK
    //Normalizer function
    private float normalizePerlinNoise(float valueToNormalize, float min, float max)
    {
        return (valueToNormalize - (float)min) / ((float)max - (float)min);
    }

    private float scalePerlinNoise(float noise, float min, float max)
    {
        return (noise * (max - min) + min);
    }

    // Start is called before the first frame update
    void Start()
    {

        if (randomSeed == 0) randomSeed = (int)System.DateTime.Now.Ticks;
        Random.InitState(randomSeed);

        Vector3[,,] perlinGrid = new Vector3[x, y, z];
        //Vector3[,,] noiseValues = new Vector3[x, y, z];
        float[,,] noiseValues = new float[x, y, z];

        perlinGrid = initRandomVertices(perlinGrid);

        float maxNoise = 0f;
        float minNoise = 0f;

        //Remove equals to not make a cube from i <= myTest and try to start from 1 instead of 0
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {

                    /*if (!unitCubeExists(i, j, k, perlinGrid))
                    {
                        noiseValues[i, j, k] = Vector3.zero;
                        continue;
                    }*/

                    //Calculate perlin noise for point inside the current unit cube
                    //float noise = 0f;

                    float noise = perlinNoise(i, j, k, perlinGrid);

                    if (noise < minNoise) minNoise = noise;
                    if (noise > maxNoise) maxNoise = noise;

                    //Noise is the new height!
                    //noiseValues[i, j, k] = new Vector3(i, noise, k);
                    noiseValues[i, j, k] = noise;
                    //Debug.Log("noise: " + noise);

                }
            }
        }

        Debug.Log("minNoise: " + minNoise + "and maxNoise: " + maxNoise);
        Debug.Log("floorMinNoise: " + Mathf.FloorToInt(minNoise) + "and ceilMaxNoise: " + Mathf.CeilToInt(maxNoise));

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {

                    if (noiseValues[i, j, k].Equals(Vector3.zero)) continue;

                    //Debug.Log(("space x  = " + perlinGrid[i, j, k].x + ", y  =" + perlinGrid[i, j, k].y + ", z  =" + perlinGrid[i, j, k].z));
                    //float notNorm = noiseValues[i, j, k].y;
                    float notNorm = noiseValues[i, j, k];

                    //noiseValues[i, j, k].y = normalizePerlinNoise(noiseValues[i, j, k].y, minNoise, (maxNoise));
                    noiseValues[i, j, k] = normalizePerlinNoise(noiseValues[i, j, k], minNoise, (maxNoise));

                    //SCALING FACTOR FOR THE NOISE!
                    //noiseValues[i, j, k].y = noiseValues[i, j, k].y * (y - 0) + 0;

                    //noiseValues[i, j, k].y = scalePerlinNoise(noiseValues[i, j, k].y, 0, y);
                    noiseValues[i, j, k] = scalePerlinNoise(noiseValues[i, j, k], 0, y);

                    
                    //Instantiate(cloud, new Vector3(i, noiseValues[i, j, k].y, k), Quaternion.identity);
                    Instantiate(cloud, new Vector3(i, noiseValues[i, j, k], k), Quaternion.identity);

                    //if (normalize == true) noise = normalizedNoise;

                }
            }
        }
    }

}
