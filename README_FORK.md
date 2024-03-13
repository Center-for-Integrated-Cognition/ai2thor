# AI2THOR Fork for CIC usage

This is a fork of [AI2THOR](https://github.com/allenai/ai2thor) for usage at CIC. Thus far the changes are minimal:

* [Windows support](https://github.com/allenai/ai2thor/pull/1192)

We will send PR's upstream for changes that may be relevant to others, but many of the changes will be specific to our own research, particularly changes to assets.

See also the original readme [here](./README.md).

In addition to having the project dependencies installed, also `pip install invoke`.

Update the `url` property for the case where `cic_fork=True` in build.py. You'll need to choose a new tag name for the release.

Make sure everything you want is committed and pushed.
Build the wheel with `invoke build-pip-commit`. Upload that wheel to the release.

Build the `unity` subproject. Currently we then manually edit the zip to be the expected structure, but we should look into using `invoke ci-build` to properly structure it. Manual steps:

* add metadata.json:

```json
{"server_types": ["WSGI", "FIFO"]}
```

* top-level directory should be named `thor-<platform>-<commit sha1>`
* data directory should be named `thor-<platform>-<commit sha1>_Data`
* executable should be named `thor-<platform>-<commit sha1>`
    - TODO: Windows doesn't need a .exe, does it?

Upload the resulting zip to the release on GitHub. Double-check that you've matched the download URL specified in build.py in the previous step. Double-check that the commit sha1 in the zip name matches the commit sha1 in the wheel name.

Tell users to `pip install -f <wheel url>`. When they import AI2THOR for the first time in a running script, it will download and unpack the zip you've created and run the binary.
