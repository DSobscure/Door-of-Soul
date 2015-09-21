using System;
using System.Collections.Generic;
using System.Linq;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using ExitGames.Logging;
using DSDataStructure;
using DSProtocol;
using DSSerializable.CharacterStructure;
using DSSerializable;
using DSDataStructure.WorldLevelStructure;

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
                        foreach (Container targetContainer in container.Location.ContainerDictionary.Values)
                        {
                            foreach (Soul targetSoul in targetContainer.SoulDictionary.Values)
                            {
                                peers.Add(targetSoul.SourceAnswer.Peer);
                            }
                        }
                    }
                    break;
            }
            return peers;
        }
    }
}
