using System;
using System.Collections.Generic;
using System.Linq;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using ExitGames.Logging;
using DSServerStructure;
using DSCommunicationProtocol;
using DSSerializable.CharacterStructure;
using DSSerializable;
using DSServerStructure.WorldLevelStructure;

namespace DSServer
{
    public partial class DSPeer : PeerBase
    {
        private List<DSPeer> GetPeerListByMessageLevel(Container container, MessageLevel level)
        {
            List<DSPeer> peers = new List<DSPeer>();
            switch (level)
            {
                case MessageLevel.Scene:
                    {
                        foreach (Container targetContainer in container.location.containerDictionary.Values)
                        {
                            foreach (Soul targetSoul in targetContainer.soulDictionary.Values)
                            {
                                peers.Add(targetSoul.sourceAnswer.peer);
                            }
                        }
                    }
                    break;
            }
            return peers;
        }
    }
}
