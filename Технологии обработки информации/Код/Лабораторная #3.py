import numpy as np
from PyDynamic.misc.impinvar import impinvar

T0 = 0.01
fs = 1000

denom = 1 + 5.51*T0 + 7.161*T0**2

a1 = (1 + 1.65*T0) / denom
a2 = -1 / denom
b0 = -(2 + 11.02*T0) / denom
b1 = 1 / denom

b_analog = [b0, b1]
a_analog = [1, a1, a2]

# Вызов без выравнивания
bz, az = impinvar(b_analog, a_analog, fs)

print("Цифровой числитель bz:", bz)
print("Цифровой знаменатель az:", az)