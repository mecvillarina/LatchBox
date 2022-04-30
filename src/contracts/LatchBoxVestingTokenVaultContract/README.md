# LatchBoxVestingTokenVaultContract

# Overview

LatchBox Platform Vesting feature fully utilizes this contract and it was deployed on Neo N3 Testnet (877933390)
- Contract ScriptHash: `0xbea9990a498bab7773e6deb93bcf69bc922e0596`

# Features

**Token Vesting** is similar with token lock but it supports multiple unlock periods. This is designed for newly launched/upcoming cryptocurrency projects on NEO blockchain that underwent presale/ICO and have vesting period for the releases of their token for their investors without manually releasing it to them. Also, this could be use to lock team allocated tokens and only unlock on the promised period and to gain and keep the trust of their community.

- The `initiator` of the vesting can:
  - **Add Vesting**, the initiator can choose the NEP-17 Token to be vested, define the periods and option to make it revocable anytime. For every period, the initiator can define unlock date and the token amount and address of each receivers of that period.
  - **Revoke Vesting**, the initiator can revoke the vesting anytime only if it is defined on **Add Vesting** that the vesting was revocable and all receiver doesn't claim it yet.
  - **Claim Refund**, after revoking the vesting, only unclaimed token can be refunded.

- The `receiver` of the vesting can:
  - **Claim Vesting**, the `receiver` of a specific vesting period can claim their token when the unlock date has passed and given that the initiator of that vesting doesn't revoke it.

- And anyone can view:
  - **Vesting Previewer**, it contains the vesting details including the period timeline and the receivers' details and it has a link that is shareable and publicly viewable.

# Smart Contract Interface

```c#
void AddVesting(UInt160 tokenScriptHash, BigInteger totalAmount, bool isRevocable, LatchBoxVestingPeriodParameter[] periods); // add new vesting
void ClaimVesting(BigInteger vestingIdx, BigInteger periodIdx); // claim vesting from specific period
void RevokeVesting(BigInteger vestingIdx); //revoke vesting
void ClaimRefund(UInt160 tokenScriptHash); //claim refund
BigInteger GetVestingsCount(); //returns number of vestings
Map<ByteString, object> GetVestingTransaction(BigInteger vestingIdx);  //Get specific vesting transaction
Map<ByteString, object>[] GetVestingsByInitiator(UInt160 initiatorAddress); //Get All vestings of a specific initiator
Map<ByteString, Map<ByteString, object>> GetVestingsByReceiver(UInt160 receiverAddress); //Get All vesting of a specific receiver
Map<ByteString, BigInteger> GetRefunds(); //Get All Refunds
void ValidateNEP17Token(UInt160 tokenScriptHash); //Validates if the given tokenScriptHash is valid for Vesting
void Deploy(object data, bool update); //Deploy contract
void Update(ByteString nefFile, string manifest);  //Update contract
void Destroy(); //Destroy contract
UInt160 GetPaymentTokenScriptHash(); //If payment has been setup, it will return the payment token script hash 
BigInteger GetPaymentTokenAddVestingFee(); //If payment has been setup, it will return the add vesting fee, otherwise it returns 0. 
BigInteger GetPaymentTokenClaimVestingFee(); //If payment has been setup, it will return the claim vesting fee, otherwise it returns 0. 
BigInteger GetPaymentTokenRevokeVestingFee(); //If payment has been setup, it will return the revoke vesting fee, otherwise it returns 0. 
BigInteger GetPaymentBurnedAmount();  //Get Burned Amount from the vesting transactions
void SetupPayment(UInt160 tokenScriptHash, BigInteger addVestingFee, BigInteger claimVestingFee, BigInteger revokeVestingFee) ; //setup payment for vesting transactions.
```
