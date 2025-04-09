using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class vars : MonoBehaviour
{

    #region variables

    #region Dictionaries

    public static Dictionary<string,GameObject> mold_pfb_dict=new Dictionary<string,GameObject>();   //this is gonna be the unchangable fundamental component
    public static Dictionary<string,GameObject> mold_outline_pfb_dict=new Dictionary<string,GameObject>();  //tagを引数に
    public static Dictionary<string,float> mold_height_dict=new Dictionary<string,float>(){{"Cube",0.5f},{"Sphere",0.5f},{"Capsule",1}};
    public static Dictionary<string,Vector3> mold_up_dir_to_surface_dict=new Dictionary<string,Vector3>(){{"Cube",new Vector3(0,1,0)},{"Sphere",new Vector3(0,1,0)},{"Capsule",new Vector3(0,1,0)}};

    public static Dictionary<string,GameObject> item_pfb_dict=new Dictionary<string,GameObject>();   //全てのitemのdict
    
    public static Dictionary<string,int> player_item_dict=new Dictionary<string,int>();  //Every item player has
    public static Dictionary<string,int> item_genre_dict=new Dictionary<string,int>();
    //1:field item (the items player can use in fields)  2:parts ingredient (the items player can use as ingredients of parts)  3:parts (the ones player created)  4:instrument (the ones player set some command) 5:equipment (the ones player can equip) 6:other (like ore or leaf)

    public static List<Dictionary<string,int>> player_item_dicts=new List<Dictionary<string,int>>(); //All items splited into dict by their genre

    // Use player_items istead of dict bellow
    public static Dictionary<string,int> player_field_item_dict=new Dictionary<string,int>();
    public static Dictionary<string,int> player_parts_ingredient_dict=new Dictionary<string,int>();
    public static Dictionary<string,int> player_parts_dict=new Dictionary<string,int>();
    public static Dictionary<string,int> player_instrument_dict=new Dictionary<string,int>();
    public static Dictionary<string,int> player_equipment_dict=new Dictionary<string,int>();
    public static Dictionary<string,int> player_other_dict=new Dictionary<string,int>();

    public static Dictionary<string,Material> mtr_dict=new Dictionary<string,Material>();

    public static Dictionary<string,Sprite> item_spr_dict=new Dictionary<string,Sprite>();
    public static Dictionary<string,Sprite> mold_spr_dict=new Dictionary<string,Sprite>();

    #endregion
    
    #region MoldPfbs
    public GameObject CubePfb;
    public GameObject SpherePfb;
    public GameObject CapsulePfb;
    public GameObject EnginePfb;
    public GameObject PipePfb;
    public GameObject TankPfb;
    public GameObject PillarPfb;
    public GameObject WheelPfb;
    public GameObject PlatePfb;
    public GameObject PistonPfb;
    public GameObject MobiusPfb;
    public GameObject Reactor1Pfb;
    public GameObject Reactor2Pfb;
    public GameObject Turbine1Pfb;
    public GameObject Aeroplane1Pfb;
    public GameObject Generator1Pfb;

    public GameObject CubeOutlinePfb;
    public GameObject SphereOutlinePfb;
    public GameObject CapsuleOutlinePfb;
    public GameObject EngineOutlinePfb;
    public GameObject PipeOutlinePfb;
    public GameObject TankOutlinePfb;
    #endregion

    #region FileFolder&GameData

    //FileFolder関連
    public static FileFolder userfolder=new FileFolder();    //もしstaticではなかったらこのuserfolderに付け加えられないの？
    public static List<string> opening_file_path=new List<string>();  //userfolderは含めない
    public static FileFolder opening_file;

    //ゲームデータ関連　　　　　(注:これは必ず最後に宣言)
    public GameData GameData=new GameData();

    #endregion

    #region Sprite
    public Sprite CubeSpr;
    public Sprite SphereSpr;
    public Sprite CapsuleSpr;
    public Sprite EngineSpr;
    public Sprite PipeSpr;
    public Sprite TankSpr;
    public Sprite PillarSpr;
    public Sprite WheelSpr;
    public Sprite PlateSpr;
    public Sprite PistonSpr;
    public Sprite MobiusSpr;
    public Sprite Reactor1Spr;
    public Sprite Reactor2Spr;
    public Sprite Turbine1Spr;
    public Sprite Aeroplane1Spr;
    public Sprite Generator1Spr;
    #endregion

    #region Field
    public static List<Vector3> field_info;
    #endregion

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        // PlayerPrefsがHaveGameDataを持つのはlabで保存された時だけ
        if(PlayerPrefs.HasKey("HaveGameData")){   //この場合わけはGameDataに入れた方が良さそう
            Debug.Log("初期化状態ではありません");
            this.GameData.LoadSavedData();
            this.GameData.SetVars();
        }else{  //初期化状態の時
            Debug.Log("初期化状態です");
            this.initialization();
        }

        set_item_pfb_dict();
        set_mold_pfb_dict();
        set_mold_outline_pfb_dict();
        set_item_spr_dict();
        set_mold_spr_dict();
        set_mtr_dict();
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.Space)){
            this.initialization();
            Debug.Log("初期化しました。");
        }
    }

    void set_item_pfb_dict(){
    }

    void set_mold_pfb_dict(){
        vars.mold_pfb_dict.Add("Cube",this.CubePfb);
        vars.mold_pfb_dict.Add("Sphere",this.SpherePfb);
        vars.mold_pfb_dict.Add("Capsule",this.CapsulePfb);
        vars.mold_pfb_dict.Add("Engine",this.EnginePfb);
        vars.mold_pfb_dict.Add("Pipe",this.PipePfb);
        vars.mold_pfb_dict.Add("Tank",this.TankPfb);
        vars.mold_pfb_dict.Add("Pillar",this.PillarPfb);
        vars.mold_pfb_dict.Add("Wheel",this.WheelPfb);
        vars.mold_pfb_dict.Add("Plate",this.PlatePfb);
        vars.mold_pfb_dict.Add("Piston",this.PistonPfb);
        vars.mold_pfb_dict.Add("Mobius",this.MobiusPfb);
        vars.mold_pfb_dict.Add("Reactor1",this.Reactor1Pfb);
        vars.mold_pfb_dict.Add("Reactor2",this.Reactor2Pfb);
        vars.mold_pfb_dict.Add("Turbine1",this.Turbine1Pfb);
        vars.mold_pfb_dict.Add("Aeroplane1",this.Aeroplane1Pfb);
        vars.mold_pfb_dict.Add("Generator1",this.Generator1Pfb);
    }

    void set_mold_outline_pfb_dict(){
        vars.mold_outline_pfb_dict.Add("Cube",this.CubeOutlinePfb);
        vars.mold_outline_pfb_dict.Add("Sphere",this.SphereOutlinePfb);
        vars.mold_outline_pfb_dict.Add("Capsule",this.CapsuleOutlinePfb);
        vars.mold_outline_pfb_dict.Add("Engine",this.EngineOutlinePfb);
        vars.mold_outline_pfb_dict.Add("Pipe",this.PipeOutlinePfb);
        vars.mold_outline_pfb_dict.Add("Tank",this.TankOutlinePfb);
    }

    void set_item_spr_dict(){
        
    }

    void set_mold_spr_dict(){
        vars.mold_spr_dict.Add("Cube",this.CubeSpr);
        vars.mold_spr_dict.Add("Sphere",this.SphereSpr);
        vars.mold_spr_dict.Add("Capsule",this.CapsuleSpr);
        vars.mold_spr_dict.Add("Engine",this.EngineSpr);
        vars.mold_spr_dict.Add("Pipe",this.PipeSpr);
        vars.mold_spr_dict.Add("Tank",this.TankSpr);
        vars.mold_spr_dict.Add("Pillar",this.PillarSpr);
        vars.mold_spr_dict.Add("Wheel",this.WheelSpr);
        vars.mold_spr_dict.Add("Plate",this.PlateSpr);
        vars.mold_spr_dict.Add("Piston",this.PistonSpr);
        vars.mold_spr_dict.Add("Mobius",this.MobiusSpr);
        vars.mold_spr_dict.Add("Reactor1",this.Reactor1Spr);
        vars.mold_spr_dict.Add("Reactor2",this.Reactor2Spr);
        vars.mold_spr_dict.Add("Turbine1",this.Turbine1Spr);
        vars.mold_spr_dict.Add("Aeroplane1",this.Aeroplane1Spr);
        vars.mold_spr_dict.Add("Generator1",this.Generator1Spr);
    }

    void set_mtr_dict(){
    }

    public void initialization(){   //これを実行すればすべて初期化される
        PlayerPrefs.DeleteAll();

        vars.opening_file=new FileFolder();
        vars.opening_file.CreateFile("File1",new Dictionary<string,GameObject>());

        vars.userfolder=new FileFolder();
        vars.userfolder.CreateFolder("userfolder");
        vars.userfolder.AddChild(vars.opening_file);
        vars.opening_file_path.Add("File1");

        this.GameData.initialization();
        this.GameData.SetGameData();
    }

}

public class FileFolder
{
    public bool is_file_created;
    public bool is_file;
    public string name;
    public Dictionary<string,GameObject> objs;  //後々廃止

    //File情報
    public FileFolder parent;
    public Dictionary<string,FileFolder> children=new Dictionary<string,FileFolder>();

    //Object Properties
    public GameObject objects;  //全てのオブジェクトの親
    public List<int> obj_structure_list=new List<int>();
    public List<string> obj_mold_list=new List<string>();
    public List<string> obj_name_list=new List<string>();
    public List<GameObject> obj_list=new List<GameObject>();
    public List<string> obj_mtr_list=new List<string>();


    public FileFolder(){
        this.is_file_created=true;
    }

    public void CreateFile(string name, Dictionary<string,GameObject> objs)
    {
        this.is_file=true;
        this.name=name;
        this.objs=objs;
    }

    public void CreateFolder(string name){
        this.is_file=false;
        this.name=name;
    }

    public void AddChild(FileFolder filefolder){
        filefolder.parent=this;

        if(this.is_file==false){
            this.children.Add(filefolder.name,filefolder);
        }else{
            Debug.Log("This is a file, so you can't add the child");
        }
    }

    public FileFolder GetChild(string name){
        foreach(var key in this.children.Keys){
            if(name==key){
                return this.children[key];
            }
        }

        return null;
    }

    public FileFolder GetChildById(int id){
        int i=0;
        foreach(var key in this.children.Keys){
            if(i==id)  return this.children[key];
            i+=1;
        }

        return null;
    }

    public int childCount(){
        if(this.children==null){
            return 0;
        }

        int i=0;
        foreach(var key in this.children.Keys){
            i+=1;
        }
        return i;
    }

    public List<string> GetPath(){   //userfolderはここではpathに含めない
        List<string> p=new List<string>();
        FileFolder f=this;
        while(f.parent!=null){
            p.Insert(0,f.name);
            f=f.parent;
        }
        return p;
    }

    public FileFolder GetFileFolderByRelativePath(List<string> path){
        FileFolder f=vars.userfolder;
        foreach(var f_name in path){
            f=f.GetChild(f_name);
        }
        return f;
    }

    public void SetObjectProperties(GameObject objects){   //saveするときにやるから今のところ使わなくていいが、もしworkspaceのSaveGameObjectにエラーがなければコピー&修正してここに貼り付ける
    }
}

[Serializable]
public class GameData : object
{
    #region variables

    public string name;

    //FileFolderのデータはすべてここ
    public FileFolder userfolder=new FileFolder();

    //DictData  Dictionary型は二つのListに分けて保存
    public List<string> player_item_dict_key=new List<string>();
    public List<int> player_item_dict_value=new List<int>();

    public List<string> opening_file_path=new List<string>();

    #endregion

    public GameData(){    //ここではゲーム中の変更も加味してvarsの値がクラスに収納されるようにした   //そうすると最初にGameData作るときにnullを代入してしまう  //コンストラクタは特に何もしないことにする
        this.name="data";
    }

    public void SetGameData(){  //vars → GameDataのプロパティ
        this.userfolder=vars.userfolder;
        this.opening_file_path=vars.opening_file_path;
        (this.player_item_dict_key,this.player_item_dict_value)=this.DictToList<string,int>(vars.player_item_dict);
    }

    public void Save(){      //GameDataのプロパティ → PlayerPrefs
        if(PlayerPrefs.HasKey("HaveGameData")==false){
            PlayerPrefs.SetString("HaveGameData","true");
        }
        PlayerPrefs.SetString("SavedGameData",this.GetJsonData());   //GameDataのプロパティのうちstring,float,intおよびそのListはここで保存される
        SaveUserFolder();   //userfolderを保存する
    }

    public void LoadSavedData(){    //PlayerPrefs → GameDataのプロパティ　(わざわざこうしてるのはJsonUtility.FromJsonOverwriteが便利だから)    例外:userfolderは完全ではない(ItemCtr2で改めて作る)
        if(PlayerPrefs.HasKey("SavedGameData")){
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString("SavedGameData"), this);   //ここはstringデータだから
        }else Debug.Log("error");

        if(PlayerPrefs.HasKey("userfolder structure")){
            LoadUserFolder();    //userfolderの枠組みだけthis.userfolderに作る(つまりthis.userfolderは)
        }else Debug.Log("error");
    }

    public void SetVars(){   //GameDataのプロパティ → vars
        vars.userfolder=this.userfolder;
        vars.opening_file_path=this.opening_file_path;
        vars.opening_file=new FileFolder().GetFileFolderByRelativePath(this.opening_file_path);
        vars.player_item_dict=this.ListToDict<string,int>(this.player_item_dict_key,this.player_item_dict_value);
    }

    #region functions

    public Dictionary<T1,T2> ListToDict<T1,T2>(List<T1> list_key,List<T2> list_value){
        Dictionary<T1,T2> dict=new Dictionary<T1,T2>();
        if(list_key.Count==list_value.Count){
            for(int i=0;i<list_key.Count;i++){
                dict.Add(list_key[i],list_value[i]);
            }
        }else{
            Debug.Log("the length of key and value must be same");
        }
        return dict;
    }

    public (List<T1> keys, List<T2> values) DictToList<T1,T2>(Dictionary<T1,T2> dict){
        List<T1> list_key=new List<T1>();
        List<T2> list_value=new List<T2>();
        foreach(var key in dict.Keys){
            list_key.Add(key);
            list_value.Add(dict[key]);
        }
        return (list_key,list_value);
    }

    public string GetJsonData() {   //これをfloatに使うとおそらくデータ量が跳ね上がる
		return JsonUtility.ToJson(this);
	}

    public void LoadUserFolder(){  //PlayerPrefsからthis.userfolderのデータ構造だけ先に作る(all_filesで必要)
        string userfolder_structure=PlayerPrefs.GetString("userfolder structure");
        List<string> reconstruct_list=new List<string>();
        string[] arr=userfolder_structure.Split(',');  //注:'はchar型で"はsrtring型  ここではchar型を引数に
        reconstruct_list.AddRange(arr);

        bool end=false;
        int i=1;
        FileFolder f=new FileFolder();
        f.CreateFolder("userfolder");

        while(end==false){
            FileFolder child=new FileFolder();
            Debug.Log(reconstruct_list[i]);
            if(reconstruct_list[i+1]==""){
                child.CreateFile(reconstruct_list[i],new Dictionary<string,GameObject>());
                i+=1;
                f.AddChild(child);
                child=child.parent;
                while(reconstruct_list[i+1]==""){
                    child=child.parent;
                    i+=1;
                    if(i==reconstruct_list.Count-1){
                        end=true;
                        break;
                    }
                }
            }else{
                child.CreateFolder(reconstruct_list[i]);
                f.AddChild(child);
            }
            i+=1;
            f=child;
        }

        this.userfolder=f;
    }

    public void SaveUserFolder(){   //vars.userfolderをPlayerPrefsに保存する
        FileFolder f=vars.userfolder;
        string userfolder_structure="userfolder";
        bool end=false;
        int node=-1;
        int index=0;
        List<int> num_node=new List<int>();
        List<int> index_node=new List<int>();
        List<string> name_path=new List<string>();

        while(true){
            while(f.childCount()!=0){
                num_node.Add(f.childCount());
                index_node.Add(index);
                node+=1;
                f=f.GetChildById(index);
                name_path.Add(f.name);
                PlayerPrefs.SetString("userfolder,"+string.Join(",",name_path),null);//このcodeではここでのみ処理を行えばいい
                userfolder_structure+=","+f.name;
                index=0;
            }

            f=f.parent;
            index_node[node]+=1;
            name_path.RemoveAt(name_path.Count-1);
            userfolder_structure+=",";

            //ここでnodeはぶち当たった末端のnode

            while(num_node[node]==index_node[node]){
                num_node.RemoveAt(num_node.Count-1);
                index_node.RemoveAt(index_node.Count-1);
                node-=1;
                if(node==-1){
                    userfolder_structure.Substring(0,userfolder_structure.Length-1);
                    end=true;
                    break;
                }
                userfolder_structure+=",";
                name_path.RemoveAt(name_path.Count-1);
                index_node[node]+=1;
                f=f.parent;
            }
            if(end) break;
            index=index_node[node];
            node-=1;
            name_path.Add(f.name);
            num_node.RemoveAt(num_node.Count-1);
            index_node.RemoveAt(index_node.Count-1);
        }

        PlayerPrefs.SetString("userfolder structure",userfolder_structure);
    }

    public void Confirm(){
        Debug.Log(PlayerPrefs.GetString("SavedGameData"));
    }

    public void initialization(){
        this.userfolder=new FileFolder();
        this.player_item_dict_key=new List<string>();
        this.player_item_dict_value=new List<int>();
        this.opening_file_path=new List<string>();
    }

    #endregion
}