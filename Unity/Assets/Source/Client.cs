using UnityEngine;

namespace Game
{
    public class Client : MonoBehaviour
    {
        public static Client Ins { get; private set; }

        public static NetworkManager NetworkMgr = new NetworkManager();
        public static ServerListManager ServerMgr = new ServerListManager();
        public static UpdateManager UpdateMgr = new UpdateManager();
        public static ResourceManager ResMgr = new ResourceManager();
        public static GameManager GameMgr = new GameManager();
        public static PoolManager PoolMgr = new PoolManager();
        public static LuaManager LuaMgr = new LuaManager();

        void Awake()
        {
            Ins = this;
            DontDestroyOnLoad(gameObject);

            Util.SetResolution(ConstSetting.Resolution);
            Application.targetFrameRate = ConstSetting.FrameRate;
            QualitySettings.blendWeights = ConstSetting.Blend;
            Screen.sleepTimeout = ConstSetting.SleepTime;

            FairyGUI.UIObjectFactory.SetLoaderExtension(typeof(MyLoader));

            GameMgr.Init();

            Debug.LogError(Application.persistentDataPath);
        }

        void Update()
        {
            ResMgr.Update();
            UpdateMgr.Update();
            NetworkMgr.Update();
        }

        void OnDestroy()
        {
            NetworkMgr.Dispose();
            ServerMgr.Dispose();
            UpdateMgr.Dispose();
            ResMgr.Dispose();
            GameMgr.Dispose();
            PoolMgr.Dispose();
            LuaMgr.Dispose();

            NetworkMgr = null;
            ServerMgr = null;
            UpdateMgr = null;
            ResMgr = null;
            GameMgr = null;
            PoolMgr = null;
            LuaMgr = null;
            Ins = null;
        }
    }
}