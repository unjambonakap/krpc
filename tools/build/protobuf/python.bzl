def _impl(ctx):
    output = ctx.outputs.out
    protoc_output = output.path + '.tmp-proto-py'

    if (not output.path.endswith('_pb2.py')):
      fail('protoc output path must end with _pb2.py')

    sub_commands = [
        'rm -rf %s' % protoc_output,
        'mkdir -p %s' % protoc_output,
        '%s --python_out=%s %s' % (ctx.file._protoc.path, protoc_output, ctx.file.src.path),
        'cp %s/protobuf/*.py %s' % (protoc_output, output.path)
    ]

    ctx.actions.run_shell(
        inputs = [ctx.file.src, ctx.file._protoc],
        outputs = [output],
        command = ' && '.join(sub_commands),
        mnemonic = 'ProtobufPython',
        use_default_shell_env = True
    )

protobuf_py = rule(
    implementation = _impl,
    attrs = {
        'src': attr.label(allow_single_file=['.proto']),
        '_protoc': attr.label(
            default=Label('//tools/build/protobuf:protoc_new'),
            allow_single_file=True),
        'out': attr.output(mandatory=True)
    },
    output_to_genfiles = True
)
