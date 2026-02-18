#!/bin/bash

echo "Executing Unity scene with FFI impl"
~/Unity/Hub/Editor/6000.2.15f1/Editor/Unity \
  -batchmode \
  -nographics \
  -projectPath UnityApp \
  -executeMethod Benchmark.Native \
  -logFile FFI-unity-benchmark.log | grep "SHELL" 2>/dev/null

sleep 2

echo "Moving generated files to their proper location"
mv UnityApp/native.csv measurements/data
mv ~/.config/unity3d/DefaultCompany/UnityApp/unity_native.csv measurements/data

cd collektive-lib/
echo "Executing socket server in background"
./gradlew socket-server:run &
SERVER_PID=$!

cd ..
echo "Executing Unity scene with Socket impl (this may take few minutes)"
~/Unity/Hub/Editor/6000.2.15f1/Editor/Unity \
  -batchmode \
  -nographics \
  -projectPath UnityApp \
  -executeMethod Benchmark.Socket \
  -logFile socket-unity-benchmark.log | grep "SHELL" 2>/dev/null
sleep 5

echo "Moving generated files to their proper location"
mv collektive-lib/socket-server/socket.csv measurements/data
mv ~/.config/unity3d/DefaultCompany/UnityApp/unity_socket.csv measurements/data

if [ -n "$SERVER_PID" ]; then
  echo "Stopping socket-server"
  kill $SERVER_PID
  echo "Socket server (PID $SERVER_PID) terminated."
fi

cd measurements/
echo "Computing benchmark"
python -m venv venv
source venv/bin/activate
pip install --upgrade pip
pip install -r requirements.txt
python3 computation.py
cd ..
