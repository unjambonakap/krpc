using System;
using System.Collections.Generic;

namespace KRPC.Service
{
    /// <summary>
    /// Use to invoke the method that implement an RPC
    /// </summary>
    public interface IProcedureHandler
    {
        object Invoke (params object[] arguments);

        IEnumerable<ProcedureParameter> Parameters { get; }

        RPCInterfaceType ReturnType { get; }

        bool ReturnIsNullable { get; }
    }
}
