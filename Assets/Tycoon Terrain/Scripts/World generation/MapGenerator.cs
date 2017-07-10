using UnityEngine;
using System.Collections;

namespace TycoonTerrain{
	public class MapGenerator  {

		public enum RandomDistributionType{
			LINEAR,
			GAUSSIAN,
			SQUARE_ROOT,
			TWO_RANDS,
			THREE_RANDS,
		}

		public static float[,] CalcPerlinNoiseMap(int size, float scale = 1f, float min = 0, float max = 1 ) {
			// we set origins for the noise
			float xOrg = Random.Range(0f, 1024 * scale);
			float yOrg = Random.Range(0f, 1024 * scale);
			//float xOrg = 0; // If set 0 then you always get the same map
			//float yOrg = 0;

			float[,] noiseMap = new float[size,size];
			int y = 0;
			while (y < size) {
				int x = 0;
				while (x < size) {
					float xCoord = xOrg + x / (float)size * scale;
					float yCoord = yOrg + y / (float)size * scale;
					float sample = (max-min) * Mathf.PerlinNoise(xCoord, yCoord) + min;
					noiseMap[x, y] = sample;
					x++;
				}
				y++;
			}
			return noiseMap;
		}


		#region cahced values
		private static int heightMapSize;
		private static float hillyness;
		private static RandomDistributionType randomType;
		#endregion
		public static int[,] DiamondSquare(int heightMapSize, RandomDistributionType randomType, float hillyness){
			MapGenerator.heightMapSize = heightMapSize;
			MapGenerator.hillyness = hillyness;
			MapGenerator.randomType = randomType;

			// calculate heightmap
			int[,] heightMap = new int[heightMapSize,heightMapSize];
			//First set the four corners of the map
			if (Random.Range(0f,1f) < hillyness)
				heightMap[0,0] = GetRandomHeight(-heightMapSize/4,heightMapSize/4);
			if (Random.Range(0f,1f) < hillyness)
				heightMap[0,heightMapSize - 1] = GetRandomHeight(-heightMapSize/4,heightMapSize/4);
			if (Random.Range(0f,1f) < hillyness)
				heightMap[heightMapSize - 1,0] = GetRandomHeight(-heightMapSize/4,heightMapSize/4);
			if (Random.Range(0f,1f) < hillyness)
				heightMap[heightMapSize - 1,heightMapSize - 1] = GetRandomHeight(-heightMapSize/4,heightMapSize/4);
			
			for (int halfStep = (heightMapSize - 1) / 2; halfStep > 0; halfStep /= 2) {
				for (int x = halfStep; x < heightMapSize - halfStep; x += 2 * halfStep) {
					for (int y = halfStep; y < heightMapSize - halfStep; y += 2 * halfStep) {
						DiamondStep(heightMap, halfStep, x, y);		
					}	
				}
			}

			return heightMap;


		}

		/// <summary>
		/// Postprocess a heightmap to insure that incline is never more than 1 between two tiles. Use this in case you use 
		/// some other algorithm than the specialized DiamondSquare, eg Perlin noise etc
		/// </summary>
		/// <param name="heightMap">Height map.</param>
		static void PostProcessHeightmap(int[,] heightMap){
			int size = heightMap.Length;
			Debug.Log("size : " + size);
			for (int x = 1; x < size; x++) {
				heightMap[x,0] = Mathf.Clamp(heightMap[x,0], heightMap[x-1,0] - 1, heightMap[x - 1,0] + 1);
			}
			
			for (int y = 1; y < size; y++) {
				heightMap[0,y] = Mathf.Clamp(heightMap[0,y], heightMap[0,y-1] - 1, heightMap[0,y - 1] + 1);
			}
			
			for (int x = 1; x < size; x++) {
				for (int y = 1; y < size; y++) {
					heightMap[x,y] = Mathf.Clamp(heightMap[x,y], heightMap[x-1,y-1] - 1, heightMap[x - 1,y - 1] + 1);
					heightMap[x,y] = Mathf.Clamp(heightMap[x,y], heightMap[x,y-1] - 1, heightMap[x,y - 1] + 1);
					heightMap[x,y] = Mathf.Clamp(heightMap[x,y], heightMap[x-1,y] - 1, heightMap[x - 1,y] + 1);
				}
			}
		}
		
		/// <summary>
		/// Creates diamonds of squares
		/// </summary>
		/// <param name="level">Level.</param>
		static void DiamondStep(int[,] heightMap, int halfStep, int x, int y){
			if(halfStep == 0)
				return;
			
			// We set a min and max so that we never gat more than 45 degree incline
			int minHeight = Mathf.Min (heightMap [x - halfStep, y - halfStep],
			                           heightMap [x + halfStep, y - halfStep],
			                           heightMap [x - halfStep, y + halfStep],
			                           heightMap [x + halfStep, y + halfStep]);
			int maxHeight = Mathf.Max (heightMap [x - halfStep, y - halfStep],
			                           heightMap [x + halfStep, y - halfStep],
			                           heightMap [x - halfStep, y + halfStep],
			                           heightMap [x + halfStep, y + halfStep]);
			
			//Midheight is used in the basic version of the algorithm
			//heightMap[x, y] = midHeight + hillyness * halfStep * Random.Range(-1f,1f);;
			
			//In this discrete version we instead use a max and min value to make sure that one discrete step never incline more than 1
			if(Random.Range(0f,1f) < hillyness)
				// note that dividing the halfstep here isn't exactly correct BUT it makes slopes have a bigger chance of being non diagonal
				// which works very nice in this type of discrete map.
				heightMap[x, y] =  GetRandomHeight(maxHeight - halfStep/2, minHeight + halfStep/2); //Random.Range(maxHeight - halfStep, minHeight + halfStep + 1);
			else 
				heightMap[x, y] = (minHeight + maxHeight)/2;
			// Square step
			SquareStep(heightMap, halfStep, x - halfStep, y);
			SquareStep(heightMap, halfStep, x, y - halfStep);
			
			
			//Handle edges of map when we reach them
			if(x + halfStep == heightMapSize - 1)
				SquareStep(heightMap, halfStep, x + halfStep, y);
			if(y + halfStep == heightMapSize - 1)
				SquareStep(heightMap, halfStep, x, y + halfStep);
			
		}

		/// <summary>
		/// Creates squares of diamonds
		/// </summary>
		static void SquareStep(int[,] heightMap, int halfStep, int x, int y){
			if(halfStep == 0)
				return;
			
			//   c
			//
			//a  x  b
			//
			//   d
			
			int maxHeight = int.MinValue;
			int minHeight = int.MaxValue;
			//Debug.Log ("Square step x: " + x + " y: " + y + " halfstep: " + halfStep);
			if (x - halfStep >= 0) {
				minHeight = Mathf.Min (heightMap [x - halfStep, y], minHeight);
				maxHeight = Mathf.Max (heightMap [x - halfStep, y], maxHeight);
				//Debug.Log(" lowest neighbour: " + (minHeight).ToString() + " highest neighbour: " + (maxHeight).ToString());
			}
			if (x + halfStep < heightMapSize) {
				minHeight = Mathf.Min (heightMap [x + halfStep, y], minHeight);
				maxHeight = Mathf.Max (heightMap [x + halfStep, y], maxHeight);
				//Debug.Log(" lowest neighbour: " + (minHeight).ToString() + " highest neighbour: " + (maxHeight).ToString());
			}
			
			if (y - halfStep >= 0) {
				minHeight = Mathf.Min (heightMap [x, y - halfStep], minHeight);
				maxHeight = Mathf.Max (heightMap [x, y - halfStep], maxHeight);
				//Debug.Log(" lowest neighbour: " + (minHeight).ToString() + " highest neighbour: " + (maxHeight).ToString());
			}
			if (y + halfStep < heightMapSize) {
				minHeight = Mathf.Min (heightMap [x, y + halfStep], minHeight);
				maxHeight = Mathf.Max (heightMap [x, y + halfStep], maxHeight);
				//Debug.Log(" lowest neighbour: " + (minHeight).ToString() + " highest neighbour: " + (maxHeight).ToString());
			}

			//We can't set a new height that is higher than min height + halfstep or lower than max height - halfstep
			if (Random.Range(0f,1f) < hillyness && maxHeight - halfStep / 2 < minHeight + halfStep / 2) {
				heightMap [x, y] = GetRandomHeight(maxHeight - halfStep / 2, minHeight + halfStep / 2);
			} else {
				heightMap [x, y] = (maxHeight + minHeight)/2;
			}
		}


		/// <summary>
		/// Returns a random number between -1 and 1
		/// 
		/// Note that this is my own crazy stuff adapted for this purpose only and do not necessarily correspond correctly to their names
		/// in the mathematical sense
		/// </summary>
		/// <returns>The random.</returns>
		/// <param name="randomType">Random type.</param>
		static float GetRandom(RandomDistributionType randomType){
			switch (randomType) {
			case RandomDistributionType.LINEAR:
				return Random.Range(-1f, 1f);
			case RandomDistributionType.SQUARE_ROOT:
				return Mathf.Sqrt( Random.Range(0f, 0.5f)) * (Random.Range(0f,1f) > 0.5f ? -1:1);
			case RandomDistributionType.GAUSSIAN:
				float u, v, S;
				
				do
				{
					u = 2.0f * Random.Range(0f, 1f) - 1.0f;
					v = 2.0f * Random.Range(0f, 1f) - 1.0f;
					S = u * u + v * v;
				}
				while (S >= 1.0f);
				
				float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
				fac = u * fac/3f;
				return Mathf.Clamp(fac,-1f, 1f);
			case RandomDistributionType.TWO_RANDS:
				return Random.Range(-1f, 1f) * Random.Range(0f,1f) ;
			case RandomDistributionType.THREE_RANDS:
				return Random.Range(-1f, 1f) * Random.Range(0f,1f)* Random.Range(0f,1f) ;
			default:
				return Random.Range(-1f, 1f);
			}
		}
		
		/// <summary>
		/// Gets random height between min and max (inclusive)
		/// </summary>
		/// <returns>The random height.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		static int GetRandomHeight(int min, int max){
			return (max + min)/2 + Mathf.RoundToInt( (GetRandom(randomType)) * ((max - min)/2) );
		}
		 
	}
}