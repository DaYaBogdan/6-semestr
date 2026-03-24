import matplotlib.pyplot as plt
import numpy as np
from scipy.signal import zpk2tf, residue, freqs

Tstart = 0
Tend = 3
Tstep = 0.1

zeros = np.array([-1.65])
poluses = np.array([-2.1, -3.41]) 
tau = np.array([i for i in np.arange(Tstart, Tend, Tstep)])

b, a = zpk2tf(zeros, poluses, 1)
r, p, k = residue(b, a)

def h(_tau: float):
    h = 0
    for n in range(len(r)):
        h += r[n] * np.exp(poluses[n] * _tau)
    return h

def T(time: float):
    T = 0
    for n in range(len(r)):
        T += r[n]/poluses[n] * np.exp(poluses[n] * time)
    return b[0]/a[0] + T

def makeZerosNPolusesDiagram():
    plt.plot(poluses, '*r')
    plt.plot(zeros, 'db')
    plt.xlabel("Real")
    plt.ylabel("Image")
    plt.legend(["poluses", "zeros"])
    plt.show()

def printPolynoms():
    print(f"r: {r} \np: {p}\nk: {k}")

def makeGodograph():
    w = [0, *np.logspace(-2, 3, 100), np.inf]
    _, fchx = freqs(b, a, w)
    plt.figure(figsize=(10, 8))
    plt.plot(np.real(fchx), np.imag(fchx), '-b', linewidth=1.5, label='Годограф')
    plt.plot(np.real(fchx[0]), np.imag(fchx[0]), 'ro', markersize=8, label='ω = 0')
    plt.plot(np.real(fchx[-1]), np.imag(fchx[-1]), 'go', markersize=8, label='ω = ∞')
    plt.axhline(y=0, color='k', linewidth=0.5)
    plt.axvline(x=0, color='k', linewidth=0.5)
    plt.grid(True, alpha=0.3)
    plt.title("Годограф")
    plt.xlabel('Re H(jω)')
    plt.ylabel('Im H(jω)')
    plt.axis('equal')
    plt.legend()
    plt.show()
    
    mag = np.abs(fchx)
    phase = np.angle(fchx)
    plt.subplot(2, 1, 1)
    plt.loglog(w, mag, '-k')
    plt.title("The Magnitude")
    plt.grid(True)
    plt.subplot(2, 1, 2)
    plt.semilogx(w, phase, '-k')
    plt.title("The Phase")
    plt.grid(True)
    plt.show()

def findHTau():
    array_of_h = [h(_tau) for _tau in tau] 
    
    print("h(tau):", array_of_h)
    plt.plot(tau, array_of_h)
    plt.xlabel("tau")
    plt.ylabel("h(tau)")
    plt.show()

def findTt():
    array_of_T = [T(time) for time in tau]
    
    print("T(t):", array_of_T)
    plt.plot(tau, array_of_T)
    plt.xlabel("t")
    plt.ylabel("T")
    plt.show()
    
# makeZerosNPolusesDiagram()
# printPolynoms()
makeGodograph()
# findHTau()
# findTt()