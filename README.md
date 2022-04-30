# LatchBox

**LatchBox** is a fully decentralized token lock and vesting platform on Neo N3 Blockchain. LatchBox aims to protect every cryptocurrency community from rug pulls and traitorous advisors. The native token used in LatchBox platform is named `LATCH` with a ScriptHash of `0xb9c2c7f906c66d5b9a27597a7168ccb0fe8c2442`.

## LatchBox Platform
- **App Portal** - https://app-latchbox-testnet.azurewebsites.net/
- **Blockchain** - NEO Blockchain (N3)
    - **Network** - TESTNET (877933390)
    - **RPC Url** - http://seed1t4.neo.org:20332

## Features
- **FOR TESTNET**, anyone can buy `LATCH` with `NEO` or `GAS` on the LatchBox Portal, the current conversion are:
    - 1 `NEO` = 500 `LATCH`
    - 1 `GAS` - 50 `LATCH`

- **Token Lock** provides locking of NEP-17 Tokens in a Smart Contract for certain period of time. This is designed for the people who loves to hold their tokens for a long time and have no intention selling it for a cheap price. 
    
    The `initiator` of the lock can:
    - **Add Lock**, the initiator can choose the NEP-17 Token to be locked, define the unlock date, option to make it revocable anytime, and the token amount and address of each receiver of the lock.
    - **Revoke Lock**, the initiator can revoke the lock anytime only if it is defined on **Add Lock** that the lock was revocable and all receiver doesn't claim it yet.
    - **Claim Refund**, after revoking the lock, only unclaimed token can be refunded.

    The `receiver` of the lock can:
    - **Claim Lock**, the `receiver` of a lock can claim their token when the unlock date has passed and given that the initiator of the lock doesn't revoke it.

    And anyone can view:
    - **Lock Previewer**, it contains the lock details including the receivers' details (amount and address) and it has a link that is shareable and publicly viewable.

- **Token Vesting** is similar with token lock but it supports multiple unlock periods. This is designed for newly launched Cryptocurrency projects on NEO blockchain that undergo presale/ICO and have vesting period for the releases of the token for their investors without manually releasing it to them. Also, this could use to lock team allocated tokens and only unlock on the promised period and to gain and keep the trust of their community to them.
    
    The `initiator` of the vesting can:
    - **Add Vesting**, the initiator can choose the NEP-17 Token to be vested, define the periods and option to make it revocable anytime. For every period, the initiator can define unlock date and the token amount and address of each receivers of that period.
    - **Revoke Vesting**, the initiator can revoke the vesting anytime only if it is defined on **Add Vesting** that the vesting was revocable and all receiver doesn't claim it yet.
    - **Claim Refund**, after revoking the vesting, only unclaimed token can be refunded.

    The `receiver` of the vesting can:
    - **Claim Vesting**, the `receiver` of a specific vesting period can claim their token when the unlock date has passed and given that the initiator of that vesting doesn't revoke it.

    And anyone can view:
    - **Vesting Previewer**, it contains the vesting details including the period timeline and the receivers' details and it has a link that is shareable and publicly viewable.

## Costs
All token lock and vesting transactions requires `LATCH` and `GAS` Token, the cost will vary in the future but for now it will be:
- `GAS` will be used as transaction fee.
- Locks
    - Add Lock - **5 `LATCH`**
    - Claim Lock - **2 `LATCH`**
    - Revoke Lock - **10 `LATCH`**
    - Claim Refunds - **FREE**
- Vestings
    - Add Vesting - **10 `LATCH`**
    - Claim Vesting - **2 `LATCH`**
    - Revoke Vesting - **10 `LATCH`**
    - Claim Refunds - **FREE**

The cost is defined in the smart contract and the contract owner can change it anytime. User can see the current cost anytime before confirming the transaction. 

## Transaction Allocation
All token lock and vesting transactions will be paid using `LATCH` and it will be distributed as follows:
- 50% will be **burned**
- 40% will go to **staking wallet**, in the future, this will primarily used when the staking program in the platform has been developed.
- 5% will go to **foundation wallet**, this will primarily used for the development of the platform.
- 5% will go to **platform wallet**, this will used to pay for the infrastructure (cloud hosting).

## Dashboard Statistics
LatchBox Platform has dashboard to track the following:
- **Platform Token** - token information about `LATCH`.
- **Lock Token Vault Contract** - total number of created token locks, the amount burned in `LATCH` for using Locks feature.
- **Vesting Token Vault Contract** - total number of created token vestings, the amount burned in `LATCH` for using Vesting feature.

## Smart Contracts
- **Platform Token (LATCH)** - `0xb9c2c7f906c66d5b9a27597a7168ccb0fe8c2442` ([See Code](src/contracts/LatchBoxToken/src/))
- **Lock Token Vault Contract** - `0x3dccbd00cdd180aec6bf702eab5bd534b5c0e285` ([See Code](src/contracts/LatchBoxLockTokenVaultContract/src/))
- **Vesting Token Vault Contract** - `0xbea9990a498bab7773e6deb93bcf69bc922e0596` ([See Code](src/contracts/LatchBoxVestingTokenVaultContract/src/))

## Technology Stack & Tools
- Cloud Service Provider: Microsoft Azure
- Web Frontend: 
    - IDE: Visual Studio 2022
    - Web Framework: Blazor Server/.NET 6
	- C# as Programming Language 
	- Deployed on Azure App Service.
- Neo N3 Smart Contract:
	- IDE: Visual Studio Code
    - Tool: NEO N3 Visual DevTracker / Neo SDK
    - C# as Programming Language

## Setup Guide:
- LatchBox Portal [Setup Guide](src/client/README.md)