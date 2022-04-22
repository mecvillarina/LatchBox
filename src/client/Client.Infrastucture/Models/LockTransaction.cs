using Client.Infrastructure.Extensions;
using Neo;
using Neo.VM.Types;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class LockTransaction
    {
        public BigInteger LockIndex { get; private set; }
        public UInt160 TokenScriptHash { get; private set; }
        public UInt160 InitiatorHash160 { get; private set; }
        public string InitiatorAddress { get; private set; }
        public DateTimeOffset CreationTime { get; private set; }
        public DateTimeOffset StartTime { get; private set; }
        public double DurationInDays { get; private set; }
        public bool IsRevocable { get; private set; }
        public bool IsRevoked { get; private set; }
        public bool IsActive { get; private set; }
        public DateTimeOffset UnlockTime { get; private set; }

        public List<LockReceiver> Receivers { get; private set; }

        public LockTransaction(Map map, ProtocolSettings protocolSettings)
        {
            LockIndex = map["Idx"].GetInteger();
            TokenScriptHash = Neo.Network.RPC.Utility.GetScriptHash(map["TokenScriptHash"].FromByteStringToAccount(), protocolSettings);
            InitiatorHash160 = Neo.Network.RPC.Utility.GetScriptHash(map["InitiatorAddress"].FromByteStringToAccount(), protocolSettings);
            InitiatorAddress = InitiatorHash160.ToAddress(protocolSettings.AddressVersion);
            CreationTime = map["CreationTime"].GetInteger().ToDateTimeOffsetFromMilliseconds();
            StartTime = map["StartTime"].GetInteger().ToDateTimeOffsetFromMilliseconds();
            DurationInDays = TimeSpan.FromMilliseconds(((long)map["DurationInMilliseconds"].GetInteger())).TotalDays;
            IsRevocable = map["IsRevocable"].GetBoolean();
            IsRevoked = map["IsRevoked"].GetBoolean();
            IsActive = map["IsActive"].GetBoolean();
            UnlockTime = StartTime.AddDays(DurationInDays);
            Receivers = new List<LockReceiver>();

            var receiverArr = (Neo.VM.Types.Array)map["Receivers"];

            foreach (var receiverDataArr in receiverArr)
            {
                var receiverData = (Neo.VM.Types.Array)receiverDataArr;
                var receiver = new LockReceiver();
                receiver.ReceiverHash160 = Neo.Network.RPC.Utility.GetScriptHash(receiverData[0].FromByteStringToAccount(), protocolSettings);
                receiver.ReceiverAddress = receiver.ReceiverHash160.ToAddress(protocolSettings.AddressVersion);
                receiver.Amount = receiverData[1].GetInteger();
                var dateClaimed = receiverData[2].GetInteger();

                if (dateClaimed > 0)
                {
                    receiver.DateClaimed = dateClaimed.ToDateTimeOffsetFromMilliseconds();
                }

                var dateRevoked = receiverData[3].GetInteger();

                if (dateRevoked > 0)
                {
                    receiver.DateRevoked = dateClaimed.ToDateTimeOffsetFromMilliseconds();
                }

                receiver.IsActive = receiverData[4].GetBoolean();

                Receivers.Add(receiver);
            }
        }
    }
}
