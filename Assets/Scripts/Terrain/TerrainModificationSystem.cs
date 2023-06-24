﻿using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Rendering;
using UnityEngine;

namespace Opencraft.Terrain
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TerrainGenerationSystem))]
    [BurstCompile]
    // System responsible for editing terrain area block buffers and marking those areas as needing to be remeshed
    public partial struct TerrainModificationSystem : ISystem
    {
        private double lastUpdate;
        private bool updateAll;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TerrainSpawner>();
            state.RequireForUpdate<TerrainArea>();
            lastUpdate = -5.0;
            updateAll = true;
        }

        public void OnUpdate(ref SystemState state)
        {

            if (state.World.Time.ElapsedTime - lastUpdate < 5.0)
            {
                return;
            }
            lastUpdate = state.World.Time.ElapsedTime;
            
            // Stand-in system, currently just inverts the terrain area at 0,0,0 every 5 seconds
            foreach (var (terrainArea, entity) in SystemAPI.Query<RefRO<TerrainArea>>()
                         .WithEntityAccess())
            {
                if (!terrainArea.ValueRO.location.Equals(new int3(0, 0, 0)))
                {
                    continue;
                }

                DynamicBuffer<TerrainBlocks> blocksBuffer =
                    state.EntityManager.GetBuffer<TerrainBlocks>(entity, isReadOnly: false);
                DynamicBuffer<int> blocks = blocksBuffer.Reinterpret<int>();
                if (updateAll)
                {
                    for (int i = 0; i < blocks.Length; i++)
                    {
                        blocks[i] = blocks[i] == 1 ? -1 : 1;
                    }
                }
                else
                {
                    blocks[0] = blocks[0] == 1 ? -1 : 1;
                }

                // Mark that these area need to be meshed
                // TODO how does this interact with clients marking remesh as false? may be better to do using RPC
                state.EntityManager.SetComponentEnabled<Remesh>(entity, true);
            }

        }
    }
}