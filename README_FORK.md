# AI2THOR Fork for CIC usage

This is a fork of [AI2THOR](https://github.com/allenai/ai2thor) for usage at CIC. We will send PR's upstream for changes that may be relevant to others, but many of the changes will be specific to our own research, particularly changes to assets.

See also the original readme [here](./README.md).

## Building for Windows

We have an open PR with AI2 for fixing the Python code on Windows:

* [Windows support](https://github.com/allenai/ai2thor/pull/1192)

However, even with these changes, AI2 will not distribute Windows builds. Therefore, we have to build our own Windows binaries.

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
* executable should be named `thor-<platform>-<commit sha1>` (with `.exe` on Windows)

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

## Editing Scenes

You'll need to follow the instructions the [unity subproject readme](./unity/README.md) to get the correct version of Unity setup. You can then edit the scenes in Unity and export them to the Python API. Note that if you are on Linux there is a shortcut to installing Unity Hub and the correct version of the Unity Editor:

```bash
pip install invoke
invoke install_unity_hub
invoke install_unity_editor
```

Once you have these installed, open Unity Hub and open the projects tab. Add the `unity` project, then double-click the project name to open the editor.

### Creating New Scenes

TODO

### Adding/Editing Objects

TODO

## Debugging in Unity

From Luca Weihs (not yet tested):

0. Ensure you have AI2-THOR installed and that you have Flask==2.0.1 and Werkzeug==2.0.1 installed (using the correct versions of these requirements is important).
1. Start the Unity editor and open any Unity scene (e.g. FloorPlan1_physics.unity)
2. Initialize the Python controller with the parameters `start_unity=False`, `port=8200`, and `server_class=ai2thor.wsgi_server.WsgiServer`:
 
```python
controller = ai2thor.controller.Controller(
        start_unity=False,
        port=8200,
        scene="FloorPlan1",
        server_class=ai2thor.wsgi_server.WsgiServer,
        visibilityScheme="Distance"
    )
```

3. Press play in the Unity editor, Unity should now be connected to the Python server.
4. Run all the commands from Python that you want.
5. Call controller.stop() to unblock Unity.
6. Depending on the os and the frame that you create your controller in (I'd scope it in a with) the socket port might be left bound so to unbound it, destruct the controller object or run lsof -ti:8200 | xargs kill -9 to unbind.
 
Here's a quick script that will start a default agent in FloorPlan1 and then moves it a bit, let me know if you have any questions:
 
```python
import ai2thor
import ai2thor.controller
import ai2thor.wsgi_server
import time
 
print("Press play in the unity editor a second or two after this prints.")
controller = ai2thor.controller.Controller(
    start_unity=False,
    port=8200,
    height=300,
    width=300,
    gridSize=0.25,
    scene="FloorPlan1",
    server_class=ai2thor.wsgi_server.WsgiServer,
    visibilityScheme="Distance"
)
 
print("Rotating left")
controller.step("RotateLeft")
time.sleep(1)
 
print("Moving ahead 3 times")
controller.step("MoveAhead")
controller.step("MoveAhead")
controller.step("MoveAhead")
```