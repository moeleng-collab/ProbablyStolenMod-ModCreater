"""Generate original short PCM sound effects for ModuleCrafter enhancement."""
from pathlib import Path
import wave

import numpy as np

RATE = 44100
OUT = Path(__file__).resolve().parent / "Sounds"
OUT.mkdir(exist_ok=True)
RNG = np.random.default_rng(3840773884)


def timeline(seconds):
    return np.arange(round(RATE * seconds), dtype=np.float64) / RATE


def tone(t, frequency, decay, amplitude=1.0, attack=0.001):
    envelope = (1.0 - np.exp(-t / attack)) * np.exp(-t / decay)
    return amplitude * envelope * np.sin(2.0 * np.pi * frequency * t)


def noise_burst(t, decay, amplitude=1.0, smooth=1):
    noise = RNG.standard_normal(t.size)
    if smooth > 1:
        noise = np.convolve(noise, np.ones(smooth) / smooth, mode="same")
    return amplitude * noise * np.exp(-t / decay)


def delayed(t, signal_fn, delay):
    shifted = t - delay
    return np.where(shifted >= 0.0, signal_fn(np.maximum(shifted, 0.0)), 0.0)


def normalize(signal, peak=0.82):
    signal = np.tanh(signal * 1.15)
    signal *= peak / max(1e-9, float(np.max(np.abs(signal))))
    # Four-millisecond edge ramps prevent clicks at the file boundaries.
    edge = min(round(RATE * 0.004), signal.size // 2)
    ramp = np.linspace(0.0, 1.0, edge)
    signal[:edge] *= ramp
    signal[-edge:] *= ramp[::-1]
    return signal


def save(name, signal):
    pcm = np.round(normalize(signal) * 32767.0).astype("<i2")
    path = OUT / f"{name}.wav"
    with wave.open(str(path), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(RATE)
        output.writeframes(pcm.tobytes())
    print(f"{path.name}: {len(pcm) / RATE:.3f}s, peak={np.max(np.abs(pcm))}, bytes={path.stat().st_size}")


# One bright hammer-on-anvil strike with inharmonic metal partials.
t = timeline(0.62)
success = sum(
    tone(t, frequency, decay, amplitude)
    for frequency, decay, amplitude in [
        (683, 0.19, 0.85),
        (1197, 0.15, 0.55),
        (1871, 0.11, 0.36),
        (2767, 0.075, 0.24),
        (3913, 0.045, 0.12),
    ]
)
success += noise_burst(t, 0.009, 0.65, smooth=2)
success += delayed(t, lambda x: tone(x, 146, 0.055, 0.45) + noise_burst(x, 0.012, 0.2, 12), 0.0)
save("module_crafter_enhance_success", success)

# A quiet, dull metallic clunk for stay/drop/clip failures.
t = timeline(0.36)
failure = sum(
    tone(t, frequency, decay, amplitude, attack=0.0015)
    for frequency, decay, amplitude in [
        (173, 0.075, 0.8),
        (287, 0.055, 0.48),
        (463, 0.035, 0.22),
    ]
)
failure += noise_burst(t, 0.017, 0.42, smooth=18)
save("module_crafter_enhance_failure", failure)

# A heavy strike followed by several irregular metallic fragment bursts.
t = timeline(1.05)
ruined = sum(
    tone(t, frequency, decay, amplitude)
    for frequency, decay, amplitude in [
        (521, 0.16, 0.75),
        (997, 0.11, 0.52),
        (1613, 0.075, 0.35),
        (2371, 0.05, 0.22),
    ]
)
ruined += noise_burst(t, 0.012, 0.7, smooth=3)
for delay, frequency, decay, amplitude in [
    (0.075, 2387, 0.07, 0.36),
    (0.135, 3181, 0.06, 0.31),
    (0.225, 1777, 0.09, 0.29),
    (0.335, 2713, 0.075, 0.23),
    (0.475, 1297, 0.11, 0.18),
]:
    ruined += delayed(
        t,
        lambda x, f=frequency, d=decay, a=amplitude: tone(x, f, d, a)
        + noise_burst(x, d * 0.45, a * 0.8, smooth=2),
        delay,
    )
save("module_crafter_enhance_ruined", ruined)
