using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraCtr : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region variables

    WorkspaceCtr WorkspaceCtr;
    public static bool is_pointer_on_camera_window;

    //Camera
    public Camera MainCamera;
    public GameObject OutlineCamera;
    public GameObject OutlinedObjCamera;
    public GameObject Axes;
    public static Vector3 cam_eA;
    public static Vector3 cam_center;
    public static float cam_len;
    float coe_scroll = 0.01f;
    float coe_angle = 0.1f;
    public static float coe_move=0.02f;

    //transformパネル
    int transform_mode = 1; //1:posi 2:rot 3:scl
    public Transform x1;
    public Transform x2;
    public Transform y1;
    public Transform y2;
    public Transform z1;
    public Transform z2;

    public Transform x1_rot;
    public Transform x2_rot;
    public Transform y1_rot;
    public Transform y2_rot;
    public Transform z1_rot;
    public Transform z2_rot;

    Vector2 posi1;
    bool is_raycast_hit=false;
    Vector3 move_point;
    float hit_to_camera;

    //obj grab
    bool is_count = false;
    int update_count = 0;
    
    public static GameObject grabbed_obj;
    public static bool is_pointer_in_input;
    public static bool is_pointer_in_output;
    Vector2 start_mouse_posi;
    Vector2 force_dir;

    //obj select
    bool is_obj_grabbed;
    bool is_dragged=false;

    //outline
    public static GameObject Outline;
    public static List<GameObject> SelectedObj=new List<GameObject>();
    public Dictionary<GameObject,GameObject> outline_dict=new Dictionary<GameObject,GameObject>();

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        this.WorkspaceCtr=GameObject.Find("Controller").GetComponent<WorkspaceCtr>();
        this.MainCamera=GameObject.Find("Main Camera").GetComponent<Camera>();
        //varsで設定してしまったほうがいい（後で）
        CameraCtr.cam_center=new Vector3(0, 0, 0);
        CameraCtr.cam_eA=new Vector3(30,-45,0);
        CameraCtr.cam_len=10f;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(EventSystem.current.IsPointerOverGameObject(this.gameObject));

        if(this.WorkspaceCtr.mode==1){
            if(this.WorkspaceCtr.tool_name=="Drill"){
                if(CameraCtr.is_pointer_on_camera_window && !this.WorkspaceCtr.is_drilling){  //後者がないとdrilling中にDrillSearchが行われてバグる
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    this.is_raycast_hit=Physics.Raycast(ray, out hit, 40.0f, LayerMask.GetMask("Default"));
                    this.WorkspaceCtr.DrillSearch(this.is_raycast_hit, ray, hit);
                }
            }else if(this.WorkspaceCtr.tool_name=="Welding"){
                if(CameraCtr.is_pointer_on_camera_window && !this.WorkspaceCtr.is_welding){
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    this.is_raycast_hit=Physics.Raycast(ray, out hit, 40.0f, LayerMask.GetMask("Default"));
                    this.WorkspaceCtr.WeldingSearch(this.is_raycast_hit, ray, hit);
                }
            }
        }
    }

    void OnGUI()
    {

    }

    public void OnPointerClick(PointerEventData data)
    {

    }

    public void OnPointerDown(PointerEventData data)
    {
        if(this.WorkspaceCtr.mode==0){
            Ray ray = this.MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 40.0f, LayerMask.GetMask("Default"))){
                this.is_raycast_hit=true;
                this.is_obj_grabbed=true;
                move_point=hit.point;
                hit_to_camera=(Camera.main.transform.position-hit.point).magnitude;

                if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
                    if(hit.collider.gameObject==this.WorkspaceCtr.CameraCenter || this.WorkspaceCtr.SelectedObjs.Contains(this.WorkspaceCtr.CameraCenter)){
                        this.WorkspaceCtr.SelectObj(new List<GameObject>(){hit.collider.gameObject});
                    }else{
                        List<GameObject> new_SelectedObjs=new List<GameObject>(this.WorkspaceCtr.SelectedObjs);
                        if(!new_SelectedObjs.Contains(hit.collider.gameObject)) new_SelectedObjs.Add(hit.collider.gameObject);
                        this.WorkspaceCtr.SelectObj(new_SelectedObjs);
                    }
                }else{
                    this.WorkspaceCtr.SelectObj(new List<GameObject>(){hit.collider.gameObject});
                }
                /*if(CameraCtr.SelectedObj==null){  //ここでnullを弾かないと次の分岐でエラー起こる
                    GameObject g=this.hit.collider.gameObject;
                    //while(g.tag!="elemental"){
                    //    g=g.transform.parent.gameObject;
                    //}
                    this.WorkspaceCtr.SelectObj(g);
                    this.is_obj_grabbed=true;
                }/else if(this.hit.collider.gameObject==CameraCtr.SelectedObj){
                    this.is_obj_grabbed=true;
                }else if(this.hit.collider.transform.IsChildOf(CameraCtr.SelectedObj.transform)){   //元々選んでいるobjをつかんだ場合  //おそらくHitObjの子obj全てを調べる必要がある
                    this.is_obj_grabbed=true;
                }else{
                    this.WorkspaceCtr.SelectObj(this.hit.collider.gameObject);
                }*/
            }else{
                this.WorkspaceCtr.SelectOff();
                this.is_raycast_hit=false;
            }
        }else if(this.WorkspaceCtr.mode==1){  //Tool Mode
            if(this.WorkspaceCtr.tool_name=="Drill"){
                if(CameraCtr.is_pointer_on_camera_window){
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if(Physics.Raycast(ray, out hit, 40.0f, LayerMask.GetMask("Default"))){
                        this.is_raycast_hit=true;
                        Debug.Log("drill start");
                        this.WorkspaceCtr.DrillStart();
                    }else{
                        this.is_raycast_hit=false;
                    }
                    
                }
            }else if(this.WorkspaceCtr.tool_name=="Welding"){
                if(CameraCtr.is_pointer_on_camera_window){
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if(Physics.Raycast(ray, out hit, 40.0f, LayerMask.GetMask("Default"))){
                        this.is_raycast_hit=true;
                        this.WorkspaceCtr.WeldingStart();
                    }else{
                        this.is_raycast_hit=false;
                    }
                }
            }
        }
    }

    public void OnScroll(PointerEventData data)
    {
        Vector3 cam_dir = CameraCtr.cam_center - Camera.main.transform.position;

        Vector3 vec=Camera.main.transform.position+this.coe_scroll * data.scrollDelta.y * cam_dir;
        Camera.main.transform.position=vec;
        this.OutlineCamera.transform.position=vec;
        this.OutlinedObjCamera.transform.position=vec;
        //this.SelectedObjCamera.transform.position=vec;

        CameraCtr.cam_len=(vec-CameraCtr.cam_center).magnitude;
        this.Axes.transform.localScale=new Vector3(1,1,1)*CameraCtr.cam_len/10;
    }

    public void OnPointerUp(PointerEventData data)
    {
        if(this.is_dragged==false){  
            if(this.is_raycast_hit==false){
                this.WorkspaceCtr.SelectOff();  //when click on the air
            }
        }else{
            if(this.WorkspaceCtr.SelectedObjs.Count!=0){
                if(this.WorkspaceCtr.SelectedObjs[0].tag=="CameraCenter"){
                    CameraCtr.cam_center=this.WorkspaceCtr.SelectedObjs[0].transform.position;
                    Vector3 posi_vec=this.GetCameraPosition(CameraCtr.cam_eA, CameraCtr.cam_center, CameraCtr.cam_len);
                    Camera.main.transform.position = posi_vec;
                    this.OutlineCamera.transform.position = posi_vec;
                }else{
                    this.WorkspaceCtr.MoveEnd();
                    //CameraCtr.OutlineOff();
                    //CameraCtr.OutlineObj(CameraCtr.SelectedObj);
                    //this.WorkspaceCtr.SelectObj(CameraCtr.SelectedObj);  //カメラを動かした時のPointerUpでこれがないとだめ（？）
                }
            }
        }
        this.is_obj_grabbed=false;
        this.is_dragged=false;
        this.is_raycast_hit=false;

        if(this.WorkspaceCtr.mode==1){
            if(this.WorkspaceCtr.tool_name=="Drill"){
                if(this.WorkspaceCtr.is_drilling) this.WorkspaceCtr.DrillEnd();
            }
        }
    }

    public void OnBeginDrag(PointerEventData data)
    {
        
    }

    public void OnDrag(PointerEventData data)
    {  //カメラを動かすときのdrag
        this.is_dragged=true;
        
        if (this.is_obj_grabbed == false)
        {
            if(!this.is_raycast_hit){
                CameraCtr.cam_eA.x -= this.coe_angle * data.delta.y;
                CameraCtr.cam_eA.y += this.coe_angle * data.delta.x;

                Vector3 eA_vec=new Vector3(CameraCtr.cam_eA.x, CameraCtr.cam_eA.y, 0);
                Camera.main.transform.eulerAngles=CameraCtr.cam_eA;  //isn't this eA_vec?
                this.OutlineCamera.transform.eulerAngles=CameraCtr.cam_eA;
                this.OutlinedObjCamera.transform.eulerAngles=CameraCtr.cam_eA;

                Vector3 posi_vec=this.GetCameraPosition(CameraCtr.cam_eA,CameraCtr.cam_center,CameraCtr.cam_len);
                Camera.main.transform.position = posi_vec;
                this.OutlineCamera.transform.position=posi_vec;
                this.OutlinedObjCamera.transform.position=posi_vec;
            }
        }else{

            this.WorkspaceCtr.MoveObj(this.WorkspaceCtr.SelectedObjs, out move_point, move_point, hit_to_camera);
/*
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 40.0f, LayerMask.GetMask("Default"))){
                CameraCtr.SelectedObj.transform.position=hit.point+vars.mold_height_dict[CameraCtr.SelectedObj.GetComponent<MoldInfo>().mold_name]*hit.normal;
                CameraCtr.SelectedObj.transform.rotation=Quaternion.FromToRotation(vars.mold_up_dir_to_surface_dict[CameraCtr.SelectedObj.GetComponent<MoldInfo>().mold_name],hit.normal);
                if(CameraCtr.Outline!=null){
                    CameraCtr.OutlineOff();
                    CameraCtr.OutlineObj(CameraCtr.SelectedObj);//こうしないとするとOutlinePfbにスクリプトをつけて（Moldを追加するときに毎回やるのはだるい）,Objと結びつけてCamera.Outlineの子オブジェクトそれぞれに対してpositionをelemental objと同じにすればいい
                }
            }else{
                Vector3 free_move_vec=CameraCtr.coe_move*(data.delta.x*Camera.main.transform.right+data.delta.y*Camera.main.transform.up);
                CameraCtr.SelectedObj.transform.position+=free_move_vec;
                if(CameraCtr.Outline!=null) CameraCtr.Outline.transform.position+=free_move_vec;   //ここの分岐はCameraCenterを動かす時に必要  //わざわざ+=にしているのはそれぞれのoutlineの中心が異なるから平行移動する方が楽だから（そうでなければ全てのelemental objのpositionを割り当てればいい)

                /*
                if(this.WorkspaceCtr.restraint_mode==0){
                    CameraCtr.SelectedObj.transform.position+=free_move_vec;
                    if(CameraCtr.Outline!=null) CameraCtr.Outline.transform.position+=free_move_vec;   //ここの分岐はCameraCenterを動かす時に必要
                }else if(this.WorkspaceCtr.restraint_mode==1){
                    CameraCtr.SelectedObj.transform.position+=new Vector3(0,0,free_move_vec.z);
                    if(CameraCtr.Outline!=null) CameraCtr.Outline.transform.position+=new Vector3(0,0,free_move_vec.z);
                }else if(this.WorkspaceCtr.restraint_mode==2){
                    CameraCtr.SelectedObj.transform.position+=new Vector3(free_move_vec.x,0,0);
                    if(CameraCtr.Outline!=null) CameraCtr.Outline.transform.position+=new Vector3(free_move_vec.x,0,0);
                }else if(this.WorkspaceCtr.restraint_mode==3){
                    CameraCtr.SelectedObj.transform.position+=new Vector3(0,free_move_vec.y,0);
                    if(CameraCtr.Outline!=null) CameraCtr.Outline.transform.position+=new Vector3(0,free_move_vec.y,0);
                }else if(this.WorkspaceCtr.restraint_mode==4){
                    CameraCtr.SelectedObj.transform.position+=new Vector3(free_move_vec.x,0,free_move_vec.z);
                    if(CameraCtr.Outline!=null) CameraCtr.Outline.transform.position+=new Vector3(free_move_vec.x,0,free_move_vec.z);
                }else if(this.WorkspaceCtr.restraint_mode==5){
                    CameraCtr.SelectedObj.transform.position+=new Vector3(free_move_vec.x,free_move_vec.y,0);
                    if(CameraCtr.Outline!=null) CameraCtr.Outline.transform.position+=new Vector3(free_move_vec.x,free_move_vec.y,0);
                }else if(this.WorkspaceCtr.restraint_mode==6){
                    CameraCtr.SelectedObj.transform.position+=new Vector3(0,free_move_vec.y,free_move_vec.z);
                    if(CameraCtr.Outline!=null) CameraCtr.Outline.transform.position+=new Vector3(0,free_move_vec.y,free_move_vec.z);
                }
*/
        }

        if(this.WorkspaceCtr.mode==1){
            if(this.WorkspaceCtr.tool_name=="Drill"){
                if(this.WorkspaceCtr.is_drilling){
                    this.WorkspaceCtr.Drilling();
                }
            }
        }

        //this.is_raycast_hit=false;
        
    }
    

    public void OnEndDrag(PointerEventData data)
    {  //パネルの上でドラッグが始まればパネルの外でドラッグが終わっても呼び出される
    /*
        if(this.WorkspaceCtr.mode==0){  //これの意味がわからない
            CameraCtr.OutlineOff();
            CameraCtr.OutlineObj(CameraCtr.SelectedObj);
        }
    */
    }

    public void OnPointerEnter(PointerEventData data){
        CameraCtr.is_pointer_on_camera_window=true;
    }

    public void OnPointerExit(PointerEventData data){
        CameraCtr.is_pointer_on_camera_window=false;
    }

    public static void OutlineObj(List<GameObject> g_list, int obj_layer=7){
        CameraCtr.Outline=new GameObject();
        CameraCtr.SelectedObj=g_list;
        
        bool end=false;
        int node=-1;
        int index=0;
        List<int> num_node=new List<int>();
        List<int> index_node=new List<int>();
        
        for(int i=0;i<g_list.Count;i++){
            GameObject g=g_list[i];

            GameObject set1=Instantiate(vars.mold_outline_pfb_dict[g.GetComponent<MoldInfo>().mold_name], CameraCtr.Outline.transform);
            set1.transform.position=g.transform.position;
            set1.transform.eulerAngles=g.transform.eulerAngles;
            //set1.transform.localScale=new Vector3(set1.transform.localScale.x*g.transform.localScale.x, set1.transform.localScale.y*g.transform.localScale.y, set1.transform.localScale.z*g.transform.localScale.z);
            set1.layer=6;
            g.layer=obj_layer;
        }

        /*
        if(g.tag!="Item"){  //should be g.transform.childCount!=0, but then, I need to prepare outline pfb for all objects included fbx file. outline should be done by shader.
            while(true){
                while(g.tag!="Item"){
                    num_node.Add(g.transform.childCount);
                    index_node.Add(index);
                    node+=1;
                    g=g.transform.GetChild(index).gameObject;

                    //ここに処理
                    GameObject set=Instantiate(vars.mold_outline_pfb_dict[g.GetComponent<MoldInfo>().mold_name],CameraCtr.Outline.transform);
                    set.transform.position=g.transform.position;
                    set.transform.eulerAngles=g.transform.eulerAngles;
                    //set.transform.localScale=new Vector3(set.transform.localScale.x*g.transform.localScale.x, set.transform.localScale.y*g.transform.localScale.y, set.transform.localScale.z*g.transform.localScale.z);
                    set.layer=6;
                    g.layer=obj_layer;

                    index=0;
                }

                g=g.transform.parent.gameObject;
                index_node[node]+=1;

                //ここでnodeはぶち当たった末端のnode
                while(num_node[node]==index_node[node]){
                    num_node.RemoveAt(num_node.Count-1);
                    index_node.RemoveAt(index_node.Count-1);
                    node-=1;
                    if(node==-1){
                        end=true;
                        break;
                    }
                    index_node[node]+=1;
                    g=g.transform.parent.gameObject;
                }
                if(end) break;
                index=index_node[node];
                node-=1;
                num_node.RemoveAt(num_node.Count-1);
                index_node.RemoveAt(index_node.Count-1);
            }
        }
        */
    }

    public static void OutlineOff(){
        if(CameraCtr.Outline!=null){
            Destroy(CameraCtr.Outline);
        }

        for(int i=0;i<CameraCtr.SelectedObj.Count;i++){
            if(CameraCtr.SelectedObj[i]!=null) CameraCtr.SelectedObj[i].layer=0;
        }
    }

    public Vector3 GetCameraPosition(Vector3 eA, Vector3 center, float cam_len){
        double sitax=eA.x*Math.PI/180;
        double sitay=eA.y*Math.PI/180;

        float cosx = (float)Math.Cos(sitax);
        float cosy = (float)Math.Cos(sitay);
        float sinx = (float)Math.Sin(sitax);
        float siny = (float)Math.Sin(sitay);
        return center + cam_len * new Vector3(-cosx * siny, sinx, -cosx * cosy);
    }


    //refer game note 2023 p4
    public static Vector3 GetCrossPointBetweenPlaneAndLine(Vector3 normal_plane, Vector3 point_plane, Vector3 dir_line, Vector3 point_line){
        float dot = Vector3.Dot(dir_line, normal_plane);

        if(dot == 0){
            Debug.Log("no cross point error");
            return Vector3.zero;
        }

        float h = Vector3.Dot(point_line - point_plane, normal_plane);
        //h: length between point and plane with sign aligned normal_plane (+ when point_line is in the space where normal_plane is directed)

        return - h * dir_line / dot + point_line;
    }

}
