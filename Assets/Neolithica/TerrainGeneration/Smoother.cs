using System;
using UnityEngine;

namespace Neolithica.TerrainGeneration {
    public class Smoother {

        public Smoother(float[,] heightMap) {
            mHeightMap = heightMap;
            mDims = new[] { mHeightMap.GetLength(0), mHeightMap.GetLength(1) };
        }

        public float[,] Smooth(float radius, float bias) {
            float[,] newHeightMap = new float[mDims[0], mDims[1]];

            for (int i = 0; i < mDims[0]; i++) {
                for (int j = 0; j < mDims[1]; j++)
                    newHeightMap[i, j] = SmoothSample(i, j, radius, bias);
            }

            return newHeightMap;
        }

        private float SmoothSample(int x, int y, float radius, float bias) {
            if (radius < 1.0f)
                throw new ArgumentOutOfRangeException("radius", "Radius must be at least 1.0");

            float weight = radius * bias;
            float sum = mHeightMap[x, y] * radius * bias;
            int sampleRange = (int)radius;

            for (int i = -sampleRange; i <= sampleRange; i++) {
                for (int j = -sampleRange; j <= sampleRange; j++) {
                    if (i == 0 && j == 0)
                        continue;

                    int u = x + i;
                    int v = y + j;

                    if (!CoordsInRange(u, v))
                        continue;

                    float localWeight = radius - Mathf.Sqrt((x - u) * (x - u) + (y - v) * (y - v));
                    if (localWeight < 0.0f)
                        continue;

                    weight += localWeight;
                    sum += mHeightMap[u, v] * localWeight;
                }
            }

            return sum / weight;
        }

        private bool CoordsInRange(int x, int y) {
            return x >= 0 && x < mDims[0] && y >= 0 && y < mDims[1];
        }

        private int[] mDims;
        private float[,] mHeightMap;
    }
}