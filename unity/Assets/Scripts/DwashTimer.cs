using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DwashTimer : MonoBehaviour
{
    public int intDWDuration = 20;
    public int intDWTimer = 0;
    public int DWDone = 0;
    // Start is called before the first frame update
    void Start()
    {
        intDWTimer = 0;
        DWDone = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (intDWTimer >= intDWDuration) {
            DWDone = 1;
        }
        
    }
}
