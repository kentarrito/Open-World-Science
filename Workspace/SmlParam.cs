using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmlParam : MonoBehaviour
{
    Dictionary<string, float[]> param_dict;

    // Start is called before the first frame update
    void Start()
    {
        param_dict = new Dictionary<string, float[]>();
    }

}

interface ISimulationHandler
{
	void SetParameters();
	void SimulationStep();
}

