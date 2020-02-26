using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

    public int seed;
    public BiomeAttributes biome;

    public Transform player;
    public Vector3 spawnPosition;

    public Material material;
    public BlockType[] blocktypes;

    Dictionary<Vector3, Chunk> chunks = new Dictionary<Vector3, Chunk>();

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private void Start() {

        Random.InitState(seed);

        spawnPosition = new Vector3(0, 70, 0);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        

    }

    private void Update() {

        /*playerChunkCoord = GetChunkCoordFromVector3(player.position);

        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();*/

    }

    void GenerateWorld () {

        for (int x = -5; x < 5; x++) {
            for (int y = -5; y < 5; y++) { 
                for (int z = -5; z < 5; z++) {

                    CreateNewChunk(x, y, z);

                }
            }
        }

        player.position = spawnPosition;

    }

    ChunkCoord GetChunkCoordFromVector3 (Vector3 pos) {

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt(pos.y / VoxelData.ChunkHeight);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, y, z);

    }

    void CheckViewDistance () {

        ChunkCoord coord = GetChunkCoordFromVector3(player.position);

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);
        

        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++) {
            for (int y = coord.y - VoxelData.ViewDistanceInChunks; y < coord.y + VoxelData.ViewDistanceInChunks; y++) {
                    for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++) {

                            if (chunks.ContainsKey(new Vector3 (x, y, z)) == false)
                                CreateNewChunk(x, y, z);
                            else if (!chunks[(new Vector3(x, y, z))].isActive) {

                            chunks[(new Vector3(x, y, z))].isActive = true;
                                activeChunks.Add(new ChunkCoord(x, y, z));
                                
                            }
                        

                        for (int i = previouslyActiveChunks.Count - 1; i > -1; i--) {

                            if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, y, z)))
                                previouslyActiveChunks.RemoveAt(i);

                        }


                    }

                    

            }
            
            
        }

        foreach (ChunkCoord c in previouslyActiveChunks)
            chunks[(new Vector3(c.x, c.y, c.z))].isActive = false;
        
    }

    public bool CheckForVoxel(float _x, float _y, float _z) {

        int xCheck = Mathf.FloorToInt(_x);
        int yCheck = Mathf.FloorToInt(_y);
        int zCheck = Mathf.FloorToInt(_z);

        int xChunk = xCheck / VoxelData.ChunkWidth;
        int yChunk = yCheck / VoxelData.ChunkHeight;
        int zChunk = zCheck / VoxelData.ChunkWidth;

        xCheck -= (xChunk * VoxelData.ChunkWidth);
        yCheck -= (yChunk * VoxelData.ChunkHeight);
        zCheck -= (zChunk * VoxelData.ChunkWidth);
        return blocktypes[chunks[new Vector3(xChunk, yChunk, zChunk)].voxelMap[xCheck, yCheck, zCheck]].isSolid;

    }

    public byte GetVoxel (Vector3 pos) {

        int yPos = Mathf.FloorToInt(pos.y);

        /* BASIC TERRAIN PASS */

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 76000, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = 3;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 4;
        else if (yPos > terrainHeight)
            return 0;
        else
            voxelValue = 2;

        /* SECOND PASS */

        if (voxelValue == 2) {

            foreach(Lode lode in biome.lodes) {

                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;

            }

        }
         return voxelValue;

    }

    void CreateNewChunk (int x, int y, int z) {

        chunks.Add(new Vector3(x, y, z), new Chunk(new ChunkCoord(x, y, z), this));
        activeChunks.Add(new ChunkCoord(x, y, z));

    }


}

[System.Serializable]
public class BlockType {

    public string blockName;
    public bool isSolid;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public int GetTextureID(int faceIndex) {

        switch (faceIndex) {

            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;

        }
    }

}
