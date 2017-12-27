#! /bin/sh

# Example install script for Unity3D project. See the entire example: https://github.com/JonathanPorta/ci-build

# This link changes from time to time. I haven't found a reliable hosted installer package for doing regular
# installs like this. You will probably need to grab a current link from: http://unity3d.com/get-unity/download/archive
echo 'Downloading from https://netstorage.unity3d.com/unity/a9f86dcd79df/MacEditorInstaller/Unity-2017.3.0f3.pkg: '
curl -o Unity.pkg https://netstorage.unity3d.com/unity/a9f86dcd79df/MacEditorInstaller/Unity-2017.3.0f3.pkg

echo 'Installing Unity.pkg'
sudo installer -dumplog -package Unity.pkg -target /