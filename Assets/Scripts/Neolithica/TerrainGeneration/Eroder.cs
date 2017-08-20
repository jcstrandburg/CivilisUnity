using System;
using Neolithica.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Neolithica.TerrainGeneration {
    public class Eroder {

        public float Deposits;
        public float Erosions;
        public float PitDeposits;
        public int ErosionsSkipped = 0;
        public int DepositsSkipped = 0;

        public Eroder(float[,] heightMap, EroderSettings settings) {
            mHeightMap = heightMap;
            mSettings = settings;
        }

        // runs an erosion simulation for 1 water particle on the height map
        public void Erode() {
            const int maxTicks = 100;

            int w = mHeightMap.GetLength(0);
            int h = mHeightMap.GetLength(1);
            Vector2 currentPos = new Vector2(Random.Range(0, w - 1), Random.Range(0, h - 1));

            float soil = 0.0f;
            float vel = 0.0f;
            float water = 1.0f;

            Vector2 direction = GetGradient(currentPos);
            while (direction.sqrMagnitude <= 0.0000001f) {
                direction = Random.insideUnitCircle;
            }
            direction = direction.normalized;

            int tickCount = 0;
            while (water > 0.001f && tickCount < maxTicks) {
                Vector2 gradient = GetGradient(currentPos);
                direction = Vector2.Lerp(direction, gradient, mSettings.Inertia).normalized;
                Vector2 newPos = currentPos + direction;

                if (!IsInsideMap(newPos))
                    return;

                float heightOld = GetHeight(currentPos);
                float heightNew = GetHeight(newPos);
                float heightDiff = heightNew - heightOld;

                if (heightDiff > 0.0f) {
                    float x = Mathf.Min(soil, heightDiff);
                    soil -= x;
                    DepositAt(currentPos, x);
                    PitDeposits += x;
                }
                else {
                    float capacity = Mathf.Max(-heightDiff, mSettings.MinSlope) * vel * water * mSettings.Capacity;
                    if (soil < capacity) {
                        float x = Mathf.Min((capacity - soil) * mSettings.Erosion, -heightDiff * 3.0f);
                        soil += x;
                        ErodeAt(currentPos, x, mSettings.ErosionRadius);
                    }
                    else {
                        float x = (soil - capacity) * mSettings.Deposition;
                        soil -= x;
                        DepositAt(currentPos, x);
                    }
                }

                if (soil < -0.0001f)
                    throw new InvalidOperationException("Unexpected negative soil value");

                float velSquared = vel * vel + heightDiff * mSettings.Gravity;
                if (velSquared > 0)
                    vel = Mathf.Sqrt(velSquared);
                else
                    vel = 0;

                if (Single.IsNaN(vel))
                    throw new InvalidOperationException("vel NaN detected");

                water = water * (1 - mSettings.Evaporation);
                currentPos = newPos;
                tickCount++;
            }
        }

        private void DepositAt(Vector2 pos, float f) {
            IntVector intPos = (IntVector)pos;

            if (f < 0)
                throw new ArgumentOutOfRangeException("f");

            Deposits += f;

            float xInterp = pos.x - intPos.x;
            float yInterp = pos.y - intPos.y;

            if (IsInsideMap(intPos.x + 1, intPos.y))
                mHeightMap[intPos.y, intPos.x + 1] += (xInterp + 1 - yInterp) * 0.25f * f;

            if (IsInsideMap(intPos.x, intPos.y + 1))
                mHeightMap[intPos.y + 1, intPos.x] += (1 - xInterp + yInterp) * 0.25f * f;

            if (IsInsideMap(intPos.x + 1, intPos.y + 1))
                mHeightMap[intPos.y + 1, intPos.x + 1] += (xInterp + yInterp) * 0.25f * f;

            if (IsInsideMap(intPos.x, intPos.y))
                mHeightMap[intPos.y, intPos.x] += (2 - xInterp - yInterp) * 0.25f * f;
        }

        private void ErodeAt(Vector2 pos, float f, float radius) {
            if (radius < 1.0f)
                throw new ArgumentOutOfRangeException("radius", "Radius must be at least 1.0");

            int sampleRange = (int)radius;
            float[,] weights = new float[sampleRange * 2 + 1, sampleRange * 2 + 1];
            float totalWeight = 0;

            for (int i = -sampleRange; i <= sampleRange; i++) {
                for (int j = -sampleRange; j <= sampleRange; j++) {
                    int u = (int)pos.x + i;
                    int v = (int)pos.y + j;

                    if (!IsInsideMap(u, v))
                        continue;

                    float localWeight = radius - Mathf.Sqrt((pos.x - u) * (pos.x - u) + (pos.y - v) * (pos.y - v));

                    if (localWeight < 0)
                        continue;

                    totalWeight += localWeight;
                    weights[i + sampleRange, j + sampleRange] = localWeight;
                }
            }

            for (int i = -sampleRange; i <= sampleRange; i++) {
                for (int j = -sampleRange; j <= sampleRange; j++) {

                    int u = (int)pos.x + i;
                    int v = (int)pos.y + j;

                    if (weights[i + sampleRange, j + sampleRange] <= 0)
                        continue;

                    mHeightMap[u, v] -= f * weights[i + sampleRange, j + sampleRange] / totalWeight;
                }
            }
        }

        private void ErodeAt(Vector2 pos, float f) {
            IntVector intPos = (IntVector)pos;

            if (f < 0)
                throw new ArgumentOutOfRangeException("f");

            Erosions += f;

            float xInterp = pos.x - intPos.x;
            float yInterp = pos.y - intPos.y;

            if (IsInsideMap(intPos.x + 1, intPos.y))
                mHeightMap[intPos.y, intPos.x + 1] -= (xInterp + 1 - yInterp) * 0.25f * f;

            if (IsInsideMap(intPos.x, intPos.y + 1))
                mHeightMap[intPos.y + 1, intPos.x] -= (1 - xInterp + yInterp) * 0.25f * f;

            if (IsInsideMap(intPos.x + 1, intPos.y + 1))
                mHeightMap[intPos.y + 1, intPos.x + 1] -= (xInterp + yInterp) * 0.25f * f;

            if (IsInsideMap(intPos.x, intPos.y))
                mHeightMap[intPos.y, intPos.x] -= (2 - xInterp - yInterp) * 0.25f * f;
        }

        private bool IsInsideMap(Vector2 pos) {
            return pos.x >= 0 && pos.x < mHeightMap.GetLength(1)
                   && pos.y >= 0 && pos.y < mHeightMap.GetLength(0);
        }

        private bool IsInsideMap(IntVector pos) {
            return IsInsideMap(pos.x, pos.y);
        }

        private bool IsInsideMap(int x, int y) {
            return x >= 0 && x < mHeightMap.GetLength(1)
                   && y >= 0 && y < mHeightMap.GetLength(0);
        }

        private float GetHeight(Vector2 pos) {
            IntVector intPos = (IntVector)pos;
            return mHeightMap[intPos.y, intPos.x];
        }

        private float GetHeight(IntVector pos) {
            return mHeightMap[pos.y, pos.x];
        }

        private Vector2 GetGradient(Vector2 pos) {
            IntVector p00 = (IntVector)pos;
            IntVector p01 = new IntVector(p00.x, p00.y + 1);
            IntVector p10 = new IntVector(p00.x + 1, p00.y);
            IntVector p11 = new IntVector(p00.x + 1, p00.y + 1);

            float h00 = GetHeight(p00);
            float h01 = IsInsideMap(p01) ? GetHeight(p01) : h00;
            float h10 = IsInsideMap(p10) ? GetHeight(p10) : h00;
            float h11 = IsInsideMap(p11) ? GetHeight(p11) : h00;

            return new Vector2(
                x: h00 + h01 - h10 - h11,
                y: h00 + h10 - h01 - h11
            );
        }

        private readonly float[,] mHeightMap;
        private readonly EroderSettings mSettings;
    }
}