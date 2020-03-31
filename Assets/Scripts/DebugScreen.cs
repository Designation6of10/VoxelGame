﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{

    World world;
    Text text;
    public Transform highlight;

    float frameRate;
    float timer;

    void Start() {

        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();

    }


    void Update() {

        string debugText = "Voxel Game";
        debugText += "\n";
        debugText += frameRate + " fps";
        debugText += "\n\n";
        debugText += "XYZ: " + world.player.transform.position.x + " / " + world.player.transform.position.y + " / " + world.player.transform.position.z;
        debugText += "\n";
        debugText += "Chunk " + world.playerChunkCoord.x + " / " + world.playerChunkCoord.y + " / " + world.playerChunkCoord.z;
        debugText += "\n";
        debugText += "Looking at block " + Mathf.FloorToInt(highlight.position.x) + " / " + Mathf.FloorToInt(highlight.position.y) + " / " + Mathf.FloorToInt(highlight.position.z);


        text.text = debugText;

        if (timer > 1f) {

            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;

        }
        else
            timer += Time.deltaTime;

    }
}