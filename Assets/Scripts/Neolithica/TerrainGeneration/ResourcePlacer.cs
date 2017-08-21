using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Neolithica.TerrainGeneration {
    public class ResourcePlacer {
        public ResourcePlacer(TerrainData terrainData, float waterHeight, NewGameSettings settings, IEnumerable<ResourceSettings> resourceSettings) {
            mTerrainData = terrainData;
            mSettings = settings;
            mWaterHeight = waterHeight;
            mResourceSettings = resourceSettings.ToDictionary(rs => rs.Type);
        }

        public ResourcePlacementType GetPlacementType(float x, float y) {
            float height = mTerrainData.GetInterpolatedHeight(x, y) / mTerrainData.size.y;

            var matches2 = mPlacementTypes
                .Select(type => new
                {
                    fitness = GetFitness(
                        x: x,
                        y: y,
                        height: height,
                        waterHeight: mWaterHeight,
                        fitnessHelper: fitnessHelpers[type],
                        resourceSetting: mResourceSettings[type],
                        offset: resourceGenOffset[type]),
                    type
                })
                .ToList();

            return matches2.Any()
                ? matches2.OrderByDescending(it => it.fitness).First().type
                : ResourcePlacementType.None;
        }

        private float GetFitness(float x, float y, float height, float waterHeight, Func<float, float, float> fitnessHelper, ResourceSettings resourceSetting, float offset) {
            float noise = Mathf.PerlinNoise(
                mSettings.seed + resourceSetting.Frequency * x + offset,
                mSettings.seed + resourceSetting.Frequency * y + offset);

            float fitness = fitnessHelper(height, waterHeight) * noise;

            if (fitness > 1 - resourceSetting.Abundance)
                return fitness;

            return -1.0f;
        }

        private readonly List<ResourcePlacementType> mPlacementTypes = 
            new List<ResourcePlacementType> {
                ResourcePlacementType.Berries,
                ResourcePlacementType.Trees,
                ResourcePlacementType.Fish,
                ResourcePlacementType.Stone,
                ResourcePlacementType.Gold,
                ResourcePlacementType.Doodad,
            };

        private readonly Dictionary<ResourcePlacementType, float> resourceGenOffset =
            new Dictionary<ResourcePlacementType, float> {
                [ResourcePlacementType.Berries] = 0.0f,
                [ResourcePlacementType.Trees]   = 1.5f,
                [ResourcePlacementType.Fish]    = 3.4f,
                [ResourcePlacementType.Gold]    = 3.7f,
                [ResourcePlacementType.Stone]   = 6.9f,
                [ResourcePlacementType.Doodad]  = 10.0f,
            };

        private readonly Dictionary<ResourcePlacementType, ResourceSettings> mResourceSettings;

        private static float FishFitness(float height, float waterHeight) {
            return height < waterHeight ? 1.0f : 0.0f;
        }

        private static float FertilityFitness(float height, float waterHeight) {
            return height > waterHeight ? Mathf.Pow((1.0f - height) / (1.0f - waterHeight), 2) : 0.0f;
        }

        private static float MountainFitness(float height, float waterHeight) {
            return height > waterHeight ? Mathf.Pow(height / (1.0f - waterHeight), 1.5f) : 0.0f;
        }

        private static float DryLandFitness(float height, float waterHeight) {
            return height > waterHeight ? 1.0f : 0.0f;
        }

        private readonly Dictionary<ResourcePlacementType, Func<float, float, float>> fitnessHelpers =
            new Dictionary<ResourcePlacementType, Func<float, float, float>> {
                {ResourcePlacementType.Berries, FertilityFitness},
                {ResourcePlacementType.Trees, FertilityFitness},
                {ResourcePlacementType.Fish, FishFitness},
                {ResourcePlacementType.Gold, MountainFitness},
                {ResourcePlacementType.Stone, MountainFitness},
                {ResourcePlacementType.Doodad, DryLandFitness},
            };

        private readonly TerrainData mTerrainData;
        private readonly NewGameSettings mSettings;
        private readonly float mWaterHeight;
    }
}
