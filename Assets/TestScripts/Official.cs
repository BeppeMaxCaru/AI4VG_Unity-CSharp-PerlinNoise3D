using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Official : MonoBehaviour
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

    [Min(0)]
    public int samplingSize = 50;

    [Range(0f, 1f)]
    public float cloudiness = 1f;

    //Restrict to sky box
    public bool normalize = false;

    public bool borderOffset = false;
    [Min(0f)]
    public float maximumOffset = 0f;

    public bool fractals = false;
    [Min(0)]
    public int octaves = 0;
    [Min(0)]
    public float persistence = 0f;

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
                    //Debug.Log(perlinGrid[i, j, k].x);
                }
            }
        }

        return perlinGrid;

    }

    //OK
    //Calculate perlin noise for current point in the cube
    private float perlinNoise(int i, int j, int k, Vector3[,,] perlinGrid)
    {

        //Phase 1
        //Find current cube vertices
        int X0 = Mathf.FloorToInt(perlinGrid[i, j, k].x);
        int X1 = Mathf.CeilToInt(perlinGrid[i, j, k].x);

        int Y0 = Mathf.FloorToInt(perlinGrid[i, j, k].y);
        int Y1 = Mathf.CeilToInt(perlinGrid[i, j, k].y);

        int Z0 = Mathf.FloorToInt(perlinGrid[i, j, k].z);
        int Z1 = Mathf.CeilToInt(perlinGrid[i, j, k].z);

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

        Debug.Log("perlinX: " + perlinX);
        Debug.Log("perlinY: " + perlinY);
        Debug.Log("perlinZ: " + perlinZ);

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

        Debug.Log("final lerp: " + lerpedXYZ);

        //Return noise value inside the unit cube
        return lerpedXYZ;

    }

    //OK
    //Perlin noise generator function by Perlin himself
    private float fadeByPerlin(float side)
    {
        //Debug.Log("Side " + side);
        float final = 6 * Mathf.Pow(side, 5) - 15 * Mathf.Pow(side, 4) + 10 * Mathf.Pow(side, 3);
        //Debug.Log("final: " + final);
        return final;
        //return 6 * Mathf.Pow(side, 5) - 15 * Mathf.Pow(side, 4) + 10 * Mathf.Pow(side, 3);

    }

    //OK
    //Normalizer function
    private float normalizePerlinNoise(float valueToNormalize, int min, int max)
    {
        return (valueToNormalize - (float)min) / ((float)max - (float)min);
    }

    //TO DO
    //Fractal function
    public float fractalNoise(float x, float y, float z, Vector3[,,] perlinGrid, float noise, int octaves, float persistence)
    {
        noise = 0f;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;  // Used for normalizing result to 0.0 - 1.0

        for (int i = 0; i < octaves; i++)
        {
            //noise = noise + perlinNoise(x * frequency, y * frequency, z * frequency, perlinGrid) * amplitude;

            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= 2;
        }

        return (noise / maxValue);
    }

    // Start is called before the first frame update
    void Start()
    {

        if (randomSeed == 0) randomSeed = (int)System.DateTime.Now.Ticks;
        Random.InitState(randomSeed);

        Vector3[,,] perlinGrid = new Vector3[x, y, z];
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

                    //Create cloud or skip
                    float v = Random.Range(0f, 1f);
                    if (v > cloudiness)
                    {
                        //Debug.Log(v);
                        continue;
                    }

                    //Calculate perlin noise for point inside the current unit cube
                    float noise = 0f;

                    //Calculate fractal noise
                    if (fractals == true)
                    {

                        float frequency = 1;
                        float amplitude = 1;

                        for (int t = 0; t < octaves; t++)
                        {
                            //noise = noise + perlinNoise(i * frequency, j * frequency, k * frequency, perlinGrid) * amplitude;

                            amplitude = amplitude * persistence;
                            frequency = frequency * 2;
                        }

                    }
                    //Calculate perlin noise
                    else
                    {
                        try
                        {
                            noise = perlinNoise(i, j, k, perlinGrid);
                            Debug.Log(noise);
                        }
                        catch
                        {

                        }
                    }

                    if (noise < minNoise) minNoise = noise;
                    if (noise > maxNoise) maxNoise = noise;

                    //Noise is the new height!
                    perlinGrid[i, j, k].y = noise;
                    Debug.Log("noise: " + noise);

                    //if (normalize == true) noise = normalizedNoise;

                    /*if (borderOffset == true)
                    {
                        float rand = Random.Range(-maximumOffset, maximumOffset);
                        //Debug.Log(rand);
                        Instantiate(cloud, new Vector3(i + rand, noise, k + rand), Quaternion.identity);
                        //Instantiate(cloud, new Vector3(i + rand, finalNoise + rand, k + rand), Quaternion.identity);
                    }
                    else
                        Instantiate(cloud, new Vector3(i, noise, k), Quaternion.identity);*/

                    //if (rand < cloudProbability) Instantiate(cloud, new Vector3(i + rand, finalNoise + rand, k + rand), Quaternion.identity);
                    //Debug.Log(("space x  = " + normalizedLerp + ", y  =" + normalizedY + ", z  =" + normalizedZ));
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

                    Debug.Log(("space i  = " + i + ", j  =" + perlinGrid[i, j, k].y + ", k  =" + k));
                    float notNorm = perlinGrid[i, j, k].y;

                    perlinGrid[i, j, k].y = normalizePerlinNoise(perlinGrid[i, j, k].y, Mathf.FloorToInt(minNoise), Mathf.CeilToInt(maxNoise));

                    //MISSING SCALING FACTOR!
                    perlinGrid[i, j, k].y = perlinGrid[i, j, k].y * (y - 0) + 0;

                    //perlinGrid[i, j, k].y = normalizePerlinNoise(perlinGrid[i, j, k].y, 0, 10);
                    //Debug.Log(("space x  = " + x + ", y  =" + perlinGrid[i, j, k].y + ", z  =" + z));
                    Instantiate(cloud, new Vector3(i, perlinGrid[i, j, k].y, k), Quaternion.identity);

                    //if (normalize == true) noise = normalizedNoise;

                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
