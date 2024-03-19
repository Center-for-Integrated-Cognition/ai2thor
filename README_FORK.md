# AI2THOR Fork for CIC usage

This is a fork of [AI2THOR](https://github.com/allenai/ai2thor) for usage at CIC. Thus far the changes are minimal:

* [Windows support](https://github.com/allenai/ai2thor/pull/1192)

We will send PR's upstream for changes that may be relevant to others, but many of the changes will be specific to our own research, particularly changes to assets.

See also the original readme [here](./README.md).

In addition to having the project dependencies installed, also `pip install invoke wheel`.

Update the `url` property for the case where `cic_fork=True` in build.py. You'll need to choose a new tag name for the release.

Make sure everything you want is committed and pushed.
Build the wheel with `invoke build-pip-commit`. Upload that `.whl` file located under `dist/` to the release.

Build the `unity` subproject. Currently we then manually edit the zip to be the expected structure, but we should look into using `invoke ci-build` to properly structure it. Manual steps:

* add metadata.json:

```json
{"server_types": ["WSGI", "FIFO"]}
```

* There should be no top-level directory inside of the zip. The zip should directly contain each of the required build files.
* data directory should be named `thor-<platform>-<commit sha1>_Data`
* executable should be named `thor-<platform>-<commit sha1>` (no `.exe`!)

Note that the platform name for Windows is `StandaloneWindows64`.

Upload the resulting zip to the release on GitHub. Double-check that you've matched the download URL specified in build.py in the previous step. Double-check that the commit sha1 in the zip name matches the commit sha1 in the wheel name. If on a Mac, remove the `__MACOSX` directory from the zip file with `zip -d <zip file> __MACOSX/\*` (failing to do so will result in errors when the user downloads the zip).

Place the sha256 of the zip file in a text file and upload that to the release as well. It should have the same name as the zip file, but with the extension changed to `.sha256`. Quick snippet for generating the sha256:

```bash
sha256sum <zip file> > <zip file>.sha256
```

Works on my machine :) You can also do it in Python like so:

```python
import hashlib

def sha256sum(filename):
    with open(filename, 'rb', buffering=0) as f:
        return hashlib.file_digest(f, 'sha256').hexdigest()
```

Tell users to `pip install -f <wheel url>`. When they import AI2THOR for the first time in a running script, it will download and unpack the zip you've created and run the binary.
