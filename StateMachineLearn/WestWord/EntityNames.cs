﻿namespace StateMachineLearn;

public enum EntityName
{
    EntityMinerBob,
    EntityElsa,
    EntityFly,
}

public static class EnumExtensions
{
    public static string GetName(this EntityName entityName)
    {
        return entityName switch
        {
            EntityName.EntityMinerBob => "Miner Bob",
            EntityName.EntityElsa => "Elsa",
            EntityName.EntityFly => "Fly",
            _ => "Unknown"
        };
    }
}

