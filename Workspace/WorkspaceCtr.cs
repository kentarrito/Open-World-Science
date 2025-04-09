using System;
//using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using Parabox.CSG;


public class WorkspaceCtr : MonoBehaviour
{
    #region variables
    public int mode=0;  //0:normal  1:using tool

    public bool test1=true;

    #region ObjList
    public GameObject objects;
    public Dictionary<string, GameObject> item_dict = new Dictionary<string, GameObject>(); //key: item_specific_name

    public GameObject ObjectBtnPfb;
    public GameObject CameraCenter;
    #endregion

    #region general
    int update_count=0;
    bool is_update_count=false;
    public List<GameObject> SelectedObjs=new List<GameObject>();

    //MoveObj
    int move_obj_count = 0;
    int obj_stop_count = 3;
    bool stop_moving_obj;
    Vector3 InputMousePosition;


    float hole_threshold=0.8f;
    public bool is_moving_obj_in_hole_start;
    bool is_moving_obj_in_hole;
    public GameObject ObjInHole;
    public GameObject HoleObj;

    Vector3 pre_cursor_point_on_hole_normal;
    public Vector3 hole_posi;
    public Vector3 hole_normal;

    #endregion

    #region Mold
    public int num_trigger_obj=0;
    bool is_mold_opening = false;
    bool is_mold_closing = false;
    public GameObject MoldPanel;
    public GameObject OpenPanel;
    public GameObject ContentMoldScrollView;
    public GameObject OpenMoldBtn;
    public GameObject CloseMoldBtn;

    public bool is_mold_selected=false;
    public string selected_mold_name;
    //public GameObject SelectedMoldPfb;
    public GameObject MoldScrollViewBtnPfb;
    #endregion

    #region Restraint
    public int restraint_mode=0;
    public GameObject RestraintPanel;
    public GameObject RestraintAxes;
    public GameObject Rx;
    public GameObject Ry;
    public GameObject Rz;
    public GameObject RfreeBtn;
    public GameObject RxBtn;
    public GameObject RyBtn;
    public GameObject RzBtn;
    public GameObject RxyBtn;
    public GameObject RyzBtn;
    public GameObject RzxBtn;
    #endregion

    #region  ObjList
    bool is_objlist_opening = false;
    bool is_objlist_closing = false;
    public GameObject InfoPanel;
    public GameObject ObjListPanel;
    public GameObject DetailScrollView;
    public GameObject OpenObjListBtn;
    public GameObject CloseObjListBtn;
    public List<TMP_InputField> TransformsInDetailInf = new List<TMP_InputField>();
    public TMP_Text DetailItemName;
    #endregion

    #region Simulate
    public GameObject SimulateObj;
    bool is_simulate_update = false;
    int simulate_count = 0;
    List<List<float>> sim_obj_posis = new List<List<float>>();
    #endregion

    #region Tool
    public GameObject ToolPanel;
    public string tool_name;

    [SerializeField] GameObject SphereCollider;
    [SerializeField] GameObject CircleArea;
    Vector3 tool_search_hit_point;
    Vector3 tool_search_hit_normal;
    Vector3 tool_search_ray_dir;
    Vector3 start_cursor_position;

    //Drill
    public GameObject DrillArea;  //showing the area to be digged out
    public GameObject DrillCylinderTop;
    public GameObject DrillCylinder;  //a cylinder to dig a hole
    public GameObject DrillObj;  //some cool drill object
    GameObject UndrilledObj;
    GameObject DrilledObj;
    public float drill_radius;
    public float drill_depth;

    public bool is_drilling=false;
    
    RaycastHit drill_search_hit;
    Ray drill_search_ray;

    Vector3 drill_point;
    Vector3 drill_point_normal;
    Vector3 drill_start_cursor_position;
    float drill_speed_coe=0.1f;
    float drill_speed_space=0.003f;  //depth by which drill dig a hole in an update when space is pushed
    
    //Welding machine
    [SerializeField] GameObject WeldMObj;
    public bool is_welding=false;

    [SerializeField] GameObject WeldingSearchSphere;

    //Others
    List<string> MoldNamesInsertableToHole = new List<string>(){"Pipe","Pillar"};
    

    
    #endregion

    #region File Panel
    public InputField file_name_inf_inf;
    public GameObject file_name;
    public GameObject decide_file_name_btn;
    public GameObject alter_file_name_btn;
    public GameObject file_name_inf;

    public GameObject content_all_files;
    public GameObject all_files_panel;
    public GameObject file_btn_pfb;
    string selected_f_name;
    List<string> selected_folder_path;
    List<GameObject> file_btn_objs=new List<GameObject>();

    public GameObject create_new_file_panel;
    public InputField new_file_name_inf_inf;
    public GameObject create_new_folder_panel;
    public InputField new_folder_name_inf_inf;
    public GameObject check_remove_panel;
    public Text check_remove_text;
    #endregion

    #endregion


    void Start(){
        this.SetObjListPanel(this.objects, new List<GameObject>());
        this.SetMoldPanel();
        this.SetTool();
    }

    void Update(){
        if (this.is_update_count){
            this.update_count += 1;
        }

        if (this.is_simulate_update){
            UpdateSimulate();
        }

        //ここは後でボタンにつけたスクリプトに写した方がいい,アニメーション系は基本複雑になるからWorkspaceCtrには書かないこと
        #region UI operator
        
        if (this.is_mold_opening){
            if (this.MoldPanel.GetComponent<RectTransform>().sizeDelta.y>=500){
                this.MoldPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 500);
                this.OpenPanel.transform.localScale=new Vector3(1,1,1);
                this.is_mold_opening = false;
            }else{
                this.MoldPanel.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 84);
                this.OpenPanel.transform.localScale+=new Vector3(0,0.2f,0);
            }
        }

        if (this.is_mold_closing){
            if (this.MoldPanel.GetComponent<RectTransform>().sizeDelta.y<=80){
                this.MoldPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 80);
                this.OpenPanel.transform.localScale=new Vector3(1,0,1);
                this.is_mold_closing = false;
            }else{
                this.MoldPanel.GetComponent<RectTransform>().sizeDelta -= new Vector2(0, 84);
                this.OpenPanel.transform.localScale-=new Vector3(0,0.2f,0);
            }
        }

        if (this.is_objlist_opening)
        {
            if (this.InfoPanel.GetComponent<RectTransform>().sizeDelta.x >= 400)
            {
                this.InfoPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 0);
                this.ObjListPanel.transform.localScale=new Vector3(1,1,1);
                this.is_objlist_opening = false;
            }
            else{
                this.InfoPanel.GetComponent<RectTransform>().sizeDelta += new Vector2(95, 0);
                this.ObjListPanel.transform.localScale+=new Vector3(0.2f,0,0);
            }
        }

        if (this.is_objlist_closing)
        {
            if (this.InfoPanel.GetComponent<RectTransform>().sizeDelta.x <= 20)
            {
                this.InfoPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 0);
                this.ObjListPanel.transform.localScale=new Vector3(0,1,1);
                this.is_objlist_closing = false;
            }
            else{
                this.InfoPanel.GetComponent<RectTransform>().sizeDelta -= new Vector2(95, 0);
                this.ObjListPanel.transform.localScale-=new Vector3(0.2f,0,0);
            }

        }
        #endregion

        #region Drill
        if(this.DrillArea.activeInHierarchy || this.is_drilling){
            if(Input.GetKeyDown("space")){
                this.DrillStart();
            }

            if(Input.GetKeyUp("space")){
                DrillEnd();
            }
            
            if(Input.GetKey("space") && this.is_drilling){
                this.drill_depth += this.drill_speed_space;

                this.SetDrillScale(this.drill_radius, this.drill_depth);
                this.SetDrillOnObjTop(this.drill_point, this.drill_point_normal);

                (GameObject result, bool is_drill_outside_obj, bool is_drill_inside_obj) = this.Subtract(this.UndrilledObj, this.DrillCylinder, -this.drill_point_normal);
                
                Destroy(this.DrilledObj);
                this.DrilledObj=result;
                this.DrilledObj.SetActive(true);

                //this.DrillObj.transform.rotation=Quaternion.FromToRotation(new Vector3(0,1,0), this.drill_point_normal);
                this.DrillObj.transform.position=this.drill_point-this.drill_point_normal*this.drill_depth;

                this.DrillObj.GetComponent<DrillCtr>().DrillProgress(true);
                
                if(is_drill_outside_obj && !is_drill_inside_obj){
                    DrillEnd();
                }
            }

            
        }
        #endregion
    }

    #region General
/*
    //CameraCenterは同時に選択できないようにする
    //Detailのパネルを動かすこととoutline関数がstaticであったほうが便利なことからObjの選択はこのスクリプトに統一
    public void SelectObj(GameObject obj){
        //this.SelectOff();  //SetObjListPanelを二回くりかえしているのが少しもったいない

        List<GameObject> g_list=new List<GameObject>();
        g_list.Add(obj);
        CameraCtr.SelectedObj=g_list;

        SetRB(obj.GetComponent<Rigidbody>(), true);

        if(obj==this.CameraCenter){
            this.CameraCenter.SetActive(true);
        }else{
            //CameraCtr.OutlineObj(new List<GameObject>(){obj});
            OutlineObj(new List<GameObject>(){obj});
        }

        this.SetObjListPanel(this.objects, CameraCtr.SelectedObj);
        //this.SetRestraint(0);
        this.SetDetailScrollView(obj);
    }
*/

    public void SelectObj(List<GameObject> objs){
        if(objs.Contains(this.CameraCenter)){
            this.CameraCenter.SetActive(true);
            OutlineOff(SelectedObjs);

        }else{
            foreach(var obj in objs){
                if(SelectedObjs.Contains(obj)){
                    SelectedObjs.Remove(obj);
                }
            }

            //Rigidbodyの設定はOutlineの中に入れてしまうことにする
            OutlineOff(SelectedObjs);
            OutlineObj(objs);   //ここがSelectedObjsと被っていると、OutlineコンポーネントがDestroyしきれずエラーが出る

            this.CameraCenter.SetActive(false);
        }

        SelectedObjs=objs;

        this.SetObjListPanel(this.objects, SelectedObjs);
        this.SetDetailScrollView(objs[0]);
        
    }

/*
    //add obj to the SelectedObj(list)
    public void MultiSelectObj(GameObject obj){
        if(obj!=this.CameraCenter){
            OutlineOff();
            CameraCtr.SelectedObj.Add(obj);
            //CameraCtr.OutlineObj(CameraCtr.SelectedObj);
            OutlineObj(CameraCtr.SelectedObj);

            SetRB(obj.GetComponent<Rigidbody>(), true);
        }else{
            List<GameObject> g_list=new List<GameObject>();
            g_list.Add(obj);
            CameraCtr.SelectedObj=g_list;
            this.CameraCenter.SetActive(true);
        }

        this.SetObjListPanel(this.objects, CameraCtr.SelectedObj);
        //this.SetDetailScrollView(obj);
    }
*/

    public void SelectOff(){   //注:DestroyでOutlineコンポーネントを消しているのでSelectOffとSelectObjを同時に使うと、元々SelectされてたObjと新しくSelectするObjが重なってた場合、バグる.　全部消したい時だけに使うこと
        if(SelectedObjs.Contains(this.CameraCenter)) this.CameraCenter.SetActive(false);
        else OutlineOff(SelectedObjs);

        SelectedObjs = new List<GameObject>();

        this.SetObjListPanel(this.objects, new List<GameObject>());
        //this.RestraintOff();
        //if(this.DetailScrollView!=null) this.DetailScrollView.SetActive(false);
    }

    void OutlineObj(List<GameObject> objs){  //この中にSetRBが含まれていることに注

        foreach(var obj in objs){
            if(obj.GetComponent<Outline>()==null){
                var outline = obj.AddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.OutlineColor = Color.green;
                outline.OutlineWidth = 3f;
            }

            if(obj.GetComponent<Rigidbody>()!=null){
                SetRB(obj.GetComponent<Rigidbody>(), true);
            }
        }
    }

    void OutlineOff(List<GameObject> objs){

        foreach(var obj in objs){
            Destroy(obj.GetComponent<Outline>());

            if(obj.GetComponent<Rigidbody>()!=null) SetRB(obj.GetComponent<Rigidbody>(), false);

        }

    }

    void SetRB(Rigidbody rb, bool is_seleted){  //この説明はnote 2023/9/30
        if(is_seleted){
            rb.drag=Mathf.Infinity;
            rb.angularDrag=Mathf.Infinity;
            rb.constraints=RigidbodyConstraints.None;
        }else{
            rb.drag=1;
            rb.angularDrag=1;
            rb.constraints=RigidbodyConstraints.FreezeAll;
        }
    }

    public void MoveObj(List<GameObject> objs, out Vector3 new_point, Vector3 pre_point, float hit_to_camera, bool stop_obj = false){  //outの変数はassignされてないから参照はできない  //hit_to_camera: the length form camera to hit_point
        if(stop_moving_obj){  //when you wanna stop obj for a while. Ex. when UniqueJoint occurs, you would wanna stop moveing obj a bit.
            if(move_obj_count <= obj_stop_count){
                new_point = pre_point;
                move_obj_count += 1;
                return;
            }else{
                move_obj_count = 0;
                stop_moving_obj = false;
            }
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        /*
        if (Physics.Raycast(ray, out hit, 40.0f, LayerMask.GetMask("Default"))){  //Put obj on the obj where raycast is poked
            obj.transform.position=hit.point+vars.mold_height_dict[obj.GetComponent<MoldInfo>().mold_name]*hit.normal;
            obj.transform.rotation=Quaternion.FromToRotation(vars.mold_up_dir_to_surface_dict[obj.GetComponent<MoldInfo>().mold_name],hit.normal);
            //ここは本当はOutlinePfbにスクリプトをつけてOutlinedObjに紐づければposition合わせるだけだから簡単。他にOutlinePfbにスクリプトが必要になったらそうする
            if(obj!=this.CameraCenter){
                CameraCtr.OutlineOff();
                CameraCtr.OutlineObj(obj,8);//動かしている間はraycastが当たらないようにしないとcameraにぐーんと近づいてくる,EndDragの際に必ずlayerを7に戻すこと（そうしないともう一度クリックできない） //こうしないとするとOutlinePfbにスクリプトをつけて（Moldを追加するときに毎回やるのはだるい）,Objと結びつけてCamera.Outlineの子オブジェクトそれぞれに対してpositionをelemental objと同じにすればいい
            }
        }else{
            obj.transform.position=Camera.main.transform.position+CameraCtr.cam_len*ray.direction;
            if(obj!=this.CameraCenter){
                CameraCtr.OutlineOff();
                CameraCtr.OutlineObj(obj,8);//こうしないとするとOutlinePfbにスクリプトをつけて（Moldを追加するときに毎回やるのはだるい）,Objと結びつけてCamera.Outlineの子オブジェクトそれぞれに対してpositionをelemental objと同じにすればいい
            }
        }
        */

        /*
        obj.transform.position=Camera.main.transform.position+CameraCtr.cam_len*ray.direction;
        if(obj!=this.CameraCenter){
            CameraCtr.OutlineOff();
            CameraCtr.OutlineObj(obj,8);//こうしないとするとOutlinePfbにスクリプトをつけて（Moldを追加するときに毎回やるのはだるい）,Objと結びつけてCamera.Outlineの子オブジェクトそれぞれに対してpositionをelemental objと同じにすればいい
        }
        */

        /*
        if(Physics.Raycast(ray, out hit, 40.0f, LayerMask.GetMask("Default"))){
            if(hit.collider.gameObject.tag=="HoleSphereCollider"){
                is_moving_obj_in_hole_start=true;
            }
        }
        */

        Vector3 move_vec;

        if(is_moving_obj_in_hole_start || is_moving_obj_in_hole){
            if(is_moving_obj_in_hole_start){
                is_moving_obj_in_hole = true;
                is_moving_obj_in_hole_start = false;
                pre_cursor_point_on_hole_normal = CameraCtr.GetCrossPointBetweenPlaneAndLine(Vector3.Cross(this.hole_normal, Camera.main.transform.up).normalized, this.hole_posi, ray.direction, Camera.main.transform.position);

                Vector3 aim_posi = Vector3.Dot(this.ObjInHole.transform.position - this.hole_posi, this.hole_normal) * this.hole_normal + this.hole_posi;

                Quaternion rot_aim = Quaternion.FromToRotation(new Vector3(0,1,0), this.hole_normal);
                
                RotateObjs(this.SelectedObjs, rot_aim, this.ObjInHole);

                Physics.IgnoreCollision(this.ObjInHole.GetComponent<Collider>(), this.HoleObj.GetComponent<Collider>());

                move_vec = aim_posi - this.ObjInHole.transform.position;

            }else{
                
                Vector2 move_vec_on_screen = new Vector2(Input.mousePosition.x - InputMousePosition.x, Input.mousePosition.y - InputMousePosition.y);
                Vector2 hole_normal_dir_on_screen = new Vector2(Vector3.Dot(this.hole_normal, Camera.main.transform.right), Vector3.Dot(this.hole_normal, Camera.main.transform.up)).normalized;
                float dot = Vector3.Dot(move_vec_on_screen.normalized, hole_normal_dir_on_screen);
                //Debug.Log(this.hole_normal+" "+Camera.main.transform.right+" "+Camera.main.transform.up+" "+InputMousePosition);

                if(dot > this.hole_threshold || dot < -this.hole_threshold){  //slide along the hole
                    move_vec = dot * (pre_cursor_point_on_hole_normal - CameraCtr.GetCrossPointBetweenPlaneAndLine(Vector3.Cross(this.hole_normal, Camera.main.transform.up).normalized, this.hole_posi, ray.direction, Camera.main.transform.position)).magnitude * this.hole_normal;

                    pre_cursor_point_on_hole_normal = CameraCtr.GetCrossPointBetweenPlaneAndLine(Vector3.Cross(this.hole_normal, Camera.main.transform.up).normalized, this.hole_posi, ray.direction, Camera.main.transform.position);

                }else{
                    //objをもとに戻すスクリプト
                    //Physics.IgnoreCollision(this.ObjInHole.GetComponent<Collider>(), this.HoleObj.GetComponent<Collider>(), false);
                    move_vec=Vector3.zero;
                }
            }
        }else{
            move_vec = Camera.main.transform.position + hit_to_camera * ray.direction - pre_point;
        }

        new_point = pre_point + move_vec;

        foreach(var obj in objs){
            obj.transform.position += move_vec;
        }

        this.InputMousePosition = Input.mousePosition;

    }

    //dir_lea[0] and dir_lea[1] are the directions child_obj's y axis and x axis should be oriented to. 
    public void TriggerUniqueJointCollider(GameObject parent_obj, GameObject child_obj, Vector3 center_posi, Vector3[] dir_ea){
        Vector3 move_vec;

        if(SelectedObjs.Contains(parent_obj)){
            //when parent should be oriented to one direction, we gotta take inverse to get q_aim since dir_ea is the direction child_obj should be directed to.
            Quaternion q_aim = Quaternion.Inverse(Quaternion.FromToRotation(Vector3.up, dir_ea[0]));  //これが本当に動くかわからない
            RotateObjs(this.SelectedObjs, q_aim, parent_obj);

            q_aim = Quaternion.Inverse(Quaternion.FromToRotation(Vector3.right, dir_ea[1]));
            RotateObjs(this.SelectedObjs, q_aim, parent_obj);

            move_vec = - (center_posi - child_obj.transform.position);

        }else if(SelectedObjs.Contains(child_obj)){

            Quaternion q_aim = Quaternion.FromToRotation(Vector3.up, dir_ea[0]);
            RotateObjs(this.SelectedObjs, q_aim, child_obj);

            q_aim = Quaternion.FromToRotation(Vector3.right, dir_ea[1]);
            RotateObjs(this.SelectedObjs, q_aim, child_obj);

            move_vec = center_posi - child_obj.transform.position;

        }else{
            Debug.Log("error");
            move_vec = Vector3.zero;
        }

        foreach(var obj in SelectedObjs){
            obj.transform.position += move_vec;
        }

        this.stop_moving_obj = true;
    }

    //Rotate objs so that aim_obj inside it rotates to q_aim. q_aim can be gotten by Quaternion.FromToRotation(local_dir_of_obj, dir_local_dir_to_be_oriented_to)
    void RotateObjs(List<GameObject> objs, Quaternion q_aim, GameObject aim_obj){
        Transform[] original_parent = new Transform[objs.Count];

        for(int i=0; i<objs.Count; i++){
            if(objs[i] != aim_obj){
                original_parent[i] = objs[i].transform.parent;
                objs[i].transform.parent = aim_obj.transform;
            }
        }

        aim_obj.transform.rotation = Quaternion.RotateTowards(aim_obj.transform.rotation, q_aim, 180);

        for(int i=0; i<objs.Count; i++){
            if(objs[i] != aim_obj){
                objs[i].transform.parent = original_parent[i];
            }
        }
    }

    public void MoveEnd(){
        this.is_moving_obj_in_hole = false;
        this.is_moving_obj_in_hole_start = false;
    }

    #endregion

    #region UI operator

    public void OpenMold()
    {
        this.is_mold_opening = true;
        this.OpenMoldBtn.SetActive(false);
        this.CloseMoldBtn.SetActive(true);
    }

    public void CloseMold()
    {
        this.is_mold_closing = true;
        this.OpenMoldBtn.SetActive(true);
        this.CloseMoldBtn.SetActive(false);
    }

    public void OpenObjList()
    {
        this.is_objlist_opening = true;
        this.OpenObjListBtn.SetActive(false);
        this.CloseObjListBtn.SetActive(true);
    }

    public void CloseObjList()
    {
        this.is_objlist_closing = true;
        this.OpenObjListBtn.SetActive(true);
        this.CloseObjListBtn.SetActive(false);
    }

    public void Exit(){
        SceneManager.LoadScene(2);
    }

    #endregion

    #region Tool

    public void SetTool(){
        if(mode==0){
            SetDrill(false);
            SetWeldM(false);
        }
    }

    public void ClickToolBtn(){
        if(this.ToolPanel.activeInHierarchy){
            this.ToolPanel.SetActive(false);
        }else{
            this.ToolPanel.SetActive(true);
        }
    }

    public void ClickWeldingBtn(){
        this.mode=1;
        this.tool_name="Welding";
        this.ToolPanel.SetActive(false);
        SelectOff();
        SetWeldM(true);
    }

    public void ClickDrillBtn(){
        this.mode=1;
        this.tool_name="Drill";
        this.ToolPanel.SetActive(false);
        SelectOff();
        SetDrill(true);
    }

    public void ClickNoneBtn(){
        this.mode=0;
        this.tool_name=null;
        this.ToolPanel.SetActive(false);
        CameraCtr.OutlineOff();
        SetDrill(false);
        SetWeldM(false);
    }

    #region Drill

    public void SetDrill(bool drill_obj){
        /*
        this.DrillCylinder.SetActive(false);
        this.DrillCylinderTop.SetActive(false);

        this.DrillCylinder.GetComponent<Renderer>().enabled=false;
        this.DrillCylinderTop.GetComponent<Renderer>().enabled=false;
        */

        if(drill_obj){
            this.DrillObj.GetComponent<DrillCtr>().HoldDrill();
        }else{
            this.DrillObj.GetComponent<DrillCtr>().HoldOff();
        }
    }

    void SetDrillScale(float radius, float depth){   //unit:[m]  minimam change:0.001[m]  //FirstSet: r=0.1, d=0.001
        this.DrillCylinderTop.transform.localScale=new Vector3(20*radius, 1000*depth, 20*radius);
    }

    void SetDrillOnObjTop(Vector3 position, Vector3 normal){  //set it 0.0001m above the surface
        this.DrillCylinderTop.transform.position=position+0.0001f*normal;
        this.DrillCylinderTop.transform.rotation = Quaternion.FromToRotation(new Vector3(0,1,0), normal);
    }

    //called when mouse is on CameraWindow in drill mode
    public void DrillSearch(bool is_raycast_hit, Ray ray, RaycastHit hit){
        this.drill_search_hit=hit;
        this.drill_search_ray=ray;
        if(is_raycast_hit){
            this.DrillArea.SetActive(true);
            this.DrillArea.transform.position = hit.point;
            this.DrillArea.transform.rotation = Quaternion.FromToRotation(new Vector3(0,1,0), hit.normal);
        }else{
            this.DrillArea.SetActive(false);
        }
    }

    //called when pointer down on CameraWindow in drill mode
    public void DrillStart(){
        this.is_drilling=true;
        this.DrillArea.SetActive(false);

        this.UndrilledObj=drill_search_hit.collider.gameObject;
        this.UndrilledObj.SetActive(false);

        this.drill_depth=0.1f;
        this.drill_radius=0.05f;
        this.SetDrillScale(this.drill_radius, this.drill_depth);
        this.SetDrillOnObjTop(drill_search_hit.point, drill_search_hit.normal);

        this.drill_point=drill_search_hit.point;
        this.drill_point_normal = drill_search_hit.normal;
        this.drill_start_cursor_position = Camera.main.transform.position + CameraCtr.cam_len*this.drill_search_ray.direction;

        //dig a hole
        (GameObject result, bool is_drill_outside_obj, bool is_drill_inside_obj) = this.Subtract(this.UndrilledObj, this.DrillCylinder, - this.drill_point_normal);
        this.DrilledObj = result;
        this.DrilledObj.SetActive(true);
        Debug.Log(is_drill_outside_obj+", "+is_drill_inside_obj);

        this.DrillObj.GetComponent<DrillCtr>().SetDrillToSurface(this.drill_point, this.drill_point_normal);
        this.DrillObj.GetComponent<DrillCtr>().RotateBit(70);

    }

    //called when drag in drill mode
    public void Drilling(){

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Vector3 drill_cursor_position = Camera.main.transform.position + CameraCtr.cam_len*ray.direction;
        float drill_speed = Vector3.Dot(drill_cursor_position - this.drill_start_cursor_position, -this.drill_point_normal);

        if(drill_speed>0){
            this.drill_depth += this.drill_speed_coe*drill_speed;
            this.DrillObj.GetComponent<DrillCtr>().DrillProgress(true);
        }else{
            this.DrillObj.GetComponent<DrillCtr>().DrillProgress(false);
        }

        this.SetDrillScale(this.drill_radius, this.drill_depth);
        this.SetDrillOnObjTop(this.drill_point, this.drill_point_normal);

        (GameObject result, bool is_drill_outside_obj, bool is_drill_inside_obj) = this.Subtract(this.UndrilledObj, this.DrillCylinder, -this.drill_point_normal);
        
        Destroy(this.DrilledObj);
        this.DrilledObj=result;
        this.DrilledObj.SetActive(true);

        //this.DrillObj.transform.rotation=Quaternion.FromToRotation(new Vector3(0,1,0), this.drill_point_normal);
        this.DrillObj.transform.position=this.drill_point-this.drill_point_normal*this.drill_depth;
        
        if(is_drill_outside_obj && !is_drill_inside_obj){
            DrillEnd();
        }
    }

    public void DrillEnd(){

        this.is_drilling=false;
        Destroy(this.UndrilledObj);

        /*
        this.DrilledObj.GetComponent<MoldInfo>().hole_posis.Add(this.drill_point);
        this.DrilledObj.GetComponent<MoldInfo>().hole_normals.Add(this.drill_point_normal);
        this.DrilledObj.GetComponent<MoldInfo>().hole_depthes.Add(this.drill_depth);
        */
        

        //反対側のholeにもcolliderつける必要
        GameObject hole_collider = Instantiate(this.SphereCollider, this.DrilledObj.transform);
        hole_collider.tag="HoleSphereCollider";
        hole_collider.GetComponent<SphereCollider>().radius = this.drill_radius;
        hole_collider.GetComponent<SphereColliderCtr>().WorkspaceCtr = this;
        hole_collider.GetComponent<SphereColliderCtr>().parent_obj = this.DrilledObj;
        hole_collider.GetComponent<SphereColliderCtr>().hole_normal = this.drill_point_normal;
        hole_collider.GetComponent<SphereColliderCtr>().hole_depth = this.drill_depth;
        hole_collider.transform.position=this.drill_point;

        this.DrilledObj.GetComponent<MoldInfo>().hole_collider_list.Add(hole_collider.GetComponent<SphereColliderCtr>());
        
        this.DrillObj.GetComponent<DrillCtr>().SetOff();
        this.DrillObj.GetComponent<DrillCtr>().RotateBit(0);
        this.DrillObj.GetComponent<DrillCtr>().DrillProgress(false);

    }
    
    #endregion

    #region Welding

    public void SetWeldM(bool weldm_hold){

        //this.WeldingSearchSphere.GetComponent<MeshRenderer>().enabled=false;

        if(weldm_hold){
            this.WeldMObj.GetComponent<WeldingMachineCtr>().HoldWeldM();
        }else{
            this.WeldMObj.GetComponent<WeldingMachineCtr>().HoldOff();
        }
    }

    public void WeldingSearch(bool is_raycast_hit, Ray ray, RaycastHit hit){
        tool_search_hit_point=hit.point;
        tool_search_hit_normal=hit.normal;
        tool_search_ray_dir=ray.direction;

        if(is_raycast_hit){
            this.CircleArea.SetActive(true);
            this.CircleArea.transform.position = tool_search_hit_point;
            this.CircleArea.transform.rotation = Quaternion.FromToRotation(new Vector3(0,1,0), tool_search_hit_normal);
            //大きさの情報を後で追加

            this.WeldingSearchSphere.SetActive(true);
            this.WeldingSearchSphere.transform.position = tool_search_hit_point;

        }else{
            this.CircleArea.SetActive(false);
            this.WeldingSearchSphere.SetActive(false);

        }
    }

    public void WeldingStart(){
        this.CircleArea.SetActive(false);
        this.is_welding=true;

        List<GameObject> WeldedObjs=this.WeldingSearchSphere.GetComponent<WeldingSearchSphereCtr>().ObjsOnTrigger;

        if(WeldedObjs.Count==2){
            FixedJoint[] list = WeldedObjs[0].GetComponents<FixedJoint>();
            FixedJoint joint_already_attached=null;

            for(int i=0;i<list.Length;i++){
                if(list[i].connectedBody.gameObject == WeldedObjs[1]){
                    joint_already_attached = list[i];
                    break;
                }
            }

            list = WeldedObjs[1].GetComponents<FixedJoint>();
            for(int i=0;i<list.Length;i++){
                if(list[i].connectedBody.gameObject==WeldedObjs[0]){
                    joint_already_attached=list[i];
                    break;
                }
            }

            if(joint_already_attached!=null){
                joint_already_attached.breakForce+=1;
                joint_already_attached.breakTorque+=1;
            }else{
                WeldedObjs[0].AddComponent<FixedJoint>();
                WeldedObjs[0].GetComponent<FixedJoint>().breakForce=1;
                WeldedObjs[0].GetComponent<FixedJoint>().breakTorque=1;
                WeldedObjs[0].GetComponent<FixedJoint>().connectedBody = WeldedObjs[1].GetComponent<Rigidbody>();
            }
        }

        //this.welding_point=welding_search_hit.point;
        //this.welding_point_normal = welding_search_hit.normal;
        //this.welding_start_cursor_position = Camera.main.transform.position + CameraCtr.cam_len*this.drill_search_ray.direction;

        this.WeldMObj.GetComponent<WeldingMachineCtr>().Welding(tool_search_hit_point, tool_search_hit_normal);
    }

    //本当はWeldingをドラッグできるようにしたいからWelding,WeldingEnd関数が必要
    public void Welding(){
        /*
        if(SelectedObjs.Count==2){
            if(SelectedObjs[0].parent.gameObject == this.objects && SelectedObjs[1].parent.gameObject == this.objects){
                GameObject weld = new GameObject();
                weld.name="welding";
                weld.parent=this.objects.transform;

                SelectedObjs[0].parent=weld.transform;
                SelectedObjs[1].parent=weld.transform;
                
            }else if(SelectedObjs[0].parent.gameObject != this.objects && SelectedObjs[1].parent.gameObject == this.objects){
                SelectedObjs[1].parent=SelectedObjs[0].parent;

            }else if(SelectedObjs[0].parent.gameObject == this.objects && SelectedObjs[1].parent.gameObject != this.objects){
                SelectedObjs[0].parent=SelectedObjs[1].parent;

            }else{
                Transform parent1 = SelectedObjs[1].parent;  //welding

                for(int i=0;i<SelectedObjs[1].parent.childCount;i++){
                    parent1.GetChild(i).parent=SelectedObjs[0].parent;   //注、ここはDestroyしてもそれが反映されるのは後だからGetChild(i)でいい
                }

                Destroy(parent1.gameObject);
            }
        }else{
            Debug.Log("Please select only 2 objects.");
        }
        */
    }

    public void WeldingEnd(){
        this.is_welding=false;
    }

    #endregion

    #endregion

    #region file panel

    void SetActiveByPath(List<string> name_path, FileFolder file){
        bool is_same_path=true;
        if(vars.opening_file_path.Count!=name_path.Count) is_same_path=false;
        for(int i=0;i<name_path.Count;i++){
            if(vars.opening_file_path[i]!=name_path[i]) is_same_path=false;
        }

        if(is_same_path){
            if(file.objs!=null){
                foreach(var key in file.objs.Keys){
                    file.objs[key].SetActive(true);
                    vars.opening_file.objs.Add(key,file.objs[key]);
                }
            }
        }else{
            if(file.objs!=null){
                foreach(var key in file.objs.Keys){
                    file.objs[key].SetActive(false);
                }
            }
        }
    }

    void set_first_situation_of_file_panel(){
        this.decide_file_name_btn.SetActive(false);
        this.alter_file_name_btn.SetActive(true);
        this.file_name_inf.SetActive(false);
        this.file_name.SetActive(true);
        this.file_name.GetComponent<Text>().text=vars.opening_file.name;

        this.all_files_panel.SetActive(false);
        this.create_new_file_panel.SetActive(false);
        this.create_new_folder_panel.SetActive(false);
        this.check_remove_panel.SetActive(false);
    }

    public void decide_file_name(){
        FileFolder opening_folder=vars.opening_file.parent;

        if(opening_folder.GetChild(this.file_name_inf_inf.text)!=null){
            Debug.Log("この名前は既に存在します");    //同じ名前を入力したときもこの警告が出るのはどうにかしたい
        }else if(this.file_name_inf_inf.text==""){
            Debug.Log("名前を入力してください");
        }else{
            opening_folder.children.Add(this.file_name_inf_inf.text,vars.opening_file);
            opening_folder.children.Remove(vars.opening_file.name);
            vars.opening_file.name=this.file_name_inf_inf.text;
            this.decide_file_name_btn.SetActive(false);
            this.alter_file_name_btn.SetActive(true);
            this.file_name_inf.SetActive(false);
            this.file_name.GetComponent<Text>().text=this.file_name_inf_inf.text;
            this.file_name.SetActive(true);
        }
    }

    public void alter_file_name(){
        this.decide_file_name_btn.SetActive(true);
        this.alter_file_name_btn.SetActive(false);
        this.file_name_inf.SetActive(true);
        this.file_name.SetActive(false);
        this.file_name_inf_inf.text=vars.opening_file.name;
    }

    public void open_all_files(){
        this.all_files_panel.SetActive(true);
        this.selected_f_name=vars.opening_file.name;
        this.selected_folder_path=vars.opening_file.parent.GetPath();

        set_all_files(this.selected_folder_path,this.selected_f_name);
    }

    public void save_objs(){
        this.SaveGameObject(vars.opening_file);
        Debug.Log("セーブしました。");
        
        //仮
        GameData GameData=new GameData();
        GameData.SetGameData();   //ここでvarsの値がGameDataに収納される
        GameData.Save();   //ここでPlayerPrefsに入る
        GameData.Confirm();
    }

    public void select_file(string name){
        this.selected_f_name=name;
        //this.selected_folder_path.RemoveAt(this.selected_folder_path.Count-1);
        //this.selected_folder_path.Add(name);

        set_all_files(this.selected_folder_path,this.selected_f_name);
    }

    public void create_new_file1(){
        this.create_new_file_panel.SetActive(true);
    }

    public void create_new_file2(){
        FileFolder opening_folder=vars.userfolder.GetFileFolderByRelativePath(this.selected_folder_path);

        if(opening_folder.GetChild(this.new_file_name_inf_inf.text)!=null){
            Debug.Log("この名前は既に存在します");
        }else if(this.new_file_name_inf_inf.text==""){
            Debug.Log("名前を入力してください");
        }else{
            FileFolder new_file=new FileFolder();
            new_file.CreateFile(this.new_file_name_inf_inf.text,new Dictionary<string,GameObject>());   //これ忘れないように
            opening_folder.AddChild(new_file);

            this.selected_f_name=this.new_file_name_inf_inf.text;

            set_all_files(this.selected_folder_path,this.selected_f_name);
            this.create_new_file_panel.SetActive(false);
        }
    }

    public void cancel_file_creation(){
        this.create_new_file_panel.SetActive(false);
    }

    public void create_new_folder1(){
        this.create_new_folder_panel.SetActive(true);
    }

    public void create_new_folder2(){
        FileFolder opening_folder=vars.userfolder.GetFileFolderByRelativePath(this.selected_folder_path);

        if(opening_folder.GetChild(this.new_folder_name_inf_inf.text)!=null){
            Debug.Log("この名前は既に存在します");
        }else if(this.new_folder_name_inf_inf.text==""){
            Debug.Log("名前を入力してください");
        }else{
            FileFolder new_folder=new FileFolder();
            new_folder.CreateFolder(this.new_folder_name_inf_inf.text);   //これ忘れないように
            opening_folder.AddChild(new_folder);

            this.selected_f_name=this.new_folder_name_inf_inf.text;

            set_all_files(this.selected_folder_path,this.selected_f_name);
            this.create_new_folder_panel.SetActive(false);
        }
    }

    public void cancel_folder_creation(){
        this.create_new_folder_panel.SetActive(false);
    }

    public void open_f(){
    
        FileFolder opening_folder=vars.userfolder.GetFileFolderByRelativePath(this.selected_folder_path);
        FileFolder opening_f=opening_folder.GetChild(this.selected_f_name);

        if(opening_f.is_file){
            this.all_files_panel.SetActive(false);
            this.SaveGameObject(vars.opening_file);   //どうしてもここで削除したObjectのデータがPlayerPrefsに残ってしまう。この問題は解決がかなりむずい→削除したobjのnameだけどこかに保存しておく  //後でここにsaveしますか？という警告を入れる
            this.DestroyGameObject(vars.opening_file);
            vars.opening_file_path=opening_f.GetPath();
            vars.opening_file=opening_f;
            set_first_situation_of_file_panel();
            this.SetGameObject(vars.opening_file);
        }else{
            this.selected_folder_path.Add(opening_f.name);
            this.selected_f_name=null;
            set_all_files(this.selected_folder_path,this.selected_f_name);
        }
    }

    public void remove_f(){
        this.check_remove_panel.SetActive(true);
        this.check_remove_text.text=this.selected_f_name+"を削除します。よろしいですか？";
    }

    public void yes_remove_f(){
        FileFolder opening_folder=vars.userfolder.GetFileFolderByRelativePath(this.selected_folder_path);
        opening_folder.children.Remove(this.selected_f_name);
        set_all_files(this.selected_folder_path,this.selected_f_name);
        this.check_remove_panel.SetActive(false);
    }

    public void no_remove_f(){
        this.check_remove_panel.SetActive(false);
    }

    public void back_parent(){
        this.selected_f_name=this.selected_folder_path[this.selected_folder_path.Count-1];
        this.selected_folder_path.RemoveAt(this.selected_folder_path.Count-1);

        set_all_files(this.selected_folder_path,this.selected_f_name);
    }

    void set_all_files(List<string> selected_folder_path, string selected_f_name){
        FileFolder opening_folder=vars.userfolder.GetFileFolderByRelativePath(selected_folder_path);
        int i=0;
        foreach(var obj in this.file_btn_objs){
            Destroy(obj);
        }

        foreach(var file_name in opening_folder.children.Keys){
            GameObject set=Instantiate(file_btn_pfb,this.content_all_files.transform,false);
            this.file_btn_objs.Add(set);
            set.name=file_name;
            set.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-30*i);
            //set.transform.localPosition=new Vector3(0,-30*i,0);
            set.transform.GetChild(0).GetComponent<Text>().text=file_name;
            if(file_name==selected_f_name){
                set.GetComponent<Image>().color=new Color32(124,213,250,255);
            }else if(i%2==0){
                set.GetComponent<Image>().color=new Color32(217,217,217,255);
            }else{
                set.GetComponent<Image>().color=new Color32(255,255,255,255);
            }
            i+=1;
        }
    }

    public void close_all_files(){
        this.all_files_panel.SetActive(false);
    }

    #endregion

    #region SetSaveGameObject

    public void SetGameObject(FileFolder file){   //FileFolderのpathを取得し、PlayerPrefsからそのfileのobjsをすべてInstance化。またfileのプロパティを全て設定
        List<string> file_path=file.GetPath();
        string all_path="userfolder,"+string.Join(",",file_path);

        if(PlayerPrefs.HasKey(all_path+",obj_structure_list")){  //新規作成するとここがfalseになるが問題ない
            file.obj_structure_list=this.get_obj_structure_list(PlayerPrefs.GetString(all_path+",obj_structure_list"));
            file.obj_name_list.AddRange(PlayerPrefs.GetString(all_path+",obj_name_list").Split(','));
            file.obj_mold_list.AddRange(PlayerPrefs.GetString(all_path+",obj_mold_list").Split(','));
            file.obj_mtr_list.AddRange(PlayerPrefs.GetString(all_path+",obj_mtr_list").Split(','));
        }else Debug.Log("PlayerPrefs doesn't have key: "+all_path+",obj_structure_list");

        GameObject g=new GameObject();
        for(int i=0;i<file.obj_structure_list.Count;i++){
            if(file.obj_structure_list[i]!=0){
                GameObject child=Instantiate(vars.mold_pfb_dict[file.obj_mold_list[file.obj_structure_list[i]-1]],g.transform);
                child.tag=file.obj_mold_list[file.obj_structure_list[i]-1];
                child.transform.position=new Vector3(PlayerPrefs.GetFloat(all_path+","+i.ToString()+",px"), PlayerPrefs.GetFloat(all_path+","+i.ToString()+",py"), PlayerPrefs.GetFloat(all_path+","+i.ToString()+",px"));
                child.transform.eulerAngles=new Vector3(PlayerPrefs.GetFloat(all_path+","+i.ToString()+",ex"),PlayerPrefs.GetFloat(all_path+","+i.ToString()+",ey"),PlayerPrefs.GetFloat(all_path+","+i.ToString()+",ez"));
                child.transform.localScale=new Vector3(PlayerPrefs.GetFloat(all_path+","+i.ToString()+",sx"),PlayerPrefs.GetFloat(all_path+","+i.ToString()+",sy"),PlayerPrefs.GetFloat(all_path+","+i.ToString()+",sz"));
                child.name=file.obj_name_list[file.obj_structure_list[i]-1];
                child.GetComponent<Renderer>().material=vars.mtr_dict[file.obj_mold_list[file.obj_structure_list[i]-1]];
                g=child;
            }else{
                g=g.transform.parent.gameObject;
            }
        }
        file.objects=g;
    }

    public void DestroyGameObject(FileFolder file){   //fileに属するインスタンス化されたobjをすべて削除
        if(file.is_file){
            Destroy(file.objects); //一気にできるか試す
        }else Debug.Log("error");
    }

    public void SaveGameObject(FileFolder file){       //FileFolder→PlayerPrefs
        string path="userfolder,"+string.Join(",",file.GetPath())+",";
        int obj_num=1;

        file.obj_structure_list=new List<int>();
        file.obj_name_list=new List<string>();
        file.obj_mold_list=new List<string>();
        file.obj_mtr_list=new List<string>();

        GameObject g = file.objects;
        bool end=false;
        int node=-1;
        int index=0;
        List<int> num_node=new List<int>();
        List<int> index_node=new List<int>();
        
        while(true){
            while(g.transform.childCount!=0){
                num_node.Add(g.transform.childCount);
                index_node.Add(index);
                node+=1;
                g=g.transform.GetChild(index).gameObject;

                //ここはいらないかもしれない（objectの生成消滅の時に毎回変更しそう)　その時は上のところも治すこと
                file.obj_structure_list.Add(obj_num);
                file.obj_name_list.Add(g.name);
                file.obj_mold_list.Add(g.name.Substring(0,g.name.Length-10));
                file.obj_mtr_list.Add(this.get_mtr_name(g.GetComponent<Material>()));

                string obj_num_str=obj_num.ToString();
                PlayerPrefs.SetFloat(path+obj_num_str+",px",g.transform.position.x);
                PlayerPrefs.SetFloat(path+obj_num_str+",py",g.transform.position.y);
                PlayerPrefs.SetFloat(path+obj_num_str+",pz",g.transform.position.z);
                PlayerPrefs.SetFloat(path+obj_num_str+",ex",g.transform.eulerAngles.x);
                PlayerPrefs.SetFloat(path+obj_num_str+",ey",g.transform.eulerAngles.y);
                PlayerPrefs.SetFloat(path+obj_num_str+",ez",g.transform.eulerAngles.z);
                PlayerPrefs.SetFloat(path+obj_num_str+",sx",g.transform.localScale.x);
                PlayerPrefs.SetFloat(path+obj_num_str+",sy",g.transform.localScale.y);
                PlayerPrefs.SetFloat(path+obj_num_str+",sz",g.transform.localScale.z);
                obj_num+=1;

                index=0;
            }

            g=g.transform.parent.gameObject;
            index_node[node]+=1;
            file.obj_structure_list.Add(0);

            //ここでnodeはぶち当たった末端のnode
            while(num_node[node]==index_node[node]){
                num_node.RemoveAt(num_node.Count-1);
                index_node.RemoveAt(index_node.Count-1);
                node-=1;
                if(node==-1){
                    end=true;
                    break;
                }
                file.obj_structure_list.Add(0);
                index_node[node]+=1;
                g=g.transform.parent.gameObject;
            }
            if(end) break;
            index=index_node[node];
            node-=1;
            num_node.RemoveAt(num_node.Count-1);
            index_node.RemoveAt(index_node.Count-1);
        }

        PlayerPrefs.SetString(path+"obj_structure_list",this.get_obj_structure_list_str(file.obj_structure_list));
        PlayerPrefs.SetString(path+"obj_name_list",string.Join(",",file.obj_name_list));
        PlayerPrefs.SetString(path+"obj_mold_list",string.Join(",",file.obj_mold_list));
        PlayerPrefs.SetString(path+"obj_mtr_list",string.Join(",",file.obj_mtr_list));

        PlayerPrefs.SetString("HaveGameData","true");
    }

    #region functions
    public List<int> get_obj_structure_list(string obj_structure_list_key){
        List<int> obj_structure_list=new List<int>();
        int last_index=0;

        for(int i=0;i<obj_structure_list_key.Length;i++){
            if(obj_structure_list_key[i]==','){
                obj_structure_list.Add(Int32.Parse(obj_structure_list_key.Substring(last_index,i-last_index)));
                last_index=i;
            }
        }

        obj_structure_list.Add(Int32.Parse(obj_structure_list_key.Substring(last_index,obj_structure_list_key.Length-last_index-1)));

        return obj_structure_list;
    }

    public string get_mtr_name(Material mtr){
        foreach(var key in vars.mtr_dict.Keys){
            if(mtr==vars.mtr_dict[key]){
                return key;
            }
        }
        Debug.Log("error");
        return "";
    }

    public string get_obj_structure_list_str(List<int> obj_structure_list){
        string str="";
        foreach(var key in obj_structure_list){
            str+=key.ToString()+",";
        }
        str.Substring(0,str.Length-1);

        return str;
    }
    #endregion

    #endregion

    #region MoldPanel
    public void SetMoldPanel(){   //varsに登録しているMoldPfbを表示。自作のやつはあらかじめvarsに入れておく
        for(int j=0;j<this.ContentMoldScrollView.transform.childCount;j++) Destroy(this.ContentMoldScrollView.transform.GetChild(0).gameObject);

        int i=0;
        foreach(var key in vars.mold_spr_dict.Keys){
            GameObject set=Instantiate(this.MoldScrollViewBtnPfb, this.ContentMoldScrollView.transform);
            set.name=key;
            set.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-10-70*i);
            //set.transform.GetChild(1).GetComponent<MoldScrollViewBtnCtr>().MoldPfb=vars.mold_pfb_dict[key];
            //set.transform.GetChild(1).GetComponent<MoldScrollViewBtnCtr>().mold_name=key;
            set.transform.GetChild(1).GetComponent<Image>().sprite=vars.mold_spr_dict[key];
            //if(key==selected_mold_name) set.transform.GetChild(0).gameObject.SetActive(true);
            //else set.transform.GetChild(0).gameObject.SetActive(false);
            i+=1;
        }
        this.ContentMoldScrollView.GetComponent<RectTransform>().sizeDelta=new Vector2(0,20+70*i);
    }

    public void SelectMold(string mold_name){  //親の方のMoldBtn  //unselectedにしたい場合はmold_nameをnullに
        if(mold_name!=null) this.is_mold_selected=true;
        else this.is_mold_selected=false;

        for(int i=0;i<this.ContentMoldScrollView.transform.childCount;i++){
            if(this.ContentMoldScrollView.transform.GetChild(i).gameObject.name==mold_name){
                this.ContentMoldScrollView.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
            }else{
                this.ContentMoldScrollView.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region ObjList

    public void SetObjListPanel(GameObject objects, List<GameObject> SelectedObj){
        GameObject g=objects;
        bool end=false;
        int node=-1;
        int index=0;
        List<int> num_node=new List<int>();
        List<int> index_node=new List<int>();

        string blank;
        int i=0;
        
        for(int j=0;j<this.ObjListPanel.transform.childCount;j++){
            if(this.ObjListPanel.transform.GetChild(j).gameObject.tag != "DetailScrollView"){
                Destroy(this.ObjListPanel.transform.GetChild(j).gameObject);
            }
        }


        while(true){
            while(g.tag!="Item" && g.tag!="CameraCenter"){
            //while(g.transform.childCount!=0){
                num_node.Add(g.transform.childCount);
                index_node.Add(index);
                node+=1;
                //Debug.Log(g.name);
                g = g.transform.GetChild(index).gameObject;

                //ここに処理
                GameObject set=Instantiate(this.ObjectBtnPfb,this.ObjListPanel.transform);
                set.GetComponent<ObjectBtnCtr>().obj=g;
                set.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-30*i);
                i+=1;
                blank="";
                for(int j=1;i<node;j++) blank+=" ";
                if(g.tag=="Item") set.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text=blank+g.GetComponent<ItemInfo>().name;
                else if(g.tag=="CameraCenter") set.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text=blank+g.name;
                //set.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text=blank+g.tag;   //後でtagではなく名前入力にする
                
                if(SelectedObj.Contains(g)){
                    set.GetComponent<Image>().color=new Color32(178,178,178,255);
                }else{
                    set.GetComponent<Image>().color=new Color32(212,212,212,255);
                }

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

        //if(SelectedObj.Count!=0) SetDetailScrollView(SelectedObj[0]);
    
    }

    public void SetDetailScrollView(GameObject obj){
        this.DetailScrollView.SetActive(true);

        this.DetailItemName.text = obj.GetComponent<ItemInfo>().item_type;

        Debug.Log(obj.transform.position.x.ToString());
        
        this.TransformsInDetailInf[0].text = obj.transform.position.x.ToString();
        this.TransformsInDetailInf[1].text = obj.transform.position.y.ToString();
        this.TransformsInDetailInf[2].text = obj.transform.position.z.ToString();
        this.TransformsInDetailInf[3].text = obj.transform.eulerAngles.x.ToString();
        this.TransformsInDetailInf[4].text = obj.transform.eulerAngles.y.ToString();
        this.TransformsInDetailInf[5].text = obj.transform.eulerAngles.z.ToString();
        this.TransformsInDetailInf[6].text = obj.transform.localScale.x.ToString();
        this.TransformsInDetailInf[7].text = obj.transform.localScale.y.ToString();
        this.TransformsInDetailInf[8].text = obj.transform.localScale.z.ToString();

    }

    public void EditDescription(string dscp){
        if(this.SelectedObjs.Count!=0){
            this.SelectedObjs[0].GetComponent<ItemInfo>().description = dscp;
        }else{
            Debug.Log("ERROR");
        }
    }

    #endregion

    #region Simulate
    public void ClickSimulate(){
        /*
        EngineSml EngineSml = this.SelectedObjs[0].GetComponent<EngineSml>();

        EngineSml.StartSim(this.SelectedObjs[0].GetComponent<ItemInfo>().description);

        //is_simulate_update = true;
        */
    }


    void UpdateSimulate(){
        if(simulate_count == sim_obj_posis.Count){
            is_simulate_update = false;
        }else{
            //this.SimulateObj.transform.position = sim_obj_posis[simulate_count];
            simulate_count += 1;
        }
    }

    List<float> StringToList(string str){
        //str should be like [1.0, 2.0, ...]
        char[] del = {',', '[', ']'};
        string[] arr = str.Split(del);
        List<float> sim_obj_posis = new List<float>();

        for(int i=0;i<arr.Length;i++){
            if(i>0 && i < arr.Length -1){
                sim_obj_posis.Add((float)Convert.ToDouble(arr[i]));
            }
        }

        return sim_obj_posis;
    }

    #endregion

    #region Restraint

    public void SetRestraint(int mode){
        this.restraint_mode=mode;
        this.SetRestraintPanel(mode);
        //this.SetRestraintAxes(CameraCtr.SelectedObj,mode);   //ここはselectedobjがlistに変わってないから直す必要
    }

    public void SetRestraintPanel(int mode){
        this.RestraintPanel.SetActive(true);
        this.RfreeBtn.GetComponent<Image>().color=new Color32(255,255,255,255);
        this.RxBtn.GetComponent<Image>().color=new Color32(255,255,255,255);
        this.RyBtn.GetComponent<Image>().color=new Color32(255,255,255,255);
        this.RzBtn.GetComponent<Image>().color=new Color32(255,255,255,255);
        this.RxyBtn.GetComponent<Image>().color=new Color32(255,255,255,255);
        this.RyzBtn.GetComponent<Image>().color=new Color32(255,255,255,255);
        this.RzxBtn.GetComponent<Image>().color=new Color32(255,255,255,255);

        if(mode==0){
            this.RfreeBtn.GetComponent<Image>().color=new Color32(255,156,0,255);
        }else if(mode==1){
            this.RxBtn.GetComponent<Image>().color=new Color32(255,156,0,255);
        }else if(mode==2){
            this.RyBtn.GetComponent<Image>().color=new Color32(255,156,0,255);
        }else if(mode==3){
            this.RzBtn.GetComponent<Image>().color=new Color32(255,156,0,255);
        }else if(mode==4){
            this.RxyBtn.GetComponent<Image>().color=new Color32(255,156,0,255);
        }else if(mode==5){
            this.RyzBtn.GetComponent<Image>().color=new Color32(255,156,0,255);
        }else if(mode==6){
            this.RzxBtn.GetComponent<Image>().color=new Color32(255,156,0,255);
        }
    }

    public void SetRestraintAxes(GameObject obj,int mode){
        this.RestraintAxes.SetActive(true);
        this.RestraintAxes.transform.position=obj.transform.position;

        this.Rx.SetActive(false);
        this.Ry.SetActive(false);
        this.Rz.SetActive(false);

        if(mode==1){
            this.Rx.SetActive(true);
        }else if(mode==2){
            this.Ry.SetActive(true);
        }else if(mode==3){
            this.Rz.SetActive(true);
        }else if(mode==4){
            this.Rx.SetActive(true);
            this.Ry.SetActive(true);
        }else if(mode==5){
            this.Ry.SetActive(true);
            this.Rz.SetActive(true);
        }else if(mode==6){
            this.Rz.SetActive(true);
            this.Rx.SetActive(true);
        }
    }

    public void RestraintOff(){
        this.RestraintPanel.SetActive(false);
        this.RestraintAxes.SetActive(false);
    }

    #endregion

    #region other functions

    public void StartCount()
    {
        this.update_count = 0;
        this.is_update_count = true;
    }

    public void EndCount()
    {
        this.update_count = 0;
        this.is_update_count = false;
    }

    public void SetPhysics(GameObject obj, bool isKinematic, bool isTrigger, bool isRenderer){
        GameObject g = obj;
        bool end=false;
        int node=-1;
        int index=0;
        List<int> num_node=new List<int>();
        List<int> index_node=new List<int>();
        
        if(g.GetComponent<Rigidbody>()!=null){
            g.GetComponent<Rigidbody>().isKinematic=isKinematic;
        }
        if(g.GetComponent<Collider>()!=null){
            g.GetComponent<Collider>().isTrigger=isTrigger;
        }
        if(g.GetComponent<Renderer>()!=null){
            g.GetComponent<Renderer>().enabled=isRenderer;
        }

        if(g.transform.childCount!=0){
            while(true){
                while(g.transform.childCount!=0){
                    num_node.Add(g.transform.childCount);
                    index_node.Add(index);
                    node+=1;
                    g=g.transform.GetChild(index).gameObject;

                    //ここに処理
                    if(g.GetComponent<Rigidbody>()!=null){
                        g.GetComponent<Rigidbody>().isKinematic=isKinematic;
                    }
                    if(g.GetComponent<Collider>()!=null){
                        g.GetComponent<Collider>().isTrigger=isTrigger;
                    }
                    if(g.GetComponent<Renderer>()!=null){
                        g.GetComponent<Renderer>().enabled=isRenderer;
                    }

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
    }

    public void search_node_template(GameObject obj){  //注:objには処理が行われない
        GameObject g = obj;
        bool end=false;
        int node=-1;
        int index=0;
        List<int> num_node=new List<int>();
        List<int> index_node=new List<int>();
        
        while(true){
            while(g.transform.childCount!=0){
                num_node.Add(g.transform.childCount);
                index_node.Add(index);
                node+=1;
                g=g.transform.GetChild(index).gameObject;

                //ここに処理

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

    /*
    public void AttachCollider(GameObject obj, string collider_type, Vector3 collider_size, Vector3 collider_center, Vector3 collider_direction){
        switch(collider_type){
            case "box":
                obj.AddComponent<BoxCollider>();
                obj.BoxCollider.center=collider_center;
                obj.BoxCollider.size=collider_size;
            
            case "sphere":
                obj.AddComponent<SphereCollider>();
                obj.SphereCollider.center=collider_center;
                obj.SphereCollider.radius=collider_size.x;

            case "capsule":
                obj.AddComponent<CapsuleCollider>();
                obj.CapsuleCollider.center=collider_center;

                obj.CapsuleCollider.radius=collider_size.x;
                obj.CapsuleCollider.height=collider_size.y;

                obj.CapsuleCollider.direction=collider_direction;

            case "mesh":
                obj.AddComponent<MeshCollider>();
        }
    }
    */


    //obj1-obj2
    //inside,outside: all surface directed to normal vector is inside, outside
    (GameObject, bool, bool) Subtract(GameObject obj1, GameObject obj2, Vector3 normal){

        //this is because mesh is generated with the center at (0,0,0)
        Vector3 ori_posi=obj1.transform.position;
        Vector3 ori_rot=obj1.transform.localEulerAngles;
        Vector3 ori_sca=obj1.transform.localScale;
        
        Transform original_parent=obj2.transform.parent;
        obj2.transform.parent=obj1.transform;

        obj1.transform.position=new Vector3(0,0,0);
        obj1.transform.localEulerAngles=new Vector3(0,0,0);
        obj1.transform.localScale=new Vector3(1,1,1);

        obj2.transform.parent=null;

        GameObject new_obj = Instantiate(obj1, obj1.transform.parent);

        (Model result, bool outside, bool inside) = CSG.SubtractWithTag1(obj1, obj2, normal);

        obj2.transform.parent=obj1.transform;

        obj1.transform.position=ori_posi;
        obj1.transform.localEulerAngles=ori_rot;
        obj1.transform.localScale=ori_sca;

        obj2.transform.parent=original_parent;
        
        new_obj.GetComponent<MeshFilter>().sharedMesh = result.mesh;
        new_obj.GetComponent<MeshRenderer>().sharedMaterials = result.materials.ToArray();

        new_obj.transform.position = ori_posi;
        new_obj.transform.localEulerAngles = ori_rot;
        new_obj.transform.localScale = ori_sca;

        return (new_obj, outside, inside);
    }
    #endregion
}