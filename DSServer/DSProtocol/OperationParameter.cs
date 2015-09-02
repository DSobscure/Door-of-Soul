namespace DSProtocol
{
    public enum OpenDSParameterItem
    {
        Account,
        Password
    }

    public enum GetSoulListParameterItem
    {
        AnswerUniqueID
    }

    public enum GetContainerListParameterItem
    {
        SoulUniqueID
    }

    public enum ActiveSoulParameterItem
    {
        SoulUniqueID
    }

    public enum GetSceneDataParameterItem
    {
        SceneUniqueID
    }

    public enum ProjectToSceneParameterItem
    {
        ContainerUniqueID,
        SceneUniqueID
    }

    public enum ControlTheSceneParameterItem
    {
        AdministratorUniqueID,
        SceneUniqueID
    }

    public enum ContainerMoveRequestParameterItem
    {

    }

    public enum SendMoveTargetPositionParameterItem
    {
        SceneUniqueID,
        ContainerUniqueID,
        PositionX,
        PositionY,
        PositionZ
    }

    public enum ContainerPositionUpdateParameterItem
    {
        SceneUniqueID,
        ContainerUniqueID,
        PositionX,
        PositionY,
        PositionZ
    }
}
