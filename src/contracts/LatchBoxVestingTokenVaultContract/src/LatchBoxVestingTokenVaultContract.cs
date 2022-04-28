using System.ComponentModel;
using System.Numerics;

using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace LatchBoxVestingTokenVaultContract
{
    [DisplayName("mecvillarina.LatchBoxVestingTokenVaultContract")]
    [ManifestExtra("Author", "Mark Erwin Villarina")]
    [ContractPermission("*", "transfer", "burn", "balanceOf", "symbol")]
    public partial class LatchBoxVestingTokenVaultContract : SmartContract
    {
        private const byte Prefix_VestingIndex = 0x01;
        private const byte Prefix_Vestings = 0x02;
        private const byte Prefix_VestingInitiatorIndexes = 0x03;
        private const byte Prefix_VestingPeriodIndex = 0x04;
        private const byte Prefix_VestingPeriods = 0x05;
        private const byte Prefix_VestingReceivers = 0x06;
        private const byte Prefix_VestingReceiverPeriods = 0x07;
        private const byte Prefix_Refunds = 0x08;

        private static StorageMap Vestings => new StorageMap(Storage.CurrentContext, Prefix_Vestings);
        private static StorageMap VestingInitiatorIndexes => new StorageMap(Storage.CurrentContext, Prefix_VestingInitiatorIndexes);
        private static StorageMap VestingPeriods => new StorageMap(Storage.CurrentContext, Prefix_VestingPeriods);
        private static StorageMap VestingReceivers => new StorageMap(Storage.CurrentContext, Prefix_VestingReceivers);
        private static StorageMap VestingReceiverPeriods => new StorageMap(Storage.CurrentContext, Prefix_VestingReceiverPeriods);

        private static StorageMap Refunds => new StorageMap(Storage.CurrentContext, Prefix_Refunds);

        private static Transaction Tx => (Transaction)Runtime.ScriptContainer;

        public delegate void OnCreatedLatchBoxVestingDelegate(BigInteger vestingIdx);
        public delegate void OnClaimedLatchBoxVestingDelegate(BigInteger vestingIdx, BigInteger periodIdx, UInt160 receiverAddress, BigInteger claimedAmount);
        public delegate void OnRevokedLatchBoxVestingDelegate(BigInteger vestingIdx, BigInteger unlockedAmount);
        public delegate void OnClaimedRefundDelegate(UInt160 recipientAddress, UInt160 tokenScriptHash, BigInteger refundedAmount);

        [DisplayName("CreatedLatchBoxVesting")]
        public static event OnCreatedLatchBoxVestingDelegate OnCreatedLatchBoxVesting = default!;

        [DisplayName("ClaimedLatchBoxVesting")]
        public static event OnClaimedLatchBoxVestingDelegate OnClaimedLatchBoxVesting = default!;

        [DisplayName("RevokedLatchBoxVesting")]
        public static event OnRevokedLatchBoxVestingDelegate OnRevokedLatchBoxVesting = default!;

        [DisplayName("ClaimedRefund")]
        public static event OnClaimedRefundDelegate OnClaimedRefund = default!;
        
        public static void AddVesting(UInt160 tokenScriptHash, BigInteger totalAmount, bool isRevocable, LatchBoxVestingPeriodParameter[] periods)
        {
            ValidateNEP17Token(tokenScriptHash);
            if (totalAmount <= 0) ReportErrorAndThrow("The parameter total amount MUST be greater than 0.");
            ValidateVestingPeriodParameterData(periods, totalAmount);

            var tokenBalance = NEP17BalanceOf(tokenScriptHash, Tx.Sender);
            var tokenSymbol = NEP17Symbol(tokenScriptHash);
            if(tokenBalance < totalAmount) ReportErrorAndThrow($"Insufficient {tokenSymbol} balance. ");

            var paymentFee = GetPaymentTokenAddVestingFee();

            if(paymentFee > 0)
            {
                var paymentTokenScriptHash = GetPaymentTokenScriptHash();

                var paymentTokenBalance = NEP17BalanceOf(paymentTokenScriptHash, Tx.Sender);
                var paymentTokenSymbol = NEP17Symbol(paymentTokenScriptHash);

                if(paymentTokenBalance < (paymentFee + (tokenScriptHash == paymentTokenScriptHash ? totalAmount : 0))) ReportErrorAndThrow($"Insufficient {paymentTokenSymbol} balance. ");

                var burnAmount = (paymentFee * 50) / 100;
                var foundationAmount = (paymentFee * 5) / 100;
                var stakingAmount = (paymentFee * 40) / 100;
                var platformAmount = (paymentFee * 5) / 100;
                
                if (!NEP17Transfer(paymentTokenScriptHash, Tx.Sender, Runtime.ExecutingScriptHash, paymentFee)) ReportErrorAndThrow("NEP-17 Token Add LatchBox Vesting Payment Transfer Failed");

                NEP17Burn(paymentTokenScriptHash, Runtime.ExecutingScriptHash, burnAmount);
                if (!NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, FoundationAddress, foundationAmount)) ReportErrorAndThrow("NEP-17 Token Add LatchBox Vesting Payment Transfer Failed");
                if (!NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, StakingAddress, stakingAmount)) ReportErrorAndThrow("NEP-17 Token Add LatchBox Vesting Payment Transfer Failed");
                if (!NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, PlatformAddress, platformAmount)) ReportErrorAndThrow("NEP-17 Token Add LatchBox Vesting Payment Transfer Failed");

                UpdatePaymentBurnAmount(burnAmount);
            }

            if (!NEP17Transfer(tokenScriptHash, Tx.Sender, Runtime.ExecutingScriptHash, totalAmount)) ReportErrorAndThrow("NEP-17 Token Transfer Failed");

            var vestingIdx = GetVestingIndex();
            IncrementVestingIndex(1);
            var vesting = new LatchBoxVesting();
            vesting.TokenScriptHash = tokenScriptHash;
            vesting.InitiatorAddress = Tx.Sender;
            vesting.CreationTime = Runtime.Time;
            vesting.IsActive = true;
            vesting.IsRevocable = isRevocable;
            vesting.IsRevoked = false;

            SetVesting((ByteString)vestingIdx, vesting);
            UpdateVestingInitiatorIndexes(Tx.Sender, vestingIdx);

            foreach(var periodData in periods)
            {
                var periodIdx = GetVestingPeriodIndex();
                IncrementVestingPeriodIndex(1);
                
                LatchBoxVestingPeriod period = new LatchBoxVestingPeriod();
                period.PeriodIndex = periodIdx;
                period.Name = periodData.Name;
                period.TotalAmount = periodData.TotalAmount;
                period.UnlockTime = periodData.UnlockTime;

                AddVestingPeriod((ByteString)vestingIdx, period);
                foreach(var receiverData in periodData.Receivers)
                {
                    LatchBoxVestingReceiver receiver = new LatchBoxVestingReceiver();
                    receiver.Address = receiverData.Address;
                    receiver.Amount = receiverData.Amount;
                    receiver.DateClaimed = 0;
                    receiver.DateRevoked = 0;
                    receiver.Name = receiverData.Name;
                    receiver.PeriodIndex = periodIdx;
                    UpdateVestingReceiverPeriods(receiverData.Address, vestingIdx, periodIdx);
                    AddVestingReceivers((ByteString) periodIdx, receiver);
                }

            }

            OnCreatedLatchBoxVesting(vestingIdx);
        }

        public void ClaimVesting(BigInteger vestingIdx, BigInteger periodIdx)
        {
            var transaction = GetVestingTransactionOrThrow(vestingIdx);

            int periodIndex = GetPeriodIndexByPeriodIdx(transaction.Periods, periodIdx);
            if (periodIndex == -1) ReportErrorAndThrow("No authorization.");
            
            int receiverIndex = GetReceiverIndexByPeriodIdxAndAddress(transaction.Receivers, periodIdx, Tx.Sender);
            if (receiverIndex == -1) ReportErrorAndThrow("No authorization.");

            var receiver = transaction.Receivers[receiverIndex];
            var period = transaction.Periods[periodIndex];

            if (transaction.Vesting.IsRevoked || (receiver.DateRevoked > 0)) ReportErrorAndThrow("LatchBox Vesting has been already revoked by the initiator.");

            if (!transaction.Vesting.IsActive) ReportErrorAndThrow("LatchBox Vesting is not active anymore.");

            if (receiver.DateClaimed > 0) ReportErrorAndThrow("LatchBox Vesting has been claimed.");

            if (Runtime.Time < period.UnlockTime) ReportErrorAndThrow("LatchBox Vesting selected period is not yet ready to be claimed.");

            var paymentFee = GetPaymentTokenClaimVestingFee();

            if(paymentFee > 0)
            {
                var paymentTokenScriptHash = GetPaymentTokenScriptHash();
                
                var paymentTokenBalance = NEP17BalanceOf(paymentTokenScriptHash, Tx.Sender);
                var paymentTokenSymbol = NEP17Symbol(paymentTokenScriptHash);
                if(paymentTokenBalance < paymentFee) ReportErrorAndThrow($"Insufficient {paymentTokenSymbol} balance. ");

                var burnAmount = (paymentFee * 50) / 100;
                var foundationAmount = (paymentFee * 5) / 100;
                var stakingAmount = (paymentFee * 40) / 100;
                var platformAmount = (paymentFee * 5) / 100;

                if (!NEP17Transfer(paymentTokenScriptHash, Tx.Sender, Runtime.ExecutingScriptHash, paymentFee)) ReportErrorAndThrow("NEP-17 Token Claim LatchBox Vesting Payment Transfer Failed");

                NEP17Burn(paymentTokenScriptHash, Runtime.ExecutingScriptHash, burnAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, FoundationAddress, foundationAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, StakingAddress, stakingAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, PlatformAddress, platformAmount);

                UpdatePaymentBurnAmount(burnAmount);
            }

            if (!NEP17Transfer(transaction.Vesting.TokenScriptHash, Runtime.ExecutingScriptHash, receiver.Address, receiver.Amount)) ReportErrorAndThrow("NEP-17 Token Transfer Failed");

            receiver.DateClaimed = Runtime.Time;

            UpdateVestingReceiversForAllPeriods(transaction.Periods, transaction.Receivers);

            bool isAllClaimed = IsAllReceiversClaimed(transaction.Receivers);
            if (isAllClaimed)
            {
                transaction.Vesting.IsActive = false;
                SetVesting((ByteString)vestingIdx, transaction.Vesting);
            }

            OnClaimedLatchBoxVesting(vestingIdx, periodIdx, receiver.Address, receiver.Amount);
        }

        public static void RevokeVesting(BigInteger vestingIdx)
        {
            var transaction = GetVestingTransactionOrThrow(vestingIdx);

            if (!Runtime.CheckWitness(transaction.Vesting.InitiatorAddress)) ReportErrorAndThrow("No authorization.");

            if (transaction.Vesting.IsRevoked) ReportErrorAndThrow("LatchBox Vesting has been already revoked by the initiator.");

            if (!transaction.Vesting.IsActive) ReportErrorAndThrow("LatchBox Vesting is not active anymore.");

            if (!transaction.Vesting.IsRevocable) ReportErrorAndThrow("LatchBox Vesting is irrevocable.");

            var paymentFee = GetPaymentTokenRevokeVestingFee();

            if(paymentFee > 0)
            {
                var paymentTokenScriptHash = GetPaymentTokenScriptHash();
                
                var paymentTokenBalance = NEP17BalanceOf(paymentTokenScriptHash, Tx.Sender);
                var paymentTokenSymbol = NEP17Symbol(paymentTokenScriptHash);
                if(paymentTokenBalance < paymentFee) ReportErrorAndThrow($"Insufficient {paymentTokenSymbol} balance. ");

                var burnAmount = (paymentFee * 50) / 100;
                var foundationAmount = (paymentFee * 5) / 100;
                var stakingAmount = (paymentFee * 40) / 100;
                var platformAmount = (paymentFee * 5) / 100;

                if (!NEP17Transfer(paymentTokenScriptHash, Tx.Sender, Runtime.ExecutingScriptHash, paymentFee)) ReportErrorAndThrow("NEP-17 Token Revoke LatchBox Vesting Payment Transfer Failed");

                NEP17Burn(paymentTokenScriptHash, Runtime.ExecutingScriptHash, burnAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, FoundationAddress, foundationAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, StakingAddress, stakingAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, PlatformAddress, platformAmount);

                UpdatePaymentBurnAmount(burnAmount);
            }

            transaction.Vesting.IsRevoked = true;
            transaction.Vesting.IsActive = false;

            SetVesting((ByteString)vestingIdx, transaction.Vesting);

            BigInteger totalRefundAmount = 0;
            foreach (var receiver in transaction.Receivers)
            {
                if (receiver.DateClaimed == 0 && receiver.DateRevoked == 0)
                {
                    receiver.DateRevoked = Runtime.Time;
                    totalRefundAmount += receiver.Amount;
                    UpdateRefunds(transaction.Vesting.InitiatorAddress, transaction.Vesting.TokenScriptHash, receiver.Amount);
                }
            }

            UpdateVestingReceiversForAllPeriods(transaction.Periods, transaction.Receivers);

            OnRevokedLatchBoxVesting(vestingIdx, totalRefundAmount);
        }

        public static void ClaimRefund(UInt160 tokenScriptHash)
        {
            ValidateNEP17Token(tokenScriptHash);

            var refund = GetRefundOrThrow(Tx.Sender, tokenScriptHash);

            if (!NEP17Transfer(tokenScriptHash, Runtime.ExecutingScriptHash, Tx.Sender, refund.Amount)) ReportErrorAndThrow("NEP-17 Token Transfer Failed");

            RemoveRefund(Tx.Sender, tokenScriptHash);
            OnClaimedRefund(Tx.Sender, tokenScriptHash, refund.Amount);
        }

        [Safe]
        public static BigInteger GetVestingsCount() => GetVestingIndex();

        [Safe]
        public static Map<ByteString, object> GetVestingTransaction(BigInteger vestingIdx)
        {
            var transaction = GetVestingTransactionOrThrow(vestingIdx);
            var map = VestingTransactionToMap(transaction);
            return map;
        }

        [Safe]
        public static Map<ByteString, object>[] GetVestingsByInitiator(UInt160 initiatorAddress)
        {
            ValidateAddress(initiatorAddress);
            List<Map<ByteString, object>> maps = new();

            var value = VestingInitiatorIndexes[initiatorAddress];

            if (value != null)
            {
                var indexes = (List<BigInteger>)(StdLib.Deserialize(value));

                foreach (var index in indexes)
                {
                    var transaction = GetVestingTransactionOrThrow(index);
                    var map = VestingTransactionToMap(transaction);
                    maps.Add(map);
                }
            }

            return maps;
        }

        [Safe]
        public static Map<ByteString, Map<ByteString, object>> GetVestingsByReceiver(UInt160 receiverAddress)
        {
            ValidateAddress(receiverAddress);
            Map<ByteString, Map<ByteString, object>> maps = new();

            var value = VestingReceiverPeriods[receiverAddress];

            if (value != null)
            {
                var data = (List<LatchBoxVestingReceiverPeriod>)(StdLib.Deserialize(value));

                foreach (var row in data)
                {
                    var transaction = GetVestingTransactionOrThrow(row.VestingIdx);
                    var map = VestingTransactionToMap(transaction);
                    maps[(ByteString)row.VestingIdx] = map;
                }
            }

            return maps;
        }

        [Safe]
        public static Map<ByteString, BigInteger> GetRefunds()
        {
            var value = Refunds[Tx.Sender];
            Map<ByteString, BigInteger> map = new();
            if (value != null)
            {
                var refunds = (List<LatchBoxRefund>)(StdLib.Deserialize(value));

                foreach (var refund in refunds)
                {
                    map[refund.TokenScriptHash] = refund.Amount;
                }
            }
            return map;
        }


        public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
        {
            //do nothing
        }

       
        private static Map<ByteString, object> VestingTransactionToMap(LatchBoxVestingTransaction transaction)
        {
            Map<ByteString, object> map = new();
            map["Idx"] = transaction.Idx;
            map["CreationTime"] = transaction.Vesting.CreationTime;
            map["InitiatorAddress"] = transaction.Vesting.InitiatorAddress;
            map["IsActive"] = transaction.Vesting.IsActive;
            map["IsRevocable"] = transaction.Vesting.IsRevocable;
            map["IsRevoked"] = transaction.Vesting.IsRevoked;
            map["TokenScriptHash"] = transaction.Vesting.TokenScriptHash;
            map["Periods"] = transaction.Periods;
            map["Receivers"] = transaction.Receivers;

            return map;
        }

        private static LatchBoxVestingTransaction GetVestingTransactionOrThrow(BigInteger vestingIdx)
        {
            var vestingObj = Vestings[(ByteString)vestingIdx];
            if (vestingObj == null) ReportErrorAndThrow("LatchBox Vesting doesn't exists.");

            var vesting = (LatchBoxVesting)StdLib.Deserialize(vestingObj);

            var periods = (List<LatchBoxVestingPeriod>)(StdLib.Deserialize(VestingPeriods[(ByteString)vestingIdx]));
            var receivers = new List<LatchBoxVestingReceiver>();
            
            foreach(var period in periods)
            {
                var periodReceivers = (List<LatchBoxVestingReceiver>)(StdLib.Deserialize(VestingReceivers[(ByteString)period.PeriodIndex]));
                
                foreach(var periodReceiver in periodReceivers)
                {
                    receivers.Add(periodReceiver);
                }
            }
            return new LatchBoxVestingTransaction() { Idx = vestingIdx, Vesting = vesting, Periods = periods, Receivers = receivers };
        }

        private static void AddVestingReceivers(ByteString periodIdx, LatchBoxVestingReceiver receiverObj)
        {
            var value = VestingReceivers[periodIdx];
            List<LatchBoxVestingReceiver> receivers = new List<LatchBoxVestingReceiver>();
            if (value != null)
                receivers = (List<LatchBoxVestingReceiver>)(StdLib.Deserialize(value));

            receivers.Add(receiverObj);
            VestingReceivers[periodIdx] = StdLib.Serialize(receivers);
        }
        
        private static void UpdateVestingReceiversForAllPeriods(List<LatchBoxVestingPeriod> periods, List<LatchBoxVestingReceiver> receivers)
        {
              foreach(var period in periods)
            {
                var newReceivers = new List<LatchBoxVestingReceiver>();

                foreach(var receiver in receivers)
                {
                    if(receiver.PeriodIndex == period.PeriodIndex)
                    {
                        newReceivers.Add(receiver);
                    }
                }

                UpdateVestingReceivers((ByteString)period.PeriodIndex, newReceivers);
            }
        }

        private static void UpdateVestingReceivers(ByteString periodIdx, List<LatchBoxVestingReceiver> newReceivers)
        {
            VestingReceivers[periodIdx] = StdLib.Serialize(newReceivers);
        }

        private static void AddVestingPeriod(ByteString vestingIdx, LatchBoxVestingPeriod periodObj)
        {
            var value = VestingPeriods[vestingIdx];
            List<LatchBoxVestingPeriod> periods = new List<LatchBoxVestingPeriod>();
            if (value != null)
                periods = (List<LatchBoxVestingPeriod>)(StdLib.Deserialize(value));

            periods.Add(periodObj);
            VestingPeriods[vestingIdx] = StdLib.Serialize(periods);
        }

        private static void UpdateVestingInitiatorIndexes(ByteString initiator, BigInteger vestingIdx)
        {
            var value = VestingInitiatorIndexes[initiator];
            List<BigInteger> indexes = new List<BigInteger>();
            if (value != null)
                indexes = (List<BigInteger>)(StdLib.Deserialize(value));

            indexes.Add(vestingIdx);
            VestingInitiatorIndexes[initiator] = StdLib.Serialize(indexes);
        }

        private static void UpdateVestingReceiverPeriods(ByteString receiver, BigInteger vestingIdx, BigInteger periodIdx)
        {
            var value = VestingReceiverPeriods[receiver];
            List<LatchBoxVestingReceiverPeriod> data = new List<LatchBoxVestingReceiverPeriod>();
            if (value != null)
                data = (List<LatchBoxVestingReceiverPeriod>)(StdLib.Deserialize(value));

            data.Add(new LatchBoxVestingReceiverPeriod()
            {
                VestingIdx = vestingIdx,
                PeriodIdx = vestingIdx
            });
            VestingReceiverPeriods[receiver] = StdLib.Serialize(data);
        }

        private static bool IsAllReceiversClaimed(List<LatchBoxVestingReceiver> receivers)
        {
            foreach (var receiver in receivers)
            {
                if (receiver.DateClaimed == 0)
                {
                    return false;
                }
            }

            return true;
        }
        private static int GetPeriodIndexByPeriodIdx(List<LatchBoxVestingPeriod> periods, BigInteger periodIdx)
        {
            int receiverIndex = -1;
            for (int i = 0; i < periods.Count; i++)
            {
                if (periods[i].PeriodIndex == periodIdx)
                {
                    receiverIndex = i;
                    break;
                }
            }

            return receiverIndex;
        }

        private static int GetReceiverIndexByPeriodIdxAndAddress(List<LatchBoxVestingReceiver> receivers, BigInteger periodIdx, UInt160 receiverAddress)
        {
            int receiverIndex = -1;
            for (int i = 0; i < receivers.Count; i++)
            {
                if (receivers[i].Address == receiverAddress && receivers[i].PeriodIndex == periodIdx)
                {
                    receiverIndex = i;
                    break;
                }
            }

            return receiverIndex;
        }

        private static void SetVesting(ByteString vestingIdx, LatchBoxVesting vesting)
        {
            Vestings[vestingIdx] = StdLib.Serialize(vesting);
        }

        private static BigInteger GetVestingIndex()
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_VestingIndex };
            return (BigInteger)Storage.Get(context, key);
        }

        private static BigInteger GetVestingPeriodIndex()
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_VestingPeriodIndex };
            return (BigInteger)Storage.Get(context, key);
        }

        private static void IncrementVestingIndex(int increment)
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_VestingIndex };
            var idx = (BigInteger)Storage.Get(context, key);
            idx = idx + increment;
            Storage.Put(context, key, idx);
        }

        private static void IncrementVestingPeriodIndex(int increment)
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_VestingPeriodIndex };
            var idx = (BigInteger)Storage.Get(context, key);
            idx = idx + increment;
            Storage.Put(context, key, idx);
        }

        
        private static LatchBoxRefund GetRefundOrThrow(ByteString initiator, UInt160 tokenScriptHash)
        {
            var value = Refunds[initiator];

            if (value != null)
            {
                var refunds = (List<LatchBoxRefund>)(StdLib.Deserialize(value));

                foreach (var refund in refunds)
                {
                    if (refund.TokenScriptHash == tokenScriptHash)
                    {
                        return refund;
                    }
                }
            }

            ReportErrorAndThrow("No refund found.");
            return null;
        }

        private static void UpdateRefunds(ByteString initiator, UInt160 token, BigInteger amount)
        {
            var value = Refunds[initiator];
            List<LatchBoxRefund> refunds = new List<LatchBoxRefund>();
            if (value != null)
                refunds = (List<LatchBoxRefund>)(StdLib.Deserialize(value));

            LatchBoxRefund assetRefund = null;

            foreach (var refund in refunds)
            {
                if (refund.TokenScriptHash == token)
                {
                    assetRefund = refund;
                    break;
                }
            }

            if (assetRefund != null)
            {
                assetRefund.Amount += amount;
            }
            else
            {
                var refund = new LatchBoxRefund();
                refund.TokenScriptHash = token;
                refund.Amount = amount;
                refunds.Add(refund);
            }

            Refunds[initiator] = StdLib.Serialize(refunds);
        }

        private static void RemoveRefund(ByteString initiator, UInt160 token)
        {
            var value = Refunds[initiator];
            List<LatchBoxRefund> refunds = new List<LatchBoxRefund>();
            if (value != null)
                refunds = (List<LatchBoxRefund>)(StdLib.Deserialize(value));

            if (refunds.Count > 0)
            {
                int refundIdx = -1;
                for (int i = 0; i < refunds.Count; i++)
                {
                    if (refunds[i].TokenScriptHash == token)
                    {
                        refundIdx = i;
                        break;
                    }
                }

                if (refundIdx != -1)
                    refunds.RemoveAt(refundIdx);

                if (refunds.Count > 0)
                {
                    Refunds[initiator] = StdLib.Serialize(refunds);
                }
                else
                {
                    Refunds.Delete(initiator);
                }

            }
        }
        
        private static bool NEP17Transfer(UInt160 scriptHash, UInt160 from, UInt160 to, BigInteger amount, object data = null)
        {
            return (bool)Contract.Call(scriptHash, "transfer", CallFlags.All, from, to, amount, data);
        }

        private static void NEP17Burn(UInt160 scriptHash, UInt160 from, BigInteger amount, object data = null)
        {
            Contract.Call(scriptHash, "burn", CallFlags.All, from, amount, data);
        }

        private static BigInteger NEP17BalanceOf(UInt160 scriptHash, UInt160 account)
        {
           return (BigInteger) Contract.Call(scriptHash, "balanceOf", CallFlags.All, account);
        }

        private static string NEP17Symbol(UInt160 scriptHash)
        {
           return (string) Contract.Call(scriptHash, "symbol", CallFlags.All);
        }
    }
}
