using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoController : MonoBehaviour {

    private GameObject planet;
    private GameObject sunLight;
    private MapGenerator mapGen;

    void Awake() { 
        planet = GameObject.FindGameObjectWithTag("GGPlanet");
        sunLight = GameObject.FindGameObjectWithTag("SunLight");
        mapGen = FindObjectOfType<MapGenerator>();
        mapGen.GroundGrowing();
	}
	
	// Update is called once per frame
	void Update () {
        planet.transform.Rotate(Vector3.down * Time.deltaTime);
        sunLight.transform.Rotate(Vector3.up * Time.deltaTime *1.3f);
		
	}

    private void OnApplicationQuit()
    {
        mapGen.HaltThreads();
    }

    private void OnDestroy()
    {
        mapGen.HaltThreads();
    }
}
