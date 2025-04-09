using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneCtr : MonoBehaviour
{
    public GameObject cube;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.cube.transform.eulerAngles += new Vector3(0, 1, 0);
    }

    public void GameStart(){
        SceneManager.LoadScene(1);
    }
}
