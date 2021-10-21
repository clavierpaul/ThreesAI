from tf_agents import environments
import threes_env

from tf_agents.environments import utils
import numpy as np

environment = threes_env.ThreesEnv()
print(environment.reset())
for i in range(4):
    input()
    print(environment.step(np.array([1])))
    