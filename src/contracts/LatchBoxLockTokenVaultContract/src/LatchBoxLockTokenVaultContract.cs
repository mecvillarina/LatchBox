using System.ComponentModel;
using System.Numerics;

using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace LatchBoxLockTokenVaultContract
{
    [DisplayName("mecvillarina.LatchBoxLockTokenVaultContract")]
    [ManifestExtra("Author", "Mark Erwin Villarina")]
    [ContractPermission("*", "transfer", "burn", "balanceOf", "symbol")]
    public partial class LatchBoxLockTokenVaultContract : SmartContract
    {
        private const byte Prefix_LockIndex = 0x01;
        private const byte Prefix_Locks = 0x02;
        private const byte Prefix_LockReceivers = 0x03;
        private const byte Prefix_LockInitiatorIndexes = 0x04;
        private const byte Prefix_LockReceiverIndexes = 0x05;
        private const byte Prefix_LockAssetIndexes = 0x06;
        private const byte Prefix_Refunds = 0x07;
        private const byte Prefix_AssetCounter = 0x08;

        private static StorageMap Locks => new StorageMap(Storage.CurrentContext, Prefix_Locks);
        private static StorageMap LockReceivers => new StorageMap(Storage.CurrentContext, Prefix_LockReceivers);
        private static StorageMap LockInitiatorIndexes => new StorageMap(Storage.CurrentContext, Prefix_LockInitiatorIndexes);
        private static StorageMap LockLockReceiverIndexes => new StorageMap(Storage.CurrentContext, Prefix_LockReceiverIndexes);
        private static StorageMap LockAssetIndexes => new StorageMap(Storage.CurrentContext, Prefix_LockAssetIndexes);
        private static StorageMap Refunds => new StorageMap(Storage.CurrentContext, Prefix_Refunds);
        private static StorageMap AssetCounter => new StorageMap(Storage.CurrentContext, Prefix_AssetCounter);
        private static Transaction Tx => (Transaction)Runtime.ScriptContainer;

        public delegate void OnCreatedLatchBoxLockDelegate(BigInteger lockIdx);
        public delegate void OnClaimedLatchBoxLockDelegate(BigInteger lockIdx, UInt160 receiverAddress, BigInteger claimedAmount);
        public delegate void OnRevokedLatchBoxLockDelegate(BigInteger lockIdx, BigInteger unlockedAmount);
        public delegate void OnClaimedRefundDelegate(UInt160 recipientAddress, UInt160 tokenScriptHash, BigInteger refundedAmount);

        [DisplayName("CreatedLatchBoxLock")]
        public static event OnCreatedLatchBoxLockDelegate OnCreatedLatchBoxLock = default!;

        [DisplayName("ClaimedLatchBoxLock")]
        public static event OnClaimedLatchBoxLockDelegate OnClaimedLatchBoxLock = default!;

        [DisplayName("RevokedLatchBoxLock")]
        public static event OnRevokedLatchBoxLockDelegate OnRevokedLatchBoxLock = default!;

        [DisplayName("ClaimedRefund")]
        public static event OnClaimedRefundDelegate OnClaimedRefund = default!;

        public static void AddLock(UInt160 tokenScriptHash, BigInteger totalAmount, BigInteger unlockTime, LatchBoxLockReceiverParameter[] receivers, bool isRevocable)
        {
            ValidateNEP17Token(tokenScriptHash);
            if (totalAmount <= 0) ReportErrorAndThrow("The parameter total amount MUST be greater than 0.");
            if ((unlockTime - Runtime.Time) < 86400000) ReportErrorAndThrow("The parameter unlockTime MUST be at least 1 day.");
            ValidateLockReceiverParameterData(receivers, totalAmount);

            var tokenBalance = NEP17BalanceOf(tokenScriptHash, Tx.Sender);
            var tokenSymbol = NEP17Symbol(tokenScriptHash);
            if(tokenBalance < totalAmount) ReportErrorAndThrow($"Insufficient {tokenSymbol} balance. ");

            var paymentFee = GetPaymentTokenAddLockFee();

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
                
                if (!NEP17Transfer(paymentTokenScriptHash, Tx.Sender, Runtime.ExecutingScriptHash, paymentFee)) ReportErrorAndThrow("NEP-17 Token Add LatchBox Lock Payment Transfer Failed");

                NEP17Burn(paymentTokenScriptHash, Runtime.ExecutingScriptHash, burnAmount);
                if (!NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, FoundationAddress, foundationAmount)) ReportErrorAndThrow("NEP-17 Token Add LatchBox Lock Payment Transfer Failed");
                if (!NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, StakingAddress, stakingAmount)) ReportErrorAndThrow("NEP-17 Token Add LatchBox Lock Payment Transfer Failed");
                if (!NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, PlatformAddress, platformAmount)) ReportErrorAndThrow("NEP-17 Token Add LatchBox Lock Payment Transfer Failed");

                UpdatePaymentBurnAmount(burnAmount);
            }

            if (!NEP17Transfer(tokenScriptHash, Tx.Sender, Runtime.ExecutingScriptHash, totalAmount)) ReportErrorAndThrow("NEP-17 Token Transfer Failed");

            var lockIdx = GetLockIndex();
            IncrementLockIndex(1);
            var latchBoxLock = new LatchBoxLock();
            latchBoxLock.TokenScriptHash = tokenScriptHash;
            latchBoxLock.InitiatorAddress = Tx.Sender;
            latchBoxLock.CreationTime = Runtime.Time;
            latchBoxLock.StartTime = Runtime.Time;
            latchBoxLock.UnlockTime = unlockTime;
            latchBoxLock.IsActive = true;
            latchBoxLock.IsRevocable = isRevocable;
            latchBoxLock.IsRevoked = false;

            SetLock((ByteString)lockIdx, latchBoxLock);
            UpdateLockInitiatorIndexes(Tx.Sender, lockIdx);

            foreach (var receiverData in receivers)
            {
                LatchBoxLockReceiver receiver = new LatchBoxLockReceiver();
                receiver.ReceiverAddress = receiverData.ReceiverAddress;
                receiver.Amount = receiverData.Amount;
                receiver.DateClaimed = 0;
                receiver.DateRevoked = 0;
                receiver.IsActive = true;

                UpdateLockReceiverIndexes(receiverData.ReceiverAddress, lockIdx);
                AddLockReceivers((ByteString)lockIdx, receiver);
            }

            UpdateLockAssetIndexes(tokenScriptHash, lockIdx);
            IncrementAssetLockedCounter(tokenScriptHash, totalAmount);

            OnCreatedLatchBoxLock(lockIdx);
        }

        public void ClaimLock(BigInteger lockIdx)
        {
            var transaction = GetLockTransactionOrThrow(lockIdx);

            int receiverIndex = GetReceiverIndexByAddress(transaction.Receivers, Tx.Sender);
            if (receiverIndex == -1) ReportErrorAndThrow("No authorization.");

            var receiver = transaction.Receivers[receiverIndex];

            if (transaction.Lock.IsRevoked || (!receiver.IsActive && receiver.DateRevoked > 0)) ReportErrorAndThrow("LatchBox Lock has been already revoked by the initiator.");

            if (!transaction.Lock.IsActive) ReportErrorAndThrow("LatchBox Lock is not active anymore.");

            if (!receiver.IsActive && receiver.DateClaimed > 0) ReportErrorAndThrow("LatchBox Lock has been claimed.");

            if (Runtime.Time < transaction.Lock.UnlockTime) ReportErrorAndThrow("LatchBox Lock is not yet ready to be claimed.");

            var paymentFee = GetPaymentTokenClaimLockFee();

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

                if (!NEP17Transfer(paymentTokenScriptHash, Tx.Sender, Runtime.ExecutingScriptHash, paymentFee)) ReportErrorAndThrow("NEP-17 Token Claim LatchBox Lock Payment Transfer Failed");

                NEP17Burn(paymentTokenScriptHash, Runtime.ExecutingScriptHash, burnAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, FoundationAddress, foundationAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, StakingAddress, stakingAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, PlatformAddress, platformAmount);

                UpdatePaymentBurnAmount(burnAmount);
            }

            if (!NEP17Transfer(transaction.Lock.TokenScriptHash, Runtime.ExecutingScriptHash, receiver.ReceiverAddress, receiver.Amount)) ReportErrorAndThrow("NEP-17 Token Transfer Failed");

            receiver.DateClaimed = Runtime.Time;
            receiver.IsActive = false;

            UpdateLockReceivers((ByteString)lockIdx, transaction.Receivers);
            IncrementAssetUnlockedCounter(transaction.Lock.TokenScriptHash, receiver.Amount);

            bool isAllClaimed = IsAllReceiversClaimed(transaction.Receivers);
            if (isAllClaimed)
            {
                transaction.Lock.IsActive = false;
                SetLock((ByteString)lockIdx, transaction.Lock);
            }

            OnClaimedLatchBoxLock(lockIdx, receiver.ReceiverAddress, receiver.Amount);
        }

        public static void RevokeLock(BigInteger lockIdx)
        {
            var transaction = GetLockTransactionOrThrow(lockIdx);

            if (!Runtime.CheckWitness(transaction.Lock.InitiatorAddress)) ReportErrorAndThrow("No authorization.");

            if (transaction.Lock.IsRevoked) ReportErrorAndThrow("LatchBox Lock has been already revoked by the initiator.");

            if (!transaction.Lock.IsActive) ReportErrorAndThrow("LatchBox Lock is not active anymore.");

            if (!transaction.Lock.IsRevocable) ReportErrorAndThrow("LatchBox Lock is irrevocable.");

            var paymentFee = GetPaymentTokenRevokeLockFee();

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

                if (!NEP17Transfer(paymentTokenScriptHash, Tx.Sender, Runtime.ExecutingScriptHash, paymentFee)) ReportErrorAndThrow("NEP-17 Token Revoke LatchBox Lock Payment Transfer Failed");

                NEP17Burn(paymentTokenScriptHash, Runtime.ExecutingScriptHash, burnAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, FoundationAddress, foundationAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, StakingAddress, stakingAmount);
                NEP17Transfer(paymentTokenScriptHash, Runtime.ExecutingScriptHash, PlatformAddress, platformAmount);

                UpdatePaymentBurnAmount(burnAmount);
            }

            transaction.Lock.IsRevoked = true;
            transaction.Lock.IsActive = false;

            SetLock((ByteString)lockIdx, transaction.Lock);

            BigInteger totalRefundAmount = 0;
            foreach (var receiver in transaction.Receivers)
            {
                if (receiver.DateClaimed == 0 && receiver.DateRevoked == 0)
                {
                    receiver.DateRevoked = Runtime.Time;
                    receiver.IsActive = false;
                    totalRefundAmount += receiver.Amount;
                    UpdateRefunds(transaction.Lock.InitiatorAddress, transaction.Lock.TokenScriptHash, receiver.Amount);
                }
            }

            UpdateLockReceivers((ByteString)lockIdx, transaction.Receivers);

            OnRevokedLatchBoxLock(lockIdx, totalRefundAmount);
        }

        public static void ClaimRefund(UInt160 tokenScriptHash)
        {
            ValidateNEP17Token(tokenScriptHash);

            var refund = GetRefundOrThrow(Tx.Sender, tokenScriptHash);

            if (!NEP17Transfer(tokenScriptHash, Runtime.ExecutingScriptHash, Tx.Sender, refund.Amount)) ReportErrorAndThrow("NEP-17 Token Transfer Failed");

            RemoveRefund(Tx.Sender, tokenScriptHash);
            IncrementAssetUnlockedCounter(tokenScriptHash, refund.Amount);
            OnClaimedRefund(Tx.Sender, tokenScriptHash, refund.Amount);
        }

        [Safe]
        public static BigInteger GetLocksCount() => GetLockIndex();

        [Safe]
        public static Map<ByteString, object> GetLockTransaction(BigInteger lockIdx)
        {
            var transaction = GetLockTransactionOrThrow(lockIdx);
            var map = LockTransactionToMap(transaction);
            return map;
        }

        [Safe]
        public static Map<ByteString, object>[] GetLocksByInitiator(UInt160 initiatorAddress)
        {
            ValidateAddress(initiatorAddress);
            List<Map<ByteString, object>> maps = new();

            var value = LockInitiatorIndexes[initiatorAddress];

            if (value != null)
            {
                var indexes = (List<BigInteger>)(StdLib.Deserialize(value));

                foreach (var index in indexes)
                {
                    var transaction = GetLockTransactionOrThrow(index);
                    var map = LockTransactionToMap(transaction);
                    maps.Add(map);
                }
            }

            return maps;
        }

        [Safe]
        public static Map<ByteString, object>[] GetLocksByReceiver(UInt160 receiverAddress)
        {
            ValidateAddress(receiverAddress);
            List<Map<ByteString, object>> maps = new();

            var value = LockLockReceiverIndexes[receiverAddress];

            if (value != null)
            {
                var indexes = (List<BigInteger>)(StdLib.Deserialize(value));

                foreach (var index in indexes)
                {
                    var transaction = GetLockTransactionOrThrow(index);
                    var map = LockTransactionToMap(transaction);
                    maps.Add(map);
                }
            }

            return maps;
        }

        [Safe]
        public static Map<ByteString, object>[] GetLocksByAsset(UInt160 tokenScriptHash)
        {
            ValidateNEP17Token(tokenScriptHash);
            List<Map<ByteString, object>> maps = new();

            var value = LockAssetIndexes[tokenScriptHash];

            if (value != null)
            {
                var indexes = (List<BigInteger>)(StdLib.Deserialize(value));

                foreach (var index in indexes)
                {
                    var transaction = GetLockTransactionOrThrow(index);
                    var map = LockTransactionToMap(transaction);
                    maps.Add(map);
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

        [Safe]
        public static Map<ByteString, object> GetAssetsCounter()
        {
            var iterator = AssetCounter.Find(FindOptions.DeserializeValues | FindOptions.RemovePrefix);
            Map<ByteString, object> map = new();

            while (iterator.Next())
            {
                var kvp = (object[])iterator.Value;
                var key = (ByteString)kvp[0];
                map[key] = (LatchBoxAssetCounter)kvp[1];
            }

            return map;
        }

        public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
        {
            //do nothing
        }

        private static Map<ByteString, object> LockTransactionToMap(LatchBoxLockTransaction transaction)
        {
            Map<ByteString, object> map = new();
            map["Idx"] = transaction.Idx;
            map["CreationTime"] = transaction.Lock.CreationTime;
            map["UnlockTime"] = transaction.Lock.UnlockTime;
            map["InitiatorAddress"] = transaction.Lock.InitiatorAddress;
            map["IsActive"] = transaction.Lock.IsActive;
            map["IsRevocable"] = transaction.Lock.IsRevocable;
            map["IsRevoked"] = transaction.Lock.IsRevoked;
            map["StartTime"] = transaction.Lock.StartTime;
            map["TokenScriptHash"] = transaction.Lock.TokenScriptHash;
            map["Receivers"] = transaction.Receivers;

            return map;
        }

        private static LatchBoxLockTransaction GetLockTransactionOrThrow(BigInteger lockIdx)
        {
            var lockObj = Locks[(ByteString)lockIdx];
            if (lockObj == null) ReportErrorAndThrow("LatchBox Lock doesn't exists.");

            var @lock = (LatchBoxLock)StdLib.Deserialize(lockObj);

            var receivers = (List<LatchBoxLockReceiver>)(StdLib.Deserialize(LockReceivers[(ByteString)lockIdx]));
            return new LatchBoxLockTransaction() { Idx = lockIdx, Lock = @lock, Receivers = receivers };
        }

        private static void UpdateLockInitiatorIndexes(ByteString initiator, BigInteger lockIdx)
        {
            var value = LockInitiatorIndexes[initiator];
            List<BigInteger> indexes = new List<BigInteger>();
            if (value != null)
                indexes = (List<BigInteger>)(StdLib.Deserialize(value));

            indexes.Add(lockIdx);
            LockInitiatorIndexes[initiator] = StdLib.Serialize(indexes);
        }

        private static void UpdateLockReceiverIndexes(ByteString receiver, BigInteger lockIdx)
        {
            var value = LockLockReceiverIndexes[receiver];
            List<BigInteger> indexes = new List<BigInteger>();
            if (value != null)
                indexes = (List<BigInteger>)(StdLib.Deserialize(value));

            indexes.Add(lockIdx);
            LockLockReceiverIndexes[receiver] = StdLib.Serialize(indexes);
        }

        private static int GetReceiverIndexByAddress(List<LatchBoxLockReceiver> receivers, UInt160 receiverAddress)
        {
            int receiverIndex = -1;
            for (int i = 0; i < receivers.Count; i++)
            {
                if (receivers[i].ReceiverAddress == receiverAddress)
                {
                    receiverIndex = i;
                    break;
                }
            }

            return receiverIndex;
        }

        private static void AddLockReceivers(ByteString lockIdx, LatchBoxLockReceiver receiverObj)
        {
            var value = LockReceivers[lockIdx];
            List<LatchBoxLockReceiver> receivers = new List<LatchBoxLockReceiver>();
            if (value != null)
                receivers = (List<LatchBoxLockReceiver>)(StdLib.Deserialize(value));

            receivers.Add(receiverObj);
            LockReceivers[lockIdx] = StdLib.Serialize(receivers);
        }

        private static void UpdateLockReceivers(ByteString lockIdx, List<LatchBoxLockReceiver> newReceivers)
        {
            LockReceivers[lockIdx] = StdLib.Serialize(newReceivers);
        }

        private static bool IsAllReceiversClaimed(List<LatchBoxLockReceiver> receivers)
        {
            foreach (var receiver in receivers)
            {
                if (receiver.IsActive && receiver.DateClaimed == 0)
                {
                    return false;
                }
            }

            return true;
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

        private static void SetLock(ByteString lockIdx, LatchBoxLock latchBoxLock)
        {
            Locks[lockIdx] = StdLib.Serialize(latchBoxLock);
        }

        private static BigInteger GetLockIndex()
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_LockIndex };
            return (BigInteger)Storage.Get(context, key);
        }

        private static void IncrementLockIndex(int increment)
        {
            StorageContext context = Storage.CurrentContext;
            byte[] key = new byte[] { Prefix_LockIndex };
            var idx = (BigInteger)Storage.Get(context, key);
            idx = idx + increment;
            Storage.Put(context, key, idx);
        }
        
        private static void UpdateLockAssetIndexes(ByteString tokenScriptHash, BigInteger lockIdx)
        {
            var value = LockAssetIndexes[tokenScriptHash];
            List<BigInteger> indexes = new List<BigInteger>();
            if (value != null)
                indexes = (List<BigInteger>)(StdLib.Deserialize(value));

            indexes.Add(lockIdx);
            LockAssetIndexes[tokenScriptHash] = StdLib.Serialize(indexes);
        }

        private static void IncrementAssetLockedCounter(ByteString tokenScriptHash, BigInteger amount)
        {
            var value = AssetCounter[tokenScriptHash];
            LatchBoxAssetCounter counter = new LatchBoxAssetCounter() { LockedAmount = 0, UnlockedAmount = 0 };
            if(value != null)
                counter = (LatchBoxAssetCounter)(StdLib.Deserialize(value));
            
            counter.LockedAmount = counter.LockedAmount + amount;
            AssetCounter[tokenScriptHash] = StdLib.Serialize(counter);
        }

        private static void IncrementAssetUnlockedCounter(ByteString tokenScriptHash, BigInteger amount)
        {
            var value = AssetCounter[tokenScriptHash];
            LatchBoxAssetCounter counter = new LatchBoxAssetCounter() { LockedAmount = 0, UnlockedAmount = 0 };
            if(value != null)
                counter = (LatchBoxAssetCounter)(StdLib.Deserialize(value));
            
            counter.UnlockedAmount = counter.UnlockedAmount + amount;
            AssetCounter[tokenScriptHash] = StdLib.Serialize(counter);
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
