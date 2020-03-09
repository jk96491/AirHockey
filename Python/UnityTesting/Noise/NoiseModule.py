import numpy as np

mu = 0.6
theta = 1e-5
sigma = 1e-2

class OU_noise:
    def __init__(self, action_size, ):
        self.action_size = action_size
        self.reset()

    def reset(self):
        self.X = np.ones(self.action_size) * mu

    def sample(self):
        dx = theta * (mu - self.X) + sigma * np.random.randn(len(self.X))
        self.X += dx
        return self.X