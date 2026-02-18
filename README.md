# Performance comparison between Sockets and FFI interaction between Unity and Collektive 

This experiment has the goal to compare the performance of a gradient descent developed in [Collektive](https://github.com/Collektive/collektive) Aggregate Computing framework using [Unity](https://unity.com/) as a simulation engine.

### Companion artifact for the paper entitled _"High-Fidelity Simulation of Aggregate Computing Systems with Collektivity"_ 
Submitted to [Coordination 2025](https://www.discotec.org/2026/coordination) conference 
### Authors
- Filippo Gurioli (filippo.gurioli@studio.unibo.it)
- Martina Baiardi (m.baiardi@unibo.it)
- Angela Cortecchia (angela.cortecchia@unibo.it)
- Danilo Pianini (danilo.pianini@unibo.it)


## Project Structure
Here follows a brief description of the project structure, concerning the files and folders that are relevant for the experiment.

aggregate-gradient
├── App/...                             # @FILIPPO todo
├── collektive-lib
│   ├── socket-server/...               # JVM Application that launches a Socket server for Collektive
│   ├── lib/...                         # Collektive library that uses platform-agnostic data structure and defines the entrypoint 
│   └── ...
├── measurements
│   ├── charts/...                      # Charts produced by the execution of computation.py 
│   └── data/...                        # Pre-run simulation results in .csv format  
│   └── copmutation.py                  # Python script that generates charts from data/* and saves them in charts/*  
├── Unity-App/...                       # Unity project that implements the front-end of the experiment, with both native and socket-based integration with Collektive                  
└── ...

In the following sections we will refer to the collektive-lib as the **back-end**, and to the Unity-App as the **front-end**.

# Data Collection

The experiment collects simulation results in `.csv` format, which are stored in the `data` folder. 
Four files are produced by the benchmark script:
- `native.csv`: back-end performance for the native integration
- `socket.csv`: back-end performance for the socket-based integration
- `unity_native.csv`: front-end performance for the native integration
- `unity_socket.csv`: front-end performance for the socket-based integration

@Filippo todo: check that the description above is correct, in particular the names of the files and their content.

Each file contains 3 columns:
- `t_ns`: nano seconds elapsed since the epoch (1/1/1970)
- `id`: the name of the metric that it is being measured
- `duration_ns`: the measured duration of that metric

### Id classification

Each id represents a specific measured metric:

#### Back-end measures (i.e. inside native.csv and socket.csv)

- `step.service`: measures the whole computation time, since when the step request is received to when the step computation result is sent back.
- `step.compute`: measures the computation of the Collektive algorithm.

#### Front-end measures (i.e. inside unity_native.csv and unity_socket.csv)

- `step.unity.e2e`: time measured for the entire computation.

#### FFI interaction measures

- `step.native.call`: time taken to receive answer from back-end.
- `step.native.parse`: time taken to covert the basic data into usable data inside Unity.

#### Socket interaction measures

- `step.socket.send`: time spent to send the request of a step on the TCP stream (serialization included).
- `step.socket.parse`: time spent to parse back-end response (JSON format) into Unity data structures.
- `step.socket.wait`: time elapsed from when Unity sends a step request and receives the response from the back-end.
- `step.unity.apply`: time spent to apply the received state to Unity’s internal data structures (values and links).

## Sanity checks

To spot invalid data, the following sanity checks have been applied to the raw data:
- Any row where `t_ns` or `duration_ns` are less than 0 have been discarded.
- Any row belonging to warm-up sample has been discarded. Warm-up samples refers to caching, JIT, first allocations and connection setup.
- A warm-up window of 30 samples is discarded, as it empirically captures effects related to caching, initial allocations, JIT compilation, and connection establishment.

@Filippo non mi è chiara la differenza tra il secondo e il terzo

## Performance Result Summary

The experimental results highlight a significant measurements gap between the native integration and the socket-based integration.

On the back-end, the native approach exhibits a median overhead of approximately **7.6 µs**, while the socket-based solution reaches a median overhead of about **230 µs**. This corresponds to a slowdown of roughly **30×** at the median, increasing to over **90×** at the 99th percentile. These results indicate that socket-based inter-process communication introduces substantial and highly variable overhead compared to in-process native calls.

On the front-end end-to-end measurements, the difference is even more pronounced. The native solution shows a median latency of approximately **0.59 ms**, whereas the socket-based approach reaches around **200 ms**. This results in a slowdown of about **337×** at the median, exceeding **600×** at the 95th percentile and approaching **900×** at the 99th percentile. This confirms that communication costs dominate the overall latency when sockets are used in a step-driven execution model.

Overall, the measurements quantitatively demonstrate that native integration is orders of magnitude more efficient and more stable than a socket-based approach for fine-grained interactions.

### Back-end Overhead Distribution

![Back-end overhead distribution](./charts/back-end-overhead.png)

### Front-end Ent-to-End Latency Distribution

![Front-end end-to-end latency distribution](./charts/front-end-e2e.png)

# Reproduce the experiment
There are two ways to reproduce the experiment, either by running the benchmark script or by using the pre-run data in `measurements/data` and running the `computation.py` script to generate the charts in `measurements/charts`.

The pre-collected data in `measurements/data` is the result of running the benchmark script, and it can be used to reproduce the charts without needing to run the full experiment again. This is useful for quickly verifying the results or for testing the chart generation process. The results were obtained by running the benchmark script on a machine with the following specifications:
- CPU: @ FILIPPO TODO: add CPU specifications
- RAM: @ FILIPPO TODO: add RAM specifications
- OS: @ FILIPPO TODO: add OS specifications
- @ FILIPPO TODO: add any other relevant specifications, such as GPU, Java version, Unity version, etc.

## Running the benchmark script

### Requirements

- `Linux` operating system
- `Python 3.10` or higher
- `Unity` version `6000.2.15f1`

> Please verify the following libraries are installed on your system: `libcrypt.so.1` and `libxml2.so.2`. These are required for the proper functioning of the Unity Engine.

### Reproduction steps
1. Clone the repository with `git clone <repository-url>` 
2. `cd aggregate-gradient`
3. Run the benchmark script with `bash ./benchmark.sh`


## Generate the charts from pre-run data

### Requirements

- `Python 3.10` or higher

### Reproduction steps
1. Clone the repository with `git clone <repository-url>`
2. `cd aggregate-gradient/measurements`
3. `python -m venv venv`
4. `source venv/bin/activate`
5. `pip install --upgrade pip`
6. `pip install -r requirements.txt`
7. `python computation.py`
