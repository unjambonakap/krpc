#!/bin/bash
set -euv

pushd lib/extra
./info.sh
popd

bazel build -s //protobuf:csharp //protobuf:python --spawn_strategy=standalone
bazel build -s //server --spawn_strategy=standalone
./tools/install.sh
# upon krpc breakage
#bazel build -s //client/python:python-without-stubs
#pip install bazel-bin/client/python/krpc-python-0.4.9.zip 
#bazel build -s //tools/krpctools
#pip install bazel-bin/tools/krpctools/krpctools-0.4.9.zip


bazel build -s //client/python:python
pip install bazel-bin/client/python/krpc-python-0.4.9.zip 

