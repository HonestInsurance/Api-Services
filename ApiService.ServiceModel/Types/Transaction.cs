/**
 * @description Transaction Data Transfer Objects (DTOs)
 * @copyright (c) 2017 HIC Limited (NZBN: 9429043400973)
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using ServiceStack;
using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;


namespace ApiService.ServiceModel
{
    [FunctionOutput]
    public class TransactionHash {
        [ApiMember(IsRequired = true, Description = "The transaction hash of the transaction submitted.")]
        public string Hash { get; set; }
    }

    [FunctionOutput]
    public class TransactionResult {
        [ApiMember(IsRequired = true, Description = "The hash of the transaction")]
        public string TransactionHash { get; set; }
        [ApiMember(IsRequired = true, Description = "The index of the transaction")]
        public uint TransactionIndex { get; set; }
        
        [ApiMember(IsRequired = true, Description = "The hash of the block this transaction belongs to")]
        public string BlockHash { get; set; }

        [ApiMember(IsRequired = true, Description = "The number of the block this transaction belongs to")]
        public uint BlockNumber { get; set; }

        [ApiMember(IsRequired = true, Description = "Amount of gas used for this transaction")]
        public uint GasUsed { get; set; }

        [ApiMember(IsRequired = true, Description = "The cummulated amount of gas used for this transaction")]
        public uint CumulativeGasUsed { get; set; }

        [ApiMember(IsRequired = true, Description = "Indicates if the transaction completed successfully")]
        public bool Success { get; set; }

        [ApiMember(IsRequired = true, Description = "The address of the contract that was created (if applicable)")]
        public string ContractAddress { get; set; }

        [ApiMember(IsRequired = true, Description = "Number of event logs that were triggered as part of this transaction")]
        public uint LogCount { get; set; }
        
        [ApiMember(IsRequired = false, Description = "List of the contracts that triggered event logs as part of this transaction")]
        public List<string> ContractReferenceAdr { get; set; }

        [ApiMember(IsRequired = false, Description = "List of object hashes this transaction relates to")]
        public List<string> ObjectReferenceHash { get; set; }
    }
}
