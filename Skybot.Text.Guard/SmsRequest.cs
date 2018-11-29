using System;
using System.Collections.Generic;
using System.Text;

namespace Skybot.Text.Guard
{
    public class SmsRequest
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
    }
}
