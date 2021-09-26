load('//bzl/bazel-skylib/lib:paths.bzl', 'paths')
load('//tools/build:csharp.bzl', 'csharp_reference', 'csharp_references')

files = [
'extra/icsharp.kernel/lib/net40/iCSharp.Messages.dll',
'extra/icsharp.kernel/lib/net40/iCSharp.Kernel.dll',
'extra/icsharp.kernel/lib/net40/Common.dll',
'extra/icsharp.kernel/libs/Newtonsoft.Json.dll',
'extra/icsharp.kernel/libs/ICSharpCode.NRefactory.dll',
'extra/icsharp.kernel/libs/Mono.Cecil.Pdb.dll',
'extra/icsharp.kernel/libs/ICSharpCode.NRefactory.CSharp.dll',
'extra/icsharp.kernel/libs/Mono.Cecil.Rocks.dll',
'extra/icsharp.kernel/libs/NetMQ.dll',
'extra/icsharp.kernel/libs/Mono.Cecil.dll',
'extra/icsharp.kernel/libs/ICSharpCode.NRefactory.Xml.dll',
'extra/icsharp.kernel/libs/Mono.CSharp.dll',
'extra/icsharp.kernel/libs/Mono.Cecil.Mdb.dll',
'extra/icsharp.kernel/libs/Common.Logging.dll',
'extra/icsharp.kernel/libs/Common.Logging.Core.dll',
'extra/icsharp.kernel/libs/Common.Logging.Log4Net1213.dll',
'extra/icsharp.kernel/libs/log4net.dll',
'extra/icsharp.kernel/libs/ICSharpCode.NRefactory.Cecil.dll',
'extra/icsharp.kernel/libs/AsyncIO.dll',
'extra/icsharp.kernel/libs/System.Xml.Linq.dll',
'mono-4.5/System.Numerics.dll',
'mono-4.5/System.Runtime.Serialization.dll',
'mono-4.5/System.Data.dll',
'mono-4.5/System.ServiceModel.Internals.dll',
'mono-4.5/System.ServiceModel.dll',
'mono-4.5/System.Transactions.dll',
'mono-4.5/System.ComponentModel.DataAnnotations.dll',
'mono-4.5/System.IdentityModel.dll',
]



def libs_name():
  res= []
  for x in files:
    rulename = '//lib:'+x
    res.append(rulename)
  return res

def define_refs():
  for x in files:
    rulename = '//lib:'+x
    bname=paths.basename(x)
    name = paths.split_extension(bname)[0]
    csharp_reference(name = name, file = rulename)

def build_deps():
  res= []
  for x in files:
    rulename = '//lib:'+x
    bname=paths.basename(x)
    name = paths.split_extension(bname)[0]
    res.append('//tools/build/ksp:%s'%name)
  return res


extras = struct(files=files, define_refs=define_refs, build_deps=build_deps,
libs_name=libs_name)
