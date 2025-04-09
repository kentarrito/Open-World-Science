using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct UniqueJointInfo
{
    public List<Vector3> center_lposi_list;
    public List<Vector3[]> dir_lea_list;  //Vector3[0] and Vector3[1] are the directions child_obj's y axis and x axis should be oriented to. 

    public string center_area_eq;
    public string dir_eq;

    public float layer;
}
