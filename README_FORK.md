# AI2THOR Fork for CIC usage

This is a fork of [AI2THOR](https://github.com/allenai/ai2thor) for usage at CIC. We will send PR's upstream for changes that may be relevant to others, but many of the changes will be specific to our own research, particularly changes to assets.

See also the original readme [here](./README.md).

## Building for Windows

We have an open PR with AI2 for fixing the Python code on Windows:

* [Windows support](https://github.com/allenai/ai2thor/pull/1192)

However, even with these changes, AI2 will not distribute Windows builds. Therefore, we have to build our own Windows binaries.

### Install Dependencies

In addition to having the project dependencies installed, also `pip install invoke wheel`.

### Point URL to a New GitHub Release, then Commit and Push

* Create a new release on GitHub.  You'll need to choose a new tag name.

* Then, update the `url` property for the case where `cic_fork=True` in build.py to point to the new release files (which you will upload in following steps).

* Make sure everything you want is committed and pushed, as you will need a stable commit hash.

### Generate Wheel

Build the wheel with `invoke build-pip-commit`. Upload that `.whl` file located under `dist/` to the new GitHub release.

### Build Unity Project

Build the `unity` subproject via `invoke local-build --arch=StandaloneWindows64`. This generates a .zip file.

### Restructure Zip File

Currently we then manually edit the zip to be the expected structure, but we should look into using `invoke ci-build` to properly structure it. The easiest way to do this part is to download a previous release's zip and inspect the contents. The manual checks/steps we have had to follow previously were:

* There should be no top-level directory inside of the zip. The zip should directly contain each of the required build files.
* Ensure that there is a `metadata.json` file with these contents :

```json
{"server_types": ["WSGI", "FIFO"]}
```

* The data directory should be named `thor-StandaloneWindows64-<commit hash>_Data`
* The executable should be named `thor-StandaloneWindows64-<commit hash>.exe`
* If you create the zip file on a Mac, remove the `__MACOSX` directory from the zip file with `zip -d <zip file> __MACOSX/\*` (failing to do so will result in errors when the user downloads the zip).
* Name the zip file to match the `url` property in `build.py`, which you updated earlier (so probably `ai2thor_windows.zip`).
* Double-check one more time that the commit hash in the zipped file names matches the commit hash in the wheel name!
* Upload the .zip file to the new GitHub release.

### Generate SHA Sum

Place the sha256 of the zip file in a text file and upload that to the release as well. It should have the same name as the zip file, but with the extension changed to `.sha256`. To generate the sha256:

```bash
sha256sum <zip file>
```

Works on my machine :) You can also do it in Python like so:

```python
import hashlib

def sha256sum(filename):
    with open(filename, 'rb', buffering=0) as f:
        return hashlib.file_digest(f, 'sha256').hexdigest()
```

### Installing the Windows Build

For the THOR-Soar project, you should update the package install URL in the `requirements-Windows.txt` file to point to the new wheel URL. Users should then follow the [instructions](https://github.com/Center-for-Integrated-Cognition/THOR-Soar/blob/main/docs/installation.md#updating-dependencies) in the THOR-Soar repo to update their dependencies. The new ai2thor zip will be downloaded when they next run the THOR-Soar application. You should test this yourself and use the ControllerGUI to move around in the scene to ensure that everything works as expected.

To specifically install the new `ai2thor` release in some other project, users just need to `pip install -f <wheel url>`. When they import AI2THOR for the first time in a running script, it will download and unpack the zip you've created and run the binary.

## Working with Unity

We can customize scenes a bit via configuration or by using procTHOR scenes (see the [THOR-Soar connector readme](https://github.com/Center-for-Integrated-Cognition/THOR-Soar/tree/main/connector#changing-the-scene-setup)). However, changes to iTHOR scenes (the nice-looking ones with lots of interactivity) that cannot be accomplished via agent actions require editing and building the `unity` project. This project generates the `AI2Thor` executable, which is called from Python and controls the Unity display window, physics simulation, etc.

As a first step, you will need to check out (or download) this repository to your computer. We recommend putting this repository next to your copies of the [THOR-Soar repository](https://github.com/Center-for-Integrated-Cognition/THOR-Soar) and, in CALM's case, also the [ITL-agent repository](https://github.com/Center-for-Integrated-Cognition/ITL-Agent).

### Unity Setup

#### Linux

Install Unity Hub via the instructions on the Unity website: https://docs.unity3d.com/hub/manual/InstallHub.html#install-hub-linux.

Next you should load the project from Unity Hub. This will do 2 things for you:

1) It will require you to input your Unity credentials, which will be saved for future use (such as building the project)
2) It will install the correct version of the Unity editor for the project

> GOTCHA: On my machine (Aaron, Arch-based linux) it would not recognize the credentials. I instead added something to `tasks.py` so you can specity the environment variable `UNITY_HUB_PATH` to where unityhub is installed (for me, `/opt/unityhub/unityhub`). You may need to manually build the project, but it works. 

To load the project, open Unity Hub and open the projects tab. Add the `unity` sub-project found in this repo, then double-click the project name to open the editor.

> GOTCHA: If your project loads with compile errors that have no message or source location, you won't be able to compile it, and I don't know how to fix this. The CALM machine is already set up, so you may wish to use that instead.
> Aaron: I had this issue, and I looked into the Editor.log (on my machine in ~/.config/unity3d). This shows the error was related to not finding the right ICU (internationalization) library. I wasn't sure how to fix this, but a workaround is to run unity with `export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1`



### Building the `unity` Subproject

#### Linux/CALM Machine

Depending on what Python environment you're using, you may need to install the project dependencies, as well as `invoke`:

    cd <ai2thor repository path>
    pip install .
    pip install boto3
    pip install invoke

Now you can build using this command in the terminal:

    invoke local-build --arch=Linux64 --no-batchmode

This launches a Unity GUI with a progress bar while building the project. On a fresh checkout this might take 30 minutes. On a pre-existing project (as should be the case on the CALM machine!) this should be much faster. TODO: there is a `--scenes scene1,scene2,scene3...` for only compiling certain scenes (which would greatly speed up the build), but it seems to cause the controller startup to freeze with a closeup of the fridge.

To monitor the build process in more detail, you may wish to open a separate terminal and tail the build log, which is output in the project root directory:

    tail -f thor-Linux64-local.log

> GOTCHA: running in batch mode, meaning without opening the Unity UI, currently fails on the CALM machine because we haven't been able to save Unity credentials in a keychain yet. Build in a remote desktop session and use `--no-batchmode` for now.

>GOTCHA: If the build seems to hang forever with the Unity progress modal stating that it is "Building GI data," it may actually be stuck forever. It will time out eventually, after which time you can simply re-run the build and it should succeed. It might be okay to cancel the build when you see this happening, but I haven't tried that yet.

The build will create many new `.meta` and `.mat` files. Please don't check these into git. TODO: can we `.gitignore` these? Or should we in fact be adding these to git?

### Using your unity Build in the Python Connector

Set the `ai2_thor_local_executable_path` variable in your config file to the path of the executable you built. This will be located under `<project root>/unity/builds/thor-<platform>-local/...`. The ITL-Agent repository's `rosie.config` already contains example configuration.

### Creating New Scenes

TODO

### Adding/Editing Objects

TODO

### Debugging in Unity

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
