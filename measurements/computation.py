import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from pathlib import Path

# ----------------------------
# Configuration
# ----------------------------

WARMUP_SAMPLES = 30

DATA_DIR = Path("data")

FILES = {
    "native_be": DATA_DIR / "native.csv",
    "socket_be": DATA_DIR / "socket.csv",
    "native_fe": DATA_DIR / "unity_native.csv",
    "socket_fe": DATA_DIR / "unity_socket.csv",
}

# ----------------------------
# 1. Load & clean data
# ----------------------------

def load_csv(path: Path) -> pd.DataFrame:
    df = pd.read_csv(path)
    return df


def remove_invalid(df: pd.DataFrame) -> pd.DataFrame:
    return df[(df["t_ns"] >= 0) & (df["duration_ns"] >= 0)]


def remove_warmup(df: pd.DataFrame, warmup: int = WARMUP_SAMPLES) -> pd.DataFrame:
    return (
        df.sort_values("t_ns")
          .groupby("id", as_index=False)
          .nth(slice(warmup, None))
          .reset_index(drop=True)
    )


def preprocess(path: Path) -> pd.DataFrame:
    df = load_csv(path)
    df = remove_invalid(df)
    df = remove_warmup(df)
    return df


# ----------------------------
# 3. Statistical analysis
# ----------------------------

def stats(series: pd.Series) -> dict:
    return {
        "count": len(series),
        "median": series.median(),
        "mean": series.mean(),
        "p95": series.quantile(0.95),
        "p99": series.quantile(0.99),
        "std": series.std(),
    }


def compute_stats(df: pd.DataFrame) -> pd.DataFrame:
    rows = []
    for metric, group in df.groupby("id"):
        s = stats(group["duration_ns"])
        s["id"] = metric
        rows.append(s)
    return pd.DataFrame(rows).set_index("id")


# ----------------------------
# 4. Overheads
# ----------------------------

def backend_overhead(df: pd.DataFrame) -> pd.Series:
    """
    overhead_k = service_k - compute_k
    Assumes both series are aligned by order.
    """
    service = df[df["id"] == "step.service"]["duration_ns"].reset_index(drop=True)
    compute = df[df["id"] == "step.compute"]["duration_ns"].reset_index(drop=True)

    n = min(len(service), len(compute))
    return service.iloc[:n] - compute.iloc[:n]


def frontend_cost(df: pd.DataFrame) -> pd.Series:
    """
    End-to-end cost only (no overhead decomposition)
    """
    return df[df["id"] == "step.unity.e2e"]["duration_ns"]


# ----------------------------
# 5. Speedup
# ----------------------------

def speedup(socket_series: pd.Series, native_series: pd.Series) -> dict:
    """
    speedup = socket / native
    """
    n = min(len(socket_series), len(native_series))
    ratio = socket_series.iloc[:n].values / native_series.iloc[:n].values

    return {
        "mean_speedup": np.mean(ratio),
        "median_speedup": np.median(ratio),
        "p95_speedup": np.quantile(ratio, 0.95),
        "p99_speedup": np.quantile(ratio, 0.99),
    }


# ----------------------------
# 6. Plotting
# ----------------------------

def plot_latency(series_dict: dict, title: str):
    plt.figure(figsize=(8, 5))
    for label, s in series_dict.items():
        plt.plot(s.reset_index(drop=True), label=label)
    plt.xlabel("Sample")
    plt.ylabel("Duration (ns)")
    plt.title(title)
    plt.legend()
    plt.grid(True)
    plt.tight_layout()
    plt.show()


def plot_box(series_dict: dict, title: str):
    plt.figure(figsize=(6, 4))
    plt.boxplot(
        series_dict.values(),
        tick_labels=list(series_dict.keys()),
        showfliers=False
    )
    plt.ylabel("Duration (ns)")
    plt.title(title)
    plt.grid(True)
    plt.tight_layout()
    plt.show()

def print_stats_block(title: str, native: dict, socket: dict, unit: str = "ns"):
    print(f"\n=== {title} ===")
    header = f"{'Metric':<12}{'Native':>18}{'Socket':>18}"
    print(header)
    print("-" * len(header))

    def fmt(x):
        return f"{x:,.2f}"

    rows = [
        ("count", lambda d: d["count"]),
        ("median", lambda d: d["median"]),
        ("mean", lambda d: d["mean"]),
        ("p95", lambda d: d["p95"]),
        ("p99", lambda d: d["p99"]),
        ("std", lambda d: d["std"]),
    ]

    for name, getter in rows:
        n = getter(native)
        s = getter(socket)
        print(f"{name:<12}{fmt(n):>18}{fmt(s):>18}")

    print(f"{'(unit)':<12}{unit:>18}{unit:>18}")


def print_speedup_block(title: str, speedup_stats: dict):
    print(f"\n=== {title} ===")
    print(f"{'Metric':<16}{'Socket / Native':>20}")
    print("-" * 36)

    def fmt(x):
        return f"{x:,.2f}×"

    rows = [
        ("mean", speedup_stats["mean_speedup"]),
        ("median", speedup_stats["median_speedup"]),
        ("p95", speedup_stats["p95_speedup"]),
        ("p99", speedup_stats["p99_speedup"]),
    ]

    for name, value in rows:
        print(f"{name:<16}{fmt(value):>20}")

# ----------------------------
# Main analysis
# ----------------------------

def main():
    dfs = {k: preprocess(v) for k, v in FILES.items()}

    # Backend overheads
    native_be_overhead = backend_overhead(dfs["native_be"])
    socket_be_overhead = backend_overhead(dfs["socket_be"])

    # Frontend end-to-end
    native_fe_cost = frontend_cost(dfs["native_fe"])
    socket_fe_cost = frontend_cost(dfs["socket_fe"])

    # Statistics
    print_stats_block(
        "Backend overhead statistics",
        stats(native_be_overhead),
        stats(socket_be_overhead),
        unit="ns",
    )

    print_stats_block(
        "Frontend end-to-end statistics",
        stats(native_fe_cost),
        stats(socket_fe_cost),
        unit="ns",
    )

    # Speedups
    print_speedup_block(
        "Backend overhead speedup",
        speedup(socket_be_overhead, native_be_overhead),
    )

    print_speedup_block(
        "Frontend end-to-end speedup",
        speedup(socket_fe_cost, native_fe_cost),
    )

    # Plots
    plot_box(
        {
            "Native BE overhead": native_be_overhead,
            "Socket BE overhead": socket_be_overhead,
        },
        "Backend overhead comparison",
    )

    plot_box(
        {
            "Native FE e2e": native_fe_cost,
            "Socket FE e2e": socket_fe_cost,
        },
        "Frontend end-to-end comparison",
    )


if __name__ == "__main__":
    main()
