using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Neolithica.TerrainGeneration {
    public class ResourcePlacer {
        public ResourcePlacer(TerrainData terrainData, float waterHeight, float seed, IEnumerable<ResourceSettings> resourceSettings) {
            mTerrainData = terrainData;
            mSeed = seed;
            mWaterHeight = waterHeight;
            mResourceSettings = resourceSettings.ToDictionary(rs => rs.Type);
        }

        public ResourcePlacementType GetPlacementType(float x, float y) {
            float height = mTerrainData.GetInterpolatedHeight(x, y) / mTerrainData.size.y;

            var matches = sPlacementTypes
                .Select(type => new
                {
                    Fitness = GetFitness(
                        x: x,
                        y: y,
                        height: height,
                        fitnessHelper: sFitnessHelpers[type],
                        resourceSetting: mResourceSettings[type],
                        offset: sResourceGenOffsets[type]),
                    Type = type
                })
                .Where(it => it.Fitness > 0)
                .ToList();

            return matches.Any()
                ? matches.OrderByDescending(it => it.Fitness).First().Type
                : ResourcePlacementType.None;
        }

        private float GetFitness(
            float x,
            float y,
            float height,
            Func<float, float, float> fitnessHelper,
            ResourceSettings resourceSetting,
            float offset)
        {
            float noise = Mathf.PerlinNoise(
                mSeed + resourceSetting.Frequency * x + offset,
                mSeed + resourceSetting.Frequency * y + offset);

            float fitness = fitnessHelper(height, mWaterHeight) * noise;

            if (fitness > 1.0f || fitness < 0.0f) {
                Debug.Log($"Fitness value out of bounds: {fitness}");
            }

            if (fitness > 1 - resourceSetting.Abundance)
                return fitness;

            return -1.0f;
        }

        private static readonly IReadOnlyList<ResourcePlacementType> sPlacementTypes = 
            new List<ResourcePlacementType> {
                ResourcePlacementType.Berries,
                ResourcePlacementType.Trees,
                ResourcePlacementType.Fish,
                ResourcePlacementType.Stone,
                ResourcePlacementType.Gold,
                ResourcePlacementType.Doodad,
            };

        private static readonly IReadOnlyDictionary<ResourcePlacementType, float> sResourceGenOffsets =
            new Dictionary<ResourcePlacementType, float> {
                [ResourcePlacementType.Berries] = 0.0f,
                [ResourcePlacementType.Trees] = 1.5f,
                [ResourcePlacementType.Fish] = 3.4f,
                [ResourcePlacementType.Gold] = 3.7f,
                [ResourcePlacementType.Stone] = 6.9f,
                [ResourcePlacementType.Doodad] = 10.0f,
            };

        private static float FishFitness(float height, float waterHeight) {
            return height < waterHeight ? 1.0f : 0.0f;
        }

        private static float FertilityFitness(float height, float waterHeight) {
            return height > waterHeight ? Mathf.Pow((1.0f - height) / (1.0f - waterHeight), 2) : 0.0f;
        }

        private static float MountainFitness(float height, float waterHeight) {
            return height > waterHeight ? Mathf.Pow(height / (0.3f - waterHeight), 1.5f) : 0.0f;
        }

        private static float DryLandFitness(float height, float waterHeight) {
            return height > waterHeight ? 1.0f : 0.0f;
        }

        private static readonly IReadOnlyDictionary<ResourcePlacementType, Func<float, float, float>> sFitnessHelpers =
            new Dictionary<ResourcePlacementType, Func<float, float, float>> {
                [ResourcePlacementType.Berries] = FertilityFitness,
                [ResourcePlacementType.Trees] = FertilityFitness,
                [ResourcePlacementType.Fish] = FishFitness,
                [ResourcePlacementType.Gold] = MountainFitness,
                [ResourcePlacementType.Stone] = MountainFitness,
                [ResourcePlacementType.Doodad] = DryLandFitness,
            };

        private readonly TerrainData mTerrainData;
        private readonly IReadOnlyDictionary<ResourcePlacementType, ResourceSettings> mResourceSettings;
        private readonly float mSeed;
        private readonly float mWaterHeight;
    }
}
