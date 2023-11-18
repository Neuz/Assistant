using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Assistant.Messages
{
    public class GotoMessage<Type>:ValueChangedMessage<Type>
    {
        public GotoMessage(Type value) : base(value)
        {
        }
    }
}
