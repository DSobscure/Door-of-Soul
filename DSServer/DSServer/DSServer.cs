using System;
using System.Collections.Generic;
using System.IO;
using Photon.SocketServer;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net.Config;
using DSDataStructure.WorldLevelStructure;
using DSDataStructure;
using DSProtocol;

namespace DSServer
{
    public class DSServer : ApplicationBase
    {
        public static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        public Graph worldGraph { get; set; }
        public Dictionary<Guid, DSPeer> WandererDictionary { get; set; }
        public Dictionary<int, Scene> SceneDictionary { get; set; }
        public Dictionary<int, Container> ContainerDictionary { get; set; }
        public Dictionary<int, Answer> AnswerDictionary { get; set; }
        public Dictionary<int, Scene> SceneAdministratorDictionary { get; set; }
        public DSDatabase database;

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            return new DSPeer(initRequest.Protocol, initRequest.PhotonPeer, this);
        }

        protected override void Setup()
        {
            log4net.GlobalContext.Properties["Photon:ApplicationLogPath"] =
                Path.Combine(this.ApplicationPath, "log");

            string path = Path.Combine(this.BinaryPath, "log4net.config");
            var file = new FileInfo(path);
            if (file.Exists)
            {
                LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
                XmlConfigurator.ConfigureAndWatch(file);
            }

            WandererDictionary = new Dictionary<Guid, DSPeer>();
            SceneDictionary = new Dictionary<int, Scene>();
            ContainerDictionary = new Dictionary<int, Container>();
            AnswerDictionary = new Dictionary<int, Answer>();
            SceneAdministratorDictionary = new Dictionary<int, Scene>();

            Log.Info("Server Setup successiful!.......");

            database = new DSDatabase("localhost", "root", "", "door of soul");
            if (database.Connect())
            {
                Log.Info("Database Connect successiful!.......");
            }

            InitialWorldGraph();
        }

        protected override void TearDown()
        {
            database.Dispose(); 
        }

        public void Broadcast(DSPeer[] peers, BroadcastType broadcastType, Dictionary<byte, object> parameter)
        {
            EventData eventData = new EventData((byte)broadcastType, parameter);
            foreach (DSPeer peer in peers)
            {
                peer.SendEvent(eventData, new SendParameters());
            }
        }

        private void InitialWorldGraph()
        {
            SceneDictionary.Add(1,new Scene(1,"testScene",null));
        }
    }
}
