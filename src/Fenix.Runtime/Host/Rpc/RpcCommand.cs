﻿/*
 * RpcCommand
 */

using Fenix;
using Fenix.Common;
using Fenix.Common.Attributes;
using Fenix.Common.Rpc;
using Fenix.Common.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
namespace Fenix
{
    public class RpcCommand
    {
        public ulong Id => packet.Id;

        public uint FromActorId => packet.FromActorId;

        public uint ToActorId => packet.ToActorId;

        public uint FromHostId => packet.FromHostId;

        public uint ToHostId => packet.ToHostId;

        public IMessage Msg => packet.Msg;

        public Api RpcType => Global.TypeManager.GetRpcType(ProtoCode);

        public uint ProtoCode => packet.ProtoCode;

        protected NetworkType netType => packet.NetType;

        Packet packet;

        protected Entity mInvoker;

        protected Action<byte[]> callbackMethod;

        public long CallTime = 0;

        protected RpcCommand()
        {
            CallTime = TimeUtil.GetTimeStampMS();
        }

        public static RpcCommand Create(Packet packet, Action<byte[]> cb, Entity invoker)
        {
            var obj = new RpcCommand();
            obj.packet = packet;
            obj.callbackMethod = cb;
            obj.mInvoker = invoker;
            return obj;
        }

        public T ToMessage<T>() where T : IMessage
        {
            return this.Msg as T;
        }

        public void Call(Action callDone)
        { 
            var args = new List<object>();
            args.Add(this.Msg);

            if (!this.Msg.HasCallback())
            {
                callDone?.Invoke();
            }
            else
            {
                var cb = new Action<object>((cbMsg) =>
                {
                    callDone?.Invoke();
                    this.mInvoker.RpcCallback(this.Id, this.ProtoCode, this.ToHostId, this.FromHostId, this.ToActorId, this.FromActorId, this.netType, cbMsg);
                });

                args.Add(cb);
            }

            if (this.ProtoCode <= OpCode.CALL_ACTOR_METHOD)
            {
                var peer = NetManager.Instance.GetPeerById(packet.FromHostId, this.netType);
                var context = new RpcContext(this.packet, peer);
                args.Add(context);
            }

            //if (Global.IsServer)
            //{
            if (RpcType == Api.ServerApi)
                this.mInvoker.CallMethodWithMsg(this.ProtoCode, args.ToArray());
            else if (RpcType == Api.ServerOnly)
                this.mInvoker.CallMethodWithMsg(this.ProtoCode, args.ToArray());
            //}
            //else
            //{
            if (RpcType == Api.ClientApi)
                this.mInvoker.CallMethodWithMsg(this.ProtoCode, args.ToArray());
            //}
        }

        public void Callback(byte[] cbData)
        {
            this.callbackMethod?.Invoke(cbData);
        }
    }


}