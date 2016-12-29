using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof (MapGenerator))]
public class MapGenEditor : Editor {

    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;
        if(DrawDefaultInspector())
        {
            //error control
            if (mapGen.mapWidth < 1) mapGen.mapWidth = 1;
            if (mapGen.mapHeight < 1) mapGen.mapHeight = 1;

            //going through the noise functions
            for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
            {
                //transfers height / width
                mapGen.noiseFunctions[i].height = mapGen.mapHeight;
                mapGen.noiseFunctions[i].width = mapGen.mapWidth;
            
                

                //seed distribution, lots of i's scattered for fun
                if (!mapGen.useRandomSeed)
                {
                    mapGen.seedValue = mapGen.seed.GetHashCode()*i;
                }

                else
                {
                    mapGen.seedValue = Random.Range(0, 10000000);
                }
                mapGen.noiseFunctions[i].seed = mapGen.seedValue + i;
            }

            //autoupdate function, so far it's really slow, may remove
            if (mapGen.autoUpdate)
            {
                mapGen.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateMap();
        }
    }
    
}
