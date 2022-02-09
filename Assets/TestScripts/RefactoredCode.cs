using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefactoredCode : MonoBehaviour
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
    private float perlinNoise(float i, float j, float k, Vector3[,,] perlinGrid)
    {

        //Phase 1
        //Find current cube vertices
        int X0 = Mathf.FloorToInt(i);
        int X1 = Mathf.CeilToInt(i);

        int Y0 = Mathf.FloorToInt(j);
        int Y1 = Mathf.CeilToInt(j);

        int Z0 = Mathf.FloorToInt(k);
        int Z1 = Mathf.CeilToInt(k);

        //Phase 2
        //Start permutations

        //Fix X0 and permute Y and Z
        float X0Y0Z0 = Vector3.Dot(perlinGrid[X0, Y0, Z0], new Vector3(i, j, k) - new Vector3(X0, Z0, Y0));
        //Debug.Log("X0Y0Z0 " + X0Y0Z0);
        float X0Y0Z1 = Vector3.Dot(perlinGrid[X0, Y0, Z1], new Vector3(i, j, k) - new Vector3(X0, Z1, Y0));
        //Debug.Log("X0Y0Z1 " + X0Y0Z1);
        float X0Y1Z0 = Vector3.Dot(perlinGrid[X0, Y1, Z0], new Vector3(i, j, k) - new Vector3(X0, Z0, Y1));
        //Debug.Log("X0Y1Z0 " + X0Y1Z0);
        float X0Y1Z1 = Vector3.Dot(perlinGrid[X0, Y1, Z1], new Vector3(i, j, k) - new Vector3(X0, Z1, Y1));
        //Debug.Log("X0Y1Z1 " + X0Y1Z1);

        //Fix X1 and permute Y and Z
        float X1Y0Z0 = Vector3.Dot(perlinGrid[X1, Y0, Z0], new Vector3(i, j, k) - new Vector3(X1, Z0, Y0));
        //Debug.Log("X1Y0Z0 " + X1Y0Z0);
        float X1Y0Z1 = Vector3.Dot(perlinGrid[X1, Y0, Z1], new Vector3(i, j, k) - new Vector3(X1, Z1, Y0));
        //Debug.Log("X1Y0Z1 " + X1Y0Z1);
        float X1Y1Z0 = Vector3.Dot(perlinGrid[X1, Y1, Z0], new Vector3(i, j, k) - new Vector3(X1, Z0, Y1));
        //Debug.Log("X1Y1Z0 " + X1Y1Z0);
        float X1Y1Z1 = Vector3.Dot(perlinGrid[X1, Y1, Z1], new Vector3(i, j, k) - new Vector3(X1, Z1, Y1));

        //Phase 3
        //Use perlin function to smooth values
        float perlinX = fadeByPerlin(i);
        float perlinY = fadeByPerlin(j);
        float perlinZ = fadeByPerlin(k);

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

        //Return noise value inside the unit cube
        return lerpedXYZ;

    }

    //OK
    //Perlin noise generator function by Perlin himself
    private float fadeByPerlin(float side)
    {
        return 6 * Mathf.Pow(side, 5) - 15 * Mathf.Pow(side, 4) + 10 * Mathf.Pow(side, 3);
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
            noise = noise + perlinNoise(x * frequency, y * frequency, z * frequency, perlinGrid) * amplitude;

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

        //Remove equals to not make a cube from i <= myTest and try to start from 1 instead of 0
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {

                    //Create cloud or skip
                    float v = Random.Range(0f, 1f);
                    if (v > cloudiness) {
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
                            noise = noise + perlinNoise(i * frequency, j * frequency, k * frequency, perlinGrid) * amplitude;

                            amplitude = amplitude * persistence;
                            frequency = frequency * 2;
                        }

                    } 
                    //Calculate perlin noise
                    else
                    {
                        noise = perlinNoise(i, j, k, perlinGrid);
                    }

                    

                    //ERRATO
                    //Normalize final noise
                    float normalizedNoise = normalizePerlinNoise(noise, 0, y);

                    //float rand = Random.value;
                    //Debug.Log(rand);
                    //Make cloud clustering

                    if (normalize == true) noise = normalizedNoise;

                    if (borderOffset == true)
                    {
                        float rand = Random.Range(-maximumOffset, maximumOffset);
                        //Debug.Log(rand);
                        Instantiate(cloud, new Vector3(i + rand, noise, k + rand), Quaternion.identity);
                        //Instantiate(cloud, new Vector3(i + rand, finalNoise + rand, k + rand), Quaternion.identity);
                    } else 
                        Instantiate(cloud, new Vector3(i, noise, k), Quaternion.identity);

                    //if (rand < cloudProbability) Instantiate(cloud, new Vector3(i + rand, finalNoise + rand, k + rand), Quaternion.identity);
                    //Debug.Log(("space x  = " + normalizedLerp + ", y  =" + normalizedY + ", z  =" + normalizedZ));
                }
            }
        }



    }

    // Update is called once per frame
    void Update()
    {

    }
}


