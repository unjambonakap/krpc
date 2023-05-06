using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KRPC.Service.Attributes;
using KRPC.Utils;
using UnityEngine;

namespace KRPC.Service
{
    public class RPCInterfaceType
    {
        
        public override string ToString() => $"RPCType({LocalType}, {RemoteType})";
        public RPCInterfaceType(Type localType) {
            if (localType == typeof(Vector3d))
                Configure<Vector3d, global::KRPC.Utils.Tuple<double,double,double>>(x=>new Utils.Tuple<double, double, double>(x.x, x.y, x.z), x=>new Vector3d(x.Item1, x.Item2, x.Item3));
            else if (localType == typeof(Vector3))
                Configure<Vector3, global::KRPC.Utils.Tuple<float,float,float>>(x=>new Utils.Tuple<float, float, float>(x.x, x.y, x.z), x=>new Vector3(x.Item1, x.Item2, x.Item3));
            else if (localType == typeof(Vector2))
                Configure<Vector2, global::KRPC.Utils.Tuple<float,float>>(x=>new Utils.Tuple<float, float>(x.x, x.y), x=>new Vector2(x.Item1, x.Item2));
            else if (localType == typeof(Quaternion))
                Configure<Quaternion, global::KRPC.Utils.Tuple<float,float, float, float>>(x=>new Utils.Tuple<float, float, float, float>(x.x, x.y, x.z, x.w),
                 x=>new Quaternion(x.Item1, x.Item2, x.Item3, x.Item4));
            else if (localType == typeof(QuaternionD))
                Configure<QuaternionD, global::KRPC.Utils.Tuple<double,double, double, double>>(x=>new Utils.Tuple<double, double, double, double>(x.x, x.y, x.z, x.w),
                 x=>new QuaternionD(x.Item1, x.Item2, x.Item3, x.Item4));
            else
                LocalType = RemoteType = localType;

        }
        public Type LocalType { get; set; }
        public Type RemoteType { get; set; }
        public Func<object, object> Local2Remote { get; set; }
        public Func<object, object> Remote2Local { get; set; }

        internal void Configure<T1, T2>(Func<T1, T2> local2remote, Func<T2, T1> remote2local)
        {
            LocalType = typeof(T1);
            RemoteType = typeof(T2);
            Local2Remote = x => local2remote((T1)x);
            Remote2Local = x=>remote2local((T2)x);
        }
    }
    /// <summary>
    /// Used to invoke a class method with the KRPCMethod attribute.
    /// Invoke() gets the instance of the class using the guid
    /// (which is always the first parameter) and runs the method.
    /// </summary>
    sealed class ClassMethodHandler : IProcedureHandler
    {
        readonly MethodInfo method;
        readonly ProcedureParameter[] parameters;
        readonly object[] methodArguments;

        public ClassMethodHandler(Type classType, MethodInfo methodInfo, bool returnIsNullable)
        {
            method = methodInfo;
            var parameterList = method.GetParameters().Select(x => new ProcedureParameter(method, x)).ToList();
            parameterList.Insert(0, new ProcedureParameter(classType, "this"));
            parameters = parameterList.ToArray();
            methodArguments = new object[parameters.Length - 1];
            ReturnIsNullable = returnIsNullable;
            ReturnType = new RPCInterfaceType(methodInfo.ReturnType);
        }

        /// <summary>
        /// Invokes a method on an object. The first parameter must be an the objects GUID, which is
        /// used to fetch the instance, and the remaining parameters are passed to the method.
        /// </summary>
        public object Invoke(params object[] arguments)
        {
            object instance = arguments[0];
            // TODO: should be able to invoke default arguments using Type.Missing, but get "System.ArgumentException : failed to convert parameters"
            for (int i = 1; i < arguments.Length; i++)
                methodArguments[i - 1] = (arguments[i] == Type.Missing) ? parameters[i].DefaultValue : arguments[i];
            return method.Invoke(instance, methodArguments);
        }

        public IEnumerable<ProcedureParameter> Parameters
        {
            get { return parameters; }
        }

        public RPCInterfaceType ReturnType
        {
            get; private set;
        }

        public bool ReturnIsNullable { get; private set; }
    }
}
