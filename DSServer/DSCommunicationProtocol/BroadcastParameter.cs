namespace DSCommunicationProtocol
{
    public enum ActiveSoulBroadcastItem
    {
        SoulUniqueID
    }

    public enum ProjectContainerBroadcastItem
    {
        SceneUniqueID,
        ContainerDataString
    }

    public enum DisconnectBroadcastItem
    {
        SoulUniqueIDListDataString,
        SceneUniqueIDListDataString,
        ContainerUniqueIDListDataString
    }

    public enum ContainerMoveRequestBroadcastItem
    {

    }

    public enum SendMoveTargetPositionBroadcastItem
    {
        SceneUniqueID,
        ContainerUniqueID,
        PositionX,
        PositionY,
        PositionZ
    }

    public enum ContainerPositionUpdateBroadcastItem
    {
        SceneUniqueID,
        ContainerUniqueID,
        PositionX,
        PositionY,
        PositionZ,
        EulerAngleY
    }

    public enum SendMessageBroadcastItem
    {
        ContainerUniqueID,
        ContainerName,
        MessageLevel,
        Message
    }
}
