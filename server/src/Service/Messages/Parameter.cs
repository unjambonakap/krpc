using System;

namespace KRPC.Service.Messages
{
    #pragma warning disable 1591
    public class Parameter : IMessage
    {
        public string Name { get; private set; }

        public RPCInterfaceType Type { get; private set; }

        public bool HasDefaultValue { get; private set; }

        public object DefaultValue {
            get { return defaultValue; }
            set {
                defaultValue = value;
                HasDefaultValue = true;
            }
        }

        object defaultValue;

        public Parameter (string name, RPCInterfaceType type)
        {
            Name = name;
            Type = type;
        }
    }
}
