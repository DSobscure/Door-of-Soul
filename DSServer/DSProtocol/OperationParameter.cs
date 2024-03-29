﻿namespace DSCommunicationProtocol
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
        PositionZ,
        EulerAngleY
    }

    public enum SendMessageParameterItem
    {
        ContainerUniqueID,
        MessageLevel,
        Message
    }
}
