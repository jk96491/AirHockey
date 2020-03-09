import random
import tensorflow as tf
import numpy as np
from UnityTesting.Class.Actor import Actor
from UnityTesting.Class.Critic import Critic
from collections import deque
import datetime
from UnityTesting.Noise.NoiseModule import OU_noise

actor_lr = 1e-4
critic_lr = 5e-4

batch_size = 128

mem_maxlen = 50000

tau = 1e-3

date_time = datetime.datetime.now().strftime("%Y%m%d-%H-%M-%S")

save_path = 'SaveModels/' + date_time + "_DDPG"
load_path = 'SaveModels/20200229-15-50-44_DDPG/model/model'

discount_factor = 0.99


class DDPGAgent:
    def __init__(self, state_size, action_size, train_mode_, load_model_):
        self.train_mode = train_mode_
        self.load_model = load_model_

        self.actor = Actor('actor', state_size, action_size)
        self.critic = Critic('critic', state_size, action_size)
        self.target_actor = Actor('target_actor', state_size, action_size)
        self.target_critic = Critic('target_critic', state_size, action_size)

        # Critic Model 학습용 (Q 함수)
        self.target_q = tf.placeholder(tf.float32, [None, 1])
        critic_loss = tf.losses.mean_squared_error(self.target_q, self.critic.predict_q)
        self.train_critic = tf.train.AdamOptimizer(critic_lr).minimize(critic_loss)

        # Actor Model 학습용
        action_grad = tf.gradients(tf.squeeze(self.critic.predict_q), self.critic.action)
        policy_grad = tf.gradients(self.actor.action,   self.actor.trainable_var, action_grad)
        for idx, grads in enumerate(policy_grad):
            policy_grad[idx] -= grads / batch_size
        self.train_actor = tf.train.AdamOptimizer(actor_lr).apply_gradients(zip(policy_grad, self.actor.trainable_var))

        self.sess = tf.Session()
        self.sess.run(tf.global_variables_initializer())

        self.Saver = tf.train.Saver()
        self.Summary, self.Merge = self.Make_Summary()
        self.OU = OU_noise(action_size)
        self.memory = deque(maxlen=mem_maxlen)

        self.soft_update_target = []
        for idx in range(len(self.actor.trainable_var)):
            softTau = (tau * self.actor.trainable_var[idx].value() + (1 - tau) * self.target_actor.trainable_var[idx].value())
            self.target_actor.trainable_var[idx].assign(softTau)
            self.soft_update_target.append(self.target_actor.trainable_var[idx])
        for idx in range(len(self.critic.trainable_var)):
            softTau = (tau * self.critic.trainable_var[idx].value()) + ((1 - tau) * self.target_critic.trainable_var[idx].value())
            self.target_critic.trainable_var[idx].assign(softTau)
            self.soft_update_target.append(self.target_critic.trainable_var[idx].assign(softTau))

        init_update_target = []
        for idx in range(len(self.actor.trainable_var)):
            init_update_target.append(self.target_actor.trainable_var[idx].assign(self.actor.trainable_var[idx]))
        for idx in range(len(self.critic.trainable_var)):
            init_update_target.append(self.target_critic.trainable_var[idx].assign(self.critic.trainable_var[idx]))

        self.sess.run(init_update_target)

        if self.load_model:
            self.Saver.restore(self.sess, load_path)

    def get_action(self, state):
        action = self.sess.run(self.actor.action, feed_dict={self.actor.state: state})
        noise = self.OU.sample()

        if self.train_mode:
            return action + noise
        else:
            return action


    def append_sample(self, state, action, reward, next_state, done):
        self.memory.append((state, action, reward, next_state, done))

    def save_model(self):
        self.Saver.save(self.sess, save_path + "/model/model")

    def train_model(self):
        mini_batch = random.sample(self.memory, batch_size)
        states = np.asarray([sample[0] for sample in mini_batch])

        actions = np.asarray([sample[1] for sample in mini_batch])
        rewards = np.asarray([sample[2] for sample in mini_batch])
        next_states = np.asarray([sample[3] for sample in mini_batch])
        dones = np.asarray([sample[4] for sample in mini_batch])

        target_actor_actions = self.sess.run(self.target_actor.action, feed_dict={self.target_actor.state: next_states})
        target_critic_predict_qs = self.sess.run(self.target_critic.predict_q, feed_dict={self.target_critic.state: next_states
            , self.target_critic.action: target_actor_actions})

        target_qs = np.asarray([reward + discount_factor * (1 - done) * target_critic_predict_q
                                for reward, target_critic_predict_q, done in zip(
                rewards, target_critic_predict_qs, dones)])

        self.sess.run(self.train_critic, feed_dict={self.critic.state: states,
                                                    self.critic.action: actions,
                                                    self.target_q: target_qs})

        actions_for_train = self.sess.run(self.actor.action, feed_dict={self.actor.state: states})
        self.sess.run(self.train_actor, feed_dict={self.actor.state: states,
                                                   self.critic.state: states,
                                                   self.critic.action: actions_for_train})

        self.sess.run(self.soft_update_target)

    def Make_Summary(self):
        self.summary_reward = tf.placeholder(tf.float32)
        self.summary_success_cnt = tf.placeholder(tf.float32)
        tf.summary.scalar("reward", self.summary_reward)
        tf.summary.scalar("success_cnt", self.summary_success_cnt)
        Summary = tf.summary.FileWriter(logdir=save_path, graph=self.sess.graph)
        Merge = tf.summary.merge_all()

        return Summary, Merge

    def Write_Summray(self, reward, success_cnt, episode):
        self.Summary.add_summary(self.sess.run(self.Merge, feed_dict={
            self.summary_reward: reward,
            self.summary_success_cnt: success_cnt}), episode)

