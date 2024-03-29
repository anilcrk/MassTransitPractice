using MassTransit.Transports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassTransit.MessageSending
{
    public class NotificationMessage
    {
        public short NotificationType { get; set; }

        public int AgentId { get; set; }

        public string Payload { get; set; }
        public string GWReferenceNo { get; set; }
    }


    public class OutgoingNotificationMessage : NotificationMessage
    {

    }


    public class CustomRoutingKeyFormatter<T> : IMessageRoutingKeyFormatter<T> where T : class
    {
        public string FormatRoutingKey(SendContext<T> context)
        {
            if (typeof(T) == typeof(OutgoingNotificationMessage))
            {
                return "outgoing.notification";
            }
            else
            {
                return "common.notification";
            }
        }
    }
}
