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
    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    private bool isCreatingChunks;
    private bool isGeneratingWorld;

    public GameObject debugScreen;

    private void Start() {

        Random.InitState(seed);

        spawnPosition = new Vector3(30, 200, 30);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
        

    }

    private void Update() {

        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        if (!isGeneratingWorld) {
            //if player changed chunks since last frame, check viewDistance
            if (!playerChunkCoord.Equals(playerLastChunkCoord))
                CheckViewDistance();

            //if chunks need creating, and the coroutine isn't currently running, start creating chunks
            if (chunksToCreate.Count > 0 && !isCreatingChunks)
                StartCoroutine("CreateChunks");

            if (Input.GetKeyDown(KeyCode.F3))
                debugScreen.SetActive(!debugScreen.activeSelf);
        }

    }

    void GenerateWorld () {
        isGeneratingWorld = true;

        //generate world from -5, -5, -5, to 5, 5, 5
        for (int x = -5; x < 5; x++) {
            for (int y = -5; y < 5; y++) { 
                for (int z = -5; z < 5; z++) {

                    //create chunk x y z and add it to the dictionary
                    chunks[new Vector3(x, y, z)] = new Chunk(new ChunkCoord(x, y, z), this, true);
                    //set chunk x y z as active
                    activeChunks.Add(new ChunkCoord(x, y, z));

                }
            }
        }

        player.position = spawnPosition;
        isGeneratingWorld = false;
    }

    IEnumerator CreateChunks() {

        isCreatingChunks = true;

        //for every chunk in need of generating, generate one, then yield
        while (chunksToCreate.Count > 0) {

            chunks[new Vector3(chunksToCreate[0].x, chunksToCreate[0].y, chunksToCreate[0].z)].Init();
            chunksToCreate.RemoveAt(0);

            yield return null;

        }

        isCreatingChunks = false;

    }

    public ChunkCoord GetChunkCoordFromVector3 (Vector3 pos) {

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt(pos.y / VoxelData.ChunkHeight);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, y, z);

    }

    public Chunk GetChunkFromVector3 (Vector3 pos) {

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int y = Mathf.FloorToInt(pos.y / VoxelData.ChunkHeight);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunks[new Vector3(x,y,z)];

    }

    void CheckViewDistance () {

        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);
        
        // Loop through each chunk according to its x coordinate, then y, then z
        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++) {
            for (int y = coord.y - VoxelData.ViewDistanceInChunks; y < coord.y + VoxelData.ViewDistanceInChunks; y++) {
                    for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++) {

                        //if the dictionary does NOT contain chunk x y z (this chunk)
                        if (chunks.ContainsKey(new Vector3(x, y, z)) == false) {
                            
                            //add chunk x y z to dictionary
                            chunks[new Vector3(x, y, z)] = new Chunk(new ChunkCoord(x, y, z), this, false);
                            
                            //tell the code that chunk x y z needs to be generated
                            chunksToCreate.Add(new ChunkCoord(x, y, z));
                            
                        }
                        //otherwise, if chunk x y z exists in the dictionary, and is not set to active,
                        else if (!chunks[(new Vector3(x, y, z))].isActive) {
                            
                            //activate chunk x y z
                            chunks[(new Vector3(x, y, z))].isActive = true;

                        }
                            activeChunks.Add(new ChunkCoord(x, y, z));


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

    public bool CheckForVoxel(Vector3 pos) {

        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (chunks.ContainsKey(new Vector3(thisChunk.x, thisChunk.y, thisChunk.z)) != false && chunks[new Vector3(thisChunk.x, thisChunk.y, thisChunk.z)].isVoxelMapPopulated)
            return blocktypes[chunks[new Vector3(thisChunk.x, thisChunk.y, thisChunk.z)].GetVoxelFromGlobalVector3(pos)].isSolid;

        return blocktypes[GetVoxel(pos)].isSolid;

    }

    public byte GetVoxel (Vector3 pos) {

        int yPos = Mathf.FloorToInt(pos.y);

        /* BASIC TERRAIN PASS */

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 76000, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = 3;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 2;
        else if (yPos > terrainHeight)
            return 0;
        else
            voxelValue = 1;

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
