using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Neolithica.TerrainGeneration {
    public class ResourcePlacer {
        public ResourcePlacer(TerrainData terrainData, float waterHeight, float seed, IEnumerable<ResourceSettings> resourceSettings) {
            mTerrainData = terrainData;
            mSeed = seed;
            mResourceSettings = resourceSettings.ToDictionary(rs => rs.Type);

            //Func<float, float> fertility = height => FitnessHelpers.PivotGradient(height, waterHeight, waterHeight + 0.025f, waterHeight + 0.2f);
            Func<float, float> fertility = height => FitnessHelpers.Gradient(height, waterHeight + 0.5f, waterHeight);
            Func<float, float> stoneAndMetal = height => FitnessHelpers.PivotGradient(height, waterHeight, waterHeight + 0.1f, waterHeight + 0.25f);
                
            mFitnessHelpers = new Dictionary<ResourcePlacementType, Func<float, float>> {
                [ResourcePlacementType.Berries] = fertility,
                [ResourcePlacementType.Trees] = fertility,
                [ResourcePlacementType.Fish] = (height) => FitnessHelpers.Water(height, waterHeight),
                [ResourcePlacementType.Gold] = stoneAndMetal,
                [ResourcePlacementType.Stone] = stoneAndMetal,
                [ResourcePlacementType.Doodad] = (height) => FitnessHelpers.DryLand(height, waterHeight),
            };
        }

        public ResourcePlacementType GetPlacementType(float x, float y) {
            float height = mTerrainData.GetInterpolatedHeight(x, y) / mTerrainData.size.y;

            var matches = mFitnessHelpers.Keys
                .Select(type => new
                {
                    Fitness = GetFitness(
                        x: x,
                        y: y,
                        height: height,
                        fitnessHelper: mFitnessHelpers[type],
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
            Func<float, float> fitnessHelper,
            ResourceSettings resourceSetting,
            float offset)
        {
            float noise = Mathf.PerlinNoise(
                mSeed + resourceSetting.Frequency * x + offset,
                mSeed + resourceSetting.Frequency * y + offset);

            float fitness = fitnessHelper(height) * noise;

            if (fitness > 1.0f || fitness < 0.0f) {
                Debug.Log($"Fitness value out of bounds: {fitness}");
            }

            if (fitness > 1 - resourceSetting.Abundance * sAbundanceModifiers[resourceSetting.Type])
                return fitness;

            return -1.0f;
        }

        private static readonly IReadOnlyDictionary<ResourcePlacementType, float> sResourceGenOffsets =
            new Dictionary<ResourcePlacementType, float> {
                [ResourcePlacementType.Berries] = 0.0f,
                [ResourcePlacementType.Trees] = 1.5f,
                [ResourcePlacementType.Fish] = 3.4f,
                [ResourcePlacementType.Gold] = 3.7f,
                [ResourcePlacementType.Stone] = 6.9f,
                [ResourcePlacementType.Doodad] = 10.0f,
            };

        private static readonly IReadOnlyDictionary<ResourcePlacementType, float> sAbundanceModifiers =
            new Dictionary<ResourcePlacementType, float> {
                [ResourcePlacementType.Berries] = 1.0f,
                [ResourcePlacementType.Trees] = 1.0f,
                [ResourcePlacementType.Fish] = 1.0f,
                [ResourcePlacementType.Gold] = 1.0f,
                [ResourcePlacementType.Stone] = 1.0f,
                [ResourcePlacementType.Doodad] = 1.0f,
            };

        private readonly TerrainData mTerrainData;
        private readonly IReadOnlyDictionary<ResourcePlacementType, ResourceSettings> mResourceSettings;
        private readonly IReadOnlyDictionary<ResourcePlacementType, Func<float, float>> mFitnessHelpers;
        private readonly float mSeed;
    }
}
