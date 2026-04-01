from scipy import signal
import numpy as np
from matplotlib import pyplot as plt

b2 = zd = np.array([1, -1.65])
a2 = pd= np.array([-2.1, -3.41])
fs = 1
system = (zd, pd)

# ============== 1. Рассчёт импульсных и переходных характеристики; ===============

h, t_h = signal.impulse(system, N=200)

plt.plot(t_h, h)
plt.show()

pp, t_pp = signal.step(system, N=50)

plt.plot(t_pp, pp)
plt.show()

# ============== 2. Построение частотных характеристик дискретной системы ===

# Формируем вектор частот (аналог freqspace)
n_freq = 2 * 512
w_norm = np.linspace(0, 1, n_freq, endpoint=False)   # нормированная частота (×π рад/отсчёт)
w_rad = w_norm * np.pi

# Комплексный коэффициент передачи
w, K = signal.freqz(b2, a2, worN=w_rad)

# АЧХ и ФЧХ
K_amp = np.abs(K)
K_amp_dB = 20 * np.log10(K_amp + 1e-12)
K_phase = np.unwrap(np.angle(K))

# freqz в стиле MATLAB (два подграфика)
plt.figure()
signal.freqz(zd, pd, worN=w_rad, fs=fs)   # или fs=2π? По умолчанию rad/sample

# Отдельный график АЧХ в децибелах
plt.figure()
plt.plot(w_norm, K_amp_dB)
plt.grid(True)
plt.title('The Magnitude of Filter')
plt.xlabel('Normalized frequency (×π rad/sample)')
plt.ylabel('dB')

# Отдельная ФЧХ (аналог phasez)
plt.figure()
plt.plot(w_norm, K_phase)
plt.grid(True)
plt.title('Phase response')
plt.xlabel('Normalized frequency (×π rad/sample)')
plt.ylabel('Phase (rad)')
plt.show()

# ============== 3. Построение фазовой и групповой задержки дискретного фильтра ===

# Фазовая задержка (phasedelay) = -phase / omega
omega = w_rad + 1e-12           # избегаем деления на ноль
tau_phase = -K_phase / omega

plt.figure()
plt.plot(w_norm, tau_phase)
plt.grid(True)
plt.title('Phase delay')
plt.xlabel('Normalized frequency (×π rad/sample)')
plt.ylabel('Samples')

# Групповая задержка (grpdelay)
w_gd, tau_group = signal.group_delay((b2, a2), w=w_rad)

plt.figure()
plt.plot(w_gd / np.pi, tau_group)   # нормируем на π
plt.grid(True)
plt.title('Group delay')
plt.xlabel('Normalized frequency (×π rad/sample)')
plt.ylabel('Samples')

# ============== 4. Построение карты нулей и полюсов для РЦФ ===

z1, p1, k1 = signal.tf2zpk(b2, a2)

def zplane(zeros, poles, ax=None):
    if ax is None:
        ax = plt.gca()
        
    unit_circle = plt.Circle((0, 0), 1, fill=False, linestyle='--', color='k')
    ax.add_artist(unit_circle)
    ax.plot(np.real(zeros), np.imag(zeros), 'o', markersize=8, fillstyle='none', color='b', label='Zeros')
    ax.plot(np.real(poles), np.imag(poles), 'x', markersize=8, color='r', label='Poles')
    ax.axhline(0, color='gray', lw=0.5)
    ax.axvline(0, color='gray', lw=0.5)
    ax.set_xlim(-1.5, 1.5)
    ax.set_ylim(-1.5, 1.5)
    ax.set_aspect('equal')
    ax.grid(True)
    ax.legend()
    ax.set_title('Zeros and Poles')

plt.figure()
zplane(z1, p1)
plt.xlabel('Real part')
plt.ylabel('Imag part')
plt.title('Zeros & Poles')