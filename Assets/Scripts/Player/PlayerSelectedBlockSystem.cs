﻿using Opencraft.Player.Authoring;
using Opencraft.Terrain.Authoring;
using Opencraft.Terrain.Blocks;
using Opencraft.Terrain.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Opencraft.Player
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PlayerMovementSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ClientSimulation)]
    [BurstCompile]
    // For every player, calculate what block, if any, they have selected
    public partial struct PlayerSelectedBlockSystem : ISystem
    {
        private BufferLookup<TerrainBlocks> _terrainBlockLookup;
        private ComponentLookup<TerrainNeighbors> _terrainNeighborLookup;
        private NativeArray<Entity> terrainAreasEntities;
        private NativeArray<LocalTransform> terrainAreaTransforms;
        private static readonly int raycastLength = 5;
        private static readonly float3 camOffset = new float3(0, Env.CAMERA_Y_OFFSET, 0);
        
        // Reusable block search input/output structs
        private TerrainUtilities.BlockSearchInput BSI;
        private TerrainUtilities.BlockSearchOutput BSO;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

            state.RequireForUpdate<Authoring.Player>();
            state.RequireForUpdate<TerrainArea>();
            state.RequireForUpdate<TerrainSpawner>();
            _terrainBlockLookup = state.GetBufferLookup<TerrainBlocks>(true);
            _terrainNeighborLookup = state.GetComponentLookup<TerrainNeighbors>(true);
            
            TerrainUtilities.BlockSearchInput.DefaultBlockSearchInput(ref BSI);
            TerrainUtilities.BlockSearchOutput.DefaultBlockSearchOutput(ref BSO);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.CompleteDependency();
            _terrainBlockLookup.Update(ref state);
            _terrainNeighborLookup.Update(ref state);
            var terrainAreasQuery = SystemAPI.QueryBuilder().WithAll<TerrainArea, LocalTransform>().Build();
            terrainAreasEntities = terrainAreasQuery.ToEntityArray(state.WorldUpdateAllocator);
            terrainAreaTransforms = terrainAreasQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            
            
            foreach (var player in SystemAPI.Query<PlayerAspect>().WithAll<Simulate>())
            {
                player.SelectedBlock.blockLoc = new int3(-1);
                player.SelectedBlock.terrainArea = Entity.Null;
                player.SelectedBlock.neighborBlockLoc = new int3(-1);
                player.SelectedBlock.neighborTerrainArea = Entity.Null;

                if (player.Player.ContainingArea == Entity.Null)
                    continue;
                
                // Use player input Yaw/Pitch to calculate the camera direction on clients
                var cameraRot =  math.mul(quaternion.RotateY(player.Input.Yaw),
                    quaternion.RotateX(-player.Input.Pitch));
                var direction = math.mul(cameraRot, math.forward());
                //var cameraPos = player.Transform.ValueRO.Position + camOffset ;
                Entity neighborTerrainArea = Entity.Null;
                int3 neighborBlockLoc = new int3(-1);
                

                // Setup search inputs
                TerrainUtilities.BlockSearchInput.DefaultBlockSearchInput(ref BSI);
                BSI.basePos = player.Transform.ValueRO.Position;
                BSI.areaEntity = player.Player.ContainingArea;
                BSI.terrainAreaPos = player.Player.ContainingAreaLocation;
                
                // Step along a ray from the players position in the direction their camera is looking
                for (int i = 0; i < raycastLength; i++)
                {
                    //float3 location = cameraPos + (direction * i);
                    TerrainUtilities.BlockSearchOutput.DefaultBlockSearchOutput(ref BSO);
                    BSI.offset = camOffset + (direction * i);
                    if (TerrainUtilities.GetBlockAtPositionByOffset(in BSI, ref BSO,
                            ref _terrainNeighborLookup, ref _terrainBlockLookup))
                    {
                        if (BSO.blockType != BlockType.Air)
                        {
                            // found selected block
                            player.SelectedBlock.blockLoc = BSO.localPos;
                            player.SelectedBlock.terrainArea = BSO.containingArea ;
                            // Set neighbor
                            player.SelectedBlock.neighborBlockLoc = neighborBlockLoc;
                            player.SelectedBlock.neighborTerrainArea = neighborTerrainArea;
                                
                            break;
                        }
                        // If this block is air, still mark it as the neighbor
                        neighborTerrainArea = BSO.containingArea;
                        neighborBlockLoc = BSO.localPos;
                    }
                    
                    
                    
                    /*if (TerrainUtilities.GetBlockLocationAtPosition(ref location,
                            ref terrainAreaTransforms,
                            out int terrainAreaIndex,
                            out int3 blockLoc))
                    {
                        Entity terrainAreaEntity = terrainAreasEntities[terrainAreaIndex];
                        Entity neighborTerrainAreaEntity = neighborTerrainAreaIndex != -1 ?
                            terrainAreasEntities[neighborTerrainAreaIndex] : Entity.Null;
                        if (_terrainBlockLookup.TryGetBuffer(terrainAreaEntity,
                                out DynamicBuffer<TerrainBlocks> terrainBlocks))
                        {
                            int blockIndex = TerrainUtilities.BlockLocationToIndex(ref blockLoc);
                            if (terrainBlocks[blockIndex].type != BlockType.Air)
                            {
                                // found selected block
                                player.SelectedBlock.blockLoc = blockLoc;
                                player.SelectedBlock.terrainArea = terrainAreaEntity ;
                                // Set neighbor
                                player.SelectedBlock.neighborBlockLoc = neighborBlockLoc;
                                player.SelectedBlock.neighborTerrainArea = neighborTerrainAreaEntity;
                                
                                break;
                            }
                            // If this block is air, still mark it as the neighbor
                            neighborTerrainAreaIndex = terrainAreaIndex;
                            neighborBlockLoc = blockLoc;
                            
                        }
                    }*/
                }
            }
            

        }
    }
}