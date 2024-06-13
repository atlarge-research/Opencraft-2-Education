﻿namespace Opencraft.Terrain.Blocks
{
    // The enum of all supported types
    public enum BlockType : byte
    {
        Air,
        Stone,
        Dirt,
        Tin,
        Gem,
        Grass,
        Leaf,
        Wood,
        Unbreakable,
        Power,
        Off_Wire,
        On_Wire,
        Off_Lamp,
        On_Lamp,
    }
    public static class BlockData
    {
        // Maps BlockType to texture array index, currently 1 to 1
        public static readonly int[] BlockToTexture =
        {
            0,
            (1 & 31) << 24,
            (2 & 31) << 24,
            (3 & 31) << 24,
            (4 & 31) << 24,
            (5 & 31) << 24,
            (6 & 31) << 24,
            (7 & 31) << 24,
            (8 & 31) << 24,
            (9 & 31) << 24,
            (10 & 31) << 24,
            (11 & 31) << 24,
            (12 & 31) << 24,
            (13 & 31) << 24,
        };

        // UV sizing > 1 tiles a texture across multiple blocks, currently not done for any block types
        public static float[] BlockUVSizing = new float[]
        {
            1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f
        };

        public static readonly bool[] PowerableBlock = new bool[]
        {
            false, false, false, false, false, false, false, false, false, false, true, true, true, true
        };

        public static readonly BlockType[] DepoweredState = new BlockType[]
        {
            BlockType.Air,
            BlockType.Stone,
            BlockType.Dirt,
            BlockType.Tin,
            BlockType.Gem,
            BlockType.Grass,
            BlockType.Leaf,
            BlockType.Wood,
            BlockType.Unbreakable,
            BlockType.Power,
            BlockType.Off_Wire,
            BlockType.Off_Wire,
            BlockType.Off_Lamp,
            BlockType.Off_Lamp,
        };
        public static readonly BlockType[] PoweredState = new BlockType[]
        {
            BlockType.Air,
            BlockType.Stone,
            BlockType.Dirt,
            BlockType.Tin,
            BlockType.Gem,
            BlockType.Grass,
            BlockType.Leaf,
            BlockType.Wood,
            BlockType.Unbreakable,
            BlockType.Power,
            BlockType.On_Wire,
            BlockType.On_Wire,
            BlockType.On_Lamp,
            BlockType.On_Lamp,
        };
    }


}