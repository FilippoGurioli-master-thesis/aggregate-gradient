#!/bin/bash

~/Unity/Hub/Editor/6000.2.15f1/Editor/Unity \
  -batchmode \
  -nographics \
  -projectPath UnityApp \
  -executeMethod Benchmark.Native \
  -logFile - | grep "SHELL" 2>/dev/null

mv UnityApp/native.csv measurements/data
mv ~/.config/unity3d/DefaultCompany/UnityApp/unity_native.csv measurements/data

cd collektive-lib/
./gradlew socket-server:run &
SERVER_PID=$!

cd ..
~/Unity/Hub/Editor/6000.2.15f1/Editor/Unity \
  -batchmode \
  -nographics \
  -projectPath UnityApp \
  -executeMethod Benchmark.Socket \
  -logFile - | grep "SHELL" 2>/dev/null

mv collektive-lib/socket-server/socket.csv measurements/data
mv ~/.config/unity3d/DefaultCompany/UnityApp/unity_socket.csv measurements/data

if [ -n "$SERVER_PID" ]; then
  kill $SERVER_PID
  echo "Socket server (PID $SERVER_PID) terminated."
fi

cd measurements/
python3 computation.py
cd ..
