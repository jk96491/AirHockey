import numpy as np
from collections import deque
from mlagents.envs.environment import UnityEnvironment
from UnityTesting.Agent.DDPG import DDPGAgent

state_size = 8
action_size = 3

start_train_episode = 10
run_episode = 500000
test_episode = 100000

print_interval = 5
save_interval = 100


def run(train_mode, load_model, env_name):
    env = UnityEnvironment(file_name=env_name)
    default_brain = env.brain_names[0]

    agent = DDPGAgent(state_size, action_size, train_mode, load_model)
    rewards = deque(maxlen=print_interval)
    success_cnt = 0
    step = 0

    for episode in range(run_episode + test_episode):
        if episode == run_episode:
            train_mode = False

        env_info = env.reset(train_mode=train_mode)[default_brain]
        state = env_info.vector_observations[0]
        episode_rewards = 0
        done = False

        while not done:
            step += 1

            action = agent.get_action([state])[0]
            #print(action)
            env_info = env.step(action)[default_brain]
            next_state = env_info.vector_observations[0]
            reward = env_info.rewards[0]
            done = env_info.local_done[0]

            episode_rewards += reward

            if train_mode:
                agent.append_sample(state, action, reward, next_state, done)

            state = next_state

            if episode > start_train_episode and train_mode:
                agent.train_model()

        success_cnt = success_cnt + 1 if reward == 1 else success_cnt
        rewards.append(episode_rewards)

        if episode % print_interval == 0 and episode != 0:
            print("step: {} / episode: {} / reward: {:.3f} / success_cnt: {}".format
                  (step, episode, np.mean(rewards), success_cnt))
            agent.Write_Summray(np.mean(rewards), success_cnt, episode)
            success_cnt = 0

        if train_mode and episode % save_interval == 0 and episode != 0:
            print("model saved")
            agent.save_model()

    env.close()







