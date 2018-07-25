# arshm-release

Experimental mobile app demoing cultural heritage documentation tools in augmented reality. Supports capturing and annotating photos in 3D space. Proof-of-concept features in the app include image anchoring and surface drawing tools. Implemented in Unity using the [ARInterface library](https://github.com/Unity-Technologies/experimental-ARInterface) for cross-platform AR support.

Created in Spring 2018 as part of a Computer Science Independent Work project at Princeton University, advised by Professor Branko Glisic and Rebecca Napolitano.

More information about the project is described in the paper: [Mobile AR Software for Cultural Heritage Documentation](written_final_report.pdf).

## Building

There are several scenes in this project, some of which are used for testing specific components and controllers. The scene containing the full app is named ARSHM.

To build and run this on a device:

1. Open the Build Settings window from File->Build Settings.
2. In the upper half of the window, under "Scenes in Build", ensure that ARSHM is checked while all other scenes are unchecked.
3. Ensure that the desired target platform is selected in the lower left. Currently, only Android is fully working. (If you do want to switch the target platform, be sure to click the Switch Platform button.)
4. Plug in your Android device.
5. Click "Build And Run". Unity will prompt for a location to save the generated APK.
6. The app will be installed and launched on the device automatically.

Once the correct scene and platform are selected, you can re-build the app without going through the whole process above by pressing Cmd+B or by clicking File->Build. You only need to perform the full procedure again when you want to switch scenes in the build.

## Server

A simple server implementation written in Python is contained in the `arshm-server` directory. This is a Socket.IO-based realtime server which allows clients to broadcast new annotations to other devices and to request image data. It also stores annotations in memory and broadcasts them to a new device when it connects.

Pipenv is used for dependency management. After [installing Pipenv](https://docs.pipenv.org/install/), run `pipenv install` in the server directory to install its Python dependencies. Then run the server with:

```
pipenv run python server.py
```

By default, the server will listen on port 5000.

Next, configure the Unity app to use this server. In the `ARSHM` scene, select the `MainController`, and in the inspector on the right of the editor, replace the default URL under `SocketService` with the correct IP address or hostname. If testing locally, enter `http://127.0.0.1:5000`. If testing using a device on the same network as your computer, you can find your computer's local IP address on a Mac from the Network panel in System Preferences. If the device is on a different network, you will need to set up port forwarding or use a tool like [ngrok](http://ngrok.io/) to create a tunnel to the port.
