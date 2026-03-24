from random import random
from math import log2
import matplotlib.pyplot as plt
import numpy as np

P0_MESSAGE = 0.65
P1_ERROR = 0.99
P1_CLEAR = 1 - P1_ERROR

N_MESSAGE = 50

def H_X(variable = P1_CLEAR):
    return -(variable * log2(variable) + variable * log2(variable))

def H_Y(variable = P1_CLEAR * P1_ERROR):
    return -(variable * log2(variable) + variable * log2(variable))

def H_YX(variable = P1_ERROR):
    return -(variable * log2(variable) + (1 - variable) * log2(1 - variable))

def I():
    return H_Y() - H_YX()

def generateMessage():
    message = np.arange(N_MESSAGE)
    message = [1 if random() <= P0_MESSAGE else 0 for code in message]
    return message

def generateErrors():
    errors = np.arange(N_MESSAGE)
    errors = [1 if random() <= P1_ERROR else 0 for code in errors]
    return errors

def main():
    message = generateMessage()
    errors = generateErrors()
    end_message = np.logical_xor(message, errors)
    
    vector_indexes = [i for i in range(0, N_MESSAGE, 1)]
    
    
    print("H(X): Энтропия источника = ", H_X())
    print("H(Y|X): Условная энтропия выходного сигнала = ", H_YX())
    print("H(Y): Энтропия принятого сигнала = ", H_Y())
    print("I(Y|X): Количество информации, получаемое от источника = ", I())
    input("Нажмите enter для продолжения")
    
    plt.figure(figsize=(100, 2))
    
    plt.xticks(vector_indexes)
    plt.plot(vector_indexes, message, drawstyle='steps')
    plt.show()
    
    plt.xticks(vector_indexes)
    plt.plot(vector_indexes, errors, drawstyle='steps')
    plt.show()
    
    plt.xticks(vector_indexes)
    plt.plot(vector_indexes, end_message, drawstyle='steps')
    plt.show()

main()