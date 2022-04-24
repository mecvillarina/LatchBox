using Client.Infrastructure.Extensions;
using Neo;
using Neo.VM.Types;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Client.Infrastructure.Models
{
    public class VestingTransaction
    {
        public BigInteger VestingIndex { get; private set; }
        public UInt160 TokenScriptHash { get; private set; }
        public UInt160 InitiatorHash160 { get; private set; }
        public string InitiatorAddress { get; private set; }
        public DateTimeOffset CreationTime { get; private set; }
        public bool IsRevocable { get; private set; }
        public bool IsRevoked { get; private set; }
        public bool IsActive { get; private set; }

        public List<VestingPeriod> Periods { get; private set; } = new();
        public List<VestingReceiver> Receivers { get; private set; } = new();

        public VestingTransaction(Map map, ProtocolSettings protocolSettings)
        {
            VestingIndex = map["Idx"].GetInteger();
            TokenScriptHash = Neo.Network.RPC.Utility.GetScriptHash(map["TokenScriptHash"].FromByteStringToAccount(), protocolSettings);
            InitiatorHash160 = Neo.Network.RPC.Utility.GetScriptHash(map["InitiatorAddress"].FromByteStringToAccount(), protocolSettings);
            InitiatorAddress = InitiatorHash160.ToAddress(protocolSettings.AddressVersion);
            CreationTime = map["CreationTime"].GetInteger().ToDateTimeOffsetFromMilliseconds();
            IsRevocable = map["IsRevocable"].GetBoolean();
            IsRevoked = map["IsRevoked"].GetBoolean();
            IsActive = map["IsActive"].GetBoolean();

            var periodArr = (Neo.VM.Types.Array)map["Periods"];

            foreach(var periodDataArr in periodArr)
            {
                var periodData = (Neo.VM.Types.Array)periodDataArr;
                var period = new VestingPeriod();
                period.PeriodIndex = periodData[0].GetInteger();
                period.Name = periodData[1].GetString();
                period.TotalAmount = periodData[2].GetInteger();
                period.UnlockTime = periodData[3].GetInteger().ToDateTimeOffsetFromMilliseconds();

                Periods.Add(period);
            }

            var receiverArr = (Neo.VM.Types.Array)map["Receivers"];

            foreach (var receiverDataArr in receiverArr)
            {
                var receiverData = (Neo.VM.Types.Array)receiverDataArr;
                var receiver = new VestingReceiver();
                receiver.PeriodIndex = receiverData[0].GetInteger();
                receiver.Name = receiverData[1].GetString();
                receiver.ReceiverHash160 = Neo.Network.RPC.Utility.GetScriptHash(receiverData[2].FromByteStringToAccount(), protocolSettings);
                receiver.ReceiverAddress = receiver.ReceiverHash160.ToAddress(protocolSettings.AddressVersion);
                receiver.Amount = receiverData[3].GetInteger();

                var dateClaimed = receiverData[4].GetInteger();

                if (dateClaimed > 0)
                {
                    receiver.DateClaimed = dateClaimed.ToDateTimeOffsetFromMilliseconds();
                }

                var dateRevoked = receiverData[5].GetInteger();

                if (dateRevoked > 0)
                {
                    receiver.DateRevoked = dateClaimed.ToDateTimeOffsetFromMilliseconds();
                }

                Receivers.Add(receiver);
            }
        }
    }
}
