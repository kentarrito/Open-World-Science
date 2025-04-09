using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoldInfo : MonoBehaviour
{
    public string mold_name;

    public string collider_type;

    // if collider_type:sphere, collider_size.x=radius   if collider_type:capsule, collider_size.x=radius, collider_size.y=height
    public Vector3 collider_size=new Vector3();
    
    public Vector3 collider_center=new Vector3();
    public Vector3 collider_direction=new Vector3();

    public List<Vector3> hole_posis=new List<Vector3>();
    public List<Vector3> hole_normals=new List<Vector3>();
    public List<float> hole_depthes=new List<float>();

    public List<SphereColliderCtr> hole_collider_list = new List<SphereColliderCtr>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
}
