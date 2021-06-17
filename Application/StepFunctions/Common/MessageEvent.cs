using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class MessageEvent
    {
        public MessageEvent()
        {

        }

        public MessageEvent(string targetUser, string resourceId)
        {
            this.TargetUser = targetUser;
            this.ResourceId = resourceId;
        }


        public string TargetUser { get; set; }

        public string ResourceId { get; set; }

        public string Message { get; set; }

        public string Data { get; set; }

        public bool CompleteEvent { get; set; }
    }
}
