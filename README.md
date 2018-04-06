[<img src="https://github.com/HonestInsurance/Resources/blob/master/branding/HIC_Logo_Vertical.png?raw=true" width="150">](https://www.honestinsurance.net)

-----------------------

## Overview

The API services in this repository provide a service layer to connect to a self-governing, subscription based, people-to-people insurance service deployed on an EVM powered Blockchain. More specifically, these services connect and interact to the following eight smart contracts:
* Trust
* Pool
* Bank
* Timer
* Adjustor 
* Policy
* Settlement
* Bond

For a detailed description of the purpose of these contracts and more context please refer to the [white paper](https://github.com/HonestInsurance/Resources/blob/master/research/WhitePaper-HonestInsurance.pdf?raw=true) and the [Smart-contracts](https://github.com/HonestInsurance/Smart-Contracts) repository documentation.

These API services are deployed and available at this URL:
* [https://api.honestinsurance.net](https://api.honestinsurance.net)

Further more, a detailed description of all the available endpoints, their purpose and required parameters is also presented at this url.
For testing purposes, the entire Postman api collection can be downloaded [here](https://github.com/HonestInsurance/Resources/blob/master/postman/HonestInsurance-API.postman_collection.json?raw=true) and the corresponding Postman environment configuration [here](https://github.com/HonestInsurance/Resources/blob/master/postman/HonestInsurance-API.postman_environment.json?raw=true).


-----------------------

## Looking under the hood

This solution is implemented C# and the following technology stacks are used in this solution:
* .Net Core version [2.0.6](https://github.com/dotnet/core)
* ServiceStack Core version [5.0.2](https://www.nuget.org/packages/ServiceStack.Core) (plus a valid license key)
* Nethereum version [2.4.0](https://www.nuget.org/packages/Nethereum.Web3/)
* Fluent validation (Newtonsoft.Json) version [11.0.2](https://www.nuget.org/packages/Newtonsoft.Json)

The solution is hosted on a Ubuntu Server with the following configuration:
* Ubuntu version 16.04.3 LTS
* Virtual Machine specs: 1 Core; 3.5 GB Memory; 30 GB SSD disk
* Nginx web server configured to listen on port 80 and 443 and kestrel set up to manage the process
* Certbot was used to configure Let's Encrypt certificates to enforce all traffic to be HTTPS only

-----------------------

## A look at what's there

The links below can be used to connect to a private Blockchain and retrieve policy, settlement, bond, etc. related information deployed on a private Blockchain.
* [List of all available APIs and documentation](https://api.honestinsurance.net)
* [Status of the ecosystem](https://api.honestinsurance.net/ecosystem/status?ContractAdr=0xf25186b5081ff5ce73482ad761db0eb0d25abfbf)
* [Log files of ecosystem](https://api.honestinsurance.net/ecosystem/logs?ContractAdr=0xf25186b5081ff5ce73482ad761db0eb0d25abfbf)
* [List of all bonds](https://api.honestinsurance.net/bond/list?ContractAdr=0xf25186b5081ff5ce73482ad761db0eb0d25abfbf)
* [Details of bond 1](https://api.honestinsurance.net/bond?ContractAdr=0xf25186b5081ff5ce73482ad761db0eb0d25abfbf&Idx=1)
* [List of all policies](https://api.honestinsurance.net/policy/list?ContractAdr=0xf25186b5081ff5ce73482ad761db0eb0d25abfbf)
* [Details of policy 1](https://api.honestinsurance.net/policy?ContractAdr=0xf25186b5081ff5ce73482ad761db0eb0d25abfbf&Idx=1)

-----------------------

## Gratitude

Thank you to the guys at ServiceStack for providing this framework and also making work beautifully on the .Net Core framework. To Microsoft for supporting and pushing .Net Core along. Lastly to Juan Blanco and his team for giving us Nethereum, THANK YOU!

-----------------------

## License

GPL-3.0