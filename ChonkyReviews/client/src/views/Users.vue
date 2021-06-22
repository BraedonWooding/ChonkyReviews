<template>
  <div>
    <q-toolbar class="bg-primary text-white shadow-2">
      <q-toolbar-title>Users</q-toolbar-title>
      <q-btn round color="primary" icon="add" @click="prompt = true" />
    </q-toolbar>
    <q-list bordered>
      <q-item
        v-for="user in users"
        :key="user.userId"
        class="q-my-sm"
        clickable
        v-ripple
      >
        <q-item-section avatar>
          <q-avatar color="primary" text-color="white">
            {{ user.profileName }}
          </q-avatar>
        </q-item-section>

        <q-item-section>
          <q-item-label>{{ user.profileName }}</q-item-label>
          <q-item-label caption lines="1">{{ user.email }}</q-item-label>
        </q-item-section>
      </q-item>
    </q-list>
    <q-dialog v-model="prompt">
      <q-card style="min-width: 350px">
        <q-card-section>
          <div class="text-h6">Details</div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-input dense v-model="newUser.email" label="Email" autofocus />
          <q-input
            dense
            v-model="newUser.profileName"
            label="Name"
            @keyup.enter="(prompt = false), createUser()"
          />
        </q-card-section>

        <q-card-actions align="right" class="text-primary">
          <q-btn
            flat
            label="Cancel"
            v-close-popup
            @click="user = { email: '', profileName: '' }"
          />
          <q-btn flat label="Add User" v-close-popup @click="createUser()" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script lang="ts">
import { onMounted, Ref, ref } from "vue";

import axios from "axios";

import { User } from "../store/types";

export default {
  name: "LayoutDefault",

  setup(): any {
    const users: Ref<User[]> = ref([]);
    const newUser: Ref<User> = ref({ profileName: "", email: "", userId: "" });

    async function getUsers() {
      const resp = await axios.get("/api/user/all");
      users.value = resp.data;
    }

    async function createUser() {
      await axios.post("/api/user", newUser.value);
      await getUsers();
    }

    onMounted(getUsers);

    return {
      users,
      prompt: ref(false),
      newUser,
      createUser,
    };
  },
};
</script>
