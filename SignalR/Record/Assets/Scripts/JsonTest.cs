using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class JsonTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        JsonConvert.SerializeObject(new { test = "test" });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
