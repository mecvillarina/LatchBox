# Overview

LatchBox Platform Locks feature relies in this contract and it was deployed on Neo N3 Testnet (877933390)
- Contract ScriptHash: `0x3dccbd00cdd180aec6bf702eab5bd534b5c0e285`

# Features

**Token Lock** provides locking of NEP-17 Tokens in a Smart Contract for certain period of time. This is designed for those people who loves to hold their tokens for a long time and have no intention selling it for a cheap price. 
    
- The `initiator` of the lock can:
  - **Add Lock**, the initiator can choose the NEP-17 Token to be locked, define the unlock date, option to make it revocable anytime, and the token amount and address of each receiver of the lock.
  - **Revoke Lock**, the initiator can revoke the lock anytime only if it is defined on **Add Lock** that the lock was revocable and all receiver doesn't claim it yet.
  - **Claim Refund**, after revoking the lock, only unclaimed token can be refunded.

- The `receiver` of the lock can:
  - **Claim Lock**, the `receiver` of a lock can claim their token when the unlock date has passed and given that the initiator of the lock doesn't revoke it.

- And anyone can view:
  - **Lock Previewer**, it contains the lock details including the receivers' details (amount and address) and it has a link that is shareable and publicly viewable.

# Smart Contract Interface

```c#
void AddLock(UInt160 tokenScriptHash, BigInteger totalAmount, BigInteger unlockTime, LatchBoxLockReceiverParameter[] receivers, bool isRevocable); // add new lock
void ClaimLock(BigInteger lockIdx); // claim lock
void RevokeLock(BigInteger lockIdx); //revoke lock
void ClaimRefund(UInt160 tokenScriptHash); //claim refund
BigInteger GetLocksCount(); //returns number of locks
Map<ByteString, object> GetLockTransaction(BigInteger lockIdx);  //Get specific lock transaction
Map<ByteString, object>[] GetLocksByInitiator(UInt160 initiatorAddress); //Get All locks of a specific initiator
Map<ByteString, object>[] GetLocksByReceiver(UInt160 receiverAddress); //Get All locks of a specific receiver
Map<ByteString, object>[] GetLocksByAsset(UInt160 tokenScriptHash);  //Get All locks of a specific NEP-17 Token
Map<ByteString, BigInteger> GetRefunds(); //Get All Refunds
Map<ByteString, object> GetAssetsCounter();  //Get All NEP-17 Tokens Locked and unlocked count.
void ValidateNEP17Token(UInt160 tokenScriptHash); //Validates if the given tokenScriptHash is valid for Locking
void Deploy(object data, bool update); //Deploy contract
void Update(ByteString nefFile, string manifest);  //Update contract
void Destroy(); //Destroy contract
UInt160 GetPaymentTokenScriptHash(); //If payment has been setup, it will return the payment token script hash 
BigInteger GetPaymentTokenAddLockFee(); //If payment has been setup, it will return the add lock fee, otherwise it returns 0. 
BigInteger GetPaymentTokenClaimLockFee(); //If payment has been setup, it will return the claim lock fee, otherwise it returns 0. 
BigInteger GetPaymentTokenRevokeLockFee(); //If payment has been setup, it will return the revoke lock fee, otherwise it returns 0. 
BigInteger GetPaymentBurnedAmount();  //Get Burned Amount from the lock transactions
SetupPayment(UInt160 tokenScriptHash, BigInteger addLockFee, BigInteger claimLockFee, BigInteger revokeLockFee); //setup payment for lock transactions.
```
