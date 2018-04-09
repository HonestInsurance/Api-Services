/**
 * @description Timer Data Transfer Objects (DTOs)
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
    public class TimerNotificationEntry {
        [ApiMember(IsRequired = true, Description = "The address of the contract to ping")]
        [Parameter("address", "notificationAddress", 1)]
        public string Address { get; set; }

        [ApiMember(IsRequired = true, Description = "The subject describing the ping notification")]
        [Parameter("uint", "subject", 2)]
        public ulong Subject { get; set; }

        [ApiMember(IsRequired = true, Description = "The message containing further info on the notification")]
        [Parameter("bytes32", "message", 3)]
        public string Message { get; set; }

        [ApiMember(IsRequired = true, Description = "The timestamp this notification is scheduled")]
        public ulong Timestamp { get; set; }
    }

    [FunctionOutput]
    public class TimerNotifications {
        [ApiMember(IsRequired = true, Description = "Notification log entries")]
        public List<TimerNotificationEntry> NotificationEntries {get; set;}
    }
}