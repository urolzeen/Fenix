﻿//AUTOGEN, do not modify it!

using Fenix.Common;
using Fenix.Common.Attributes;
using Fenix.Common.Rpc;
using MessagePack; 
using System.ComponentModel;
using System; 

namespace Fenix.Common.Message
{
    [MessageType(OpCode.REMOVE_ACTOR_REQ)]
    [MessagePackObject]
    public class RemoveActorReq : IMessage
    {
        [Key(0)]
        public UInt32 actorId { get; set; }

    }
}

